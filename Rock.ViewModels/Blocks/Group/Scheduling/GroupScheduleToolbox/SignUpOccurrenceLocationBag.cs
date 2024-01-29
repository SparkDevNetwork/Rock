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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about an additional time sign-up occurrence location for the group location schedule toolbox block.
    /// </summary>
    public class SignUpOccurrenceLocationBag
    {
        /// <summary>
        /// Gets or sets the occurrence location unique identifier.
        /// </summary>
        public Guid LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the occurrence location name.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the occurrence location order.
        /// </summary>
        public int LocationOrder { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity for this occurrence location.
        /// </summary>
        public int MaximumCapacity { get; set; }

        /// <summary>
        /// Gets or sets the count of people scheduled for this occurrence location.
        /// </summary>
        public int PeopleScheduledCount { get; set; }

        /// <summary>
        /// Gets or sets the count of people needed for this occurrence location.
        /// </summary>
        public int PeopleNeededCount { get; set; }

        /// <summary>
        /// Gets whether this location is at maximum capacity.
        /// </summary>
        public bool IsAtMaximumCapacity
        {
            get
            {
                // If there isn't a maximum capacity, this location will always allow sign-ups.
                if ( this.MaximumCapacity == 0 )
                {
                    return false;
                }

                return this.PeopleScheduledCount >= this.MaximumCapacity;
            }
        }
    }
}
