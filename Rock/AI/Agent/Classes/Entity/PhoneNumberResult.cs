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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Result model for a person's profile.
    /// </summary>
    internal class PhoneNumberResult : EntityResultBase
    {
        /// <summary>
        /// The phone number value.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The type of phone number (e.g., Mobile, Home, Work).
        /// </summary>
        public KeyNameResult PhoneType { get; set; }

        /// <summary>
        /// Indicates if SMS is enabled for this phone number.
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Indicates if this phone number is unlisted.
        /// </summary>
        public bool IsUnlisted { get; set; }
    }
}
