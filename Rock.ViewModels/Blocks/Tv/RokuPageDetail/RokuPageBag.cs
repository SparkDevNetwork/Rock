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
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Tv.RokuPageDetail
{
    /// <summary>
    /// Describes the Roku Page
    /// </summary>
    public class RokuPageBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating when the Page should be displayed in the navigation.
        /// </summary>
        public bool ShowInMenu { get; set; }

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the description of the page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the scenegraph for the roku page.
        /// </summary>
        public string Scenegraph { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable page views].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
        public RockCacheabilityBag RockCacheability { get; set; }
    }
}
