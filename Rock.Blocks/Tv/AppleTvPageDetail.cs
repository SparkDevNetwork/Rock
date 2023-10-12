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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tv.Classes;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.AppleTvAppDetail;
using Rock.ViewModels.Blocks.Tv.AppleTvPageDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Allows a person to edit an Apple TV application.
    /// </summary>

    [DisplayName( "Apple TV Page Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV page." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d8419b3c-eda1-46fc-9810-b1d81fb37cb3" )]
    [Rock.SystemGuid.BlockTypeGuid( "adbf3377-a491-4016-9375-346496a25fb4" )]
    public class AppleTvPageDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string SitePageId = "SitePageId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        public override string ObsidianFileUrl => $"{base.ObsidianFileUrl}";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Page>();

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
        private AppleTvPageDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AppleTvPageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Page for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="page">The page to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Page is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePage( Page page, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Page.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Page.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Page.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AppleTvPageBag"/> that represents the entity.</returns>
        private AppleTvPageBag GetCommonEntityBag( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var response = entity.AdditionalSettings.FromJsonOrNull<ApplePageResponse>() ?? new ApplePageResponse();
            var cacheability = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();

            return new AppleTvPageBag
            {
                Description = entity.Description,
                IdKey = entity.IdKey,
                Name = entity.InternalName,
                ShowInMenu = entity.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed,
                PageTVML = response.Content,
                IsSystem = entity.IsSystem,
                RockCacheability = ToCacheabilityBag( cacheability ),
            };
        }

        /// <summary>
        /// Converts the <see cref="RockCacheability"/> entity to a <see cref="RockCacheabilityBag"/>
        /// </summary>
        /// <param name="cacheability">The <see cref="RockCacheability"/> entity</param>
        /// <returns></returns>
        private RockCacheabilityBag ToCacheabilityBag( RockCacheability cacheability )
        {
            var bag = new RockCacheabilityBag()
            {
                MaxAge = new TimeIntervalBag
                {
                    Unit = TimeIntervalUnit.Minutes,
                    Value = 0
                },
                RockCacheabilityType = RockCacheablityType.Public.ConvertToInt(),
                SharedMaxAge = new TimeIntervalBag
                {
                    Unit = TimeIntervalUnit.Minutes,
                    Value = 0
                }
            };

            if ( cacheability == null )
            {
                return bag;
            }

            if ( cacheability.MaxAge != null )
            {
                bag.MaxAge = new TimeIntervalBag
                {
                    Unit = cacheability.MaxAge.Unit,
                    Value = cacheability.MaxAge.Value
                };
            }

            if ( cacheability.RockCacheablityType == RockCacheablityType.Private )
            {
                bag.RockCacheabilityType = cacheability.RockCacheablityType.ConvertToInt();
            }
            else
            {
                bag.RockCacheabilityType = cacheability.RockCacheablityType.ConvertToInt();

                if ( cacheability.SharedMaxAge != null )
                {
                    bag.SharedMaxAge = new TimeIntervalBag
                    {
                        Unit = cacheability.SharedMaxAge.Unit,
                        Value = cacheability.SharedMaxAge.Value
                    };
                }
            }

            return bag;
        }

        /// <summary>
        /// Converts the <see cref="RockCacheabilityBag"/> to a <see cref="RockCacheability"/>.
        /// </summary>
        /// <param name="cacheControlHeaderSettings">The cacheability bag.</param>
        /// <returns></returns>
        private static RockCacheability ToCacheability( RockCacheabilityBag cacheControlHeaderSettings )
        {
            var cacheability = new RockCacheability()
            {
                MaxAge = new TimeInterval
                {
                    Unit = cacheControlHeaderSettings.MaxAge.Unit,
                    Value = cacheControlHeaderSettings.MaxAge.Value,
                },
                RockCacheablityType = ( RockCacheablityType ) cacheControlHeaderSettings.RockCacheabilityType,
            };

            if ( cacheability.RockCacheablityType == RockCacheablityType.Public )
            {
                cacheability.SharedMaxAge = new TimeInterval
                {
                    Unit = cacheControlHeaderSettings.SharedMaxAge.Unit,
                    Value = cacheControlHeaderSettings.SharedMaxAge.Value,
                };
            }

            return cacheability;
        }

        /// <summary>
        /// Updates the additional settings fot the entity.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private static void SaveAdditionalSettings( AppleTvPageBag bag, Page entity )
        {
            var pageResponse = entity.AdditionalSettings.FromJsonOrNull<ApplePageResponse>() ?? new ApplePageResponse();
            pageResponse.Content = bag.PageTVML;
            entity.AdditionalSettings = pageResponse.ToJson();
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="AppleTvPageBag"/> that represents the entity.</returns>
        private AppleTvPageBag GetEntityBagForView( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.PageGuid = entity.Guid.ToString();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="AppleTvPageBag"/> that represents the entity.</returns>
        private AppleTvPageBag GetEntityBagForEdit( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Page entity, DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
                () =>
                {
                    entity.InternalName = box.Entity.Name;
                    entity.BrowserTitle = box.Entity.Name;
                    entity.PageTitle = box.Entity.Name;
                } );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.ShowInMenu ),
                () => entity.DisplayInNavWhen = box.Entity.ShowInMenu ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never );

            box.IfValidProperty( nameof( box.Entity.RockCacheability ),
                () => entity.CacheControlHeaderSettings = ToCacheability( box.Entity.RockCacheability ).ToJson() );

            box.IfValidProperty( nameof( box.Entity.PageTVML ),
                () => SaveAdditionalSettings( box.Entity, entity ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Page"/> to be viewed or edited on the page.</returns>
        private Page GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Page, PageService>( rockContext, PageParameterKey.SitePageId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId )
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
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( Page entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Page entity, out BlockActionResult error )
        {
            var entityService = new PageService( rockContext );
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
                entity = new Page();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Page.FriendlyTypeName}." );
                return false;
            }

            return true;
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

                var box = new DetailBlockBox<AppleTvPageBag, AppleTvAppDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();
                var entityService = new PageService( rockContext );
                var site = SiteCache.Get( applicationId );

                if ( site == null )
                {
                    return ActionBadRequest( "Please provide the Apple Application this page belongs to." );
                }

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
                if ( !ValidatePage( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                if ( isNew )
                {
                    entity.ParentPageId = site.DefaultPageId;
                    entity.LayoutId = site.DefaultPage.LayoutId;

                    // Set the order of the new page to be the last one
                    var currentMaxOrder = entityService.GetByParentPageId( site.DefaultPageId )
                        .OrderByDescending( p => p.Order )
                        .Select( p => p.Order )
                        .FirstOrDefault();
                    entity.Order = currentMaxOrder + 1;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    rockContext.SaveChanges();
                } );

                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId )
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
                var entityService = new PageService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<AppleTvPageBag, AppleTvPageDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<AppleTvPageBag, AppleTvAppDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
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

        /// <summary>
        /// Helper class for Apple Page Additional Settings Configuration
        /// </summary>
        private sealed class ApplePageResponse
        {
            /// <summary>
            /// Gets or sets the content.
            /// </summary>
            public string Content { get; set; }
        }

        #endregion
    }
}
