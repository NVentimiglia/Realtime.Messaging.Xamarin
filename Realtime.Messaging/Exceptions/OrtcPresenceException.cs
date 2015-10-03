using System;
using System.Runtime.Serialization;

namespace RealtimeFramework.Messaging.Exceptions {
    /// <summary>
    /// Represents the exception thrown when an ORTC client face problems with ORTC presence operations
    /// </summary>
    public class OrtcPresenceException : System.Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcPresenceException"/> class.
        /// </summary>
        public OrtcPresenceException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcPresenceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OrtcPresenceException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="OrtcPresenceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public OrtcPresenceException(string message, System.Exception inner) : base(message, inner) { }
        /*
        protected OrtcPresenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }*/
    }
}