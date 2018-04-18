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
    public partial class SiteFavIcon : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "EnabledForShortening", c => c.Boolean(nullable: false));
            AddColumn("dbo.Site", "FavIconBinaryFileId", c => c.Int());
            CreateIndex("dbo.Site", "FavIconBinaryFileId");
            AddForeignKey("dbo.Site", "FavIconBinaryFileId", "dbo.BinaryFile", "Id");

            Sql( @"
    UPDATE [Site] SET [EnabledForShortening] = 1
	WHERE [Guid] NOT IN ( '05E96F7B-B75E-4987-825A-B6F51F8D9CAA', 'A5FA7C3C-A238-4E0B-95DE-B540144321EC', '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A' )
" );

            // SK: Rename MyWorkflowsLiquid to MyWorkflowsLava.cs
            Sql( @"
    DELETE [BlockType] WHERE [Path]='~/Blocks/WorkFlow/MyWorkflowsLava.ascx'
    UPDATE [BlockType] SET 
        [Path] = '~/Blocks/WorkFlow/MyWorkflowsLava.ascx', 
        [Name] = 'My Workflows Lava' 
    WHERE [Guid] = '4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1'
" );

            // JE: Add Rapid Attendance Page
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Rapid Attendance Entry", "", "78B79290-3234-4D8C-96D3-1901901BA1DD", "fa fa-calendar-check-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Rapid Attendance Entry", "Provides a way to manually enter attendance for a large group of people in an efficient manner.", "~/Blocks/CheckIn/RapidAttendanceEntry.ascx", "Check-in", "6C2ED1FA-218B-4ACC-B661-A2618F310CD4" );
            RockMigrationHelper.AddBlock( "78B79290-3234-4D8C-96D3-1901901BA1DD", "", "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "Rapid Attendance Entry", "Main", "", "", 0, "24560306-8535-4119-BA4A-CBC172C3832C" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Parent Group", "ParentGroup", "", "Select the parent group whose immediate childeren will be displayed as options to take attendance for.", 0, @"", "A24BB4C5-4493-405F-91A6-FEEF257FF7B5" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Show Current Attendees", "DefaultShowCurrentAttendees", "", "Should the Current Attendees grid be visible by default. When the grid is enabled performance will be reduced.", 1, @"False", "B291C231-3FCE-4B40-8C9E-C03A8DF3BE39" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Parent Group", "IncludeParentGroup", "", "If true then the parent group will be included as an option in addition to it's children.", 1, @"False", "248FB07D-D939-405C-BC4E-0D6A03EBE2D1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "B291C231-3FCE-4B40-8C9E-C03A8DF3BE39" );
            RockMigrationHelper.DeleteAttribute( "A24BB4C5-4493-405F-91A6-FEEF257FF7B5" );
            RockMigrationHelper.DeleteAttribute( "248FB07D-D939-405C-BC4E-0D6A03EBE2D1" );
            RockMigrationHelper.DeleteBlock( "24560306-8535-4119-BA4A-CBC172C3832C" );
            RockMigrationHelper.DeleteBlockType( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4" );
            RockMigrationHelper.DeletePage( "78B79290-3234-4D8C-96D3-1901901BA1DD" ); 

            DropForeignKey("dbo.Site", "FavIconBinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.Site", new[] { "FavIconBinaryFileId" });
            DropColumn("dbo.Site", "FavIconBinaryFileId");
            DropColumn("dbo.Site", "EnabledForShortening");
        }
    }
}
