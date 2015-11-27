using System;

namespace Realtime.Messaging
{
	public interface IPushNotification
	{
		string Token { get; }
		void Register();
	}
}

