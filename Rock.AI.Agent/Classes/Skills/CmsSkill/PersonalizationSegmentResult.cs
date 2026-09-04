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

namespace Rock.AI.Agent.Classes.Skills.CmsSkill;

/// <summary>
/// A single personalization segment as it appears in a lookup. Identity only; the
/// configuration comes from the detail tool.
/// </summary>
internal class PersonalizationSegmentResult : EntityResultBase
{
    /// <summary>
    /// The friendly name of the segment.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The programmatic key of the segment, referenced from Lava and personalization.
    /// </summary>
    public string SegmentKey { get; set; }
}
