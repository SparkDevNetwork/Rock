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
namespace Rock.SystemKey
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupTypeAttributeKey
    {
        #region Registration Specific

        /// <summary>
        /// The grouptype attribute key for checkin registration CanCheckInKnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_CANCHECKINKNOWNRELATIONSHIPTYPES = "core_checkin_registration_CanCheckInKnownRelationshipTypes";

        /// <summary>
        /// The grouptype attribute key for checkin registration SameFamilyKnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_SAMEFAMILYKNOWNRELATIONSHIPTYPES = "core_checkin_registration_SameFamilyKnownRelationshipTypes";

        /// <summary>
        /// The grouptype attribute key for checkin registration KnownRelationshipTypes
        /// </summary>
        public const string CHECKIN_REGISTRATION_KNOWNRELATIONSHIPTYPES = "core_checkin_registration_KnownRelationshipTypes";

        /// <summary>
        /// The grouptype attribute key for checkin registration EnableCheckInAfterRegistration
        /// </summary>
        public const string CHECKIN_REGISTRATION_ENABLECHECKINAFTERREGISTRATION = "core_checkin_registration_EnableCheckInAfterRegistration";

        /// <summary>
        /// The grouptype attribute key for checkin registration WorkflowTypes to launch when a new family is added
        /// </summary>
        public const string CHECKIN_REGISTRATION_ADDFAMILYWORKFLOWTYPES = "core_checkin_registration_AddFamilyWorkflowTypes";

        /// <summary>
        /// The grouptype attribute key for checkin registration WorkflowTypes to launch when a new person is added
        /// </summary>
        public const string CHECKIN_REGISTRATION_ADDPERSONWORKFLOWTYPES = "core_checkin_registration_AddPersonWorkflowTypes";

        /// <summary>
        /// The grouptype attribute key for checkin registration OptionalAttributesforFamilies
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORFAMILIES = "core_checkin_registration_OptionalAttributesforFamilies";

        /// <summary>
        /// The grouptype attribute key for checkin registration RequiredAttributesforFamilies
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORFAMILIES = "core_checkin_registration_RequiredAttributesforFamilies";

        /// <summary>
        /// The grouptype attribute key for checkin registration OptionalAttributesforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORCHILDREN = "core_checkin_registration_OptionalAttributesforChildren";

        /// <summary>
        /// The grouptype attribute key for checkin registration RequiredAttributesforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORCHILDREN = "core_checkin_registration_RequiredAttributesforChildren";

        /// <summary>
        /// The grouptype attribute key for checkin registration OptionalAttributesforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_OPTIONALATTRIBUTESFORADULTS = "core_checkin_registration_OptionalAttributesforAdults";

        /// <summary>
        /// The grouptype attribute key for checkin registration RequiredAttributesforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_REQUIREDATTRIBUTESFORADULTS = "core_checkin_registration_RequiredAttributesforAdults";

        /// <summary>
        /// The grouptype attribute key for checkin registration DisplayAlternateIdFieldforChildren
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORCHILDREN = "core_checkin_registration_DisplayAlternateIdFieldforChildren";

        /// <summary>
        /// The grouptype attribute key for checkin registration DisplayAlternateIdFieldforAdults
        /// </summary>
        public const string CHECKIN_REGISTRATION_DISPLAYALTERNATEIDFIELDFORADULTS = "core_checkin_registration_DisplayAlternateIdFieldforAdults";

        /// <summary>
        /// The grouptype attribute key for checkin registration Default Person Connection Status (DefinedValue.Guid)
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

        #endregion Registration Specific

        #region Checkin

        /// <summary>
        /// Attribute key for GroupTypes that allow checkout
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ALLOW_CHECKOUT = "core_checkin_AllowCheckout";

        /// <summary>
        /// Attribute key for GroupTypes that have presence enabled
        /// </summary>
        public const string CHECKIN_GROUPTYPE_ENABLE_PRESENCE = "core_checkin_EnablePresence";

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
        /// The grouptype attribute key for the checkin familyselect lava template
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
        /// The grouptype attribute key for the checkin start lava template
        /// </summary>
        public const string CHECKIN_START_LAVA_TEMPLATE = "core_checkin_StartLavaTemplate";

        /// <summary>
        /// The grouptype attribute key for the checkin success lava template
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
