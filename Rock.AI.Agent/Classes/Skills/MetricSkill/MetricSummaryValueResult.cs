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

namespace Rock.AI.Agent.Classes.Skills.MetricSkill;

/// <summary>
/// A single value which contains all the specific values for the date.
/// </summary>
internal class MetricSummaryValueResult
{
    /// <summary>
    /// The date for this value result.
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// The total value across all partitioned values for this date.
    /// </summary>
    public decimal? Total { get; set; }

    /// <summary>
    /// The value for this date if there are no partitions.
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// The partitioned values for this date. The last object is the value and
    /// the proceeding objects are the partition value indexes. This provides
    /// a very concise way to return partitioned values that the language model
    /// can still understand.
    /// </summary>
    public List<List<object>> Values { get; set; }
}
