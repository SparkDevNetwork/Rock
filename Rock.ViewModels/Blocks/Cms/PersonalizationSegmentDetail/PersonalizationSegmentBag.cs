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
using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PersonalizationSegmentDetail
{
    /// <summary>
    /// The item details for the Personalization Segment Detail block.
    /// </summary>
    public class PersonalizationSegmentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the additional filter configuration.
        /// </summary>
        public AdditionalFilterConfigurationBag AdditionalFilterConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the collection of Categories that this Rock.Model.PersonalizationSegment is associated with.
        /// NOTE: Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the description of the segment.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the filter data view.
        /// </summary>
        public ListItemBag FilterDataView { get; set; }

        /// <summary>
        /// Gets or sets the filter data view identifier.
        /// </summary>
        public int? FilterDataViewId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last refresh datetime for persistence.
        /// </summary>
        public DateTime? PersistedLastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last run duration in milliseconds for persistence.
        /// </summary>
        public int? PersistedLastRunDurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the persistence type (schedule or interval)
        /// </summary>
        public string PersistenceType { get; set; }

        /// <summary>
        /// Gets or sets the persistence schedule type (named or unique)
        /// </summary>
        public string PersistenceScheduleType { get; set; }

        /// <summary>
        /// Gets or sets the persisted schedule.
        /// </summary>
        public ListItemBag PersistedSchedule { get; set; }

        /// <summary>
        /// Gets or sets the iCalendar content for a unique schedule.
        /// </summary>
        public string UniqueScheduleICalendarContent { get; set; }

        /// <summary>
        /// Gets or sets the interval in minutes for persistence.
        /// </summary>
        public int? PersistedScheduleIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the segment key.
        /// </summary>
        public string SegmentKey { get; set; }

        /// <summary>
        /// Gets or sets the duration in milliseconds it takes to update the segment.
        /// </summary>
        public double? TimeToUpdateDurationMilliseconds { get; set; }
    }
}
