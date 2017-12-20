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
    /// System file types.  
    /// </summary>
    public class SystemSetting
    {
        /// <summary>
        /// Percent Confidence threshold for automatically setting gender based on a name
        /// </summary>
        public const string GENDER_AUTO_FILL_CONFIDENCE = "core_GenderAutoFillConfidence";

        /// <summary>
        /// The minimum distance in miles person/family has to have moved before automatically inactivatingn their record
        /// </summary>
        public const string NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE = "core_MinimumMoveDistanceToInactivate";

        /// <summary>
        /// Should a NCOA 48 month move request change a family's address to 'previous'
        /// </summary>
        public const string NCOA_SET_48_MONTH_AS_PREVIOUS = "core_Set48monthAsPrevious";

        /// <summary>
        /// Should a NCOA invalid address change a family's address to 'previous'
        /// </summary>
        public const string NCOA_SET_INVALID_AS_PREVIOUS = "core-SetinvalidAsAddress";

        /// <summary>
        /// Settings for how people/families should be reactivated
        /// </summary>
        public const string DATA_AUTOMATION_REACTIVATE_PEOPLE = "core_DataAutomationReactivatePeople";

        /// <summary>
        /// Settings for how people/families should be inactivated
        /// </summary>
        public const string DATA_AUTOMATION_INACTIVATE_PEOPLE = "core_DataAutomationInactivatePeople";

        /// <summary>
        /// Settings for if/when a family's campus should be updated
        /// </summary>
        public const string DATA_AUTOMATION_UPDATE_FAMILY_CAMPUS = "core_DataAutomationUpdateFamilyCampus";

        /// <summary>
        /// The font awesome pro key
        /// </summary>
        public const string FONT_AWESOME_PRO_KEY = "core_FontAwesomeProKey";

        /// <summary>
        /// Enable multi time zone support. Default is false
        /// </summary>
        public const string ENABLE_MULTI_TIME_ZONE_SUPPORT = "core_EnableMultiTimeZoneSupport";
    }
}