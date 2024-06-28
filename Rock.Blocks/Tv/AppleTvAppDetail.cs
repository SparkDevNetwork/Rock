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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.AppleTvAppDetail;
using Rock.Web.Cache;
using static Rock.Model.BenevolenceType;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Allows a person to edit an Apple TV application.
    /// </summary>

    [DisplayName( "Apple TV Application Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV application.." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e66d1530-8e39-4c00-8fa4-078482e56080" )]
    [Rock.SystemGuid.BlockTypeGuid( "cdab601d-1369-44cb-a146-4e80c7d66bcd" )]
    public class AppleTvAppDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string ShowApplicationJs = "ShowApplicationJs";
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
                var box = new DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Site>();

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
        private AppleTvAppDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AppleTvAppDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Site for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="site">The Site to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Site is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSite( Site site, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Site.FriendlyTypeName} was not found.";
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
                    box.Entity = GetEntityBagForView( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Site.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Site.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AppleTvAppBag"/> that represents the entity.</returns>
        private AppleTvAppBag GetCommonEntityBag( Site entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AppleTvAppBag
            {
                IdKey = entity.IdKey,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                Name = entity.Name,
                Description = entity.Description,
                EnablePageViews = entity.EnablePageViews,
                LoginPage = new ViewModels.Rest.Controls.PageRouteValueBag()
                {
                    Page = entity.LoginPage.ToListItemBag(),
                    Route = entity.LoginPageRoute.ToListItemBag(),
                },
                EnablePageViewGeoTracking = entity.EnablePageViewGeoTracking
            };

            var additionalSettings = entity.AdditionalSettings.FromJsonOrNull<AppleTvApplicationSettings>() ?? new AppleTvApplicationSettings();
            bag.ApplicationStyles = additionalSettings.ApplicationStyles;
            bag.ApplicationJavascript = additionalSettings.ApplicationScript;
            bag.ShowApplicationJavascript = PageParameter( PageParameterKey.ShowApplicationJs ).AsBoolean();

            // Set the API key
            if ( entity.Id > 0 )
            {
                var apiKeyLogin = new UserLoginService(rockContext).Get(additionalSettings.ApiKeyId ?? 0);
                bag.ApiKey = apiKeyLogin != null ? apiKeyLogin.ApiKey : GenerateApiKey();
            }

            // Get page view retention
            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            bag.PageViewRetentionPeriod = new InteractionChannelService( new RockContext() ).Queryable()
                    .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == entity.Id )
                    .Select( c => c.RetentionDuration )
                    .FirstOrDefault();

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="AppleTvAppBag"/> that represents the entity.</returns>
        private AppleTvAppBag GetEntityBagForView( Site entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="AppleTvAppBag"/> that represents the entity.</returns>
        private AppleTvAppBag GetEntityBagForEdit( Site entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            if ( entity.Id == 0 )
            {
                var stream = GetType().Assembly.GetManifestResourceStream( "Rock.Blocks.DefaultTvApplication.js" );

                if ( stream != null )
                {
                    using ( var reader = new StreamReader( stream ) )
                    {
                        bag.ApplicationJavascript = reader.ReadToEnd();
                    }
                }
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
        private bool UpdateEntityFromBox( Site entity, DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag> box, RockContext rockContext )
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

            box.IfValidProperty( nameof( box.Entity.EnablePageViews ),
                () => entity.EnablePageViews = box.Entity.EnablePageViews );

            box.IfValidProperty( nameof( box.Entity.EnablePageViewGeoTracking ),
                () => entity.EnablePageViewGeoTracking = box.Entity.EnablePageViewGeoTracking );

            box.IfValidProperty( nameof( box.Entity.LoginPage ),
                () =>
                {
                    entity.LoginPageId = box.Entity.LoginPage.Page.GetEntityId<Page>( rockContext );
                    entity.LoginPageRouteId = box.Entity.LoginPage.Route.GetEntityId<PageRoute>( rockContext );
                } );

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
        /// <returns>The <see cref="Site"/> to be viewed or edited on the page.</returns>
        private Site GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Site, SiteService>( rockContext, PageParameterKey.SiteId );
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
        private string GetSecurityGrantToken( Site entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( rockContext );
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
                entity = new Site();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Site.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Site.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <returns></returns>
        private string GenerateApiKey()
        {
            // Generate a unique random 12 digit api key
            return Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        /// <summary>
        /// Saves the API key.
        /// </summary>
        /// <param name="restLoginId">The rest login identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="applicationName">Name of the Apple Tv App.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private int SaveApiKey( int? restLoginId, string apiKey, string userName, string applicationName, RockContext rockContext )
        {
            var userLoginService = new UserLoginService( rockContext );
            var personService = new PersonService( rockContext );
            UserLogin userLogin = null;

            // the key gets saved in the api key field of a user login (which you have to create if needed)
            var entityType = new EntityTypeService( rockContext )
                .Get( "Rock.Security.Authentication.Database" );

            Person restPerson;
            if ( restLoginId.HasValue )
            {
                userLogin = userLoginService.Get( restLoginId.Value );
                restPerson = userLogin.Person;
            }
            else
            {
                restPerson = new Person();
                personService.Add( restPerson );
            }

            // the rest user name gets saved as the last name on a person
            restPerson.LastName = applicationName;
            restPerson.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            restPerson.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            rockContext.SaveChanges();

            if ( userLogin == null )
            {
                userLogin = new UserLogin();
                userLoginService.Add( userLogin );
            }

            userLogin.UserName = userName;
            userLogin.IsConfirmed = true;
            userLogin.ApiKey = apiKey;
            userLogin.EntityTypeId = entityType.Id;
            userLogin.PersonId = restPerson.Id;

            rockContext.SaveChanges();

            return userLogin.Id;
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

                var box = new DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SiteService( rockContext );

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
                if ( !ValidateSite( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;
                var additionalSettings = entity.AdditionalSettings.FromJsonOrNull<AppleTvApplicationSettings>() ?? new AppleTvApplicationSettings();

                entity.SiteType = SiteType.Tv;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    // Create/Modify API Key
                    additionalSettings.ApiKeyId = SaveApiKey( additionalSettings.ApiKeyId, box.Entity.ApiKey, $"tv_application_{entity.Id}", entity.Name, rockContext );
                    additionalSettings.ApplicationStyles = box.Entity.ApplicationStyles;
                    additionalSettings.ApplicationScript = box.Entity.ApplicationJavascript;
                    entity.AdditionalSettings = additionalSettings.ToJson();
                    rockContext.SaveChanges();
                } );

                // Create interaction channel for this site
                var interactionChannelService = new InteractionChannelService( rockContext );
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var interactionChannelForSite = interactionChannelService.Queryable()
                    .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == entity.Id ).FirstOrDefault();

                if ( interactionChannelForSite == null )
                {
                    interactionChannelForSite = new InteractionChannel();
                    interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                    interactionChannelForSite.ChannelEntityId = entity.Id;
                    interactionChannelService.Add( interactionChannelForSite );
                }

                interactionChannelForSite.Name = entity.Name;
                interactionChannelForSite.RetentionDuration = entity.EnablePageViews ? box.Entity.PageViewRetentionPeriod : null;
                interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Page>().Id;

                rockContext.SaveChanges();

                if ( isNew )
                {
                    var layoutService = new LayoutService( rockContext );

                    var layout = new Layout
                    {
                        Name = "Homepage",
                        FileName = "Homepage.xaml",
                        Description = string.Empty,
                        SiteId = entity.Id
                    };

                    layoutService.Add( layout );
                    rockContext.SaveChanges();

                    var pageService = new PageService( rockContext );
                    var page = new Rock.Model.Page
                    {
                        InternalName = "Start Screen",
                        BrowserTitle = "Start Screen",
                        PageTitle = "Start Screen",
                        DisplayInNavWhen = DisplayInNavWhen.WhenAllowed,
                        Description = string.Empty,
                        LayoutId = layout.Id,
                        Order = 0
                    };

                    pageService.Add( page );
                    rockContext.SaveChanges();

                    entity.DefaultPageId = page.Id;
                    rockContext.SaveChanges();

                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.SiteId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity, rockContext ) );
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
                var entityService = new SiteService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<AppleTvAppBag, AppleTvAppDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
