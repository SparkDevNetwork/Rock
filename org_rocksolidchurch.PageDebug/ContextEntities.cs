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

using System.ComponentModel;
using Rock.Attribute;
using Rock.Model;
using Rock.Obsidian.Blocks;

namespace org_rocksolidchurch.PageDebug
{
    /// <summary>
    /// Shows the page's context entities.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Context Entities" )]
    [Category( "Rock Solid Church > Page Debug" )]
    [Description( "Shows the page's context entities" )]
    [IconCssClass( "fa fa-pizza-slice" )]

    public class ContextEntities : PluginObsidianBlockType
    {
    }
}
