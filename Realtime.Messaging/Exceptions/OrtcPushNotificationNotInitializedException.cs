using System;

namespace Realtime.Messaging.Exceptions
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

