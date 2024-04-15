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

using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels.Blocks.Internal;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Base for a standard Detail block type for an entity. This is a block that
    /// will display an entity with the option to edit and save changes.
    /// </summary>
    public abstract class RockEntityDetailBlockType<TEntity, TEntityBag> : RockDetailBlockType
        where TEntity : Rock.Data.Entity<TEntity>, new()
        where TEntityBag : class
    {
        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation. The block's <see cref="RockContext"/>
        /// property will be used to access the database.
        /// </summary>
        /// <returns>The entity to be viewed or edited on the page.</returns>
        protected abstract TEntity GetInitialEntity();

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            var entity = GetInitialEntity();

            if ( entity is IHasAttributes attributedEntity )
            {
                attributedEntity.LoadAttributes( RockContext );
            }

            return GetSecurityGrant( entity ).ToToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        protected virtual Rock.Security.SecurityGrant GetSecurityGrant( TEntity entity )
        {
            var grant = new Rock.Security.SecurityGrant();

            if ( entity is IHasAttributes attributedEntity )
            {
                grant.AddRulesForAttributes( attributedEntity, RequestContext.CurrentPerson );
            }

            return grant;
        }

        /// <summary>
        /// Prepares the detail box by setting all standard framework level values.
        /// This should be called by subclasses after the box has been initialized
        /// with all values required by the subclass instance.
        /// </summary>
        /// <param name="box">The box to be initialized.</param>
        /// <param name="entity">The entity that will be displayed on the page.</param>
        protected void PrepareDetailBox( IDetailBlockBox box, TEntity entity )
        {
            box.EntityTypeName = typeof( TEntity ).Name;
            box.EntityTypeGuid = EntityTypeCache.Get<TEntity>( true, RockContext ).Guid;
            box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<TEntity>();

            if ( entity != null && box.ErrorMessage.IsNullOrWhiteSpace() )
            {
                box.SecurityGrantToken = GetSecurityGrant( entity ).ToToken();
            }
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        protected abstract bool TryGetEntityForEditAction( string idKey, out TEntity entity, out BlockActionResult error );

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        protected abstract bool UpdateEntityFromBox( TEntity entity, ValidPropertiesBox<TEntityBag> box );

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="TEntityBag"/> that represents the entity.</returns>
        protected abstract TEntityBag GetEntityBagForView( TEntity entity );

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="TEntityBag"/> that represents the entity.</returns>
        protected abstract TEntityBag GetEntityBagForEdit( TEntity entity );

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( ValidPropertiesBox<TEntityBag> box )
        {
            if ( !( box.Bag is EntityBagBase entityBag ) )
            {
                return ActionBadRequest( "Attributes are not supported by this block." );
            }

            if ( !TryGetEntityForEditAction( entityBag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !( entity is IHasAttributes attributeEntity ) )
            {
                return ActionBadRequest( "Attributes are not supported by this block." );
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            attributeEntity.LoadAttributes( RockContext );

            var refreshedBox = new ValidPropertiesBox<TEntityBag>
            {
                Bag = GetEntityBagForEdit( entity )
            };

            var refreshedBag = refreshedBox.Bag as EntityBagBase;

            var oldAttributeGuids = entityBag.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
            var newAttributeGuids = refreshedBag.Attributes.Values.Select( a => a.AttributeGuid );

            // If the attributes haven't changed then return a 204 status code.
            if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
            }

            // Replace any values for attributes that haven't changed with
            // the value sent by the client. This ensures any unsaved attribute
            // value changes are not lost.
            foreach ( var kvp in refreshedBag.Attributes )
            {
                if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                {
                    refreshedBag.AttributeValues[kvp.Key] = entityBag.AttributeValues[kvp.Key];
                }
            }

            return ActionOk( refreshedBox );
        }
    }
}
