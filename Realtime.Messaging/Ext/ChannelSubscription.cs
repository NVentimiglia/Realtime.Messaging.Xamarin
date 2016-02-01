namespace Realtime.Messaging.Ext {
    internal class ChannelSubscription {
        private bool isSubscribing;
        internal bool IsSubscribing {
            get { return isSubscribing; }
            set {
                if (value) {
                    isSubscribed = false;
                }
                isSubscribing = value;
            }
        }

        private bool isSubscribed;
        internal bool IsSubscribed {
            get { return isSubscribed; }
            set {
                if (value) {
                    isSubscribing = false;
                }
                isSubscribed = value;
            }
        }

        internal bool SubscribeOnReconnected { get; set; }
        internal OrtcClient.OnMessageDelegate OnMessage { get; set; }

		internal bool isWithNotification { get; set; }

		internal ChannelSubscription(bool subscribeOnReconnected, OrtcClient.OnMessageDelegate onMessage, bool withNotification) {
            this.SubscribeOnReconnected = subscribeOnReconnected;
            this.OnMessage = onMessage;
            this.IsSubscribed = false;
            this.IsSubscribing = false;
			this.isWithNotification = withNotification;
        }

        internal ChannelSubscription() {
            
        }
    }
}
