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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents a reference (anchor) to a specific entity instance within a chat or AI agent context.
    /// Used to provide contextual grounding for agent responses, such as linking to people, groups, or other entities.
    /// </summary>
    internal class ContextAnchor
    {
        /// <summary>
        /// Gets or sets the entity type identifier (e.g., Person, Group, etc.).
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity type (for example, "Person" or "Group").
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity instance.
        /// </summary>
        public string EntityKey { get; set; }

        /// <summary>
        /// Gets or sets a display name for this anchor, often the entity's name or description.
        /// </summary>
        public string Name { get; set; }
    }
}
