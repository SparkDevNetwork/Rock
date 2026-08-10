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

using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.MetricSkill;

/// <summary>
/// The summary results for a metric.
/// </summary>
internal class MetricSummaryResult
{
    /// <summary>
    /// The partitions that are defined for the data set.
    /// </summary>
    public List<string> Partitions { get; set; }

    /// <summary>
    /// The possible values for the partitions used in the data set. The key is
    /// name of the partition.
    /// </summary>
    public Dictionary<string, List<string>> PartitionValues { get; set; }

    /// <summary>
    /// The data set.
    /// </summary>
    public List<MetricSummaryValueResult> Data { get; set; }

    /// <summary>
    /// The label that describes what each unit is.
    /// </summary>
    public string UnitOfMeasure { get; set; }

    /// <summary>
    /// The type of value that is being measured.
    /// </summary>
    public UnitType? UnitType { get; set; }
}
