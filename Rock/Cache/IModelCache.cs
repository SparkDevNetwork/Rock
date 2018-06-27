namespace Rock.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModelCache
    {
        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        void ReloadAttributeValues();

        /// <summary>
        /// Sets the value of an attribute key in memory. Once values have been set, use the <see cref="SaveAttributeValues()" /> method to save all values to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void SetAttributeValue( string key, string value );
    }
}