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

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// Result model for a page removed by the DeletePage tool.
/// </summary>
internal class PageDeleteResult
{
    /// <summary>
    /// Whether the page was deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The internal name the deleted page had.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// How many blocks were removed along with the page.
    /// </summary>
    public int DeletedBlockCount { get; set; }
}
