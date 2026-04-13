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
    public partial class FinalChopWebFormsServiceMetricEntryBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "(final) Service Metric Entry 19.0",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "535E1879-CD4C-432B-9312-B27B3A668D88", "E6144C7A-2E95-431B-AB75-C588D151ACA4" }, // Service Metric Entry
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_190_CHOP_SERVICE_METRIC_ENTRY_FINAL,
                blockAttributeKeysToIgnore: new Dictionary<string, string>  {
                { "535E1879-CD4C-432B-9312-B27B3A668D88", "EnableDebug" } // Registration Entry
            } );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No way to undo this.
        }
    }
}
