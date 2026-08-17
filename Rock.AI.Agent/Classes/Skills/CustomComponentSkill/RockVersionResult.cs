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

namespace Rock.AI.Agent.Classes.Skills.CustomComponentSkill;

/// <summary>
/// Result model describing the Rock version this instance is running.
/// </summary>
internal class RockVersionResult
{
    /// <summary>
    /// The semantic version of this Rock instance, e.g. <c>1.20.0</c>. Pass this
    /// to release-scoped knowledge base lookups.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The full product version string of this Rock instance.
    /// </summary>
    public string FullVersion { get; set; }
}
