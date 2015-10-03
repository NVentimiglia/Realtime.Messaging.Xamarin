using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RealtimeFramework.Messaging.Exceptions;

namespace RealtimeFramework.Messaging.Ext {
    internal class MsgProcessor {
        private Client client;

        internal MsgProcessor(Client context) {
            this.client = context;
        }

        /// <summary>
        /// Processes the operation validated.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        internal void ProcessOperationValidated(string arguments) {
            if (!String.IsNullOrEmpty(arguments)) {

                bool isValid = false;

                // Try to match with authentication
                Match validatedAuthMatch = Regex.Match(arguments, Constants.VALIDATED_PATTERN);

                if (validatedAuthMatch.Success) {
                    isValid = true;

                    string userPermissions = String.Empty;

                    if (validatedAuthMatch.Groups["up"].Length > 0) {
                        userPermissions = validatedAuthMatch.Groups["up"].Value;
                    }

                    if (validatedAuthMatch.Groups["set"].Length > 0) {
                        //_sessionExpirationTime = int.Parse(validatedAuthMatch.Groups["set"].Value);
                    }
                    /*
                    if (String.IsNullOrEmpty(ReadLocalStorage(_applicationKey, _sessionExpirationTime))) {
                        CreateLocalStorage(_applicationKey);
                    }*/

                    if (!String.IsNullOrEmpty(userPermissions) && userPermissions != "null") {
                        MatchCollection matchCollection = Regex.Matches(userPermissions, Constants.PERMISSIONS_PATTERN);

                        var permissions = new List<KeyValuePair<string, string>>();

                        foreach (Match match in matchCollection) {
                            string channel = match.Groups["key"].Value;
                            string hash = match.Groups["value"].Value;

                            permissions.Add(new KeyValuePair<string, string>(channel, hash));
                        }

                        client._permissions = new List<KeyValuePair<string, string>>(permissions);
                    }
                }

                if (isValid) {
                    client._isConnecting = false;
                    client.context.IsConnected = true;
                    if (client.context.HeartbeatActive) {
                        client.startHeartbeat();
                        
                        //_heartbeatTimer.Interval = HeartbeatTime * 1000;
                        //_heartbeatTimer.Start();
                    }
                    if (client._alreadyConnectedFirstTime) {
                        List<String> channelsToRemove = new List<String>();

                        // Subscribe to the previously subscribed channels
                        foreach (KeyValuePair<string, ChannelSubscription> item in client._subscribedChannels) {
                            string channel = item.Key;
                            ChannelSubscription channelSubscription = item.Value;

                            // Subscribe again
                            if (channelSubscription.SubscribeOnReconnected && (channelSubscription.IsSubscribing || channelSubscription.IsSubscribed)) {
                                channelSubscription.IsSubscribing = true;
                                channelSubscription.IsSubscribed = false;

                                var domainChannelCharacterIndex = channel.IndexOf(':');
                                var channelToValidate = channel;

                                if (domainChannelCharacterIndex > 0) {
                                    channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
                                }

                                string hash = client._permissions.Where(c => c.Key == channel || c.Key == channelToValidate).FirstOrDefault().Value;

                                string s = String.Format("subscribe;{0};{1};{2};{3}", client.context._applicationKey, client.context._authenticationToken, channel, hash);

                                client.DoSend(s);
                            } else {
                                channelsToRemove.Add(channel);
                            }
                        }

                        for (int i = 0; i < channelsToRemove.Count; i++) {
                            ChannelSubscription removeResult = null;
                            client._subscribedChannels.TryRemove(channelsToRemove[i], out removeResult);
                        }

                        // Clean messages buffer (can have lost message parts in memory)
                        client._multiPartMessagesBuffer.Clear();

                        client.context.DelegateReconnectedCallback();
                    } else {
                        client._alreadyConnectedFirstTime = true;

                        // Clear subscribed channels
                        client._subscribedChannels.Clear();

                        client.context.DelegateConnectedCallback();
                    }

                    if (arguments.IndexOf("busy") < 0)
                    {
                        client._reconnectTimer.Stop();
                    }

                    client._callDisconnectedCallback = true;
                }
            }
        }

        /// <summary>
        /// Processes the operation subscribed.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        internal void ProcessOperationSubscribed(string arguments) {
            if (!String.IsNullOrEmpty(arguments)) {
                Match subscribedMatch = Regex.Match(arguments, Constants.CHANNEL_PATTERN);

                if (subscribedMatch.Success) {
                    string channelSubscribed = String.Empty;

                    if (subscribedMatch.Groups["channel"].Length > 0) {
                        channelSubscribed = subscribedMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(channelSubscribed)) {
                        ChannelSubscription channelSubscription = null;
                        client._subscribedChannels.TryGetValue(channelSubscribed, out channelSubscription);

                        if (channelSubscription != null) {
                            channelSubscription.IsSubscribing = false;
                            channelSubscription.IsSubscribed = true;
                        }

                        client.context.DelegateSubscribedCallback(channelSubscribed);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation unsubscribed.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        internal void ProcessOperationUnsubscribed(string arguments) {
            if (!String.IsNullOrEmpty(arguments)) {
                Match unsubscribedMatch = Regex.Match(arguments, Constants.CHANNEL_PATTERN);

                if (unsubscribedMatch.Success) {
                    string channelUnsubscribed = String.Empty;

                    if (unsubscribedMatch.Groups["channel"].Length > 0) {
                        channelUnsubscribed = unsubscribedMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(channelUnsubscribed)) {
                        ChannelSubscription channelSubscription = null;
                        client._subscribedChannels.TryGetValue(channelUnsubscribed, out channelSubscription);

                        if (channelSubscription != null) {
                            channelSubscription.IsSubscribed = false;
                        }

                        client.context.DelegateUnsubscribedCallback(channelUnsubscribed);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation error.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        internal void ProcessOperationError(string arguments) {
            if (!String.IsNullOrEmpty(arguments)) {
                Match errorMatch = Regex.Match(arguments, Constants.EXCEPTION_PATTERN);

                if (errorMatch.Success) {
                    string op = String.Empty;
                    string error = String.Empty;
                    string channel = String.Empty;

                    if (errorMatch.Groups["op"].Length > 0) {
                        op = errorMatch.Groups["op"].Value;
                    }

                    if (errorMatch.Groups["error"].Length > 0) {
                        error = errorMatch.Groups["error"].Value;
                    }

                    if (errorMatch.Groups["channel"].Length > 0) {
                        channel = errorMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(error)) {
                        client.context.DelegateExceptionCallback(new OrtcGenericException(error));
                    }

                    if (!String.IsNullOrEmpty(op)) {
                        switch (op) {
                            case "validate":
                                if (!String.IsNullOrEmpty(error) && (error.Contains("Unable to connect") || error.Contains("Server is too busy"))) {
                                    client.context.IsConnected = false;
                                    client.DoReconnect();
                                } else {
                                    client.DoStopReconnecting();
                                }
                                break;
                            case "subscribe":
                                if (!String.IsNullOrEmpty(channel)) {
                                    ChannelSubscription channelSubscription = null;
                                    client._subscribedChannels.TryGetValue(channel, out channelSubscription);

                                    if (channelSubscription != null) {
                                        channelSubscription.IsSubscribing = false;
                                    }
                                }
                                break;
                            case "subscribe_maxsize":
                            case "unsubscribe_maxsize":
                            case "send_maxsize":
                                if (!String.IsNullOrEmpty(channel)) {
                                    ChannelSubscription channelSubscription = null;
                                    client._subscribedChannels.TryGetValue(channel, out channelSubscription);

                                    if (channelSubscription != null) {
                                        channelSubscription.IsSubscribing = false;
                                    }
                                }

                                client.DoStopReconnecting();
                                client.DoDisconnect();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation received.
        /// </summary>
        /// <param name="message">The message.</param>
        //private void OldProcessOperationReceived(string message)
        //{
        //    Match receivedMatch = Regex.Match(message, RECEIVED_PATTERN);

        //    // Received
        //    if (receivedMatch.Success)
        //    {
        //        string channelReceived = String.Empty;
        //        string messageReceived = String.Empty;

        //        if (receivedMatch.Groups["channel"].Length > 0)
        //        {
        //            channelReceived = receivedMatch.Groups["channel"].Value;
        //        }

        //        if (receivedMatch.Groups["message"].Length > 0)
        //        {
        //            messageReceived = receivedMatch.Groups["message"].Value;
        //        }

        //        if (!String.IsNullOrEmpty(channelReceived) && !String.IsNullOrEmpty(messageReceived) && _subscribedChannels.ContainsKey(channelReceived))
        //        {
        //            messageReceived = messageReceived.Replace(@"\\n", Environment.NewLine).Replace("\\\\\"", @"""").Replace("\\\\\\\\", @"\");

        //            // Multi part
        //            Match multiPartMatch = Regex.Match(messageReceived, MULTI_PART_MESSAGE_PATTERN);

        //            string messageId = String.Empty;
        //            int messageCurrentPart = 1;
        //            int messageTotalPart = 1;
        //            bool lastPart = false;
        //            List<BufferedMessage> messageParts = null;

        //            if (multiPartMatch.Success)
        //            {
        //                if (multiPartMatch.Groups["messageId"].Length > 0)
        //                {
        //                    messageId = multiPartMatch.Groups["messageId"].Value;
        //                }

        //                if (multiPartMatch.Groups["messageCurrentPart"].Length > 0)
        //                {
        //                    messageCurrentPart = Int32.Parse(multiPartMatch.Groups["messageCurrentPart"].Value);
        //                }

        //                if (multiPartMatch.Groups["messageTotalPart"].Length > 0)
        //                {
        //                    messageTotalPart = Int32.Parse(multiPartMatch.Groups["messageTotalPart"].Value);
        //                }

        //                if (multiPartMatch.Groups["message"].Length > 0)
        //                {
        //                    messageReceived = multiPartMatch.Groups["message"].Value;
        //                }
        //            }

        //            // Is a message part
        //            if (!String.IsNullOrEmpty(messageId))
        //            {
        //                if (!_multiPartMessagesBuffer.ContainsKey(messageId))
        //                {
        //                    _multiPartMessagesBuffer.TryAdd(messageId, new List<BufferedMessage>());
        //                }

        //                _multiPartMessagesBuffer.TryGetValue(messageId, out messageParts);

        //                if (messageParts != null)
        //                {
        //                    messageParts.Add(new BufferedMessage(messageCurrentPart, messageReceived));

        //                    // Last message part
        //                    if (messageParts.Count == messageTotalPart)
        //                    {
        //                        messageParts.Sort();

        //                        lastPart = true;
        //                    }
        //                }
        //            }
        //            // Message does not have multipart, like the messages received at announcement channels
        //            else
        //            {
        //                lastPart = true;
        //            }

        //            if (lastPart)
        //            {
        //                if (_subscribedChannels.ContainsKey(channelReceived))
        //                {
        //                    ChannelSubscription channelSubscription = null;
        //                    _subscribedChannels.TryGetValue(channelReceived, out channelSubscription);

        //                    if (channelSubscription != null)
        //                    {
        //                        var ev = channelSubscription.OnMessage;

        //                        if (ev != null)
        //                        {
        //                            if (!String.IsNullOrEmpty(messageId) && _multiPartMessagesBuffer.ContainsKey(messageId))
        //                            {
        //                                messageReceived = String.Empty;
        //                                lock (messageParts)
        //                                {
        //                                    foreach (BufferedMessage part in messageParts)
        //                                    {
        //                                        if (part != null)
        //                                        {
        //                                            messageReceived = String.Format("{0}{1}", messageReceived, part.Message);
        //                                        }
        //                                    }
        //                                }

        //                                // Remove from messages buffer
        //                                List<BufferedMessage> removeResult = null;
        //                                _multiPartMessagesBuffer.TryRemove(messageId, out removeResult);
        //                            }

        //                            if (!String.IsNullOrEmpty(messageReceived))
        //                            {
        //                                if (_synchContext != null)
        //                                {
        //                                    _synchContext.Post(obj => ev(obj, channelReceived, messageReceived), this);
        //                                }
        //                                else
        //                                {
        //                                    ev(this, channelReceived, messageReceived);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Unknown
        //        DelegateExceptionCallback(new OrtcGenericException(String.Format("Unknown message received: {0}", message)));

        //        //DoDisconnect();
        //    }
        //}

        internal void ProcessOperationReceived(string message) {
            Match receivedMatch = Regex.Match(message, Constants.RECEIVED_PATTERN);

            // Received
            if (receivedMatch.Success) {
                string channelReceived = String.Empty;
                string messageReceived = String.Empty;

                if (receivedMatch.Groups["channel"].Length > 0) {
                    channelReceived = receivedMatch.Groups["channel"].Value;
                }

                if (receivedMatch.Groups["message"].Length > 0) {
                    messageReceived = receivedMatch.Groups["message"].Value;
                }

                if (!String.IsNullOrEmpty(channelReceived) && !String.IsNullOrEmpty(messageReceived) && client._subscribedChannels.ContainsKey(channelReceived)) {
                    messageReceived = messageReceived.Replace(@"\\n", Environment.NewLine).Replace("\\\\\"", @"""").Replace("\\\\\\\\", @"\");

                    // Multi part
                    Match multiPartMatch = Regex.Match(messageReceived, Constants.MULTI_PART_MESSAGE_PATTERN);

                    string messageId = String.Empty;
                    int messageCurrentPart = 1;
                    int messageTotalPart = 1;
                    bool lastPart = false;
                    RealtimeDictionary<int, BufferedMessage> messageParts = null;

                    if (multiPartMatch.Success) {
                        if (multiPartMatch.Groups["messageId"].Length > 0) {
                            messageId = multiPartMatch.Groups["messageId"].Value;
                        }

                        if (multiPartMatch.Groups["messageCurrentPart"].Length > 0) {
                            messageCurrentPart = Int32.Parse(multiPartMatch.Groups["messageCurrentPart"].Value);
                        }

                        if (multiPartMatch.Groups["messageTotalPart"].Length > 0) {
                            messageTotalPart = Int32.Parse(multiPartMatch.Groups["messageTotalPart"].Value);
                        }

                        if (multiPartMatch.Groups["message"].Length > 0) {
                            messageReceived = multiPartMatch.Groups["message"].Value;
                        }
                    }

                    lock (client._multiPartMessagesBuffer) {
                        // Is a message part
                        if (!String.IsNullOrEmpty(messageId)) {
                            if (!client._multiPartMessagesBuffer.ContainsKey(messageId)) {
                                client._multiPartMessagesBuffer.Add(messageId, new RealtimeDictionary<int, BufferedMessage>());
                            }


                            client._multiPartMessagesBuffer.TryGetValue(messageId, out messageParts);

                            if (messageParts != null) {
                                lock (messageParts) {

                                    messageParts.Add(messageCurrentPart, new BufferedMessage(messageCurrentPart, messageReceived));

                                    // Last message part
                                    if (messageParts.Count == messageTotalPart) {
                                        //messageParts.Sort();

                                        lastPart = true;
                                    }
                                }
                            }
                        }
                            // Message does not have multipart, like the messages received at announcement channels
                        else {
                            lastPart = true;
                        }

                        if (lastPart) {
                            if (client._subscribedChannels.ContainsKey(channelReceived)) {
                                ChannelSubscription channelSubscription = null;
                                client._subscribedChannels.TryGetValue(channelReceived, out channelSubscription);

                                if (channelSubscription != null) {
                                    var ev = channelSubscription.OnMessage;

                                    if (ev != null) {
                                        if (!String.IsNullOrEmpty(messageId) && client._multiPartMessagesBuffer.ContainsKey(messageId)) {
                                            messageReceived = String.Empty;
                                            //lock (messageParts)
                                            //{
                                            var bufferedMultiPartMessages = new List<BufferedMessage>();

                                            foreach (var part in messageParts.Keys) {
                                                bufferedMultiPartMessages.Add(messageParts[part]);
                                            }

                                            bufferedMultiPartMessages.Sort();

                                            foreach (var part in bufferedMultiPartMessages) {
                                                if (part != null) {
                                                    messageReceived = String.Format("{0}{1}", messageReceived, part.Message);
                                                }
                                            }
                                            //}

                                            // Remove from messages buffer
                                            RealtimeDictionary<int, BufferedMessage> removeResult = null;
                                            client._multiPartMessagesBuffer.TryRemove(messageId, out removeResult);
                                        }

                                        if (!String.IsNullOrEmpty(messageReceived)) {
                                            if (client.context._synchContext != null) {
                                                client.context._synchContext.Post(obj => ev(obj, channelReceived, messageReceived), this);
                                            } else {
                                                ev(this, channelReceived, messageReceived);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } else {
                // Unknown
                client.context.DelegateExceptionCallback(new OrtcGenericException(String.Format("Unknown message received: {0}", message)));

                //DoDisconnect();
            }
        }


    }
}
