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
    public partial class GroupMemberNavigation : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update name and file name from FamilyNav to GroupMemberNavigation
            RockMigrationHelper.UpdateBlockTypeByGuid( "Group Member Navigation", "Allows you to switch between the other members of the groups of the configured group type.", "~/Blocks/Crm/PersonDetail/GroupMemberNavigation.ascx", "CRM > Person Detail", "35D091FA-8311-42D1-83F7-3E67B9EE9675" );
            
            // Attribute for BlockType
            //   BlockType: Group Member Navigation
            //   Category: CRM > Person Detail
            //   Attribute: Group Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D091FA-8311-42D1-83F7-3E67B9EE9675", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "Group Type", @"", 0, @"790E3215-3B10-442B-AF69-616C0DCB998E", "7AE4B36C-6EA7-4C4B-BA80-CB0F3BB2347C" );

            // Attribute for BlockType
            //   BlockType: Group Member Navigation
            //   Category: CRM > Person Detail
            //   Attribute: Show Only Primary Group Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D091FA-8311-42D1-83F7-3E67B9EE9675", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Primary Group Members", "ShowOnlyPrimaryGroupMembers", "Show Only Primary Group Members", @"", 0, @"False", "D54788BF-D769-4E14-842E-28D6613AC3BB" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
