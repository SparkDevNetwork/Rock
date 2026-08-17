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

namespace Rock.AI.Agent.Classes.Skills.PageBuilderSkill;

/// <summary>
/// Lightweight result model for a page matched by the FindPages tool.
/// </summary>
internal class PageResult : EntityResultBase
{
    /// <summary>
    /// The internal (administrative) name of the page.
    /// </summary>
    public string InternalName { get; set; }

    /// <summary>
    /// The title shown to visitors when the page renders.
    /// </summary>
    public string PageTitle { get; set; }

    /// <summary>
    /// The name of the site the page belongs to, when known.
    /// </summary>
    public string SiteName { get; set; }
}
