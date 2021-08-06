using Microsoft.EntityFrameworkCore;

namespace Rock.Rest
{
    /// <summary>
    /// Contains all the dependencies required by <see cref="EntityController{TEntity}"/>.
    /// </summary>
    /// <remarks>
    /// This allows us to add new dependencies without breaking inheritance
    /// compatibility. Otherwise subclasses would have to be updated if we add
    /// new constructor parameters.
    /// </remarks>
    public sealed class EntityControllerDependencies
    {
        /// <summary>
        /// Gets the entity service factory.
        /// </summary>
        /// <value>
        /// The entity service factory.
        /// </value>
        public IEntityServiceFactory EntityServiceFactory { get; }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>
        /// The database context.
        /// </value>
        public Rock.Data.RockContext RockContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityControllerDependencies"/> class.
        /// </summary>
        /// <param name="entityServiceFactory">The entity service factory.</param>
        /// <param name="rockContext">The database context.</param>
        public EntityControllerDependencies( IEntityServiceFactory entityServiceFactory, Rock.Data.RockContext rockContext )
        {
            EntityServiceFactory = entityServiceFactory;
            RockContext = rockContext;
        }
    }
}
