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
    public partial class UpdateCampusTeamImplementation : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Group Member List:Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.GROUPS_GROUP_MEMBER_LIST, SystemGuid.FieldType.TEXT, "Block Title", "BlockTitle", "Block Title", @"The text used in the title/header bar for this block.", 0, @"Group Members", "EB6292BE-96EA-4E08-A8CA-7245ACAA151D" );
            
            // Attrib Value for Block:Group Member List, Attribute:Block Title Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "EB6292BE-96EA-4E08-A8CA-7245ACAA151D", @"Campus Team" );

            // Change the names of the default Roles for 'Campus Team' GroupType
            RockMigrationHelper.AddGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM, "Campus Pastor", "Pastor of a Campus", 0, 1, null, SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_PASTOR, true, true, false );
            RockMigrationHelper.AddGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM, "Campus Administrator", "Administrator of a Campus", 1, null, null, SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_ADMINISTRATOR, true, false, true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib Value for Block:Group Member List, Attribute:Block Title Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "EB6292BE-96EA-4E08-A8CA-7245ACAA151D" );
            
            // Attrib for BlockType: Group Member List:Block Title
            RockMigrationHelper.DeleteAttribute( "EB6292BE-96EA-4E08-A8CA-7245ACAA151D" );
        }
    }
}
