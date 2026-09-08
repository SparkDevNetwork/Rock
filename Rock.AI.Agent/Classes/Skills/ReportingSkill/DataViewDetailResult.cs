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
/// A single data view in full detail. The raw filter tree is not returned;
/// <see cref="FilterDescription"/> carries a human-readable summary instead.
/// </summary>
internal class DataViewDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the data view.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the data view.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The category the data view is filed under.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// The entity type the data view selects.
    /// </summary>
    public KeyNameResult EntityType { get; set; }

    /// <summary>
    /// Indicates that the data view's results are persisted on a schedule rather
    /// than evaluated on every run.
    /// </summary>
    public bool IsPersisted { get; set; }

    /// <summary>
    /// How often, in minutes, the persisted results are refreshed, when the data
    /// view is persisted on an interval.
    /// </summary>
    public int? PersistedScheduleIntervalMinutes { get; set; }

    /// <summary>
    /// The transformation applied to the data view's results, when one is
    /// configured.
    /// </summary>
    public KeyNameResult TransformEntityType { get; set; }

    /// <summary>
    /// Indicates that deceased people are included, for a person data view.
    /// </summary>
    public bool IncludeDeceased { get; set; }

    /// <summary>
    /// A human-readable summary of the data view's filters.
    /// </summary>
    public string FilterDescription { get; set; }
}
