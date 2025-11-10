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

namespace Rock.ViewModels.Blocks.AI.AISkillDetail
{
    /// <summary>
    /// The item details for the AI Skill Detail block.
    /// </summary>
    public class AISkillBag : EntityBagBase
    {
        /// <summary>
        /// The entity type that represents the C# class that implements the
        /// tools for this skill. If this is not null then the skill and
        /// related tools should not allow editing beyond enabling or
        /// disabling them.
        /// </summary>
        public ListItemBag CodeEntityType { get; set; }

        /// <summary>
        /// The description of the skill, which provides additional context or
        /// information about its intended purpose and functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The instructions that provide context about when and how this
        /// skill should be used.
        /// </summary>
        public List<ListItemBag> Instructions { get; set; }

        /// <summary>
        /// The friendly name of the skill that will be used to identify it in
        /// the UI.
        /// </summary>
        public string Name { get; set; }
    }
}
