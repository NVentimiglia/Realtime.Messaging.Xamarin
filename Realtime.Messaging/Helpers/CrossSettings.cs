using System;
using Xamarin.Forms;

namespace Realtime.Messaging.Helpers
{
	public class CrossSettings
	{
		static Lazy<ISettings> settings = new Lazy<ISettings>(() => CreateSettings(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

		/// <summary>
		/// Current settings to use
		/// </summary>
		public static ISettings Current
		{
			get
			{
				ISettings ret = settings.Value;
				if (ret == null)
				{
					throw NotImplementedInReferenceAssembly();
				}
				return ret;
			}
		}

		static ISettings CreateSettings()
		{
			#if PORTABLE
			return null;
			#else
			return DependencyService.Get<ISettings>();
			#endif
		}

		internal static Exception NotImplementedInReferenceAssembly()
		{
			return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the Xam.Plugins.Settings NuGet package from your main application project in order to reference the platform-specific implementation.");
		}
	}
}

