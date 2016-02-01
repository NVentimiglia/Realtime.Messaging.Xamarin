using System;

namespace Realtime.Messaging.Exceptions
{
	public class OrtcGcmException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		public OrtcGcmException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public OrtcGcmException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public OrtcGcmException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}

