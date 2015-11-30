using System;
using System.Collections.Generic;

namespace Realtime.Messaging
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

