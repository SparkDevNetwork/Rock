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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 265, "18.1" )]
    public class MigrationRollupsForV18_1_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddFinancialBatchIndex_20251023_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
        }

        #region JPH: Add FinancialBatch Index

        /// <summary>
        /// JPH - Add a new index to the FinancialBatch table - up.
        /// </summary>
        private void JPH_AddFinancialBatchIndex_20251023_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.1 - Add FinancialBatch Index",
                description: "This job will add a new index to the FinancialBatch table.",
                jobType: "Rock.Jobs.PostV181AddFinancialBatchIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_181_ADD_FINANCIALBATCH_INDEX );
        }

        #endregion

    }
}