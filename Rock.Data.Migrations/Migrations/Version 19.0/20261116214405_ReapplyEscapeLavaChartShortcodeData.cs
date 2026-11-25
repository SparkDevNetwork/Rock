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
//
namespace Rock.Data.Migrations;

/// <summary>
/// Encodes the data item and dataset values that the Chart Lava shortcode writes into its script block, on sites where the markup was written again after <see cref="EscapeLavaChartShortcodeData"/> ran.
/// </summary>
/// <seealso cref="Rock.Data.Migration" />
[SqlMigration( 20261116214405, "19.0", ReplacesHotFixNumber = 347 )]
internal class ReapplyEscapeLavaChartShortcodeData : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        /*
            11/16/26 - JMH

            The v19.0 EF migration Rollup_20260114 replaces the whole Chart shortcode markup.
            A site that ran hotfix migration 346 on v17 or v18 and then upgrades into v19
            gets the unencoded markup back, and 346 does not run a second time.
            This runs the same SQL under a new number, after the EF migrations.
            Where 346 already applied, the old fragments are gone and nothing changes.

            Reason: Keep the Chart shortcode encoding on sites that upgrade into v19.
        */
        Sql( EscapeLavaChartShortcodeData.EscapeChartMarkupSql );
    }
}
