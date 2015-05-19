using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Realtime.XamarinSample
{
	public partial class OrtcView : ContentPage
	{
		enum ConsoleState { Connection, Authentication, Presence }

		OrtcExample ortcExample;

		Color blackBrush = Color.Black;
		Color grayBrush = Color.Gray;


		public OrtcView()
		{
			this.InitializeComponent();

			SetControls(ConsoleState.Connection);

			ortcExample = new OrtcExample();

			BindingContext = ortcExample;

			BtConnect.Clicked += ortcExample.DoConnect;
			BtDisconnect.Clicked += ortcExample.DoDisconnect;
			BtSend.Clicked += ortcExample.DoSendMessage;
			BtSubscribe.Clicked += ortcExample.DoSubscribe;
			BtUnsubscribe.Clicked += ortcExample.DoUnsubscribe;
			BtGetPresence.Clicked += ortcExample.DoGetPresence;
			BtEnablePresence.Clicked += ortcExample.DoEnablePresence;
			BtDisablePresence.Clicked += ortcExample.DoDisablePresence;
			BtAuthenticate.Clicked += ortcExample.DoSaveAuthentication;

			BtConnection.Clicked += BtConnection_Click;
			BtAuthentication.Clicked += BtAuthentication_Click;
			BtPresence.Clicked += BtPresence_Click;
		}

		void BtPresence_Click(object sender, EventArgs e) {
			SetControls(ConsoleState.Presence);
		}

		void BtAuthentication_Click(object sender, EventArgs e) {
			SetControls(ConsoleState.Authentication);
		}

		void BtConnection_Click(object sender, EventArgs e) {
			SetControls(ConsoleState.Connection);
		}


		private void SetControls(ConsoleState consoleState) {
			BtConnection.TextColor = consoleState == ConsoleState.Connection ? blackBrush : grayBrush;
			BtAuthentication.TextColor = consoleState == ConsoleState.Authentication ? blackBrush : grayBrush;
			BtPresence.TextColor = consoleState == ConsoleState.Presence ? blackBrush : grayBrush;

			//TbPrivateKey.Enabled = consoleState != ConsoleState.Connection;
			//TbMessage.IsChecked = consoleState == ConsoleState.Connection;
			BtConnect.IsEnabled = consoleState == ConsoleState.Connection;
			BtDisconnect.IsEnabled = consoleState == ConsoleState.Connection;
			BtSend.IsEnabled = consoleState == ConsoleState.Connection;
			BtSubscribe.IsEnabled = consoleState == ConsoleState.Connection;
			BtUnsubscribe.IsEnabled = consoleState == ConsoleState.Connection;
			BtGetPresence.IsEnabled = consoleState == ConsoleState.Connection;

			//CbMetadata.IsEnabled = consoleState == ConsoleState.Presence;
			BtEnablePresence.IsEnabled = consoleState == ConsoleState.Presence;
			BtDisablePresence.IsEnabled = consoleState == ConsoleState.Presence;

			//CbIsPrivate.IsEnabled = consoleState == ConsoleState.Authentication;
		//	TbTTL.IsEnabled = consoleState == ConsoleState.Authentication;
			//CbPermissions.IsEnabled = consoleState == ConsoleState.Authentication;
			BtAuthenticate.IsEnabled = consoleState == ConsoleState.Authentication;
		}

	}
}
