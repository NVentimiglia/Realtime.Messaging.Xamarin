using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Realtime.Messaging;
using Realtime.Messaging.Droid;
using Realtime.Messaging.Helpers;

namespace Messaging.Sample.Droid
{
    [Activity(Label = "Messaging.Sample", Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light",  
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }

    [Application]
    public class MainApplication : Application
    {
        public override void OnCreate()
        {
            base.OnCreate();
            
            new PushNotificationAppStarter(this);
        }
    }
}

