using System.Collections.Generic;

namespace Realtime.Messaging.Helpers
{
	public class CrossPushNotificationMessage
	{
		public string channel {
			get;
			set;
		}
		public string message {
			get;
			set;
		}
		public IDictionary<string,object> payload {
			get;
			set;
		}
	}
}

