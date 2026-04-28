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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrationList
{
    /// <summary>
    /// Describes a single registrant shown inside the "Registrants" column of
    /// the Registration Instance Registration List grid.
    /// </summary>
    public class RegistrantInfoBag
    {
        /// <summary>
        /// Gets or sets the display name (nickname + last name) of the
        /// registrant.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this registrant is on the
        /// registration's wait list. Drives the "WL" warning label in the
        /// grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the registrant is on the wait list; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnWaitList { get; set; }
    }
}
