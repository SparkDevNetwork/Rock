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
    public partial class GroupRequirements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupRequirement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        GroupRequirementTypeId = c.Int(nullable: false),
                        GroupRoleId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .ForeignKey("dbo.GroupRequirementType", t => t.GroupRequirementTypeId, cascadeDelete: true)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupRoleId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => new { t.GroupId, t.GroupRequirementTypeId, t.GroupRoleId }, unique: true, name: "IDX_GroupRequirementTypeGroup")
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.GroupRequirementType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        CanExpire = c.Boolean(nullable: false),
                        ExpireInDays = c.Int(),
                        RequirementCheckType = c.Int(nullable: false),
                        SqlExpression = c.String(),
                        DataViewId = c.Int(),
                        PositiveLabel = c.String(maxLength: 150),
                        NegativeLabel = c.String(maxLength: 150),
                        CheckboxLabel = c.String(maxLength: 150),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.DataViewId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.GroupMemberRequirement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupMemberId = c.Int(nullable: false),
                        GroupRequirementId = c.Int(nullable: false),
                        RequirementMetDateTime = c.DateTime(),
                        LastRequirementCheckDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupMember", t => t.GroupMemberId, cascadeDelete: true)
                .ForeignKey("dbo.GroupRequirement", t => t.GroupRequirementId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.GroupMemberId)
                .Index(t => t.GroupRequirementId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.Group", "MustMeetRequirementsToAddMember", c => c.Boolean());

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Requirement Types", "", "0EFB7285-5D70-4798-ADE9-908311ECC074", "fa fa-check-square-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "0EFB7285-5D70-4798-ADE9-908311ECC074", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Requirement Type Detail", "", "E64270AA-9246-4BA4-B1A9-EC2212F586DC", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Group Requirement Type Detail", "Displays the details of the given group requirement type for editing.", "~/Blocks/Groups/GroupRequirementTypeDetail.ascx", "Groups", "68FC983E-05F0-4067-83AC-97DD226F5071" );
            RockMigrationHelper.UpdateBlockType( "Group Requirement Type List", "List of Group Requirement Types", "~/Blocks/Groups/GroupRequirementTypeList.ascx", "Groups", "1270E3F7-5ACB-4044-94CD-E2B4368FF391" );
   
            // Add Block to Page: Group Requirement Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( "0EFB7285-5D70-4798-ADE9-908311ECC074", "", "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "Group Requirement Type List", "Main", "", "", 0, "98A9E817-CC9F-4766-A4E1-482062821979" );

            // Add Block to Page: Group Requirement Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "E64270AA-9246-4BA4-B1A9-EC2212F586DC", "", "68FC983E-05F0-4067-83AC-97DD226F5071", "Group Requirement Type Detail", "Main", "", "", 0, "97DF3504-2CFE-4C81-B3B6-B6781D1B3C35" );

            // Attrib for BlockType: Group Requirement Type List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "B5BAD3CB-357E-460B-B629-9C4DFAE0BC3D" );
   
            // Attrib Value for Block:Group Requirement Type List, Attribute:Detail Page Page: Group Requirement Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "98A9E817-CC9F-4766-A4E1-482062821979", "B5BAD3CB-357E-460B-B629-9C4DFAE0BC3D", @"e64270aa-9246-4ba4-b1a9-ec2212f586dc" );

            
            // Misc Round-ups
            RockMigrationHelper.UpdateLayout(Rock.SystemGuid.Site.SITE_ROCK_INTERNAL, "Error", "Error", "", "9433041E-FD96-4DD8-A60F-A641C48BED7D", false );
            RockMigrationHelper.UpdateLayout(Rock.SystemGuid.Site.SITE_ROCK_INTERNAL, "Homepage", "Homepage", "", "1A8A455E-619F-437E-A7BD-50C09B5B3576", false);

            RockMigrationHelper.UpdateBlockType( "Campus Context Setter - Device", "Block that can be used to set the campus context for the site based on the location of the device.", "~/Blocks/Core/CampusContextSetter.Device.ascx", "Core", "0B9C6253-C72E-4CAA-B38A-BA359BC712E4" );
            RockMigrationHelper.UpdateBlockType( "Transaction Entry - Kiosk", "Block used to process giving from a kiosk.", "~/Blocks/Finance/TransactionEntry.Kiosk.ascx", "Finance", "D10900A8-C2C1-4414-A443-3781A5CF371C" );
            RockMigrationHelper.UpdateBlockType( "Prayer Request Entry - Kiosk", "Allows prayer requests to be added from a kiosk.", "~/Blocks/Prayer/PrayerRequestEntry.Kiosk.ascx", "Prayer", "9D8ED334-F1F5-4377-9E27-B8C0852CF34D" );
            // Attrib for BlockType: Group Tree View:Show Filter Option to include Inactive Groups
            RockMigrationHelper.AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter Option to include Inactive Groups", "ShowFilterOption", "", "", 6, @"True", "4633ED5A-7A2C-4A78-B092-6733FED8CFA6" );

            RockMigrationHelper.UpdateFieldType( "Group And Role", "", "Rock", "Rock.Field.Types.GroupAndRoleFieldType", "202F32AD-75C0-4259-850A-D08C0F3D3080" );

            // Delete the old MarketingCampaign entities that we got rid of a long time ago
            try
            {
                Sql( @"
select Id into #deadEntityTypeIds from EntityType where Guid in (
'048CEB0E-0673-42C5-8935-2A06AD03B850', -- Marketing Campaign Ad
'772C19CE-FDDD-4057-BC81-9E359231E78E', -- Marketing Campaign
'2F1818BD-4BC8-4562-BD89-E17512C2F3A6', -- Marketing Campaign Ad Type
'60169E17-26E2-4C25-9AAD-3F96A874E372', -- Marketing Campaign Audience
'1CA3E384-92D9-4688-8B8D-31513449BCE0') -- Marketing Campaign Campus

delete from Audit where EntityTypeId in (select id from #deadEntityTypeIds)
delete from Auth where EntityTypeId in (select id from #deadEntityTypeIds)
delete from Attribute where EntityTypeId in (select id from #deadEntityTypeIds)
delete from EntityType where Id in  (select id from #deadEntityTypeIds)
drop table #deadEntityTypeIds" );
            }
            catch
            {
                // ignore if they can't be deleted
            }
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Requirement Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "B5BAD3CB-357E-460B-B629-9C4DFAE0BC3D" );
            // Remove Block: Group Requirement Type Detail, from Page: Group Requirement Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "97DF3504-2CFE-4C81-B3B6-B6781D1B3C35" );
            // Remove Block: Group Requirement Type List, from Page: Group Requirement Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "98A9E817-CC9F-4766-A4E1-482062821979" );
            
            RockMigrationHelper.DeleteBlockType( "1270E3F7-5ACB-4044-94CD-E2B4368FF391" ); // Group Requirement Type List
            RockMigrationHelper.DeleteBlockType( "68FC983E-05F0-4067-83AC-97DD226F5071" ); // Group Requirement Type Detail
            RockMigrationHelper.DeletePage( "E64270AA-9246-4BA4-B1A9-EC2212F586DC" ); //  Page: Group Requirement Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "0EFB7285-5D70-4798-ADE9-908311ECC074" ); //  Page: Group Requirement Types, Layout: Full Width, Site: Rock RMS
            
            DropForeignKey("dbo.GroupMemberRequirement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupMemberRequirement", "GroupRequirementId", "dbo.GroupRequirement");
            DropForeignKey("dbo.GroupMemberRequirement", "GroupMemberId", "dbo.GroupMember");
            DropForeignKey("dbo.GroupMemberRequirement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "GroupRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.GroupRequirement", "GroupRequirementTypeId", "dbo.GroupRequirementType");
            DropForeignKey("dbo.GroupRequirementType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirementType", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.GroupRequirementType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupRequirement", "GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupRequirement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.GroupMemberRequirement", new[] { "ForeignId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "Guid" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "GroupRequirementId" });
            DropIndex("dbo.GroupMemberRequirement", new[] { "GroupMemberId" });
            DropIndex("dbo.GroupRequirementType", new[] { "ForeignId" });
            DropIndex("dbo.GroupRequirementType", new[] { "Guid" });
            DropIndex("dbo.GroupRequirementType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupRequirementType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupRequirementType", new[] { "DataViewId" });
            DropIndex("dbo.GroupRequirement", new[] { "ForeignId" });
            DropIndex("dbo.GroupRequirement", new[] { "Guid" });
            DropIndex("dbo.GroupRequirement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupRequirement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupRequirement", "IDX_GroupRequirementTypeGroup");
            DropColumn("dbo.Group", "MustMeetRequirementsToAddMember");
            DropTable("dbo.GroupMemberRequirement");
            DropTable("dbo.GroupRequirementType");
            DropTable("dbo.GroupRequirement");
        }
    }
}
