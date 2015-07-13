// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class EventDetailPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex("dbo.EventItem", "PhotoId");
            Sql( "Update EventItem set PhotoId = null where PhotoId not in (select Id from BinaryFile)" );
            AddForeignKey("dbo.EventItem", "PhotoId", "dbo.BinaryFile", "Id");

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Details Page", "DetailsPage", "", "Detail page for events", 0, @"", "ABFA11BD-19AF-4F8D-BE41-F667C928EB50" );
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "ABFA11BD-19AF-4F8D-BE41-F667C928EB50", @"8a477cc6-4a12-4fbe-8037-e666476dd413" ); // Details Page

            // Page: Event Details
            RockMigrationHelper.AddPage( "2E6FED28-683F-4726-8CF1-2822E8E73B03", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Event Details", "", "8A477CC6-4A12-4FBE-8037-E666476DD413", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Calendar Item Lava", "Renders a particular calendar item using Lava.", "~/Blocks/Event/CalendarItemLava.ascx", "Event", "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9" );
            RockMigrationHelper.AddBlock( "8A477CC6-4A12-4FBE-8037-E666476DD413", "", "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9", "Calendar Item Lava", "Main", "", "", 0, "FC400B7B-760A-4090-9E9E-F049631E7BB4" );

            RockMigrationHelper.AddBlockTypeAttribute( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the calendar item name.", 0, @"False", "5B3B9094-50C1-4E5A-8C6B-748E83BE792F" );

            RockMigrationHelper.AddBlockTypeAttribute( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "Registration page for events", 0, @"", "5FE92034-CC9B-4C3E-95B1-09CC91B0417A" );

            RockMigrationHelper.AddBlockTypeAttribute( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of events.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalCalendarItem.lava' %}", "100F84DD-4526-41CD-80BC-246E15CC8E04" );

            RockMigrationHelper.AddBlockTypeAttribute( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "ABABD149-FD7C-468D-9D9E-CCE657CB8172" );

            RockMigrationHelper.AddBlockAttributeValue( "FC400B7B-760A-4090-9E9E-F049631E7BB4", "ABABD149-FD7C-468D-9D9E-CCE657CB8172", @"False" ); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue( "FC400B7B-760A-4090-9E9E-F049631E7BB4", "5B3B9094-50C1-4E5A-8C6B-748E83BE792F", @"False" ); // Set Page Title

            RockMigrationHelper.AddBlockAttributeValue( "FC400B7B-760A-4090-9E9E-F049631E7BB4", "100F84DD-4526-41CD-80BC-246E15CC8E04", @"{% include '~/Themes/Stark/Assets/Lava/ExternalCalendarItem.lava' %}" ); // Lava Template

            RockMigrationHelper.AddBlockAttributeValue( "FC400B7B-760A-4090-9E9E-F049631E7BB4", "5FE92034-CC9B-4C3E-95B1-09CC91B0417A", @"f7ca6e0f-c319-47ab-9a6d-247c5716d846,349782f9-b8d3-4c29-84f5-44b86cd198c2" ); // Registration Page

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "5FE92034-CC9B-4C3E-95B1-09CC91B0417A" );
            RockMigrationHelper.DeleteAttribute( "100F84DD-4526-41CD-80BC-246E15CC8E04" );
            RockMigrationHelper.DeleteAttribute( "5B3B9094-50C1-4E5A-8C6B-748E83BE792F" );
            RockMigrationHelper.DeleteAttribute( "ABABD149-FD7C-468D-9D9E-CCE657CB8172" );
            RockMigrationHelper.DeleteBlock( "FC400B7B-760A-4090-9E9E-F049631E7BB4" );
            RockMigrationHelper.DeleteBlockType( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9" );
            RockMigrationHelper.DeletePage( "8A477CC6-4A12-4FBE-8037-E666476DD413" ); //  Page: Event Details

            RockMigrationHelper.DeleteAttribute( "ABFA11BD-19AF-4F8D-BE41-F667C928EB50" );

            DropForeignKey("dbo.EventItem", "PhotoId", "dbo.BinaryFile");
            DropIndex("dbo.EventItem", new[] { "PhotoId" });
        }
    }
}
