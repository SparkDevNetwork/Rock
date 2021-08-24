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
    public partial class CodeGenerated_0803 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Attribute for BlockType: Dynamic Data:Enable Quick Return
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Quick Return", "EnableQuickReturn", "Enable Quick Return", @"When enabled, viewing the block will cause it to be added to the Quick Return list in the bookmarks feature.", 0, @"False", "0E6BDD50-E1FA-4EB8-A6EC-9466D9F63131" );

            // Attribute for BlockType: Workflow Entry:Log Interaction when Form is Viewed
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interaction when Form is Viewed", "LogInteractionOnView", "Log Interaction when Form is Viewed", @"", 5, @"False", "5044EA47-D14E-44C6-9B8B-B8FC14007A5D" );

            // Attribute for BlockType: Workflow Entry:Log Interaction when Form is Completed
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interaction when Form is Completed", "LogInteractionOnCompletion", "Log Interaction when Form is Completed", @"", 6, @"False", "D164879E-8B1C-44A1-8BC3-73527FB33182" );

            // Attribute for BlockType: Personal Link List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7546752-C3DC-4B96-88D9-A431F2D1C989", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A04887DD-3822-4EBC-A835-A0E3EB984CA2" );

            // Attribute for BlockType: Personal Link List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7546752-C3DC-4B96-88D9-A431F2D1C989", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5E36152E-90DF-407D-A994-57C697074C8F" );

            // Attribute for BlockType: Personal Link Section List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0BFD74A8-1888-4407-9102-D3FCEABF3095", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2F96489D-4BAC-4F34-9CA9-71668234B7A3" );

            // Attribute for BlockType: Personal Link Section List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0BFD74A8-1888-4407-9102-D3FCEABF3095", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "538594D6-1156-46F5-A375-756AB1F1D354" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Enable Quick Return Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("0E6BDD50-E1FA-4EB8-A6EC-9466D9F63131");

            // Log Interaction when Form is Completed Attribute for BlockType: Workflow Entry
            RockMigrationHelper.DeleteAttribute("D164879E-8B1C-44A1-8BC3-73527FB33182");

            // Log Interaction when Form is Viewed Attribute for BlockType: Workflow Entry
            RockMigrationHelper.DeleteAttribute("5044EA47-D14E-44C6-9B8B-B8FC14007A5D");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Personal Link Section List
            RockMigrationHelper.DeleteAttribute("2F96489D-4BAC-4F34-9CA9-71668234B7A3");

            // core.CustomActionsConfigs Attribute for BlockType: Personal Link Section List
            RockMigrationHelper.DeleteAttribute("538594D6-1156-46F5-A375-756AB1F1D354");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Personal Link List
            RockMigrationHelper.DeleteAttribute("5E36152E-90DF-407D-A994-57C697074C8F");

            // core.CustomActionsConfigs Attribute for BlockType: Personal Link List
            RockMigrationHelper.DeleteAttribute("A04887DD-3822-4EBC-A835-A0E3EB984CA2");
        }
    }
}
