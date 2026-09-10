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

using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Provides access to Rock's reports and data views: listing them, reading their
/// configuration, running a data view to get the records it selects, and running
/// a report to get its rows.
/// </summary>
/// <remarks>
/// <para>
/// Data view execution is fully headless. Report execution goes through an
/// internal Rock core shim (<c>Rock.Reporting.AgentReportRunner</c>) so the
/// System.Web dependency of the report engine never enters this assembly.
/// </para>
/// </remarks>
[Description( "Provides access to Rock's reports and data views: listing them, reading their configuration, and running data views and reports to get their results." )]
[AgentSkillGuid( "39BB9DB1-569A-44C1-9F1D-61E8B16D8846" )]
[EntityTypeGuid( "F8E8E905-6893-442F-8331-6DFC352C86C1" )]
internal sealed partial class ReportingSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Test Seams

    /*
        9/1/26 - CLAUDE

        Data view and report execution add the source id to the generated query
        as a SQL comment via TagWith. That tagging relies on the query provider's
        SQL translation layer, which does not exist under the mocked context used
        by unit tests, where it silently drops every result. This internal seam
        lets those tests turn tagging off so the rest of the execution path can be
        exercised. It defaults to false so production behavior is unchanged.

        Reason: Allow headless unit testing of data view execution against a mock.
    */

    /// <summary>
    /// Gets or sets a value indicating whether query tagging should be disabled
    /// when running data views and reports. Defaults to <c>false</c>. Intended
    /// only as a test seam; see the note above.
    /// </summary>
    internal bool IsQueryTaggingDisabled { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Reporting Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public ReportingSkill( ILogger<ReportingSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion
}
