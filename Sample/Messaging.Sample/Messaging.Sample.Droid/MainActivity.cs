using System;

using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Messaging.Sample.Droid
{
    [Activity(Label = "Messaging.Sample", Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light",  
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Websockets.Droid.WebsocketConnection.Link();
            
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }
    }
}

