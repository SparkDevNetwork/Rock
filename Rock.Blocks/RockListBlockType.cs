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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Obsidian.UI;
using Rock.ViewModels.Core.Grid;

namespace Rock.Blocks
{
    /// <summary>
    /// Base for a standard List block type. This is a block that will
    /// display a list of items and usually links to a detail page.
    /// </summary>
    /// <strong>This is an internal API</strong> that supports the Rock
    /// infrastructure and not subject to the same compatibility standards
    /// as public APIs. It may be changed or removed without notice in any
    /// release and should therefore not be directly used in any plug-ins.
    /// </remarks>
    [RockInternal( "1.16" )]
    public abstract class RockListBlockType<T> : RockBlockType
        where T : class
    {
        #region Methods

        /// <summary>
        /// Checks if the current person is allowed to create the specified
        /// entity set.
        /// </summary>
        /// <param name="entitySetBag">The entity set bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected virtual bool IsAllowedToCreateEntitySet( GridEntitySetBag entitySetBag )
        {
            return true;
        }

        /// <summary>
        /// Checks if the current person is allowed to create the specified
        /// communication.
        /// </summary>
        /// <param name="communicationBag">The communication bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected virtual bool IsAllowedToCreateCommunication( GridCommunicationBag communicationBag )
        {
            return true;
        }

        /// <summary>
        /// Gets the queryable that will be used to load data from the database.
        /// </summary>
        /// <param name="rockContext">The rock context that will be used to access the database.</param>
        /// <returns>A <see cref="IQueryable{T}"/> for the type.</returns>
        protected abstract IQueryable<T> GetListQueryable( RockContext rockContext );

        /// <summary>
        /// Gets the queryable that will have proper default ordering applied.
        /// </summary>
        /// <param name="queryable">The queryable to have ordering applied to it.</param>
        /// <param name="rockContext">The rock context that will be used to access the database.</param>
        /// <returns>A <see cref="IQueryable{T}"/> that will return results in the desired order.</returns>
        protected virtual IQueryable<T> GetOrderedListQueryable( IQueryable<T> queryable, RockContext rockContext )
        {
            return queryable;
        }

        /// <summary>
        /// Gets the list of items from the queryable.
        /// </summary>
        /// <remarks>
        /// This provides a last chance for subclasses to modify the list of
        /// items before they are processed by the builder. For example, to
        /// filter items by security.
        /// </remarks>
        /// <param name="queryable">The queryable representing the items.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of the items that will be displayed on the grid.</returns>
        protected virtual List<T> GetListItems( IQueryable<T> queryable, RockContext rockContext )
        {
            return queryable.ToList();
        }

        /// <summary>
        /// Gets the grid builder that will provide all the details and values
        /// of the grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        protected abstract GridBuilder<T> GetGridBuilder();

        /// <summary>
        /// Gets the grid data bag that will be sent to the client.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="GridDataBag"/>.</returns>
        protected virtual GridDataBag GetGridDataBag( RockContext rockContext )
        {
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( rockContext );
            qry = GetOrderedListQueryable( qry, rockContext );

            // Get the items from the queryable.
            var items = GetListItems( qry, rockContext );

            return GetGridBuilder().Build( items );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the bag that describes the grid data to be displayed in the
        /// block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public virtual BlockActionResult GetGridData()
        {
            return ActionOk( GetGridDataBag( RockContext ) );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the entity set.</returns>
        [BlockAction]
        public virtual BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            if ( !IsAllowedToCreateEntitySet( entitySet ) )
            {
                return ActionForbidden( "You are not allowed to create entity sets." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Creates a communication for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the communication.</returns>
        [BlockAction]
        public virtual BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            if ( communication == null )
            {
                return ActionBadRequest( "No communication data was provided." );
            }

            if ( !IsAllowedToCreateCommunication( communication ) )
            {
                return ActionForbidden( "You are not allowed to create communications." );
            }

            var rockCommunication = GridHelper.CreateCommunication( communication, RequestContext );

            if ( rockCommunication == null )
            {
                return ActionBadRequest( "Grid has no recipients." );
            }

            return ActionOk( rockCommunication.Id.ToString() );
        }

        #endregion
    }
}
