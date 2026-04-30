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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseDocumentList
{
    /// <summary>
    /// The additional options for the Knowledge Base Document List block.
    /// </summary>
    public class KnowledgeBaseDocumentListOptionsBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the parent folder, used in the
        /// panel title to give the admin context.
        /// </summary>
        public string FolderName { get; set; }
    }
}
