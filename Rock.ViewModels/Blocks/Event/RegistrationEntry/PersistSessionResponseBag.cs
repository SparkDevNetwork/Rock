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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Represents a response containing session persistence details for the Registration Entry block.
    /// </summary>
    public class PersistSessionResponseBag
    {
        /// <summary>
        /// Gets or sets the expiration date and time for the registration session.
        /// </summary>
        public DateTimeOffset ExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of spots remaining for the registration instance.
        /// </summary>
        public int? SpotsRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the timeout is disabled for the registration instance.
        /// </summary>
        public bool IsTimeoutDisabled { get; set; }
    }
}
