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
    public partial class AddMySettingsPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "My Settings", "", "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "fa fa-sliders" ); // Site:Rock RMS

            // Add Block to Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page List", "Main", "", "", 0, "A795BE67-F69B-4D7B-91E1-4F0883F1B718" );

            // Attrib for BlockType: Login Status:My Settings Page
            RockMigrationHelper.AddBlockTypeAttribute( "04712F3D-9667-4901-A49D-4507573EF7AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Settings Page", "MySettingsPage", "", "Page for user to view their settings (if blank option will not be displayed)", 0, @"", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0" );

            // Attrib Value for Block:Login Status, Attribute:My Settings Page , Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "82AF461F-022D-4ADB-BB12-F220CD605459", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );

            // Attrib Value for Block:Login Status, Attribute:My Settings Page , Layout: Left Sidebar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "791A6AA0-D498-4795-BB5F-21609175826F", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );

            // Attrib Value for Block:Login Status, Attribute:My Settings Page , Layout: Person Profile, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "19C2140D-498A-4675-B8A2-18B281736F6E", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );

            // Attrib Value for Block:Login Status, Attribute:My Settings Page , Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2356DEDC-803F-4782-A8E9-D0D88393EC2E", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );

            // Attrib Value for Block:Page List, Attribute:Enable Debug Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page List, Attribute:Is Secondary Block Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page List, Attribute:Number of Levels Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page List, Attribute:Include Current QueryString Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page List, Attribute:Template Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Attrib Value for Block:Page List, Attribute:Root Page Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"cf54e680-2e02-4f16-b54b-a2f2d29cd932" );

            // Attrib Value for Block:Page List, Attribute:CSS File Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page List, Attribute:Include Current Parameters Page: My Settings, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A795BE67-F69B-4D7B-91E1-4F0883F1B718", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Add/Update PageContext for Page:Workflow Configuration, Entity: Rock.Model.WorkflowType, Parameter: WorkflowTypeId
            RockMigrationHelper.UpdatePageContext( "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A", "Rock.Model.WorkflowType", "WorkflowTypeId", "E904932A-4551-4A5A-B6BF-EF60AD8E90E6" );

            //
            // Custom changes

            // hide settings page from the menu
            Sql( @"UPDATE [Page]	
	            SET [DisplayInNavWhen] = 3
	            WHERE [Guid] = 'CF54E680-2E02-4F16-B54B-A2F2D29CD932'" );

            // remove My Account links from internal menu
            Sql( @"
                    DECLARE @MyAccountAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'MyAccountPage')
                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @MyAccountAttributeId" );

            // move pages under my settings
            Sql( @" DECLARE @MySettingsPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'CF54E680-2E02-4F16-B54B-A2F2D29CD932')
                    
                    -- move communication templates page
                    UPDATE [Page] 
                    SET [ParentPageId] = @MySettingsPageId
	                    , [Order] = 1
                    WHERE [Guid] = 'EA611245-7A5E-4995-A3C6-EB97C6FD7C8D'

                    -- move change password page
                    UPDATE [Page] 
                    SET [ParentPageId] = @MySettingsPageId
	                    , [Order] = 0
                    WHERE [Guid] = '4508223C-2989-4592-B764-B3F372B6051B'

                    -- blank out the login controls my account setting
                    DECLARE @MyAccountAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'MyAccountPage')
                    DELETE FROM [AttributeValue] WHERE [AttributeId] = @MyAccountAttributeId" );

            // add icon to the change password page
            Sql( @"UPDATE [Page]
	                SET [IconCssClass] = 'fa fa-shield'
	                WHERE [Guid] = '4508223C-2989-4592-B764-B3F372B6051B'" );

            // add my account setting to external layouts
            Sql( @"

            DECLARE @MyAccountAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'MyAccountPage')

            DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '5A5C6063-EA0D-4EDD-A394-4B1B772F2041')
            IF @BlockId IS NOT NULL
            BEGIN
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		        VALUES (0, @MyAccountAttributeId, @BlockId, 'c0854f84-2e8b-479c-a3fb-6b47be89b795,5ed1e22a-2f7e-43ef-b8f4-3c7bd7f3fcf9', 'e842877b-5f3a-758a-4ea5-509c5145648a')
            END

            SET @BlockId = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'AD5D5155-D5AC-4445-A2C1-C4E8DC6CF23E')
            IF @BlockId IS NOT NULL
            BEGIN
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		        VALUES (0, @MyAccountAttributeId, @BlockId, 'c0854f84-2e8b-479c-a3fb-6b47be89b795,5ed1e22a-2f7e-43ef-b8f4-3c7bd7f3fcf9', '90c8d264-af2b-90af-4ce3-b2a808247c0d')
            END

            SET @BlockId = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '5CE3D668-85BF-4B3F-91BE-AB4BF8BA24B9')
            IF @BlockId IS NOT NULL
            BEGIN
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		        VALUES (0, @MyAccountAttributeId, @BlockId, 'c0854f84-2e8b-479c-a3fb-6b47be89b795,5ed1e22a-2f7e-43ef-b8f4-3c7bd7f3fcf9', '4b6badc8-d450-8880-4a06-bfc8132a5d20')
            END

            SET @BlockId = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'ED8662A3-44A9-4181-8D0B-55087E1062D1')
            IF @BlockId IS NOT NULL
            BEGIN
                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		        VALUES (0, @MyAccountAttributeId, @BlockId, 'c0854f84-2e8b-479c-a3fb-6b47be89b795,5ed1e22a-2f7e-43ef-b8f4-3c7bd7f3fcf9', '16cd398f-34e4-cf91-432f-6986310fbe5b')
            END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // move pages back home
            Sql( @"  DECLARE @ToolsComPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '7F79E512-B9DB-4780-9887-AD6D63A39050')
                    DECLARE @SupportPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '936C90C4-29CF-4665-A489-7C687217F7B8')
                    
                    -- move communication templates page
                    UPDATE [Page] 
                    SET [ParentPageId] = @ToolsComPageId
	                    , [Order] = 4
                    WHERE [Guid] = 'EA611245-7A5E-4995-A3C6-EB97C6FD7C8D'

                    -- move change password page
                    UPDATE [Page] 
                    SET [ParentPageId] = @SupportPageId
	                    , [Order] = 99
                    WHERE [Guid] = '4508223C-2989-4592-B764-B3F372B6051B'" );
            
            // Attrib for BlockType: Login Status:My Settings Page
            RockMigrationHelper.DeleteAttribute( "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0" );
            // Attrib for BlockType: Login:Prompt Message
            RockMigrationHelper.DeleteAttribute( "7DE6B0F4-00C3-4A57-B828-607D0F635653" );
            // Remove Block: Page List, from Page: My Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A795BE67-F69B-4D7B-91E1-4F0883F1B718" );

            RockMigrationHelper.DeletePage( "CF54E680-2E02-4F16-B54B-A2F2D29CD932" ); //  Page: My Settings, Layout: Full Width, Site: Rock RMS

            // remove my settings page block setting
            RockMigrationHelper.DeleteBlockAttributeValue( "82AF461F-022D-4ADB-BB12-F220CD605459", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0" );
            RockMigrationHelper.DeleteBlockAttributeValue( "791A6AA0-D498-4795-BB5F-21609175826F", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0" );
            RockMigrationHelper.DeleteBlockAttributeValue( "2356DEDC-803F-4782-A8E9-D0D88393EC2E", "FAF7DAAF-4927-44A8-BF4B-080FF556EBB0" );
        }
    }
}
