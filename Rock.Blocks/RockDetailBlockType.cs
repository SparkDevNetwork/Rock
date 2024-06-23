// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;

using Rock.Data;

namespace Rock.Blocks
{
    /// <summary>
    /// Base for a standard Detail block type. This is a block that
    /// will display an entity with the option to edit and save changes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    public abstract class RockDetailBlockType : RockBlockType
    {
        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The entity to be viewed or edited on the page.</returns>
        protected TEntity GetInitialEntity<TEntity, TService>( RockContext rockContext, string entityIdKey )
            where TService : Service<TEntity>
            where TEntity : Rock.Data.Entity<TEntity>, new()
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );
            var id = !PageCache.Layout.Site.DisablePredictableIds ? entityId.AsIntegerOrNull() : null;
            var guid = entityId.AsGuidOrNull();

            // If a zero identifier is specified then create a new entity.
            if ( ( id.HasValue && id.Value == 0 ) || ( guid.HasValue && guid.Value == Guid.Empty ) || ( !id.HasValue && !guid.HasValue && entityId.IsNullOrWhiteSpace() ) )
            {
                return new TEntity
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            var entityService = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );

            return entityService.GetNoTracking( entityId, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation. The block's <see cref="RockContext"/>
        /// property will be used to access the database.
        /// </summary>
        /// <returns>The entity to be viewed or edited on the page.</returns>
        protected TEntity GetInitialEntity<TEntity>( string entityIdKey )
            where TEntity : Rock.Data.Entity<TEntity>, new()
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );
            var id = !PageCache.Layout.Site.DisablePredictableIds ? entityId.AsIntegerOrNull() : null;
            var guid = entityId.AsGuidOrNull();

            // If a zero identifier is specified then create a new entity.
            if ( ( id.HasValue && id.Value == 0 ) || ( guid.HasValue && guid.Value == Guid.Empty ) || ( !id.HasValue && !guid.HasValue && entityId.IsNullOrWhiteSpace() ) )
            {
                return new TEntity
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            var entityService = ( Service<TEntity> ) Reflection.GetServiceForEntityType( typeof( TEntity ), RockContext );

            return entityService.GetNoTracking( entityId, !PageCache.Layout.Site.DisablePredictableIds );
        }
    }
}
