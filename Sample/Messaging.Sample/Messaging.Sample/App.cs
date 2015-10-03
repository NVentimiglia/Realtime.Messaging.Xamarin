using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Messaging.Sample
{
    public class App : Application
    {
        public App()
		{
			MainPage = new MainView();
		}

		protected override void OnSleep()
		{
			//((MainView)MainPage).ForceDisconnect ();
			
		}
    }
}
