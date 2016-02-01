using System;
using Realtime.Messaging.Ext;

namespace Realtime.Messaging
{
    /// <summary>
    /// Static Factory for getting WebSocket Connection instances
    /// </summary>
    /// <remarks>
    /// Must be Init by platform code
    /// </remarks>
    internal static class ConnectionFactory
    {
        static Func<IRealtimeConnection> factoryMethod;

        /// <summary>
        /// Call from platform code. e.g. : Websockets.Droid.Platform.Init();
        /// </summary>
        /// <param name="factory"></param>
        public static void Init(Func<IRealtimeConnection> factory)
        {
            factoryMethod = factory;
        }

        /// <summary>
        /// Returns a new websocket instance
        /// </summary>
        /// <returns></returns>
        public static IRealtimeConnection Create()
        {
            if (factoryMethod == null)
            {
                throw  new Exception("Websocket factory is not initialized !");
            }

            return factoryMethod();
        }
    }
}