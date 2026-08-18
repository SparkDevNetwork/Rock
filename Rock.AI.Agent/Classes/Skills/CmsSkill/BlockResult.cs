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
/// Result model for a block placement. The IdKey is the block id to pass to
/// the CustomComponent skill's AddOrUpdateCustomComponent tool.
/// </summary>
internal class BlockResult : EntityResultBase
{
    /// <summary>
    /// The administrative name of the block.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The zone on the page, layout, or site the block is placed in.
    /// </summary>
    public string Zone { get; set; }

    /// <summary>
    /// The position of the block within its zone.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Where the block is implemented: <c>Page</c>, <c>Layout</c>, or
    /// <c>Site</c>. Layout and site blocks render on every page that uses the
    /// layout or site.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// The block type of the placement, summarized.
    /// </summary>
    public BlockTypeResult BlockType { get; set; }

    /// <summary>
    /// The relative URL of the page the block is placed on, when the block is
    /// a page block.
    /// </summary>
    public string PageUrl { get; set; }
}
