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
/// A single Lava shortcode. The shortcode's markup, documentation, and parameter
/// definitions are large and are not carried here.
/// </summary>
internal class ShortcodeResult : EntityResultBase
{
    /// <summary>
    /// The friendly name of the shortcode.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The tag used to invoke the shortcode in Lava, such as <c>accordion</c> in
    /// <c>{[ accordion ]}</c>.
    /// </summary>
    public string TagName { get; set; }

    /// <summary>
    /// Whether the shortcode is an inline tag or a block tag with a closing tag.
    /// </summary>
    public string TagType { get; set; }

    /// <summary>
    /// The description of the shortcode.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates that the shortcode is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that the shortcode is part of Rock's core configuration and
    /// cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }
}
