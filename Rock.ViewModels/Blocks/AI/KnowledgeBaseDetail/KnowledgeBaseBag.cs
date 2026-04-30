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

namespace Rock.ViewModels.Blocks.AI.KnowledgeBaseDetail
{
    /// <summary>
    /// The bag that contains the editable fields for a knowledge base.
    /// </summary>
    public class KnowledgeBaseBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name of the knowledge base.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the long-form description of the knowledge base.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the optional context that should be passed to retrieval
        /// or LLM prompts to describe what kind of content this knowledge base
        /// contains.
        /// </summary>
        public string ContextHint { get; set; }
    }
}
