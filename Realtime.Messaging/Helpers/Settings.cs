// Helpers/Settings.cs
using System.Collections.Generic;

namespace Realtime.Messaging.Helpers
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class Settings
  {
    private static ISettings AppSettings
    {
      get
      {
        return CrossSettings.Current;
      }
    }

    #region Setting Constants

    private const string RegistrationIdKey = "registrationId";
	private const string TokenKey = "token";
    private static readonly string SettingsDefault = string.Empty;
    #endregion

    public static string RegistrationId
    {
      get
      {
		return AppSettings.GetValueOrDefault(RegistrationIdKey, SettingsDefault);
      }
      set
      {
		AppSettings.AddOrUpdateValue(RegistrationIdKey, value);
      }
    }

	public static string Token
	{
		get
		{
			return AppSettings.GetValueOrDefault(TokenKey, SettingsDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue(TokenKey, value);
		}
	}
  }
}