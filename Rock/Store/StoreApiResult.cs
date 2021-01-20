namespace Rock.Store
{
    /// <summary>
    /// A generic class the will return the data or any api error message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoreApiResult<T>
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the error response.
        /// </summary>
        /// <value>
        /// The error response.
        /// </value>
        public string ErrorResponse { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError { get => ErrorResponse.IsNotNullOrWhiteSpace(); }
    }
}
