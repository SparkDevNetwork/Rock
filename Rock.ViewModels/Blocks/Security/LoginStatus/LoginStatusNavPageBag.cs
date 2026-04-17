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

namespace Rock.ViewModels.Blocks.Security.LoginStatus
{
    /// <summary>
    /// Represents a single custom navigation page item in the Login Status dropdown.
    /// </summary>
    public class LoginStatusNavPageBag
    {
        /// <summary>
        /// Gets or sets the display title for the navigation item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the navigation item.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is a visual divider
        /// rather than a clickable link.
        /// </summary>
        public bool IsDivider { get; set; }
    }
}
