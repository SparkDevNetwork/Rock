using System;
using System.Linq;

using Microsoft.Extensions.Configuration;

using Rock.Data;

namespace RockWebCore
{
    /// <summary>
    /// Factory to create service instances for the given entity type.
    /// </summary>
    /// <seealso cref="Rock.Rest.IEntityServiceFactory" />
    internal class EntityServiceFactory : Rock.Rest.IEntityServiceFactory
    {
        /// <inheritdoc/>
        public Service<TEntity> GetEntityService<TEntity>( RockContext rockContext )
            where TEntity : Entity<TEntity>, new()
        {
            var baseServiceType = typeof( Service<> ).MakeGenericType( typeof( TEntity ) );

            var serviceTypes = typeof( RockContext ).Assembly
                .GetTypes().AsEnumerable();

            serviceTypes = serviceTypes
                .Where( a => !a.IsAbstract && a.BaseType == baseServiceType );

            var serviceType = serviceTypes
                .Single();

            return ( Service<TEntity> ) Activator.CreateInstance( serviceType, rockContext );
        }
    }
}
