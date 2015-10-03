using System;

namespace RealtimeFramework.Messaging.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when a client tries to do a operation that requires the client to be connected.
    /// </summary>
    public class OrtcNotConnectedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcNotConnectedException"/> class.
        /// </summary>
        public OrtcNotConnectedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcNotConnectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OrtcNotConnectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcNotConnectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public OrtcNotConnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}