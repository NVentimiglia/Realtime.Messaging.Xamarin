using System;

namespace RealtimeFramework.Messaging.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when the connection metadata is to big.
    /// </summary>
    public class OrtcMaxLengthException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcEmptyFieldException"/> class.
        /// </summary>
        public OrtcMaxLengthException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcEmptyFieldException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OrtcMaxLengthException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcEmptyFieldException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public OrtcMaxLengthException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}