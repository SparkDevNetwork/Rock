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
    public partial class LessPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Themes", "", "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "fa fa-picture-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Theme Styler", "", "A74EEC7C-4F9E-48F5-A996-74A856981B4C", "fa fa-paint-brush" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Content Channel Item Personal List Lava", "Displays a list of content items for the person using a Lava template.", "~/Blocks/Cms/ContentChannelItemPersonalListLava.ascx", "CMS", "13E4D4B5-0929-4ED6-9E59-05A6D511FA06" );
            RockMigrationHelper.UpdateBlockType( "Theme List", "Lists themes in the Theme folder.", "~/Blocks/Cms/ThemeList.ascx", "CMS", "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694" );
            RockMigrationHelper.UpdateBlockType( "Theme Styler", "Allows you to change the LESS variables of a theme.", "~/Blocks/Cms/ThemeStyler.ascx", "CMS", "92B100A0-43D7-4D73-B410-17DA8697606A" );

            RockMigrationHelper.UpdateBlockType( "Group Member RemoveFrom URL", "Removes a person from a group based on inputs from the URL query string (GroupId, PersonGuid).", "~/Blocks/Groups/GroupMemberRemoveFromUrl.ascx", "Groups", "0159CE20-7B41-4D53-985C-81877ED75767" );

            // Add Block to Page: Themes, Site: Rock RMS
            RockMigrationHelper.AddBlock( "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "", "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694", "Theme List", "Main", "", "", 0, "197363EE-20CD-47AC-97F8-B47401F7B2C5" );
            // Add Block to Page: Theme Styler, Site: Rock RMS
            RockMigrationHelper.AddBlock( "A74EEC7C-4F9E-48F5-A996-74A856981B4C", "", "92B100A0-43D7-4D73-B410-17DA8697606A", "Theme Styler", "Main", "", "", 0, "5F2DE624-248E-47D4-A124-1633C64D977C" );
            // Attrib for BlockType: Theme List:Theme Styler Page
            RockMigrationHelper.AddBlockTypeAttribute( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Theme Styler Page", "ThemeStylerPage", "", "Page to use for the theme styler page.", 0, @"", "A10C3369-CABB-4B53-9151-9A9521529709" );
            // Attrib Value for Block:Theme List, Attribute:Theme Styler Page Page: Themes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "197363EE-20CD-47AC-97F8-B47401F7B2C5", "A10C3369-CABB-4B53-9151-9A9521529709", @"a74eec7c-4f9e-48f5-a996-74a856981b4c" );
            RockMigrationHelper.UpdateFieldType( "Binary File Types", "", "Rock", "Rock.Field.Types.BinaryFileTypesFieldType", "A5365133-FF87-48B3-BCA3-AF6D871F8895" );
            RockMigrationHelper.UpdateFieldType( "Markdown", "", "Rock", "Rock.Field.Types.MarkdownFieldType", "C2FBCF94-0FDF-4DFD-93A5-19FE6A409C84" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Theme List:Theme Styler Page
            RockMigrationHelper.DeleteAttribute( "A10C3369-CABB-4B53-9151-9A9521529709" );

            // Remove Block: Theme Styler, from Page: Theme Styler, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5F2DE624-248E-47D4-A124-1633C64D977C" );

            // Remove Block: Theme List, from Page: Themes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "197363EE-20CD-47AC-97F8-B47401F7B2C5" );

            RockMigrationHelper.DeleteBlockType( "0159CE20-7B41-4D53-985C-81877ED75767" ); // Group Member RemoveFrom URL
            RockMigrationHelper.DeleteBlockType( "92B100A0-43D7-4D73-B410-17DA8697606A" ); // Theme Styler
            RockMigrationHelper.DeleteBlockType( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694" ); // Theme List
            RockMigrationHelper.DeleteBlockType( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06" ); // Content Channel Item Personal List Lava
            RockMigrationHelper.DeletePage( "A74EEC7C-4F9E-48F5-A996-74A856981B4C" ); //  Page: Theme Styler, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "BC2AFAEF-712C-4173-895E-81347F6B0B1C" ); //  Page: Themes, Layout: Full Width, Site: Rock RMS
        }
    }
}
