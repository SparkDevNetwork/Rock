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

namespace Rock.AI.Agent.Classes.Skills.ReportingSkill;

/// <summary>
/// One record selected by a data view. Identity only; the record's fields are
/// read through that entity's own skill.
/// </summary>
internal class DataViewItemResult : EntityResultBase
{
    /// <summary>
    /// A display name for the record, from the entity's <c>ToString()</c>.
    /// </summary>
    public string Name { get; set; }
}
