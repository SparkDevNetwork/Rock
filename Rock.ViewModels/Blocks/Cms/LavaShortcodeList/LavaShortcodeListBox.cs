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

namespace Rock.ViewModels.Blocks.Cms.LavaShortcodeDetail
{
    /// <summary>
    /// The Lava Short Code Bag
    /// </summary>
    public class LavaShortcodeListBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of categories that any LavaShortcode might use.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the page to navigate to when editing a LavaShortcode.
        /// </summary>
        public string DetailPage {  get; set; }

        /// <summary>
        /// Gets or sets the collection of LavaShortcodes.
        /// </summary>
        public List<LavaShortcodeBag> LavaShortcodes { get; set; }

        /// <summary>
        /// Gets or sets whether the user is authorized to perform add or edit actions.
        /// </summary>
        public bool UserCanEdit { get; set; }
    }
}
