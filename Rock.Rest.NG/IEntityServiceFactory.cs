namespace Rock.Rest
{
    /// <summary>
    /// Factory to create new services for the given entity type.
    /// </summary>
    public interface IEntityServiceFactory
    {
        /// <summary>
        /// Gets the entity service for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="rockContext">The rock context that provides database access.</param>
        /// <returns>An implementation of <see cref="Rock.Data.Service{T}"/> that will handle access to the entity.</returns>
        Rock.Data.Service<TEntity> GetEntityService<TEntity>( Rock.Data.RockContext rockContext )
            where TEntity : Rock.Data.Entity<TEntity>, new();
    }
}
