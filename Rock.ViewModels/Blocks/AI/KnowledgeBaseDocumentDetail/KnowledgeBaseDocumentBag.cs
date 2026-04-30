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

using System;

using Rock.Enums.AI;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseDocumentDetail
{
    /// <summary>
    /// The bag that contains the editable fields for a knowledge base document.
    /// </summary>
    public class KnowledgeBaseDocumentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the friendly name of the document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the source entity in Rock cached for
        /// display so the UI can show provenance without an extra join.
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets the Rock-side identifier of the source content within
        /// the parent folder's source type. For Manual documents this is an
        /// admin-entered free-text identifier; for Content Channel documents
        /// the bound value comes from <see cref="SourceContentChannelItem"/>
        /// and this field is unused.
        /// </summary>
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets the picked Content Channel Item when the parent
        /// folder is bound to the Content Channel source type. The Value is
        /// the item's Guid (as written by <c>ContentChannelItemPicker</c>)
        /// and the Text is the item's Title. The server resolves the Guid to
        /// the integer Id and writes it into <see cref="SourceKey"/> on save,
        /// and copies the Text into <see cref="SourceName"/> as a static
        /// display cache. Unused for Manual documents.
        /// </summary>
        public ListItemBag SourceContentChannelItem { get; set; }

        /// <summary>
        /// Gets or sets the indexing service's identifier for this document.
        /// Populated by the sync worker after the document is accepted.
        /// </summary>
        public string DocumentKey { get; set; }

        /// <summary>
        /// Gets or sets the optional source URL. May be the Rock-side detail
        /// page URL or an external link.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the optional binary file reference when the source
        /// content is a file (PDF, audio, etc.) rather than text.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the raw content sent to the indexing service.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle status of this document in the
        /// indexing service. Read-only in v1 (managed by the sync worker).
        /// </summary>
        public IndexStatus IndexStatus { get; set; }

        /// <summary>
        /// Gets or sets the date and time the document was last successfully
        /// indexed. Read-only in v1.
        /// </summary>
        public DateTime? IndexDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source content has
        /// changed since the last successful index. Read-only in v1.
        /// </summary>
        public bool IsIndexDirty { get; set; }

        /// <summary>
        /// Gets or sets the parent folder reference, used in the breadcrumb
        /// and view-mode display.
        /// </summary>
        public ListItemBag KnowledgeBaseFolder { get; set; }
    }
}
