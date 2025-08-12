namespace Rock.Enums.AI.Agent
{
    /// <summary>
    /// Indicates the overall outcome of a lookup operation.
    /// </summary>
    /// <remarks>
    /// Use <see cref="FunctionStatus.Success"/> when items were found,
    /// <see cref="FunctionStatus.NoData"/> when the operation succeeded but returned no items,
    /// and <see cref="FunctionStatus.Error"/> when the operation failed.
    /// </remarks>
    public enum FunctionStatus
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
