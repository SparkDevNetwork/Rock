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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderDetail
{
    /// <summary>
    /// The bag that contains the editable fields for a knowledge base folder.
    /// </summary>
    public class KnowledgeBaseFolderBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the long-form description of the folder.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets optional context the folder contributes at retrieval time.
        /// </summary>
        public string ContextHint { get; set; }

        /// <summary>
        /// Gets or sets the parent <see cref="Rock.Model.KnowledgeBase"/> reference.
        /// </summary>
        public ListItemBag KnowledgeBase { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> identifier that
        /// identifies the polymorphic kind of content provided by this folder.
        /// Null when the folder is "Manual" (no Rock entity is bound).
        /// </summary>
        public int? SourceEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the picked content channel when
        /// <see cref="SourceEntityTypeId"/> identifies a Content Channel source.
        /// The bag's <c>Value</c> holds the channel's IdKey, which is what is
        /// persisted to <see cref="Rock.Model.KnowledgeBaseFolder.SourceKey"/>.
        /// </summary>
        public ListItemBag SourceContentChannel { get; set; }
    }
}
