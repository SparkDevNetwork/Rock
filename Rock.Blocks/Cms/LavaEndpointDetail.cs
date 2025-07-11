﻿// <copyright>
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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Cms;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LavaEndpointDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular lava endpoint.
    /// </summary>

    [DisplayName( "Lava Endpoint Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular lava endpoint." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "45F8578C-C2D6-478D-B20E-D2FA274AB96F" )]
    [Rock.SystemGuid.BlockTypeGuid( "5466EA16-DAC2-490B-9161-92BCA6CDFC1A" )]
    public class LavaEndpointDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LavaApplicationId = "LavaApplicationId";
            public const string LavaEndpointId = "LavaEndpointId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<LavaEndpoint>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private LavaEndpointDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new LavaEndpointDetailOptionsBag();
            options.HttpMethodOptions = typeof( LavaEndpointHttpMethod ).ToEnumListItemBag();
            options.SecurityModeOptions = typeof( LavaEndpointSecurityMode ).ToEnumListItemBag();
            return options;
        }

        /// <summary>
        /// Validates the MediaAccount for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="lavaEndpoint">The LavaEndpoint to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LavaApplication is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLavaEndpoint( LavaEndpoint lavaEndpoint, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                if ( loadAttributes )
                {
                    entity.LoadAttributes( rockContext );
                }

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LavaEndpoint.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( box.IsEditable )
                    {
                        box.Entity = GetEntityBagForEdit( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LavaApplication.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {LavaApplication.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LavaEndpointBag"/> that represents the entity.</returns>
        private LavaEndpointBag GetCommonEntityBag( LavaEndpoint entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new LavaEndpointBag
            {
                IdKey = entity.IdKey,
                IsActive = entity.IsActive,
                Name = entity.Name,
                HttpMethod = entity.HttpMethod,
                CodeTemplate = entity.CodeTemplate,
                Description = entity.Description,
                Slug = entity.Slug,
                EnabledLavaCommands = entity.EnabledLavaCommands.SplitDelimitedValues().Select( x => new ListItemBag { Value = x, Text = x } ).ToList(),
                RateLimitRequestPerPeriod = entity.RateLimitRequestPerPeriod,
                RateLimitPeriodDurationSeconds = entity.RateLimitPeriodDurationSeconds,
                SecurityMode = entity.SecurityMode,
                CacheControlHeaderSettings = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>()?.ToCacheabilityBag(),
                EnableCrossSiteForgeryProtection = entity.GetAdditionalSettings<LavaEndpointAdditionalSettings>()?.EnableCrossSiteForgeryProtection ?? true
            };

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="LavaEndpointBag"/> that represents the entity.</returns>
        private LavaEndpointBag GetEntityBagForView( LavaEndpoint entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, true, IsAttributeIncluded );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="LavaApplicationBag"/> that represents the entity.</returns>
        private LavaEndpointBag GetEntityBagForEdit( LavaEndpoint entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, true, IsAttributeIncluded );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( LavaEndpoint entity, DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
               () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Slug ),
                () => entity.Slug = box.Entity.Slug );

            box.IfValidProperty( nameof( box.Entity.HttpMethod ),
               () => entity.HttpMethod = box.Entity.HttpMethod ?? LavaEndpointHttpMethod.Get );

            box.IfValidProperty( nameof( box.Entity.CodeTemplate ),
               () => entity.CodeTemplate = box.Entity.CodeTemplate );

            
            box.IfValidProperty( nameof( box.Entity.EnabledLavaCommands ),
                () => entity.EnabledLavaCommands = string.Join( ",", box.Entity.EnabledLavaCommands.Select( x => x.Value ) ) );

            box.IfValidProperty( nameof( box.Entity.SecurityMode ),
               () => entity.SecurityMode = box.Entity.SecurityMode ?? LavaEndpointSecurityMode.EndpointExecute );
            
            box.IfValidProperty( nameof( box.Entity.CacheControlHeaderSettings ),
                () => entity.CacheControlHeaderSettings = box.Entity.CacheControlHeaderSettings.ToCacheability().ToJson() );
                        
            box.IfValidProperty( nameof( box.Entity.RateLimitPeriodDurationSeconds ),
               () => entity.RateLimitPeriodDurationSeconds = box.Entity.RateLimitPeriodDurationSeconds );

            box.IfValidProperty( nameof( box.Entity.RateLimitRequestPerPeriod ),
               () => entity.RateLimitRequestPerPeriod = box.Entity.RateLimitRequestPerPeriod );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson, true, IsAttributeIncluded );
                } );

            // Set additional settings
            entity.SetAdditionalSettings( new LavaEndpointAdditionalSettings { EnableCrossSiteForgeryProtection = box.Entity.EnableCrossSiteForgeryProtection } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="LavaApplication"/> to be viewed or edited on the page.</returns>
        private LavaEndpoint GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<LavaEndpoint, LavaEndpointService>( rockContext, PageParameterKey.LavaEndpointId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var lavaApplicationId = RequestContext.GetPageParameter( PageParameterKey.LavaApplicationId );

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LavaApplicationId] = lavaApplicationId
                } )
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IHasAttributes entity )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( entity, RequestContext.CurrentPerson )
                .ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out LavaEndpoint entity, out BlockActionResult error )
        {
            var entityService = new LavaEndpointService( rockContext );
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
                entity = new LavaEndpoint();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LavaEndpoint.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LavaEndpoint.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the attribute should be included in the block.
        /// </summary>
        /// <param name="attribute">The attribute to be checked.</param>
        /// <returns><c>true</c> if the attribute should be included, <c>false</c> otherwise.</returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            // Don't include the special attributes "Order" and "Active".
            return attribute.Key != "Order" && attribute.Key != "Active";
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var key = pageReference.GetPageParameter( PageParameterKey.LavaApplicationId );
                var pageParameters = new Dictionary<string, string>();

                var name = new LavaEndpointService( rockContext )
                    .GetSelect( key, mf => mf.Name );

                if ( name != null )
                {
                    pageParameters.Add( PageParameterKey.LavaEndpointId, key );
                }

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                var breadCrumb = new BreadCrumbLink( name ?? "New Lava Endpoint", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
                };
            }
        }

        /// <summary>
        /// Gets the lava application identifier.
        /// </summary>
        /// <returns></returns>
        private int? GetLavaApplicationId()
        {
            var lavaApplicationIdParam = PageParameter( PageParameterKey.LavaApplicationId );
            var sectionId = lavaApplicationIdParam.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( lavaApplicationIdParam );
            return sectionId;
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Block action that determines if the slug already exists.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        [BlockAction]
        public bool ValidateRoute( string slug, LavaEndpointHttpMethod method )
        {
            var endpointId = RequestContext.GetPageParameter( PageParameterKey.LavaEndpointId ).AsInteger();

            var endpoints = LavaEndpointCache.All();

            return endpoints.Where( e =>
                                    e.Slug == slug
                                    && e.HttpMethod == method
                                    && e.Id != endpointId )
                            .Any();
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LavaEndpointService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateLavaEndpoint( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;
                if ( isNew )
                {
                    int? lavaApplicationId = GetLavaApplicationId();
                    if ( !lavaApplicationId.HasValue )
                    {
                        return ActionBadRequest( "Invalid Lava Application Id" );
                    }

                    entity.LavaApplicationId = lavaApplicationId.Value;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );



                var lavaApplication = new LavaApplicationService(rockContext).Get( entity.LavaApplicationId );

                return ActionContent( System.Net.HttpStatusCode.OK, this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LavaApplicationId] = lavaApplication.IdKey
                } ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LavaEndpointService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var applicationId = entity.LavaApplication.IdKey;
                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LavaApplicationId] = applicationId
                } ) );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<LavaEndpointBag, LavaEndpointDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}