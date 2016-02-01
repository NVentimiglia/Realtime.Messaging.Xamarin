using System;
using System.Threading.Tasks;
using Foundation;
using Realtime.Messaging.Exceptions;
using Realtime.Messaging.Ext;
using Realtime.Messaging.IOS;
using Square.SocketRocket;

[assembly: Xamarin.Forms.Dependency(typeof(WebsocketConnection))]

namespace Realtime.Messaging.IOS
{
    [Preserve]
    public class WebsocketConnection : IWebsocketConnection
    {
        public static void Link()
        {

        }
        static WebsocketConnection()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        #region Events (4)

        public event Action OnOpened = delegate { };
        public event Action OnClosed = delegate { };
        public event Action<Exception> OnError = delegate { };
        public event Action<string> OnMessageReceived = delegate { };

        #endregion

        #region Attributes

        private WebSocket _client = null;

        #endregion

        void DoOpen(string uri)
        {

            DoClose();


            _client = new WebSocket(new NSUrl(uri));

            _client.ReceivedMessage += _client_ReceivedMessage;
            _client.WebSocketClosed += _client_WebSocketClosed;
            _client.WebSocketFailed += _client_WebSocketFailed;
            _client.WebSocketOpened += _client_WebSocketOpened;
        }

        void DoClose()
        {
            if (_client != null)
            {
                _client.ReceivedMessage -= _client_ReceivedMessage;
                _client.WebSocketClosed -= _client_WebSocketClosed;
                _client.WebSocketFailed -= _client_WebSocketFailed;
                _client.WebSocketOpened -= _client_WebSocketOpened;

                if (_client.ReadyState == ReadyState.Open)
                {
                    _client.Close();
                }

                _client.Dispose();
                _client = null;
            }
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
                DoOpen(connectionUrl);
                _client.Open();
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
            try
            {
                DoClose();

                var ev = OnClosed;
                if (ev != null)
                {
                    ev();
                }
            }
            catch (Exception ex)
            {

                var ev = OnError;
                if (ev != null)
                {
                     ev(new OrtcGenericException("Unable to close socket."));
                }
            }
        }

        public void Send(string message)
        {
            if (_client != null && _client.ReadyState == ReadyState.Open)
            {
                try
                {
                    message = String.Format("\"{0}\"", message);
                    _client.Send(new NSString(message));
                }
                catch (Exception ex)
                {

                    var ev = OnError;
                    if (ev != null)
                    {
                        ev(new OrtcGenericException("Unable to write to socket."));
                    }
                }
            }
        }


        // Handlers


        private void _client_WebSocketOpened(object sender, EventArgs e)
        {
            var ev = OnOpened;
            if (ev != null)
            {
                ev();
            }
        }

        private void _client_WebSocketFailed(object sender, WebSocketFailedEventArgs e)
        {
            var ev = OnError;
            if (ev != null)
            {
                if (e.Error != null)
                    ev(new Exception(e.Error.Description));
                else
                    ev(new Exception("Unknown WebSocket Error!"));
            }

            var ev2 = OnClosed;
            if (ev2 != null)
            {
                ev2();
            }
        }

        private void _client_WebSocketClosed(object sender, WebSocketClosedEventArgs e)
        {
            var ev = OnClosed;
            if (ev != null)
            {
                ev();
            }
        }

        private void _client_ReceivedMessage(object sender, WebSocketReceivedMessageEventArgs e)
        {
            var ev = OnMessageReceived;
            if (ev != null)
            {
                var m = e.Message.ToString();
                ev(m);
            }
        }
    }
}
