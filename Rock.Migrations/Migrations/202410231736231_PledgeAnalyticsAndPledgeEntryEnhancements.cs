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
namespace Rock.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PledgeAnalyticsAndPledgeEntryEnhancements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._202410231736231_PledgeAnalyticsAndPledgeEntryEnhancements_spFinance_PledgeAnalyticsQuery );
            Sql( MigrationSQL._202410231736231_PledgeAnalyticsAndPledgeEntryEnhancements_spFinance_PledgeAnalyticsQueryWithMultipleAccountIds );
            ChopPledgeEntryBlock();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DROP PROCEDURE [dbo].[spFinance_PledgeAnalyticsQueryWithMultipleAccountIds]" );
        }

        #region KH: Chop PledgeEntry Block

        private void ChopPledgeEntryBlock()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Pledge Entry Block",
                blockTypeReplacements: new Dictionary<string, string> {
{ "20B5568E-A010-4E15-9127-E63CF218D6E5", "0455ECBD-D54D-4485-BF4D-F469048AE10F" }, // Pledge Entry
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_167_CHOP_PLEDGE_ENTRY_BLOCK,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "20B5568E-A010-4E15-9127-E63CF218D6E5", "DefaultAccounts,LegendText,GivingPage,DefaultStartDate,DefaultEndDate,DefaultConnectionStatus,EnableDebug" },
            } );
        }

        #endregion
    }
}
