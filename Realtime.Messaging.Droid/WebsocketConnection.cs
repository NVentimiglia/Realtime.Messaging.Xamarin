using System;
using System.Threading.Tasks;
using Java.Nio.Charset;
using Realtime.Messaging.Droid;
using RealtimeFramework.Messaging.Exceptions;
using RealtimeFramework.Messaging.Ext;
using Square.OkHttp;
using Square.OkHttp.WS;
using Square.OkIO;

[assembly: Xamarin.Forms.Dependency(typeof(WebsocketConnection))]

namespace Realtime.Messaging.Droid
{
    public class WebsocketConnection : IWebsocketConnection
    {

        static WebsocketConnection()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        #region Events (4)

        public event Action OnOpened;
        public event Action OnClosed;
        public event Action<Exception> OnError;
        public event Action<string> OnMessageReceived;

        #endregion

        #region Attributes

        private OkHttpClient _client = null;
        private WebSocketCall _call = null;
        private WebSocketListener _listener = null;
        private IWebSocket _socket = null;
        
        #endregion

        public void NewConnection(string uri)
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            if (_call != null)
            {
                _call.Dispose();
                _call = null;
            }

            if (_listener != null)
            {
                _listener.Open -= _listener_Open;
                _listener.Close -= _listener_Close;
                _listener.Message -= _listener_Message;
                _listener.Failure -= _listener_Failure;
                _listener.Dispose();
                _listener = null;
            }

            _client = new OkHttpClient();

            // Create request for remote resource.
            Request request = new Request.Builder().Url(uri).Build();

            // Execute the request and retrieve the response.
            _call = WebSocketCall.Create(_client, request);
            _listener = _call.Enqueue();

            _listener.Open += _listener_Open;
            _listener.Close += _listener_Close;
            _listener.Message += _listener_Message;
            _listener.Failure += _listener_Failure;
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
                NewConnection(connectionUrl);
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
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (_socket != null)
                    {
                        _socket.Close(1000, "CLOSE_NORMAL");
                        _socket.Dispose();
                        _socket = null;
                    }
                }
                catch (Exception ex)
                {

                    var ev = OnError;
                    if (ev != null)
                    {
                        Task.Factory.StartNew(() => ev(new OrtcGenericException("Unable to close socket.")));
                    }
                }
            });
        }

        public void Send(string message)
        {
            Task.Factory.StartNew(() =>
            {

                if (_socket == null)
                    return;

                try
                {
                    message = "\"" + message + "\"";
                    var buffer = new OkBuffer();
                    buffer.WriteString(message, Charset.DefaultCharset());
                    _socket.SendMessage(WebSocketPayloadType.Text, buffer);
                    buffer.Close();
                }
                catch (Exception ex)
                {

                    var ev = OnError;
                    if (ev != null)
                    {
                        Task.Factory.StartNew(() => ev(new OrtcGenericException("Unable to write to socket.")));
                    }
                }
            });
        }

        private void _listener_Failure(object sender, FailureEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var ev = OnError;
                if (ev != null)
                {
                    if (e.Exception != null)
                        ev(e.Exception);
                    else
                        ev(new Exception("Unknown WebSocket Error!"));
                }
            });

            Task.Factory.StartNew(() =>
            {
                var ev2 = OnClosed;
                if (ev2 != null)
                {
                    ev2();
                }
            });
        }

        private void _listener_Message(object sender, MessageEventArgs e)
        {
            string payload = e.Payload.ReadString(Charset.DefaultCharset());
            e.Payload.Close();

            Task.Factory.StartNew(() =>
            {
                var ev = OnMessageReceived;
                if (ev != null)
                {
                    ev(payload);
                }
            });
        }

        private void _listener_Close(object sender, CloseEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var ev = OnClosed;
                if (ev != null)
                {
                    ev();
                }
            });
        }

        private void _listener_Open(object sender, OpenEventArgs e)
        {
            _socket = e.WebSocket;

            Task.Factory.StartNew(() =>
            {
                var ev = OnOpened;
                if (ev != null)
                {
                    ev();
                }
            });
        }
    }
}
