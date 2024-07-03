using System;

namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// A ping message is used to check if the other end of the connection
    /// is still there and also determine what the current latency is.
    /// </summary>
    internal class PrinterProxyMessagePing : PrinterProxyMessage
    {
        /// <summary>
        /// The point in time that this message was originally sent at.
        /// </summary>
        public DateTimeOffset SentAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessagePing"/> class.
        /// </summary>
        public PrinterProxyMessagePing()
        {
            Type = PrinterProxyMessageType.Ping;
        }
    }
}
