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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderList
{
    /// <summary>
    /// A bag that contains information about a single knowledge base folder
    /// for the Knowledge Base Folder List block's display card.
    /// </summary>
    public class KnowledgeBaseFolderSummaryBag : ITranslateIdKey
    {
        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the folder.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the friendly label for the folder's source type.
        /// "Manual" when the folder has no bound source entity type.
        /// </summary>
        public string SourceTypeName { get; set; }

        /// <summary>
        /// Gets or sets the count of documents contained in the folder.
        /// </summary>
        public int DocumentCount { get; set; }
    }
}
