namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// A request to send some data to the specified printer. The data to be
    /// printed is contained in the extra data after the message.
    /// </summary>
    internal class PrinterProxyMessagePrint : PrinterProxyMessage
    {
        /// <summary>
        /// The address of the printer that the data should be sent to. This may
        /// be in <c>host:port</c> notation.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessagePrint"/> class.
        /// </summary>
        public PrinterProxyMessagePrint()
        {
            Type = PrinterProxyMessageType.Print;
        }
    }
}
