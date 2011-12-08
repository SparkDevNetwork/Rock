namespace Rock.Data
{
    /// <summary>
    /// Represents a model that supports specific ordering
    /// </summary>
    public interface IOrdered
    {
        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        int Order { get; set; }
    }
}
