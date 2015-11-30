using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Realtime.Messaging;
using Realtime.Messaging.IOS;

namespace Messaging.Sample.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Realtime.Messaging.IOS.WebsocketConnection.Link();

			Realtime.Messaging.CrossPushNotification.Initialize<Realtime.Messaging.CrossPushNotificationListener> ();

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());



            return base.FinishedLaunching(app, options);
        }

		const string TAG = "PushNotification-APN";
		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{

			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnErrorReceived(error);
			}


		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnRegisteredSuccess(deviceToken);
			}

		}

		public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
		{
			application.RegisterForRemoteNotifications();
		}


        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived(userInfo);
			}
        }
        

		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnMessageReceived(userInfo);
			}
		}
    }
}
