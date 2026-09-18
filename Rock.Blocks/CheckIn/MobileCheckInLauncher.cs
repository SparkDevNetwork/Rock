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
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Enums.CheckIn;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Rest.CheckIn;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Launch page for checking in from a person's mobile device.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Mobile Check-in Launcher" )]
    [Category( "Check-in" )]
    [Description( "Launch page for checking in from a person's mobile device." )]
    [IconCssClass( "ti ti-device-mobile" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #region Custom Settings

    [TextField( "Enabled Devices",
        Key = AttributeKey.EnabledDevices,
        Description = "The devices to consider when determining a matching device kiosk, or leave blank for all. Typically the selection should include only one device kiosk for each geo-fenced area / campus.",
        Category = AttributeCategory.CustomSetting,
        Order = 0,
        IsRequired = false )]

    [TextField( "Theme",
        Key = AttributeKey.CheckinTheme,
        Description = "The check-in theme this page renders in, overriding the theme configured on the site. Leave blank to use the site's theme.",
        Category = AttributeCategory.CustomSetting,
        Order = 1,
        IsRequired = false )]

    [TextField( "Check-in Configuration",
        Key = AttributeKey.CheckinConfiguration,
        Description = "The check-in configuration that will be used for the check-in process.",
        Category = AttributeCategory.CustomSetting,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_WEEKLY_SERVICE_CHECKIN_AREA,
        Order = 2,
        IsRequired = true )]

    [TextField( "Check-in Areas",
        Key = AttributeKey.CheckinAreas,
        Description = "The check-in areas that will be used for the check-in process.",
        Category = AttributeCategory.CustomSetting,
        Order = 3,
        IsRequired = true )]

    [BooleanField( "Disable Location Services",
        Key = AttributeKey.DisableLocationServices,
        Description = "If disabled, the mobile device's location services will not be used and instead a list of active campuses will be shown. The selected campus will be used to find a matching device from the Devices block setting.",
        Category = AttributeCategory.CustomSetting,
        DefaultBooleanValue = false,
        Order = 4,
        IsRequired = true )]

    #endregion Custom Settings

    #region Basic Settings > General Settings

    [BooleanField( "Disable QR Code",
        Key = AttributeKey.DisableQRCode,
        Description = "If disabled, no QR code is shown on the mobile device after check-in. Use this for events that do not print labels.",
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Select All Schedules Automatically",
        Key = AttributeKey.SelectAllSchedulesAutomatically,
        Description = "When enabled, all available schedules are selected automatically instead of asking the individual to make a selection. This will also disable the 'skip' screen when there is nothing to check into, instead those individuals will quietly be skipped and not checked in.",
        DefaultBooleanValue = false,
        Order = 1,
        IsRequired = false )]

    #endregion Basic Settings > General Settings

    #region Basic Settings > Mobile Person

    [LinkedPage( "Log In Page",
        Key = AttributeKey.LoginPage,
        Description = "The page to use for logging in the person. If blank the log in button will not be shown.",
        Category = AttributeCategory.BasicSettings_MobilePerson,
        Order = 0,
        IsRequired = false )]

    [LinkedPage( "Phone Identification Page",
        Key = AttributeKey.PhoneIdentificationPage,
        Description = "Page to use for identifying the person by phone number. If blank the button will not be shown.",
        Category = AttributeCategory.BasicSettings_MobilePerson,
        Order = 1,
        IsRequired = false )]

    #endregion Basic Settings > Mobile Person

    #region Basic Settings > Text

    [CodeEditorField( "Mobile Check-in Header",
        Key = AttributeKey.MobileCheckinHeaderTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Mobile Check-in",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 0,
        IsRequired = true )]

    [CodeEditorField( "Identify You Prompt Template",
        Key = AttributeKey.IdentifyYouPromptTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Before we proceed we'll need to identify you for check-in.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 1,
        IsRequired = true )]

    [CodeEditorField( "Allow Location Prompt",
        Key = AttributeKey.AllowLocationPromptTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to your location when you're visiting this site.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 2,
        IsRequired = true )]

    [CodeEditorField( "Location Progress",
        Key = AttributeKey.LocationProgressTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Determining location...",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 3,
        IsRequired = true )]

    [CodeEditorField( "Welcome Back",
        Key = AttributeKey.WelcomeBackTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! Great to see you back. Select the Check In button to get started.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 4,
        IsRequired = true )]

    [CodeEditorField( "No Services",
        Key = AttributeKey.NoServicesTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! There are currently no services ready for check-in at this time.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 5,
        IsRequired = true )]

    [CodeEditorField( "Can't Determine Location",
        Key = AttributeKey.UnableToDetermineLocationTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! We can't determine your location. Please be sure to enable location permissions for your device.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 6,
        IsRequired = true )]

    [CodeEditorField( "No Devices Found",
        Key = AttributeKey.NoDevicesFoundTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! Currently, you're not close enough to check in. Please try again once you're closer to the campus.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 7,
        IsRequired = true )]

    [CodeEditorField( "No People Message",
        Key = AttributeKey.NoPeopleMessageTemplate,
        Description = "Text to display when there is not anyone in the family that can check in.",
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Sorry, no one in your family is eligible to check in at this location.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 8,
        IsRequired = false )]

    [CodeEditorField( "No Campuses Found",
        Key = AttributeKey.NoCampusesFoundTemplate,
        Category = AttributeCategory.BasicSettings_Text,
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! There are currently no active campuses ready for check-in at this time.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 100,
        Order = 9,
        IsRequired = true )]

    #endregion Basic Settings > Text

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "FA4A6783-BFAA-4129-AE24-5BF871518EE9" )]
    [Rock.SystemGuid.BlockTypeGuid( "1703315B-6255-499D-9B27-76245A314640" )]
    //// was [Rock.SystemGuid.BlockTypeGuid( "1703315B-6255-499D-9B27-76245A314640" )]
    //[Rock.SystemGuid.BlockTypeGuid( "FA4D15E6-4C85-4247-A374-5E592E711CFD" )]
    public class MobileCheckInLauncher : RockBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            // Custom Settings
            public const string EnabledDevices = "DeviceIdList";
            public const string CheckinTheme = "CheckinTheme";
            public const string CheckinConfiguration = "CheckinConfiguration_GroupTypeGuid";
            public const string CheckinAreas = "ConfiguredAreas_GroupTypeIds";
            public const string DisableLocationServices = "DisableLocationServices";

            // Basic Settings > General Settings
            public const string DisableQRCode = "DisableQRCode";
            public const string SelectAllSchedulesAutomatically = "SelectAllSchedulesAutomatically";

            // Basic Settings > Mobile Person
            public const string LoginPage = "LoginPage";
            public const string PhoneIdentificationPage = "PhoneIdentificationPage";

            // Basic Settings > Text
            public const string MobileCheckinHeaderTemplate = "MobileCheckinHeader";
            public const string IdentifyYouPromptTemplate = "IdentifyYouPromptTemplate";
            public const string AllowLocationPromptTemplate = "AllowLocationPermissionPromptTemplate";
            public const string LocationProgressTemplate = "LocationProgress";
            public const string WelcomeBackTemplate = "WelcomeBackTemplate";
            public const string NoServicesTemplate = "NoScheduledDevicesAvailableTemplate";
            public const string UnableToDetermineLocationTemplate = "UnableToDetermineMobileLocationTemplate";
            public const string NoDevicesFoundTemplate = "NoDevicesFoundTemplate";
            public const string NoPeopleMessageTemplate = "NoPeopleMessage";
            public const string NoCampusesFoundTemplate = "NoCampusesFoundTemplate";
        }

        private static class AttributeCategory
        {
            public const string CustomSetting = "CustomSetting";

            public const string BasicSettings_GeneralSettings = "";
            public const string BasicSettings_MobilePerson = "Mobile Person";
            public const string BasicSettings_Text = "Text";
        }

        private static class PageParameterKey
        {
            public const string ImpersonationToken = "rckipid";
            public const string ReturnUrl = "returnUrl";
            public const string Theme = "theme";
        }

        private static class NavigationUrlKey
        {
            public const string LoginPage = "LoginPage";
            public const string PhoneIdentificationPage = "PhoneIdentificationPage";
        }

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            // The check-in screens this block reuses carry no styles of their own and are styled entirely by the
            // kiosk block's stylesheet.
            RequestContext.Response.AddCssLink( RequestContext.ResolveRockUrl( "~/Styles/Blocks/Checkin/CheckInKiosk.css" ), true );

            var themeRedirectUrl = GetThemeRedirectUrl();
            if ( themeRedirectUrl.IsNotNullOrWhiteSpace() )
            {
                // Nothing else is worth resolving for a page that is about to be replaced.
                return new MobileCheckInLauncherInitializationBox
                {
                    ThemeRedirectUrl = themeRedirectUrl
                };
            }

            var errorMessage = GetConfigurationErrorMessage();
            var headerTemplate = GetAttributeValue( AttributeKey.MobileCheckinHeaderTemplate );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return new MobileCheckInLauncherInitializationBox
                {
                    HeaderHtml = headerTemplate.ResolveMergeFields( RequestContext.GetCommonMergeFields() ),
                    ErrorMessage = errorMessage
                };
            }

            var individual = GetIdentifiedIndividual();

            // The Lava templates address the individual checking in, who is not always the logged in person.
            var mergeFields = RequestContext.GetCommonMergeFields( individual );

            var box = new MobileCheckInLauncherInitializationBox
            {
                HeaderHtml = headerTemplate.ResolveMergeFields( mergeFields ),
                IdentifyYouPromptHtml = GetAttributeValue( AttributeKey.IdentifyYouPromptTemplate ).ResolveMergeFields( mergeFields ),
                IsIndividualIdentified = individual != null,
                NavigationUrls = GetBoxNavigationUrls()
            };

            if ( individual == null )
            {
                return box;
            }

            box.AreAllSchedulesSelectedAutomatically = GetAttributeValue( AttributeKey.SelectAllSchedulesAutomatically ).AsBoolean();
            // The code is rebuilt from the cookie rather than held in the page, so a refresh does not cost the
            // individual the labels they just checked in for.
            var rememberedSessionGuids = GetOwnedAttendanceSessionGuids( GetAttendanceSessionGuidsFromCookie(), individual );

            box.IsCheckInCompleted = rememberedSessionGuids.Any();
            box.QrCodeImageUrl = GetAttendanceSessionQrCodeUrl( rememberedSessionGuids );

            box.NoPeopleMessageHtml = GetAttributeValue( AttributeKey.NoPeopleMessageTemplate ).ResolveMergeFields( mergeFields );
            box.FamilyIdKey = individual.PrimaryFamily?.IdKey;
            box.FamilyName = individual.PrimaryFamily?.Name;
            box.IsLocationServicesDisabled = GetAttributeValue( AttributeKey.DisableLocationServices ).AsBoolean();

            if ( box.IsLocationServicesDisabled )
            {
                SetCampusSelectionValues( box, individual, mergeFields );
            }
            else
            {
                box.IsLocationApprovalRemembered = RequestContext.GetCookieValue( CheckInCookieKey.RockHasLocationApproval ).AsBoolean();
                box.AllowLocationPromptHtml = GetAttributeValue( AttributeKey.AllowLocationPromptTemplate ).ResolveMergeFields( mergeFields );
                box.LocationProgressHtml = GetAttributeValue( AttributeKey.LocationProgressTemplate ).ResolveMergeFields( mergeFields );
                box.UnableToDetermineLocationHtml = GetAttributeValue( AttributeKey.UnableToDetermineLocationTemplate ).ResolveMergeFields( mergeFields );
            }

            return box;
        }

        #endregion RockBlockType Implementation

        #region IHasCustomActions Implementation

        /// <inheritdoc/>
        public List<BlockCustomActionBag> GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/CheckIn/mobileCheckInLauncherCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions Implementation

        #region Block Actions

        /// <summary>
        /// Gets the values and all other required details for the custom settings modal.
        /// </summary>
        /// <param name="deviceIds">The hashed identifiers of the currently selected devices, used to limit the
        /// available areas.</param>
        /// <returns>A box containing the custom settings values and options.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings( List<string> deviceIds )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var director = new CheckInDirector( RockContext );

            var devices = ( deviceIds ?? new List<string>() )
                .Select( idKey => DeviceCache.GetByIdKey( idKey, RockContext ) )
                .Where( d => d != null )
                .ToList();

            try
            {
                return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
                {
                    Options = new CustomSettingsOptionsBag
                    {
                        DeviceItems = GetDeviceItems(),
                        ThemeItems = GetThemeItems(),
                        CheckInConfigurationOptions = director.GetConfigurationTemplateBags(),
                        AreaItems = GetAreaItems( director, devices )
                    },
                    Settings = new CustomSettingsBag
                    {
                        Devices = GetConfiguredDeviceIdKeys(),
                        Theme = GetAttributeValue( AttributeKey.CheckinTheme ),
                        CheckInConfiguration = GetConfiguredCheckInConfigurationIdKey(),
                        CheckInAreas = GetConfiguredAreaIdKeys(),
                        IsLocationServicesDisabled = GetAttributeValue( AttributeKey.DisableLocationServices ).AsBoolean()
                    }
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Saves the values from the custom settings modal.
        /// </summary>
        /// <param name="box">The box containing the custom settings values.</param>
        /// <returns>A response that indicates if the save was successful.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> box )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( RockContext ).Get( BlockId );

            block.LoadAttributes( RockContext );

            box.IfValidProperty( nameof( box.Settings.Devices ),
                () => block.SetAttributeValue( AttributeKey.EnabledDevices, GetDeviceIdListAttributeValue( box.Settings.Devices ) ) );

            box.IfValidProperty( nameof( box.Settings.Theme ),
                () => block.SetAttributeValue( AttributeKey.CheckinTheme, box.Settings.Theme ) );

            box.IfValidProperty( nameof( box.Settings.CheckInConfiguration ),
                () => block.SetAttributeValue( AttributeKey.CheckinConfiguration, GetCheckinConfigurationAttributeValue( box.Settings.CheckInConfiguration ) ) );

            box.IfValidProperty( nameof( box.Settings.CheckInAreas ),
                () => block.SetAttributeValue( AttributeKey.CheckinAreas, GetConfiguredAreasAttributeValue( box.Settings.CheckInAreas ) ) );

            box.IfValidProperty( nameof( box.Settings.IsLocationServicesDisabled ),
                () => block.SetAttributeValue( AttributeKey.DisableLocationServices, box.Settings.IsLocationServicesDisabled.ToString() ) );

            block.SaveAttributeValues( RockContext );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Resolves the kiosk to check in at from the individual's current location.
        /// </summary>
        /// <param name="latitude">The latitude reported by the individual's browser.</param>
        /// <param name="longitude">The longitude reported by the individual's browser.</param>
        /// <returns>A bag containing the matched kiosk, or the message to show when nothing matched.</returns>
        [BlockAction]
        public BlockActionResult GetKioskByLocation( double latitude, double longitude )
        {
            // A reported position means permission was granted, so the prompt can be skipped next time.
            RememberLocationApproval();

            return ActionOk( GetKioskResolutionBag( GetKioskByGeoFencing( latitude, longitude ) ) );
        }

        /// <summary>
        /// Resolves the kiosk to check in at from the campus the individual picked.
        /// </summary>
        /// <param name="deviceIdKey">The hashed identifier of the kiosk device serving the picked campus.</param>
        /// <returns>A bag containing the matched kiosk, or the message to show when it is no longer available.</returns>
        [BlockAction]
        public BlockActionResult GetKioskByDevice( string deviceIdKey )
        {
            // The campus buttons are built from an explicit device list, so a blank setting offers no valid pick.
            var kiosk = GetConfiguredDeviceIds().Any() ? GetAllowedKiosk( deviceIdKey ) : null;

            return ActionOk( GetKioskResolutionBag( kiosk ) );
        }

        /// <summary>
        /// Evaluates whether check-in can start at the kiosk the individual resolved.
        /// </summary>
        /// <param name="kioskIdKey">The hashed identifier of the kiosk the individual is checking in at.</param>
        /// <returns>A bag containing the availability and the message to show with it.</returns>
        [BlockAction]
        public BlockActionResult GetKioskAvailability( string kioskIdKey )
        {
            var kiosk = GetAllowedKiosk( kioskIdKey );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            return ActionOk( GetKioskAvailabilityBag( kiosk ) );
        }

        /// <summary>
        /// Gets the family members the identified individual can check in.
        /// </summary>
        /// <remarks>
        /// The options bag arrives shaped by the imported check-in flow, which posts the same body the REST API
        /// expects. Its template, area, and family identifiers are ignored in favor of block settings and the
        /// identified individual; only the kiosk is read from it, and only after being re-validated against the
        /// Devices setting.
        /// </remarks>
        /// <param name="options">The options posted by the check-in flow.</param>
        /// <returns>A bag containing the attendees and the schedules available across them.</returns>
        [BlockAction]
        public BlockActionResult GetFamilyMembers( FamilyMembersOptionsBag options )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual?.PrimaryFamily == null )
            {
                return ActionUnauthorized();
            }

            var kiosk = GetAllowedKiosk( options?.KioskId );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            var configuration = GetConfiguredCheckInConfiguration()?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var familyIdKey = individual.PrimaryFamily.IdKey;
                var session = new CheckInDirector( RockContext ).CreateSession( configuration );

                session.LoadAndPrepareAttendeesForFamily( familyIdKey, GetConfiguredAreas(), kiosk, null );

                return ActionOk( new FamilyMembersResponseBag
                {
                    FamilyId = familyIdKey,
                    PossibleSchedules = session.GetAllPossibleScheduleBags(),
                    People = session.GetAttendeeBags()
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Gets the check-in opportunities available to a single family member.
        /// </summary>
        /// <remarks>
        /// The template, area, and family identifiers on the options bag are ignored in favor of block settings and
        /// the identified individual. The person identifier is honored, but only after it is confirmed to be
        /// someone this individual may check in.
        /// </remarks>
        /// <param name="options">The options posted by the check-in flow.</param>
        /// <returns>A bag containing the areas, groups, locations and schedules that member can pick from.</returns>
        [BlockAction]
        public BlockActionResult GetAttendeeOpportunities( AttendeeOpportunitiesOptionsBag options )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual?.PrimaryFamily == null )
            {
                return ActionUnauthorized();
            }

            var kiosk = GetAllowedKiosk( options?.KioskId );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            var configuration = GetConfiguredCheckInConfiguration()?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var familyIdKey = individual.PrimaryFamily.IdKey;
                var session = new CheckInDirector( RockContext ).CreateSession( configuration );

                // The person identifier on the options bag must be checked against the people this individual is
                // actually allowed to check in.
                if ( !IsCheckInAllowed( GetCheckInAllowedPersonIds( session, familyIdKey ), options.PersonId ) )
                {
                    return ActionForbidden( "Individual is not available for check-in." );
                }

                session.LoadAndPrepareAttendeesForPerson( options.PersonId, familyIdKey, GetConfiguredAreas(), kiosk, null );

                if ( !session.Attendees.Any() )
                {
                    return ActionBadRequest( "Individual was not found or is not available for check-in." );
                }

                return ActionOk( new AttendeeOpportunitiesResponseBag
                {
                    Opportunities = session.GetOpportunityCollectionBag( session.Attendees[0].Opportunities )
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Records attendance for the family members the individual selected.
        /// </summary>
        /// <remarks>
        /// The template and area identifiers on the options bag are ignored in favor of block settings. Every
        /// person identifier in the requests is confirmed to be someone this individual may check in before
        /// anything is saved. A save that is not pending completes the check-in, so labels the resolved device
        /// routes to the server print here.
        /// </remarks>
        /// <param name="options">The options posted by the check-in flow.</param>
        /// <returns>A bag containing the created attendance records and any messages raised while saving.</returns>
        [BlockAction]
        public async Task<BlockActionResult> SaveAttendance( SaveAttendanceOptionsBag options )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual?.PrimaryFamily == null )
            {
                return ActionUnauthorized();
            }

            if ( options?.Session == null )
            {
                return ActionBadRequest( "Check-in session was not provided." );
            }

            var requests = options.Requests ?? new List<AttendanceRequestBag>();
            var kiosk = GetAllowedKiosk( options.KioskId );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            // A family check-in saves once per attendee under a single session guid, so the guid recurring is
            // expected. A guid already carrying somebody else's records is not, and would attach this check-in to
            // their labels and audit trail.
            if ( !IsAttendanceSessionAllowed( options.Session.Guid, individual ) )
            {
                return ActionBadRequest( "Check-in session was not valid." );
            }

            var configuration = GetConfiguredCheckInConfiguration()?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                // The individuals identified on the options bag must be checked against the people this individual is
                // actually allowed to check in.
                var allowedPersonIds = GetCheckInAllowedPersonIds( session, individual.PrimaryFamily.IdKey );
                if ( requests.Any( r => !IsCheckInAllowed( allowedPersonIds, r.PersonId ) ) )
                {
                    return ActionForbidden( "Individual is not available for check-in." );
                }

                session.AttendanceSourceValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_MOBILE.AsGuid(), RockContext )?.Id;

                var sessionRequest = GetAttendanceSessionRequest( options.Session, individual );

                var result = session.SaveAttendance( sessionRequest, requests, kiosk, RequestContext.ClientInformation.IpAddress );

                // A save that is not pending completes the check-in, so labels the device routes to the server print
                // now. Client-routed labels are discarded because a phone cannot print them; print messages ride the
                // result into the response.
                if ( !sessionRequest.IsPending )
                {
                    var cts = new CancellationTokenSource( 5000 );
                    await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, kiosk, null, new LabelPrintProvider(), cts.Token );
                }

                return ActionOk( new SaveAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Confirms the attendance staged by a family check-in, turning the pending records into completed ones.
        /// </summary>
        /// <remarks>
        /// The template identifier on the options bag is ignored in favor of block settings. Labels the resolved
        /// device routes to the server print as part of confirming; client-routed labels print at a kiosk when the
        /// success QR code is scanned.
        /// </remarks>
        /// <param name="options">The options posted by the check-in flow.</param>
        /// <returns>A bag containing the confirmed attendance records.</returns>
        [BlockAction]
        public async Task<BlockActionResult> ConfirmAttendance( ConfirmAttendanceOptionsBag options )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual == null )
            {
                return ActionUnauthorized();
            }

            if ( options == null )
            {
                return ActionBadRequest( "Check-in session was not provided." );
            }

            var kiosk = GetAllowedKiosk( options.KioskId );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            if ( !IsAttendanceSessionAllowed( options.SessionGuid, individual ) )
            {
                return ActionBadRequest( "Check-in session was not valid." );
            }

            var configuration = GetConfiguredCheckInConfiguration()?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );
                var result = session.ConfirmAttendance( options.SessionGuid );

                // Confirming completes the check-in, so labels the device routes to the server print now.
                // Client-routed labels are discarded because a phone cannot print them; print messages ride the
                // result into the response.
                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, kiosk, null, new LabelPrintProvider(), cts.Token );

                return ActionOk( new ConfirmAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Discards the attendance a family check-in staged but never confirmed.
        /// </summary>
        /// <param name="sessionGuid">The unique identifier of the check-in session to discard.</param>
        /// <returns>An empty result when the records were discarded.</returns>
        [BlockAction]
        public BlockActionResult DeletePendingAttendance( Guid sessionGuid )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual == null )
            {
                return ActionUnauthorized();
            }

            if ( !IsAttendanceSessionAllowed( sessionGuid, individual ) )
            {
                return ActionBadRequest( "Check-in session was not valid." );
            }

            try
            {
                new CheckInDirector( RockContext ).DeletePendingAttendance( sessionGuid );

                return ActionOk();
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Records a completed check-in and gets the QR code image a kiosk scans to print the labels for it.
        /// </summary>
        /// <remarks>
        /// The session is remembered in a cookie whether or not a code is shown, so a repeat check-in produces one
        /// code covering every session, the code survives a page refresh, and turning codes on partway through an
        /// event still prints for everyone who checked in before the change.
        /// </remarks>
        /// <param name="sessionGuid">The unique identifier of the check-in session that was completed.</param>
        /// <returns>The url of the QR code image, or an empty string when there is no code to show.</returns>
        [BlockAction]
        public BlockActionResult RecordCompletedCheckIn( Guid sessionGuid )
        {
            var individual = GetIdentifiedIndividual();

            if ( individual == null )
            {
                return ActionUnauthorized();
            }

            var candidateSessionGuids = GetAttendanceSessionGuidsFromCookie();

            candidateSessionGuids.Add( sessionGuid );

            var sessionGuids = GetOwnedAttendanceSessionGuids( candidateSessionGuids, individual );

            RememberAttendanceSessionGuids( sessionGuids );

            return ActionOk( GetAttendanceSessionQrCodeUrl( sessionGuids ) );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the message explaining why check-in cannot proceed, when the block is not configured completely.
        /// </summary>
        /// <returns>The message to show in place of the check-in flow, or null when the configuration is complete.</returns>
        private string GetConfigurationErrorMessage()
        {
            if ( GetConfiguredCheckInConfiguration() == null || !GetConfiguredAreas().Any() )
            {
                return "Mobile check-in is not configured.";
            }

            var hasLoginPage = GetAttributeValue( AttributeKey.LoginPage ).IsNotNullOrWhiteSpace();
            var hasPhoneIdentificationPage = GetAttributeValue( AttributeKey.PhoneIdentificationPage ).IsNotNullOrWhiteSpace();

            if ( !hasLoginPage && !hasPhoneIdentificationPage )
            {
                return "A Login Page or Phone Identification Page must be specified.";
            }

            return null;
        }

        /// <summary>
        /// Gets the individual who is checking in, either from the current login or from the unsecured person
        /// identifier cookie left behind by a prior phone identification.
        /// </summary>
        /// <returns>The individual checking in, or <c>null</c> when they cannot be identified.</returns>
        private Person GetIdentifiedIndividual()
        {
            if ( RequestContext.CurrentPerson != null )
            {
                return RequestContext.CurrentPerson;
            }

            var personAliasGuid = RequestContext.GetCookieValue( Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER ).AsGuidOrNull();

            if ( !personAliasGuid.HasValue )
            {
                return null;
            }

            return new PersonAliasService( RockContext ).GetPerson( personAliasGuid.Value );
        }

        /// <summary>
        /// Gets the URLs the client navigates to from the block.
        /// </summary>
        /// <returns>The URLs keyed by the NavigationUrlKey constants.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.LoginPage] = GetIdentificationPageUrl( AttributeKey.LoginPage ),
                [NavigationUrlKey.PhoneIdentificationPage] = GetIdentificationPageUrl( AttributeKey.PhoneIdentificationPage )
            };
        }

        /// <summary>
        /// Gets the URL of one of the identification pages, with a return URL that brings the individual back to
        /// this page once they have been identified.
        /// </summary>
        /// <param name="attributeKey">The key of the linked page setting to build the URL from.</param>
        /// <returns>The URL of the identification page, or an empty string when the page is not configured.</returns>
        private string GetIdentificationPageUrl( string attributeKey )
        {
            if ( GetAttributeValue( attributeKey ).IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return this.GetLinkedPageUrl( attributeKey, new Dictionary<string, string>
            {
                [PageParameterKey.ReturnUrl] = GetReturnUrl()
            } );
        }

        /// <summary>
        /// Gets the URL of this page to return to after identification. Any impersonation token is left off so it
        /// is not handed to another page.
        /// </summary>
        /// <returns>The URL of the page this block is on.</returns>
        private string GetReturnUrl()
        {
            var parameters = RequestContext.GetPageParameters()
                .Where( p => !p.Key.Equals( "PageId", StringComparison.OrdinalIgnoreCase )
                    && !p.Key.Equals( PageParameterKey.ImpersonationToken, StringComparison.OrdinalIgnoreCase ) )
                .ToDictionary( p => p.Key, p => p.Value );

            return this.GetCurrentPageUrl( parameters, skipExistingParameters: true );
        }

        /// <summary>
        /// Gets the check-in sessions this browser has completed recently.
        /// </summary>
        /// <returns>The unique identifiers of the check-in sessions, which the client asserts and nothing backs.</returns>
        private List<Guid> GetAttendanceSessionGuidsFromCookie()
        {
            return RequestContext.GetCookieValue( CheckInCookieKey.AttendanceSessionGuids )
                .SplitDelimitedValues()
                .AsGuidList()
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Narrows a list of check-in sessions to the ones whose attendance this individual checked in, keeping the
        /// order they were given in.
        /// </summary>
        /// <remarks>
        /// A session that carries no attendance at all is dropped as well. Only the most recent attendance for a
        /// person holds a session, so an earlier session becomes empty once they check in again, and a code covering
        /// it would be denser for nothing.
        /// </remarks>
        /// <param name="sessionGuids">The unique identifiers of the check-in sessions to narrow.</param>
        /// <param name="individual">The individual whose sessions these must be.</param>
        /// <returns>The unique identifiers of the check-in sessions the individual may hand a kiosk.</returns>
        private List<Guid> GetOwnedAttendanceSessionGuids( List<Guid> sessionGuids, Person individual )
        {
            if ( sessionGuids?.Any() != true )
            {
                return new List<Guid>();
            }

            // Queried rather than read off the individual, whose alias collection may belong to another context, and
            // left unexecuted so this composes into a subquery.
            var aliasIds = new PersonAliasService( RockContext ).Queryable()
                .Where( pa => pa.PersonId == individual.Id )
                .Select( pa => pa.Id );

            var sessionAttendanceQuery = new AttendanceService( RockContext ).Queryable()
                .Where( a => sessionGuids.Contains( a.AttendanceCheckInSession.Guid ) );

            // A session holding anybody else's records is not this individual's to print, so the whole session is
            // withheld rather than just the records within it.
            var foreignSessionGuidQuery = sessionAttendanceQuery
                .Where( a => !a.CheckedInByPersonAliasId.HasValue
                    || !aliasIds.Contains( a.CheckedInByPersonAliasId.Value ) )
                .Select( a => a.AttendanceCheckInSession.Guid );

            var ownedSessionGuids = sessionAttendanceQuery
                .Where( a => !foreignSessionGuidQuery.Contains( a.AttendanceCheckInSession.Guid ) )
                .Select( a => a.AttendanceCheckInSession.Guid )
                .Distinct()
                .ToList();

            return sessionGuids.Where( g => ownedSessionGuids.Contains( g ) ).ToList();
        }

        /// <summary>
        /// Gets the URL of the QR code image encoding the specified check-in sessions.
        /// </summary>
        /// <param name="sessionGuids">The unique identifiers of the check-in sessions to encode.</param>
        /// <returns>The URL of the QR code image, or an empty string when there is no code to show.</returns>
        private string GetAttendanceSessionQrCodeUrl( List<Guid> sessionGuids )
        {
            // The single place the setting is honored, so sessions are still recorded while it is on and a code
            // appears for all of them the moment it is turned off.
            if ( GetAttributeValue( AttributeKey.DisableQRCode ).AsBoolean() )
            {
                return string.Empty;
            }

            if ( sessionGuids?.Any() != true )
            {
                return string.Empty;
            }

            // A next-gen kiosk reads this prefix and the shortened session identifiers to find the attendance whose
            // labels it should print.
            var shortSessionGuids = sessionGuids
                .Select( g => GuidHelper.ToShortString( g ) )
                .ToList()
                .AsDelimited( "," );

            var qrCodeData = Uri.EscapeDataString( $"PCL+{shortSessionGuids}" );

            return RequestContext.ResolveRockUrl( $"~/GetQRCode.ashx?data={qrCodeData}&outputType=svg" );
        }

        /// <summary>
        /// Records the check-in sessions the QR code covers so it can be rebuilt on a later page load.
        /// </summary>
        /// <param name="sessionGuids">The unique identifiers of the check-in sessions to remember.</param>
        private void RememberAttendanceSessionGuids( List<Guid> sessionGuids )
        {
            ResponseContext.AddCookie( new BrowserCookie
            {
                Name = CheckInCookieKey.AttendanceSessionGuids,
                Value = sessionGuids.AsDelimited( "," ),

                // Reset on every check-in, so the code stays available for as long as the individual is at the event
                // without outliving the day.
                Expires = RockDateTime.Now.AddHours( 8 )
            } );
        }

        /// <summary>
        /// Sets the campuses the individual can pick from, or the message explaining why none can be offered.
        /// </summary>
        /// <param name="box">The box the values are placed on.</param>
        /// <param name="individual">The individual checking in, whose campus is offered first.</param>
        /// <param name="mergeFields">The merge fields used to render any message.</param>
        private void SetCampusSelectionValues( MobileCheckInLauncherInitializationBox box, Person individual, Dictionary<string, object> mergeFields )
        {
            var configuredDeviceIds = GetConfiguredDeviceIds();

            // A campus is only offered when a kiosk serves it, so a blank device list offers nothing.
            if ( !configuredDeviceIds.Any() )
            {
                box.MessageHtml = GetAttributeValue( AttributeKey.NoDevicesFoundTemplate ).ResolveMergeFields( mergeFields );
                return;
            }

            var kioskDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), RockContext )?.Id;

            var campusKiosks = DeviceCache.All( RockContext )
                .Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId
                    && d.IsActive
                    && configuredDeviceIds.Contains( d.Id ) )
                .OrderBy( d => d.Id )
                .Select( d => new { Kiosk = d, CampusId = d.GetCampusId() } )
                .Where( d => d.CampusId.HasValue )
                .GroupBy( d => d.CampusId.Value )
                .Select( g => new
                {
                    Campus = CampusCache.Get( g.Key, RockContext ),
                    KioskIdKey = g.First().Kiosk.IdKey
                } )
                .Where( c => c.Campus != null )
                .ToList();

            if ( !campusKiosks.Any() )
            {
                box.MessageHtml = GetAttributeValue( AttributeKey.NoCampusesFoundTemplate ).ResolveMergeFields( mergeFields );
                return;
            }

            // The individual's own campus leads the list so it becomes the highlighted, most likely choice.
            box.CampusDeviceItems = campusKiosks
                .OrderByDescending( c => c.Campus.Id == individual.PrimaryCampusId )
                .ThenBy( c => c.Campus.Name )
                .Select( c => new ListItemBag { Value = c.KioskIdKey, Text = c.Campus.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the identifiers of the devices the Devices setting limits check-in to.
        /// </summary>
        /// <returns>A list of device identifiers, empty when the setting places no limit.</returns>
        private List<int> GetConfiguredDeviceIds()
        {
            return GetAttributeValue( AttributeKey.EnabledDevices )
                .SplitDelimitedValues()
                .AsIntegerList();
        }

        /// <summary>
        /// Records that the individual granted location permission so they are not prompted for it again.
        /// </summary>
        private void RememberLocationApproval()
        {
            ResponseContext.AddCookie( new BrowserCookie
            {
                Name = CheckInCookieKey.RockHasLocationApproval,
                Value = "true",
                Expires = RockDateTime.Now.AddYears( 1 )
            } );
        }

        /// <summary>
        /// Finds the kiosk whose geo-fence contains the specified coordinates.
        /// </summary>
        /// <param name="latitude">The latitude to match.</param>
        /// <param name="longitude">The longitude to match.</param>
        /// <returns>The matching kiosk, or <c>null</c> when the coordinates fall outside every allowed kiosk.</returns>
        private DeviceCache GetKioskByGeoFencing( double latitude, double longitude )
        {
            var kioskDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), RockContext ).Id;
            var configuredDeviceIds = GetConfiguredDeviceIds();

            try
            {
                var kioskQuery = new DeviceService( RockContext )
                    .GetDevicesByGeocode( latitude, longitude, kioskDeviceTypeValueId )
                    .Where( d => d.IsActive );

                if ( configuredDeviceIds.Any() )
                {
                    kioskQuery = kioskQuery.Where( d => configuredDeviceIds.Contains( d.Id ) );
                }

                // Overlapping geo-fences cannot say which kiosk is nearer, so the lowest identifier wins.
                var kioskId = kioskQuery
                    .OrderBy( d => d.Id )
                    .Select( d => d.Id )
                    .FirstOrDefault();

                return kioskId > 0 ? DeviceCache.Get( kioskId, RockContext ) : null;
            }
            catch ( Exception ex )
            {
                // One invalid geo-fence fails the whole comparison, so treat it as being out of range.
                ExceptionLogService.LogException( new Exception( "Error while trying to find matching geo-fenced kiosk. This is likely due to an invalid geo-fence on a kiosk.", ex ) );

                return null;
            }
        }

        /// <summary>
        /// Gets the kiosk named by the client, confirming it is still one this block offers.
        /// </summary>
        /// <param name="kioskIdKey">The hashed identifier of the kiosk to check.</param>
        /// <returns>The kiosk, or <c>null</c> when it is not one this block offers.</returns>
        private DeviceCache GetAllowedKiosk( string kioskIdKey )
        {
            var kiosk = DeviceCache.GetByIdKey( kioskIdKey, RockContext );

            if ( kiosk == null || !kiosk.IsActive )
            {
                return null;
            }

            var configuredDeviceIds = GetConfiguredDeviceIds();

            // A blank Devices setting places no limit, the same as it does for the geo-fence match.
            return !configuredDeviceIds.Any() || configuredDeviceIds.Contains( kiosk.Id )
                ? kiosk
                : null;
        }

        /// <summary>
        /// Gets the resolution to return to the client for the specified kiosk.
        /// </summary>
        /// <param name="kiosk">The kiosk that was matched, or <c>null</c> when nothing matched.</param>
        /// <returns>A bag containing the kiosk, or the message explaining why nothing matched.</returns>
        private KioskResolutionBag GetKioskResolutionBag( DeviceCache kiosk )
        {
            if ( kiosk == null )
            {
                return new KioskResolutionBag
                {
                    MessageHtml = GetMessageHtml( AttributeKey.NoDevicesFoundTemplate )
                };
            }

            return new KioskResolutionBag
            {
                Kiosk = new CheckInItemBag
                {
                    Id = kiosk.IdKey,
                    Name = kiosk.Name
                },
                Availability = GetKioskAvailabilityBag( kiosk )
            };
        }

        /// <summary>
        /// Gets whether check-in can start at the specified kiosk right now, with the message shown alongside it.
        /// </summary>
        /// <param name="kiosk">The kiosk the individual is checking in at.</param>
        /// <returns>A bag containing the availability and its message.</returns>
        private KioskAvailabilityBag GetKioskAvailabilityBag( DeviceCache kiosk )
        {
            var director = new CheckInDirector( RockContext );
            var status = director.GetKioskStatus( GetConfiguredAreas(), kiosk, null );

            var messageTemplateKey = status.IsCheckInActive
                ? AttributeKey.WelcomeBackTemplate
                : AttributeKey.NoServicesTemplate;

            return new KioskAvailabilityBag
            {
                IsCheckInAvailable = status.IsCheckInActive,
                MessageHtml = GetMessageHtml( messageTemplateKey, kiosk, status.NextStartDateTime ),
                Configuration = status.IsCheckInActive ? GetKioskConfigurationBag( director, kiosk ) : null
            };
        }

        /// <summary>
        /// Gets the kiosk, template and areas the check-in flow runs against.
        /// </summary>
        /// <param name="director">The check-in director used to build the template bag.</param>
        /// <param name="kiosk">The kiosk the individual is checking in at.</param>
        /// <returns>The configuration the check-in flow is constructed from.</returns>
        private KioskConfigurationBag GetKioskConfigurationBag( CheckInDirector director, DeviceCache kiosk )
        {
            var configurationTemplate = GetConfiguredCheckInConfiguration();
            var kioskBag = CheckInKioskSetup.GetKioskBag( kiosk );
            var templateBag = director.GetConfigurationTemplateBag( configurationTemplate );

            // Registering an individual, editing a family and removing a family member are staff actions taken at a
            // supervised kiosk. The check-in screens offer them whenever the device and template allow them, so they
            // are withheld here where one omission covers every screen that could present them.
            kioskBag.AllowAddingIndividualsToExistingFamilies = AdultsOrChildrenSelectionMode.None;
            kioskBag.IsEditingFamiliesEnabled = false;
            templateBag.IsRemoveFromFamilyAtKioskAllowed = false;

            return new KioskConfigurationBag
            {
                Kiosk = kioskBag,
                Template = templateBag,
                Areas = GetConfiguredAreas()
                    .Select( area => new CheckInItemBag
                    {
                        Id = area.IdKey,
                        Name = area.Name
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Gets whether the individual may write to the check-in session with the specified identifier. A guid that
        /// carries no attendance yet is unclaimed, and one that carries only this individual's records is theirs to
        /// keep adding to. An attendance with no recorded check-in person counts as somebody else's.
        /// </summary>
        /// <param name="sessionGuid">The unique identifier the check-in flow is working under.</param>
        /// <param name="individual">The individual performing the check-in.</param>
        /// <returns><c>true</c> when the session may be written to; otherwise <c>false</c>.</returns>
        private bool IsAttendanceSessionAllowed( Guid sessionGuid, Person individual )
        {
            // Queried rather than read off the individual, whose alias collection may belong to another context, and
            // left unexecuted so this composes into a subquery.
            var aliasIds = new PersonAliasService( RockContext ).Queryable()
                .Where( pa => pa.PersonId == individual.Id )
                .Select( pa => pa.Id );

            return !new AttendanceService( RockContext ).Queryable()
                .Any( a => a.AttendanceCheckInSession.Guid == sessionGuid
                    && ( !a.CheckedInByPersonAliasId.HasValue
                        || !aliasIds.Contains( a.CheckedInByPersonAliasId.Value ) ) );
        }

        /// <summary>
        /// Gets the identifiers of everyone the individual may check in. This is the engine's own definition, so
        /// it covers the immediate family plus anyone tied to it by a "can check in" known relationship.
        /// </summary>
        /// <param name="session">The check-in session used to resolve the family.</param>
        /// <param name="familyIdKey">The hashed identifier of the family being checked in.</param>
        /// <returns>The person identifiers that may appear in a check-in request.</returns>
        private HashSet<int> GetCheckInAllowedPersonIds( CheckInSession session, string familyIdKey )
        {
            return new HashSet<int>( session.GetGroupMembersQueryForFamily( familyIdKey )
                .Select( gm => gm.PersonId ) );
        }

        /// <summary>
        /// Gets whether a person named by the client is one the individual may check in.
        /// </summary>
        /// <param name="allowedPersonIds">The person identifiers the individual may check in.</param>
        /// <param name="personIdKey">The hashed identifier the client sent.</param>
        /// <returns><c>true</c> when the person may be checked in by this individual.</returns>
        private bool IsCheckInAllowed( HashSet<int> allowedPersonIds, string personIdKey )
        {
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var personId = IdHasher.Instance.GetId( personIdKey );

            return personId.HasValue && allowedPersonIds.Contains( personId.Value );
        }

        /// <summary>
        /// Builds the session request that attendance is saved under, replacing every value the client could
        /// otherwise use to change what the engine enforces or what lands on the attendance record.
        /// </summary>
        /// <param name="sessionBag">The session details posted by the check-in flow.</param>
        /// <param name="individual">The individual checking in.</param>
        /// <returns>The session request to save attendance with.</returns>
        private AttendanceSessionRequest GetAttendanceSessionRequest( AttendanceSessionRequestBag sessionBag, Person individual )
        {
            return new AttendanceSessionRequest( sessionBag )
            {
                PerformedByPersonId = individual.IdKey,
                FamilyId = individual.PrimaryFamily.IdKey,

                // The launcher never searches, so attendance records the same family lookup the legacy block did.
                SearchMode = FamilySearchMode.FamilyId,
                SearchTerm = null,

                // A family check-in stages each attendee as pending and confirms them together once the last one is
                // done, so this belongs to the flow. Room capacity is not the individual's to waive.
                IsPending = sessionBag.IsPending,
                IsCapacityThresholdEnforced = true
            };
        }

        /// <summary>
        /// Renders one of the message templates for the individual checking in.
        /// </summary>
        /// <param name="attributeKey">The key of the Lava template setting to render.</param>
        /// <returns>The rendered message.</returns>
        private string GetMessageHtml( string attributeKey )
        {
            var mergeFields = RequestContext.GetCommonMergeFields( GetIdentifiedIndividual() );

            return GetAttributeValue( attributeKey ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Renders one of the message templates for the individual checking in, with the kiosk and its next opening
        /// available to the template.
        /// </summary>
        /// <param name="attributeKey">The key of the Lava template setting to render.</param>
        /// <param name="kiosk">The kiosk the individual is checking in at.</param>
        /// <param name="nextActiveDateTime">When check-in next opens at the kiosk, or <c>null</c> when it does not
        /// open again today.</param>
        /// <returns>The rendered message.</returns>
        private string GetMessageHtml( string attributeKey, DeviceCache kiosk, DateTimeOffset? nextActiveDateTime )
        {
            var mergeFields = RequestContext.GetCommonMergeFields( GetIdentifiedIndividual() );

            // Templates reach the kiosk name through `Kiosk.Device.Name` and its campus through `Kiosk.CampusId`,
            // so both are offered alongside the device's own members. `KioskGroupTypes` has no equivalent here and
            // is not offered at all.
            var kioskInfo = new LavaDataObject
            {
                ["Device"] = kiosk,
                ["Name"] = kiosk.Name,
                ["CampusId"] = kiosk.GetCampusId()
            };

            mergeFields.Add( "Kiosk", kioskInfo );

            // The template gets the campus-local time, and DateTime.MaxValue stands in for check-in not opening
            // again today.
            mergeFields.Add( "NextActiveTime", nextActiveDateTime?.DateTime ?? DateTime.MaxValue );

            return GetAttributeValue( attributeKey ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the check-in themes available for selection.
        /// </summary>
        /// <remarks>
        /// The value is the theme's name, which is also its folder name under Themes, because that is what
        /// <see cref="SiteCache.Theme"/> validates and stores. Only themes whose purpose is check-in are offered;
        /// any other theme lacks the layout this page is bound to, and selecting one would leave the page resolving
        /// against a layout file that does not exist.
        /// </remarks>
        /// <returns>A list of items whose values are theme names.</returns>
        private List<ListItemBag> GetThemeItems()
        {
            var checkInPurposeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.THEME_PURPOSE_CHECKIN.AsGuid() );

            if ( !checkInPurposeValueId.HasValue )
            {
                return new List<ListItemBag>();
            }

            return new ThemeService( RockContext ).Queryable()
                .Where( t => t.IsActive && t.PurposeValueId == checkInPurposeValueId.Value )
                .OrderBy( t => t.Name )
                .Select( t => new ListItemBag
                {
                    Value = t.Name,
                    Text = t.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the url that reloads this page in the configured check-in theme, or an empty string when the page is
        /// already in it.
        /// </summary>
        /// <remarks>
        /// The theme is resolved from the site before any block runs, so it can only be changed by loading the page
        /// again. <see cref="SiteCache.Theme"/> honors a "theme" query string parameter, validates that the folder
        /// exists, and remembers it in a per-site cookie, so the parameter is needed only until that cookie is set.
        /// A theme it rejects is silently ignored, which would loop forever, so a request already carrying the
        /// parameter never asks again.
        /// </remarks>
        /// <returns>The url to load, or an empty string when no reload is needed.</returns>
        private string GetThemeRedirectUrl()
        {
            var siteCache = PageCache?.Layout?.Site;
            if ( siteCache == null )
            {
                return string.Empty;
            }

            // Get the block's configured theme, if any.
            var blockConfiguredTheme = GetAttributeValue( AttributeKey.CheckinTheme )?.Trim() ?? string.Empty;

            // If the block has no configured theme, fall back to the site's configured theme.
            var blockOrSiteConfiguredTheme = blockConfiguredTheme.IsNotNullOrWhiteSpace()
                ? blockConfiguredTheme
                : siteCache.ConfiguredTheme ?? string.Empty;

            // Simply reading the `siteCache.Theme` property does many things:
            //  1. Checks for a "theme" page parameter and validates it:
            //      a. If it names a theme that exists, a site-specific cookie is set to remember it for future
            //         requests, and that theme is returned.
            //      b. If it is an empty string, any preexisting site-specific cookie is cleared and the site's
            //         configured theme is returned.
            //      c. If it names a theme that does not exist, it is ignored entirely and evaluation continues at
            //         step 2, so a request can never be forced into a theme that is not installed.
            //  2. If no "theme" page parameter is present, or it was ignored by 1c, the property then checks for a
            //     site-specific cookie and validates it:
            //      a. If the cookie names a theme that exists, that theme is returned.
            //      b. If it names a theme that does not exist, the cookie is cleared and the site's configured theme
            //         is returned.
            //  3. If there was no usable "theme" page parameter and no cookie, the site's configured theme is returned.
            //
            // In all cases, the returned value is the theme this page is already rendered in, so when it matches
            // `blockOrSiteConfiguredTheme` there is no need to reload the page.
            var activePageTheme = siteCache.Theme;
            if ( blockOrSiteConfiguredTheme.Equals( activePageTheme, StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Empty;
            }

            // Reaching this point means the page is not in the theme it should be, so a reload is warranted unless
            // this request is already the result of a reload. Two things have to be checked to know that:
            //  1. Is a "theme" page parameter present at all? `PageParameter()` cannot answer this, because it returns
            //     an empty string both for a parameter that is missing and for one that is present but blank. Those
            //     two cases mean opposite things here: blank is the value that clears the cookie, so treating a
            //     missing parameter as blank would match on the very first request and the cookie would never be
            //     cleared.
            //  2. Does its value match what would be sent again? If it does, that reload has already been tried and
            //     left the page in some other theme, so repeating it would loop forever. For a configured theme that
            //     means the theme does not exist (see 1c above). If it does not match, someone named a different
            //     theme by hand in the url, and the configured one should replace it.
            var hasThemeParameter = RequestContext.GetPageParameters()
                .Any( p => p.Key.Equals( PageParameterKey.Theme, StringComparison.OrdinalIgnoreCase ) );

            if ( hasThemeParameter && PageParameter( PageParameterKey.Theme ).Equals( blockConfiguredTheme, StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Empty;
            }

            var parameters = RequestContext.GetPageParameters()
                .Where( p =>
                    !p.Key.Equals( "PageId", StringComparison.OrdinalIgnoreCase )
                    && !p.Key.Equals( PageParameterKey.ImpersonationToken, StringComparison.OrdinalIgnoreCase )
                )
                .ToDictionary( p => p.Key, p => p.Value );

            // Sent verbatim, because a blank value is what tells the site to forget the theme cookie rather than
            // replace it.
            parameters[PageParameterKey.Theme] = blockConfiguredTheme;

            return this.GetCurrentPageUrl( parameters, skipExistingParameters: true );
        }

        /// <summary>
        /// Gets the active check-in kiosk devices available for selection.
        /// </summary>
        /// <returns>A list of items whose values are device hashed identifiers.</returns>
        private List<ListItemBag> GetDeviceItems()
        {
            var kioskDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), RockContext )?.Id;

            return DeviceCache.All( RockContext )
                .Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId && d.IsActive )
                .OrderBy( d => d.Name )
                .Select( d => new ListItemBag { Value = d.IdKey, Text = d.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the check-in areas available for the specified devices. When no devices are specified the areas of
        /// every valid configuration are returned.
        /// </summary>
        /// <param name="director">The check-in director used to load the areas.</param>
        /// <param name="devices">The devices whose areas are included.</param>
        /// <returns>A list of items whose values are group type hashed identifiers.</returns>
        private List<ListItemBag> GetAreaItems( CheckInDirector director, List<DeviceCache> devices )
        {
            var areaSummaries = director.GetCheckInAreaSummaries( null, null );

            if ( areaSummaries == null )
            {
                return new List<ListItemBag>();
            }

            if ( devices.Any() )
            {
                var deviceAreaIdKeys = new HashSet<string>( devices
                    .SelectMany( d => director.GetKioskAreas( d ) )
                    .Select( gt => gt.IdKey ) );

                areaSummaries = areaSummaries
                    .Where( a => deviceAreaIdKeys.Contains( a.Id ) )
                    .ToList();
            }

            return areaSummaries
                .Select( a => new ListItemBag { Value = a.Id, Text = a.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the hashed identifiers of the devices currently stored in the Devices setting.
        /// </summary>
        /// <returns>A list of device hashed identifiers.</returns>
        private List<string> GetConfiguredDeviceIdKeys()
        {
            return GetConfiguredDeviceIds()
                .Select( id => DeviceCache.Get( id, RockContext )?.IdKey )
                .Where( idKey => idKey.IsNotNullOrWhiteSpace() )
                .ToList();
        }

        /// <summary>
        /// Gets the check-in configuration template the block is set to use.
        /// </summary>
        /// <returns>The configuration template, or <c>null</c> when the setting names one that no longer exists.</returns>
        private GroupTypeCache GetConfiguredCheckInConfiguration()
        {
            return GroupTypeCache.Get( GetAttributeValue( AttributeKey.CheckinConfiguration ).AsGuid(), RockContext );
        }

        /// <summary>
        /// Gets the check-in areas the block is set to use, which scope both the kiosk's status and the
        /// opportunities offered to the individual.
        /// </summary>
        /// <returns>The configured areas.</returns>
        private List<GroupTypeCache> GetConfiguredAreas()
        {
            return GetAttributeValue( AttributeKey.CheckinAreas )
                .SplitDelimitedValues()
                .AsIntegerList()
                .Select( id => GroupTypeCache.Get( id, RockContext ) )
                .Where( area => area != null )
                .ToList();
        }

        /// <summary>
        /// Gets the hashed identifier of the group type currently stored in the Check-in Configuration setting.
        /// </summary>
        /// <returns>The group type's hashed identifier, or an empty string when not configured.</returns>
        private string GetConfiguredCheckInConfigurationIdKey()
        {
            return GetConfiguredCheckInConfiguration()?.IdKey ?? string.Empty;
        }

        /// <summary>
        /// Gets the hashed identifiers of the areas currently stored in the Check-in Areas setting.
        /// </summary>
        /// <returns>A list of group type hashed identifiers.</returns>
        private List<string> GetConfiguredAreaIdKeys()
        {
            return GetConfiguredAreas()
                .Select( area => area.IdKey )
                .ToList();
        }

        /// <summary>
        /// Converts the selected device hashed identifiers to the comma-delimited list of device ids the Devices
        /// setting stores.
        /// </summary>
        /// <param name="deviceIdKeys">The selected device hashed identifiers.</param>
        /// <returns>The value to store in the Devices setting.</returns>
        private string GetDeviceIdListAttributeValue( List<string> deviceIdKeys )
        {
            return ( deviceIdKeys ?? new List<string>() )
                .Select( idKey => DeviceCache.GetByIdKey( idKey, RockContext )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .ToList()
                .AsDelimited( "," );
        }

        /// <summary>
        /// Converts the selected configuration template hashed identifier to the group type guid the Check-in
        /// Configuration setting stores.
        /// </summary>
        /// <param name="templateIdKey">The selected template's hashed identifier.</param>
        /// <returns>The value to store in the Check-in Configuration setting.</returns>
        private string GetCheckinConfigurationAttributeValue( string templateIdKey )
        {
            return GroupTypeCache.GetByIdKey( templateIdKey, RockContext )?.Guid.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Converts the selected area hashed identifiers to the comma-delimited list of group type ids the Check-in
        /// Areas setting stores.
        /// </summary>
        /// <param name="areaIdKeys">The selected area hashed identifiers.</param>
        /// <returns>The value to store in the Check-in Areas setting.</returns>
        private string GetConfiguredAreasAttributeValue( List<string> areaIdKeys )
        {
            return ( areaIdKeys ?? new List<string>() )
                .Select( idKey => GroupTypeCache.GetByIdKey( idKey, RockContext )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .ToList()
                .AsDelimited( "," );
        }

        #endregion Private Methods
    }
}
