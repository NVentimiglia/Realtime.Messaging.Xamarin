using System;
using Android.App;
using Android.Util;
using Android.Gms.Gcm.Iid;
using Android.Gms.Gcm;
using Android.Content;
using Realtime.Messaging.Helpers;

namespace Realtime.Messaging.Droid
{
	
	[Service(Exported = false)]
	public class RegistrationIntentService : IntentService
	{
		const string Tag = "RegistrationIntentService";

		static object locker = new object();

		public RegistrationIntentService() : base("RegistrationIntentService") { }

		protected override void OnHandleIntent (Intent intent)
		{
			try
			{
				Log.Info ("RegistrationIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
					var instanceID = InstanceID.GetInstance (this);
					var token = instanceID.GetToken (
						CrossPushNotification.SenderId, GoogleCloudMessaging.InstanceIdScope, null);

					Log.Info ("RegistrationIntentService", "GCM Registration Token: " + token);
					StoreRegistrationToken (token);
					CrossPushNotification.PushNotificationListener.OnRegistered(token,DeviceType.Android);
				}
			}
			catch (Exception e)
			{
				Log.Debug("RegistrationIntentService", "Failed to get a registration token: " + e.ToString());
				CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - " + e.ToString(), Tag), DeviceType.Android);
				return;
			}
		}

		private void StoreRegistrationToken (string token)
		{
			Settings.RegistrationId = token;
		}
	}
}

