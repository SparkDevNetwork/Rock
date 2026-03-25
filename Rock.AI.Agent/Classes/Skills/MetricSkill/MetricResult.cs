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

namespace Rock.AI.Agent.Classes.Skills.MetricSkill;

/// <summary>
/// A single metric record.
/// </summary>
internal class MetricResult : EntityResultBase
{
    /// <summary>
    /// The title of the metric.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The description of the metric that should include details on the purpose.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The person in charge of this metric.
    /// </summary>
    public PersonResult ChampionPerson { get; set; }
}
