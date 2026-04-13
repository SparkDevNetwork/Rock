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

namespace Rock.ViewModels.Blocks.Cms.HtmlContentApproval
{
    /// <summary>
    /// The additional configuration options for the HTML Content Approval block.
    /// </summary>
    public class HtmlContentApprovalOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current person is authorized
        /// to approve HTML content (via the block's "Approve" security action).
        /// When true, the approval toggle column is interactive and the "Approved By"
        /// filter is visible.
        /// </summary>
        public bool IsApproveVisible { get; set; }

        /// <summary>
        /// Gets or sets the list of sites available for filtering.
        /// </summary>
        public List<ListItemBag> Sites { get; set; }
    }
}
