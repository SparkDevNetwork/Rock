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

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// Represents a single group placement link displayed on the registration template view panel.
    /// </summary>
    public class RegistrationTemplateGroupPlacementBag
    {
        /// <summary>
        /// Gets or sets the placement name shown as the link text.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to navigate to when the placement link is clicked.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class configured on the placement, used as
        /// the leading icon of the link.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
