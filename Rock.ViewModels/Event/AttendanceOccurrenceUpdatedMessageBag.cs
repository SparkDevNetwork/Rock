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

namespace Rock.ViewModels.Event
{
    /// <summary>
    /// Details about an attendance occurrence record that is transmitted over
    /// the RealTime engine.
    /// </summary>
    public class AttendanceOccurrenceUpdatedMessageBag
    {
        /// <summary>
        /// Gets or sets th attendance occurrence unique identifier.
        /// </summary>
        /// <value>The occurrence unique identifier.</value>
        public Guid OccurrenceGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the location that was attended.
        /// </summary>
        /// <value>The unique identifier of the location that was attended.</value>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group the person attended.
        /// </summary>
        /// <value>The unique identifier of the group the person attended.</value>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the attendance occurrence type.
        /// </summary>
        /// <value>The unique identifier of the attendance occurrence type.</value>
        public Guid? AttendanceOccurrenceTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the attendance occurrence type.
        /// </summary>
        /// <value>The unique identifier of the attendance occurrence type.</value>
        public bool? DidNotOccur { get; set; }
    }
}
