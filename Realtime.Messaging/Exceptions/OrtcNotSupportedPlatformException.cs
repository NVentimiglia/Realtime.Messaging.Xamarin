using System;

namespace Realtime.Messaging.Exceptions
{
	public class OrtcNotSupportedPlatformException : Exception
	{
		public OrtcNotSupportedPlatformException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrtcNotSupportedPlatformException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public OrtcNotSupportedPlatformException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrtcNotSupportedPlatformException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public OrtcNotSupportedPlatformException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}

