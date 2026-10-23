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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The values used to move a registration to another registration instance.
    /// </summary>
    public class MoveRegistrationRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the registration instance to move to.
        /// </summary>
        public string RegistrationInstanceKey { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the group to move the registrants to, or
        /// null to leave group membership unchanged.
        /// </summary>
        public string GroupKey { get; set; }

        /// <summary>
        /// Gets or sets the comment added to the audit history.
        /// </summary>
        public string Comment { get; set; }
    }
}
