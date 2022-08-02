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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.LayoutDetail
{
    public class LayoutBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the user defined description of the Layout.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the file name portion of the associated .Net ASCX UserControl that provides the HTML Markup and code for this Layout.
        /// Value should not include the extension.  And the path is relative to the theme folder.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the layout mobile phone.
        /// </summary>
        public string LayoutMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the layout mobile tablet.
        /// </summary>
        public string LayoutMobileTablet { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the Layout.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Site that this Layout Block is associated with.
        /// </summary>
        public ListItemBag Site { get; set; }
    }
}
