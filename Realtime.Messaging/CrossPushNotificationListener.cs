using System;
using System.Diagnostics;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using Realtime.Messaging.Helpers;

namespace Realtime.Messaging
{
	public class CrossPushNotificationListener : IPushNotificationListener
	{
		//Here you will receive all push notification messages
		//Messages arrives as a dictionary, the device type is also sent in order to check specific keys correctly depending on the platform.
		void IPushNotificationListener.OnMessage(IDictionary<string, object> Parameters, DeviceType deviceType)
		{
			CrossPushNotificationMessage pushNotificationData = new CrossPushNotificationMessage ();

			string channel = Parameters ["C"].ToString();
			pushNotificationData.channel = channel;

			string message = Parameters ["M"].ToString();
			var parsedMsg = parseOrtcMultipartMessage (message);
			pushNotificationData.message = parsedMsg;

			if (deviceType == DeviceType.Android) {
				object payload;
				Parameters.TryGetValue ("P", out payload);

				if (payload != null) {
					Dictionary<string, object> payloadDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>> (payload.ToString ());
					pushNotificationData.payload = payloadDictionary;
				}
			} else if (deviceType == DeviceType.iOS) {

				Dictionary<string, object> payloadDictionary = new Dictionary<string, object> ();

				foreach (var key in Parameters.Keys)
				{
					if (!(String.Equals (key, "A") || String.Equals(key,"C") || String.Equals(key,"M") || String.Equals(key,"aps") || String.Equals(key,"alert"))) {
						payloadDictionary.Add (key, Parameters [key]);
					}
				}
					
				pushNotificationData.payload = payloadDictionary;
			}

			MessagingCenter.Send (this, "DelegatePushNotification", pushNotificationData);
		}
		//Gets the registration token after push registration
		void IPushNotificationListener.OnRegistered(string Token, DeviceType deviceType)
		{
			Debug.WriteLine(string.Format("Push Notification - Device Registered - Token : {0}", Token));
			if (deviceType == DeviceType.Android) {
				MessagingCenter.Send (this, "RegistrationId", Token);
			}
		}

		//Fires when error
		void IPushNotificationListener.OnError(string message, DeviceType deviceType)
		{
			Debug.WriteLine(string.Format("Push notification error - {0}",message));
		}

		private string parseOrtcMultipartMessage(string ortcMessage){
			var messageParts = ortcMessage.Split ('-');
	
			String parsedMessage = "";

			try{
				parsedMessage = messageParts[1].Substring(messageParts[1].IndexOf("_") + 1);
			} catch (Exception){
				// probably a custom push message, use the received string with no parsing
				parsedMessage = ortcMessage;
			}
				
			return parsedMessage;
		}

	}
}

