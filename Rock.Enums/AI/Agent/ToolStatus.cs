namespace Rock.Enums.AI.Agent
{
    /// <summary>
    /// Indicates the overall outcome of a tool call.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    public enum ToolStatus
    {
        /// <summary>
        /// The lookup executed successfully and returned one or more items.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The lookup executed successfully but returned no items.
        /// </summary>
        NoData = 1,

        /// <summary>
        /// The lookup failed. See the error message on the result for details.
        /// </summary>
        Error = 2
    }
}
