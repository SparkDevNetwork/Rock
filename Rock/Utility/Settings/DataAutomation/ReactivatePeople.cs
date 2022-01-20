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

namespace Rock.Utility.Settings.DataAutomation
{
    /// <summary>
    ///
    /// </summary>
    public class ReactivatePeople
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReactivatePeople"/> class.
        /// </summary>
        public ReactivatePeople()
        {
            IsLastContributionEnabled = true;
            LastContributionPeriod = 90;

            IsAttendanceInServiceGroupEnabled = true;
            AttendanceInServiceGroupPeriod = 90;

            IsRegisteredInAnyEventEnabled = true;
            RegisteredInAnyEventPeriod = 90;

            IsAttendanceInGroupTypeEnabled = true;
            AttendanceInGroupTypeDays = 90;

            IsSiteLoginEnabled = true;
            SiteLoginPeriod = 90;

            IsPrayerRequestEnabled = true;
            PrayerRequestPeriod = 90;

            IsPersonAttributesEnabled = true;
            PersonAttributesDays = 90;

            IsInteractionsEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is last contribution enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is last contribution enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsLastContributionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the last contribution period.
        /// </summary>
        /// <value>
        /// The last contribution period.
        /// </value>
        public int LastContributionPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is attendance in service group enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is attendance in service group enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttendanceInServiceGroupEnabled { get; set; }

        /// <summary>
        /// Gets or sets the attendance in service group period.
        /// </summary>
        /// <value>
        /// The attendance in service group period.
        /// </value>
        public int AttendanceInServiceGroupPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is registered in any event is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is registered in any event is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegisteredInAnyEventEnabled { get; set; }

        /// <summary>
        /// Gets or sets the period under which registeration for any event.
        /// </summary>
        /// <value>
        /// The period under which registered for any event.
        /// </value>
        public int RegisteredInAnyEventPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is attendance in group type enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is attendance in group type enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttendanceInGroupTypeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of the attendance in group.
        /// </summary>
        /// <value>
        /// The type of the attendance in group.
        /// </value>
        public List<int> AttendanceInGroupType { get; set; }

        /// <summary>
        /// Gets or sets the attendance in group type days.
        /// </summary>
        /// <value>
        /// The attendance in group type days.
        /// </value>
        public int AttendanceInGroupTypeDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is site log in enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is site log in enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSiteLoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets the sitelog in period.
        /// </summary>
        /// <value>
        /// The site login period.
        /// </value>
        public int SiteLoginPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is prayer request enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is prayer request enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrayerRequestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the prayer request period.
        /// </summary>
        /// <value>
        /// The prayer request period.
        /// </value>
        public int PrayerRequestPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is person attributes enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is person attributes enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonAttributesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the person attributes.
        /// </summary>
        /// <value>
        /// The person attributes.
        /// </value>
        public List<int> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the person attributes days.
        /// </summary>
        /// <value>
        /// The person attributes days.
        /// </value>
        public int PersonAttributesDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is include data view enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is include data view enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIncludeDataViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the include data view.
        /// </summary>
        /// <value>
        /// The include data view.
        /// </value>
        public int? IncludeDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is exclude data view enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is exclude data view enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsExcludeDataViewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the exclude data view.
        /// </summary>
        /// <value>
        /// The exclude data view.
        /// </value>
        public int? ExcludeDataView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is interactions enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is interactions enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteractionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>
        /// The interactions.
        /// </value>
        public List<InteractionItem> Interactions { get; set; }
    }
}
