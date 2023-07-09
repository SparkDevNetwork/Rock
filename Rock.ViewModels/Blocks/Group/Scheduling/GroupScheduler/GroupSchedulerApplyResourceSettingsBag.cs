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
using Rock.Enums.Blocks.Group.Scheduling;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The resource settings to apply to user preferences for the group scheduler.
    /// </summary>
    public class GroupSchedulerApplyResourceSettingsBag
    {
        /// <summary>
        /// Gets or sets the group ID for this occurrence.
        /// </summary>
        /// <value>
        /// The group ID for this occurrence.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the resource list source type.
        /// </summary>
        /// <value>
        /// The resource list source type.
        /// </value>
        public ResourceListSourceType ResourceListSourceType { get; set; }

        /// <summary>
        /// Gets or sets the resource alternate group guid (if any).
        /// </summary>
        /// <value>
        /// The resource alternate group guid (if any).
        /// </value>
        public Guid? ResourceAlternateGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the resource data view guid (if any).
        /// </summary>
        /// <value>
        /// The resource data view guid (if any).
        /// </value>
        public Guid? ResourceDataViewGuid { get; set; }
    }
}
