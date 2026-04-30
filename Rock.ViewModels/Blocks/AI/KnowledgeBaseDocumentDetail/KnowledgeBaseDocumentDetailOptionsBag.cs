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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseDocumentDetail
{
    /// <summary>
    /// The additional options for the Knowledge Base Document Detail block.
    /// Carries the parent folder's source-binding metadata so the edit panel
    /// can render the correct Source Key picker for the document.
    /// </summary>
    public class KnowledgeBaseDocumentDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a friendly display label for the parent folder's
        /// source kind (e.g., "Content Channel", "Manual"). Used as a
        /// read-only label near the conditional Source Key picker.
        /// </summary>
        public string SourceTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent folder's source
        /// kind is Content Channel. When <c>true</c>, the edit panel renders
        /// a <c>DropDownList</c> populated from <see cref="ContentChannelItemOptions"/>;
        /// when <c>false</c>, the panel falls back to a free-text input for
        /// Manual documents.
        /// </summary>
        public bool IsContentChannelSource { get; set; }

        /// <summary>
        /// Gets or sets the list of Content Channel Items belonging to the
        /// parent folder's bound channel. Each item's <c>Value</c> is the
        /// item's integer Id (as string), and <c>Text</c> is the item's
        /// Title. Populated only when <see cref="IsContentChannelSource"/>
        /// is <c>true</c>; otherwise <c>null</c>.
        ///
        /// Populated server-side rather than fetched lazily because the
        /// parent folder's channel binding is fixed at the document's
        /// create time (the folder's SourceEntityTypeId and SourceKey are
        /// read-only after creation), so the option set is stable for the
        /// lifetime of this block instance.
        /// </summary>
        public List<ListItemBag> ContentChannelItemOptions { get; set; }
    }
}
