using System;

namespace RealtimeFramework.Messaging.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when a channel is already subscribed by the client.
    /// </summary>
    public class OrtcAlreadySubscribedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadySubscribedException"/> class.
        /// </summary>
        public OrtcAlreadySubscribedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadySubscribedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OrtcAlreadySubscribedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcAlreadySubscribedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public OrtcAlreadySubscribedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}