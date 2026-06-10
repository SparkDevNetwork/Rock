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
using System.Reflection;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using SuccessLavaTemplateDisplayMode = Rock.Enums.CheckIn.SuccessLavaTemplateDisplayMode;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Displays the settings for a check-in configuration.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType{TEntity, TEntityBag}" />

    [DisplayName( "Check-in Configuration Settings" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays the settings for a check-in configuration." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Schedule Builder Page",
        Key = AttributeKey.ScheduleBuilderPage,
        Description = "Page used to manage schedules for this check-in configuration.",
        Order = 0,
        IsRequired = false )]

    [BooleanField(
        "Show Classic Check-in Settings",
        Key = AttributeKey.ShowClassicCheckInSettings,
        Description = "Enabling this will show Classic Check-in Settings for this configuration. Note: Trailblazer Mode must be enabled.",
        DefaultBooleanValue = true,
        Order = 1,
        IsRequired = false )]

    #endregion Block Attributes

    [SystemGuid.EntityTypeGuid( "7d1dec32-3a94-45b4-b567-48d9478041b9" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "7ea2e093-2f33-4213-a33e-9e9a7a760181" )]
    [Rock.SystemGuid.BlockTypeGuid( "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6" )]
    public class CheckInConfigurationSettings : RockEntityDetailBlockType<GroupType, CheckInConfigurationSettingsBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowClassicCheckInSettings = "ShowClassicCheckInSettings";
            public const string ScheduleBuilderPage = "SchedulePage";
        }

        private static class PageParameterKey
        {
            public const string CheckInConfiguration = "CheckInConfiguration";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        private const string FRIENDLY_TYPE_NAME = "Check-in Configuration";

        #endregion Fields

        #region RockEntityDetailBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CheckInConfigurationSettingsBag, CheckInConfigurationSettingsOptionsBag>();

            SetBoxInitialEntityState( box );

            box.Options = GetBoxOptions();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <inheritdoc/>
        protected override CheckInConfigurationSettingsBag GetEntityBagForView( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );

            // ScheduledTimes is a display-only summary used by the view panel; it is not a settable attribute.
            // Mirroring the legacy block, we compute it here so it appears on view but not edit.
            bag.ScheduledTimes = GetScheduleTimes( entity );

            // SearchTypeFormatted and PhoneNumberCompare are display-only projections of their underlying
            // attributes (formatted human-readable text) used by the view panel. Mirroring the legacy block.
            if ( bag.SearchSettings != null && entity.AttributeValues != null )
            {
                if ( entity.AttributeValues.TryGetValue( "core_checkin_SearchType", out var searchTypeAttribute ) )
                {
                    bag.SearchSettings.SearchTypeFormatted = searchTypeAttribute.ValueFormatted;

                    var searchTypeGuid = searchTypeAttribute.Value.AsGuid();

                    if ( searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid() ) ||
                        searchTypeGuid.Equals( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() ) )
                    {
                        if ( entity.AttributeValues.TryGetValue( "core_checkin_PhoneSearchType", out var phoneSearchTypeAttribute ) )
                        {
                            bag.SearchSettings.PhoneNumberCompare = phoneSearchTypeAttribute.ValueFormatted;
                        }
                    }
                }
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override CheckInConfigurationSettingsBag GetEntityBagForEdit( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupType entity, ValidPropertiesBox<CheckInConfigurationSettingsBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // This pattern of nested bags and using ValidProperties as a flat list of the properties inside the nested
            // bag should not be followed. We will eventually come up with a pattern for how to handle this safely. The
            // concern is that two sub-bags might have the same property name which could lead to data loss during a
            // save operation from an older client. However, because this block is rather complex/specific, it would
            // not be a supported configuration to have a non-Obsidian implementation talk to the server so the risk
            // for this specific block is minimal.

            // While most of these settings are persisted as GroupType attributes, some are persisted on the
            // AdditionalSettings JSON blob. Go ahead and deserialize the AdditionalSettings into a strongly-typed
            // object so we can work with those settings more easily, as needed below.
            var checkInTemplateSettings = entity.GetAdditionalSettings<CheckInTemplateSettings>();

            // Basic Settings.
            box.IfValidProperty( nameof( box.Bag.BasicSettings.Name ),
                () => entity.Name = box.Bag.BasicSettings?.Name );

            box.IfValidProperty( nameof( box.Bag.BasicSettings.IconCssClass ),
                () => entity.IconCssClass = box.Bag.BasicSettings?.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.BasicSettings.Description ),
                () => entity.Description = box.Bag.BasicSettings?.Description );

            // Type Flow Settings.
            box.IfValidProperty( nameof( box.Bag.TypeFlowSettings.CheckInType ),
                () => entity.SetAttributeValue( "core_checkin_CheckInType", box.Bag.TypeFlowSettings?.CheckInType ) );

            box.IfValidProperty( nameof( box.Bag.TypeFlowSettings.AutoSelectOptions ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectOptions", ( int? ) box.Bag.TypeFlowSettings?.AutoSelectOptions ) );

            box.IfValidProperty( nameof( box.Bag.TypeFlowSettings.AutoSelectDaysBack ),
                () => entity.SetAttributeValue( "core_checkin_AutoSelectDaysBack", box.Bag.TypeFlowSettings?.AutoSelectDaysBack ) );

            box.IfValidProperty( nameof( box.Bag.TypeFlowSettings.UseSameOptions ),
                () => entity.SetAttributeValue( "core_checkin_UseSameOptions", ( box.Bag.TypeFlowSettings?.UseSameOptions ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.TypeFlowSettings.PreventDuplicateCheckin ),
                () => entity.SetAttributeValue( "core_checkin_PreventDuplicateCheckin", ( box.Bag.TypeFlowSettings?.PreventDuplicateCheckin ?? false ).ToString() ) );

            // Kiosk Features Settings.
            box.IfValidProperty( nameof( box.Bag.KioskFeaturesSettings.AllowCheckoutAtKiosk ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK, ( box.Bag.KioskFeaturesSettings?.AllowCheckoutAtKiosk ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.KioskFeaturesSettings.EnableRemoveFamilyKiosk ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK, ( box.Bag.KioskFeaturesSettings?.EnableRemoveFamilyKiosk ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.KioskFeaturesSettings.EnablePresence ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE, ( box.Bag.KioskFeaturesSettings?.EnablePresence ?? false ).ToString() ) );

            // Display Settings.
            box.IfValidProperty( nameof( box.Bag.DisplaySettings.PromotionsContentChannelGuid ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL, box.Bag.DisplaySettings?.PromotionsContentChannelGuid ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.HidePhotos ),
                () => entity.SetAttributeValue( "core_checkin_HidePhotos", ( box.Bag.DisplaySettings?.HidePhotos ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.DisplayLocationCount ),
                () => entity.SetAttributeValue( "core_checkin_DisplayLocationCount", ( box.Bag.DisplaySettings?.DisplayLocationCount ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.DisplaySettings.AchievementTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES, ( box.Bag.DisplaySettings?.AchievementTypes ?? new List<string>() ).AsDelimited( "," ) ) );

            // Supervision Settings.
            box.IfValidProperty( nameof( box.Bag.SupervisionSettings.EnableManager ),
                () => entity.SetAttributeValue( "core_checkin_EnableManagerOption", ( box.Bag.SupervisionSettings?.EnableManager ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SupervisionSettings.EnableOverride ),
                () => entity.SetAttributeValue( "core_checkin_EnableOverride", ( box.Bag.SupervisionSettings?.EnableOverride ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SupervisionSettings.AllowCheckoutInManager ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER, ( box.Bag.SupervisionSettings?.AllowCheckoutInManager ?? false ).ToString() ) );

            // Search Settings.
            box.IfValidProperty( nameof( box.Bag.SearchSettings.SearchType ),
                () => entity.SetAttributeValue( "core_checkin_SearchType", box.Bag.SearchSettings?.SearchType ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.MinPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MinimumPhoneSearchLength", box.Bag.SearchSettings?.MinPhoneLength ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.MaxPhoneLength ),
                () => entity.SetAttributeValue( "core_checkin_MaximumPhoneSearchLength", box.Bag.SearchSettings?.MaxPhoneLength ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.PhoneSearchType ),
                () => entity.SetAttributeValue( "core_checkin_PhoneSearchType", ( int? ) box.Bag.SearchSettings?.PhoneSearchType ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.MaxResults ),
                () => entity.SetAttributeValue( "core_checkin_MaxSearchResults", box.Bag.SearchSettings?.MaxResults ) );

            box.IfValidProperty( nameof( box.Bag.SearchSettings.SearchRegex ),
                () => entity.SetAttributeValue( "core_checkin_RegularExpressionFilter", box.Bag.SearchSettings?.SearchRegex ) );

            // Security Codes Settings.
            box.IfValidProperty( nameof( box.Bag.SecurityCodesSettings.CodeAlphaNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeLength", box.Bag.SecurityCodesSettings?.CodeAlphaNumericLength ) );

            box.IfValidProperty( nameof( box.Bag.SecurityCodesSettings.CodeAlphaLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeAlphaLength", box.Bag.SecurityCodesSettings?.CodeAlphaLength ) );

            box.IfValidProperty( nameof( box.Bag.SecurityCodesSettings.CodeNumericLength ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericLength", box.Bag.SecurityCodesSettings?.CodeNumericLength ) );

            box.IfValidProperty( nameof( box.Bag.SecurityCodesSettings.CodeRandom ),
                () => entity.SetAttributeValue( "core_checkin_SecurityCodeNumericRandom", ( box.Bag.SecurityCodesSettings?.CodeRandom ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SecurityCodesSettings.UseSameCodeForFamily ),
                () => entity.SetAttributeValue( "core_checkin_ReuseSameCode", ( box.Bag.SecurityCodesSettings?.UseSameCodeForFamily ?? false ).ToString() ) );

            // General Registration Settings.
            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.DefaultPersonConnectionStatus ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS, box.Bag.GeneralRegistrationSettings?.DefaultPersonConnectionStatus?.Value ) );

            // Default Person Record Source persists directly on GroupType.GroupMemberRecordSourceValueId (not as a
            // discrete attribute). Falls back to the "Check-in" record source when no explicit value is selected.
            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.DefaultPersonRecordSource ),
                () =>
                {
                    var selectedGuid = box.Bag.GeneralRegistrationSettings?.DefaultPersonRecordSource?.Value.AsGuidOrNull();
                    var selectedId = selectedGuid.HasValue ? DefinedValueCache.GetId( selectedGuid.Value ) : null;
                    entity.GroupMemberRecordSourceValueId = selectedId
                        ?? DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_CHECK_IN.AsGuid() );
                } );

            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.EnableCheckInAfterRegistration ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION, ( box.Bag.GeneralRegistrationSettings?.EnableCheckInAfterRegistration ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.DisplaySmsEnabled ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON, ( box.Bag.GeneralRegistrationSettings?.DisplaySmsEnabled ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.SmsEnabledByDefault ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED, ( box.Bag.GeneralRegistrationSettings?.SmsEnabledByDefault ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.GeneralRegistrationSettings.DisplaySuffix ),
                () => checkInTemplateSettings.DisplaySuffix = box.Bag.GeneralRegistrationSettings.DisplaySuffix );

            // Adult Registration Settings.
            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.RequiredAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS, ( box.Bag.AdultRegistrationSettings?.RequiredAttributesForAdults ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.OptionalAttributesForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS, ( box.Bag.AdultRegistrationSettings?.OptionalAttributesForAdults ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.DisplayBirthdateForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS, box.Bag.AdultRegistrationSettings?.DisplayBirthdateForAdults ) );

            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.DisplayRaceForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS, box.Bag.AdultRegistrationSettings?.DisplayRaceForAdults ) );

            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.DisplayEthnicityForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS, box.Bag.AdultRegistrationSettings?.DisplayEthnicityForAdults ) );

            box.IfValidProperty( nameof( box.Bag.AdultRegistrationSettings.DisplayAlternateIdForAdults ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS, ( box.Bag.AdultRegistrationSettings?.DisplayAlternateIdForAdults ?? false ).ToString() ) );

            // Child Registration Settings.
            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.RequiredAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN, ( box.Bag.ChildRegistrationSettings?.RequiredAttributesForChildren ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.OptionalAttributesForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN, ( box.Bag.ChildRegistrationSettings?.OptionalAttributesForChildren ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayBirthdateForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN, box.Bag.ChildRegistrationSettings?.DisplayBirthdateForChildren ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayGradeForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN, box.Bag.ChildRegistrationSettings?.DisplayGradeForChildren ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayRaceForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN, box.Bag.ChildRegistrationSettings?.DisplayRaceForChildren ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayEthnicityForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN, box.Bag.ChildRegistrationSettings?.DisplayEthnicityForChildren ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayAlternateIdForChildren ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN, ( box.Bag.ChildRegistrationSettings?.DisplayAlternateIdForChildren ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.DisplayMobilePhoneForChildren ),
                () => checkInTemplateSettings.DisplayMobilePhoneOnChildren = box.Bag.ChildRegistrationSettings.DisplayMobilePhoneForChildren );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.RequireRelationshipTypeSelectionForChildren ),
                () => checkInTemplateSettings.ForceSelectionOfKnownRelationshipType = box.Bag.ChildRegistrationSettings?.RequireRelationshipTypeSelectionForChildren ?? false );

            box.IfValidProperty( nameof( box.Bag.ChildRegistrationSettings.GradeConfirmationAge ),
                () => checkInTemplateSettings.GradeConfirmationAge = box.Bag.ChildRegistrationSettings?.GradeConfirmationAge );

            // Family Registration Settings.
            box.IfValidProperty( nameof( box.Bag.FamilyRegistrationSettings.DisplayAddressForFamilies ),
                () => checkInTemplateSettings.DisplayAddressOnFamilies = box.Bag.FamilyRegistrationSettings.DisplayAddressForFamilies );

            box.IfValidProperty( nameof( box.Bag.FamilyRegistrationSettings.RequiredAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES, ( box.Bag.FamilyRegistrationSettings?.RequiredAttributesForFamilies ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.FamilyRegistrationSettings.OptionalAttributesForFamilies ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES, ( box.Bag.FamilyRegistrationSettings?.OptionalAttributesForFamilies ?? new List<string>() ).AsDelimited( "," ) ) );

            // Child Relationship Settings.
            box.IfValidProperty( nameof( box.Bag.ChildRelationshipSettings.ChildRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES, ( box.Bag.ChildRelationshipSettings?.ChildRelationshipTypes ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.ChildRelationshipSettings.AddChildToParentsFamilyRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES, ( box.Bag.ChildRelationshipSettings?.AddChildToParentsFamilyRelationshipTypes ?? new List<string>() ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.ChildRelationshipSettings.AddChildToNewFamilyRelationshipTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES, ( box.Bag.ChildRelationshipSettings?.AddChildToNewFamilyRelationshipTypes ?? new List<string>() ).AsDelimited( "," ) ) );

            // Registration Workflow Settings. Stored as comma-delimited workflow type Guids.
            box.IfValidProperty( nameof( box.Bag.RegistrationWorkflowSettings.NewFamilyWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES, ( box.Bag.RegistrationWorkflowSettings?.NewFamilyWorkflowTypes ?? new List<ListItemBag>() ).ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationWorkflowSettings.NewPersonWorkflowTypes ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES, ( box.Bag.RegistrationWorkflowSettings?.NewPersonWorkflowTypes ?? new List<ListItemBag>() ).ConvertAll( wft => wft.Value ).AsDelimited( "," ) ) );

            // Additional Filters & Settings.
            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.AbilityLevelDetermination ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION, ( int? ) box.Bag.AdditionalFiltersAndSettings?.AbilityLevelDetermination ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.GradeAndAgeMatchingBehavior ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_GRADE_AND_AGE_MATCHING_BEHAVIOR, ( int? ) box.Bag.AdditionalFiltersAndSettings?.GradeAndAgeMatchingBehavior ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.AgeRestriction ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_AGE_RESTRICTION, ( int? ) box.Bag.AdditionalFiltersAndSettings?.AgeRestriction ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.EnableProximityCheckIn ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN, ( box.Bag.AdditionalFiltersAndSettings?.EnableProximityCheckIn ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.ProximityAttendanceNotificationTemplate ),
                () => checkInTemplateSettings.ProximityAttendanceNotificationTemplate = box.Bag.AdditionalFiltersAndSettings?.ProximityAttendanceNotificationTemplate );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.PreventInactivePeople ),
                () => entity.SetAttributeValue( "core_checkin_PreventInactivePeople", ( box.Bag.AdditionalFiltersAndSettings?.PreventInactivePeople ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.AgeRequired ),
                () => entity.SetAttributeValue( "core_checkin_AgeRequired", ( box.Bag.AdditionalFiltersAndSettings?.AgeRequired ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.AdditionalFiltersAndSettings.GradeRequired ),
                () => entity.SetAttributeValue( "core_checkin_GradeRequired", ( box.Bag.AdditionalFiltersAndSettings?.GradeRequired ?? false ).ToString() ) );

            // Special Needs Settings.
            box.IfValidProperty( nameof( box.Bag.SpecialNeedsSettings.HideSpecialNeedsGroups ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS, ( box.Bag.SpecialNeedsSettings?.HideSpecialNeedsGroups ?? false ).ToString() ) );

            box.IfValidProperty( nameof( box.Bag.SpecialNeedsSettings.HideNonSpecialNeedsGroups ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS, ( box.Bag.SpecialNeedsSettings?.HideNonSpecialNeedsGroups ?? false ).ToString() ) );

            // Classic Display Settings.
            box.IfValidProperty( nameof( box.Bag.ClassicDisplaySettings.RefreshInterval ),
                () => entity.SetAttributeValue( "core_checkin_RefreshInterval", box.Bag.ClassicDisplaySettings?.RefreshInterval ) );

            box.IfValidProperty( nameof( box.Bag.ClassicDisplaySettings.SuccessTemplateDisplayMode ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE, ( int? ) box.Bag.ClassicDisplaySettings?.SuccessTemplateDisplayMode ) );

            box.IfValidProperty( nameof( box.Bag.ClassicDisplaySettings.SuccessTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE, box.Bag.ClassicDisplaySettings?.SuccessTemplate ) );

            // Classic Templates Settings.
            box.IfValidProperty( nameof( box.Bag.ClassicTemplatesSettings.StartTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE, box.Bag.ClassicTemplatesSettings?.StartTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicTemplatesSettings.FamilySelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE, box.Bag.ClassicTemplatesSettings?.FamilySelectTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicTemplatesSettings.PersonSelectTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE, box.Bag.ClassicTemplatesSettings?.PersonSelectTemplate ) );

            // Classic Custom Header Text Settings.
            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.ActionSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.ActionSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.CheckoutPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.CheckoutPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.PersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.PersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.MultiPersonSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.MultiPersonSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.GroupTypeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.GroupTypeSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.TimeSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.TimeSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.AbilityLevelSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.AbilityLevelSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.LocationSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.LocationSelectHeaderTemplate ) );

            box.IfValidProperty( nameof( box.Bag.ClassicCustomHeaderTextSettings.GroupSelectHeaderTemplate ),
                () => entity.SetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE, box.Bag.ClassicCustomHeaderTextSettings?.GroupSelectHeaderTemplate ) );

            // Custom Attributes. The Custom Attributes stack on the client renders any GroupType attribute that
            // is not explicitly managed by this block (see BuildAttributeExcludeList). The stack is only visible
            // if at least one such attribute exists on this configuration's GroupType.
            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () => entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded ) );

            // Re-serialize the AdditionalSettings back onto the entity now that we've made any necessary updates to it.
            entity.SetAdditionalSettings( checkInTemplateSettings );

            return true;
        }

        /// <inheritdoc/>
        protected override GroupType GetInitialEntity()
        {
            var entity = GetInitialEntity<GroupType, GroupTypeService>( RockContext, PageParameterKey.CheckInConfiguration );

            if ( entity?.Id == 0 )
            {
                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                entity.GroupTypePurposeValueId = templatePurpose?.Id;
            }

            return entity;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out GroupType entity, out BlockActionResult error )
        {
            var entityService = new GroupTypeService( RockContext );
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
                entity = new GroupType();
                entityService.Add( entity );

                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                entity.GroupTypePurposeValueId = templatePurpose?.Id;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{FRIENDLY_TYPE_NAME} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {FRIENDLY_TYPE_NAME}." );
                return false;
            }

            return true;
        }

        #endregion RockEntityDetailBlockType Implementation

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

            return ActionOk( new ValidPropertiesBox<CheckInConfigurationSettingsBag>
            {
                Bag = bag,
                ValidProperties = GetValidProperties( bag )
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<CheckInConfigurationSettingsBag> box )
        {
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                if ( templatePurpose != null )
                {
                    entity.GroupTypePurposeValueId = templatePurpose.Id;
                }
            }

            entity.LoadAttributes( RockContext );

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateGroupType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            RefreshConnectedKiosks();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.CheckInConfiguration] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<CheckInConfigurationSettingsBag>
            {
                Bag = bag,
                ValidProperties = GetValidProperties( bag )
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
            var entityService = new GroupTypeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Detach the GroupType from any parent/child GroupType associations before delete so the
            // many-to-many self-reference rows in GroupTypeAssociation are removed cleanly. Mirrors the
            // legacy WebForms block.
            entity.ParentGroupTypes.Clear();
            entity.ChildGroupTypes.Clear();

            entityService.Delete( entity );
            RockContext.SaveChanges();

            RefreshConnectedKiosks();

            var pageRef = new Rock.Web.PageReference( PageCache.Id );
            var routeId = PageCache.PageRoutes.FirstOrDefault()?.Id;

            if ( routeId.HasValue )
            {
                pageRef.RouteId = routeId.Value;
            }

            return ActionOk( pageRef.BuildUrl() );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CheckInConfigurationSettingsBag, CheckInConfigurationSettingsOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FRIENDLY_TYPE_NAME} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( FRIENDLY_TYPE_NAME );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( FRIENDLY_TYPE_NAME );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CheckInConfigurationSettingsBag"/> that represents the entity.</returns>
        private CheckInConfigurationSettingsBag GetCommonEntityBag( GroupType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            // While most of these settings are persisted as GroupType attributes, some are persisted on the
            // AdditionalSettings JSON blob. Go ahead and deserialize the AdditionalSettings into a strongly-typed
            // object so we can work with those settings more easily, as needed below.
            var checkInTemplateSettings = entity.GetAdditionalSettings<CheckInTemplateSettings>();

            return new CheckInConfigurationSettingsBag
            {
                IdKey = entity.IdKey,
                BasicSettings = new CheckInBasicSettingsBag
                {
                    Name = entity.Name,
                    IconCssClass = entity.IconCssClass,
                    Description = entity.Description
                },
                TypeFlowSettings = new CheckInTypeFlowSettingsBag
                {
                    CheckInType = entity.GetAttributeValue( "core_checkin_CheckInType" ),
                    AutoSelectOptions = ( AutoSelectMode? ) entity.GetAttributeValue( "core_checkin_AutoSelectOptions" ).AsIntegerOrNull(),
                    AutoSelectDaysBack = entity.GetAttributeValue( "core_checkin_AutoSelectDaysBack" ).AsIntegerOrNull() ?? 10,
                    UseSameOptions = entity.GetAttributeValue( "core_checkin_UseSameOptions" ).AsBoolean(),
                    PreventDuplicateCheckin = entity.GetAttributeValue( "core_checkin_PreventDuplicateCheckin" ).AsBoolean( true )
                },
                KioskFeaturesSettings = new CheckInKioskFeaturesSettingsBag
                {
                    AllowCheckoutAtKiosk = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK ).AsBoolean(),
                    EnableRemoveFamilyKiosk = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK ).AsBoolean(),
                    EnablePresence = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean()
                },
                DisplaySettings = new CheckInDisplaySettingsBag
                {
                    PromotionsContentChannelGuid = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL ).AsGuidOrNull(),
                    HidePhotos = entity.GetAttributeValue( "core_checkin_HidePhotos" ).AsBoolean( true ),
                    DisplayLocationCount = entity.GetAttributeValue( "core_checkin_DisplayLocationCount" ).AsBoolean( true ),
                    AchievementTypes = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES ).SplitDelimitedValues().ToList()
                },
                SupervisionSettings = new CheckInSupervisionSettingsBag
                {
                    EnableManager = entity.GetAttributeValue( "core_checkin_EnableManagerOption" ).AsBoolean( true ),
                    EnableOverride = entity.GetAttributeValue( "core_checkin_EnableOverride" ).AsBoolean( true ),
                    AllowCheckoutInManager = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean()
                },
                SearchSettings = new CheckInSearchSettingsBag
                {
                    SearchType = entity.GetAttributeValue( "core_checkin_SearchType" ).AsGuidOrNull(),
                    MinPhoneLength = entity.GetAttributeValue( "core_checkin_MinimumPhoneSearchLength" ).AsIntegerOrNull() ?? 4,
                    MaxPhoneLength = entity.GetAttributeValue( "core_checkin_MaximumPhoneSearchLength" ).AsIntegerOrNull() ?? 10,
                    PhoneSearchType = ( PhoneSearchMode? ) entity.GetAttributeValue( "core_checkin_PhoneSearchType" ).AsIntegerOrNull(),
                    MaxResults = entity.GetAttributeValue( "core_checkin_MaxSearchResults" ).AsIntegerOrNull() ?? 100,
                    SearchRegex = entity.GetAttributeValue( "core_checkin_RegularExpressionFilter" )
                },
                SecurityCodesSettings = new CheckInSecurityCodesSettingsBag
                {
                    CodeAlphaNumericLength = entity.GetAttributeValue( "core_checkin_SecurityCodeLength" ).AsIntegerOrNull(),
                    CodeAlphaLength = entity.GetAttributeValue( "core_checkin_SecurityCodeAlphaLength" ).AsIntegerOrNull(),
                    CodeNumericLength = entity.GetAttributeValue( "core_checkin_SecurityCodeNumericLength" ).AsIntegerOrNull(),
                    CodeRandom = entity.GetAttributeValue( "core_checkin_SecurityCodeNumericRandom" ).AsBoolean( true ),
                    UseSameCodeForFamily = entity.GetAttributeValue( "core_checkin_ReuseSameCode" ).AsBoolean( false )
                },
                GeneralRegistrationSettings = new CheckInGeneralRegistrationSettingsBag
                {
                    DefaultPersonConnectionStatus = GetDefaultPersonConnectionStatusListItemBag( entity ),
                    DefaultPersonRecordSource = GetDefaultPersonRecordSourceListItemBag( entity ),
                    EnableCheckInAfterRegistration = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION ).AsBoolean(),
                    DisplaySmsEnabled = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON ).AsBoolean(),
                    SmsEnabledByDefault = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED ).AsBoolean(),
                    DisplaySuffix = checkInTemplateSettings.DisplaySuffix
                },
                AdultRegistrationSettings = new CheckInAdultRegistrationSettingsBag
                {
                    RequiredAttributesForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS ).SplitDelimitedValues().ToList(),
                    OptionalAttributesForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS ).SplitDelimitedValues().ToList(),
                    DisplayBirthdateForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS ),
                    DisplayRaceForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS ),
                    DisplayEthnicityForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS ),
                    DisplayAlternateIdForAdults = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS ).AsBoolean()
                },
                ChildRegistrationSettings = new CheckInChildRegistrationSettingsBag
                {
                    RequiredAttributesForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN ).SplitDelimitedValues().ToList(),
                    OptionalAttributesForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN ).SplitDelimitedValues().ToList(),
                    DisplayBirthdateForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN ),
                    DisplayGradeForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN ),
                    DisplayMobilePhoneForChildren = checkInTemplateSettings.DisplayMobilePhoneOnChildren,
                    DisplayRaceForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN ),
                    DisplayEthnicityForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN ),
                    DisplayAlternateIdForChildren = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN ).AsBoolean(),
                    RequireRelationshipTypeSelectionForChildren = checkInTemplateSettings.ForceSelectionOfKnownRelationshipType,
                    GradeConfirmationAge = checkInTemplateSettings.GradeConfirmationAge
                },
                FamilyRegistrationSettings = new CheckInFamilyRegistrationSettingsBag
                {
                    DisplayAddressForFamilies = checkInTemplateSettings.DisplayAddressOnFamilies,
                    RequiredAttributesForFamilies = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES ).SplitDelimitedValues().ToList(),
                    OptionalAttributesForFamilies = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES ).SplitDelimitedValues().ToList()
                },
                ChildRelationshipSettings = new CheckInChildRelationshipSettingsBag
                {
                    ChildRelationshipTypes = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList(),
                    AddChildToParentsFamilyRelationshipTypes = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList(),
                    AddChildToNewFamilyRelationshipTypes = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES ).SplitDelimitedValues().ToList()
                },
                RegistrationWorkflowSettings = GetRegistrationWorkflowSettings( entity ),
                AdditionalFiltersAndSettings = new CheckInAdditionalFiltersAndSettingsBag
                {
                    AbilityLevelDetermination = ( AbilityLevelDeterminationMode? ) entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION ).AsIntegerOrNull(),
                    GradeAndAgeMatchingBehavior = ( GradeAndAgeMatchingMode? ) entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_GRADE_AND_AGE_MATCHING_BEHAVIOR ).AsIntegerOrNull(),
                    AgeRestriction = ( AgeRestrictionMode? ) entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_AGE_RESTRICTION ).AsIntegerOrNull(),
                    EnableProximityCheckIn = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN ).AsBoolean(),
                    ProximityAttendanceNotificationTemplate = entity.GetAdditionalSettings<CheckInTemplateSettings>().ProximityAttendanceNotificationTemplate,
                    PreventInactivePeople = entity.GetAttributeValue( "core_checkin_PreventInactivePeople" ).AsBoolean( true ),
                    AgeRequired = entity.GetAttributeValue( "core_checkin_AgeRequired" ).AsBoolean( true ),
                    GradeRequired = entity.GetAttributeValue( "core_checkin_GradeRequired" ).AsBoolean( true )
                },
                SpecialNeedsSettings = new CheckInSpecialNeedsSettingsBag
                {
                    HideSpecialNeedsGroups = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS ).AsBoolean(),
                    HideNonSpecialNeedsGroups = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS ).AsBoolean()
                },
                ClassicDisplaySettings = new CheckInClassicDisplaySettingsBag
                {
                    RefreshInterval = entity.GetAttributeValue( "core_checkin_RefreshInterval" ).AsIntegerOrNull(),
                    SuccessTemplateDisplayMode = ( SuccessLavaTemplateDisplayMode? ) entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE ).AsIntegerOrNull(),
                    SuccessTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE )
                },
                ClassicTemplatesSettings = new CheckInClassicTemplatesSettingsBag
                {
                    StartTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE ),
                    FamilySelectTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE ),
                    PersonSelectTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE )
                },
                ClassicCustomHeaderTextSettings = new CheckInClassicCustomHeaderTextSettingsBag
                {
                    ActionSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE ),
                    CheckoutPersonSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                    PersonSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                    MultiPersonSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE ),
                    GroupTypeSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE ),
                    TimeSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE ),
                    AbilityLevelSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE ),
                    LocationSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE ),
                    GroupSelectHeaderTemplate = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE )
                }
            };
        }

        /// <summary>
        /// Builds the <see cref="CheckInRegistrationWorkflowSettingsBag"/>. The workflow type Guids stored on the
        /// GroupType attributes are resolved into <see cref="ListItemBag"/> entries (Text = workflow type name,
        /// Value = workflow type Guid) so the WorkflowTypePicker can display them.
        /// </summary>
        /// <param name="entity">The check-in configuration GroupType to inspect.</param>
        /// <returns>A populated <see cref="CheckInRegistrationWorkflowSettingsBag"/>.</returns>
        private CheckInRegistrationWorkflowSettingsBag GetRegistrationWorkflowSettings( GroupType entity )
        {
            var workflowTypeService = new WorkflowTypeService( RockContext );

            var newFamilyGuids = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList();
            var newPersonGuids = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES ).SplitDelimitedValues().AsGuidList();

            return new CheckInRegistrationWorkflowSettingsBag
            {
                NewFamilyWorkflowTypes = workflowTypeService.GetByGuids( newFamilyGuids ).ToListItemBagList(),
                NewPersonWorkflowTypes = workflowTypeService.GetByGuids( newPersonGuids ).ToListItemBagList()
            };
        }

        /// <summary>
        /// Builds a comma-delimited summary of the active service times associated with this check-in
        /// configuration's groups (and descendant areas). Used as a display-only field on the view panel.
        /// </summary>
        /// <param name="groupType">The check-in configuration GroupType to inspect.</param>
        /// <returns>A comma-delimited string of distinct, ordered active schedule names.</returns>
        private string GetScheduleTimes( GroupType groupType )
        {
            var descendantGroupTypeIds = new GroupTypeService( RockContext )
                .GetCheckinAreaDescendants( groupType.Id )
                .Select( a => a.Id );

            return new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.GroupType.Id == groupType.Id
                    || descendantGroupTypeIds.Contains( gl.Group.GroupTypeId )
                )
                .Where( gl =>
                    gl.Group.IsActive &&
                    !gl.Group.IsArchived
                )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsActive )
                .Select( s => s.Name )
                .Distinct()
                .OrderBy( s => s )
                .ToList()
                .AsDelimited( ", " );
        }

        /// <summary>
        /// Resolves the configured default person connection status defined value into a ListItemBag for
        /// the bag's <see cref="CheckInGeneralRegistrationSettingsBag.DefaultPersonConnectionStatus"/> field.
        /// </summary>
        /// <param name="entity">The check-in configuration GroupType to inspect.</param>
        /// <returns>A populated ListItemBag, or <c>null</c> if no value is configured or the value can no longer be resolved.</returns>
        private static ListItemBag GetDefaultPersonConnectionStatusListItemBag( GroupType entity )
        {
            var valueGuid = entity.GetAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS ).AsGuidOrNull();

            if ( !valueGuid.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( valueGuid.Value )?.ToListItemBag();
        }

        /// <summary>
        /// Resolves the configured default person record source defined value into a ListItemBag for the bag's
        /// <see cref="CheckInGeneralRegistrationSettingsBag.DefaultPersonRecordSource"/> field. Falls back to the
        /// "Check-in" record source defined value when the GroupType has no explicit value stored.
        /// </summary>
        /// <param name="entity">The check-in configuration GroupType to inspect.</param>
        /// <returns>A populated ListItemBag, or <c>null</c> if neither the stored value nor the fallback can be resolved.</returns>
        private static ListItemBag GetDefaultPersonRecordSourceListItemBag( GroupType entity )
        {
            var valueId = entity.GroupMemberRecordSourceValueId
                ?? DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_CHECK_IN.AsGuid() );

            if ( !valueId.HasValue )
            {
                return null;
            }

            return DefinedValueCache.Get( valueId.Value )?.ToListItemBag();
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CheckInConfigurationSettingsOptionsBag GetBoxOptions()
        {
            var options = new CheckInConfigurationSettingsOptionsBag
            {
                ShowClassicCheckInSettings = GetAttributeValue( AttributeKey.ShowClassicCheckInSettings ).AsBoolean(),
                PromotionsContentChannels = ContentChannelCache.All()
                    .Where( cc => cc.ContentChannelType.ShowInChannelList )
                    .OrderBy( cc => cc.Name )
                    .ToListItemBagList(),
                AchievementTypes = AchievementTypeCache.All()
                    .Where( at => at.IsActive )
                    .OrderBy( at => at.Name )
                    .ToListItemBagList(),
                SearchTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE.AsGuid() )?.DefinedValues
                    .Where( dv => dv.GetAttributeValue( "UserSelectable" ).AsBooleanOrNull() ?? true )
                    .ToListItemBagList(),
                PersonAttributes = GetPersonAttributeOptions(),
                FamilyAttributes = GetFamilyAttributeOptions(),
                RelationshipTypes = GetRelationshipTypeOptions(),
                ValidProperties = GetValidProperties( new CheckInConfigurationSettingsBag() )
            };

            return options;
        }

        /// <summary>
        /// Builds the list of selectable person attributes used by the required/optional adult and child
        /// attribute pickers in the registration sections.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> entries representing each available person attribute.</returns>
        private static List<ListItemBag> GetPersonAttributeOptions()
        {
            var fakePerson = new Person();
            fakePerson.LoadAttributes();

            return fakePerson.Attributes
                .Select( a =>
                    new ListItemBag
                    {
                        Text = a.Value.Name,
                        Value = a.Value.Guid.ToString()
                    }
                )
                .ToList();
        }

        /// <summary>
        /// Builds the list of selectable family (group) attributes used by the required/optional family
        /// attribute pickers in the Family Registration Fields stack.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> entries representing each available family attribute.</returns>
        private static List<ListItemBag> GetFamilyAttributeOptions()
        {
            var fakeFamily = new Model.Group { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id };
            fakeFamily.LoadAttributes();

            return fakeFamily.Attributes
                .Select( a =>
                    new ListItemBag
                    {
                        Text = a.Value.Name,
                        Value = a.Value.Guid.ToString()
                    }
                )
                .ToList();
        }

        /// <summary>
        /// Builds the list of selectable known-relationship roles used by the three pickers in the Child
        /// Relationship Settings stack.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> entries representing each available relationship role.</returns>
        private static List<ListItemBag> GetRelationshipTypeOptions()
        {
            var options = new List<ListItemBag>
            {
                new ListItemBag { Text = "Child", Value = "0" }
            };

            var knownRelationshipsGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            if ( knownRelationshipsGroupType != null )
            {
                foreach ( var role in knownRelationshipsGroupType.Roles.Where( r => r.Name != "Child" ) )
                {
                    options.Add( new ListItemBag { Text = role.Name, Value = role.Id.ToString() } );
                }
            }

            return options;
        }

        /// <summary>
        /// Determines whether a given GroupType attribute should be surfaced in the Custom Attributes stack.
        /// Returns true for any attribute whose key is NOT explicitly managed by a first-class field on this block.
        /// Used as the attribute filter for both the load and save paths so the stack and the underlying persisted
        /// values stay in sync.
        /// </summary>
        /// <param name="attribute">The attribute to evaluate.</param>
        /// <returns><c>true</c> if the attribute should appear in the Custom Attributes stack; otherwise <c>false</c>.</returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            return !BuildAttributeExcludeList().Contains( attribute.Key );
        }

        /// <summary>
        /// Builds the list of GroupType attribute keys that this block explicitly manages and should NOT
        /// surface as raw attributes in the Custom Attributes stack.
        /// </summary>
        /// <returns>A list of attribute keys to exclude from the Custom Attributes stack.</returns>
        private static List<string> BuildAttributeExcludeList()
        {
            return new List<string>
            {
                // Type Flow Settings.
                "core_checkin_CheckInType",
                "core_checkin_AutoSelectOptions",
                "core_checkin_AutoSelectDaysBack",
                "core_checkin_UseSameOptions",
                "core_checkin_PreventDuplicateCheckin",

                // Kiosk Features.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE,

                // Orphaned legacy GroupType attribute. A v14 migration (Rollup_0610) split this attribute into
                // CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK and CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER and
                // copied its values into both, but never deleted the original. Excluded here so the stale
                // entry does not surface in the Custom Attributes stack.
                "core_checkin_AllowCheckout",

                // Display.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL,
                "core_checkin_HidePhotos",
                "core_checkin_DisplayLocationCount",
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES,

                // Supervision.
                "core_checkin_EnableManagerOption",
                "core_checkin_EnableOverride",
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER,

                // Search & Security Codes.
                "core_checkin_SearchType",
                "core_checkin_PhoneSearchType",
                "core_checkin_MinimumPhoneSearchLength",
                "core_checkin_MaximumPhoneSearchLength",
                "core_checkin_MaxSearchResults",
                "core_checkin_RegularExpressionFilter",
                "core_checkin_SecurityCodeLength",
                "core_checkin_SecurityCodeAlphaLength",
                "core_checkin_SecurityCodeNumericLength",
                "core_checkin_SecurityCodeNumericRandom",
                "core_checkin_ReuseSameCode",
                "core_checkin_RefreshInterval",

                // General Registration.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYSMSBUTTON,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DEFAULTSMSENABLED,

                // Adult Registration.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS,

                // Children Registration.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN,

                // Family Registration.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES,

                // Child Relationship Settings.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES,

                // Registration Workflows.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES,

                // Additional Filters & Settings.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_GRADE_AND_AGE_MATCHING_BEHAVIOR,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_AGE_RESTRICTION,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PROXIMITY_CHECKIN,
                "core_checkin_PreventInactivePeople",
                "core_checkin_AgeRequired",
                "core_checkin_GradeRequired",

                // Special Needs.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS,

                // Classic Check-in Lava templates. These are managed by the Classic Check-in Settings section
                // (when enabled via the "Show Classic Check-in Settings" block setting). Excluded
                // unconditionally so they never appear as raw attributes.
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_START_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_FAMILYSELECT_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE,
                Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE
            };
        }

        /// <summary>
        /// Builds a flat list of leaf property names by recursively walking the nested sub-bags inside the supplied bag.
        /// The client uses this list to seed the ValidProperties on its edit box.
        /// </summary>
        /// <param name="bag">The bag whose property tree will be walked.</param>
        /// <returns>A flat list of leaf property names found inside the bag.</returns>
        private List<string> GetValidProperties( CheckInConfigurationSettingsBag bag )
        {
            var validProperties = new List<string>();

            var properties = bag.GetType().GetProperties();

            AddValidProperties( validProperties, properties );

            return validProperties;
        }

        /// <summary>
        /// Adds the supplied property names to <paramref name="validProperties"/>. When a property's type is a non-generic,
        /// non-string, non-ListItemBag class, its own properties are walked recursively so that nested sub-bag leaf names
        /// are included alongside the sub-bag name itself.
        /// </summary>
        /// <param name="validProperties">The running list of valid property names.</param>
        /// <param name="properties">The properties to be evaluated.</param>
        private static void AddValidProperties( List<string> validProperties, PropertyInfo[] properties )
        {
            foreach ( var propertyInfo in properties )
            {
                var propertyType = propertyInfo.PropertyType;
                if ( propertyType.IsClass && !propertyType.IsGenericType && propertyType != typeof( string ) && propertyType != typeof( ListItemBag ) )
                {
                    AddValidProperties( validProperties, propertyInfo.PropertyType.GetProperties() );
                }

                validProperties.Add( propertyInfo.Name );
            }
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

        /// <summary>
        /// Validates the GroupType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="groupType">The GroupType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the GroupType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateGroupType( GroupType groupType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Clears the kiosk device cache and pushes a refresh notification to all connected kiosks so configuration
        /// changes propagate without waiting for an app recycle.
        /// </summary>
        private void RefreshConnectedKiosks()
        {
#if NET472_OR_GREATER
            // Temporary until legacy check-in is removed.
            KioskDevice.Clear();
#endif

            // I know, this is a terrible hack. But we need to force the
            // kiosks to refresh and we don't want to make this public yet. -dsh
            typeof( GroupType ).Assembly.GetType( "Rock.CheckIn.v2.CheckInDirector" )
                ?.GetMethod( "SendRefreshKioskConfiguration" )
                ?.Invoke( null, new object[0] );
        }

        #endregion Private Methods
    }
}
