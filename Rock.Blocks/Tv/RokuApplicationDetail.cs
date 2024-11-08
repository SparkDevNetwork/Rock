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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tv.Classes;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.RokuApplicationDetail;
using Rock.ViewModels.Blocks.Cms.SiteDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Displays the details of a Roku application.
    /// </summary>

    [DisplayName( "Roku Application Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Displays the details of a Roku application." )]
    [IconCssClass( "fa fa-tv" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "89843e83-addb-4140-aa54-926adccd5558" )]
    [Rock.SystemGuid.BlockTypeGuid( "261903df-8632-456b-8272-4e4fff07147a" )]
    public class RokuApplicationDetail : RockEntityDetailBlockType<Site, RokuApplicationBag>
    {
        #region Keys

        /// <summary>
        /// The page parameter keys for this block.
        /// </summary>
        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string ShowRokuComponents = "ShowRokuComponents";
        }

        /// <summary>
        /// The navigation URL keys for this block.
        /// </summary>
        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<RokuApplicationBag, RokuApplicationDetailOptionsBag>();

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
        private RokuApplicationDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new RokuApplicationDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Site for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="site">The Site to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Site is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSite( Site site, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RokuApplicationBag, RokuApplicationDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Site.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Site.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Site.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SiteBag"/> that represents the entity.</returns>
        private RokuApplicationBag GetCommonEntityBag( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var siteSettings = entity.AdditionalSettings.FromJsonOrNull<RokuTvApplicationSettings>();

            return new RokuApplicationBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                LoginPage = entity.LoginPage.ToListItemBag(),
                Name = entity.Name,
                EnablePageViews = entity.EnablePageViews,
                ApiKey = GetApiKey( entity ),
                IsActive = entity.IsActive,
                PageViewRetentionDuration = GetPageViewRetentionDuration( entity ),
                ShowRokuComponents = PageParameter( PageParameterKey.ShowRokuComponents ).AsBoolean(),
                RokuComponents = siteSettings?.RockComponents
            };
        }

        /// <inheritdoc/>
        protected override RokuApplicationBag GetEntityBagForView( Site entity )
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
        protected override RokuApplicationBag GetEntityBagForEdit( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            if( entity.Id == 0 )
            {
                bag.RokuComponents = RokuConstants.DEFAULT_ROKU_COMPONENTS;
            }

            return bag;
        }

        /// <summary>
        /// Saves the API key.
        /// </summary>
        /// <param name="restLoginId">The rest login identifier.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private int SaveApiKey( int? restLoginId, string siteName, string apiKey, string userName, RockContext rockContext )
        {
            var userLoginService = new UserLoginService( rockContext );
            var personService = new PersonService( rockContext );
            UserLogin userLogin = null;
            Person restPerson = null;

            // the key gets saved in the api key field of a user login (which you have to create if needed)
            var entityType = new EntityTypeService( rockContext )
                .Get( "Rock.Security.Authentication.Database" );

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
            restPerson.LastName = siteName;
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

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Site entity, ValidPropertiesBox<RokuApplicationBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.LoginPage ),
                () => entity.LoginPageId = box.Bag.LoginPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.EnablePageViews ),
                () => entity.EnablePageViews = box.Bag.EnablePageViews );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override Site GetInitialEntity()
        {
            return GetInitialEntity<Site, SiteService>( RockContext, PageParameterKey.SiteId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( RockContext );
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
        /// Gets the API key for the Roku application.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private string GetApiKey( Site site )
        {
            // Get API key
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<RokuTvApplicationSettings>();
            if( additionalSettings == null )
            {
                return string.Empty;
            }

            var apiKeyLogin = new UserLoginService( RockContext ).Get( additionalSettings.ApiKeyId ?? 0 );
            return apiKeyLogin?.ApiKey ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the page view retention duration for a specified site.
        /// </summary>
        /// <param name="site">The site for which to retrieve the page view retention duration.</param>
        /// <returns>The page view retention duration for the specified site, or null if not found.</returns>
        private int? GetPageViewRetentionDuration( Site site )
        {
            if( site.Id == 0 )
            {
                return null;
            }

            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var retentionDuration = new InteractionChannelService( new RockContext() ).Queryable()
                    .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == site.Id )
                    .Select( c => c.RetentionDuration )
                    .FirstOrDefault();

            return retentionDuration;
        }

        /// <summary>
        /// Sets the retention duration for page views on a site.
        /// </summary>
        /// <param name="site">The site for which to set the retention duration.</param>
        /// <param name="value">The retention duration value to set. Can be null.</param>
        private void SetPageViewRetentionDuration( Site site, int? value )
        {
            // Create interaction channel for this site
            var interactionChannelService = new InteractionChannelService( RockContext );
            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var interactionChannelForSite = interactionChannelService.Queryable()
                .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == site.Id ).FirstOrDefault();

            if ( interactionChannelForSite == null )
            {
                interactionChannelForSite = new InteractionChannel();
                interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                interactionChannelForSite.ChannelEntityId = site.Id;
                interactionChannelService.Add( interactionChannelForSite );
            }

            interactionChannelForSite.Name = site.Name;
            interactionChannelForSite.RetentionDuration = value;
            interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;
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

            return ActionOk( new ValidPropertiesBox<RokuApplicationBag>
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
        public BlockActionResult Save( ValidPropertiesBox<RokuApplicationBag> box )
        {
            var entityService = new SiteService( RockContext );

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
            if ( !ValidateSite( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;
            var additionalSettings = entity.AdditionalSettings.FromJsonOrNull<RokuTvApplicationSettings>() ?? new RokuTvApplicationSettings();
            additionalSettings.TvApplicationType = TvApplicationType.Roku;
            entity.SiteType = SiteType.Tv;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                SetPageViewRetentionDuration( entity, box.Bag.PageViewRetentionDuration );

                // Create/Modify API Key
                additionalSettings.ApiKeyId = SaveApiKey( additionalSettings.ApiKeyId, entity.Name, box.Bag.ApiKey, $"tv_application_{entity.Id}", RockContext );
                additionalSettings.RockComponents = box.Bag.RokuComponents;
                entity.AdditionalSettings = additionalSettings.ToJson();

                RockContext.SaveChanges();
            } );

            if ( isNew )
            {
                var layoutService = new LayoutService( RockContext );

                var layout = new Layout
                {
                    Name = "Homepage",
                    FileName = "Homepage.xaml",
                    Description = string.Empty,
                    SiteId = entity.Id
                };

                layoutService.Add( layout );
                RockContext.SaveChanges();

                var pageService = new PageService( RockContext );
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
                RockContext.SaveChanges();

                entity.DefaultPageId = page.Id;
                RockContext.SaveChanges();
                additionalSettings.RockComponents = RokuConstants.DEFAULT_ROKU_COMPONENTS;

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = entity.IdKey
                } ) );
            }

            entity.AdditionalSettings = additionalSettings.ToJson();
            RockContext.SaveChanges();

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<RokuApplicationBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new SiteService( RockContext );

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

        #endregion

        #region Helper Classes

        private static class RokuConstants
        {
            public const string DEFAULT_ROKU_COMPONENTS = @"###COMPONENT>Rock:Page###
<component name=""Rock:Page"" extends=""Group"">
    <interface>
        <field id=""initialFocus"" type=""string"" />
    </interface>
</component>
###COMPONENT>Rock:ContentNode###
<component name=""Rock:ContentNode"" extends=""ContentNode"">
    <interface>
        <field id=""rockCommand"" type=""string"" />
        <field id=""rockPageGuid"" type=""string"" />
        <field id=""rockPageCacheControl"" type=""string"" />
        <field id=""rockPageSuppressInteraction"" type=""bool"" />
        <field id=""rockPageShowLoading"" type=""bool"" />
        <field id=""rockVideoUrl"" type=""string"" />
        <field id=""rockVideoMediaElementGuid"" type=""string"" />
        <field id=""rockVideoRelatedEntityTypeIds"" type=""int"" />
        <field id=""rockVideoRelatedEntityId"" type=""int"" />
        <field id=""rockVideoEnableResume"" type=""bool"" value=""true"" />
        <field id=""rockVideoTitle"" type=""string"" />
        <field id=""rockVideoSubtitle"" type=""string"" />
        <field id=""rockVideoArtworkImageUrl"" type=""string"" />
        <field id=""rockVideoDescription"" type=""string"" />
        <field id=""rockAudioUrl"" type=""string"" />
        <field id=""rockAudioMediaElementGuid"" type=""string"" />
        <field id=""rockAudioRelatedEntityTypeIds"" type=""int"" />
        <field id=""rockAudioRelatedEntityId"" type=""int"" />
        <field id=""rockAudioEnableResume"" type=""bool"" value=""true"" />
        <field id=""rockAudioTitle"" type=""string"" />
        <field id=""rockAudioSubtitle"" type=""string"" />
        <field id=""rockAudioArtworkImageUrl"" type=""string"" />
        <field id=""rockAudioDescription"" type=""string"" />
        <field id=""rockInteractionGuid"" type=""string"" />
        <field id=""rockLoginPageGuid"" type=""string"" />
        <field id=""rockLoginSuccessPageGuid"" type=""string"" />
        <field id=""rockLogoutSuccessPageGuid"" type=""string"" />
        <field id=""rockWatchMap"" type=""string"" />
    </interface>
</component>
###COMPONENT>Rock:FocusGroup###
<component name=""Rock:FocusGroup"" extends=""LayoutGroup"">
    <script type=""text/brightscript"">
        <![CDATA[
        sub init()
            m.top.setFocus(true)  ' Set focus to the component itself
            m.currentIndex = 0    ' Start focus at the first child
        end sub

        ' Function to handle key press events
        function onKeyEvent(key as String, press as Boolean) as Boolean
            if press 
                layoutDirection = m.top.layoutDirection

                ' Handle left + down navigation
                if (layoutDirection = ""horiz"" and key = ""left"") or (layoutDirection = ""vert"" and key = ""down"")
                    handleLeftOrDownNavigation()
                    return true
                ' Handle up + right navigation
                elseif (layoutDirection = ""horiz"" and key = ""right"") or (layoutDirection = ""vert"" and key = ""up"")
                    handleUpOrRightNavigation()
                    return true
                end if
            end if
            return false
        end function

        ' Handle left and down navigation with wrapping
        sub handleLeftOrDownNavigation()
            if m.currentIndex > 0
                m.currentIndex -= 1
            else
                ' If we are at the first item, wrap to the last one
                m.currentIndex = m.top.getChildCount() - 1
            end if
            updateFocus()
        end sub

        ' Handle up and right navigation with wrapping
        sub handleUpOrRightNavigation()
            if m.currentIndex < m.top.getChildCount() - 1
                m.currentIndex += 1
            else
                ' If we are at the last item, wrap to the first one
                m.currentIndex = 0
            end if
            updateFocus()
        end sub

        ' Set focus to the current child based on m.currentIndex
        sub updateFocus()
            focusedChild = m.top.getChild(m.currentIndex)
            focusedChild.setFocus(true)
        end sub
        ]]>
    </script>
</component>
###COMPONENT>Rock:Button###
<component name=""Rock:Button"" extends=""Button"">
    <interface>
        <field id=""centerText"" type=""bool"" />
        <field id=""rockCommand"" type=""string"" />
        <field id=""rockPageGuid"" type=""string"" />
        <field id=""rockPageCacheControl"" type=""string"" />
        <field id=""rockPageSuppressInteraction"" type=""bool"" />
        <field id=""rockPageShowLoading"" type=""bool"" />
        <field id=""rockVideoUrl"" type=""string"" />
        <field id=""rockVideoMediaElementGuid"" type=""string"" />
        <field id=""rockVideoRelatedEntityTypeId"" type=""int"" />
        <field id=""rockVideoRelatedEntityId"" type=""int"" />
        <field id=""rockVideoEnableResume"" type=""bool"" value=""true"" />
        <field id=""rockVideoTitle"" type=""string"" />
        <field id=""rockVideoSubtitle"" type=""string"" />
        <field id=""rockVideoArtworkImageUrl"" type=""string"" />
        <field id=""rockVideoDescription"" type=""string"" />
        <field id=""rockAudioUrl"" type=""string"" />
        <field id=""rockAudioMediaElementGuid"" type=""string"" />
        <field id=""rockAudioRelatedEntityTypeIds"" type=""int"" />
        <field id=""rockAudioRelatedEntityId"" type=""int"" />
        <field id=""rockAudioEnableResume"" type=""bool"" value=""true"" />
        <field id=""rockAudioTitle"" type=""string"" />
        <field id=""rockAudioSubtitle"" type=""string"" />
        <field id=""rockAudioArtworkImageUrl"" type=""string"" />
        <field id=""rockAudioDescription"" type=""string"" />
        <field id=""rockInteractionGuid"" type=""string"" />
        <field id=""rockLoginPageGuid"" type=""string"" />
        <field id=""rockLoginSuccessPageGuid"" type=""string"" />
        <field id=""rockLogoutSuccessPageGuid"" type=""string"" />
        <field id=""rockWatchMap"" type=""string"" />
    </interface>

    <script type=""text/brightscript"">
        <![CDATA[
            sub init()
                for i = 0 to m.top.getChildCount() - 1
                    child = m.top.getChild(i)
                    if child.subtype() = ""Label""
                        child.horizAlign = ""center""
                        child.observeField(""horizAlign"", ""onHorizAlignChanged"")
                        exit for
                    end if
                end for
            end sub

            sub onHorizAlignChanged(msg as object)
                label = msg.getRoSGNode()
                if m.top.centerText and label.horizAlign <> ""center""
                    label.horizAlign = ""center""
                end if
            end sub
        ]]>
    </script>
</component>";
        }

        #endregion
    }
}
