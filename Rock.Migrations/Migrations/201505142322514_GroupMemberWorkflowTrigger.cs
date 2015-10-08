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
    public partial class GroupMemberWorkflowTrigger : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupMemberWorkflowTrigger",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(nullable: false),
                        GroupTypeId = c.Int(),
                        GroupId = c.Int(),
                        Name = c.String(maxLength: 100),
                        WorkflowTypeId = c.Int(nullable: false),
                        TriggerType = c.Int(nullable: false),
                        TypeQualifier = c.String(maxLength: 200),
                        WorkflowName = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.WorkflowTypeId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);

            Sql( @"
    UPDATE [Block] SET [Zone] = 'SectionA1'
	WHERE [Guid] = 'EF8BB598-E991-421F-96A1-3019B3D855A6'
	
    DECLARE @PageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'B1CA86DC-9890-4D26-8EBD-488044E1B3DD')
    IF @PageId IS NOT NULL
    BEGIN
        INSERT INTO [PageRoute]
	        ([IsSystem], [PageId], [Route], [Guid])
	    VALUES
	        (1, @PageId, 'AddTransaction', '0F854BB9-0044-1E94-48BD-B794E93E39FA')
    END
" );

            // Add Block to Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Transaction Links", "SectionA2", "", "", 0, "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A" );

            // Add/Update HtmlContent for Block: Transaction Links
            RockMigrationHelper.UpdateHtmlContentBlock( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", @"<div class=""panel panel-block"">
    
    <div class=""panel-body"">
        <a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">Add One-time Gift</a>
        <a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">New Scheduled Transaction</a>
    </div>
    
</div>", "B20065F8-2FC9-4BE2-B0B3-7E8612393AAE" );

            // Attrib Value for Block:Transaction Links, Attribute:Cache Duration Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Transaction Links, Attribute:Require Approval Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Transaction Links, Attribute:Enable Versioning Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Transaction Links, Attribute:Context Parameter Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Transaction Links, Attribute:Context Name Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Attrib Value for Block:Transaction Links, Attribute:Use Code Editor Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Transaction Links, Attribute:Image Root Folder Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Transaction Links, Attribute:User Specific Folders Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Transaction Links, Attribute:Document Root Folder Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Transaction Links, Attribute:Enable Debug Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [Block] SET
	[Zone] = 'SectionC1'
	WHERE [Guid] = 'EF8BB598-E991-421F-96A1-3019B3D855A6'
" );

            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] = '0F854BB9-0044-1E94-48BD-B794E93E39FA')" );

            // Remove Block: Transaction Links, from Page: Contributions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A" );

            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "WorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.GroupMemberWorkflowTrigger", "GroupId", "dbo.Group");
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "ForeignId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "Guid" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "WorkflowTypeId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "GroupId" });
            DropIndex("dbo.GroupMemberWorkflowTrigger", new[] { "GroupTypeId" });
            DropTable("dbo.GroupMemberWorkflowTrigger");
        }
    }
}
