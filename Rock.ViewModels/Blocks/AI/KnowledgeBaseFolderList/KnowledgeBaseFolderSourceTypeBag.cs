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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderList
{
    /// <summary>
    /// A bag describing one supported source type that can be selected from
    /// the Add affordance on the Knowledge Base Folder List block.
    /// </summary>
    public class KnowledgeBaseFolderSourceTypeBag
    {
        /// <summary>
        /// Gets or sets the friendly label for the source type (for example,
        /// "Content Channel" or "Manual").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class displayed next to the source type's
        /// label in the Add menu.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> identifier for
        /// this source type. Null when the source type is "Manual" (no Rock
        /// entity is bound; documents are added by hand).
        /// </summary>
        public int? SourceEntityTypeId { get; set; }
    }
}
