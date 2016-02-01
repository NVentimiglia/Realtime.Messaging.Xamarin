using System;

namespace Realtime.Messaging.Ext
{
    public interface IWebsocketConnection
    {
        event Action OnOpened;
        event Action OnClosed;
        event Action<Exception> OnError;
        event Action<string> OnMessageReceived;

        void Connect(string url);

        void Close();

        void Send(string message);

    }
}
