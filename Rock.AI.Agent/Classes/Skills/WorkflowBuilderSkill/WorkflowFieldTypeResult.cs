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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// A reference to a field type, carrying its class name alongside the usual key
/// and name.
/// </summary>
/// <remarks>
/// The class name is here because a guessed field type class broke an import in an
/// early build and raised no error. Seeing the real one lets a caller recognize the
/// field type it is looking at rather than infer it from a display name. It is
/// output only; every tool that takes a field type takes its key.
/// </remarks>
internal class WorkflowFieldTypeResult : KeyNameResult
{
    /// <summary>
    /// The full class name of the field type, such as
    /// <c>Rock.Field.Types.SelectSingleFieldType</c>. Output only.
    /// </summary>
    public string Class { get; set; }
}
