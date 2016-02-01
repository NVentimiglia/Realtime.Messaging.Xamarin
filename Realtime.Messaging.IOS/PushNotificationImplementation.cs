#if __UNIFIED__
using Foundation;
using UIKit;
using Realtime.Messaging.IOS;


#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Realtime.Messaging.Exceptions;
using Realtime.Messaging.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(PushNotificationImplementation))]

namespace Realtime.Messaging.IOS
{
	[Preserve]
	public class PushNotificationImplementation : IPushNotification,  IPushNotificationHandler
	{

		public string Token {
			get {
				return NSUserDefaults.StandardUserDefaults.StringForKey ("token");
			}

		}

		public void Register()
		{
			if (!CrossPushNotification.IsInitialized)
			{

				throw NewPushNotificationNotInitializedException();
			}

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				UIUserNotificationType userNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
				UIUserNotificationSettings settings = UIUserNotificationSettings.GetSettingsForTypes(userNotificationTypes, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			}
			else
			{
				UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
			} 
		}

		public void Unregister()
		{
			if (!CrossPushNotification.IsInitialized)
			{

				throw NewPushNotificationNotInitializedException();
			}
			UIApplication.SharedApplication.UnregisterForRemoteNotifications();



		}

		#region IPushNotificationListener implementation

		public void OnMessageReceived (NSDictionary userInfo)
		{
			Console.WriteLine ("OnMessageReceived");
			var parameters = new Dictionary<string, object>();

			foreach (NSString key in userInfo.Keys)
			{
				if(key == "aps")
				{
					NSDictionary aps = userInfo.ValueForKey(key) as NSDictionary;

					if(aps != null)
					{
						foreach(var apsKey in aps)
							parameters.Add(apsKey.Key.ToString(), apsKey.Value);
					}
				}
				parameters.Add(key, userInfo.ValueForKey(key));
			}

			if (CrossPushNotification.IsInitialized)
			{
				CrossPushNotification.PushNotificationListener.OnMessage(parameters, DeviceType.iOS);
			}else
			{
				throw NewPushNotificationNotInitializedException();
			}
		}

		public void OnErrorReceived (NSError error)
		{
			Debug.WriteLine("{0} - Registration Failed.", "CrossPushNotification");

			if (CrossPushNotification.IsInitialized)
			{
				CrossPushNotification.PushNotificationListener.OnError(error.LocalizedDescription, DeviceType.iOS);
			}
			else
			{
				throw NewPushNotificationNotInitializedException();
			}
		}

		public void OnRegisteredSuccess (NSData token)
		{
			Debug.WriteLine("{0} - Succesfully Registered.", "CrossPushNotification");


			string trimmedDeviceToken =token.Description;
			if (!string.IsNullOrWhiteSpace(trimmedDeviceToken))
			{
				trimmedDeviceToken = trimmedDeviceToken.Trim('<');
				trimmedDeviceToken = trimmedDeviceToken.Trim('>');
				trimmedDeviceToken = trimmedDeviceToken.Trim();
				trimmedDeviceToken = trimmedDeviceToken.Replace(" ","");
			}
			Console.WriteLine("{0} - Token: {1}", "CrossPushNotification", trimmedDeviceToken);


			if (CrossPushNotification.IsInitialized)
			{
				CrossPushNotification.PushNotificationListener.OnRegistered(trimmedDeviceToken, DeviceType.iOS);
			}
			else
			{
				throw NewPushNotificationNotInitializedException();
			}


			Realtime.Messaging.Helpers.Settings.Token = trimmedDeviceToken;
		}


		OrtcPushNotificationNotInitializedException NewPushNotificationNotInitializedException()
		{
			string description = "CrossPushNotification Plugin is not initialized. Should initialize before use on FinishedLaunching method of AppDelegate class. Example:  CrossPushNotification.Initialize<CrossPushNotificationListener>()";

			return new OrtcPushNotificationNotInitializedException(description);
		}

		#endregion
	}
}

