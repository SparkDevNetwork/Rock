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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderList
{
    /// <summary>
    /// The box that contains all the initialization information for the
    /// Knowledge Base Folder List block.
    /// </summary>
    public class KnowledgeBaseFolderListInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the friendly name of the parent knowledge base, used
        /// in the panel title to give the admin context.
        /// </summary>
        public string KnowledgeBaseName { get; set; }

        /// <summary>
        /// Gets or sets the list of folder summaries to display.
        /// </summary>
        public List<KnowledgeBaseFolderSummaryBag> FolderSummaries { get; set; }

        /// <summary>
        /// Gets or sets the supported source types that the admin can pick
        /// from when adding a new folder.
        /// </summary>
        public List<KnowledgeBaseFolderSourceTypeBag> SourceTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Add affordance should
        /// be shown to the current person.
        /// </summary>
        public bool IsAddEnabled { get; set; }
    }
}
