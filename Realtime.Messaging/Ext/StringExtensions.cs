using System.Text.RegularExpressions;

namespace RealtimeFramework.Messaging.Ext
{
    /// <summary>
    /// Class used for String methods extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if the input is valid.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static bool OrtcIsValidInput(this string s)
        {
            return s != null && Regex.Match(s, @"^[\w-:\/\.]*$").Success;
        }

        /// <summary>
        /// Checks if the URL is valid.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static bool OrtcIsValidUrl(this string s)
        {
            return (s != null && (Regex.Match(s, @"^\s*(http|https)://(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(/|/([\w#!:.?+=&%@!\-/]))?\s*$")).Success);
        }
    }
}
