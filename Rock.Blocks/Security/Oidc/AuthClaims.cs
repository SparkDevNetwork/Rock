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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.AuthClaims;
using Rock.Web.Cache;

namespace Rock.Blocks.Security.Oidc
{
    /// <summary>
    /// Displays a list of auth claims for a given auth scope.
    /// </summary>
    [DisplayName( "OpenID Connect Claims" )]
    [Category( "Security > OIDC" )]
    [Description( "Block for displaying and editing available OpenID Connect claims." )]
    [IconCssClass( "ti ti-lock" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CustomizedGrid]
    [SystemGuid.EntityTypeGuid( "3EBA8801-B43E-421F-B343-87835A0F8519" )]
    // was [SystemGuid.BlockTypeGuid( "C269F723-C77B-4289-822A-6444638A5E88" )]
    [SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OIDC_CLAIMS )]
    public class AuthClaims : RockEntityListBlockType<AuthClaim>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ScopeId = "ScopeId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AuthClaimsOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AuthClaimsOptionsBag GetBoxOptions()
        {
            var options = new AuthClaimsOptionsBag()
            {
                IsBlockVisible = true
            };

            var scopeKey = RequestContext.GetPageParameter( PageParameterKey.ScopeId );
            if ( scopeKey.IsNullOrWhiteSpace() )
            {
                options.ErrorMessage = "No Auth Scope Id was specified.";
            }
            else if ( scopeKey.AsIntegerOrNull() == 0 )
            {
                options.IsBlockVisible = false;
            }
            else
            {
                var scope = new AuthScopeService( new RockContext() ).Get( scopeKey, !PageCache.Layout.Site.DisablePredictableIds );
                if ( scope == null )
                {
                    options.ErrorMessage = "No Auth Scope Id was specified.";
                }
            }

            return options;
        }

        /// <summary>
        /// Determines if the add and delete buttons should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add and delete buttons should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthClaim> GetListQueryable( RockContext rockContext )
        {
            var queryable = new AuthClaimService( rockContext ).Queryable().AsNoTracking();

            var authScopeKey = RequestContext.GetPageParameter( PageParameterKey.ScopeId );
            if ( authScopeKey.IsNullOrWhiteSpace() )
            {
                return queryable.Where( ac => false );
            }

            var authScope = new AuthScopeService( rockContext ).Get( authScopeKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( authScope == null )
            {
                return queryable.Where( ac => false );
            }

            queryable = queryable.Where( ac => ac.ScopeId == authScope.Id );

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthClaim> GetOrderedListQueryable( IQueryable<AuthClaim> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( ac => ac.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AuthClaim> GetGridBuilder()
        {
            return new GridBuilder<AuthClaim>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "publicName", a => a.PublicName )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem );
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was successfully retrieved or created; otherwise <c>false</c>.</returns>
        private bool TryGetEntityForEditAction( string idKey, out AuthClaim entity, out BlockActionResult error )
        {
            var entityService = new AuthClaimService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                var authScopeKey = RequestContext.GetPageParameter( PageParameterKey.ScopeId );
                if ( authScopeKey.IsNullOrWhiteSpace() )
                {
                    entity = null;
                    error = ActionBadRequest( "The auth scope id is required to create a claim." );
                    return false;
                }

                var authScope = new AuthScopeService( RockContext ).Get( authScopeKey, !PageCache.Layout.Site.DisablePredictableIds );
                if ( authScope == null )
                {
                    entity = null;
                    error = ActionBadRequest( $"{AuthScope.FriendlyTypeName} not found." );
                    return false;
                }

                entity = new AuthClaim();
                entity.ScopeId = authScope.Id;
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AuthClaim.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {AuthClaim.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the auth claim data for editing.
        /// </summary>
        /// <param name="key">The identifier of the auth claim to edit. Empty or null for a new claim.</param>
        /// <returns>A bag containing the auth claim properties.</returns>
        [BlockAction]
        public BlockActionResult GetAuthClaimBag( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var authClaim, out var actionError ) )
            {
                return actionError;
            }

            var bag = new AuthClaimBag
            {
                IdKey = authClaim.IdKey,
                Name = authClaim.Name,
                PublicName = authClaim.PublicName,
                Value = authClaim.Value,
                IsActive = authClaim.IsActive,
                IsSystem = authClaim.IsSystem
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Saves the auth claim. Handles both creating new claims and
        /// updating existing ones.
        /// </summary>
        /// <param name="bag">The request bag containing the claim data to save.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( AuthClaimRequestBag bag )
        {
            if ( !TryGetEntityForEditAction( bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // For system claims, only allow updating the PublicName.
            if ( !entity.IsSystem )
            {
                /*
                     2/16/2026 - MSE

                     Added validation to check for an existing AuthClaim with the same Name
                     before saving. The Name must be unique across all AuthScopes. The
                     AuthClaim.Name column has a unique database index (IX_Name), which
                     would otherwise result in a database constraint violation error.

                     Reason: Ensure Name uniqueness and prevent database constraint errors.
                */
                if ( bag.Name.IsNotNullOrWhiteSpace() )
                {
                    var existingClaim = new AuthClaimService( RockContext ).Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( ac => ac.Name == bag.Name && ac.Id != entity.Id );

                    if ( existingClaim != null )
                    {
                        return ActionBadRequest( $"A claim with the name '{bag.Name}' already exists." );
                    }
                }

                entity.Name = bag.Name;
                entity.Value = bag.Value;
                entity.IsActive = bag.IsActive;
            }

            entity.PublicName = bag.PublicName;

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified auth claim. System claims cannot be deleted.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( entity.IsSystem )
            {
                return ActionBadRequest( $"Cannot delete a system {AuthClaim.FriendlyTypeName}." );
            }

            var entityService = new AuthClaimService( RockContext );

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
