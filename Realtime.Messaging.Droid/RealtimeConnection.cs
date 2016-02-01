using System;
using System.Diagnostics;
using Android.Runtime;
using Realtime.Messaging.Droid;
using Realtime.Messaging.Exceptions;
using Realtime.Messaging.Ext;
using Websockets;
using Websockets.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(RealtimeConnection))]

namespace Realtime.Messaging.Droid
{
    [Preserve]
    public class RealtimeConnection : IWebsocketConnection
    {

        static RealtimeConnection()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
           WebsocketConnection.Link();
        }

        #region Events (4)

        public event Action OnOpened = delegate { };
        public event Action OnClosed = delegate { };
        public event Action<Exception> OnError = delegate { };
        public event Action<string> OnMessageReceived = delegate { };

        #endregion

        #region Attributes

        private IWebSocketConnection _socket = null;
        #endregion


        public RealtimeConnection()
        {
            _socket = new WebsocketConnection();
            _socket.OnClosed += _socket_OnClosed;
            _socket.OnOpened += _socket_OnOpened;
            _socket.OnError += _socket_OnError;
            _socket.OnLog += _socket_OnLog;
            _socket.OnMessage += _socket_OnMessage;
        }


        public void Connect(string url)
        {

            //System.Diagnostics.Debug.WriteLine("Connection.Connect");
            Uri uri = null;
            var connectionId = Strings.RandomString(8);
            var serverId = Strings.RandomNumber(1, 1000);

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                throw new OrtcEmptyFieldException(String.Format("Invalid URL: {0}", url));
            }

            var prefix = uri != null && "https".Equals(uri.Scheme) ? "wss" : "ws";
            var connectionUrl = String.Format("{0}://{1}:{2}/broadcast/{3}/{4}/websocket", prefix, uri.DnsSafeHost, uri.Port, serverId, connectionId);

            try
            {
                _socket.Open(connectionUrl);
            }
            catch (Exception ex)
            {
                var ev = OnError;
                if (ev != null)
                {
                    ev(new OrtcNotConnectedException("Websocket has encountered an error.", ex));
                }
            }
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Send(string message)
        {
            message = "\"" + message + "\"";
            _socket.Send(message);
        }



        private void _socket_OnMessage(string obj)
        {
            OnMessageReceived(obj);
        }

        private void _socket_OnLog(string obj)
        {
            Trace.WriteLine(obj);
        }

        private void _socket_OnError(string obj)
        {
            OnError(new OrtcGenericException(obj));
        }

        private void _socket_OnOpened()
        {
            OnOpened();
        }

        private void _socket_OnClosed()
        {
            OnClosed();
        }
    }
}
