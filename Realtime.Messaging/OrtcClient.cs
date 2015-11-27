using System;
using System.Threading;
using System.Threading.Tasks;

using RealtimeFramework.Messaging.Ext;
using RealtimeFramework.Messaging.Exceptions;
using Xamarin.Forms;
using Realtime.Messaging.Helpers;
using Realtime.Messaging;
using System.Collections.Generic;

namespace RealtimeFramework.Messaging
{       
    /// <summary>
    /// The ORTC (Open Real-Time Connectivity) was developed to add a layer of abstraction to real-time full-duplex web communications platforms by making real-time web applications independent of those platforms.
    /// OrtcClient instance provides methods for sending and receiving data in real-time over the web.
    /// </summary>
    ///         /// <example>
    /// <code>
    ///     string applicationKey = "myApplicationKey";
    ///     string authenticationToken = "myAuthenticationToken"; 
    ///                 
    ///     // Create ORTC client
    ///     OrtcClient ortcClient = new RealtimeFramework.Messaging.OrtcClient();
    ///     ortcClient.ClusterUrl = "http://ortc-developers.realtime.co/server/2.1/";
    ///                 
    ///     // Ortc client handlers
    ///     ortcClient.OnConnected += new OnConnectedDelegate(ortc_OnConnected);
    ///     ortcClient.OnSubscribed += new OnSubscribedDelegate(ortc_OnSubscribed);
    ///     ortcClient.OnException += new OnExceptionDelegate(ortc_OnException);
    ///                 
    ///     ortcClient.connect(applicationKey, authenticationToken);
    ///     
    ///     private void ortc_OnConnected(object sender)
    ///     {
    ///         ortcClient.Subscribe("channel1", true, OnMessageCallback);
    ///     }
    ///     
    ///     private void OnMessageCallback(object sender, string channel, string message)
    ///     {
    ///         System.Diagnostics.Debug.Writeline("Message received: " + message);
    ///     }
    ///     private void ortc_OnSubscribed(object sender, string channel)
    ///     {
    ///         ortcClient.Send(channel, "your message");
    ///     }
    ///         
    ///     private void ortc_OnException(object sender, Exception ex)
    ///     {
    ///         System.Diagnostics.Debug.Writeline(ex.Message);
    ///     }
    /// 
    /// </code>
    /// </example>
    public class OrtcClient
    {
        #region Attributes (5)
        internal RealtimeFramework.Messaging.Client _client;
        private string _url;
        private string _clusterUrl;
        private bool _isCluster;
        private int _heartbeatTime;
        private int _heartbeatFail;
        internal string _applicationKey;
        internal string _authenticationToken;
		private string _googleProjectNumber;
		internal string _registrationId;
		internal string _token;
	
        internal SynchronizationContext _synchContext; // To synchronize different contexts, preventing cross-thread operation errors (Windows Application and WPF Application))
        

        #endregion


        #region Properties (9)

        /// <summary>
        /// Gets or sets the client object identifier.
        /// </summary>
        /// <value>Object identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the gateway URL.
        /// </summary>
        /// <value>Gateway URL where the socket is going to connect.</value>
        public string Url {
            get {
                return _url;
            }
            set {
                _isCluster = false;
                _url = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the cluster gateway URL.
        /// </summary>
        public string ClusterUrl {
            get {
                return _clusterUrl;
            }
            set {
                _isCluster = true;
                _clusterUrl = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is cluster.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cluster; otherwise, <c>false</c>.
        /// </value>
        public bool IsCluster {
            get { return _isCluster; }
            set { _isCluster = value; }
        }

        /// <summary>
        /// Gets or sets the connection timeout. Default value is 5000 miliseconds.
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// Gets a value indicating whether this client object is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this client is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the client connection metadata.
        /// </summary>
        /// <value>
        /// Connection metadata.
        /// </value>
        public string ConnectionMetadata { get; set; }

        /// <summary>
        /// Gets or sets the client announcement subchannel.
        /// </summary>
        /// /// <value>
        /// Announcement subchannel.
        /// </value>
        public string AnnouncementSubChannel { get; set; }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>
        /// The session id.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this client has a heartbeat activated.
        /// </summary>
        /// <value>
        /// <c>true</c> if the heartbeat is active; otherwise, <c>false</c>.
        /// </value>
        public bool HeartbeatActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many times can the client fail the heartbeat.
        /// </summary>
        /// <value>
        /// Failure limit.
        /// </value>
        public int HeartbeatFails {
            get { return _heartbeatFail; }
            set { _heartbeatFail = value > Constants.HEARTBEAT_MAX_FAIL ? Constants.HEARTBEAT_MAX_FAIL : (value < Constants.HEARTBEAT_MIN_FAIL ? Constants.HEARTBEAT_MIN_FAIL : value); }
        }

        /// <summary>
        /// Gets or sets the heartbeat interval.
        /// </summary>
        /// <value>
        /// Interval in seconds between heartbeats.
        /// </value>
        public int HeartbeatTime {
            get { return _heartbeatTime; }
            set { _heartbeatTime = value > Constants.HEARTBEAT_MAX_TIME ? Constants.HEARTBEAT_MAX_TIME : (value < Constants.HEARTBEAT_MIN_TIME ? Constants.HEARTBEAT_MIN_TIME : value); }
        }

		/// <summary>
		/// Gets or sets the cluster gateway URL.
		/// </summary>
		public string GoogleProjectNumber {
			get {
				return _googleProjectNumber;
			}
			set {
				if (!String.IsNullOrEmpty (value) && _googleProjectNumber != value) {
					_googleProjectNumber = value.Trim();
					var regId = Settings.RegistrationId;
					if (String.IsNullOrEmpty (regId)) {
						MessagingCenter.Send (this, "GoogleProjectNumber", _googleProjectNumber);
					} else {
						CrossPushNotification.SenderId = _googleProjectNumber;
					}
				}
			}
		}

        #endregion

        #region Delegates

        /// <summary>
        /// Occurs when the client connects to the gateway.
        /// </summary>
        /// <exclude/>
        public delegate void OnConnectedDelegate(object sender);

        /// <summary>
        /// Occurs when the client disconnects from the gateway.
        /// </summary>
        /// <exclude/>
        public delegate void OnDisconnectedDelegate(object sender);

        /// <summary>
        /// Occurs when the client subscribed to a channel.
        /// </summary>
        /// <exclude/>
        public delegate void OnSubscribedDelegate(object sender, string channel);

        /// <summary>
        /// Occurs when the client unsubscribed from a channel.
        /// </summary>
        /// <exclude/>
        public delegate void OnUnsubscribedDelegate(object sender, string channel);

        /// <summary>
        /// Occurs when there is an exception.
        /// </summary>
        /// <exclude/>
        public delegate void OnExceptionDelegate(object sender, Exception ex);

        /// <summary>
        /// Occurs when the client attempts to reconnect to the gateway.
        /// </summary>
        /// <exclude/>
        public delegate void OnReconnectingDelegate(object sender);

        /// <summary>
        /// Occurs when the client reconnected to the gateway.
        /// </summary>
        /// <exclude/>
        public delegate void OnReconnectedDelegate(object sender);

        /// <summary>
        /// Occurs when the client receives a message in the specified channel.
        /// </summary>
        /// <exclude/>
        public delegate void OnMessageDelegate(object sender, string channel, string message);
		/// <summary>
		/// Occurs when the client receives a push notification in the specified channel.
		/// </summary>
		/// <exclude/>
		public delegate void OnPushNotificationDelegate(object sender, string channel, string message, IDictionary<string,object> payload);

        #endregion

        /// <summary>
        /// OrtcClient Constructor
        /// </summary>
        public OrtcClient() {
            ConnectionTimeout = 5;
            

            IsConnected = false;
            IsCluster = false;


            HeartbeatActive = false;
            HeartbeatFails = 3;
            HeartbeatTime = 15;
            
            //_heartbeatTimer.Elapsed += new ElapsedEventHandler(_heartbeatTimer_Elapsed);
            
            // To catch unobserved exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // To use the same context inside the tasks and prevent cross-thread operation errors (Windows Application and WPF Application)
            _synchContext = SynchronizationContext.Current;

            _client = new Client(this);

			if (Device.OS == TargetPlatform.Android) {
				MessagingCenter.Subscribe<CrossPushNotificationListener, string> (this, "RegistrationId", (page, regId) => {
					_registrationId = regId;
				});
			}
        }
			
        #region Public Methods

        /// <summary>
        /// Connects to the gateway with the application key and authentication token. The gateway must be set before using this method.
        /// </summary>
        /// <param name="appKey">Your application key to use ORTC.</param>
        /// <param name="authToken">Authentication token that identifies your permissions.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Connect("myApplicationKey", "myAuthenticationToken");
        ///   </code>
        ///   </example>
        public void Connect(string appKey, string authToken) {
            if (IsConnected) {
                DelegateExceptionCallback(new OrtcAlreadyConnectedException("Already connected"));
            } else if (String.IsNullOrEmpty(ClusterUrl) && String.IsNullOrEmpty(Url)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("URL and Cluster URL are null or empty"));
            } else if (String.IsNullOrEmpty(appKey)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Application Key is null or empty"));
            } else if (String.IsNullOrEmpty(authToken)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Authentication ToKen is null or empty"));
            } else if (!IsCluster && !Url.OrtcIsValidUrl()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Invalid URL"));
            } else if (IsCluster && !ClusterUrl.OrtcIsValidUrl()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Invalid Cluster URL"));
            } else if (!appKey.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Application Key has invalid characters"));
            } else if (!authToken.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Authentication Token has invalid characters"));
            } else if (AnnouncementSubChannel != null && !AnnouncementSubChannel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Announcement Subchannel has invalid characters"));
            } else if (!String.IsNullOrEmpty(ConnectionMetadata) && ConnectionMetadata.Length > Constants.MAX_CONNECTION_METADATA_SIZE) {
                DelegateExceptionCallback(new OrtcMaxLengthException(String.Format("Connection metadata size exceeds the limit of {0} characters", Constants.MAX_CONNECTION_METADATA_SIZE)));
            } else if (_client._isConnecting) {
                DelegateExceptionCallback(new OrtcAlreadyConnectedException("Already trying to connect"));
            } else {
                _client.DoStopReconnecting();

                _authenticationToken = authToken;
                _applicationKey = appKey;

				if (Device.OS == TargetPlatform.Android && !String.IsNullOrEmpty(_googleProjectNumber)) {
					var regId = Settings.RegistrationId;
					if (String.IsNullOrEmpty(regId)) {
						MessagingCenter.Send(this, "GoogleProjectNumber", _googleProjectNumber);
					} else {
						_registrationId = regId;
					}
				}
                _client.DoConnect();
            }
        }


        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="message">Message to be sent.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Send("channelName", "messageToSend");
        ///   </code>
        ///   </example>
        public void Send(string channel, string message) {
            if (!IsConnected) {
                DelegateExceptionCallback(new OrtcNotConnectedException("Not connected"));
            } else if (String.IsNullOrEmpty(channel)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Channel is null or empty"));
            } else if (!channel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Channel has invalid characters"));
            } else if (String.IsNullOrEmpty(message)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Message is null or empty"));
            } else if (channel.Length > Constants.MAX_CHANNEL_SIZE) {
                DelegateExceptionCallback(new OrtcMaxLengthException(String.Format("Channel size exceeds the limit of {0} characters", Constants.MAX_CHANNEL_SIZE)));
            } else {
                _client.Send(channel, message);                
            }
        }

        /// <summary>
        /// Sends a message to a channel for a specific application key
        /// </summary>
        /// <param name="applicationKey">An application key</param>
        /// <param name="privateKey">A private key</param>
        /// <param name="channel">A channel name</param>
        /// <param name="message">A message to send</param>
        public void SendProxy(string applicationKey, string privateKey, string channel, string message) {
            if (!IsConnected) {
                DelegateExceptionCallback(new OrtcNotConnectedException("Not connected"));
            } else if (String.IsNullOrEmpty(applicationKey)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Application Key is null or empty"));
            } else if (String.IsNullOrEmpty(privateKey)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Private Key is null or empty"));
            } else if (String.IsNullOrEmpty(channel)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Channel is null or empty"));
            } else if (!channel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Channel has invalid characters"));
            } else if (String.IsNullOrEmpty(message)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Message is null or empty"));
            } else if (channel.Length > Constants.MAX_CHANNEL_SIZE) {
                DelegateExceptionCallback(new OrtcMaxLengthException(String.Format("Channel size exceeds the limit of {0} characters", Constants.MAX_CHANNEL_SIZE)));
            } else { 
                _client.SendProxy(applicationKey, privateKey, channel, message);
            }
        }

        /// <summary>
        /// Subscribes to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="subscribeOnReconnected">Subscribe to the specified channel on reconnect.</param>
        /// <param name="onMessage"><see cref="OnMessageDelegate"/> callback.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Subscribe("channelName", true, OnMessageCallback);
        /// private void OnMessageCallback(object sender, string channel, string message)
        /// {
        /// // Do something
        /// }
        ///   </code>
        ///   </example>
        public void Subscribe(string channel, bool subscribeOnReconnected, OnMessageDelegate onMessage) {
            if (!IsConnected) {
                DelegateExceptionCallback(new OrtcNotConnectedException("Not connected"));
            } else if (String.IsNullOrEmpty(channel)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Channel is null or empty"));
            } else if (!channel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Channel has invalid characters"));
            } else if(channel.Length > Constants.MAX_CHANNEL_SIZE) {
                DelegateExceptionCallback(new OrtcMaxLengthException(String.Format("Channel size exceeds the limit of {0} characters", Constants.MAX_CHANNEL_SIZE)));
            } else if (_client._subscribedChannels.ContainsKey(channel)) {
                ChannelSubscription channelSubscription = null;
                _client._subscribedChannels.TryGetValue(channel, out channelSubscription);

                if (channelSubscription != null) {
                    if (channelSubscription.IsSubscribing) {
                        DelegateExceptionCallback(new OrtcSubscribedException(String.Format("Already subscribing to the channel {0}", channel)));
                    } else if (channelSubscription.IsSubscribed) {
                        DelegateExceptionCallback(new OrtcSubscribedException(String.Format("Already subscribed to the channel {0}", channel)));
                    } else {
                        _client.subscribe(channel, subscribeOnReconnected, onMessage, false);
                    }
                }
            } else {
                _client.subscribe(channel, subscribeOnReconnected, onMessage, false);          
            }
        }

		/// <summary>
		/// Subscribes to a channel with notifications.
		/// </summary>
		/// <param name="channel">Channel name.</param>
		/// <param name="subscribeOnReconnected">Subscribe to the specified channel on reconnect.</param>
		/// <param name="onMessage"><see cref="OnMessageDelegate"/> callback.</param>
		/// <example>
		///   <code>
		/// ortcClient.SubscribeWithNotifications("channelName", true, OnMessageCallback);
		/// private void OnMessageCallback(object sender, string channel, string message)
		/// {
		/// // Do something
		/// }
		///   </code>
		///   </example>
		public void SubscribeWithNotifications(string channel, bool subscribeOnReconnected, OnMessageDelegate onMessage) {
			if (Device.OS == TargetPlatform.iOS || Device.OS == TargetPlatform.Android) {
				if (!IsConnected) {
					DelegateExceptionCallback (new OrtcNotConnectedException ("Not connected"));
				} else if (String.IsNullOrEmpty (channel)) {
					DelegateExceptionCallback (new OrtcEmptyFieldException ("Channel is null or empty"));
				} else if (!channel.OrtcIsValidInput ()) {
					DelegateExceptionCallback (new OrtcInvalidCharactersException ("Channel has invalid characters"));
				} else if (channel.Length > Constants.MAX_CHANNEL_SIZE) {
					DelegateExceptionCallback (new OrtcMaxLengthException (String.Format ("Channel size exceeds the limit of {0} characters", Constants.MAX_CHANNEL_SIZE)));
				} else if (_client._subscribedChannels.ContainsKey (channel)) {
					ChannelSubscription channelSubscription = null;
					_client._subscribedChannels.TryGetValue (channel, out channelSubscription);

					if (channelSubscription != null) {
						if (channelSubscription.IsSubscribing) {
							DelegateExceptionCallback (new OrtcSubscribedException (String.Format ("Already subscribing to the channel {0}", channel)));
						} else if (channelSubscription.IsSubscribed) {
							DelegateExceptionCallback (new OrtcSubscribedException (String.Format ("Already subscribed to the channel {0}", channel)));
						} else {
							_client.subscribe (channel, subscribeOnReconnected, onMessage, true);
						}
					}
				} else if (Device.OS == TargetPlatform.Android && String.IsNullOrEmpty (_googleProjectNumber)) {
					DelegateExceptionCallback (new OrtcGcmException ("You have to provide a your Google Project ID to use the GCM notifications"));
				} else {
					_client.subscribe (channel, subscribeOnReconnected, onMessage, true);          
				}
			} else {
				DelegateExceptionCallback (new OrtcNotSupportedPlatformException ("Subscribe with notifications is only available on platforms Android/iOS"));
			}
		}

        /// <summary>
        /// Unsubscribes from a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Unsubscribe("channelName");
        ///   </code>
        ///   </example>
        public void Unsubscribe(string channel) {
            if (!IsConnected) {
                DelegateExceptionCallback(new OrtcNotConnectedException("Not connected"));                
            } else if (String.IsNullOrEmpty(channel)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Channel is null or empty"));
            } else if (!channel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Channel has invalid characters"));
            } else if (!_client._subscribedChannels.ContainsKey(channel)) {
                DelegateExceptionCallback(new OrtcNotSubscribedException(String.Format("Not subscribed to the channel {0}", channel)));
            } else if (channel.Length > Constants.MAX_CHANNEL_SIZE){
                DelegateExceptionCallback(new OrtcMaxLengthException(String.Format("Channel size exceeds the limit of {0} characters", Constants.MAX_CHANNEL_SIZE)));
            } else if (_client._subscribedChannels.ContainsKey(channel)) {
                ChannelSubscription channelSubscription = null;
                _client._subscribedChannels.TryGetValue(channel, out channelSubscription);
                if (channelSubscription != null && !channelSubscription.IsSubscribed) {
                   DelegateExceptionCallback(new OrtcNotSubscribedException(String.Format("Not subscribed to the channel {0}", channel)));
                } else {
					_client.unsubscribe(channel, channelSubscription.isWithNotification,Device.OS); 
                }
            } else {
                DelegateExceptionCallback(new OrtcNotSubscribedException(String.Format("Not subscribed to the channel {0}", channel)));
            }
        }

        /// <summary>
        /// Disconnects from the gateway.
        /// </summary>
        /// <example>
        ///   <code>
        /// ortcClient.Disconnect();
        ///   </code>
        ///   </example>
        public void Disconnect() {
            _client.DoStopReconnecting();

            // Clear subscribed channels
            _client._subscribedChannels.Clear();

			if (!IsConnected) {
				_client.DoDisconnect();
				DelegateDisconnectedCallback ();
			} else {
				_client.DoDisconnect();
            }
        }

        /// <summary>
        /// Indicates whether is subscribed to a channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <returns>
        ///   <c>true</c> if subscribed to the channel; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSubscribed(string channel) {
            bool result = false;

            if (!IsConnected) {
                DelegateExceptionCallback(new OrtcNotConnectedException("Not connected"));
            } else if (String.IsNullOrEmpty(channel)) {
                DelegateExceptionCallback(new OrtcEmptyFieldException("Channel is null or empty"));
            } else if (!channel.OrtcIsValidInput()) {
                DelegateExceptionCallback(new OrtcInvalidCharactersException("Channel has invalid characters"));
            } else {
                result = false;

                if (_client._subscribedChannels.ContainsKey(channel)) {
                    ChannelSubscription channelSubscription = null;
                    _client._subscribedChannels.TryGetValue(channel, out channelSubscription);

                    if (channelSubscription != null && channelSubscription.IsSubscribed) {
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the subscriptions in the specified channel and if active the first 100 unique metadata.
        /// </summary>
        /// <param name="channel">Channel with presence data active.</param>
        /// <param name="callback"><see cref="OnPresenceDelegate"/>Callback with error <see cref="OrtcPresenceException"/> and result <see cref="Presence"/>.</param>
        /// <example>
        /// <code>
        /// client.Presence("presence-channel", (error, result) =>
        /// {
        ///     if (error != null)
        ///     {
        ///         System.Diagnostics.Debug.Writeline(error.Message);
        ///     }
        ///     else
        ///     {
        ///         if (result != null)
        ///         {
        ///             System.Diagnostics.Debug.Writeline(result.Subscriptions);
        /// 
        ///             if (result.Metadata != null)
        ///             {
        ///                 foreach (var metadata in result.Metadata)
        ///                 {
        ///                     System.Diagnostics.Debug.Writeline(metadata.Key + " - " + metadata.Value);
        ///                 }
        ///             }
        ///         }
        ///         else
        ///         {
        ///             System.Diagnostics.Debug.Writeline("There is no presence data");
        ///         }
        ///     }
        /// });
        /// </code>
        /// </example>
        public void Presence(String channel, OnPresenceDelegate callback) {
            var isCluster = !String.IsNullOrEmpty(this.ClusterUrl);
            var url = String.IsNullOrEmpty(this.ClusterUrl) ? this.Url : this.ClusterUrl;

            Ext.Presence.GetPresence(url, isCluster, this._applicationKey, this._authenticationToken, channel, callback);
        }

        /// <summary>
        /// Enables presence for the specified channel with first 100 unique metadata if metadata is set to true.
        /// </summary>
        /// <param name="privateKey">The private key provided when the ORTC service is purchased.</param>
        /// <param name="channel">Channel to activate presence.</param>
        /// <param name="metadata">Defines if to collect first 100 unique metadata.</param>
        /// <param name="callback">Callback with error <see cref="OrtcPresenceException"/> and result.</param>
        /// /// <example>
        /// <code>
        /// client.EnablePresence("myPrivateKey", "presence-channel", false, (error, result) =>
        /// {
        ///     if (error != null)
        ///     {
        ///         System.Diagnostics.Debug.Writeline(error.Message);
        ///     }
        ///     else
        ///     {
        ///         System.Diagnostics.Debug.Writeline(result);
        ///     }
        /// });
        /// </code>
        /// </example>
        public void EnablePresence(String privateKey, String channel, bool metadata, OnEnablePresenceDelegate callback) {
            var isCluster = !String.IsNullOrEmpty(this.ClusterUrl);
            var url = String.IsNullOrEmpty(this.ClusterUrl) ? this.Url : this.ClusterUrl;

            Ext.Presence.EnablePresence(url, isCluster, this._applicationKey, privateKey, channel, metadata, callback);
        }

        /// <summary>
        /// Disables presence for the specified channel.
        /// </summary>
        /// <param name="privateKey">The private key provided when the ORTC service is purchased.</param>
        /// <param name="channel">Channel to disable presence.</param>
        /// <param name="callback">Callback with error <see cref="OrtcPresenceException"/> and result.</param>        
        public void DisablePresence(String privateKey, String channel, OnDisablePresenceDelegate callback) {
            var isCluster = !String.IsNullOrEmpty(this.ClusterUrl);
            var url = String.IsNullOrEmpty(this.ClusterUrl) ? this.Url : this.ClusterUrl;

            Ext.Presence.DisablePresence(url, isCluster, this._applicationKey, privateKey, channel, callback);
        }

		/// <summary>
		/// Enables push Notifications
		/// </summary>
		/// <param name="callback">Callback when push notification is received.     
		public void SetOnPushNotification(OrtcClient.OnPushNotificationDelegate callback) {

			MessagingCenter.Subscribe<CrossPushNotificationListener, CrossPushNotificationMessage> (this, "DelegatePushNotification", (page, pushNotificationData) => {
				DelegatePushNotificationCallback(pushNotificationData.channel, pushNotificationData.message, pushNotificationData.payload);
			});

			OnPushNotification += callback;
			if (Device.OS == TargetPlatform.Android) {
				MessagingCenter.Send (this, "SetOnPushNotification");
			}
			else if (Device.OS == TargetPlatform.iOS){
				if (String.IsNullOrEmpty(Settings.Token)) {
					CrossPushNotification.Current.Register ();
				} else {
					_token = Settings.Token;
				}
			}
		}
			
        #endregion


        /// <summary>
        /// Occurs when a connection attempt was successful.
        /// </summary>
        public event OrtcClient.OnConnectedDelegate OnConnected;

        /// <summary>
        /// Occurs when the client connection terminated. 
        /// </summary>
        public event OrtcClient.OnDisconnectedDelegate OnDisconnected;

        /// <summary>
        /// Occurs when the client subscribed to a channel.
        /// </summary>
        public event OrtcClient.OnSubscribedDelegate OnSubscribed;

        /// <summary>
        /// Occurs when the client unsubscribed from a channel.
        /// </summary>
        public event OrtcClient.OnUnsubscribedDelegate OnUnsubscribed;

        /// <summary>
        /// Occurs when there is an error.
        /// </summary>
        public event OrtcClient.OnExceptionDelegate OnException;

        /// <summary>
        /// Occurs when a client attempts to reconnect.
        /// </summary>
        public event OrtcClient.OnReconnectingDelegate OnReconnecting;

        /// <summary>
        /// Occurs when a client reconnected.
        /// </summary>
        public event OrtcClient.OnReconnectedDelegate OnReconnected;

		/// <summary>
		/// Occurs when a push notification is received.
		/// </summary>
		private event OrtcClient.OnPushNotificationDelegate OnPushNotification;

        internal void DelegateConnectedCallback() {
            var ev = OnConnected;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj), this);
                } else {
                    Task.Factory.StartNew(() => ev(this));
                }
            }
        }

        internal void DelegateDisconnectedCallback() {
            var ev = OnDisconnected;
            
            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj), this);
                } else {
                    Task.Factory.StartNew(() => ev(this));
                }
            }
        }

        internal void DelegateSubscribedCallback(string channel) {
            var ev = OnSubscribed;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj, channel), this);
                } else {
                    Task.Factory.StartNew(() => ev(this, channel));
                }
            }
        }

        internal void DelegateUnsubscribedCallback(string channel) {
            var ev = OnUnsubscribed;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj, channel), this);
                } else {
                    Task.Factory.StartNew(() => ev(this, channel));
                }
            }
        }

        internal void DelegateExceptionCallback(Exception ex) {
            var ev = OnException;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj, ex), this);
                } else {
                    Task.Factory.StartNew(() => ev(this, ex));
                }
            }
        }

        internal void DelegateReconnectingCallback() {
            var ev = OnReconnecting;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj), this);
                } else {
                    Task.Factory.StartNew(() => ev(this));
                }
            }
        }

        internal void DelegateReconnectedCallback() {
            var ev = OnReconnected;

            if (ev != null) {
                if (_synchContext != null) {
                    _synchContext.Post(obj => ev(obj), this);
                } else {
                    Task.Factory.StartNew(() => ev(this));
                }
            }
        }

		internal void DelegatePushNotificationCallback(string channel, string message, IDictionary<string, object> payload) {
			var ev = OnPushNotification;

			if (ev != null) {
				if (_synchContext != null) {
					_synchContext.Post(obj => ev(obj, channel, message, payload), this);
				} else {
					Task.Factory.StartNew(() => ev(this, channel, message, payload));
				}
			}
		}


        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            e.SetObserved();
        }
    }
}
