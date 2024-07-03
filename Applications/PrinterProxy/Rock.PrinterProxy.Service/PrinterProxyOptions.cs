namespace Rock.PrinterProxy.Service;

/// <summary>
/// The options that will be used to initialize <see cref="RockPrinterProxy"/>.
/// </summary>
internal class PrinterProxyOptions
{
    /// <summary>
    /// The base URL of the server to connect to, such as
    /// <c>https://rock.rocksolidchurchdemo.com</c>.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The name of the proxy. If not specified then the computer name will
    /// be used instead.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the Rock Device to connect as. Multiple proxies can
    /// connect as a single device and will be used for load balancing and/or
    /// failover.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The priority of this proxy connection compared to others. The proxy with
    /// the lowest priority value will be used for printing.
    /// </summary>
    public int Priority { get; set; }
}
