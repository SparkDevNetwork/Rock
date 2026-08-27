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

namespace Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;

/// <summary>
/// Result model for the authored source of a Forge Content block placement.
/// </summary>
internal class ForgeContentResult
{
    /// <summary>
    /// The authored Vue single-file-component source.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The Vue version the stored compiled output was compiled against.
    /// </summary>
    public string CompiledVueVersion { get; set; }
}
