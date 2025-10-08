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

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a system (configured) phone number that can be used for communication actions
    /// such as sending SMS messages. Includes meta data about the number and (optionally) the
    /// person currently assigned to monitor or respond from this number.
    /// </summary>
    internal class SystemPhoneNumberResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the display name of the system phone number.
        /// </summary>
        /// <value>The friendly name of the number (e.g. "Main SMS Line").</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the system phone number.
        /// </summary>
        /// <value>A longer description to aid the AI or users in choosing an appropriate number.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the formatted phone number value.
        /// </summary>
        /// <value>The actual phone number (usually E.164 or a formatted local representation).</value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the person currently assigned to this number (if any).
        /// </summary>
        /// <value>A <see cref="PersonResult"/> describing the assigned individual or <c>null</c>.</value>
        public PersonResult AssignedToPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this number is enabled for SMS messaging.
        /// </summary>
        /// <value><c>true</c> if the number can send and receive SMS; otherwise, <c>false</c>.</value>
        public bool IsSmsEnabled { get; set; }
    }
}
