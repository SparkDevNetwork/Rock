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
/// Result model for a successful Custom Component compile and save.
/// </summary>
internal class ComponentSaveResult
{
    /// <summary>
    /// The IdKey of the block placement whose component was saved.
    /// </summary>
    public string BlockIdKey { get; set; }

    /// <summary>
    /// The Vue version the stored output was compiled against.
    /// </summary>
    public string CompiledVueVersion { get; set; }
}
