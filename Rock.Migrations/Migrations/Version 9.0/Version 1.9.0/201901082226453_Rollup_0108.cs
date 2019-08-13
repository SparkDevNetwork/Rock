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
    public partial class Rollup_0108 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateIsSystemForCategories();
            FixTypeInTemplate();
            FixPageRouteForContentPage();
            AddWorkflowImportExportBlockType();
            AddPersonAttributesToCheckinManagerProfile();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Codes the gen migrations up.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Share Workflow","Export and import workflows from Rock.","~/Blocks/WorkFlow/ShareWorkflow.ascx","WorkFlow","5A074483-2845-4569-B277-3B030DA61111");
            // Attrib for BlockType: Workflow Type Detail:Export Workflows Page
            RockMigrationHelper.UpdateBlockTypeAttribute("E1FF677D-5E52-4259-90C7-5560ECBBD82B","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Export Workflows Page","ExportWorkflowsPage","",@"Page used to export workflows.",4,@"","4C602FC8-6E80-4E5A-90E4-7F54DFB5A98F");
            // Attrib for BlockType: Person Profile:Adult Attribute Category
            RockMigrationHelper.UpdateBlockTypeAttribute("48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1","309460EF-0CC5-41C6-9161-B3837BA3D374","Adult Attribute Category","AdultAttributeCategory","",@"The adult Attribute Category to display attributes from.",4,@"","0D34EADF-22B0-4DA7-91EC-1F29A217D943");
            // Attrib for BlockType: Person Profile:Child Attribute Category
            RockMigrationHelper.UpdateBlockTypeAttribute("48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1","309460EF-0CC5-41C6-9161-B3837BA3D374","Child Attribute Category","ChildAttributeCategory","",@"The children Attribute Category to display attributes from.",3,@"","62D60C22-EE97-4422-A9C7-155E157B0B2A");
            // Attrib for BlockType: SMS Conversations:Person Info Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("3497603B-3BE6-4262-B7E9-EC01FC7140EB","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Person Info Lava Template","PersonInfoLavaTemplate","",@"A Lava template to display person information about the selected Communication Recipient.",5,@"","A9AF883A-152E-44D8-AD5E-FD96CAF3A67E");
            // Attrib for BlockType: SMS Conversations:Allowed SMS Numbers
            RockMigrationHelper.UpdateBlockTypeAttribute("3497603B-3BE6-4262-B7E9-EC01FC7140EB","59D5A94C-94A0-4630-B80A-BB25697D74C7","Allowed SMS Numbers","AllowedSMSNumbers","",@"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",1,@"","2F486671-07C6-448E-B647-A15C16C5B3A2");
            // Attrib for BlockType: SMS Conversations:Show only personal SMS number
            RockMigrationHelper.UpdateBlockTypeAttribute("3497603B-3BE6-4262-B7E9-EC01FC7140EB","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show only personal SMS number","ShowOnlyPersonalSmsNumber","",@"Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",2,@"False","CEBF759C-1B83-48BE-82E1-9E6B7C491333");
            // Attrib for BlockType: SMS Conversations:Hide personal SMS numbers
            RockMigrationHelper.UpdateBlockTypeAttribute("3497603B-3BE6-4262-B7E9-EC01FC7140EB","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Hide personal SMS numbers","HidePersonalSmsNumbers","",@"Only SMS Numbers that are not associated with a person. The numbers without a 'ResponseRecipient' attribute value.",3,@"False","09A89FDD-D3E4-43F3-95AE-D080F959FF0A");
            // Attrib for BlockType: SMS Conversations:Enable SMS Send
            RockMigrationHelper.UpdateBlockTypeAttribute("3497603B-3BE6-4262-B7E9-EC01FC7140EB","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable SMS Send","EnableSmsSend","",@"Allow SMS messages to be sent from the block.",4,@"True","EE7FD478-70BE-44D7-A40F-02EF4AAFEB71");

        }

        /// <summary>
        /// Codes the gen migrations down.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Workflow Type Detail:Export Workflows Page
            RockMigrationHelper.DeleteAttribute("4C602FC8-6E80-4E5A-90E4-7F54DFB5A98F");
            // Attrib for BlockType: SMS Conversations:Enable SMS Send
            RockMigrationHelper.DeleteAttribute("EE7FD478-70BE-44D7-A40F-02EF4AAFEB71");
            // Attrib for BlockType: SMS Conversations:Hide personal SMS numbers
            RockMigrationHelper.DeleteAttribute("09A89FDD-D3E4-43F3-95AE-D080F959FF0A");
            // Attrib for BlockType: SMS Conversations:Show only personal SMS number
            RockMigrationHelper.DeleteAttribute("CEBF759C-1B83-48BE-82E1-9E6B7C491333");
            // Attrib for BlockType: SMS Conversations:Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute("2F486671-07C6-448E-B647-A15C16C5B3A2");
            // Attrib for BlockType: SMS Conversations:Person Info Lava Template
            RockMigrationHelper.DeleteAttribute("A9AF883A-152E-44D8-AD5E-FD96CAF3A67E");
            // Attrib for BlockType: Person Profile:Child Attribute Category
            RockMigrationHelper.DeleteAttribute("62D60C22-EE97-4422-A9C7-155E157B0B2A");
            // Attrib for BlockType: Person Profile:Adult Attribute Category
            RockMigrationHelper.DeleteAttribute("0D34EADF-22B0-4DA7-91EC-1F29A217D943");
            RockMigrationHelper.DeleteBlockType("5A074483-2845-4569-B277-3B030DA61111"); // Share Workflow
        }

                /// <summary>
        /// SK: Set IsSystem = 1 in a new migration roll-up (so system items cannot be accidentally deleted) 
        /// </summary>
        private void UpdateIsSystemForCategories()
        {
                        Sql( @"
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'E919E722-F895-44A4-B86D-38DB8FBA1844'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'F6B98D0C-197D-433A-917B-0C39A80A79E8'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '9AF28593-E631-41E4-B696-78015A4D6F7B'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '7B879922-5DA6-41EE-AC0B-45CEFFB99458'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'BA5B3BFE-C6C2-4B13-89A2-83F0A67486DA'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '41BFFA70-905A-4A88-B8E7-9F1227760183'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'FD1AF609-6907-47D1-A11D-C9FE19AFD585'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'AE55DDF5-C81A-44C7-B5AC-AC8A2768CDA4'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '4FEDD93A-940E-4414-BEAB-67A384D6CD35'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'F5FF6C2E-925C-4D02-A6CD-37185C7FFC66'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '54F8D7D6-700A-4F6A-BB5A-2E2FEF3E4244'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = '6FE3AFD4-CA1B-4C84-9C89-252063EBA755'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'CD2621DC-5DAD-4142-9144-AF2301C353A9'
              UPDATE [Category] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [CreatedDateTime] IS NULL AND [Guid] = 'C8E0FD8D-3032-4ACD-9DB9-FF70B11D6BCC'
            " );
        }

        /// <summary>
        /// MP: Fix article typo in Content Channel View Detail block in the Lava Template fixes #3350
        /// </summary>
        private void FixTypeInTemplate()
        {
            Sql( @"UPDATE AttributeValue
                SET Value = REPLACE(Value, '<artcile class=""message-detail""', '<article class=""message-detail""')
                WHERE Value LIKE '%<artcile class=""message-detail""%'
	                AND AttributeId IN (
		                SELECT Id
		                FROM Attribute
		                WHERE [Guid] = '47C56661-FB70-4703-9781-8651B8B49485'
		                )");
        }

        /// <summary>
        /// SK: Fixed Page Route for content page.
        /// </summary>
        private void FixPageRouteForContentPage()
        {
            Sql( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '117B547B-9D71-4EE9-8047-176676F5DC8C')

                UPDATE
                   [PageRoute]
                SET 
                   [Route] = 'ContentChannel/{contentChannelGuid}'
                WHERE [PageId] = @PageId" );
        }

        /// <summary>
        /// SK: Add Workflow Import/Export Block Type and new page under new page Power Tools
        /// </summary>
        private void  AddWorkflowImportExportBlockType()
        {
            RockMigrationHelper.AddPage( true, "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Import/Export", "", "B6096C72-FE05-472F-B668-B31253DD5E25", "fa fa-sign-out" ); 
            RockMigrationHelper.UpdateBlockType( "Share Workflow", "Export and import workflows from Rock.", "~/Blocks/WorkFlow/ShareWorkflow.ascx", "WorkFlow", "DA262642-A07E-43B0-BE27-8CEF6070C9B8" );
            RockMigrationHelper.AddBlock( true, "B6096C72-FE05-472F-B668-B31253DD5E25".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"DA262642-A07E-43B0-BE27-8CEF6070C9B8".AsGuid(), "Share Workflow","Main",@"<div class=""alert alert-info"">Please note that due to the complexities of workflows, not all workflow types can successfully be exported and re-imported.</div>",@"",0,"71C4BE33-C715-4609-965C-661A3AD95BB8");   
            RockMigrationHelper.UpdateBlockTypeAttribute("E1FF677D-5E52-4259-90C7-5560ECBBD82B","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Export Workflows Page","ExportWorkflowsPage","",@"Page used to export workflows.",4,@"","46A4152D-D132-4545-BB8A-7565D5F0A52A");  
            RockMigrationHelper.AddBlockAttributeValue("2C330A26-1A1C-4B36-80FA-4CB96198F985","46A4152D-D132-4545-BB8A-7565D5F0A52A",@"b6096c72-fe05-472f-b668-b31253dd5e25");  
        }

        /// <summary>
        /// SK: Added Person Attributes to Check-in Manager Profile Page
        /// </summary>
        private void AddPersonAttributesToCheckinManagerProfile()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Check-in Manager Child Attributes", "fa fa-child", "", "672715D8-F632-4CC7-B7DA-C65758438835" );
            RockMigrationHelper.UpdatePersonAttributeCategory( "Check-in Manager Adult Attributes", "fa fa-male", "", "660F8769-B7F6-469E-ABF3-7315042F814C" );

            InsertCateogryForExistingChildAttributes( "4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A", "672715D8-F632-4CC7-B7DA-C65758438835" );
            InsertCateogryForExistingChildAttributes( "DBD192C9-0AA1-46EC-92AB-A3DA8E056D31", "672715D8-F632-4CC7-B7DA-C65758438835" );
            InsertCateogryForExistingChildAttributes( "F832AB6F-B684-4EEA-8DB4-C54B895C79ED", "672715D8-F632-4CC7-B7DA-C65758438835" );

            RockMigrationHelper.UpdateBlockTypeAttribute("48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1","309460EF-0CC5-41C6-9161-B3837BA3D374","Adult Attribute Category","AdultAttributeCategory","",@"The adult Attribute Category to display attributes from.",4,@"","3D99E198-7860-46A4-AC15-AA5817C9EA68");  
            RockMigrationHelper.UpdateBlockTypeAttribute("48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1","309460EF-0CC5-41C6-9161-B3837BA3D374","Child Attribute Category","ChildAttributeCategory","",@"The children Attribute Category to display attributes from.",3,@"","CB873DA8-9908-4AEF-AC7F-525DFACE853D");  
            RockMigrationHelper.AddBlockAttributeValue("1D33D2F9-D19C-495B-BBC8-4379AEF416FE","3D99E198-7860-46A4-AC15-AA5817C9EA68",@"660f8769-b7f6-469e-abf3-7315042f814c");  
            RockMigrationHelper.AddBlockAttributeValue("1D33D2F9-D19C-495B-BBC8-4379AEF416FE","CB873DA8-9908-4AEF-AC7F-525DFACE853D",@"672715d8-f632-4cc7-b7da-c65758438835");  
        }

        /// <summary>
        /// Inserts the cateogry for existing child attributes.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="categoryId">The category identifier.</param>
        private void InsertCateogryForExistingChildAttributes( string attributeId, string categoryId )
        {
            Sql( $@"
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{attributeId}')
                DECLARE @CategoryId int = (SELECT [Id] FROM [Category] WHERE [Guid] = '{categoryId}')

                IF (@AttributeId IS NULL OR @CategoryId IS NULL)
                BEGIN
                    RETURN
                END

                IF NOT EXISTS (
                    SELECT *
                    FROM [AttributeCategory]
                    WHERE [AttributeId] = @AttributeId
                    AND [CategoryId] = CategoryId )
                BEGIN
                    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
                    VALUES( @AttributeId, @CategoryId )
                END" );
        }

    }
}
