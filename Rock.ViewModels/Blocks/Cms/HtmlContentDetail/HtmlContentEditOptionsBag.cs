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
    /// Block-derived options that control which features the Edit HTML modal
    /// shows and how the editor is configured.
    /// </summary>
    public class HtmlContentEditOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether previous versions are kept
        /// and the Version History tab and overwrite option are shown. This is
        /// true for every Versioning &amp; Approval mode other than Off.
        /// </summary>
        public bool IsVersioningEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Approval Status control
        /// is shown and content changes must be approved before they display.
        /// </summary>
        public bool IsApprovalRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person has the
        /// Approve security action on the block. When false the approval
        /// status is displayed read-only.
        /// </summary>
        public bool IsCurrentPersonApprover { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the editor opens in code
        /// view instead of the visual editor.
        /// </summary>
        public bool IsCodeEditorDefault { get; set; }

        /// <summary>
        /// Gets or sets the encrypted root folder used by the editor's document
        /// browser.
        /// </summary>
        public string EncryptedDocumentRootFolder { get; set; }

        /// <summary>
        /// Gets or sets the encrypted root folder used by the editor's image
        /// browser.
        /// </summary>
        public string EncryptedImageRootFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document and image root
        /// folders are scoped to the current person.
        /// </summary>
        public bool IsUserSpecificRoot { get; set; }

        /// <summary>
        /// Gets or sets the merge fields offered by the editor's merge field
        /// picker, in the "FieldName^EntityType|Label" format.
        /// </summary>
        public List<string> MergeFields { get; set; }
    }
}
