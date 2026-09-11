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
    /// Payload for the Save block action of the HTML Content block.
    /// </summary>
    public class SaveHtmlContentRequestBag
    {
        /// <summary>
        /// Gets or sets the version number the editor was loaded from. The
        /// server uses this, together with the block and context it derives
        /// itself, to locate the row being edited.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the HTML or Lava content to save.
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
        /// Gets or sets the requested approval status. The server only honors
        /// this when the current person has the Approve security action.
        /// </summary>
        public HtmlContentApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the save replaces the loaded
        /// version instead of creating a new one. Only meaningful when
        /// versioning is enabled.
        /// </summary>
        public bool IsOverwriteCurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has already seen
        /// the markup validation warning and chosen to save anyway.
        /// </summary>
        public bool IsMarkupWarningAcknowledged { get; set; }
    }
}
