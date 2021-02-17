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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0216 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            KPIUpdate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            
            // Attribute for BlockType: Phone Number Lookup:Validation Code Attempts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "51BB37DA-6F3E-40EC-B80E-D381E13E01B2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Validation Code Attempts", "ValidationCodeAttempts", "Validation Code Attempts", @"The number of times a validation code verification can be re-tried before failing permanently.", 11, @"10", "34B8A631-A751-4218-A67B-4DB9B89755A7" );

            // Attribute for BlockType: Room List:Show Only Parent Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DEA7808-9AC1-4913-BF58-1CDC7922C901", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Parent Group", "ShowOnlyParentGroup", "Show Only Parent Group", @"When enabled, only the actual parent group for each check-in group-location will be shown and groups under the same parent group in the same location will be combined into one row.", 5, @"False", "3C1273A0-D87B-4A61-A2D4-0078C37999AC" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Validation Code Attempts Attribute for BlockType: Phone Number Lookup
            RockMigrationHelper.DeleteAttribute("34B8A631-A751-4218-A67B-4DB9B89755A7");

            // Show Only Parent Group Attribute for BlockType: Room List
            RockMigrationHelper.DeleteAttribute("3C1273A0-D87B-4A61-A2D4-0078C37999AC");
        }

        /// <summary>
        /// GJ: Add KPI Update
        /// </summary>
        private void KPIUpdate()
        {
            Sql( MigrationSQL._202102162206327_Rollup_0216_KPIUpdate );
        }
    }
}
