namespace Realtime.Messaging.Helpers
{
	public interface IPushNotification
	{
		string Token { get; }
		void Register();
	}
}

