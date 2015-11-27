using System;

namespace Realtime.Messaging
{
	public class OrtcPushNotificationNotInitializedException : Exception
	{
		/// <summary>
		/// Default Contructor
		/// </summary>
		public OrtcPushNotificationNotInitializedException()
		{
		}
		/// <summary>
		/// Constructor with message
		/// </summary>
		public OrtcPushNotificationNotInitializedException(string message): base(message)
		{
		}
	}
}

