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
    public partial class AddAutomationTriggersAndEvents : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AutomationEvent",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AutomationTriggerId = c.Int( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    ComponentEntityTypeId = c.Int( nullable: false ),
                    ComponentConfigurationJson = c.String(),
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
                .ForeignKey( "dbo.AutomationTrigger", t => t.AutomationTriggerId, cascadeDelete: true )
                .ForeignKey( "dbo.EntityType", t => t.ComponentEntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.AutomationTriggerId )
                .Index( t => t.ComponentEntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AutomationTrigger",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    ComponentEntityTypeId = c.Int( nullable: false ),
                    ComponentConfigurationJson = c.String(),
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
                .ForeignKey( "dbo.EntityType", t => t.ComponentEntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.Name, unique: true )
                .Index( t => t.ComponentEntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Add Page 
            //  Internal Name: Automations
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Automations", "", "C8A89678-B4D0-42B7-8F63-EE3D996E61C2", "ti ti-automation" );

            // Add Page 
            //  Internal Name: Automation Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C8A89678-B4D0-42B7-8F63-EE3D996E61C2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Automation Detail", "", "1DC99D2D-B206-47EC-8AB8-3A9AB84BA549", "" );

            // Add Page Route
            //   Page:Automations
            //   Route:admin/general/automations
            RockMigrationHelper.AddOrUpdatePageRoute( "C8A89678-B4D0-42B7-8F63-EE3D996E61C2", "admin/general/automations", "8554E83E-8FFA-41A3-A824-1D6694AB6884" );

            // Add Page Route
            //   Page:Automation Detail
            //   Route:admin/general/automations/{AutomationTriggerId}
            RockMigrationHelper.AddOrUpdatePageRoute( "1DC99D2D-B206-47EC-8AB8-3A9AB84BA549", "admin/general/automations/{AutomationTriggerId}", "FA67182A-9EF6-4147-B533-631C7A4D5C6A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AutomationTriggerDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AutomationTriggerDetail", "Automation Trigger Detail", "Rock.Blocks.Core.AutomationTriggerDetail, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "D23D35DB-2EEE-4301-BB3B-21AE0AA7987F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AutomationTriggerList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AutomationTriggerList", "Automation Trigger List", "Rock.Blocks.Core.AutomationTriggerList, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "0531E315-31CF-4BFB-A732-A40139F82346" );

            // Add/Update Obsidian Block Type
            //   Name:Automation Trigger Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AutomationTriggerDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Automation Trigger Detail", "Displays the details of a particular automation trigger.", "Rock.Blocks.Core.AutomationTriggerDetail", "Core", "A4A91333-9FF7-4E93-B9AE-15DAAF7AE185" );

            // Add/Update Obsidian Block Type
            //   Name:Automation Trigger List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AutomationTriggerList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Automation Trigger List", "Displays a list of automation triggers.", "Rock.Blocks.Core.AutomationTriggerList", "Core", "05606726-F878-4675-BD25-7E8D5D9E445D" );

            // Add Block 
            //  Block Name: Automation Trigger List
            //  Page Name: Automations
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C8A89678-B4D0-42B7-8F63-EE3D996E61C2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "05606726-F878-4675-BD25-7E8D5D9E445D".AsGuid(), "Automation Trigger List", "Main", @"", @"", 0, "9CA314BE-4AF1-4F8A-A6D0-F9F2343682E5" );

            // Add Block 
            //  Block Name: Automation Trigger Detail
            //  Page Name: Automation Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1DC99D2D-B206-47EC-8AB8-3A9AB84BA549".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A4A91333-9FF7-4E93-B9AE-15DAAF7AE185".AsGuid(), "Automation Trigger Detail", "Main", @"", @"", 0, "5034698E-30FC-4B11-849D-A665BD600D2F" ); 
            
            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "05606726-F878-4675-BD25-7E8D5D9E445D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the automation trigger details.", 0, @"", "C68FB45F-253B-43A1-B95B-9BF56CF6FB4B" );

            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "05606726-F878-4675-BD25-7E8D5D9E445D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "678D6E62-629E-43C6-B379-D169A25CC385" );

            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "05606726-F878-4675-BD25-7E8D5D9E445D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "ACD521A2-E7C7-4D94-BE65-3802786914D6" );

            // Add Block Attribute Value
            //   Block: Automation Trigger List
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Block Location: Page=Automations, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 1dc99d2d-b206-47ec-8ab8-3a9ab84ba549 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "9CA314BE-4AF1-4F8A-A6D0-F9F2343682E5", "C68FB45F-253B-43A1-B95B-9BF56CF6FB4B", @"1dc99d2d-b206-47ec-8ab8-3a9ab84ba549" );

            // Add Block Attribute Value
            //   Block: Automation Trigger List
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Block Location: Page=Automations, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "9CA314BE-4AF1-4F8A-A6D0-F9F2343682E5", "816065B5-B292-4E6C-8B76-4EB2C5FA7528", @"False" );

            // Add Block Attribute Value
            //   Block: Automation Trigger List
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Block Location: Page=Automations, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "9CA314BE-4AF1-4F8A-A6D0-F9F2343682E5", "ACD521A2-E7C7-4D94-BE65-3802786914D6", @"True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "ACD521A2-E7C7-4D94-BE65-3802786914D6" );

            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "678D6E62-629E-43C6-B379-D169A25CC385" );

            // Attribute for BlockType
            //   BlockType: Automation Trigger List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "C68FB45F-253B-43A1-B95B-9BF56CF6FB4B" );

            // Remove Block
            //  Name: Automation Trigger Detail, from Page: Automation Detail, Site: Rock RMS
            //  from Page: Automation Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5034698E-30FC-4B11-849D-A665BD600D2F" );

            // Remove Block
            //  Name: Automation Trigger List, from Page: Automations, Site: Rock RMS
            //  from Page: Automations, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9CA314BE-4AF1-4F8A-A6D0-F9F2343682E5" );

            // Delete BlockType 
            //   Name: Automation Trigger List
            //   Category: Core
            //   Path: -
            //   EntityType: Automation Trigger List
            RockMigrationHelper.DeleteBlockType( "05606726-F878-4675-BD25-7E8D5D9E445D" );

            // Delete BlockType 
            //   Name: Automation Trigger Detail
            //   Category: Core
            //   Path: -
            //   EntityType: Automation Trigger Detail
            RockMigrationHelper.DeleteBlockType( "A4A91333-9FF7-4E93-B9AE-15DAAF7AE185" );

            // Delete Page 
            //  Internal Name: Automation Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "1DC99D2D-B206-47EC-8AB8-3A9AB84BA549" );

            // Delete Page 
            //  Internal Name: Automations
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "C8A89678-B4D0-42B7-8F63-EE3D996E61C2" );

            DropForeignKey( "dbo.AutomationEvent", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AutomationEvent", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AutomationEvent", "ComponentEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AutomationEvent", "AutomationTriggerId", "dbo.AutomationTrigger" );
            DropForeignKey( "dbo.AutomationTrigger", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AutomationTrigger", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AutomationTrigger", "ComponentEntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.AutomationTrigger", new[] { "Guid" } );
            DropIndex( "dbo.AutomationTrigger", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AutomationTrigger", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AutomationTrigger", new[] { "ComponentEntityTypeId" } );
            DropIndex( "dbo.AutomationTrigger", new[] { "Name" } );
            DropIndex( "dbo.AutomationEvent", new[] { "Guid" } );
            DropIndex( "dbo.AutomationEvent", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AutomationEvent", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AutomationEvent", new[] { "ComponentEntityTypeId" } );
            DropIndex( "dbo.AutomationEvent", new[] { "AutomationTriggerId" } );
            DropTable( "dbo.AutomationTrigger" );
            DropTable( "dbo.AutomationEvent" );
        }
    }
}
