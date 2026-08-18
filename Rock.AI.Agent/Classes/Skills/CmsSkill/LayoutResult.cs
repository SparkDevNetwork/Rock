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
/// Lightweight result model for a page layout, nested inside
/// <see cref="PageResult"/> and <see cref="BlockResult"/>.
/// </summary>
internal class LayoutResult : EntityResultBase
{
    /// <summary>
    /// The name of the layout.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of the site the layout belongs to.
    /// </summary>
    public string SiteName { get; set; }
}
