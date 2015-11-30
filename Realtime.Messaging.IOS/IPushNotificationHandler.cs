using System;
using System.Collections.Generic;
#if __UNIFIED__
using Foundation;
#else
using MonoTouch.Foundation;
#endif

namespace Realtime.Messaging.IOS
{
	public interface  IPushNotificationHandler
	{
		void OnMessageReceived(NSDictionary parameters);
		void OnErrorReceived(NSError error);
		void OnRegisteredSuccess(NSData token);
	}
}

