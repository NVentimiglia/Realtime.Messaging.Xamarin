using System.Collections.Generic;

namespace Realtime.Messaging.Helpers
{
	public interface IPushNotificationListener
	{
		
		/// <summary>
		/// On Message Received
		/// </summary>
		/// <param name="Parameters"></param>
		/// <param name="deviceType"></param>
		void OnMessage(IDictionary<string, object> Parameters,DeviceType deviceType);
		/// <summary>
		/// On Registered
		/// </summary>
		/// <param name="Token"></param>
		/// <param name="deviceType"></param>
		void OnRegistered(string Token, DeviceType deviceType);
		/// <summary>
		/// OnError
		/// </summary>
		void OnError(string message,DeviceType deviceType);
	}
}

