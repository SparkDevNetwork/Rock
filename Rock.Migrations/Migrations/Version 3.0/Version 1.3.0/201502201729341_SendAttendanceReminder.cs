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
    public partial class SendAttendanceReminder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "SendAttendanceReminder", c => c.Boolean(nullable: false));

            Sql( @"
    -- Update the 'Participant' connection status value to have the correct guid
    DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '2E6540EA-63F0-40FE-BE50-F2A84735E600' )
    DECLARE @DefinedValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId AND [Value] = 'Participant' )
    UPDATE [DefinedValue] SET [Guid] = '8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061' WHERE [Id] = @DefinedValueId
" );

            RockMigrationHelper.AddPage( "4E237286-B715-4109-A578-C1445EC02707", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Attendance", "", "7EA94B4F-013B-4A79-8D01-86994EB04604", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "7EA94B4F-013B-4A79-8D01-86994EB04604", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attendance", "", "D2A75147-B031-4DF7-8E04-FDDEAE2575F1", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Group Attendance Detail", "Lists the group members for a specific occurrence datetime and allows selecting if they attended or not.", "~/Blocks/Groups/GroupAttendanceDetail.ascx", "Groups", "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B" );
            RockMigrationHelper.UpdateBlockType( "Group Attendance List", "Lists all the scheduled occurrences for a given group.", "~/Blocks/Groups/GroupAttendanceList.ascx", "Groups", "5C547728-38C2-420A-8602-3CDAAC369247" );

            // Add Block to Page: Group Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7EA94B4F-013B-4A79-8D01-86994EB04604", "", "5C547728-38C2-420A-8602-3CDAAC369247", "Group Attendance List", "Main", "", "", 0, "81D05028-E0C2-4375-B43D-7F1CD2C28E62" );
            // Add Block to Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2A75147-B031-4DF7-8E04-FDDEAE2575F1", "", "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "Group Attendance Detail", "Main", "", "", 0, "FE956364-7B56-4E73-AD11-CB6DFB21B673" );

            // Attrib for BlockType: Group Detail:Attendance Page
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "", "The page to display attendance list.", 7, @"", "F4A22874-C0FB-4F9C-91CC-9BFE416A98D0" );

            // Attrib for BlockType: Group Attendance Detail:Allow Add
            RockMigrationHelper.AddBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", 0, @"True", "D24A540E-3E6B-4790-AB77-6661F8DA292E" );
            // Attrib for BlockType: Group Attendance Detail:Workflow
            RockMigrationHelper.AddBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", 1, @"", "63376AC6-6DCC-4FC2-9E9C-5207A1B90F26" );

            // Attrib for BlockType: Group Attendance List:Allow Add
            RockMigrationHelper.AddBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", 1, @"True", "B978EBAD-333D-4E8B-8C68-19FABB87984B" );
            // Attrib for BlockType: Group Attendance List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "15299237-7F47-404D-BEFF-460F7818D3D7" );

            // Attrib Value for Block:GroupDetailRight, Attribute:Attendance Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "F4A22874-C0FB-4F9C-91CC-9BFE416A98D0", @"7ea94b4f-013b-4a79-8d01-86994eb04604" );

            // Attrib Value for Block:Group Attendance List, Attribute:Allow Add Page: Group Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "81D05028-E0C2-4375-B43D-7F1CD2C28E62", "B978EBAD-333D-4E8B-8C68-19FABB87984B", @"True" );
            // Attrib Value for Block:Group Attendance List, Attribute:Detail Page Page: Group Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "81D05028-E0C2-4375-B43D-7F1CD2C28E62", "15299237-7F47-404D-BEFF-460F7818D3D7", @"d2a75147-b031-4df7-8e04-fddeae2575f1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Attendance Detail:Allow Add
            RockMigrationHelper.DeleteAttribute( "D24A540E-3E6B-4790-AB77-6661F8DA292E" );
            // Attrib for BlockType: Group Attendance Detail:Workflow
            RockMigrationHelper.DeleteAttribute( "63376AC6-6DCC-4FC2-9E9C-5207A1B90F26" );
            // Attrib for BlockType: Group Attendance List:Allow Add
            RockMigrationHelper.DeleteAttribute( "B978EBAD-333D-4E8B-8C68-19FABB87984B" );
            // Attrib for BlockType: Group Attendance List:Detail Page
            RockMigrationHelper.DeleteAttribute( "15299237-7F47-404D-BEFF-460F7818D3D7" );
            // Attrib for BlockType: Group Detail:Attendance Page
            RockMigrationHelper.DeleteAttribute( "F4A22874-C0FB-4F9C-91CC-9BFE416A98D0" );

            // Remove Block: Group Attendance Detail, from Page: Attendance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FE956364-7B56-4E73-AD11-CB6DFB21B673" );
            // Remove Block: Group Attendance List, from Page: Group Attendance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "81D05028-E0C2-4375-B43D-7F1CD2C28E62" );

            RockMigrationHelper.DeleteBlockType( "5C547728-38C2-420A-8602-3CDAAC369247" ); // Group Attendance List
            RockMigrationHelper.DeleteBlockType( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B" ); // Group Attendance Detail

            RockMigrationHelper.DeletePage( "D2A75147-B031-4DF7-8E04-FDDEAE2575F1" ); //  Page: Attendance, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "7EA94B4F-013B-4A79-8D01-86994EB04604" ); //  Page: Group Attendance, Layout: Full Width, Site: Rock RMS

            DropColumn("dbo.GroupType", "SendAttendanceReminder");
        }
    }
}
