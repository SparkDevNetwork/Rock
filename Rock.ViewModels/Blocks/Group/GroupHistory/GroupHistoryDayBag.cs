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

namespace Rock.ViewModels.Blocks.Group.GroupHistory
{
    /// <summary>
    /// A single day on the Group History timeline along with the events that
    /// occurred on that day.
    /// </summary>
    public class GroupHistoryDayBag
    {
        /// <summary>
        /// Gets or sets the date the events occurred on. The time portion is
        /// not significant.
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Gets or sets the events that occurred on this day, ordered oldest
        /// event first.
        /// </summary>
        public List<GroupHistoryEventBag> Events { get; set; }
    }
}
