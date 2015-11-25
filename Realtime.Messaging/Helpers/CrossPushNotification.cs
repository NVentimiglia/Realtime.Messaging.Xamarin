using System;
using System.Diagnostics;
using RealtimeFramework.Messaging;

namespace Realtime.Messaging
{
	public class CrossPushNotification
	{
		public static IPushNotificationListener PushNotificationListener { get; private set; }
		public static string SenderId { get; set; }
		public static void Initialize<T>() where T : IPushNotificationListener, new()
		{
			if (PushNotificationListener == null)
			{
				PushNotificationListener = (IPushNotificationListener)Activator.CreateInstance(typeof(T));
				Debug.WriteLine("PushNotification plugin initialized.");
			}
			else
			{
				Debug.WriteLine("PushNotification plugin already initialized.");
			}
		}
	}
}

