using System;
using Android.Gms.Gcm.Iid;
using Android.App;
using Android.Content;

namespace Realtime.Messaging.Droid
{
	[Service(Exported = false), IntentFilter(new[] { "com.google.android.gms.iid.InstanceID" })]
	public class MyInstanceIDListenerService : InstanceIDListenerService
	{
		public override void OnTokenRefresh()
		{
			var intent = new Intent (this, typeof (RegistrationIntentService));
			StartService (intent);
		}
	}
}

