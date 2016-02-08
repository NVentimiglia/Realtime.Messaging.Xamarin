using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Realtime.Messaging.Helpers;

namespace Realtime.Messaging.Droid
{
	
	public class PushNotificationAppStarter
    {
        public static Application Current;
        public static Context AppContext;
		public static bool AppIsInForeground;
		public static bool OrtcClientInitialized;

        public PushNotificationAppStarter(Application current)
	    {
	        Current = current;
            AppContext = Application.Context;
            Current.RegisterActivityLifecycleCallbacks(new LifecycleCallbacks());
            
            Xamarin.Forms.MessagingCenter.Subscribe<OrtcClient, string>(AppContext, "GoogleProjectNumber", (page, senderId) => {
                CrossPushNotification.SenderId = senderId;
                if (IsPlayServicesAvailable())
                {
                    var intent = new Intent(Current, typeof(RegistrationIntentService));
                    Current.StartService(intent);
                }
                else {
                    CrossPushNotification.PushNotificationListener.OnError("Google Play Services is not available", DeviceType.Android);
                }
            });

            Xamarin.Forms.MessagingCenter.Subscribe<OrtcClient>(AppContext, "SetOnPushNotification", (page) => {
                OrtcClientInitialized = true;
                Xamarin.Forms.MessagingCenter.Send(this, "CheckPushNotification");
            });
        }

        public bool IsPlayServicesAvailable ()
		{
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (Application.Context);
			if (resultCode != ConnectionResult.Success)
			{
				return false;
			}
            return true;
		}
	}

	public class LifecycleCallbacks : Java.Lang.Object, Android.App.Application.IActivityLifecycleCallbacks
	{
		private Dictionary<string, object> CachedPushNotification;

		public LifecycleCallbacks(){
			Xamarin.Forms.MessagingCenter.Subscribe<PushNotificationAppStarter> (this, "CheckPushNotification", (page) => {
				if(CachedPushNotification != null){
					CrossPushNotification.PushNotificationListener.OnMessage (CachedPushNotification, DeviceType.Android);
				}
			});
		}

		public void OnActivityCreated (Activity activity, Bundle savedInstanceState)
		{
			if(activity.Intent.HasExtra("pushBundle")){

				var data = activity.Intent.GetBundleExtra ("pushBundle");
				var parameters = new Dictionary<string, object>();

				foreach (var key in data.KeySet())
				{
					parameters.Add (key, data.Get (key));
				}

				if (PushNotificationAppStarter.OrtcClientInitialized) {
					CrossPushNotification.PushNotificationListener.OnMessage (parameters, DeviceType.Android);
				} else {
					CachedPushNotification = parameters;
				}
			}
		}

		public void OnActivityDestroyed (Activity activity)
		{
			//Console.WriteLine ("OnActivityDestroyed on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		public void OnActivityPaused (Activity activity)
		{
			//Console.WriteLine ("OnActivityPaused on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		public void OnActivityResumed (Activity activity)
		{
			//Console.WriteLine ("OnActivityResumed on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
			if(!(activity.GetType().ToString() == "Android.App.Activity"))
				PushNotificationAppStarter.AppIsInForeground = true;
		}

		public void OnActivitySaveInstanceState (Activity activity, Bundle outState)
		{
			//Console.WriteLine ("OnActivitySaveInstanceState on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		public void OnActivityStarted (Activity activity)
		{
			//Console.WriteLine ("OnActivityStarted on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
		}

		public void OnActivityStopped (Activity activity)
		{
			//Console.WriteLine ("OnActivityStopped on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
			if(!(activity.GetType().ToString() == "Android.App.Activity"))
				PushNotificationAppStarter.AppIsInForeground = false;
		}
	}
}
