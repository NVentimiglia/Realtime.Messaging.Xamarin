using System;
using Realtime.Messaging.WinPhone;
using RealtimeFramework.Messaging.Exceptions;
using RealtimeFramework.Messaging.Ext;
using SuperSocket.ClientEngine;
using WebSocket4Net;

[assembly: Xamarin.Forms.Dependency(typeof(WebsocketConnection))]

namespace Realtime.Messaging.WinPhone
{
    public class WebsocketConnection : IWebsocketConnection
    {
        public static void Link()
        {

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

            _client = new WebSocket(uri);

            _client.MessageReceived += _client_ReceivedMessage;
            _client.Closed += _client_WebSocketClosed;
            _client.Error += _client_WebSocketFailed;
            _client.Opened += _client_WebSocketOpened;
            
            _client.Open();
        }

        void DoClose()
        {
            if (_client != null)
            {
                _client.MessageReceived -= _client_ReceivedMessage;
                _client.Closed -= _client_WebSocketClosed;
                _client.Error -= _client_WebSocketFailed;
                _client.Opened -= _client_WebSocketOpened;

                if (_client.State == WebSocketState.Open)
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
            }
            catch (Exception ex)
            {
                var ev = OnError;
                if (ev != null)
                {
                    ev(new OrtcNotConnectedException("Websocket has encountered an error.", ex));
                }

                DoClose();
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
            if (_client != null && _client.State == WebSocketState.Open)
            {
                try
                {
                    message = String.Format("\"{0}\"", message);
                    _client.Send(message);
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

        private void _client_WebSocketFailed(object sender, ErrorEventArgs e)
        {
            var ev = OnError;
            if (ev != null)
            {
                if (e.Exception != null)
                    ev(new Exception(e.Exception.Message, e.Exception));
                else
                    ev(new Exception("Unknown WebSocket Error!"));
            }

            DoClose();
        }

        private void _client_WebSocketClosed(object sender, EventArgs e)
        {
            var ev = OnClosed;
            if (ev != null)
            {
                ev();
            }
        }

        private void _client_ReceivedMessage(object s, MessageReceivedEventArgs e)
        {
            var ev = OnMessageReceived;
            if (ev != null)
            {
                var m = e.Message;
                ev(m);
            }
        }
    }
}
