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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// Result model for a block type returned by the ListBlockTypes tool, also
/// nested inside <see cref="BlockResult"/> in summarized form.
/// </summary>
internal class BlockTypeResult : EntityResultBase
{
    /// <summary>
    /// The name of the block type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The category the block type is grouped under, e.g. <c>CMS</c>.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// What the block type does.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The framework the block type is built on: <c>Obsidian</c> for
    /// entity-based block types or <c>WebForms</c> for legacy path-based ones.
    /// Prefer Obsidian block types when both exist for a purpose.
    /// </summary>
    public string Platform { get; set; }
}
