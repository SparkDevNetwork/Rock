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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.ReportingSkill;

/// <summary>
/// A single report in full detail, including its columns.
/// </summary>
internal class ReportDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the report.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the report.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The category the report is filed under.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// The entity type the report is built on.
    /// </summary>
    public KeyNameResult EntityType { get; set; }

    /// <summary>
    /// The data view that filters the report's records, when one is configured.
    /// </summary>
    public KeyNameResult DataView { get; set; }

    /// <summary>
    /// The maximum number of records the report returns, or <c>null</c> for all.
    /// </summary>
    public int? FetchTop { get; set; }

    /// <summary>
    /// The report's columns.
    /// </summary>
    public List<ReportFieldResult> Fields { get; set; }
}
