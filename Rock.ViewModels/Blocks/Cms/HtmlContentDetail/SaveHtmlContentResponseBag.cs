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

namespace Rock.ViewModels.Blocks.Cms.HtmlContentDetail
{
    /// <summary>
    /// Result of the Save block action of the HTML Content block.
    /// </summary>
    public class SaveHtmlContentResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content was written. This
        /// is false when markup warnings were returned and not yet acknowledged.
        /// </summary>
        public bool IsSaved { get; set; }

        /// <summary>
        /// Gets or sets the markup validation warnings found in the content.
        /// When non-empty and not yet acknowledged the client shows them and
        /// asks the person to save again to proceed.
        /// </summary>
        public List<string> MarkupWarnings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the save changed the content
        /// text and left it unapproved, so the new text will not be visible
        /// until it is approved. This is false when only the dates or the
        /// approval status changed.
        /// </summary>
        public bool IsApprovalPending { get; set; }
    }
}
