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

using Rock.Enums.Cms;

namespace Rock.ViewModels.Blocks.Cms.HtmlContentDetail
{
    /// <summary>
    /// A single version of HTML content as loaded into the editor.
    /// </summary>
    public class HtmlContentEditBag
    {
        /// <summary>
        /// Gets or sets the version number of this content. This is echoed back
        /// on save so the server can locate the row the editor started from.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the highest version number that exists for this block
        /// and context, or null when no content has been saved yet.
        /// </summary>
        public int? MaxVersion { get; set; }

        /// <summary>
        /// Gets or sets the raw HTML or Lava content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date the content becomes visible, or null
        /// for no lower bound.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date the content stops being visible, or
        /// null for no upper bound.
        /// </summary>
        public string ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the approval status derived from the IsApproved flag
        /// and approver fields.
        /// </summary>
        public HtmlContentApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who approved or denied this
        /// version, or null when it is pending.
        /// </summary>
        public string ApprovedByName { get; set; }

        /// <summary>
        /// Gets or sets the ISO 8601 date this version was approved or denied,
        /// or null when it is pending.
        /// </summary>
        public string ApprovedDateTime { get; set; }
    }
}
