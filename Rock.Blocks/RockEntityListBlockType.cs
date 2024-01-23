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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Base for a standard List block type. This is a block that will
    /// display a list of items and usually links to a detail page.
    /// </summary>
    /// <remarks>
    /// <strong>This is an internal API</strong> that supports the Rock
    /// infrastructure and not subject to the same compatibility standards
    /// as public APIs. It may be changed or removed without notice in any
    /// release and should therefore not be directly used in any plug-ins.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="IEntity"/> to be displayed in the grid.</typeparam>
    [RockInternal( "1.16" )]
    public abstract class RockEntityListBlockType<T> : RockListBlockType<T>
        where T : class, IEntity
    {
        #region Fields

        /// <summary>
        /// Contains the cached attributes to be included on the grid.
        /// </summary>
        private List<AttributeCache> _cachedGridAttributes;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether attribute logic should be disabled.
        /// If you are building a grid for an entity that supports attributes but
        /// you do not want the show-on-grid attributes to be shown then override
        /// this and return <c>true</c>.
        /// </summary>
        /// <value><c>true</c> if grid attributes should be disabled; otherwise, <c>false</c>.</value>
        protected virtual bool DisableAttributes => false;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override IQueryable<T> GetListQueryable( RockContext rockContext )
        {
            return ( IQueryable<T> ) Rock.Reflection.GetQueryableForEntityType( typeof( T ), rockContext );
        }

        /// <inheritdoc/>
        protected override IQueryable<T> GetOrderedListQueryable( IQueryable<T> queryable, RockContext rockContext )
        {
            if ( typeof( IOrdered ).IsAssignableFrom( typeof( T ) ) )
            {
                return queryable.OrderBy( nameof( IOrdered.Order ) )
                    .ThenBy( e => e.Id );
            }
            else
            {
                return queryable.OrderBy( e => e.Id );
            }
        }

        /// <summary>
        /// Gets the grid attributes that should be included on the Grid.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        protected List<AttributeCache> GetGridAttributes()
        {
            if ( _cachedGridAttributes == null )
            {
                _cachedGridAttributes = BuildGridAttributes();
            }

            return _cachedGridAttributes;
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        protected virtual List<AttributeCache> BuildGridAttributes()
        {
            if ( typeof( IHasAttributes ).IsAssignableFrom( typeof( T ) ) )
            {
                var entityTypeId = EntityTypeCache.Get<T>( false )?.Id;

                if ( entityTypeId.HasValue )
                {
                    return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
                }
            }

            return new List<AttributeCache>();
        }

        /// <inheritdoc/>
        protected override GridDataBag GetGridDataBag( RockContext rockContext )
        {
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( rockContext ).AsNoTracking();
            qry = GetOrderedListQueryable( qry, rockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, rockContext );

            if ( !DisableAttributes && typeof( IHasAttributes ).IsAssignableFrom( typeof( T ) ) )
            {
                var gridAttributes = GetGridAttributes();
                var gridAttributeIds = gridAttributes.Select( a => a.Id ).ToList();

                Helper.LoadFilteredAttributes( items.Cast<IHasAttributes>(), rockContext, a => gridAttributeIds.Contains( a.Id ) );
            }

            return GetGridBuilder().Build( items );
        }

        #endregion
    }
}
