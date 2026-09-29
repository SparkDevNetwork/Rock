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
    /// Settings that control when inactive people are automatically reactivated.
    /// </summary>
    public class ReactivatePeopleSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic reactivation is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last-contribution criteria is enabled.
        /// </summary>
        public bool IsLastContributionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the last-contribution criteria.
        /// </summary>
        public int? LastContributionPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attendance-in-service-group criteria is enabled.
        /// </summary>
        public bool IsAttendanceInServiceGroupEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the attendance-in-service-group criteria.
        /// </summary>
        public int? AttendanceInServiceGroupPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registered-in-any-event criteria is enabled.
        /// </summary>
        public bool IsRegisteredInAnyEventEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the registered-in-any-event criteria.
        /// </summary>
        public int? RegisteredInAnyEventPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attendance-in-group-type criteria is enabled.
        /// </summary>
        public bool IsAttendanceInGroupTypeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the group types considered for the attendance-in-group-type criteria.
        /// </summary>
        public List<string> AttendanceInGroupType { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the attendance-in-group-type criteria.
        /// </summary>
        public int? AttendanceInGroupTypeDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the site-login criteria is enabled.
        /// </summary>
        public bool IsSiteLoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the site-login criteria.
        /// </summary>
        public int? SiteLoginPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the prayer-request criteria is enabled.
        /// </summary>
        public bool IsPrayerRequestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the prayer-request criteria.
        /// </summary>
        public int? PrayerRequestPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person-attributes criteria is enabled.
        /// </summary>
        public bool IsPersonAttributesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the person attributes considered for the person-attributes criteria.
        /// </summary>
        public List<string> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the person-attributes criteria.
        /// </summary>
        public int? PersonAttributesDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the include-data-view criteria is enabled.
        /// </summary>
        public bool IsIncludeDataViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the data view a person must be in to be considered. The value is the data view unique identifier.
        /// </summary>
        public ListItemBag IncludeDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the exclude-data-view criteria is enabled.
        /// </summary>
        public bool IsExcludeDataViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the data view a person must not be in to be considered. The value is the data view unique identifier.
        /// </summary>
        public ListItemBag ExcludeDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the interactions criteria is enabled.
        /// </summary>
        public bool IsInteractionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the per-channel interaction criteria.
        /// </summary>
        public List<DataAutomationInteractionItemBag> Interactions { get; set; }
    }
}
