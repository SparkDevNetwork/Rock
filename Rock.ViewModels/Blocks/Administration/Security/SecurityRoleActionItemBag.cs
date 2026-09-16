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

namespace Rock.ViewModels.Blocks.Administration.Security
{
    /// <summary>
    /// An action that may be granted to a role being added, along with whether
    /// it should be selected by default in the Add Role form.
    /// </summary>
    public class SecurityRoleActionItemBag
    {
        /// <summary>
        /// Gets or sets the action key (e.g. "View", "Edit", "Administrate").
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the human friendly title displayed beside the checkbox.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action is selected by default.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
