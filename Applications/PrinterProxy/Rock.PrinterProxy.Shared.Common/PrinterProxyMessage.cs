using System;
using System.Collections.Generic;

namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// A generic message that will be sent to or received from the proxy.
    /// </summary>
    internal class PrinterProxyMessage
    {
        /// <summary>
        /// This is used when deserializing to quickly know what CLR type the
        /// message should be deserialized into.
        /// </summary>
        internal static IReadOnlyDictionary<PrinterProxyMessageType, Type> MessageLookup = new Dictionary<PrinterProxyMessageType, Type>
        {
            [PrinterProxyMessageType.Ping] = typeof( PrinterProxyMessagePing ),
            [PrinterProxyMessageType.Response] = typeof( PrinterProxyMessageResponse ),
            [PrinterProxyMessageType.Print] = typeof( PrinterProxyMessagePrint ),
        };

        /// <summary>
        /// The type of message that will be sent or was received. This is
        /// primarily used during serialization so we can include the message
        /// type code in the data stream.
        /// </summary>
        public PrinterProxyMessageType Type { get; set; }

        /// <summary>
        /// The unique identifier of this message.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessage"/> class.
        /// </summary>
        public PrinterProxyMessage()
        {
        }
    }
}
