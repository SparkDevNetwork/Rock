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
    public partial class GroupRequirementsGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // GroupType GroupRequirement
            DropIndex( "dbo.GroupRequirement", "IDX_GroupRequirementTypeGroup");
            AddColumn("dbo.GroupRequirement", "GroupTypeId", c => c.Int());
            AddColumn("dbo.GroupRequirement", "MustMeetRequirementToAddMember", c => c.Boolean(nullable: false));
            AlterColumn("dbo.GroupRequirement", "GroupId", c => c.Int());
            CreateIndex("dbo.GroupRequirement", new[] { "GroupId", "GroupTypeId", "GroupRequirementTypeId", "GroupRoleId" }, unique: true, name: "IDX_GroupRequirementTypeGroup");
            AddForeignKey("dbo.GroupRequirement", "GroupTypeId", "dbo.GroupType", "Id");

            Sql( @"UPDATE gr
SET gr.MustMeetRequirementToAddMember = isnull(g.MustMeetRequirementsToAddMember, 0)
FROM GroupRequirement gr
INNER JOIN [Group] g ON gr.GroupId = g.Id" );

            DropColumn("dbo.Group", "MustMeetRequirementsToAddMember");

            // MP: Updated StoredProcs for GroupMember.GroupOrder
            Sql( MigrationSQL._201706060019549_GroupRequirementsGroupType_ufnCrm_GetAddress );
            Sql( MigrationSQL._201706060019549_GroupRequirementsGroupType_ufnCrm_GetSpousePersonIdFromPersonId );

            // DT: Add Copy Button for Security Roles
            // Attrib for BlockType: Group Detail:Show Copy Button
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Copy Button", "ShowCopyButton", "", "Copies the group and all of its associated authorization rules", 10, @"False", "944FE68B-AE18-40A1-8F0C-6547BCFF1E7E" );

            // Attrib Value for Block:Security Roles Detail, Attribute:Show Copy Button Page: Security Roles Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "944FE68B-AE18-40A1-8F0C-6547BCFF1E7E", @"True" );

            // DT: Add Communication Queue Block (up)
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Queue", "", "285ED8C0-0471-4503-95DE-A8E3F179206C", "fa fa-comments-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Communication Queue", "Lists the status of all communications.", "~/Blocks/Communication/CommunicationQueue.ascx", "Communication", "694EB2F6-018D-4E99-A956-202B1FAF7FB9" );
            // Add Block to Page: Communication Queue, Site: Rock RMS
            RockMigrationHelper.AddBlock( "285ED8C0-0471-4503-95DE-A8E3F179206C", "", "694EB2F6-018D-4E99-A956-202B1FAF7FB9", "Communication Queue", "Main", @"", @"", 0, "D85ACF7C-DAFF-40A6-8938-1A0B94AC120A" );
            // Attrib for BlockType: Communication Queue:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "694EB2F6-018D-4E99-A956-202B1FAF7FB9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "A4E3E200-2784-4031-962A-41E31446ED8E" );
            // Attrib Value for Block:Communication Queue, Attribute:Detail Page Page: Communication Queue, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D85ACF7C-DAFF-40A6-8938-1A0B94AC120A", "A4E3E200-2784-4031-962A-41E31446ED8E", @"2a22d08d-73a8-4aaf-ac7e-220e8b2e7857,79c0c1a7-41b6-4b40-954d-660a4b39b8ce" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // DT: Add Communication Queue Block (down)
            // Attrib for BlockType: Communication Queue:Detail Page
            RockMigrationHelper.DeleteAttribute( "A4E3E200-2784-4031-962A-41E31446ED8E" );
            // Remove Block: Communication Queue, from Page: Communication Queue, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D85ACF7C-DAFF-40A6-8938-1A0B94AC120A" );
            RockMigrationHelper.DeleteBlockType( "694EB2F6-018D-4E99-A956-202B1FAF7FB9" ); // Communication Queue
            RockMigrationHelper.DeletePage( "285ED8C0-0471-4503-95DE-A8E3F179206C" ); //  Page: Communication Queue, Layout: Full Width, Site: Rock RMS	

            // GroupType GroupRequirement
            AddColumn( "dbo.Group", "MustMeetRequirementsToAddMember", c => c.Boolean());
            DropForeignKey("dbo.GroupRequirement", "GroupTypeId", "dbo.GroupType");
            DropIndex("dbo.GroupRequirement", "IDX_GroupRequirementTypeGroup");
            AlterColumn("dbo.GroupRequirement", "GroupId", c => c.Int(nullable: false));
            DropColumn("dbo.GroupRequirement", "MustMeetRequirementToAddMember");
            DropColumn("dbo.GroupRequirement", "GroupTypeId");
            CreateIndex("dbo.GroupRequirement", new[] { "GroupId", "GroupRequirementTypeId", "GroupRoleId" }, unique: true, name: "IDX_GroupRequirementTypeGroup");
        }
    }
}
