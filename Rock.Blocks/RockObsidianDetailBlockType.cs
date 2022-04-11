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
    /// Base for a standard Obsidian Detail block type. This is a block that
    /// will display an entity with the option to edit and save changes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />
    public abstract class RockObsidianDetailBlockType : RockObsidianBlockType
    {
        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Campus"/> to be viewed or edited on the page.</returns>
        protected TEntity GetInitialEntity<TEntity, TService>( RockContext rockContext, string entityIdKey, string entityGuidKey )
            where TService : Service<TEntity>
            where TEntity : Rock.Data.Entity<TEntity>, new()
        {
            // TODO: Add Site option for "Allow Integer Identifiers"
            int? id = null;
            var guid = RequestContext.GetPageParameter( entityGuidKey ).AsGuidOrNull();

            if ( true /* TODO: PageCache.SiteCache.IsIntegerIdentifierAllowed */ )
            {
                id = RequestContext.GetPageParameter( entityIdKey ).AsIntegerOrNull();
            }

            var entityService = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );

            // If a zero identifier is specified then create a new entity.
            if ( ( id.HasValue && id.Value == 0 ) || ( !id.HasValue && !guid.HasValue ) )
            {
                return new TEntity
                {
                    Id = 0,
                    Guid = Guid.Empty
                };
            }

            // Otherwise look for an existing one in the database.
            if ( guid.HasValue )
            {
                return entityService.GetNoTracking( guid.Value );
            }
            else if ( id.HasValue )
            {
                return entityService.GetNoTracking( id.Value );
            }
            else
            {
                return null;
            }
        }

    }
}
