using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using RealtimeFramework.Messaging;
using Android.Gms.Common;
using Android.OS;

namespace Realtime.Messaging.Droid
{
	
	[Application]
	public class PushNotificationAppStarter : Application
	{
		public static Context AppContext;
		public static bool AppIsInForeground;
		public static bool OrtcClientInitialized;

		public PushNotificationAppStarter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public override void OnCreate()
		{
			RegisterActivityLifecycleCallbacks(new LifecycleCallbacks());
			base.OnCreate();

			AppContext = this.ApplicationContext;

			Xamarin.Forms.MessagingCenter.Subscribe<OrtcClient, string>(AppContext, "GoogleProjectNumber", (page, senderId) => {
				CrossPushNotification.SenderId = senderId;
				if (IsPlayServicesAvailable()){
					var intent = new Intent (this, typeof (RegistrationIntentService));
					StartService (intent);
				}
				else{
					CrossPushNotification.PushNotificationListener.OnError("Google Play Services is not available", DeviceType.Android);
				}
			});

			Xamarin.Forms.MessagingCenter.Subscribe<OrtcClient>(AppContext, "SetOnPushNotification", (page) => {
				OrtcClientInitialized = true;
				Xamarin.Forms.MessagingCenter.Send(this,"CheckPushNotification");
			});
				
			CrossPushNotification.Initialize<CrossPushNotificationListener>();
		}
			
		private bool IsPlayServicesAvailable ()
		{
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
			if (resultCode != ConnectionResult.Success)
			{
				return false;
			}
			else
			{
				return true;
			}
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
