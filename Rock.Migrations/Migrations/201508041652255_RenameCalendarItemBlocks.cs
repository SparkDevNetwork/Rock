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
    public partial class RenameCalendarItemBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rename blocktypes
            RockMigrationHelper.RenameBlockType( "~/Blocks/Event/CalendarItemDetail.ascx", "~/Blocks/Event/EventItemDetail.ascx", null, "Calendar Event Item Detail", "Displays the details of the given calendar event item." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Event/CalendarItemList.ascx", "~/Blocks/Event/EventItemList.ascx", null, "Calendar Event Item List", "Lists all the event items in the given calendar." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Event/CalendarItemOccurrenceDetail.ascx", "~/Blocks/Event/EventItemOccurrenceDetail.ascx", null, "Calendar Event Item Occurrence Detail", "Displays the details of a given calendar event item occurrence." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Event/CalendarItemLava.ascx", "~/Blocks/Event/EventItemOccurrenceLava.ascx", null, "Calendar Event Item Occurrence Lava", "Renders a particular calendar event item occurrence using Lava." );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Event/CalendarItemOccurrenceList.ascx", "~/Blocks/Event/EventItemOccurrenceList.ascx", null, "Calendar Event Item Occurrence List", "Displays the occurrence details for a given calendar event item." );

            // Attrib for BlockType: Group Detail:Event Item Occurrence Page
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Item Occurrence Page", "EventItemOccurrencePage", "", "The page to display event item occurrence details.", 8, @"", "6114CE99-C97F-4394-93F5-B34D479AB54E" );
            // Attrib for BlockType: Group Detail:Content Item Page
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemPage", "", "The page to display registration details.", 9, @"", "45897721-F38C-4B4B-BCF9-A81D27DBB731" );

            // Attrib Value for Block:GroupDetailRight, Attribute:Event Item Occurrence Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "6114CE99-C97F-4394-93F5-B34D479AB54E", @"4b0c44ee-28e3-4753-a95b-8c57cd958fd1" );
            // Attrib Value for Block:GroupDetailRight, Attribute:Content Item Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "45897721-F38C-4B4B-BCF9-A81D27DBB731", @"d18e837c-9e65-4a38-8647-dff04a595d97" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Detail:Content Item Page
            RockMigrationHelper.DeleteAttribute( "45897721-F38C-4B4B-BCF9-A81D27DBB731" );
            // Attrib for BlockType: Group Detail:Event Item Occurrence Page
            RockMigrationHelper.DeleteAttribute( "6114CE99-C97F-4394-93F5-B34D479AB54E" );
        }
    }
}
