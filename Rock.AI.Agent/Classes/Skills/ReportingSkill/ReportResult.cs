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
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.ReportingSkill;

/// <summary>
/// A single report as it appears in a list. Identity only; the definition and
/// fields come from the detail tool.
/// </summary>
internal class ReportResult : EntityResultBase
{
    /// <summary>
    /// The name of the report.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The category the report is filed under, or <c>null</c> when it is
    /// uncategorized.
    /// </summary>
    public KeyNameResult Category { get; set; }
}
