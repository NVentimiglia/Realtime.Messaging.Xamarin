using System;
using Android.App;
using Android.Runtime;
using Realtime.Messaging.Droid;

namespace Messaging.Sample.Droid
{
    [Application][Android.Runtime.Preserve(AllMembers = true)]
    public class MainApplication : Application
    {
		public MainApplication()
		{
			
		}

        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            
        }

        public override void OnCreate()
        {
            base.OnCreate();
            
          //  new PushNotificationAppStarter(this);
        }
    }
}