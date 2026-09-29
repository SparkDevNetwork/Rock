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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Settings that control when active people are automatically inactivated.
    /// A person is inactivated when they fail to meet all of the enabled criteria.
    /// </summary>
    public class InactivatePeopleSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic inactivation is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the minimum age, in days, a record must be before it is considered for inactivation.
        /// </summary>
        public int? RecordsOlderThan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-last-contribution criteria is enabled.
        /// </summary>
        public bool IsNoLastContributionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the no-last-contribution criteria.
        /// </summary>
        public int? NoLastContributionPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-attendance-in-group-type criteria is enabled.
        /// </summary>
        public bool IsNoAttendanceInGroupTypeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the group types whose attendance is ignored by the no-attendance-in-group-type criteria.
        /// </summary>
        public List<string> AttendanceInGroupType { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the no-attendance-in-group-type criteria.
        /// </summary>
        public int? NoAttendanceInGroupTypeDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the not-registered-in-any-event criteria is enabled.
        /// </summary>
        public bool IsNotRegisteredInAnyEventEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the not-registered-in-any-event criteria.
        /// </summary>
        public int? NotRegisteredInAnyEventDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-site-login criteria is enabled.
        /// </summary>
        public bool IsNoSiteLoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the no-site-login criteria.
        /// </summary>
        public int? NoSiteLoginPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-prayer-request criteria is enabled.
        /// </summary>
        public bool IsNoPrayerRequestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the no-prayer-request criteria.
        /// </summary>
        public int? NoPrayerRequestPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-person-attributes criteria is enabled.
        /// </summary>
        public bool IsNoPersonAttributesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the person attributes ignored by the no-person-attributes criteria.
        /// </summary>
        public List<string> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the no-person-attributes criteria.
        /// </summary>
        public int? NoPersonAttributesDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the not-in-data-view criteria is enabled.
        /// </summary>
        public bool IsNotInDataViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the data view a person must not be in to be considered. The value is the data view unique identifier.
        /// </summary>
        public ListItemBag NotInDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the no-interactions criteria is enabled.
        /// </summary>
        public bool IsNoInteractionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the per-channel interaction criteria.
        /// </summary>
        public List<DataAutomationInteractionItemBag> NoInteractions { get; set; }
    }
}
