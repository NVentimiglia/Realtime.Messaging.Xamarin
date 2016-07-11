using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Realtime.Messaging;
using Realtime.Messaging.Helpers;
using Xamarin.Forms.Platform.Android;

namespace Messaging.Sample.Droid
{
    [Activity(Label = "Messaging.Sample", Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light",  
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	[Android.Runtime.Preserve(AllMembers = true)]
    public class MainActivity : FormsApplicationActivity
    {
        public MainActivity()
        {
            
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

