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
using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PersistedDatasetDetail
{
    /// <summary>
    /// Class PersistedDatasetBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class PersistedDatasetBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the unique key to use to access this persisted dataset
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow manual refresh].
        /// </summary>
        public bool AllowManualRefresh { get; set; }

        /// <summary>
        /// Gets or sets the build script. See Rock.Model.PersistedDataset.BuildScriptType
        /// </summary>
        public string BuildScript { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the PersistedDataset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands
        /// </summary>
        public List<ListItemBag> EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// The DateTime when to stop updating the Rock.Model.PersistedDataset.ResultData
        /// </summary>
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is used by the system.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the persisted last refresh date time.
        /// </summary>
        public DateTime? LastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets the memory cache duration ms.
        /// </summary>
        public int? MemoryCacheDurationHours { get; set; }

        /// <summary>
        /// Gets or sets the Name of the PersistedDataset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval Hours
        /// </summary>
        public int? RefreshIntervalHours { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval
        /// </summary>
        public int? RefreshInterval { get; set; }

        /// <summary>
        /// Gets or sets the Schedule ID.
        /// </summary>
        public int? PersistedScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the persisted schedule interval type.
        /// </summary>
        public string PersistedScheduleIntervalType { get; set; }

        /// <summary>
        /// Gets or sets the persistence type.
        /// </summary>
        public string PersistenceType { get; set; }

        /// <summary>
        /// Gets or sets the persisted schedule type.
        /// </summary>
        public string PersistedScheduleType { get; set; }

        /// <summary>
        /// Gets or sets the schedule data.
        /// </summary>
        public string PersistedSchedule { get; set; }

        /// <summary>
        /// Gets or sets the named schedules.
        /// </summary>
        public List<ListItemBag> NamedSchedules { get; set; }

        /// <summary>
        ///  Gets or sets the readable iCalendar content 
        /// </summary>
        public string FriendlyScheduleText { get; set; }

        /// <summary>
        ///  Gets or sets the time to build in milliseconds
        /// </summary>
        public double? TimeToBuildMS { get; set; }

    }
}
