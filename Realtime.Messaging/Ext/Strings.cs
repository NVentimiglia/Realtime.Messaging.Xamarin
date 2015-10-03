using System;
using System.Text;

namespace RealtimeFramework.Messaging.Ext
{
    /// <summary>
    /// Class used for operations with strings.
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// Randoms the number.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns></returns>
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        /// <summary>
        /// Randoms the string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;

            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates an id.
        /// </summary>
        /// <returns></returns>
        public static string GenerateId(int size)
        {
            string g = Guid.NewGuid().ToString().Replace("-", "");

            return g.Substring(0, size);
        }
    }
}
