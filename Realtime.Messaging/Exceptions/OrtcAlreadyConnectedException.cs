using System;

namespace RealtimeFramework.Messaging.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when the client is already connected.
    /// </summary>
    public class OrtcAlreadyConnectedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadyConnectedException"/> class.
        /// </summary>
        public OrtcAlreadyConnectedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadyConnectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OrtcAlreadyConnectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadyConnectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public OrtcAlreadyConnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}