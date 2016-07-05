using Android.App;
using Realtime.Messaging.Droid;

namespace Messaging.Sample.Droid
{
    [Application][Android.Runtime.Preserve(AllMembers = true)]
    public class MainApplication : Application
    {
		public MainApplication()
		{
			
		}

        public override void OnCreate()
        {
            base.OnCreate();
            
          //  new PushNotificationAppStarter(this);
        }
    }
}