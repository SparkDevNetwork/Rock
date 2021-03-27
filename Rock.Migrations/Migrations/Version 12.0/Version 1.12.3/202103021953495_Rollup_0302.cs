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
    public partial class Rollup_0302 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateConnectionRequestStatus();
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
            
            // Add/Update BlockType Giving Analytics Alerts
            RockMigrationHelper.UpdateBlockType("Giving Analytics Alerts","Lists of current alerts based on current filters.","~/Blocks/Finance/GivingAnalyticsAlerts.ascx","Finance","0A813EC3-EC36-499B-9EBD-C3388DC7F49D");

            // Add/Update BlockType Giving Analytics Configuration
            RockMigrationHelper.UpdateBlockType("Giving Analytics Configuration"," Block used to view and create new alert types for the giving analytics system.","~/Blocks/Finance/GivingAnalyticsConfiguration.ascx","Finance","A91ACA78-68FD-41FC-B652-17A37789EA32");

            // Attribute for BlockType: Communication Entry Wizard:Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.", 13, @"True", "83FD1108-8F81-4A58-AA4A-1381A99B5D25" );

            // Attribute for BlockType: Email Analytics:Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 1, @"180", "60AC67F7-E55B-41CC-BA31-EF0252F021FB" );

            // Attribute for BlockType: Giving Analytics Alerts:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "TransactionPage", "Detail Page", @"", 0, @"", "E264C1BD-4B7C-449C-A884-D738061744BA" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Detail Page Attribute for BlockType: Giving Analytics Alerts
            RockMigrationHelper.DeleteAttribute("E264C1BD-4B7C-449C-A884-D738061744BA");

            // Database Timeout Attribute for BlockType: Email Analytics
            RockMigrationHelper.DeleteAttribute("60AC67F7-E55B-41CC-BA31-EF0252F021FB");

            // Enable Person Parameter Attribute for BlockType: Communication Entry Wizard
            RockMigrationHelper.DeleteAttribute("83FD1108-8F81-4A58-AA4A-1381A99B5D25");

            // Delete BlockType Giving Analytics Configuration
            RockMigrationHelper.DeleteBlockType("A91ACA78-68FD-41FC-B652-17A37789EA32"); // Giving Analytics Configuration

            // Delete BlockType Giving Analytics Alerts
            RockMigrationHelper.DeleteBlockType("0A813EC3-EC36-499B-9EBD-C3388DC7F49D"); // Giving Analytics Alerts
        }

        /// <summary>
        /// SK: Update Connection request status to active for very old (-2 days) FollowupDate values
        /// </summary>
        private void UpdateConnectionRequestStatus()
        {
            Sql( @"
                UPDATE
	                [ConnectionRequest]
                SET [ConnectionState]=0
                WHERE
	                [ConnectionState]=2
                AND [FollowupDate] <= DATEADD(DD, -2, cast(getdate() as date)) " );
        }
    }
}
