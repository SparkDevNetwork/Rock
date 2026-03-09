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
        /// The tool executed successfully and optionally one or more content
        /// items. This should also be used in cases where no content is expected,
        /// such as a delete operation.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The tool executed successfully but returned no items. This should
        /// be used when data is expected but none is available. For example,
        /// a tool that lists items should return <see cref="NoData"/> instead
        /// of <see cref="Success"/> if no items were found as an otherwise
        /// empty response might confuse the agent.
        /// </summary>
        NoData = 1,

        /// <summary>
        /// The tool failed. See the error message on the result for details.
        /// </summary>
        Error = 2
    }
}
