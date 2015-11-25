// Helpers/Settings.cs
using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;
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
  }
}