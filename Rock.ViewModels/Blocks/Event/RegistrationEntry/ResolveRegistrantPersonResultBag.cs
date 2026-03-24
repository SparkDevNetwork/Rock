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
    /// Represents the result of attempting to resolve person from registrant details within a registration process.
    /// </summary>
    public class ResolveRegistrantPersonResultBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resolved person.
        /// </summary>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resolved person is already associated with another registration
        /// for the same registration instance.
        /// This is used to determine if we need to show a warning about the person being registered more than once.
        /// </summary>
        public bool IsAlreadyInAnotherRegistration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is already included in the current registration.
        /// </summary>
        public bool IsAlreadyInCurrentRegistration { get; set; }
    }
}
