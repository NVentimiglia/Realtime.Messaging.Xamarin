using System;
using System.Diagnostics;
using Realtime.Messaging.Exceptions;
using Xamarin.Forms;

namespace Realtime.Messaging.Helpers
{
	public class CrossPushNotification
	{
		static Lazy<IPushNotification> Implementation = new Lazy<IPushNotification>(() => CreatePushNotification(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
		public static bool IsInitialized { get { return (PushNotificationListener != null);  } }
		public static IPushNotificationListener PushNotificationListener { get; private set; }
		public static string SenderId { get; set; }
		public static void Initialize<T>() where T : IPushNotificationListener, new()
		{
			if (PushNotificationListener == null)
			{
				PushNotificationListener = (IPushNotificationListener)Activator.CreateInstance(typeof(T));
				Debug.WriteLine("PushNotification plugin initialized.");
			}
			else
			{
				Debug.WriteLine("PushNotification plugin already initialized.");
			}
		}

		/// <summary>
		/// Current settings to use
		/// </summary>
		public static IPushNotification Current
		{
			get
			{
				//Should always initialize plugin before use
				if (!CrossPushNotification.IsInitialized)
				{
					throw NewPushNotificationNotInitializedException();
				}
				var ret = Implementation.Value;
				if (ret == null)
				{
					throw NotImplementedInReferenceAssembly();
				}
				return ret;
			}
		}

		static IPushNotification CreatePushNotification()
		{
			#if PORTABLE
			return null;
			#else
			return DependencyService.Get<IPushNotification>();
			#endif
		}

		internal static Exception NotImplementedInReferenceAssembly()
		{
			return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
		}

		internal static OrtcPushNotificationNotInitializedException NewPushNotificationNotInitializedException()
		{
			string description = "CrossPushNotification Plugin is not initialized. Should initialize before use with CrossPushNotification Initialize method. Example:  CrossPushNotification.Initialize<CrossPushNotificationListener>()";

			return new OrtcPushNotificationNotInitializedException(description);
		}
	}
}

