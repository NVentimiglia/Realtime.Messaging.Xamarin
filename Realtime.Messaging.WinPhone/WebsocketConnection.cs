//using System;
//using System.Threading.Tasks;
//using Windows.Foundation;
//using Windows.Networking.Sockets;
//using Windows.Storage.Streams;
//using Realtime.Messaging.WinPhone;
//using RealtimeFramework.Messaging.Exceptions;
//using RealtimeFramework.Messaging.Ext;

//[assembly: Xamarin.Forms.Dependency(typeof(WebsocketConnection))]

//namespace Realtime.Messaging.WinPhone
//{
//    public class WebsocketConnection : IWebsocketConnection
//    {
//        private MessageWebSocket _websocket;
//        private DataWriter messageWriter;

//        public static void Link()
//        {

//        }

//        #region Events (4)

//        public event Action OnOpened = delegate { };
//        public event Action OnClosed = delegate { };
//        public event Action<Exception> OnError = delegate { };
//        public event Action<string> OnMessageReceived = delegate { };

//        #endregion

//        #region Methods - Public (3)

//        private Windows.Storage.Streams.DataWriter writer;

//        public void Connect(string url)
//        {

//            Uri uri = null;

//            var connectionId = Strings.RandomString(8);
//            var serverId = Strings.RandomNumber(1, 1000);

//            try
//            {
//                uri = new Uri(url);
//            }
//            catch (Exception)
//            {
//                OnError(new OrtcEmptyFieldException(string.Format("Invalid URL: {0}", url)));
//                return;
//            }

//            var prefix = uri != null && "https".Equals(uri.Scheme) ? "wss" : "ws";

//            var connectionUrl =
//                new Uri(string.Format("{0}://{1}:{2}/broadcast/{3}/{4}/websocket", prefix, uri.DnsSafeHost, uri.Port,
//                    serverId, connectionId));

//            if (_websocket != null)
//            {
//                _websocket.MessageReceived -= _websocket_MessageReceived;
//                _websocket.Closed -= _websocket_Closed;
//                _websocket.Dispose();
//            }

//            _websocket = new MessageWebSocket();
//            _websocket.Control.MessageType = SocketMessageType.Utf8;
//            _websocket.MessageReceived += _websocket_MessageReceived;
//            _websocket.Closed += _websocket_Closed;


//            _websocket.ConnectAsync(connectionUrl).Completed = (IAsyncAction source, AsyncStatus status) =>
//           {
//               System.Diagnostics.Debug.WriteLine(":: connecting status " + status);

//               if (status == AsyncStatus.Completed)
//               {
//                   messageWriter = new DataWriter(_websocket.OutputStream);
//                   Task.Factory.StartNew(() => OnOpened());
//               }
//               else if (status == AsyncStatus.Error)
//               {
//                   Task.Factory.StartNew(() => OnError(new OrtcNotConnectedException("Websocket has encountered an error.")));
//               }
//               else if (status == AsyncStatus.Started)
//               {
//               }
//           };

//        }

//        private void _websocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
//        {
//            Task.Factory.StartNew(() => OnClosed());
//        }

//        private void _websocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
//        {
//            try
//            {
//                using (var reader = args.GetDataReader())
//                {
//                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
//                    var text = reader.ReadString(reader.UnconsumedBufferLength);
//                    System.Diagnostics.Debug.WriteLine(":: recived " + text);
//                    Task.Factory.StartNew(() => OnMessageReceived(text));
//                }
//            }
//            catch
//            {
//                OnError(new OrtcGenericException("Failed to read message."));
//            }
//        }

//        public void Close()
//        {
//            if (_websocket != null)
//            {
//                _websocket.Close(1000, "Normal closure");
//            }
//        }

//        public async void Send(string message)
//        {
//            if (_websocket != null && messageWriter != null)
//            {
//                try
//                {
//                    message = "\"" + message + "\"";
//                    messageWriter.WriteString(message);
//                    await messageWriter.StoreAsync();
//                    System.Diagnostics.Debug.WriteLine(":: send: " + message);
//                }
//                catch
//                {
//                    OnError(new OrtcGenericException("Failed to send message."));
//                }
//            }
//        }

//        #endregion
//    }
//}