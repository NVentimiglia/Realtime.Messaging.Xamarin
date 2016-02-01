using System;

namespace Realtime.Messaging.Exceptions
{
	public class OrtcApnsException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		public OrtcApnsException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public OrtcApnsException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrctGcmException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public OrtcApnsException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}

