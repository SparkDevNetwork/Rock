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

namespace Rock.AI.Agent.Classes.Skills.ConnectionSkill;

/// <summary>
/// The insights result object for Connection Requests.
/// </summary>
internal class ConnectionRequestInsightsResult
{
    /// <summary>
    /// The number of active requests.
    /// </summary>
    public int ActiveCount { get; set; }

    /// <summary>
    /// The number of unassigned connection requests.
    /// </summary>
    public int UnassignedCount { get; set; }

    /// <summary>
    /// The counts by connection status.
    /// </summary>
    public List<SummaryGroupResult> CountByStatus { get; set; }

    /// <summary>
    /// The top connectors for the requested connection requests.
    /// </summary>
    public List<InsightsConnectorResult> TopConnectors { get; set; }
}
