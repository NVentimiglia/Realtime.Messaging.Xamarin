# Realtime.Messaging.Xamarin

Realtime messaging SDK for Xamarin. The Messaging Service is a highly-scalable pub/sub message broker. Using your favorite programming language you'll be able to broadcast messages to millions of users, reliably and securely. It's all in the cloud so you don't need to manage servers. 

## Platforms

Supports Android, iOS, and Windows Phone 8 Silverlight.

## Implementation

This repository includes both the SDK and a sample application using Xamarin Forms iOS, Android, and Windows Phone 8 Silverlight. The SDK includes a platform agnosted Realtime.Messaging project as well as a platform specific 'plugin'. The Android version uses OKHTTP.ws Binding Libraries. iOS uses SocketRocket Binding Libraries. The Windows Phone version uses Websockets Portable and rda.Sockets.

https://github.com/mattleibow/square-bindings

https://github.com/NVentimiglia/WebSocket.Portable


## Installation

#### NUGET
The SDK is avaliable via Nuget. Please include the NugetPackage to all projects. This includes the common PCL as well as your Xamarin platform project.

https://www.nuget.org/packages/Realtime.Xamarin/

#### IOS

For IOS 9, you will need to include a key to your **PList.info**. This may be done in notepad or any text editor.

> App Transport Security has blocked a cleartext HTTP (http://) resource load since it is insecure. Temporary exceptions can be configured via your app's Info.plist file." (See also Apple's corresponding tech note.). 
One way to workaround: Add an NSAppTransportSecurity key to the Info.plist. Under that key, add a dictionary that contains a single NSAppTransportSecurity key set to true: 


`````
<key>NSAppTransportSecurity</key>
<dict>
   <key>NSAllowsArbitraryLoads</key> <true/>
</dict>
`````

#### Android

I had an issue where OKHttp was referenced twice in my android project (From modernhttpclient and OKHttp.ws). This causes an error when building as it tried to include the java dependencies twice. The solution was to manually remove the reference (not the package, just the reference) from the Android project.


#### Dependency Service
I had issues with the dependency service and the linker. Simply put, the [Perserve] annotation *sometimes* works. If it fails, you will get a "Dependency Service Failed" exception. To fix this, we include a static Link() method within each platform implementation that may be called. This will guarantee that the linker will not strip the solution.

````
// Inside AppDelegate.cs
Realtime.Messaging.IOS.WebsocketConnection.Link();

````
## Push Notifications

**Note:** To receive Push Notifications when the app is closed you have to run the application on **Release** mode.

To include push notifications on your project you have to subscribe the channel with notifications **SubscribeWithNotifications(Channel, true, OnMessage)** and set the method **setOnPushNotification(client_OnPushNotification)** to your ortc client on **MainView.cs**:

<pre>
		...
		
		protected OrtcClient client;

        ...
        
          public void DoSubscribeNotifications(object s, EventArgs e)
		{
			Log("Subscribing with notifications...");
			<b>client.SubscribeWithNotifications(Channel, true, OnMessage);</b>
		}
		
		...

        public MainView()
        {
			client = new OrtcClient();
            client.ClusterUrl = ClusterUrlSSL;
            client.ConnectionMetadata = "Xamarin-" + new Random().Next(1000);
            client.HeartbeatTime = 2;
			//client.GoogleProjectNumber = GoogleProjectNumber;

            client.OnConnected += client_OnConnected;
            client.OnDisconnected += client_OnDisconnected;
            client.OnException += client_OnException;
            client.OnReconnected += client_OnReconnected;
            client.OnReconnecting += client_OnReconnecting;
            client.OnSubscribed += client_OnSubscribed;
            client.OnUnsubscribed += client_OnUnsubscribed;
            			 
			 ...
			 
			 <b>client.SetOnPushNotification(client_OnPushNotification);</b>

           ...
        }
        
        ...
        
       <b> void client_OnPushNotification (object sender, string channel, string message, IDictionary<string,object> payload)
		{
			if (payload != null) {
				var payloadStr = "";
				foreach (var key in payload.Keys)
				{
					payloadStr += key + ":" + payload [key] + ",";
				}

				Write (string.Format ("Push Notification - channel: {0} ; message: {1}; payload: {2}", channel, message, payloadStr));
			} else {
				Write (string.Format ("Push Notification - channel: {0} ; message: {1}:", channel, message));
			}
		}</b>
		
		

</pre>

**Note:** On Android to use Push Notifications you have set your Google Project Number on your ortc client:

````
		client.GoogleProjectNumber = "YOUR_GOOGLE_PROJECT_NUMBER";
````

Finnally you have to do some platform specific configuration:

#### IOS - Push Notification Configuration

On your iOS project add the following code to AppDelegate.cs:

<pre>
...
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Realtime.Messaging.IOS.WebsocketConnection.Link();

		    <b>Realtime.Messaging.CrossPushNotification.Initialize&lt;Realtime.Messaging.CrossPushNotificationListener> ();</b>

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());



            return base.FinishedLaunching(app, options);
        }
		
		<b>const string TAG = "PushNotification-APN";
		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{

			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnErrorReceived(error);
			}


		}
		
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnRegisteredSuccess(deviceToken);
			}

		}

		public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
		{
			application.RegisterForRemoteNotifications();
		}


        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived(userInfo);
			}
        }
        

		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			if (Realtime.Messaging.CrossPushNotification.Current is Realtime.Messaging.IOS.IPushNotificationHandler) 
			{
				((Realtime.Messaging.IOS.IPushNotificationHandler)Realtime.Messaging.CrossPushNotification.Current).OnMessageReceived(userInfo);
			}
		}
		</b>
...
</pre>

#### Android - Push Notification Configuration

On your droid project add the following code to /Properties/AndroidManifest.xml:

```

<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android" android:installLocation="auto" 
	package="YOUR_PACKAGE" ...>
	
	...
	
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.WAKE_LOCK" />
    <uses-permission android:name="com.google.android.c2dm.permission.RECEIVE" />

    <permission android:name="YOUR_PACKAGE.permission.C2D_MESSAGE" android:protectionLevel="signature" />
    <uses-permission android:name="YOUR_PACKAGE.permission.C2D_MESSAGE" />
    
    ...
    
	<application ...>
		 ...
		 
        <receiver android:name="com.google.android.gms.gcm.GcmReceiver"
            android:exported="true"
            android:permission="com.google.android.c2dm.permission.SEND">
            <intent-filter>
                <action android:name="com.google.android.c2dm.intent.RECEIVE" />
                <category android:name="YOUR_PACKAGE" />
            </intent-filter>
        </receiver>
        ...
        
    </application>
</manifest>

```

## Sample

Sample of the http://realtime.co messaging framework using Xamarin Forms. Source code included under /Sample/

![Xamarin.Droid Client](xamarin.gif)

#### Usage

- Add your application keys to MainView.cs
- Run the application

#### Questions

Post onto the Github issue system or contact me via my [blog](http://nicholasventimiglia.com)
