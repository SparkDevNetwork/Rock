namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// The type of message that is being sent or received.
    /// </summary>
    internal enum PrinterProxyMessageType
    {
        /// <summary>
        /// No message, this is treated as a "no operation" message and will be
        /// ignored if it is ever received.
        /// </summary>
        None,

        /// <summary>
        /// A message to check if the other end of the connection is still there.
        /// The response should be of type <see cref="PrinterProxyResponsePing"/>.
        /// </summary>
        Ping,

        /// <summary>
        /// A special message that contains a response to a previous message. The
        /// <see cref="PrinterProxyMessage.Id"/> value should match the original
        /// message.
        /// </summary>
        Response,

        /// <summary>
        /// A request to a raw chunk of data to a printer.
        /// </summary>
        Print
    }
}
