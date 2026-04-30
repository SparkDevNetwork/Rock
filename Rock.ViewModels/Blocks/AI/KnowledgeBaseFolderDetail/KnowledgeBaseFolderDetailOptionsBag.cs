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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderDetail
{
    /// <summary>
    /// The additional options the Knowledge Base Folder Detail block needs to
    /// present the entity for view or edit.
    /// </summary>
    public class KnowledgeBaseFolderDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the friendly label for the folder's source type
        /// ("Content Channel", "Manual", etc.). Set on first paint and not
        /// changed during the edit session because the source type is fixed
        /// after the folder's source kind is chosen on Add.
        /// </summary>
        public string SourceTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the folder's source type
        /// is a Content Channel and therefore the Content Channel picker
        /// should render.
        /// </summary>
        public bool IsContentChannelSource { get; set; }

        /// <summary>
        /// Gets or sets the active content channels available to bind a
        /// Content Channel folder to. Populated only when the folder's
        /// source type is a Content Channel.
        /// </summary>
        public List<ListItemBag> ContentChannelOptions { get; set; }
    }
}
