using System;

namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// The response object that will be returned by a Ping message.
    /// </summary>
    internal class PrinterProxyResponsePing
    {
        /// <summary>
        /// The value from <see cref="PrinterProxyMessagePing.SentAt"/>
        /// </summary>
        public DateTimeOffset RequestedAt { get; set; }

        /// <summary>
        /// The date and time that the response was sent.
        /// </summary>
        public DateTimeOffset RespondedAt { get; set; }
    }
}
