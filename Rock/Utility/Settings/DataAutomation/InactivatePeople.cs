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
    /// Settings for controlling when people should be automatically inactivated
    /// </summary>
    public class InactivatePeople
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InactivatePeople"/> class.
        /// </summary>
        public InactivatePeople()
        {
            IsNoLastContributionEnabled = true;
            NoLastContributionPeriod = 500;

            IsNoAttendanceInGroupTypeEnabled = true;
            NoAttendanceInGroupTypeDays = 500;

            IsNotRegisteredInAnyEventEnabled = true;
            NotRegisteredInAnyEventDays = 500;

            IsNoSiteLoginEnabled = true;
            NoSiteLoginPeriod = 500;

            IsNoPrayerRequestEnabled = true;
            NoPrayerRequestPeriod = 500;

            IsNoPersonAttributesEnabled = true;
            NoPersonAttributesDays = 500;

            IsNoInteractionsEnabled = true;

            RecordsOlderThan = 180;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no last contribution enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no last contribution enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoLastContributionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the no last contribution period.
        /// </summary>
        /// <value>
        /// The no last contribution period.
        /// </value>
        public int NoLastContributionPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no attendance in group type enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no attendance in group type enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoAttendanceInGroupTypeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of the attendance in group.
        /// </summary>
        /// <value>
        /// The type of the attendance in group.
        /// </value>
        public List<int> AttendanceInGroupType { get; set; }

        /// <summary>
        /// Gets or sets the no attendance in group type days.
        /// </summary>
        /// <value>
        /// The no attendance in group type days.
        /// </value>
        public int NoAttendanceInGroupTypeDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled to check if not registered in any event.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled to check if not registered in any event; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotRegisteredInAnyEventEnabled { get; set; }

        /// <summary>
        /// Gets or sets the period in which there is no registration for any event.
        /// </summary>
        /// <value>
        /// The period in which there is no registration for any event.
        /// </value>
        public int NotRegisteredInAnyEventDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no site log in enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no site log in enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoSiteLoginEnabled { get; set; }

        /// <summary>
        /// Gets or sets the no site login period.
        /// </summary>
        /// <value>
        /// The no site login period.
        /// </value>
        public int NoSiteLoginPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no prayer request enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no prayer request enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoPrayerRequestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the no prayer request period.
        /// </summary>
        /// <value>
        /// The no prayer request period.
        /// </value>
        public int NoPrayerRequestPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no person attributes enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no person attributes enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoPersonAttributesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the person attributes.
        /// </summary>
        /// <value>
        /// The person attributes.
        /// </value>
        public List<int> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the no person attributes days.
        /// </summary>
        /// <value>
        /// The no person attributes days.
        /// </value>
        public int NoPersonAttributesDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not in dataview enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not in dataview enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotInDataviewEnabled { get; set; }

        /// <summary>
        /// Gets or sets the not in dataview.
        /// </summary>
        /// <value>
        /// The not in dataview.
        /// </value>
        public int? NotInDataview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is no interactions enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is no interactions enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoInteractionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the no interactions.
        /// </summary>
        /// <value>
        /// The no interactions.
        /// </value>
        public List<InteractionItem> NoInteractions { get; set; }

        /// <summary>
        /// Gets or sets the records older than.
        /// </summary>
        /// <value>
        /// The records older than.
        /// </value>
        public int RecordsOlderThan { get; set; }
    }

}
