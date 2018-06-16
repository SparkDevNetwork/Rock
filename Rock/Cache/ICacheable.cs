namespace Rock.Cache
{
    /// <summary>
    /// Entity class that has one or more EntityCache types associated with it
    /// </summary>
    public interface ICacheable
    {
        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        void UpdateCache( System.Data.Entity.EntityState entityState, Rock.Data.DbContext dbContext );

        /// <summary>
        /// Updates the cached attribute value of the cache object associated with this entity
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        void UpdateCachedAttributeValue( string attributeKey, string value );
    }
}
