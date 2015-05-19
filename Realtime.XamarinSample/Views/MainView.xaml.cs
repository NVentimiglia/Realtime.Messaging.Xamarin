using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RealtimeFramework.Messaging;
using RealtimeFramework.Messaging.Exceptions;
using RealtimeFramework.Messaging.Ext;
using Xamarin.Forms;
using System.Linq;

namespace Realtime.XamarinSample
{
    public class MessageLine
    {
        public string Content { get; set; }
        public Color Color { get; set; }

		public override string ToString ()
		{
			return Content;
		}
    }

	public enum ConnectionState{
		Disconnected,
		Connecting,
		Connected,
	}

    public partial class MainView : ContentPage
    {
        protected OrtcClient client;

		public string AppKey = "";
		public string AuthToken = "";
		public string ClusterUrl = "";
		public string PrivateKey = "";


        public MainView()
        {
            client = new OrtcClient();
            client.ClusterUrl = ClusterUrl;
            client.ConnectionMetadata = "Xamarin-" + new Random().Next();
			client.HeartbeatTime = 15;

            client.OnConnected += client_OnConnected;
            client.OnDisconnected += client_OnDisconnected;
            client.OnException += client_OnException;
            client.OnReconnected += client_OnReconnected;
            client.OnReconnecting += client_OnReconnecting;
            client.OnSubscribed += client_OnSubscribed;
            client.OnUnsubscribed += client_OnUnsubscribed;

            Channel = "myChannel";
			Message = "Hello Realtime";
         

            BindingContext = this;
			InitializeComponent();
        }

        #region handlers
        void client_OnUnsubscribed(object sender, string channel)
        {
			Success(string.Format("Unsubscribed from {0}", channel));
			Channels.Remove (channel);
			AllChannels = string.Join(", ", Channels.ToArray() );
			IsBusy = false;
        }

        void client_OnSubscribed(object sender, string channel)
        {
			Success(string.Format("Subscribed to {0}", channel));
			Channels.Add (channel);
			AllChannels = string.Join(", ", Channels.ToArray() );
			IsBusy = false;
        }

        void client_OnReconnecting(object sender)
        {
			Warning("Reconnecting...");
			State = ConnectionState.Connecting;
			IsBusy = false;
        }

        void client_OnReconnected(object sender)
        {
			Success("Reconnected");
			State = ConnectionState.Connected;
			IsBusy = false;
        }

        void client_OnException(object sender, Exception ex)
        {
            Error(ex.Message);
        }

        void client_OnDisconnected(object sender)
        {
			Warning("Disconnected");
			State = ConnectionState.Disconnected;
			Channels.Clear();
			IsBusy = false;
        }

        void client_OnConnected(object sender)
        {
			Success("Connected");
			State = ConnectionState.Connected;
			IsBusy = false;
        }

        void OnPressence(OrtcPresenceException e, Presence arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
                Success("Got Presence ! "+arg.Subscriptions+" Clients");
                foreach (var a in arg.Metadata)
                {
                    Write("Client : "+a.Key);
                }                
			}
			IsBusy = false;
        }


        void OnEnablePressence(OrtcPresenceException e, string arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
                Success("Presence enabled "+arg);
				HasPresence = true;
			}
			IsBusy = false;
        }
        void OnDisablePressence(OrtcPresenceException e, string arg)
        {
            if (e != null)
            {
                Error(e.Message);
            }
            else
            {
				Success("Presence disabled " + arg);
				HasPresence = false;
			}
			IsBusy = false;
        }
        void OnMessage(object sender, string channel, string content)
        {
            Write(string.Format("{0} : {1}", channel, content));
        }

        #endregion

        #region write methods

        void Error(string message)
        {
            Write(message, Color.Red);
        }
        void Log(string message)
        {
            Write(message, Color.Gray);
        }

        void Warning(string message)
        {
            Write(message, Color.Yellow);
        }

        void Success(string message)
        {
            Write(message, Color.Green);
        }
        
        void Write(string message)
        {
            Write(message, Color.White);
        }

        void Write(string message, Color color)
        {
            Messages.Insert(0, new MessageLine
            {
                Color = color,
					Content = string.Format("{0} : {1}", DateTime.Now.ToString("hh:mm:ss"), message)
            });
        }
        #endregion

        #region props
		private ConnectionState _state;
		public ConnectionState State
		{
			get { return _state; }
			set
			{
				if (_state == value)
					return;
				_state = value;
				OnPropertyChanged();
				OnPropertyChanged("IsConnected");
				OnPropertyChanged("IsConnecting");
				OnPropertyChanged("IsDisconnected");
			}
		}

		public bool IsConnected
		{
			get { return State == ConnectionState.Connected; }
		}
		public bool IsConnecting
		{
			get { return State == ConnectionState.Connecting; }
		}

        public bool IsDisconnected
		{
			get { return State == ConnectionState.Disconnected; }
        }

        private bool _isPrivate;
        public bool IsPrivate
        {
            get { return _isPrivate; }
            set
            {
                if (_isPrivate == value)
                    return;
                _isPrivate = value;
                OnPropertyChanged();
            }
        }

        private bool _hasPresence;
        public bool HasPresence
        {
            get { return _hasPresence; }
            set
            {
                if (_hasPresence == value)
                    return;
                _hasPresence = value;
                OnPropertyChanged();
            }
        }

        private string _channel;
        public string Channel
        {
            get { return _channel; }
            set
            {
                if (_channel == value)
                    return;
                _channel = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value)
                    return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public string ClientMeta
        {
            get { return client.ConnectionMetadata; }
            set
            {
                if (client.ConnectionMetadata == value)
                    return;
                client.ConnectionMetadata = value;
                OnPropertyChanged();
            }
        }

        private string _allChannels;
        public string AllChannels
        {
			get { return _allChannels; }
            set
            {
				if (_allChannels == value)
                    return;
				_allChannels = value;
                OnPropertyChanged();
            }
        }

		private ObservableCollection<MessageLine> _messages  = new ObservableCollection<MessageLine>();
		public ObservableCollection<MessageLine> Messages
		{
			get { return _messages; }
			set
			{
				if (_messages == value)
					return;
				_messages = value;
				OnPropertyChanged();
			}
		}


        public ObservableCollection<string> Channels = new ObservableCollection<string>();
        #endregion

        public void DoConnect(object s, EventArgs e)
        {
            Log("Connecting...");
			if (!client.IsConnected)
				State = ConnectionState.Connecting;
			client.Connect(AppKey, Guid.NewGuid().ToString());
        }

        public void DoDisconnect(object s, EventArgs e)
        {
			Log("Disconnecting...");
            client.Disconnect();
        }

        public void DoPresence(object s, EventArgs e)
        {
			Log("Getting Presence...");
            client.Presence(Channel, OnPressence);
        }

        public void DoEnablePresence(object s, EventArgs e)
        {
			Log("Enabling Presence...");
            client.EnablePresence(PrivateKey, Channel, true, OnEnablePressence);
        }

        public void DoDisablePresence(object s, EventArgs e)
        {
			Log("Disabling Presence...");
            client.DisablePresence(PrivateKey, Channel, OnDisablePressence);
        }

        public void DoSubscribe(object s, EventArgs e)
        {
			Log("Subscribing...");
            client.Subscribe(Channel, true, OnMessage);
        }

        public void DoUnsubscribe(object s, EventArgs e)
        {
			Log("Unsubscribing...");
            client.Unsubscribe(Channel);
        }

        public void DoSend(object s, EventArgs e)
        {
            client.Send(Channel, Message);
        }

        public void DoSendTime(object s, EventArgs e)
        {
            client.Send(Channel, DateTime.Now.ToString());
		}

		public void OnAdd(object s, EventArgs e)
		{
			client.Send(Channel, DateTime.Now.ToString("hh:mm:ss.ff"));
		}
    }
}

