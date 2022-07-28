﻿namespace Microsoft.Azure.Devices.Client
{
    /// <summary>
    /// The protocol over which a transport (i.e., MQTT, AMQP) communicates
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// Communicate over TCP using the default port of the transport.
        /// </summary>
        /// <remarks>
        /// For MQTT, this this is 1883.
        /// For AMQP, this is 5671.
        /// </remarks>
        Tcp,

        /// <summary>
        /// Communicate over web socket using port 443.
        /// </summary>
        WebSocket,
    }
}