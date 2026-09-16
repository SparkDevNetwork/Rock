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
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// A workflow type as it appears in a list.
/// </summary>
internal class WorkflowTypeSummaryResult : EntityResultBase
{
    /// <summary>
    /// The name of the workflow type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the workflow type.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The category the workflow type is filed under.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// Indicates that the workflow type is active and can be started.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that instances are saved to the database rather than running
    /// only in memory.
    /// </summary>
    public bool IsPersisted { get; set; }

    /// <summary>
    /// How much detail is written to the workflow log.
    /// </summary>
    /// <remarks>
    /// Carried in the list result on purpose. A workflow left on a verbose
    /// logging level after debugging is a real and easily missed problem, and a
    /// list is where someone notices it.
    /// </remarks>
    public WorkflowLoggingLevel LoggingLevel { get; set; }

    /// <summary>
    /// How many activities the workflow type has, which tells a caller whether
    /// retrieving the whole tree is worthwhile.
    /// </summary>
    public int ActivityTypeCount { get; set; }
}
