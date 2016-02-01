using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Realtime.Messaging.Exceptions;
using Realtime.Messaging.Ext;
using Xamarin.Forms;

namespace Realtime.Messaging
{
    internal class Client
    {
        internal OrtcClient context;

        internal bool _isConnecting;
        internal bool _alreadyConnectedFirstTime;
        internal bool _callDisconnectedCallback;
        internal bool _waitingServerResponse;
        internal List<KeyValuePair<string, string>> _permissions;
        internal RealtimeDictionary<string, ChannelSubscription> _subscribedChannels;
        internal RealtimeDictionary<string, RealtimeDictionary<int, BufferedMessage>> _multiPartMessagesBuffer;
        internal IWebsocketConnection _webSocketConnection;
		internal IBalancer _balancer;
        internal DateTime? _lastKeepAlive; // Holds the time of the last keep alive received from the server

        internal Timer _reconnectTimer; // Timer to reconnect
        internal Timer _heartbeatTimer;
        internal Timer _connectionTimer;
        private MsgProcessor _msgProcessor;
        private int _gotOnOpenCount;

        internal Client(OrtcClient ortcClient)
        {
            context = ortcClient;

            _gotOnOpenCount = 0;

            _permissions = new List<KeyValuePair<string, string>>();
            _subscribedChannels = new RealtimeDictionary<string, ChannelSubscription>();
            _multiPartMessagesBuffer = new RealtimeDictionary<string, RealtimeDictionary<int, BufferedMessage>>();

            _heartbeatTimer = new Timer(_heartbeatTimer_Elapsed, context.HeartbeatTime * 1000);
            _connectionTimer = new Timer(_connectionTimer_Elapsed, Constants.SERVER_HB_COUNT * 1000);
            _reconnectTimer = new Timer(_reconnectTimer_Elapsed, context.ConnectionTimeout * 1000);

			_balancer = DependencyService.Get<IBalancer> ();

			if (_balancer == null)
				throw new OrtcGenericException(
					"DependencyService Failed, please include the platform plugins. This may be caused linker stripping.");

            _webSocketConnection = DependencyService.Get<IWebsocketConnection>();

            if (_webSocketConnection == null)
                throw new OrtcGenericException(
                    "DependencyService Failed, please include the platform plugins. This may be caused linker stripping.");

            _webSocketConnection.OnOpened += _webSocketConnection_OnOpened;
            _webSocketConnection.OnClosed += _webSocketConnection_OnClosed;
            _webSocketConnection.OnError += _webSocketConnection_OnError;
            _webSocketConnection.OnMessageReceived += _webSocketConnection_OnMessageReceived;

            _msgProcessor = new MsgProcessor(this);
        }

        private void _webSocketConnection_OnOpened()
        {
            sendValidateCommand();
        }

        private void _webSocketConnection_OnClosed()
        {
            // Clear user permissions
            var wasdisconnected = _isConnecting || context.IsConnected;
            _permissions.Clear();
            _isConnecting = false;
            context.IsConnected = false;
            _heartbeatTimer.Stop();
            _connectionTimer.Stop();

            if (_callDisconnectedCallback)
            {
                DoReconnect();
            }
            else if (wasdisconnected)
            {
                context.DelegateDisconnectedCallback();
            }
        }

        private void _webSocketConnection_OnError(Exception error)
        {
            if (_isConnecting)
            {
                context.DelegateExceptionCallback(new OrtcGenericException(error.Message));
                DoReconnect();
            }
            else
            {
                context.DelegateExceptionCallback(new OrtcGenericException(String.Format("WebSocketConnection exception: {0}", error)));
                _webSocketConnection.Close();
            }
        }

        private void sendValidateCommand()
        {
            _gotOnOpenCount++;
            if (_gotOnOpenCount > 1)
            {
                try
                {
                    //if (String.IsNullOrEmpty(ReadLocalStorage(_applicationKey, _sessionExpirationTime))) {
                    context.SessionId = Strings.GenerateId(16);
                    //}

                    string s;
                    if (context.HeartbeatActive)
                    {
                        s = String.Format("validate;{0};{1};{2};{3};{4};{5};{6}", context._applicationKey, context._authenticationToken, context.AnnouncementSubChannel, context.SessionId,
                            context.ConnectionMetadata, context.HeartbeatTime, context.HeartbeatFails);
                    }
                    else
                    {
                        s = String.Format("validate;{0};{1};{2};{3};{4}", context._applicationKey, context._authenticationToken, context.AnnouncementSubChannel, context.SessionId, context.ConnectionMetadata);
                    }
                    DoSend(s);
                }
                catch (Exception ex)
                {
                    context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Exception sending validate: {0}", ex)));
                }
            }
        }

        private void _webSocketConnection_OnMessageReceived(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                if (message[0] != 'c')
                {
                    _reconnectTimer.Stop();
                    _connectionTimer.Start();
                }

                // Open
                if (message == "o")
                {
                    sendValidateCommand();
                }
                // Heartbeat
                else if (message == "h")
                {
                    // Do nothing
                }
                else
                {
                    message = message.Replace("\\\"", @"""");

                    // Update last keep alive time
                    _lastKeepAlive = DateTime.Now;

                    // Operation
                    Match operationMatch = Regex.Match(message, Constants.OPERATION_PATTERN);

                    if (operationMatch.Success)
                    {
                        string operation = operationMatch.Groups["op"].Value;
                        string arguments = operationMatch.Groups["args"].Value;

                        switch (operation)
                        {
                            case "ortc-validated":
                                _msgProcessor.ProcessOperationValidated(arguments);
                                break;
                            case "ortc-subscribed":
                                _msgProcessor.ProcessOperationSubscribed(arguments);
                                break;
                            case "ortc-unsubscribed":
                                _msgProcessor.ProcessOperationUnsubscribed(arguments);
                                break;
                            case "ortc-error":
                                _msgProcessor.ProcessOperationError(arguments);
                                break;
                            default:
                                // Unknown operation
                                context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unknown operation \"{0}\" for the message \"{1}\"", operation, message)));

                                DoDisconnect();
                                break;
                        }
                    }
                    else
                    {
                        // Close
                        Match closeOperationMatch = Regex.Match(message, Constants.CLOSE_PATTERN);

                        if (!closeOperationMatch.Success)
                        {
                            _msgProcessor.ProcessOperationReceived(message);
                        }
                    }
                }
            }

        }
        
        internal async void DoConnect()
        {
            _isConnecting = true;
            _callDisconnectedCallback = false;
            _gotOnOpenCount = 0;


            if (context.IsCluster)
            {
                try
                {
					context.Url = await Balancer.ResolveClusterUrlAsync(context.ClusterUrl, _balancer); //"nope";// GetUrlFromCluster();

                    context.IsCluster = true;

                    if (String.IsNullOrEmpty(context.Url))
                    {
                        context.DelegateExceptionCallback(new OrtcEmptyFieldException("Unable to get URL from cluster"));

                        if (!_isConnecting)
                            return;
                        DoReconnect();
                    }
                }
                catch (Exception ex)
                {
                    context.DelegateExceptionCallback(new OrtcNotConnectedException(ex.Message));

                    if (!_isConnecting)
                        return;
                    DoReconnect();
                }
            }

            if (!_isConnecting)
                return;
            if (!String.IsNullOrEmpty(context.Url))
            {
                try
                {
                    _webSocketConnection.Connect(context.Url);

                    // Just in case the server does not respond
                    //
                    _waitingServerResponse = true;

                    StartReconnectTimer();
                    //
                }
                catch (OrtcEmptyFieldException ex)
                {
                    context.DelegateExceptionCallback(new OrtcNotConnectedException(ex.Message));
                    DoStopReconnecting();
                }
                catch (Exception ex)
                {
                    context.DelegateExceptionCallback(new OrtcNotConnectedException(ex.Message));
                    _isConnecting = false;
                }
            }

        }

        internal void DoReconnect()
        {
            if (!context.IsConnected)
            {
                context.DelegateReconnectingCallback();

                if (!_reconnectTimer.IsRunning)
                {
                    StartReconnectTimer();

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void DoStopReconnecting()
        {
            _isConnecting = false;
            _alreadyConnectedFirstTime = false;
            
            if (_reconnectTimer.IsRunning)
                context.DelegateDisconnectedCallback();

            _reconnectTimer.Stop();
        }

        /// <summary>
        /// Disconnect the TCP client.
        /// </summary>
        internal void DoDisconnect()
        {
            _callDisconnectedCallback = false;
            _heartbeatTimer.Stop();
            _connectionTimer.Stop();
            _reconnectTimer.Stop();
            try
            {
                _webSocketConnection.Close();
            }
            catch (Exception ex)
            {
                context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Error disconnecting: {0}", ex)));
            }
        }


        internal void Send(string channel, string message)
        {
            byte[] channelBytes = Encoding.UTF8.GetBytes(channel);

            var domainChannelCharacterIndex = channel.IndexOf(':');
            var channelToValidate = channel;

            if (domainChannelCharacterIndex > 0)
            {
                channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
            }

            string hash = _permissions.Where(c => c.Key == channel || c.Key == channelToValidate).FirstOrDefault().Value;

            if (_permissions != null && _permissions.Count > 0 && String.IsNullOrEmpty(hash))
            {
                context.DelegateExceptionCallback(new OrtcNotConnectedException(String.Format("No permission found to send to the channel '{0}'", channel)));
            }
            else
            {
                message = message.Replace(Environment.NewLine, "\n");

                if (channel != String.Empty && message != String.Empty)
                {
                    try
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                        List<String> messageParts = new List<String>();
                        int pos = 0;
                        int remaining;
                        string messageId = Strings.GenerateId(8);

                        // Multi part
                        while ((remaining = messageBytes.Length - pos) > 0)
                        {
                            byte[] messagePart;

                            if (remaining >= Constants.MAX_MESSAGE_SIZE - channelBytes.Length)
                            {
                                messagePart = new byte[Constants.MAX_MESSAGE_SIZE - channelBytes.Length];
                            }
                            else
                            {
                                messagePart = new byte[remaining];
                            }

                            Array.Copy(messageBytes, pos, messagePart, 0, messagePart.Length);

                            messageParts.Add(Encoding.UTF8.GetString((byte[])messagePart, 0, messagePart.Length));

                            pos += messagePart.Length;
                        }

                        for (int i = 0;i < messageParts.Count;i++)
                        {
                            string s = String.Format("send;{0};{1};{2};{3};{4}", context._applicationKey, context._authenticationToken, channel, hash, String.Format("{0}_{1}-{2}_{3}", messageId, i + 1, messageParts.Count, messageParts[i]));

                            DoSend(s);
                        }
                    }
                    catch (Exception ex)
                    {
                        string exName = null;

                        if (ex.InnerException != null)
                        {
                            exName = ex.InnerException.GetType().Name;
                        }

                        switch (exName)
                        {
                            case "OrtcNotConnectedException":
                                // Server went down
                                if (context.IsConnected)
                                {
                                    DoDisconnect();
                                }
                                break;
                            default:
                                context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unable to send: {0}", ex)));
                                break;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Sends a message through the TCP client.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        internal void DoSend(string message)
        {
            try
            {
                _webSocketConnection.Send(message);
            }
            catch (Exception ex)
            {
                context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unable to send: {0}", ex)));
            }
        }

        private void StartReconnectTimer()
        {
            if (_reconnectTimer.IsRunning)
            {
                return;
            }

            _reconnectTimer.Start();
        }

        private void _reconnectTimer_Elapsed(object sender)
        {
            if (!context.IsConnected)
            {
                if (_waitingServerResponse)
                {
                    _waitingServerResponse = false;
                    context.DelegateExceptionCallback(new OrtcNotConnectedException("Unable to connect"));
                }
                
                DoConnect();
            }
            else
            {
                _reconnectTimer.Stop();
            }
        }

        private void _heartbeatTimer_Elapsed(object sender)
        {
            if (context.IsConnected)
            {
                DoSend("b");
            }
        }

        private void _connectionTimer_Elapsed(object sender)
        {
            //System.Diagnostics.Debug.WriteLine("Server HB failed!");
            _webSocketConnection_OnClosed();
        }

        internal void SendProxy(string applicationKey, string privateKey, string channel, string message)
        {
            message = message.Replace(Environment.NewLine, "\n");
            byte[] channelBytes = Encoding.UTF8.GetBytes(channel);


            if (channel != String.Empty && message != String.Empty)
            {
                try
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    List<String> messageParts = new List<String>();
                    int pos = 0;
                    int remaining;
                    string messageId = Strings.GenerateId(8);

                    // Multi part
                    while ((remaining = messageBytes.Length - pos) > 0)
                    {
                        byte[] messagePart;

                        if (remaining >= Constants.MAX_MESSAGE_SIZE - channelBytes.Length)
                        {
                            messagePart = new byte[Constants.MAX_MESSAGE_SIZE - channelBytes.Length];
                        }
                        else
                        {
                            messagePart = new byte[remaining];
                        }

                        Array.Copy(messageBytes, pos, messagePart, 0, messagePart.Length);

                        messageParts.Add(Encoding.UTF8.GetString((byte[])messagePart, 0, messagePart.Length));

                        pos += messagePart.Length;
                    }

                    for (int i = 0;i < messageParts.Count;i++)
                    {
                        string s = String.Format("sendproxy;{0};{1};{2};{3}", applicationKey, privateKey, channel, String.Format("{0}_{1}-{2}_{3}", messageId, i + 1, messageParts.Count, messageParts[i]));

                        DoSend(s);
                    }
                }
                catch (Exception ex)
                {
                    string exName = null;

                    if (ex.InnerException != null)
                    {
                        exName = ex.InnerException.GetType().Name;
                    }

                    switch (exName)
                    {
                        case "OrtcNotConnectedException":
                            // Server went down
                            if (context.IsConnected)
                            {
                                DoDisconnect();
                            }
                            break;
                        default:
                            context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unable to send: {0}", ex)));
                            break;
                    }
                }
            }


        }

		internal void subscribe(string channel, bool subscribeOnReconnected, OrtcClient.OnMessageDelegate onMessage, bool withNotifications)
        {
            var domainChannelCharacterIndex = channel.IndexOf(':');
            var channelToValidate = channel;

            if (domainChannelCharacterIndex > 0)
            {
                channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
            }

            string hash = _permissions.Where(c => c.Key == channel || c.Key == channelToValidate).FirstOrDefault().Value;

            if (_permissions != null && _permissions.Count > 0 && String.IsNullOrEmpty(hash))
            {
                context.DelegateExceptionCallback(new OrtcNotConnectedException(String.Format("No permission found to subscribe to the channel '{0}'", channel)));
            }
            else
            {
                if (!_subscribedChannels.ContainsKey(channel))
                {
                    _subscribedChannels.Add(channel,
                        new ChannelSubscription
                        {
                            IsSubscribing = true,
                            IsSubscribed = false,
                            SubscribeOnReconnected = subscribeOnReconnected,
                            OnMessage = onMessage,
							isWithNotification = withNotifications
                        });
                }

                try
                {
                    if (_subscribedChannels.ContainsKey(channel))
                    {
                        ChannelSubscription channelSubscription = null;
                        _subscribedChannels.TryGetValue(channel, out channelSubscription);

                        channelSubscription.IsSubscribing = true;
                        channelSubscription.IsSubscribed = false;
                        channelSubscription.SubscribeOnReconnected = subscribeOnReconnected;
                        channelSubscription.OnMessage = onMessage;
						channelSubscription.isWithNotification = withNotifications;
                    }

					if (withNotifications) {
						if(Device.OS == TargetPlatform.Android){
							if (String.IsNullOrEmpty(context._registrationId)) {
								context.DelegateExceptionCallback(new OrtcGcmException("The application is not registered with GCM yet!"));
								return;
							}
							else{
								string s = String.Format("subscribe;{0};{1};{2};{3};{4};GCM", context._applicationKey, context._authenticationToken, channel, hash, context._registrationId);
								DoSend(s);
							}
						}
						else if(Device.OS == TargetPlatform.iOS){
							if (String.IsNullOrEmpty(Realtime.Messaging.Helpers.Settings.Token)) {
								context.DelegateExceptionCallback(new OrtcApnsException("The application is not registered with Apns yet!"));
								return;
							}
							else{
								string s = String.Format("subscribe;{0};{1};{2};{3};{4};Apns", context._applicationKey, context._authenticationToken, channel, hash, Realtime.Messaging.Helpers.Settings.Token);
								DoSend(s);
							}
						}
					} else{
						string s = String.Format("subscribe;{0};{1};{2};{3}", context._applicationKey, context._authenticationToken, channel, hash);
						DoSend(s);
					}
                }
                catch (Exception ex)
                {
                    string exName = null;

                    if (ex.InnerException != null)
                    {
                        exName = ex.InnerException.GetType().Name;
                    }

                    switch (exName)
                    {
                        case "OrtcNotConnectedException":
                            // Server went down
                            if (context.IsConnected)
                            {
                                DoDisconnect();
                            }
                            break;
                        default:
                            context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unable to subscribe: {0}", ex)));
                            break;
                    }
                }
            }
        }

		internal void unsubscribe(string channel, bool isWithNotification, TargetPlatform platform)
        {
            try
            {
				if(isWithNotification){
					if(platform == TargetPlatform.Android){
						string s = String.Format("unsubscribe;{0};{1};{2};GCM", context._applicationKey, channel, context._registrationId);

	                	DoSend(s);
					}
					else if(platform == TargetPlatform.iOS){
						//TO DO
					}
				}else{
					string s = String.Format("unsubscribe;{0};{1}", context._applicationKey, channel);

					DoSend(s);
				}
            }
            catch (Exception ex)
            {
                string exName = null;

                if (ex.InnerException != null)
                {
                    exName = ex.InnerException.GetType().Name;
                }

                switch (exName)
                {
                    case "OrtcNotConnectedException":
                        // Server went down
                        if (context.IsConnected)
                        {
                            DoDisconnect();
                        }
                        break;
                    default:
                        context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unable to unsubscribe: {0}", ex)));
                        break;
                }
            }
        }

        internal void startHeartbeat()
        {
            _heartbeatTimer.Start();
        }
    }
}
