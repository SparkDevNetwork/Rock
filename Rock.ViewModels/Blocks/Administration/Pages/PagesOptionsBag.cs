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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.Pages
{
    /// <summary>
    /// The additional configuration options for the Pages block.
    /// </summary>
    public class PagesOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block (grid) should be shown.
        /// This is <c>false</c> when the current person is not authorized to configure the page.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the message to display when the block is not visible because
        /// the current person is not authorized to configure the page.
        /// </summary>
        public string AuthorizationMessage { get; set; }

        /// <summary>
        /// Gets or sets the layouts available for the page's site, used to populate the
        /// layout drop down list in the add/edit modal. Each value is a Layout's Guid.
        /// </summary>
        public List<ListItemBag> Layouts { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL of the parent of the page whose children are listed.
        /// When set, the modal renders a "parent page" link so the individual can navigate up
        /// one level. <c>null</c> when the listed page is a root page (or no page is specified).
        /// </summary>
        public string ParentPageUrl { get; set; }
    }
}
