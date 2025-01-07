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

namespace Rock.SystemKey
{
    /// <summary>
    ///
    /// </summary>
    public class GroupTypeAttributeKey
    {
        #region Registration Specific

        /// <summary>
        /// The GroupType attribute key for checkin registration CanCheckInKnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES = "core_checkin_registration_CanCheckInKnownRelationshipTypes";

        /// <summary>
        /// The GroupType attribute key for checkin registration SameFamilyKnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES = "core_checkin_registration_SameFamilyKnownRelationshipTypes";

        /// <summary>
        /// The GroupType attribute key for checkin registration KnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES = "core_checkin_registration_KnownRelationshipTypes";

        /// <summary>
        /// The GroupType attribute key for checkin registration EnableCheckInAfterRegistration
        /// </summary>
        public const string CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION = "core_checkin_registration_EnableCheckInAfterRegistration";

        /// <summary>
        /// The GroupType attribute key for checkin registration WorkflowTypes to launch when a new family is added
        /// </summary>
        public const string CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES = "core_checkin_registration_AddFamilyWorkflowTypes";

        /// <summary>
        /// The GroupType attribute key for checkin registration WorkflowTypes to launch when a new person is added
        /// </summary>
        public const string CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES = "core_checkin_registration_AddPersonWorkflowTypes";

        /// <summary>
        /// The GroupType attribute key for checkin registration OptionalAttributesforFamilies
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES = "core_checkin_registration_OptionalAttributesforFamilies";

        /// <summary>
        /// The GroupType attribute key for checkin registration RequiredAttributesforFamilies
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES = "core_checkin_registration_RequiredAttributesforFamilies";

        /// <summary>
        /// The GroupType attribute key for checkin registration OptionalAttributesforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN = "core_checkin_registration_OptionalAttributesforChildren";

        /// <summary>
        /// The GroupType attribute key for checkin registration RequiredAttributesforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN = "core_checkin_registration_RequiredAttributesforChildren";

        /// <summary>
        /// The GroupType attribute key for checkin registration OptionalAttributesforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS = "core_checkin_registration_OptionalAttributesforAdults";

        /// <summary>
        /// The GroupType attribute key for checkin registration RequiredAttributesforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS = "core_checkin_registration_RequiredAttributesforAdults";

        /// <summary>
        /// The GroupType attribute key for checkin registration DisplayAlternateIdFieldforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN = "core_checkin_registration_DisplayAlternateIdFieldforChildren";

        /// <summary>
        /// The GroupType attribute key for checkin registration DisplayAlternateIdFieldforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS = "core_checkin_registration_DisplayAlternateIdFieldforAdults";

        /// <summary>
        /// The GroupType attribute key for checkin registration Default Person Connection Status (DefinedValue.Guid)
        /// </summary>
        public const string CHECKIN_REGISTRATION_DEFAULTPERSONCONNECTIONSTATUS = "core_checkin_registration_DefaultPersonConnectionStatus";

        /// <summary>
        /// Set the phone number as SMS enabled by default
        /// </summary>
        public const string CHECKIN_REGISTRATION_DEFAULTSMSENABLED = "core_checkin_registration_DefaultSmsEnabled";

        /// <summary>
        /// Show or hide the control to set SMS Enabled
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYSMSBUTTON = "core_checkin_registration_DisplaySmsButton";

        /// <summary>
        /// Show or hide the control to set the Birthdate on Children
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONCHILDREN = "core_checkin_registration_DisplayBirthdateOnChildren";

        /// <summary>
        /// Show or hide the control to set the Birthdate on Adults
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYBIRTHDATEONADULTS = "core_checkin_registration_DisplayBirthdateOnAdults";

        /// <summary>
        /// Show or hide the control to set the Grade on Children
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYGRADEONCHILDREN = "core_checkin_registration_DisplayGradeOnChildren";

        /// <summary>
        /// Show or hide the control to set the race and ethnicity on Children
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN = "core_checkin_registration_DisplayRaceChildren";

        /// <summary>
        /// Show or hide the control to set the race and ethnicity on Children
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN = "core_checkin_registration_DisplayEthnicityChildren";

        /// <summary>
        /// Show or hide the control to set the race on adults
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYRACEONADULTS = "core_checkin_registration_DisplayRaceAdult";

        /// <summary>
        /// Show or hide the control to set the ethnicity on adults
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS = "core_checkin_registration_DisplayEthnicityAdult";

        #endregion Registration Specific

        #region Checkin

        /// <summary>
        /// Attribute key to indicate if the checkin GroupType should skip past the AbilityLevelSelect block.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ABILITY_LEVEL_DETERMINATION = "core_checkin_AbilityLevelDetermination";

        /// <summary>
        /// Attribute key for check-in configuration template to specify if
        /// certain age groups should be excluded from check-in.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_AGE_RESTRICTION = "core_AgeRestriction";

        /// <summary>
        /// Attribute key for GroupTypes that allow checkout
        /// </summary>
        [RockObsolete( "1.14" )]
        [Obsolete( "This value is no longer used. Use CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK and/or CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER instead. We'll leave this in in case a plugin is still using it but won't allow the old attribute to be edited." )]
        public const string CHECKIN_GROUPTYPE_ALLOW_CHECKOUT = "core_checkin_AllowCheckout";

        /// <summary>
        /// Attribute key for GroupTypes that allow checkout using the kiosks
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_KIOSK = "core_checkin_AllowCheckout_Kiosk";

        /// <summary>
        /// Attribute key for GroupTypes that allows checkout to be enabled in the check-in Manager 
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER = "core_checkin_AllowCheckout_Manager";

        /// <summary>
        /// Attribute key for GroupTypes that allows for a "can check-in"
        /// relationship to be removed on the person select screen during
        /// check-in at a kiosk.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK = "core_AllowRemoveFromFamily_Kiosk";

        /// <summary>
        /// Attribute key for GroupTypes that have presence enabled
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ENABLE_PRESENCE = "core_checkin_EnablePresence";

        /// <summary>
        ///  Attribute Key for Checkin Types for the list of Achievement Types to use for Checkin Celebrations
        ///  Stored as a list of <see cref="Rock.Model.AchievementType"/> Guids
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES = "core_checkin_AchievementTypes";

        /// <summary>
        /// Attribute key for check-in configuration template to specify the
        /// behavior of matching Grade and Age for groups.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_GRADE_AND_AGE_MATCHING_BEHAVIOR = "core_checkin_GradeAndAgeMatchingBehavior";

        /// <summary>
        /// Attribute key for grouptypes location selection strategy (Ask, Fill in order, balance).
        /// </summary>
        public const string CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY = "core_LocationSelectionStrategy";

        /// <summary>
        /// Attribute key for check-in configuration template to specify if
        /// groups marked as special needs should be removed from the opportunity
        /// list if the person is not also marked as special needs.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_REMOVE_SPECIAL_NEEDS_GROUPS = "core_RemoveSpecialNeedsGroups";

        /// <summary>
        /// Attribute key for check-in configuration template to specify if
        /// groups <strong>not</strong> marked as special needs should be removed
        /// from the opportunity list if the person <strong>is</strong> marked as
        /// special needs.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_REMOVE_NON_SPECIAL_NEEDS_GROUPS = "core_RemoveNonSpecialNeedsGroups";

        /// <summary>
        /// Attribute key for check-in configuraiton template to specify the
        /// content channel that will drive the promotions on the welcome
        /// screen. The raw value will be the unique identifier of the channel.
        /// </summary>
        public const string CHECKIN_GROUPTYPE_PROMOTIONS_CONTENT_CHANNEL = "core_PromotionsContentChannel";

        #endregion

        #region Display

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "AbilityLevel" block
        /// </summary>
        public const string CHECKIN_ABILITY_LEVEL_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_AbilityLevelSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "ActionSelect" block
        /// </summary>
        public const string CHECKIN_ACTION_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_ActionSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "PersonSelect" block
        /// </summary>
        public const string CHECKIN_CHECKOUT_PERSON_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_CheckoutPersonSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key for the checkin FamilyLavaTemplate
        /// </summary>
        public const string CHECKIN_FAMILYSELECT_LAVA_TEMPLATE = "core_checkin_FamilyLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "GroupSelect" block
        /// </summary>
        public const string CHECKIN_GROUP_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_GroupSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "GroupTypeSelect" block
        /// </summary>
        public const string CHECKIN_GROUP_TYPE_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_GroupTypeSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "LocationSelect" block
        /// </summary>
        public const string CHECKIN_LOCATION_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_LocationSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "MultiPersonSelect" block
        /// </summary>
        public const string CHECKIN_MULTI_PERSON_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_MultiPersonSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key for the checkin PersonSelect lava template.
        /// </summary>
        public const string CHECKIN_PERSON_SELECT_ADDITIONAL_INFORMATION_LAVA_TEMPLATE = "core_checkin_PersonSelectAdditionalInformationLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "PersonSelect" block
        /// </summary>
        public const string CHECKIN_PERSON_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_PersonSelectHeaderLavaTemplate";

        /// <summary>
        /// The GroupType attribute key for the checkin start lava template
        /// </summary>
        public const string CHECKIN_START_LAVA_TEMPLATE = "core_checkin_StartLavaTemplate";

        /// <summary>
        /// The <see cref="CHECKIN_SUCCESS_LAVA_TEMPLATE" /> display mode override. Possible values are:
        /// Never (default),
        /// Replace
        /// or Append.
        /// </summary>
        public const string CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE = "core_checkin_SuccessLavaTemplateOverrideDisplayMode";

        /// <summary>
        /// The GroupType attribute key for the checkin success lava template. By default, this is no longer used,
        /// and will be rendered based on CheckinCelebration and CheckinSuccess display logic. But this behavior
        /// can be overridden using the <see cref="CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE"/> setting.
        /// </summary>
        public const string CHECKIN_SUCCESS_LAVA_TEMPLATE = "core_checkin_SuccessLavaTemplate";

        /// <summary>
        /// The GroupType attribute key of the block header lava template for the checkin "TimeSelect" block
        /// </summary>
        public const string CHECKIN_TIME_SELECT_HEADER_LAVA_TEMPLATE = "core_checkin_TimeSelectHeaderLavaTemplate";

        #endregion Display

        /// <summary>
        /// Attribute key to filter the GroupTypes available for the defined values of the "Inactive Group Reasons" defined type
        /// </summary>
        public const string INACTIVE_REASONS_GROUPTYPE_FILTER = "core_InactiveReasonsGroupTypeFilter";
    }
}
