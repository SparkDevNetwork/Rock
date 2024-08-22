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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AuthClientDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular auth client.
    /// </summary>
    [DisplayName( "OpenID Connect Client Detail" )]
    [Category( "Security > OIDC" )]
    [Description( "Displays the details of the given OpenID Connect Client." )]
    [IconCssClass( "fa fa-question" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "d7b56608-70a9-42d2-9542-3301ddbec48b" )]
    [Rock.SystemGuid.BlockTypeGuid( "8246ef8b-27e9-449e-9cab-1c267b31dbc2" )]
    public class AuthClientDetail : RockEntityDetailBlockType<AuthClient, AuthClientBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AuthClientId = "AuthClientId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        private const string CLIENT_SECRET_PLACE_HOLDER = "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<AuthClientBag, AuthClientDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AuthClientDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AuthClientDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AuthClient for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="authClient">The AuthClient to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AuthClient is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAuthClient( AuthClient authClient, out string errorMessage )
        {
            errorMessage = null;

            if ( !authClient.IsValid )
            {
                errorMessage = authClient.ValidationResults.Select( x => x.ErrorMessage ).JoinStrings( "," );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AuthClientBag, AuthClientDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AuthClient.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AuthClient.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AuthClient.FriendlyTypeName );
                }
            }

            box.ValidProperties = box.Entity?.GetType().GetProperties().Select( p => p.Name ).ToList();

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AuthClientBag"/> that represents the entity.</returns>
        private AuthClientBag GetCommonEntityBag( AuthClient entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var activeClaims = GetActiveClaims();
            var allowedClaims = entity.AllowedClaims?.FromJsonOrNull<List<string>>() ?? new List<string>();
            activeClaims.ForEach( claim => claim.IsSelected = allowedClaims.Contains( claim.ClaimName ) );

            return new AuthClientBag
            {
                IdKey = entity.IdKey,
                ClientId = entity.ClientId,
                ClientSecret = entity.Id == 0 ? string.Empty : CLIENT_SECRET_PLACE_HOLDER,
                IsActive = entity.Id == 0 || entity.IsActive,
                Name = entity.Name,
                PostLogoutRedirectUri = entity.PostLogoutRedirectUri,
                RedirectUri = entity.RedirectUri,
                ScopeApprovalExpiration = entity.ScopeApprovalExpiration,
                ScopeClaims = ToScopeClaims( activeClaims ),
            };
        }

        /// <summary>
        /// Converts the claims to a dictionary of scope and claims.
        /// </summary>
        /// <param name="claims">The active claims.</param>
        /// <returns></returns>
        private Dictionary<string, List<AuthClientScopeBag>> ToScopeClaims( List<AuthClientScopeBag> claims )
        {
            return claims.GroupBy( c => $"{c.PublicName} ({c.Name})" )
                .ToDictionary(
                cl => cl.Key,
                cl => cl.ToList() );
        }

        /// <inheritdoc/>
        protected override AuthClientBag GetEntityBagForView( AuthClient entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override AuthClientBag GetEntityBagForEdit( AuthClient entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AuthClient entity, ValidPropertiesBox<AuthClientBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ClientId ),
                () => entity.ClientId = box.Bag.ClientId );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.PostLogoutRedirectUri ),
                () => entity.PostLogoutRedirectUri = box.Bag.PostLogoutRedirectUri );

            box.IfValidProperty( nameof( box.Bag.RedirectUri ),
                () => entity.RedirectUri = box.Bag.RedirectUri );

            box.IfValidProperty( nameof( box.Bag.ScopeApprovalExpiration ),
                () => entity.ScopeApprovalExpiration = box.Bag.ScopeApprovalExpiration );

            box.IfValidProperty( nameof( box.Bag.ScopeClaims ),
                () => entity.AllowedClaims = GetSelectedClaims( box.Bag.ScopeClaims ) );

            box.IfValidProperty( nameof( box.Bag.ClientSecret ),
                () => SetClientSecret( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AuthClient GetInitialEntity()
        {
            return GetInitialEntity<AuthClient, AuthClientService>( RockContext, PageParameterKey.AuthClientId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AuthClient entity, out BlockActionResult error )
        {
            var entityService = new AuthClientService( RockContext );
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
                entity = new AuthClient();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AuthClient.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AuthClient.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the active claims.
        /// </summary>
        /// <returns></returns>
        private  List<AuthClientScopeBag> GetActiveClaims()
        {
            var authClaimService = new AuthClaimService( RockContext );

            var activeClaims = authClaimService
                .Queryable()
                .AsNoTracking()
                .Where( ac => ac.IsActive )
                .Where( ac => ac.Scope.IsActive )
                .Select( ac => new AuthClientScopeBag()
                {
                    Guid = ac.Guid,
                    ClaimName = ac.Name,
                    PublicClaimName = ac.PublicName,
                    Name = ac.Scope.Name,
                    PublicName = ac.Scope.PublicName
                } )
                .OrderBy( ac => ac.PublicName )
                .ThenBy( ac => ac.ClaimName )
                .ToList();

            return activeClaims;
        }

        /// <summary>
        /// Gets the selected claims.
        /// </summary>
        /// <param name="scopeClaims"></param>
        /// <returns></returns>
        private string GetSelectedClaims( Dictionary<string, List<AuthClientScopeBag>> scopeClaims )
        {
            var scopes = scopeClaims.SelectMany( sc => sc.Value ).ToList();
            var selectedClaims = scopes.Where( s => s.IsSelected ).Select( sc => sc.ClaimName ).ToList();
            return selectedClaims.ToJson();
        }

        /// <summary>
        /// Encrypts and updates the client secret is valid.
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="entity"></param>
        private void SetClientSecret( AuthClientBag bag, AuthClient entity )
        {
            if ( bag.ClientSecret != CLIENT_SECRET_PLACE_HOLDER )
            {
                var entityTypeName = EntityTypeCache.Get<Rock.Security.Authentication.Database>().Name;
                var databaseAuth = AuthenticationContainer.GetComponent( entityTypeName ) as Rock.Security.Authentication.Database;
                var encryptedClientSecret = databaseAuth.EncryptString( bag.ClientSecret );
                entity.ClientSecretHash = encryptedClientSecret;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<AuthClientBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<AuthClientBag> box )
        {
            var entityService = new AuthClientService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateAuthClient( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new AuthClientService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Generates a Client Secret
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GenerateClientSecret()
        {
            var secret = Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) =>
            {
                var entityTypeName = EntityTypeCache.Get<Rock.Security.Authentication.Database>().Name;
                var databaseAuth = AuthenticationContainer.GetComponent( entityTypeName ) as Rock.Security.Authentication.Database;
                var encryptedClientSecret = databaseAuth.EncryptString( key );

                return new AuthClientService( rockContext ).Queryable().Any( a => a.ClientSecretHash == encryptedClientSecret );
            } );

            return ActionOk( secret );
        }

        #endregion
    }
}
