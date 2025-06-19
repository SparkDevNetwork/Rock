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
    /// <summary>
    ///
    /// </summary>
    public partial class TempAddAgent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SchemaChangesUp();
            PagesAndBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesAndBlocksDown();
            SchemaChangesDown();
        }

        private void SchemaChangesUp()
        {
            CreateTable(
                "dbo.AIAgentSessionAnchor",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentSessionId = c.Int( nullable: false ),
                    AddedDateTime = c.DateTime( nullable: false ),
                    RemovedDateTime = c.DateTime(),
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                    Name = c.String( maxLength: 100 ),
                    IsActive = c.Boolean( nullable: false ),
                    LastRefreshedDateTime = c.DateTime( nullable: false ),
                    PayloadJson = c.String(),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgentSession", t => t.AIAgentSessionId, cascadeDelete: true )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .Index( t => t.AIAgentSessionId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgentSession",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentId = c.Int( nullable: false ),
                    PersonAliasId = c.Int( nullable: false ),
                    Name = c.String( maxLength: 100 ),
                    RelatedEntityTypeId = c.Int(),
                    RelatedEntityId = c.Int(),
                    StartDateTime = c.DateTime( nullable: false ),
                    LastMessageDateTime = c.DateTime( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgent", t => t.AIAgentId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.RelatedEntityTypeId, cascadeDelete: true )
                .Index( t => t.AIAgentId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.RelatedEntityTypeId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgent",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    AvatarBinaryFileId = c.Int(),
                    Persona = c.String(),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.BinaryFile", t => t.AvatarBinaryFileId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AvatarBinaryFileId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgentSkill",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentId = c.Int( nullable: false ),
                    AISkillId = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgent", t => t.AIAgentId, cascadeDelete: true )
                .ForeignKey( "dbo.AISkill", t => t.AISkillId, cascadeDelete: true )
                .Index( t => t.AIAgentId )
                .Index( t => t.AISkillId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AISkill",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    UsageHint = c.String(),
                    CodeEntityTypeId = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.EntityType", t => t.CodeEntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CodeEntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AISkillFunction",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AISkillId = c.Int( nullable: false ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    UsageHint = c.String(),
                    FunctionType = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AISkill", t => t.AISkillId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AISkillId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AIAgentSessionHistory",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AIAgentSessionId = c.Int( nullable: false ),
                    MessageRole = c.Int( nullable: false ),
                    MessageDateTime = c.DateTime( nullable: false ),
                    Message = c.String(),
                    IsCurrentlyInContext = c.Boolean( nullable: false ),
                    IsSummary = c.Boolean( nullable: false ),
                    TokenCount = c.Int( nullable: false ),
                    ConsumedTokenCount = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AIAgentSession", t => t.AIAgentSessionId, cascadeDelete: true )
                .Index( t => t.AIAgentSessionId )
                .Index( t => t.Guid, unique: true );
        }

        private void SchemaChangesDown()
        {
            DropForeignKey( "dbo.AIAgentSessionAnchor", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIAgentSessionAnchor", "AIAgentSessionId", "dbo.AIAgentSession" );
            DropForeignKey( "dbo.AIAgentSession", "RelatedEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIAgentSession", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgentSessionHistory", "AIAgentSessionId", "dbo.AIAgentSession" );
            DropForeignKey( "dbo.AIAgentSession", "AIAgentId", "dbo.AIAgent" );
            DropForeignKey( "dbo.AIAgent", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgent", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIAgent", "AvatarBinaryFileId", "dbo.BinaryFile" );
            DropForeignKey( "dbo.AIAgentSkill", "AISkillId", "dbo.AISkill" );
            DropForeignKey( "dbo.AISkill", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkill", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkill", "CodeEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AISkillFunction", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkillFunction", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AISkillFunction", "AISkillId", "dbo.AISkill" );
            DropForeignKey( "dbo.AIAgentSkill", "AIAgentId", "dbo.AIAgent" );
            DropIndex( "dbo.AIAgentSessionHistory", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSessionHistory", new[] { "AIAgentSessionId" } );
            DropIndex( "dbo.AISkillFunction", new[] { "Guid" } );
            DropIndex( "dbo.AISkillFunction", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AISkillFunction", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AISkillFunction", new[] { "AISkillId" } );
            DropIndex( "dbo.AISkill", new[] { "Guid" } );
            DropIndex( "dbo.AISkill", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AISkill", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AISkill", new[] { "CodeEntityTypeId" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "AISkillId" } );
            DropIndex( "dbo.AIAgentSkill", new[] { "AIAgentId" } );
            DropIndex( "dbo.AIAgent", new[] { "Guid" } );
            DropIndex( "dbo.AIAgent", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AIAgent", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AIAgent", new[] { "AvatarBinaryFileId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSession", new[] { "RelatedEntityTypeId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "PersonAliasId" } );
            DropIndex( "dbo.AIAgentSession", new[] { "AIAgentId" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "Guid" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "EntityTypeId" } );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "AIAgentSessionId" } );
            DropTable( "dbo.AIAgentSessionHistory" );
            DropTable( "dbo.AISkillFunction" );
            DropTable( "dbo.AISkill" );
            DropTable( "dbo.AIAgentSkill" );
            DropTable( "dbo.AIAgent" );
            DropTable( "dbo.AIAgentSession" );
            DropTable( "dbo.AIAgentSessionAnchor" );
        }

        private void PagesAndBlocksUp()
        {
            #region Add Pages

            // Add Page 
            //  Internal Name: AI Agents
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Agents", "", "9F7B9158-3A73-429A-A817-5909D2AED13C", "ti ti-robot-face" );

            // Add Page 
            //  Internal Name: AI Agent Providers
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "9F7B9158-3A73-429A-A817-5909D2AED13C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Agent Providers", "", "74B916C7-256A-42E1-8E4E-951450D23152", "ti ti-ai" );

            // Add Page 
            //  Internal Name: Agents
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "9F7B9158-3A73-429A-A817-5909D2AED13C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Agents", "", "DB33D4A6-C5C3-4CAE-B121-37588A513E29", "ti ti-robot" );

            // Add Page 
            //  Internal Name: AI Skills
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "9F7B9158-3A73-429A-A817-5909D2AED13C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Skills", "", "E1C14E52-9E06-4618-AB15-63261F9BA79B", "ti ti-tools" );

            // Add Page 
            //  Internal Name: AI Skill Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "E1C14E52-9E06-4618-AB15-63261F9BA79B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Skill Detail", "", "6F89544F-50C0-42D6-B925-FB6E404B434C", "" );

            // Add Page 
            //  Internal Name: AI Agent Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "DB33D4A6-C5C3-4CAE-B121-37588A513E29", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Agent Detail", "", "C7BCA1FB-B627-4A8C-8C9F-43AE69FA69FC", "" );

            #endregion

            #region Add Block Types

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AIAgentDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AIAgentDetail", "AI Agent Detail", "Rock.Blocks.Core.AIAgentDetail, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "45AE85C7-1370-4C59-8F98-E1B0E268E54D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AIAgentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AIAgentList", "AI Agent List", "Rock.Blocks.Core.AIAgentList, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "35F161D4-DE63-49EF-AF8A-B67CD7A755C6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AISkillDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AISkillDetail", "AI Skill Detail", "Rock.Blocks.Core.AISkillDetail, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "A152A125-876C-4D0B-87B6-9A99CEC8DB51" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AISkillFunctionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AISkillFunctionList", "AI Skill Function List", "Rock.Blocks.Core.AISkillFunctionList, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "FB397310-6BCB-49CD-9CCB-3506046CB14B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AISkillList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AISkillList", "AI Skill List", "Rock.Blocks.Core.AISkillList, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "8C7DAF4B-DB53-438A-84C8-4296BA17473B" );

            // Add/Update Obsidian Block Type
            //   Name:AI Agent Detail
            //   Category:Core > AI
            //   EntityType:Rock.Blocks.Core.AIAgentDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Agent Detail", "Displays the details of a particular ai agent.", "Rock.Blocks.Core.AIAgentDetail", "Core > AI", "D898E9CE-FE9B-48F7-96BF-2D69DE3C8E7C" );

            // Add/Update Obsidian Block Type
            //   Name:AI Agent List
            //   Category:Core > AI
            //   EntityType:Rock.Blocks.Core.AIAgentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Agent List", "Displays a list of ai agents.", "Rock.Blocks.Core.AIAgentList", "Core > AI", "4831074F-7B99-404E-B842-776B74765DE5" );

            // Add/Update Obsidian Block Type
            //   Name:AI Skill Detail
            //   Category:Core > AI
            //   EntityType:Rock.Blocks.Core.AISkillDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Skill Detail", "Displays the details of a particular ai skill.", "Rock.Blocks.Core.AISkillDetail", "Core > AI", "B8B8CEE9-C058-45D3-A1C7-647CAD96FD1E" );

            // Add/Update Obsidian Block Type
            //   Name:AI Skill Function List
            //   Category:Core > AI
            //   EntityType:Rock.Blocks.Core.AISkillFunctionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Skill Function List", "Displays a list of ai skill functions.", "Rock.Blocks.Core.AISkillFunctionList", "Core > AI", "1E257602-9C31-4F6C-A362-67912F06E807" );

            // Add/Update Obsidian Block Type
            //   Name:AI Skill List
            //   Category:Core > AI
            //   EntityType:Rock.Blocks.Core.AISkillList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Skill List", "Displays a list of ai skills.", "Rock.Blocks.Core.AISkillList", "Core > AI", "39F5C953-0080-441F-A77C-D45676147F91" );

            #endregion

            #region Add Blocks

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: AI Agents
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9F7B9158-3A73-429A-A817-5909D2AED13C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 0, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26" );

            // Add Block 
            //  Block Name: Components
            //  Page Name: AI Agent Providers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "74B916C7-256A-42E1-8E4E-951450D23152".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "Components", "Main", @"", @"", 0, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5" );

            // Add Block 
            //  Block Name: AI Skill List
            //  Page Name: AI Skills
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E1C14E52-9E06-4618-AB15-63261F9BA79B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "39F5C953-0080-441F-A77C-D45676147F91".AsGuid(), "AI Skill List", "Main", @"", @"", 0, "D97B50C4-3A28-47E7-B7E6-D58E3A5F7D04" );

            // Add Block 
            //  Block Name: AI Agent Detail
            //  Page Name: AI Agent Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C7BCA1FB-B627-4A8C-8C9F-43AE69FA69FC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D898E9CE-FE9B-48F7-96BF-2D69DE3C8E7C".AsGuid(), "AI Agent Detail", "Main", @"", @"", 0, "06095F93-031A-4ACA-9B05-BD3A0B714C16" );

            // Add Block 
            //  Block Name: AI Skill Function List
            //  Page Name: AI Skill Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F89544F-50C0-42D6-B925-FB6E404B434C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "1E257602-9C31-4F6C-A362-67912F06E807".AsGuid(), "AI Skill Function List", "Main", @"", @"", 1, "2F2014B0-D7F4-4572-AED0-D9BBDB3699A1" );

            // Add Block 
            //  Block Name: AI Skill Detail
            //  Page Name: AI Skill Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6F89544F-50C0-42D6-B925-FB6E404B434C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B8B8CEE9-C058-45D3-A1C7-647CAD96FD1E".AsGuid(), "AI Skill Detail", "Main", @"", @"", 0, "DC3C8C25-B28D-4638-A553-3AE7D27BB68A" );

            // Add Block 
            //  Block Name: AI Agent List
            //  Page Name: Agents
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DB33D4A6-C5C3-4CAE-B121-37588A513E29".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4831074F-7B99-404E-B842-776B74765DE5".AsGuid(), "AI Agent List", "Main", @"", @"", 0, "4AC03044-6B6C-4C4C-8AFC-1A019BF23493" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: AI Skill Detail,  Zone: Main,  Block: AI Skill Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'DC3C8C25-B28D-4638-A553-3AE7D27BB68A'" );

            // Update Order for Page: AI Skill Detail,  Zone: Main,  Block: AI Skill Function List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '2F2014B0-D7F4-4572-AED0-D9BBDB3699A1'" );

            #endregion

            #region Add Block Type Attributes

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4831074F-7B99-404E-B842-776B74765DE5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the ai agent details.", 0, @"", "2E7CA3FC-13F0-4E0B-96F7-C3F84754210D" );

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4831074F-7B99-404E-B842-776B74765DE5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6E9C1391-A6DB-4BD0-B612-E341C5F10C69" );

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4831074F-7B99-404E-B842-776B74765DE5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D7E41098-480B-49AA-9B57-A31827E36B99" );

            // Attribute for BlockType
            //   BlockType: AI Skill Function List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1E257602-9C31-4F6C-A362-67912F06E807", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F28EE03F-E97E-42A3-802B-E507514B812B" );

            // Attribute for BlockType
            //   BlockType: AI Skill Function List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1E257602-9C31-4F6C-A362-67912F06E807", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5CF1DC60-C1B7-4E9E-AE9A-D6355882FF60" );

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39F5C953-0080-441F-A77C-D45676147F91", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the ai skill details.", 0, @"", "EA204DF2-5EDE-49B1-A3C7-6C2E52659BAD" );

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39F5C953-0080-441F-A77C-D45676147F91", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DADEEC8E-23A7-4B1D-AE10-2270200F36F0" );

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39F5C953-0080-441F-A77C-D45676147F91", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D2F118FE-17FC-41BB-8CF3-9A6C03DC86D0" );

            #endregion

            #region Add Block Attribute Values

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=AI Agents, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=AI Agents, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsBlocks.lava' %} */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=AI Agents, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=AI Agents, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=AI Agents, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Components
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Agent Providers, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );

            // Add Block Attribute Value
            //   Block: Components
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Agent Providers, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5", "63B3F343-0533-4C60-AA10-1EE4ADD30E45", @"False" );

            // Add Block Attribute Value
            //   Block: Components
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Agent Providers, Site=Rock RMS
            //   Attribute: Component Container
            /*   Attribute Value: Rock.AI.Agent.AgentProviderContainer, Rock */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.AI.Agent.AgentProviderContainer, Rock" );

            // Add Block Attribute Value
            //   Block: Components
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Agent Providers, Site=Rock RMS
            //   Attribute: Support Ordering
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5", "A4889D7B-87AA-419D-846C-3E618E79D875", @"True" );

            // Add Block Attribute Value
            //   Block: Components
            //   BlockType: Components
            //   Category: Core
            //   Block Location: Page=AI Agent Providers, Site=Rock RMS
            //   Attribute: Support Security
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"False" );

            // Add Block Attribute Value
            //   Block: AI Skill List
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Block Location: Page=AI Skills, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 6f89544f-50c0-42d6-b925-fb6e404b434c */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D97B50C4-3A28-47E7-B7E6-D58E3A5F7D04", "EA204DF2-5EDE-49B1-A3C7-6C2E52659BAD", @"6f89544f-50c0-42d6-b925-fb6e404b434c" );

            // Add Block Attribute Value
            //   Block: AI Skill List
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Block Location: Page=AI Skills, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D97B50C4-3A28-47E7-B7E6-D58E3A5F7D04", "48F68111-2516-4F19-90D8-5F7A92A8A40C", @"False" );

            // Add Block Attribute Value
            //   Block: AI Skill List
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Block Location: Page=AI Skills, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D97B50C4-3A28-47E7-B7E6-D58E3A5F7D04", "D2F118FE-17FC-41BB-8CF3-9A6C03DC86D0", @"True" );

            // Add Block Attribute Value
            //   Block: AI Agent List
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Block Location: Page=Agents, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: c7bca1fb-b627-4a8c-8c9f-43ae69fa69fc */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4AC03044-6B6C-4C4C-8AFC-1A019BF23493", "2E7CA3FC-13F0-4E0B-96F7-C3F84754210D", @"c7bca1fb-b627-4a8c-8c9f-43ae69fa69fc" );

            // Add Block Attribute Value
            //   Block: AI Agent List
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Block Location: Page=Agents, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4AC03044-6B6C-4C4C-8AFC-1A019BF23493", "B5B4F5A5-08BD-4623-BE08-446A20DC0804", @"False" );

            // Add Block Attribute Value
            //   Block: AI Agent List
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Block Location: Page=Agents, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4AC03044-6B6C-4C4C-8AFC-1A019BF23493", "D7E41098-480B-49AA-9B57-A31827E36B99", @"True" );

            #endregion
        }

        private void PagesAndBlocksDown()
        {
            #region Delete Block Type Attributes

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D2F118FE-17FC-41BB-8CF3-9A6C03DC86D0" );

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "DADEEC8E-23A7-4B1D-AE10-2270200F36F0" );

            // Attribute for BlockType
            //   BlockType: AI Skill List
            //   Category: Core > AI
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EA204DF2-5EDE-49B1-A3C7-6C2E52659BAD" );

            // Attribute for BlockType
            //   BlockType: AI Skill Function List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5CF1DC60-C1B7-4E9E-AE9A-D6355882FF60" );

            // Attribute for BlockType
            //   BlockType: AI Skill Function List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F28EE03F-E97E-42A3-802B-E507514B812B" );

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "D7E41098-480B-49AA-9B57-A31827E36B99" );

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6E9C1391-A6DB-4BD0-B612-E341C5F10C69" );

            // Attribute for BlockType
            //   BlockType: AI Agent List
            //   Category: Core > AI
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "2E7CA3FC-13F0-4E0B-96F7-C3F84754210D" );

            #endregion

            #region Delete Blocks

            // Remove Block
            //  Name: AI Agent Detail, from Page: AI Agent Detail, Site: Rock RMS
            //  from Page: AI Agent Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "06095F93-031A-4ACA-9B05-BD3A0B714C16" );

            // Remove Block
            //  Name: AI Agent List, from Page: Agents, Site: Rock RMS
            //  from Page: Agents, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4AC03044-6B6C-4C4C-8AFC-1A019BF23493" );

            // Remove Block
            //  Name: AI Skill Function List, from Page: AI Skill Detail, Site: Rock RMS
            //  from Page: AI Skill Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2F2014B0-D7F4-4572-AED0-D9BBDB3699A1" );

            // Remove Block
            //  Name: AI Skill Detail, from Page: AI Skill Detail, Site: Rock RMS
            //  from Page: AI Skill Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DC3C8C25-B28D-4638-A553-3AE7D27BB68A" );

            // Remove Block
            //  Name: AI Skill List, from Page: AI Skills, Site: Rock RMS
            //  from Page: AI Skills, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D97B50C4-3A28-47E7-B7E6-D58E3A5F7D04" );

            // Remove Block
            //  Name: Components, from Page: AI Agent Providers, Site: Rock RMS
            //  from Page: AI Agent Providers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3DEA1CA1-AF6D-476E-89F7-CF4AF72105D5" );

            // Remove Block
            //  Name: Page Menu, from Page: AI Agents, Site: Rock RMS
            //  from Page: AI Agents, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8A8558A0-AE39-47F0-A5B6-4CD5DF5A9B26" );

            #endregion

            #region Delete Block Types

            // Delete BlockType 
            //   Name: AI Skill List
            //   Category: Core > AI
            //   Path: -
            //   EntityType: AI Skill List
            RockMigrationHelper.DeleteBlockType( "39F5C953-0080-441F-A77C-D45676147F91" );

            // Delete BlockType 
            //   Name: AI Skill Function List
            //   Category: Core > AI
            //   Path: -
            //   EntityType: AI Skill Function List
            RockMigrationHelper.DeleteBlockType( "1E257602-9C31-4F6C-A362-67912F06E807" );

            // Delete BlockType 
            //   Name: AI Skill Detail
            //   Category: Core > AI
            //   Path: -
            //   EntityType: AI Skill Detail
            RockMigrationHelper.DeleteBlockType( "B8B8CEE9-C058-45D3-A1C7-647CAD96FD1E" );

            // Delete BlockType 
            //   Name: AI Agent List
            //   Category: Core > AI
            //   Path: -
            //   EntityType: AI Agent List
            RockMigrationHelper.DeleteBlockType( "4831074F-7B99-404E-B842-776B74765DE5" );

            // Delete BlockType 
            //   Name: AI Agent Detail
            //   Category: Core > AI
            //   Path: -
            //   EntityType: AI Agent Detail
            RockMigrationHelper.DeleteBlockType( "D898E9CE-FE9B-48F7-96BF-2D69DE3C8E7C" );

            #endregion

            #region Delete Pages

            // Delete Page 
            //  Internal Name: AI Agent Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "C7BCA1FB-B627-4A8C-8C9F-43AE69FA69FC" );


            // Delete Page 
            //  Internal Name: AI Skill Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "6F89544F-50C0-42D6-B925-FB6E404B434C" );


            // Delete Page 
            //  Internal Name: AI Skills
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "E1C14E52-9E06-4618-AB15-63261F9BA79B" );


            // Delete Page 
            //  Internal Name: Agents
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "DB33D4A6-C5C3-4CAE-B121-37588A513E29" );


            // Delete Page 
            //  Internal Name: AI Agent Providers
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "74B916C7-256A-42E1-8E4E-951450D23152" );


            // Delete Page 
            //  Internal Name: AI Agents
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "9F7B9158-3A73-429A-A817-5909D2AED13C" );

            #endregion
        }
    }
}
