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

        #endregion Registration Specific

        #region Display

        /// <summary>
        /// The grouptype attribute key for the checkin start lava template
        /// </summary>
        public const string CHECKIN_START_LAVA_TEMPLATE = "core_checkin_StartLavaTemplate";

        /// <summary>
        /// The grouptype attribute key for the checkin familyselect lava template
        /// </summary>
        public const string CHECKIN_FAMILYSELECT_LAVA_TEMPLATE = "core_checkin_FamilyLavaTemplate";

        /// <summary>
        /// The grouptype attribute key for the checkin success lava template
        /// </summary>
        public const string CHECKIN_SUCCESS_LAVA_TEMPLATE = "core_checkin_SuccessLavaTemplate";

        #endregion Display
    }
}
