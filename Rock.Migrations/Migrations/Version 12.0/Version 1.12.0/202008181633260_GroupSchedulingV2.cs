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
    public partial class GroupSchedulingV2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Schedule", "Order", c => c.Int( nullable: false ) );
            PagesBlocksAttributesUp();
        }

        /// <summary>
        /// Pageses the blocks attributes up.
        /// </summary>
        public void PagesBlocksAttributesUp()
        {
            // Add Page Group Schedule Roster to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "896ED8DA-46A5-440B-92A0-76459869D921", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Schedule Roster", "", "37AE5C9E-7075-4F22-BDC6-189FA2584183", "fa fa-calendar-check-o" );

            // Add Page Group Schedule Communication to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "896ED8DA-46A5-440B-92A0-76459869D921", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Schedule Communication", "", "AFC2DA5B-B1D0-408C-ADBD-23E5D7A7AC67", "fa fa-envelope" );

            // Add/Update BlockType Group Schedule Roster
            RockMigrationHelper.UpdateBlockType( "Group Schedule Roster", "Allows a person to view and print a roster by defining group schedule criteria.", "~/Blocks/GroupScheduling/GroupScheduleRoster.ascx", "Group Scheduling", "730F5D9E-A411-48F2-BBDF-51146C510817" );

            // Add/Update BlockType Group Schedule Communication
            RockMigrationHelper.UpdateBlockType( "Group Schedule Communication", "Allows an individual to create a communication based on group schedule criteria.", "~/Blocks/GroupScheduling/GroupScheduleCommunication.ascx", "Group Scheduling", "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04" );

            // Add Block Schedule List to Page: Schedules, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AFFFB245-A0EB-4002-B736-A2D52DD692CF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C1B934D1-2139-471E-B2B8-B22FF4499B2F".AsGuid(), "Schedule List", "Main", @"", @"", 3, "EE783113-5564-4238-B730-B2AA57FAE268" );

            // Add Block Group Schedule Roster to Page: Group Schedule Roster, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "37AE5C9E-7075-4F22-BDC6-189FA2584183".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "730F5D9E-A411-48F2-BBDF-51146C510817".AsGuid(), "Group Schedule Roster", "Main", @"", @"", 0, "2A6F62FA-AF8D-4DF8-9C30-0B2C596F521E" );

            // Add Block Group Schedule Communication to Page: Group Schedule Communication, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AFC2DA5B-B1D0-408C-ADBD-23E5D7A7AC67".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04".AsGuid(), "Group Schedule Communication", "Main", @"", @"", 0, "18655A53-962F-4DA3-8D45-885204C0170A" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Schedules,  Zone: Main,  Block: Category Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '83DBFFDF-E92F-420D-8269-E68B9EB5BEDC'" );

            // Update Order for Page: Schedules,  Zone: Main,  Block: Schedule Category Exclusion List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '286DC221-C04A-4405-8B69-EC3D83740CD0'" );

            // Update Order for Page: Schedules,  Zone: Main,  Block: Schedule Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '765DE272-6F00-47C3-B445-303A7CF80486'" );

            // Update Order for Page: Schedules,  Zone: Main,  Block: Schedule List
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'EE783113-5564-4238-B730-B2AA57FAE268'" );

            // Attribute for BlockType: Group Schedule Roster:Enable Live Refresh
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "730F5D9E-A411-48F2-BBDF-51146C510817", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Live Refresh", "EnableLiveRefresh", "Enable Live Refresh", @"The Email address to show.", 0, @"True", "3B867E28-1D62-4649-AEEF-6F1B89433311" );

            // Attribute for BlockType: Schedule List:Filter Category From Query String
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Category From Query String", "FilterCategoryFromQueryString", "Filter Category From Query String", @"", 1, @"False", "A39C21A9-4DE7-421A-90D3-05889C9D26A1" );

            // Attribute for BlockType: Group Schedule Status Board:Roster Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roster Page", "RostersPage", "Roster Page", @"The page to use to view and print a rosters.", 1, @"", "26C78814-A8D5-4D4B-8FE7-16B0F2103B35" );

            // Attribute for BlockType: Group Schedule Status Board:Communications Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communications Page", "CommunicationsPage", "Communications Page", @"The page to use to send group scheduling communications.", 2, @"", "D66222DA-B119-4C2E-BB25-657422AE7140" );

            // Attribute for BlockType: Group Schedule Roster:Roster Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "730F5D9E-A411-48F2-BBDF-51146C510817", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Roster Lava Template", "RosterLavaTemplate", "Roster Lava Template", @"", 2, @"{% include '~~/Assets/Lava/GroupScheduleRoster.lava' %}", "FA1EF7D3-0494-441C-BBE0-699866832354" );

            // Attribute for BlockType: Group Schedule Roster:Refresh Interval (seconds)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "730F5D9E-A411-48F2-BBDF-51146C510817", "C8B6C51A-DD7C-4B75-8604-F0580697088E", "Refresh Interval (seconds)", "RefreshIntervalSeconds", "Refresh Interval (seconds)", @"The number of seconds to refresh the page. Note that setting this option too low could put a strain on the server if loaded on several clients at once.", 1, @"30", "67EE05D5-6E94-4EC3-9311-3B3BFF87C034" );

            // Block Attribute Value for Group Schedule Status Board ( Page: Group Schedule Status Board, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EEC694EB-C5A7-43E0-A8D5-77E52769252F", "26C78814-A8D5-4D4B-8FE7-16B0F2103B35", @"37ae5c9e-7075-4f22-bdc6-189fa2584183" );

            // Block Attribute Value for Group Schedule Status Board ( Page: Group Schedule Status Board, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EEC694EB-C5A7-43E0-A8D5-77E52769252F", "D66222DA-B119-4C2E-BB25-657422AE7140", @"afc2da5b-b1d0-408c-adbd-23e5d7a7ac67" );

            // Block Attribute Value for Group Schedule Roster ( Page: Group Schedule Roster, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "2A6F62FA-AF8D-4DF8-9C30-0B2C596F521E", "3B867E28-1D62-4649-AEEF-6F1B89433311", @"True" );

            // Block Attribute Value for Group Schedule Roster ( Page: Group Schedule Roster, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "2A6F62FA-AF8D-4DF8-9C30-0B2C596F521E", "FA1EF7D3-0494-441C-BBE0-699866832354", @"{% include '~~/Assets/Lava/GroupScheduleRoster.lava' %}" );

            // Block Attribute Value for Group Schedule Roster ( Page: Group Schedule Roster, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "2A6F62FA-AF8D-4DF8-9C30-0B2C596F521E", "67EE05D5-6E94-4EC3-9311-3B3BFF87C034", @"10" );

            // Block Attribute Value for Dev Links ( Page: Internal Homepage, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "6225AC0E-0146-423C-BF97-37A51695A261", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EE783113-5564-4238-B730-B2AA57FAE268", "295A1429-9581-49CF-87D9-9FA912707646", @"True" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EE783113-5564-4238-B730-B2AA57FAE268", "00F227B2-C977-4BA6-816A-F45C6FE9EF5A", @"" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EE783113-5564-4238-B730-B2AA57FAE268", "1FC92D2C-9949-48C3-B6F3-3D009CCEEE6F", @"False" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "EE783113-5564-4238-B730-B2AA57FAE268", "A39C21A9-4DE7-421A-90D3-05889C9D26A1", @"True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Schedule", "Order" );

            PagesBlocksAttributesDown();
        }

        /// <summary>
        /// Pageses the blocks attributes down.
        /// </summary>
        public void PagesBlocksAttributesDown()
        {
            // Filter Category From Query String Attribute for BlockType: Schedule List
            RockMigrationHelper.DeleteAttribute( "A39C21A9-4DE7-421A-90D3-05889C9D26A1" );

            // Communications Page Attribute for BlockType: Group Schedule Status Board
            RockMigrationHelper.DeleteAttribute( "D66222DA-B119-4C2E-BB25-657422AE7140" );

            // Roster Page Attribute for BlockType: Group Schedule Status Board
            RockMigrationHelper.DeleteAttribute( "26C78814-A8D5-4D4B-8FE7-16B0F2103B35" );

            // Refresh Interval (seconds) Attribute for BlockType: Group Schedule Roster
            RockMigrationHelper.DeleteAttribute( "67EE05D5-6E94-4EC3-9311-3B3BFF87C034" );

            // Roster Lava Template Attribute for BlockType: Group Schedule Roster
            RockMigrationHelper.DeleteAttribute( "FA1EF7D3-0494-441C-BBE0-699866832354" );

            // Enable Live Refresh Attribute for BlockType: Group Schedule Roster
            RockMigrationHelper.DeleteAttribute( "3B867E28-1D62-4649-AEEF-6F1B89433311" );

            // Remove Block: Schedule List, from Page: Schedules, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EE783113-5564-4238-B730-B2AA57FAE268" );

            // Remove Block: Group Schedule Communication, from Page: Group Schedule Communication, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "18655A53-962F-4DA3-8D45-885204C0170A" );

            // Remove Block: Group Schedule Roster, from Page: Group Schedule Roster, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2A6F62FA-AF8D-4DF8-9C30-0B2C596F521E" );

            // Delete BlockType Group Schedule Communication
            RockMigrationHelper.DeleteBlockType( "9F813A6C-25A7-491F-9C01-6D6EE6A7CA04" ); // Group Schedule Communication

            // Delete BlockType Group Schedule Roster
            RockMigrationHelper.DeleteBlockType( "730F5D9E-A411-48F2-BBDF-51146C510817" ); // Group Schedule Roster

            // Delete Page Group Schedule Communication from Site:Rock RMS
            RockMigrationHelper.DeletePage( "AFC2DA5B-B1D0-408C-ADBD-23E5D7A7AC67" ); //  Page: Group Schedule Communication, Layout: Full Width, Site: Rock RMS

            // Delete Page Group Schedule Roster from Site:Rock RMS
            RockMigrationHelper.DeletePage( "37AE5C9E-7075-4F22-BDC6-189FA2584183" ); //  Page: Group Schedule Roster, Layout: Full Width, Site: Rock RMS

        }
    }
}
