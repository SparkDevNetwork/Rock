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

namespace Rock.AI.Agent.Classes.Skills.PageSkill;

/// <summary>
/// Result model for a page added by the AddPage tool.
/// </summary>
internal class AddPageResult : EntityResultBase
{
    /// <summary>
    /// The internal name of the new page.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The relative URL the new page is reachable at. This is the friendly
    /// route when one was created, otherwise the /page/id fallback.
    /// </summary>
    public string Url { get; set; }
}
