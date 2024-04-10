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

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels.Blocks.Internal;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Base for a standard Detail block type for an entity. This is a block that
    /// will display an entity with the option to edit and save changes.
    /// </summary>
    public abstract class RockEntityDetailBlockType<TEntity> : RockDetailBlockType
        where TEntity : Rock.Data.Entity<TEntity>, new()
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
    }
}
