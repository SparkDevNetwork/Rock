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

namespace Rock.AI.Agent
{
    /// <summary>
    /// The additional settings that are defined on a <see cref="Model.AISkill"/>.
    /// </summary>
    internal class SkillInstructionSettings
    {
        /// <summary>
        /// The ordered list of purpose strings for this skill.
        /// </summary>
        public List<string> Purposes { get; set; } = new List<string>();

        /// <summary>
        /// The ordered list of usage strings for this skill.
        /// </summary>
        public List<string> Usages { get; set; } = new List<string>();

        /// <summary>
        /// The ordered list of guardrail strings for this skill.
        /// </summary>
        public List<string> Guardrails { get; set; } = new List<string>();
    }
}
