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

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// A single personalization segment in full detail.
/// </summary>
internal class PersonalizationSegmentDetailResult : EntityResultBase
{
    /// <summary>
    /// The friendly name of the segment.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The programmatic key of the segment.
    /// </summary>
    public string SegmentKey { get; set; }

    /// <summary>
    /// The description of the segment.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates that the segment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The data view that defines who is in the segment, when one is used.
    /// </summary>
    public KeyNameResult FilterDataView { get; set; }

    /// <summary>
    /// Indicates that the segment has an additional in-line filter beyond its data
    /// view. The filter itself is not returned; it is edited through Rock's
    /// personalization screens.
    /// </summary>
    public bool HasAdditionalFilter { get; set; }

    /// <summary>
    /// Indicates that the segment's membership is persisted on a schedule.
    /// </summary>
    public bool IsPersisted { get; set; }

    /// <summary>
    /// How often, in minutes, the persisted membership is refreshed, when the
    /// segment is persisted on an interval.
    /// </summary>
    public int? PersistedScheduleIntervalMinutes { get; set; }
}
