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
using System.Linq;

using Humanizer;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Enums;
using Rock.Constants;
using Rock.Data;
using Rock.DownhillCss;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

using AdditionalSiteSettings = Rock.Mobile.AdditionalSiteSettings;
using ShellType = Rock.Common.Mobile.Enums.ShellType;
using TabLocation = Rock.Mobile.TabLocation;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// Edits and configures the settings of a mobile application (a Rock Site
    /// of type <see cref="SiteType.Mobile"/>). Surfaces the application,
    /// styling, layouts, pages, and deep-link configuration on a single
    /// tabbed detail view.
    /// </summary>
    [DisplayName( "Mobile Application Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile application." )]
    [IconCssClass( "ti ti-device-mobile" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Layout Detail",
        Description = "The page that displays the configuration for a mobile layout.",
        IsRequired = true,
        Key = AttributeKey.LayoutDetail,
        Order = 0 )]

    [LinkedPage(
        "Page Detail",
        Description = "The page that displays the configuration for a mobile page.",
        IsRequired = true,
        Key = AttributeKey.PageDetail,
        Order = 1 )]

    [LinkedPage(
        "Deep Link Detail",
        Description = "The page that displays the configuration for a deep link route.",
        IsRequired = true,
        Key = AttributeKey.DeepLinkDetail,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4C512762-BD3B-4DA7-AA03-4BADE42BA897" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "50203BFE-CF77-4EBF-B472-AADEB37E043B" )]
    [Rock.SystemGuid.BlockTypeGuid( "1D001ED9-F711-4820-BED0-92150D069BA2" )]
    public class MobileApplicationDetail : RockEntityDetailBlockType<Site, MobileApplicationBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LayoutDetail = "LayoutDetail";
            public const string PageDetail = "PageDetail";
            public const string DeepLinkDetail = "DeepLinkDetail";
        }

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string Tab = "Tab";
        }

        private static class NavigationUrlKey
        {
            public const string LayoutDetailPage = "LayoutDetailPage";
            public const string LayoutDetailEditPage = "LayoutDetailEditPage";
            public const string PageDetailPage = "PageDetailPage";
            public const string DeepLinkDetailPage = "DeepLinkDetailPage";
            public const string DeepLinkDetailEditPage = "DeepLinkDetailEditPage";
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The default Phone/Tablet XAML used for a brand-new mobile
        /// application's seeded Homepage layout.
        /// </summary>
        private const string DefaultLayoutXaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             xmlns:Rock=""clr-namespace:Rock.Mobile.Cms;assembly=Rock.Mobile""
             xmlns:Common=""clr-namespace:Rock.Mobile.Common;assembly=Rock.Mobile.Common"">
    <ScrollView>
        <StackLayout>
            <Rock:Zone ZoneName=""Main"" />
        </StackLayout>
    </ScrollView>
</ContentPage>";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<MobileApplicationBag, MobileApplicationDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.NavigationUrls = GetBoxNavigationUrls();

            if ( entity != null && ( entity.Id == 0 || entity.SiteType == SiteType.Mobile ) )
            {
                box.Options = GetBoxOptions( entity, box.IsEditable );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }

            return box;
        }

        /// <summary>
        /// Builds the standard breadcrumb for a mobile application page.
        /// </summary>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.SiteId );
            var pageParameters = new Dictionary<string, string>();
            var name = "New Application";

            if ( key.IsNotNullOrWhiteSpace() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var siteName = new SiteService( rockContext ).GetSelect( key, s => s.Name );

                    if ( siteName.IsNotNullOrWhiteSpace() )
                    {
                        pageParameters.Add( PageParameterKey.SiteId, key );
                        name = siteName;
                    }
                }
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageParameters );
            var breadCrumb = new BreadCrumbLink( name, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the entity bag
        /// or sets an error message based on permissions and existence.
        /// </summary>
        private void SetBoxInitialEntityState( DetailBlockBox<MobileApplicationBag, MobileApplicationDetailOptionsBag> box, Site entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = "That mobile application does not exist.";
                return;
            }

            // Limit this block to mobile-type sites — both for safety on an
            // existing site and to default new sites to the mobile shape.
            if ( entity.Id != 0 && entity.SiteType != SiteType.Mobile )
            {
                box.ErrorMessage = "This block only supports mobile sites.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( "mobile application" );
                }
            }
            else
            {
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( "mobile application" );
                }
            }
        }

        /// <summary>
        /// Builds the read-only options bag (enums, picker qualifiers, view
        /// summary, current styles values).
        /// </summary>
        private MobileApplicationDetailOptionsBag GetBoxOptions( Site entity, bool isEditable )
        {
            var settings = GetAdditionalSettings( entity );
            var options = new MobileApplicationDetailOptionsBag
            {
                SiteId = entity.Id,
                IsEditable = isEditable,
                Styles = GetStylesBagFromSettings( entity, settings ),
                ApplicationDetailsHtml = entity.Id == 0 ? null : BuildApplicationDetailsHtml( settings, isEditable ),
                PreviewThumbnailUrl = entity.ThumbnailBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( entity.ThumbnailBinaryFileId )
                    : null,
                DeepLinkDomainsText = BuildDeepLinkDomainsText( settings.DeepLinkDomains ),
                ApplicationTypes = typeof( ShellType ).ToEnumListItemBag(),
                AndroidTabLocations = typeof( TabLocation ).ToEnumListItemBag(),
                DeviceOrientations = BuildDeviceOrientationListItemBags(),
                MobileStyleFrameworks = BuildMobileStyleFrameworkListItemBags(),
                IOSBlurStyles = typeof( IOSBlurStyle ).ToEnumListItemBag(),
                ConnectionStatusDefinedTypeGuid = SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid(),
                RecordStatusDefinedTypeGuid = SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid(),
                PersonAttributeCategoryEntityTypeGuid = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) )?.Guid,
                PersonAttributeCategoryQualifierColumn = "EntityTypeId",
                PersonAttributeCategoryQualifierValue = EntityTypeCache.Get( typeof( Person ) ).Id.ToString(),
                CampusFilterEntityTypeGuid = SystemGuid.EntityType.CAMPUS.AsGuid()
            };

            if ( entity.LatestVersionDateTime.HasValue )
            {
                var deployTimeSpan = RockDateTime.Now - entity.LatestVersionDateTime.Value;
                options.LastDeployText = $"Last Deploy: {deployTimeSpan.Humanize()} ago";
                options.LastDeployTooltip = entity.LatestVersionDateTime.Value.ToString( "dddd, MMMM d, yyyy h:mm tt" );
            }

            return options;
        }

        /// <summary>
        /// Builds the navigation URL dictionary for child detail page links
        /// and the parent page redirect.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.LayoutDetailPage] = this.GetLinkedPageUrl( AttributeKey.LayoutDetail, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["LayoutId"] = "((Key))"
                } ),
                [NavigationUrlKey.LayoutDetailEditPage] = this.GetLinkedPageUrl( AttributeKey.LayoutDetail, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["LayoutId"] = "((Key))",
                    ["autoEdit"] = "true"
                } ),
                [NavigationUrlKey.PageDetailPage] = this.GetLinkedPageUrl( AttributeKey.PageDetail, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["Page"] = "((Key))"
                } ),
                [NavigationUrlKey.DeepLinkDetailPage] = this.GetLinkedPageUrl( AttributeKey.DeepLinkDetail, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["DeepLinkRouteGuid"] = "((Key))"
                } ),
                [NavigationUrlKey.DeepLinkDetailEditPage] = this.GetLinkedPageUrl( AttributeKey.DeepLinkDetail, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["DeepLinkRouteGuid"] = "((Key))",
                    ["AutoEdit"] = "true"
                } ),
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Builds the subset of the entity bag that is safe to ship in both
        /// view and edit responses (identifier, name, description, active
        /// flag, and the deep-linking flag that drives view-mode tab
        /// visibility). Sensitive or edit-only fields — API key, OAuth /
        /// deep-link credentials, XAML / Lava templates, page pickers — are
        /// added by <see cref="GetEntityBagForEdit"/> instead so they are
        /// not exposed to view-only users via the bag payload.
        /// </summary>
        private MobileApplicationBag GetCommonEntityBag( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var settings = GetAdditionalSettings( entity );

            return new MobileApplicationBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name ?? string.Empty,
                IsActive = entity.Id == 0 ? true : entity.IsActive,
                Description = entity.Description ?? string.Empty,
                IsDeepLinkingEnabled = settings.IsDeepLinkingEnabled
            };
        }

        /// <inheritdoc/>
        protected override MobileApplicationBag GetEntityBagForView( Site entity )
        {
            var bag = GetCommonEntityBag( entity );

            if ( bag == null )
            {
                return null;
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override MobileApplicationBag GetEntityBagForEdit( Site entity )
        {
            var bag = GetCommonEntityBag( entity );

            if ( bag == null )
            {
                return null;
            }

            var settings = GetAdditionalSettings( entity );

            bag.ApiKey = LoadApiKey( settings.ApiKeyId ) ?? GenerateUniqueApiKey();
            bag.ApplicationType = ( int ) ( settings.ShellType ?? ShellType.Flyout );
            bag.AndroidTabLocation = ( int ) ( settings.TabLocation ?? TabLocation.Bottom );
            bag.LockPhoneOrientation = ( int ) settings.LockedPhoneOrientation;
            bag.LockTabletOrientation = ( int ) settings.LockedTabletOrientation;
            bag.LoginPage = PageToListItemBag( entity.LoginPageId );
            bag.ProfilePage = PageToListItemBag( settings.ProfilePageId );
            bag.InteractiveExperiencePage = PageToListItemBag( settings.InteractiveExperiencePageId );
            bag.CommunicationViewPage = PageToListItemBag( settings.CommunicationViewPageId );
            bag.SmsConversationPage = PageToListItemBag( settings.SmsConversationPageId );
            bag.ChatPage = PageToListItemBag( settings.ChatPageId );
            bag.OutreachToolboxTouchpointPage = PageToListItemBag( settings.OutreachToolboxTouchpointPageId );
            bag.CampusFilterDataView = DataViewToListItemBag( settings.CampusFilterDataViewId );
            bag.PersonAttributeCategories = CategoryCache.All( RockContext )
                .Where( c => settings.PersonAttributeCategories.Contains( c.Id ) )
                .Select( c => c.ToListItemBag() )
                .ToList();
            bag.Auth0ClientId = settings.Auth0ClientId;
            bag.Auth0Domain = settings.Auth0Domain;
            bag.Auth0ConnectionStatus = DefinedValueToListItemBag(
                settings.Auth0ConnectionStatusValueId
                ?? DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() )?.Id );
            bag.Auth0RecordStatus = DefinedValueToListItemBag(
                settings.Auth0RecordStatusValueId
                ?? DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() )?.Id );
            bag.EntraClientId = settings.EntraClientId;
            bag.EntraTenantId = settings.EntraTenantId;
            bag.EntraAuthenticationComponent = ComponentGuidToListItemBag( settings.EntraAuthenticationComponent );
            bag.EnableNotificationsAutomatically = settings.EnableNotificationsAutomatically;
            bag.FlyoutXaml = settings.FlyoutXaml;
            bag.NavigationBarActionXaml = settings.NavigationBarActionXaml;
            bag.HomepageRoutingLogic = settings.HomepageRoutingLogic;
            bag.PreviewThumbnailBinaryFile = BinaryFileToListItemBag( entity.ThumbnailBinaryFileId );
            bag.PushTokenUpdateValue = settings.PushTokenUpdateValue;
            bag.CompressUpdatePackages = settings.IsPackageCompressionEnabled;
            bag.PageViewRetentionPeriodDays = GetPageViewRetentionPeriodDays( entity );
            bag.IsDeepLinkPrefixLocked = settings.DeepLinkPathPrefix.IsNotNullOrWhiteSpace();
            bag.DeepLinkPathPrefix = settings.DeepLinkPathPrefix.IsNotNullOrWhiteSpace()
                ? $"/{settings.DeepLinkPathPrefix}/"
                : settings.DeepLinkPathPrefix;
            bag.DeepLinkDomains = settings.DeepLinkDomains;
            bag.BundleIdentifier = settings.BundleIdentifier;
            bag.TeamIdentifier = settings.TeamIdentifier;
            bag.PackageName = settings.PackageName;
            bag.CertificateFingerprint = settings.CertificateFingerprint;

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Site entity, ValidPropertiesBox<MobileApplicationBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var settings = GetAdditionalSettings( entity );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.LoginPage ),
                () => entity.LoginPageId = box.Bag.LoginPage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ApplicationType ),
                () => settings.ShellType = ( ShellType ) box.Bag.ApplicationType );

            box.IfValidProperty( nameof( box.Bag.AndroidTabLocation ),
                () => settings.TabLocation = ( TabLocation ) box.Bag.AndroidTabLocation );

            box.IfValidProperty( nameof( box.Bag.LockPhoneOrientation ),
                () => settings.LockedPhoneOrientation = ( DeviceOrientation ) box.Bag.LockPhoneOrientation );

            box.IfValidProperty( nameof( box.Bag.LockTabletOrientation ),
                () => settings.LockedTabletOrientation = ( DeviceOrientation ) box.Bag.LockTabletOrientation );

            box.IfValidProperty( nameof( box.Bag.ProfilePage ),
                () => settings.ProfilePageId = box.Bag.ProfilePage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.InteractiveExperiencePage ),
                () => settings.InteractiveExperiencePageId = box.Bag.InteractiveExperiencePage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CommunicationViewPage ),
                () => settings.CommunicationViewPageId = box.Bag.CommunicationViewPage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.SmsConversationPage ),
                () => settings.SmsConversationPageId = box.Bag.SmsConversationPage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ChatPage ),
                () => settings.ChatPageId = box.Bag.ChatPage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.OutreachToolboxTouchpointPage ),
                () => settings.OutreachToolboxTouchpointPageId = box.Bag.OutreachToolboxTouchpointPage.GetEntityId<Rock.Model.Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CampusFilterDataView ),
                () => settings.CampusFilterDataViewId = box.Bag.CampusFilterDataView.GetEntityId<DataView>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.PersonAttributeCategories ),
                () => settings.PersonAttributeCategories = ( box.Bag.PersonAttributeCategories ?? new List<ListItemBag>() )
                    .Select( c => c.GetEntityId<Category>( RockContext ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList() );

            box.IfValidProperty( nameof( box.Bag.Auth0ClientId ),
                () => settings.Auth0ClientId = box.Bag.Auth0ClientId );

            box.IfValidProperty( nameof( box.Bag.Auth0Domain ),
                () => settings.Auth0Domain = box.Bag.Auth0Domain );

            box.IfValidProperty( nameof( box.Bag.Auth0ConnectionStatus ),
                () => settings.Auth0ConnectionStatusValueId = box.Bag.Auth0ConnectionStatus.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Auth0RecordStatus ),
                () => settings.Auth0RecordStatusValueId = box.Bag.Auth0RecordStatus.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.EntraClientId ),
                () => settings.EntraClientId = box.Bag.EntraClientId );

            box.IfValidProperty( nameof( box.Bag.EntraTenantId ),
                () => settings.EntraTenantId = box.Bag.EntraTenantId );

            box.IfValidProperty( nameof( box.Bag.EntraAuthenticationComponent ),
                () => settings.EntraAuthenticationComponent = box.Bag.EntraAuthenticationComponent?.Value.AsGuidOrNull() );

            box.IfValidProperty( nameof( box.Bag.EnableNotificationsAutomatically ),
                () => settings.EnableNotificationsAutomatically = box.Bag.EnableNotificationsAutomatically );

            box.IfValidProperty( nameof( box.Bag.FlyoutXaml ),
                () => settings.FlyoutXaml = box.Bag.FlyoutXaml );

            box.IfValidProperty( nameof( box.Bag.NavigationBarActionXaml ),
                () => settings.NavigationBarActionXaml = box.Bag.NavigationBarActionXaml );

            box.IfValidProperty( nameof( box.Bag.HomepageRoutingLogic ),
                () => settings.HomepageRoutingLogic = box.Bag.HomepageRoutingLogic );

            box.IfValidProperty( nameof( box.Bag.PreviewThumbnailBinaryFile ),
                () => entity.ThumbnailBinaryFileId = box.Bag.PreviewThumbnailBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.PushTokenUpdateValue ),
                () => settings.PushTokenUpdateValue = box.Bag.PushTokenUpdateValue );

            box.IfValidProperty( nameof( box.Bag.CompressUpdatePackages ),
                () => settings.IsPackageCompressionEnabled = box.Bag.CompressUpdatePackages );

            box.IfValidProperty( nameof( box.Bag.IsDeepLinkingEnabled ),
                () => settings.IsDeepLinkingEnabled = box.Bag.IsDeepLinkingEnabled );

            // Deep linking detail fields are persisted only when the feature
            // is enabled — turning the toggle off should not wipe credentials
            // that may be in use elsewhere.
            if ( box.Bag.IsDeepLinkingEnabled )
            {
                // The prefix is locked once persisted. See IsDeepLinkPrefixLocked
                // for why; we only write the prefix on the first save.
                if ( !IsDeepLinkPrefixLocked( settings ) )
                {
                    box.IfValidProperty( nameof( box.Bag.DeepLinkPathPrefix ),
                        () => settings.DeepLinkPathPrefix = ( box.Bag.DeepLinkPathPrefix ?? string.Empty ).Trim( '/' ) );
                }

                box.IfValidProperty( nameof( box.Bag.DeepLinkDomains ),
                    () => settings.DeepLinkDomains = box.Bag.DeepLinkDomains );

                box.IfValidProperty( nameof( box.Bag.BundleIdentifier ),
                    () => settings.BundleIdentifier = box.Bag.BundleIdentifier );

                box.IfValidProperty( nameof( box.Bag.TeamIdentifier ),
                    () => settings.TeamIdentifier = box.Bag.TeamIdentifier );

                box.IfValidProperty( nameof( box.Bag.PackageName ),
                    () => settings.PackageName = box.Bag.PackageName );

                box.IfValidProperty( nameof( box.Bag.CertificateFingerprint ),
                    () => settings.CertificateFingerprint = box.Bag.CertificateFingerprint );
            }

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            // Make sure Downhill / DownhillSettings are mobile-flagged before
            // the JSON is re-serialized.
            if ( settings.DownhillSettings == null )
            {
                settings.DownhillSettings = new DownhillSettings();
            }
            settings.DownhillSettings.Platform = DownhillPlatform.Mobile;

            entity.AdditionalSettings = settings.ToJson();

            return true;
        }

        /// <inheritdoc/>
        protected override Site GetInitialEntity()
        {
            var site = GetInitialEntity<Site, SiteService>( RockContext, PageParameterKey.SiteId );

            if ( site != null && site.Id == 0 )
            {
                // Seed the new-site defaults that the Add flow expects.
                site.SiteType = SiteType.Mobile;
                site.IsActive = true;

                if ( site.AdditionalSettings.IsNullOrWhiteSpace() )
                {
                    site.AdditionalSettings = new AdditionalSiteSettings
                    {
                        IsPackageCompressionEnabled = true
                    }.ToJson();
                }
            }

            return site;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new Site
                {
                    SiteType = SiteType.Mobile
                };
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( "Mobile application not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile application" ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds the security grant token used by attribute editors so the
        /// frontend can perform attribute-aware operations safely.
        /// </summary>
        private string GetSecurityGrantToken( Site entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            if ( entity != null )
            {
                securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );
            }

            return securityGrant.ToToken();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Returns the editable bag for an existing application's Application
        /// tab, used when the user clicks Edit.
        /// </summary>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<MobileApplicationBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the Application tab fields, persisting both Site columns and
        /// the AdditionalSettings JSON. Seeds the default Homepage layout +
        /// page when creating a new application.
        /// </summary>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<MobileApplicationBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Validate uniqueness of deep-link prefix before doing any state
            // mutation so we can return a clean error message. Skip when the
            // prefix is locked — UpdateEntityFromBox won't write it anyway.
            // See IsDeepLinkPrefixLocked for why the lock is permanent.
            var existingSettings = GetAdditionalSettings( entity );

            if ( box.Bag.IsDeepLinkingEnabled && !IsDeepLinkPrefixLocked( existingSettings ) )
            {
                var deepLinkPrefix = ( box.Bag.DeepLinkPathPrefix ?? string.Empty ).Trim( '/' );

                if ( deepLinkPrefix.IsNotNullOrWhiteSpace() )
                {
                    var conflictingRoute = new PageRouteService( RockContext ).Queryable()
                        .AsEnumerable()
                        .Any( r => r.Route.StartsWith( $"{deepLinkPrefix}/" ) || r.Route == deepLinkPrefix );

                    var conflictingDeepLinkPathPrefix = new SiteService( RockContext ).Queryable()
                        .AsEnumerable()
                        .Any( s => s.Id != entity.Id
                            && s.AdditionalSettings.IsNotNullOrWhiteSpace()
                            && s.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>()?.DeepLinkPathPrefix == deepLinkPrefix );

                    if ( conflictingRoute || conflictingDeepLinkPathPrefix )
                    {
                        return ActionBadRequest( $"The 'Deep Link Path Prefix' ('{deepLinkPrefix}') is currently conflicting with another route or path prefix. Please check 'Settings > CMS Configuration > Routes' or pick a unique deep link path prefix." );
                    }
                }
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var isNew = entity.Id == 0;
            var settings = GetAdditionalSettings( entity );

            // Mark any newly attached binary file as permanent.
            var binaryFileService = new BinaryFileService( RockContext );
            if ( entity.ThumbnailBinaryFileId.HasValue )
            {
                binaryFileService.Get( entity.ThumbnailBinaryFileId.Value ).IsTemporary = false;
            }

            if ( isNew )
            {
                SeedNewMobileApplicationDefaults( entity, settings, box.Bag.ApiKey );
            }
            else
            {
                settings.ApiKeyId = SaveApiKey( settings.ApiKeyId, box.Bag.ApiKey, $"mobile_application_{entity.Id}", entity.Name );
                settings.DownhillSettings.Platform = DownhillPlatform.Mobile;
                entity.AdditionalSettings = settings.ToJson();
                RockContext.SaveChanges();
            }

            // Save attributes after the entity has an Id.
            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            // Sync the page-view retention duration onto the site's
            // interaction channel.
            box.IfValidProperty( nameof( box.Bag.PageViewRetentionPeriodDays ),
                () => UpsertInteractionChannelRetention( entity, box.Bag.PageViewRetentionPeriodDays ) );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = entity.Id.ToString()
                } ) );
            }

            // Reload to capture any post-save derived values.
            var refreshed = new SiteService( RockContext ).Get( entity.Id );
            refreshed.LoadAttributes( RockContext );

            var refreshedBag = GetEntityBagForView( refreshed );
            var refreshedSettings = GetAdditionalSettings( refreshed );

            // Return both the refreshed entity bag AND the server-rendered
            // options that depend on entity / settings state. Without these
            // the view-mode UI (application summary HTML, preview thumbnail,
            // deep-link domains banner) would stay frozen at the pre-save
            // value until the page was reloaded.
            return ActionOk( new MobileApplicationSaveResponseBag
            {
                Bag = new ValidPropertiesBox<MobileApplicationBag>
                {
                    Bag = refreshedBag,
                    ValidProperties = refreshedBag.GetType().GetProperties().Select( p => p.Name ).ToList()
                },
                ApplicationDetailsHtml = BuildApplicationDetailsHtml( refreshedSettings, isEditable: true ),
                PreviewThumbnailUrl = refreshed.ThumbnailBinaryFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( refreshed.ThumbnailBinaryFileId )
                    : null,
                DeepLinkDomainsText = BuildDeepLinkDomainsText( refreshedSettings.DeepLinkDomains )
            } );
        }

        /// <summary>
        /// Saves the Styles tab. Style state is persisted entirely inside the
        /// Site's AdditionalSettings JSON (plus the favicon binary files on
        /// the Site row).
        /// </summary>
        [BlockAction]
        public BlockActionResult SaveStyles( MobileApplicationStylesBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Style settings are required." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile application" ) );
            }

            var entity = ResolveSite();

            if ( entity == null )
            {
                return ActionBadRequest( "Mobile application not found." );
            }

            var settings = GetAdditionalSettings( entity );

            // Defensive: a pathological persisted JSON could leave these null,
            // and the field-by-field assignments below would NRE.
            if ( settings.DownhillSettings == null )
            {
                settings.DownhillSettings = new DownhillSettings();
            }
            if ( settings.DownhillSettings.ApplicationColors == null )
            {
                settings.DownhillSettings.ApplicationColors = new ApplicationColors();
            }

            entity.FavIconBinaryFileId = bag.LightHeaderImage.GetEntityId<BinaryFile>( RockContext );
            settings.DarkFavIconBinaryFileId = bag.DarkHeaderImage.GetEntityId<BinaryFile>( RockContext );
            settings.BarBackgroundColor = bag.BarBackgroundColor;
            settings.IOSEnableBarTransparency = bag.IsNavBarTransparent;
            settings.IOSBarBlurStyle = ( IOSBlurStyle ) bag.NavBarBlurStyle;

            var framework = ( MobileStyleFramework ) bag.MobileStyleFramework;

            // Legacy Xamarin Forms color slots are still consumed by V5 and
            // earlier shells; they are required as long as the user is on
            // Legacy or Blended. Suppressing the obsolete warnings is
            // intentional — these are still load-bearing on legacy shells.
            if ( framework == MobileStyleFramework.Blended || framework == MobileStyleFramework.Legacy )
            {
                settings.MenuButtonColor = bag.MenuButtonColor;
                settings.ActivityIndicatorColor = bag.ActivityIndicatorColor;
                settings.DownhillSettings.TextColor = bag.TextColor;
                settings.DownhillSettings.HeadingColor = bag.HeadingColor;
                settings.DownhillSettings.BackgroundColor = bag.BackgroundColor;

#pragma warning disable CS0618 // Type or member is obsolete
                settings.DownhillSettings.ApplicationColors.Primary = bag.Primary;
                settings.DownhillSettings.ApplicationColors.Secondary = bag.Secondary;
                settings.DownhillSettings.ApplicationColors.Success = bag.Success;
                settings.DownhillSettings.ApplicationColors.Info = bag.Info;
                settings.DownhillSettings.ApplicationColors.Danger = bag.Danger;
                settings.DownhillSettings.ApplicationColors.Warning = bag.Warning;
                settings.DownhillSettings.ApplicationColors.Light = bag.Light;
                settings.DownhillSettings.ApplicationColors.Dark = bag.Dark;
                settings.DownhillSettings.ApplicationColors.Brand = bag.Brand;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if ( framework == MobileStyleFramework.Blended || framework == MobileStyleFramework.Standard )
            {
                settings.DownhillSettings.ApplicationColors.InterfaceStrongest = bag.InterfaceStrongest;
                settings.DownhillSettings.ApplicationColors.InterfaceStronger = bag.InterfaceStronger;
                settings.DownhillSettings.ApplicationColors.InterfaceStrong = bag.InterfaceStrong;
                settings.DownhillSettings.ApplicationColors.InterfaceMedium = bag.InterfaceMedium;
                settings.DownhillSettings.ApplicationColors.InterfaceSoft = bag.InterfaceSoft;
                settings.DownhillSettings.ApplicationColors.InterfaceSofter = bag.InterfaceSofter;
                settings.DownhillSettings.ApplicationColors.InterfaceSoftest = bag.InterfaceSoftest;

                settings.DownhillSettings.ApplicationColors.PrimaryStrong = bag.PrimaryStrong;
                settings.DownhillSettings.ApplicationColors.PrimarySoft = bag.PrimarySoft;
                settings.DownhillSettings.ApplicationColors.SecondaryStrong = bag.SecondaryStrong;
                settings.DownhillSettings.ApplicationColors.SecondarySoft = bag.SecondarySoft;
                settings.DownhillSettings.ApplicationColors.BrandStrong = bag.BrandStrong;
                settings.DownhillSettings.ApplicationColors.BrandSoft = bag.BrandSoft;

                settings.DownhillSettings.ApplicationColors.SuccessStrong = bag.SuccessStrong;
                settings.DownhillSettings.ApplicationColors.SuccessSoft = bag.SuccessSoft;
                settings.DownhillSettings.ApplicationColors.InfoStrong = bag.InfoStrong;
                settings.DownhillSettings.ApplicationColors.InfoSoft = bag.InfoSoft;
                settings.DownhillSettings.ApplicationColors.DangerStrong = bag.DangerStrong;
                settings.DownhillSettings.ApplicationColors.DangerSoft = bag.DangerSoft;
                settings.DownhillSettings.ApplicationColors.WarningStrong = bag.WarningStrong;
                settings.DownhillSettings.ApplicationColors.WarningSoft = bag.WarningSoft;
            }

            settings.DownhillSettings.FontSizeDefault = bag.FontSizeDefault;
            settings.DownhillSettings.Platform = DownhillPlatform.Mobile;
            settings.DownhillSettings.MobileStyleFramework = framework;
            settings.CssStyle = bag.CssStyles;

            entity.AdditionalSettings = settings.ToJson();

            // Mark image binary files permanent.
            var binaryFileService = new BinaryFileService( RockContext );
            if ( entity.FavIconBinaryFileId.HasValue )
            {
                binaryFileService.Get( entity.FavIconBinaryFileId.Value ).IsTemporary = false;
            }
            if ( settings.DarkFavIconBinaryFileId.HasValue )
            {
                binaryFileService.Get( settings.DarkFavIconBinaryFileId.Value ).IsTemporary = false;
            }

            RockContext.SaveChanges();

            return ActionOk( GetStylesBagFromSettings( entity, settings ) );
        }

        /// <summary>
        /// Triggers an asynchronous build of the mobile application package.
        /// </summary>
        [BlockAction]
        public async System.Threading.Tasks.Task<BlockActionResult> Deploy()
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile application" ) );
            }

            var entity = ResolveSite();
            if ( entity == null || entity.Id == 0 )
            {
                return ActionBadRequest( "Mobile application not found." );
            }

            using ( var rockContext = new RockContext() )
            {
                var siteService = new SiteService( rockContext );
                await siteService.BuildMobileApplicationAsync( entity.Id );
            }

            // Refresh the deploy badge.
            var refreshed = new SiteService( RockContext ).Get( entity.Id );
            var settings = GetAdditionalSettings( refreshed );

            return ActionOk( new MobileApplicationDeployResponseBag
            {
                LastDeployText = refreshed.LatestVersionDateTime.HasValue
                    ? $"Last Deploy: {( RockDateTime.Now - refreshed.LatestVersionDateTime.Value ).Humanize()} ago"
                    : null,
                LastDeployTooltip = refreshed.LatestVersionDateTime?.ToString( "dddd, MMMM d, yyyy h:mm tt" ),
                ApplicationDetailsHtml = BuildApplicationDetailsHtml( settings, isEditable: true )
            } );
        }

        /// <summary>
        /// Returns the Layouts grid data for the active site.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetLayoutsGridData()
        {
            var entity = ResolveSite();
            var grid = new GridBuilder<MobileApplicationLayoutBag>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddTextField( "name", r => r.Name )
                .AddTextField( "description", r => r.Description );

            if ( entity == null )
            {
                return ActionOk( grid.Build( Enumerable.Empty<MobileApplicationLayoutBag>().AsQueryable() ) );
            }

            var rows = LayoutCache.All()
                .Where( l => l.SiteId == entity.Id )
                .OrderBy( l => l.Name )
                .Select( l => new MobileApplicationLayoutBag
                {
                    IdKey = IdHasher.Instance.GetHash( l.Id ),
                    Name = l.Name,
                    Description = l.Description
                } )
                .ToList();

            return ActionOk( grid.Build( rows.AsQueryable() ) );
        }

        /// <summary>
        /// Returns the Pages grid data for the active site, including the
        /// flag indicating which row is the site's default page so the UI
        /// can suppress the delete control on it.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetPagesGridData()
        {
            var entity = ResolveSite();
            var grid = new GridBuilder<MobileApplicationPageBag>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddTextField( "internalName", r => r.InternalName )
                .AddTextField( "layoutName", r => r.LayoutName )
                .AddTextField( "displayInNavWhen", r => r.DisplayInNavWhen )
                .AddField( "isDefaultPage", r => r.IsDefaultPage );

            if ( entity == null )
            {
                return ActionOk( grid.Build( Enumerable.Empty<MobileApplicationPageBag>().AsQueryable() ) );
            }

            var defaultPageId = SiteCache.Get( entity.Id )?.DefaultPageId;

            var rows = PageCache.All()
                .Where( p => p.SiteId == entity.Id )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .Select( p => new MobileApplicationPageBag
                {
                    IdKey = IdHasher.Instance.GetHash( p.Id ),
                    InternalName = p.InternalName,
                    LayoutName = p.Layout?.Name,
                    DisplayInNavWhen = p.DisplayInNavWhen.GetDisplayName(),
                    IsDefaultPage = defaultPageId.HasValue && p.Id == defaultPageId.Value
                } )
                .ToList();

            return ActionOk( grid.Build( rows.AsQueryable() ) );
        }

        /// <summary>
        /// Returns the Deep Links grid data for the active site.
        /// Page titles are pre-resolved from PageCache to avoid a per-row
        /// service lookup.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetDeepLinksGridData()
        {
            var entity = ResolveSite();
            var rows = new List<MobileApplicationDeepLinkBag>();

            if ( entity != null )
            {
                var settings = GetAdditionalSettings( entity );

                if ( settings.DeepLinkRoutes != null )
                {
                    rows = settings.DeepLinkRoutes
                        .Select( r => new MobileApplicationDeepLinkBag
                        {
                            Guid = r.Guid.ToString(),
                            Route = $"{settings.DeepLinkPathPrefix}/{r.Route}",
                            Page = ResolvePageTitle( r.MobilePageGuid ),
                            Fallback = r.UsesUrlAsFallback
                                ? r.WebFallbackPageUrl
                                : ResolvePageTitle( r.WebFallbackPageGuid ),
                            IsUrl = r.UsesUrlAsFallback
                        } )
                        .ToList();
                }
            }

            var grid = new GridBuilder<MobileApplicationDeepLinkBag>()
                .WithBlock( this )
                .AddTextField( "guid", r => r.Guid )
                .AddTextField( "route", r => r.Route )
                .AddTextField( "page", r => r.Page )
                .AddTextField( "fallback", r => r.Fallback )
                .AddField( "isUrl", r => r.IsUrl );

            return ActionOk( grid.Build( rows.AsQueryable() ) );
        }

        /// <summary>
        /// Deletes the specified mobile layout.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeleteLayout( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile layout" ) );
            }

            var layoutService = new LayoutService( RockContext );
            var layout = layoutService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( layout == null )
            {
                return ActionBadRequest( "Layout not found." );
            }

            if ( !layoutService.CanDelete( layout, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            layoutService.Delete( layout );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified mobile page after CanDelete validation.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeletePage( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile page" ) );
            }

            var pageService = new PageService( RockContext );
            var page = pageService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( page == null )
            {
                return ActionBadRequest( "Page not found." );
            }

            if ( !pageService.CanDelete( page, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            pageService.Delete( page );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders pages within the active site.
        /// </summary>
        [BlockAction]
        public BlockActionResult ReorderPage( string key, string beforeKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "mobile page" ) );
            }

            var entity = ResolveSite();
            if ( entity == null )
            {
                return ActionBadRequest( "Mobile application not found." );
            }

            var pageService = new PageService( RockContext );
            var pages = pageService.GetBySiteId( entity.Id )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .ToList();

            if ( !pages.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Unable to reorder pages." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Removes a deep link route from the site's AdditionalSettings JSON.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeleteDeepLink( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "deep link" ) );
            }

            var routeGuid = key.AsGuidOrNull();
            if ( !routeGuid.HasValue )
            {
                return ActionBadRequest( "Invalid deep link identifier." );
            }

            var entity = ResolveSite();
            if ( entity == null )
            {
                return ActionBadRequest( "Mobile application not found." );
            }

            var settings = GetAdditionalSettings( entity );
            var route = settings.DeepLinkRoutes?.FirstOrDefault( r => r.Guid == routeGuid.Value );

            if ( route == null )
            {
                return ActionBadRequest( "Deep link not found." );
            }

            settings.DeepLinkRoutes.Remove( route );
            entity.AdditionalSettings = settings.ToJson();
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders deep link routes within the AdditionalSettings JSON.
        /// </summary>
        [BlockAction]
        public BlockActionResult ReorderDeepLink( string key, string beforeKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( "deep link" ) );
            }

            var movedGuid = key.AsGuidOrNull();
            if ( !movedGuid.HasValue )
            {
                return ActionBadRequest( "Invalid deep link identifier." );
            }

            var entity = ResolveSite();
            if ( entity == null )
            {
                return ActionBadRequest( "Mobile application not found." );
            }

            var settings = GetAdditionalSettings( entity );
            var routes = settings.DeepLinkRoutes ?? new List<DeepLinkRoute>();

            var moved = routes.FirstOrDefault( r => r.Guid == movedGuid.Value );
            if ( moved == null )
            {
                return ActionBadRequest( "Deep link not found." );
            }

            routes.Remove( moved );

            var beforeGuid = beforeKey.AsGuidOrNull();
            if ( beforeGuid.HasValue )
            {
                var insertIndex = routes.FindIndex( r => r.Guid == beforeGuid.Value );
                if ( insertIndex < 0 )
                {
                    routes.Add( moved );
                }
                else
                {
                    routes.Insert( insertIndex, moved );
                }
            }
            else
            {
                routes.Add( moved );
            }

            settings.DeepLinkRoutes = routes;
            entity.AdditionalSettings = settings.ToJson();
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Reads the AdditionalSettings JSON off the site, returning a
        /// freshly-initialized instance if the JSON is empty or invalid.
        /// </summary>
        private static AdditionalSiteSettings GetAdditionalSettings( Site entity )
        {
            return entity.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
        }

        /// <summary>
        /// Once a deep-link prefix has been persisted on a site, it is
        /// permanently locked. Changing it would invalidate every
        /// previously-deployed deep-link URL on user devices, so we only
        /// honor a prefix update when no prefix is currently persisted.
        /// The lock keys off the persisted prefix alone (not the
        /// IsDeepLinkingEnabled flag) so that disabling and later
        /// re-enabling deep linking does NOT re-open the prefix for
        /// editing. Save() uses this to skip uniqueness validation when
        /// the prefix won't change anyway; UpdateEntityFromBox() uses it
        /// to enforce the immutability when persisting.
        /// </summary>
        private static bool IsDeepLinkPrefixLocked( AdditionalSiteSettings settings )
        {
            return settings.DeepLinkPathPrefix.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Resolves the active Site from the SiteId page parameter, honoring
        /// the IdKey-vs-Id setting on the current site. Returns null when
        /// the parameter is missing or the site does not exist.
        /// </summary>
        private Site ResolveSite()
        {
            var siteIdParam = PageParameter( PageParameterKey.SiteId );

            if ( siteIdParam.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new SiteService( RockContext ).Get( siteIdParam, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Materializes the Styles bag from a site's persisted settings.
        /// Reads the legacy color slots through the obsolete-suppression
        /// pragma since they remain required for older shells.
        /// </summary>
        private MobileApplicationStylesBag GetStylesBagFromSettings( Site entity, AdditionalSiteSettings settings )
        {
            var downhill = settings.DownhillSettings ?? new DownhillSettings();
            var colors = downhill.ApplicationColors ?? new ApplicationColors();

#pragma warning disable CS0618 // Type or member is obsolete
            return new MobileApplicationStylesBag
            {
                MobileStyleFramework = ( int ) downhill.MobileStyleFramework,

                InterfaceStrongest = colors.InterfaceStrongest,
                InterfaceStronger = colors.InterfaceStronger,
                InterfaceStrong = colors.InterfaceStrong,
                InterfaceMedium = colors.InterfaceMedium,
                InterfaceSoft = colors.InterfaceSoft,
                InterfaceSofter = colors.InterfaceSofter,
                InterfaceSoftest = colors.InterfaceSoftest,

                PrimaryStrong = colors.PrimaryStrong,
                PrimarySoft = colors.PrimarySoft,
                SecondaryStrong = colors.SecondaryStrong,
                SecondarySoft = colors.SecondarySoft,
                BrandStrong = colors.BrandStrong,
                BrandSoft = colors.BrandSoft,

                SuccessStrong = colors.SuccessStrong,
                SuccessSoft = colors.SuccessSoft,
                InfoStrong = colors.InfoStrong,
                InfoSoft = colors.InfoSoft,
                DangerStrong = colors.DangerStrong,
                DangerSoft = colors.DangerSoft,
                WarningStrong = colors.WarningStrong,
                WarningSoft = colors.WarningSoft,

                BarBackgroundColor = settings.BarBackgroundColor,
                IsNavBarTransparent = settings.IOSEnableBarTransparency,
                NavBarBlurStyle = ( int ) settings.IOSBarBlurStyle,
                LightHeaderImage = BinaryFileToListItemBag( entity.FavIconBinaryFileId ),
                DarkHeaderImage = BinaryFileToListItemBag( settings.DarkFavIconBinaryFileId ),

                MenuButtonColor = settings.MenuButtonColor,
                ActivityIndicatorColor = settings.ActivityIndicatorColor,
                TextColor = downhill.TextColor,
                HeadingColor = downhill.HeadingColor,
                BackgroundColor = downhill.BackgroundColor,
                Primary = colors.Primary,
                Secondary = colors.Secondary,
                Success = colors.Success,
                Info = colors.Info,
                Danger = colors.Danger,
                Warning = colors.Warning,
                Light = colors.Light,
                Dark = colors.Dark,
                Brand = colors.Brand,

                FontSizeDefault = ( int ) downhill.FontSizeDefault,
                CssStyles = settings.CssStyle ?? string.Empty
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Builds the formatted application-summary HTML used by the view
        /// panel. Server-rendered to avoid duplicating list-formatting logic
        /// in Vue. All field values are HTML-encoded so user-editable strings
        /// (notably category names) cannot inject markup into the rendered
        /// summary.
        /// </summary>
        /// <param name="settings">Persisted site settings.</param>
        /// <param name="isEditable">When <c>true</c>, the API Key row is
        /// included; otherwise it is omitted so view-only users do not see
        /// the credential.</param>
        private string BuildApplicationDetailsHtml( AdditionalSiteSettings settings, bool isEditable )
        {
            var fields = new List<KeyValuePair<string, string>>();

            if ( settings.ShellType.HasValue )
            {
                fields.Add( new KeyValuePair<string, string>( "Application Type", settings.ShellType.ToString() ) );
            }

            /*
                5/4/26 - MSE

                Gated on isEditable so view-only users do not see the
                credential in the rendered summary HTML.

                Reason: Prevent API key exposure to view-only users.
            */
            if ( isEditable )
            {
                var apiKey = LoadApiKey( settings.ApiKeyId );
                if ( apiKey.IsNotNullOrWhiteSpace() )
                {
                    fields.Add( new KeyValuePair<string, string>( "API Key", apiKey ) );
                }
            }

            if ( settings.LastDeploymentDate.HasValue )
            {
                fields.Add( new KeyValuePair<string, string>( "Last Deployed", settings.LastDeploymentDate.Value.ToShortDateTimeString() ) );
            }

            var selectedCategoryNames = CategoryCache.All( RockContext )
                .Where( c => settings.PersonAttributeCategories.Contains( c.Id ) )
                .Select( c => c.Name )
                .ToList();
            if ( selectedCategoryNames.Any() )
            {
                fields.Add( new KeyValuePair<string, string>( "Person Attribute Categories", string.Join( ", ", selectedCategoryNames ) ) );
            }

            return string.Concat( fields
                .Select( f => $"<dl><dt>{f.Key.EncodeHtml()}</dt><dd>{f.Value.EncodeHtml()}</dd></dl>" ) );
        }

        /// <summary>
        /// Renders the persisted pipe-delimited deep link domains as a
        /// comma-separated list. Returns null when no domains are configured.
        /// </summary>
        /// <remarks>
        /// The persisted format may or may not include a trailing pipe
        /// (older saves wrote "a|b|", current saves write "a|b"). Splitting
        /// on '|' and dropping empty entries lets either shape produce the
        /// same display.
        /// </remarks>
        private static string BuildDeepLinkDomainsText( string deepLinkDomains )
        {
            if ( deepLinkDomains.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var domains = deepLinkDomains
                .Split( '|' )
                .Where( d => d.IsNotNullOrWhiteSpace() )
                .ToList();

            return domains.Any() ? string.Join( ", ", domains ) : null;
        }

        /// <summary>
        /// Resolves a page Guid to its display title via <see cref="PageCache"/>,
        /// returning the empty string when the Guid is null and "No Page" when
        /// the page no longer exists.
        /// </summary>
        private static string ResolvePageTitle( Guid? pageGuid )
        {
            if ( !pageGuid.HasValue )
            {
                return string.Empty;
            }

            return PageCache.Get( pageGuid.Value )?.PageTitle ?? "No Page";
        }

        /// <summary>
        /// Loads the API key associated with the application's user login.
        /// </summary>
        private string LoadApiKey( int? userLoginId )
        {
            if ( !userLoginId.HasValue )
            {
                return null;
            }

            return new UserLoginService( RockContext ).Get( userLoginId.Value )?.ApiKey;
        }

        /// <summary>
        /// Generates a fresh, unique API key string by querying for collisions
        /// against the UserLogin table.
        /// </summary>
        private static string GenerateUniqueApiKey()
        {
            return KeyHelper.GenerateKey( ( RockContext rockContext, string key )
                => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        /// <summary>
        /// Seeds a freshly-saved mobile application with the defaults the
        /// Add flow expects: a Homepage layout (using the embedded default
        /// XAML), a Homepage page, the site's DefaultPageId pointing at
        /// that page, and a persisted API key. All work is wrapped in a
        /// single transaction so a partial failure does not leave the
        /// application in a half-seeded state.
        /// </summary>
        private void SeedNewMobileApplicationDefaults( Site entity, AdditionalSiteSettings settings, string apiKey )
        {
            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                // Persist the API key now that the site has an Id we can
                // tie the rest user to.
                settings.ApiKeyId = SaveApiKey( settings.ApiKeyId, apiKey, $"mobile_application_{entity.Id}", entity.Name );
                entity.AdditionalSettings = settings.ToJson();

                var pageService = new PageService( RockContext );
                var layoutService = new LayoutService( RockContext );
                var pageName = $"{entity.Name} Homepage";

                var layout = new Layout
                {
                    Name = "Homepage",
                    FileName = "Homepage.xaml",
                    Description = string.Empty,
                    LayoutMobilePhone = DefaultLayoutXaml,
                    LayoutMobileTablet = DefaultLayoutXaml,
                    SiteId = entity.Id
                };

                layoutService.Add( layout );
                RockContext.SaveChanges();

                var page = new Rock.Model.Page
                {
                    InternalName = pageName,
                    BrowserTitle = pageName,
                    PageTitle = pageName,
                    Description = string.Empty,
                    LayoutId = layout.Id,
                    DisplayInNavWhen = Rock.Model.DisplayInNavWhen.WhenAllowed
                };

                pageService.Add( page );
                RockContext.SaveChanges();

                entity.DefaultPageId = page.Id;
                RockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Persists the API key to a UserLogin and ensures a backing rest
        /// person exists. Returns the user login id.
        /// </summary>
        private int SaveApiKey( int? userLoginId, string apiKey, string userName, string applicationName )
        {
            var userLoginService = new UserLoginService( RockContext );
            var personService = new PersonService( RockContext );
            UserLogin userLogin = null;
            Person restPerson = null;

            var entityType = new EntityTypeService( RockContext )
                .Get( "Rock.Security.Authentication.Database" );

            if ( userLoginId.HasValue )
            {
                userLogin = userLoginService.Get( userLoginId.Value );
                restPerson = userLogin?.Person;
            }

            if ( userLogin == null )
            {
                var groupService = new GroupService( RockContext );
                var groupMemberService = new GroupMemberService( RockContext );

                restPerson = new Person();
                personService.Add( restPerson );

                var mobileApplicationUsersGroupGuid = SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS.AsGuid();
                var mobileApplicationUsersGroup = groupService
                    .Queryable()
                    .FirstOrDefault( g => g.Guid == mobileApplicationUsersGroupGuid );

                if ( mobileApplicationUsersGroup != null )
                {
                    var groupRoleId = GroupTypeCache.Get( mobileApplicationUsersGroup.GroupTypeId ).DefaultGroupRoleId;

                    if ( groupRoleId.HasValue )
                    {
                        var groupMember = new GroupMember
                        {
                            Person = restPerson,
                            GroupId = mobileApplicationUsersGroup.Id,
                            GroupRoleId = groupRoleId.Value
                        };
                        groupMemberService.Add( groupMember );
                    }
                }
            }

            // Use the application name as the person's last name so the
            // REST user shows up sensibly in the person picker.
            restPerson.LastName = applicationName;
            restPerson.RecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            restPerson.RecordStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            RockContext.SaveChanges();

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

            RockContext.SaveChanges();

            return userLogin.Id;
        }

        /// <summary>
        /// Returns the current page-view retention duration (in days) for the
        /// site's website-medium interaction channel, or null when none has
        /// been configured.
        /// </summary>
        private int? GetPageViewRetentionPeriodDays( Site entity )
        {
            if ( entity.Id == 0 )
            {
                return null;
            }

            var channelMediumWebsiteValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            return new InteractionChannelService( RockContext ).Queryable()
                .Where( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == entity.Id )
                .Select( c => c.RetentionDuration )
                .FirstOrDefault();
        }

        /// <summary>
        /// Creates or updates the website-medium interaction channel that
        /// captures page views for this mobile site, applying the desired
        /// retention period.
        /// </summary>
        private void UpsertInteractionChannelRetention( Site entity, int? retentionDurationDays )
        {
            var interactionChannelService = new InteractionChannelService( RockContext );
            var channelMediumWebsiteValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;

            var channel = interactionChannelService.Queryable()
                .FirstOrDefault( c => c.ChannelTypeMediumValueId == channelMediumWebsiteValueId && c.ChannelEntityId == entity.Id );

            if ( channel == null )
            {
                channel = new InteractionChannel
                {
                    ChannelTypeMediumValueId = channelMediumWebsiteValueId,
                    ChannelEntityId = entity.Id
                };
                interactionChannelService.Add( channel );
            }

            channel.Name = entity.Name;
            channel.RetentionDuration = retentionDurationDays;
            channel.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Builds a list-item bag for a page identifier.
        /// </summary>
        private static ListItemBag PageToListItemBag( int? pageId )
        {
            return pageId.HasValue ? PageCache.Get( pageId.Value )?.ToListItemBag() : null;
        }

        /// <summary>
        /// Builds a list-item bag for a data view identifier.
        /// </summary>
        private static ListItemBag DataViewToListItemBag( int? dataViewId )
        {
            return dataViewId.HasValue ? DataViewCache.Get( dataViewId.Value )?.ToListItemBag() : null;
        }

        /// <summary>
        /// Builds a list-item bag for a defined value identifier.
        /// </summary>
        private static ListItemBag DefinedValueToListItemBag( int? definedValueId )
        {
            return definedValueId.HasValue ? DefinedValueCache.Get( definedValueId.Value )?.ToListItemBag() : null;
        }

        /// <summary>
        /// Builds a list-item bag for a binary file identifier.
        /// </summary>
        private ListItemBag BinaryFileToListItemBag( int? binaryFileId )
        {
            if ( !binaryFileId.HasValue )
            {
                return null;
            }

            var data = new BinaryFileService( RockContext )
                .GetSelect( binaryFileId.Value, b => new { b.Guid, b.FileName } );

            return data == null
                ? null
                : new ListItemBag { Value = data.Guid.ToString(), Text = data.FileName };
        }

        /// <summary>
        /// Builds a list-item bag for an authentication-component Guid.
        /// </summary>
        private static ListItemBag ComponentGuidToListItemBag( Guid? componentGuid )
        {
            if ( !componentGuid.HasValue )
            {
                return null;
            }

            var entityType = EntityTypeCache.Get( componentGuid.Value );
            if ( entityType == null )
            {
                return null;
            }

            return new ListItemBag
            {
                Value = componentGuid.Value.ToString(),
                Text = entityType.FriendlyName
            };
        }

        /// <summary>
        /// Builds the device-orientation picker options. The "None" /
        /// "do not lock" choice is represented by the dropdown's blank
        /// item, which round-trips to <see cref="DeviceOrientation.Unknown"/>.
        /// </summary>
        private static List<ListItemBag> BuildDeviceOrientationListItemBags()
        {
            return new List<ListItemBag>
            {
                new ListItemBag { Value = ( ( int ) DeviceOrientation.Portrait ).ToString(), Text = "Portrait" },
                new ListItemBag { Value = ( ( int ) DeviceOrientation.Landscape ).ToString(), Text = "Landscape" }
            };
        }

        /// <summary>
        /// Builds the framework picker options with friendly labels (Default /
        /// Blended / Legacy) suitable for display in the Style Framework
        /// dropdown.
        /// </summary>
        private static List<ListItemBag> BuildMobileStyleFrameworkListItemBags()
        {
            return new List<ListItemBag>
            {
                new ListItemBag { Value = ( ( int ) MobileStyleFramework.Standard ).ToString(), Text = "Default (.NET MAUI)" },
                new ListItemBag { Value = ( ( int ) MobileStyleFramework.Blended ).ToString(), Text = "Blended (XF + MAUI)" },
                new ListItemBag { Value = ( ( int ) MobileStyleFramework.Legacy ).ToString(), Text = "Legacy (Xamarin Forms)" }
            };
        }

        #endregion
    }
}
