﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 102, "1.10.0" )]
    public class RapidAttendanceEntry : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //RockMigrationHelper.AddPage( true, "78B79290-3234-4D8C-96D3-1901901BA1DD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attendance List", "", "D56CD916-C3C7-4277-BEBA-0FA4C21A758D", "" ); // Site:Rock RMS
            //RockMigrationHelper.UpdateBlockType( "Attendance List", "Block for displaying the attendance history of a person or a group.", "~/Blocks/CheckIn/AttendanceList.ascx", "Checkin", "678ED4B6-D76F-4D43-B069-659E352C9BD8" );
            //RockMigrationHelper.AddBlock( true, "D56CD916-C3C7-4277-BEBA-0FA4C21A758D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "678ED4B6-D76F-4D43-B069-659E352C9BD8".AsGuid(), "Attendance List", "Main", @"", @"", 0, "9590C855-903E-4CA4-9296-FB9D6F02FA41" );

            //// Attrib for BlockType: Rapid Attendance Entry:Add Family Page              
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Family Page", "AddFamilyPage", "Add Family Page", @"Page used for adding new families.", 0, @"", "CB4EDB5A-2919-48C3-BB9F-434C1024AF92" );
            //// Attrib for BlockType: Rapid Attendance Entry:Attendance List Page             
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance List Page", "AttendanceListPage", "Attendance List Page", @"Page used to show the attendance list.", 1, @"", "2AA8EFA0-2B21-4A36-9326-5F71C621FBD7" );
            //// Attrib Value for Block:Rapid Attendance Entry, Attribute:Add Family Page Page: Rapid Attendance Entry, Site: Rock RMS           
            //RockMigrationHelper.AddBlockAttributeValue( "24560306-8535-4119-BA4A-CBC172C3832C", "CB4EDB5A-2919-48C3-BB9F-434C1024AF92", @"6A11A13D-05AB-4982-A4C2-67A8B1950C74" );
            //// Attrib Value for Block:Rapid Attendance Entry, Attribute:Attendance List Page Page: Rapid Attendance Entry, Site: Rock RMS   
            //RockMigrationHelper.AddBlockAttributeValue( "24560306-8535-4119-BA4A-CBC172C3832C", "2AA8EFA0-2B21-4A36-9326-5F71C621FBD7", @"D56CD916-C3C7-4277-BEBA-0FA4C21A758D" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
