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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance List block.
    /// </summary>
    public class RegistrationInstanceListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user can view the block.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user can view the block; otherwise, <c>false</c>.
        /// </value>
        public bool CanView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether wait list is enabled for the Registration Template.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wait list is enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool WaitListEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the registration template.
        /// </summary>
        /// <value>
        /// The name of the template.
        /// </value>
        public string TemplateName { get; set; }
    }
}
