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
    public partial class Featurev11merge : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            EntityDocumentsUp();
            SMSPipelineUp();
            AddedIncludeDeceasedToDataViewUp();
            GridLaunchWorkflowsUp();
            AutomateStepsFromDataviewUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AutomateStepsFromDataviewDown();
            GridLaunchWorkflowsDown();
            AddedIncludeDeceasedToDataViewDown();
            SMSPipelineDown();
            EntityDocumentsDown();
        }

        #region EntityDocuments

        private void EntityDocumentsUp()
        {
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985","D65F783D-87A9-4CC9-8110-E83466A0EADB","Document Types","","8C199C6C-7457-4256-9ABB-83DABD2E6282","fa fa-file-alt"); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "8C199C6C-7457-4256-9ABB-83DABD2E6282","D65F783D-87A9-4CC9-8110-E83466A0EADB","Document Type Detail","","FF0A71AD-6282-49E4-BD35-E84369E0D94A","fa fa-file-alt"); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "BF04BB7E-BE3A-4A38-A37C-386B55496303","F66758C6-3E3D-4598-AF4C-B317047B5987","Documents","","6155FBC2-03E9-48C1-B2E7-554CBB7589A5",""); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute("6155FBC2-03E9-48C1-B2E7-554CBB7589A5","Person/{PersonId}/PersonDocs","AF2B7CB5-9CBA-41C4-A2DE-AB84FB5C3552");// for Page:Documents
            RockMigrationHelper.UpdateBlockType("Documents","Add documents to the current context object.","~/Blocks/Crm/Documents.ascx","CRM","A8456E2D-1930-4FF7-8A46-FB0800AC31E0");
            RockMigrationHelper.UpdateBlockType("Document Type Detail","Displays the details of the given Document Type for editing.","~/Blocks/Crm/DocumentTypeDetail.ascx","CRM","85E9AA73-7C96-4731-8DD6-AA604C35E536");
            RockMigrationHelper.UpdateBlockType("Document Type List","Shows a list of all document types.","~/Blocks/Crm/DocumentTypeList.ascx","CRM","C679A2C6-8126-4EF5-8C28-269A51EC4407");

            // Add Block to Page: Document Types Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8C199C6C-7457-4256-9ABB-83DABD2E6282".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"C679A2C6-8126-4EF5-8C28-269A51EC4407".AsGuid(), "Document Type List","Main",@"",@"",0,"F484447A-52AA-43BD-B351-1C174A6FFD2C"); 
            // Add Block to Page: Document Type Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FF0A71AD-6282-49E4-BD35-E84369E0D94A".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"85E9AA73-7C96-4731-8DD6-AA604C35E536".AsGuid(), "Document Type Detail","Main",@"",@"",0,"3465892E-B454-4D22-9579-29A43C5FF799"); 
            // Add Block to Page: Documents Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6155FBC2-03E9-48C1-B2E7-554CBB7589A5".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"A8456E2D-1930-4FF7-8A46-FB0800AC31E0".AsGuid(), "Documents","SectionC1",@"",@"",0,"B7E7B14D-C173-4DEA-9AB7-281190F2F7D6"); 
            
            // Attrib for BlockType: Documents:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "E53CF77B-5519-4962-BE7C-2896A4C36B35" );
            // Attrib for BlockType: Documents:Heading Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Title", "HeadingTitle", "Heading Title", @"The title of the heading.", 0, @"", "DA4ECD62-B718-4133-9405-202F3DFB6570" );
            // Attrib for BlockType: Documents:Show Security Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Button", "ShowSecurityButton", "Show Security Button", @"Show or hide the security button to add or edit security for the document.", 3, @"True", "E0D334F0-84C5-4A3D-8559-C50D98B52050" );
            // Attrib for BlockType: Documents:Heading Icon Css Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading Icon Css Class", "HeadingIconCssClass", "Heading Icon Css Class", @"The Icon CSS Class for use in the heading.", 1, @"", "519CA644-5323-4B00-BA3F-A9D2BE40CF58" );
            // Attrib for BlockType: Documents:Document Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "1FD31CDC-E5E2-431B-8D53-72FC0430044D", "Document Types", "DocumentTypes", "Document Types", @"The document types that should be displayed.", 2, @"", "67AD4129-CBA3-4003-91A9-B332F6389A04" );
            
            // Attrib for BlockType: Document Type List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C679A2C6-8126-4EF5-8C28-269A51EC4407", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "6395BD1B-9A12-479E-979A-5C22C5B0C59F" );
            
            // Attrib Value for Block:Document Type List, Attribute:Detail Page Page: Document Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("F484447A-52AA-43BD-B351-1C174A6FFD2C","6395BD1B-9A12-479E-979A-5C22C5B0C59F",@"ff0a71ad-6282-49e4-bd35-e84369e0d94a");

            // Attrib Value for Block:Documents, Attribute:Entity Type Page: Documents, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B7E7B14D-C173-4DEA-9AB7-281190F2F7D6","E53CF77B-5519-4962-BE7C-2896A4C36B35",@"72657ed8-d16e-492e-ac12-144c5e7567e7");
            
            // Attrib Value for Block:Documents, Attribute:Show Security Button Page: Documents, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B7E7B14D-C173-4DEA-9AB7-281190F2F7D6","E0D334F0-84C5-4A3D-8559-C50D98B52050",@"True");
            
            // Attrib Value for Block:Documents, Attribute:Heading Title Page: Documents, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B7E7B14D-C173-4DEA-9AB7-281190F2F7D6","DA4ECD62-B718-4133-9405-202F3DFB6570",@"Documents");

            // Attrib Value for Block:Documents, Attribute:Heading Icon Css Class Page: Documents, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("B7E7B14D-C173-4DEA-9AB7-281190F2F7D6","519CA644-5323-4B00-BA3F-A9D2BE40CF58",@"fa fa-file-alt");

            RockMigrationHelper.UpdateFieldType("Document Type","","Rock","Rock.Field.Types.DocumentTypeFieldType","1FD31CDC-E5E2-431B-8D53-72FC0430044D");
            
            // Add/Update PageContext for Page:Documents, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "6155FBC2-03E9-48C1-B2E7-554CBB7589A5", "Rock.Model.Person", "PersonId", "C2436E4D-318D-48A0-BE5F-8E8AF1950E03");

            Sql( @"
                -- Don't display the page name in the bread crumb for the DocumentTypeDetail page.
                UPDATE [dbo].[Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'FF0A71AD-6282-49E4-BD35-E84369E0D94A'

                -- Menu order for the person profile page
                UPDATE [dbo].[Page] SET [Order] = 0 WHERE [Guid] = '5E036ADE-C2A4-4988-B393-DAC58230F02E' --Person Search
                UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25' --Person Profile
                UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '1C737278-4CBA-404B-B6B3-E3F0E05AB5FE' --Extended Attributes
                UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = 'CB9ABA3B-6962-4A42-BDA1-EA71B7309232' --Steps
                UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = '183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D' --Groups
                UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = '6155FBC2-03E9-48C1-B2E7-554CBB7589A5' --Documents
                UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892' --Contributions
                UPDATE [dbo].[Page] SET [Order] = 7 WHERE [Guid] = '15FA4176-1C8E-409D-8B47-85ADA35DE5D2' --Benevolence
                UPDATE [dbo].[Page] SET [Order] = 8 WHERE [Guid] = '0E56F56E-FB32-4827-A69A-B90D43CB47F5' --Security
                UPDATE [dbo].[Page] SET [Order] = 9 WHERE [Guid] = 'BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418' --History
                UPDATE [dbo].[Page] SET [Order] = 10 WHERE [Guid] = '985D7F56-9FD6-421B-B406-2D6B87CAFAE9' --Assessments" );
        }

        private void EntityDocumentsDown()
        {
            RockMigrationHelper.DeleteAttribute("6395BD1B-9A12-479E-979A-5C22C5B0C59F"); // Attrib for BlockType: Document Type List:Detail Page
            RockMigrationHelper.DeleteAttribute("67AD4129-CBA3-4003-91A9-B332F6389A04"); // Attrib for BlockType: Documents:Document Types
            RockMigrationHelper.DeleteAttribute("519CA644-5323-4B00-BA3F-A9D2BE40CF58"); // Attrib for BlockType: Documents:Heading Icon Css Class
            RockMigrationHelper.DeleteAttribute("E0D334F0-84C5-4A3D-8559-C50D98B52050"); // Attrib for BlockType: Documents:Show Security Button
            RockMigrationHelper.DeleteAttribute("DA4ECD62-B718-4133-9405-202F3DFB6570"); // Attrib for BlockType: Documents:Heading Title
            RockMigrationHelper.DeleteAttribute("E53CF77B-5519-4962-BE7C-2896A4C36B35"); // Attrib for BlockType: Documents:Entity Type

            RockMigrationHelper.DeleteBlock("B7E7B14D-C173-4DEA-9AB7-281190F2F7D6");// Remove Block: Documents, from Page: Documents, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("3465892E-B454-4D22-9579-29A43C5FF799");// Remove Block: Document Type Detail, from Page: Document Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("F484447A-52AA-43BD-B351-1C174A6FFD2C");// Remove Block: Document Type List, from Page: Document Types, Site: Rock RMS

            RockMigrationHelper.DeleteBlockType("C679A2C6-8126-4EF5-8C28-269A51EC4407"); // Document Type List
            RockMigrationHelper.DeleteBlockType("85E9AA73-7C96-4731-8DD6-AA604C35E536"); // Document Type Detail
            RockMigrationHelper.DeleteBlockType("A8456E2D-1930-4FF7-8A46-FB0800AC31E0"); // Documents

            RockMigrationHelper.DeletePage("6155FBC2-03E9-48C1-B2E7-554CBB7589A5"); //  Page: Documents, Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.DeletePage("FF0A71AD-6282-49E4-BD35-E84369E0D94A"); //  Page: Document Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("8C199C6C-7457-4256-9ABB-83DABD2E6282"); //  Page: Document Types, Layout: Full Width, Site: Rock RMS
            
            // Delete PageContext for Page:Person Documents, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "C2436E4D-318D-48A0-BE5F-8E8AF1950E03");
        }

        #endregion EntityDocuments

        #region AddedSmsPipelineAndUpdatedSmsAction

        private void SMSPipelineUp()
        {
            CreateTable(
                "dbo.SmsPipeline",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.SmsAction", "SmsPipelineId", c => c.Int(nullable: true));
            
            AddDefaultSmsPipelineAndUpdateSmsAction();

            AlterColumn( "dbo.SmsAction", "SmsPipelineId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.SmsAction", "SmsPipelineId" );
            AddForeignKey( "dbo.SmsAction", "SmsPipelineId", "dbo.SmsPipeline", "Id", cascadeDelete: true );

            UpdatePageAndBlockSettings();
        }

        private void AddDefaultSmsPipelineAndUpdateSmsAction()
        {
            Sql( @"IF (SELECT COUNT(*) FROM [dbo].[SmsAction]) > 0
                    BEGIN
                        INSERT INTO [dbo].[SmsPipeline] ([Name], [IsActive], [Guid])
                        VALUES ('Default', 1, NEWID())
                    
                        DECLARE @smsPipelineId AS INT = @@IDENTITY

                        UPDATE [dbo].[SmsAction] SET SmsPipelineID = @smsPipelineId
                    END" );
        }

        private void UpdatePageAndBlockSettings()
        {
            // Rename block to SmsPipelineDetail.ascx
            RockMigrationHelper.UpdateBlockTypeByGuid( "SMS Pipeline Detail", "Configures the pipeline that processes an incoming SMS message.", "~/Blocks/Communication/SmsPipelineDetail.ascx", "Communication", "44C32EB7-4DA3-4577-AC41-E3517442E269" );
            
            RockMigrationHelper.AddPage( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "SMS Pipeline Detail", "", "FCE39659-4D86-48D7-9C48-D837D3588C42", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "SMS Pipeline List", "Lists the SMS Pipelines currently in the system.", "~/Blocks/Communication/SmsPipelineList.ascx", "Communication", "DB6FD0BF-FDCE-48DA-919C-240F029518A2" );
            
            // Remove Block: SMS Pipeline Detail, from Page: SMS Pipeline List, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );

            // Add Block to Page: SMS Pipeline Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FCE39659-4D86-48D7-9C48-D837D3588C42".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "44C32EB7-4DA3-4577-AC41-E3517442E269".AsGuid(), "SMS Pipeline Detail", "Main", @"", @"", 0, "B191F27B-2A1D-4A2F-9E06-49BC2D8B1E5B" );

            // Add Block to Page: SMS Pipeline List Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "DB6FD0BF-FDCE-48DA-919C-240F029518A2".AsGuid(), "SMS Pipeline List", "Main", @"", @"", 0, "EC3BDDFC-4688-485B-9EF3-890B9EC39527" );
            
            // Attrib for BlockType: SMS Pipeline List:SMS Pipeline Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB6FD0BF-FDCE-48DA-919C-240F029518A2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "SMS Pipeline Detail", "SMSPipelineDetail", "SMS Pipeline Detail", @"", 0, @"", "FA40D714-9D6F-4620-97B1-07BFFFC08F96" );

            // Attrib Value for Block:SMS Pipeline List, Attribute:core.CustomGridEnableStickyHeaders Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3BDDFC-4688-485B-9EF3-890B9EC39527", "27549AA7-AE69-4121-AAAC-DBA8EE5ED6EE", @"False" );

            // Attrib Value for Block:SMS Pipeline List, Attribute:SMS Pipeline Detail Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3BDDFC-4688-485B-9EF3-890B9EC39527", "FA40D714-9D6F-4620-97B1-07BFFFC08F96", @"fce39659-4d86-48d7-9c48-d837d3588c42" );
        }

        private void SMSPipelineDown()
        {
            RemoveNonDefaultSmsActions();

            RemovePageAndBlockSettings();

            DropForeignKey( "dbo.SmsAction", "SmsPipelineId", "dbo.SmsPipeline");
            DropForeignKey("dbo.SmsPipeline", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SmsPipeline", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.SmsPipeline", new[] { "Guid" });
            DropIndex("dbo.SmsPipeline", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SmsPipeline", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SmsAction", new[] { "SmsPipelineId" });
            DropColumn("dbo.SmsAction", "SmsPipelineId");
            DropTable("dbo.SmsPipeline");
        }

        private void RemoveNonDefaultSmsActions()
        {
            Sql( @"DECLARE @defaultSmsPipelineId AS INT

                    SELECT @defaultSmsPipelineId = MIN(Id)
                    FROM [dbo].[SmsPipeline]
                    
                    IF @defaultSmsPipelineId IS NOT NULL
                    BEGIN
                        DELETE SmsAction
                        WHERE SmsPipelineId != @defaultSmsPipelineId
                    END" );
        }

        private void RemovePageAndBlockSettings()
        {
            // Attrib for BlockType: SMS Pipeline List:SMS Pipeline Detail
            RockMigrationHelper.DeleteAttribute( "FA40D714-9D6F-4620-97B1-07BFFFC08F96" );

            // Remove Block: SMS Pipeline List, from Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EC3BDDFC-4688-485B-9EF3-890B9EC39527" );
            
            // Remove Block: SMS Pipeline Detail, from Page: SMS Pipeline Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B191F27B-2A1D-4A2F-9E06-49BC2D8B1E5B" );
            
            // SMS Pipeline List
            RockMigrationHelper.DeleteBlockType( "DB6FD0BF-FDCE-48DA-919C-240F029518A2" );
            
            //  Page: SMS Pipeline Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "FCE39659-4D86-48D7-9C48-D837D3588C42" );

            // Add Block to Page: SMS Pipeline Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "44C32EB7-4DA3-4577-AC41-E3517442E269".AsGuid(), "SMS Pipeline", "Main", @"", @"", 0, "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );

            // Rename block to SmsPipeline.ascx
            RockMigrationHelper.UpdateBlockTypeByGuid( "SMS Pipeline", "Configures the pipeline that processes an incoming SMS message.", "~/Blocks/Communication/SmsPipeline.ascx", "Communication", "44C32EB7-4DA3-4577-AC41-E3517442E269" );
        }

        #endregion AddedSmsPipelineAndUpdatedSmsAction

        #region AddedIncludeDeceasedToDataView

        private void AddedIncludeDeceasedToDataViewUp()
        {
            AddColumn("dbo.DataView", "IncludeDeceased", c => c.Boolean(nullable: false));
        }

        private void AddedIncludeDeceasedToDataViewDown()
        {
            DropColumn("dbo.DataView", "IncludeDeceased");
        }

        #endregion AddedIncludeDeceasedToDataView

        #region GridLaunchWorkflows

        private void GridLaunchWorkflowsUp()
        {
            RockMigrationHelper.AddPage( true, "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Launch", "", "DCD0E0A3-F115-4609-860F-B5BF678AA41E", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "DCD0E0A3-F115-4609-860F-B5BF678AA41E", "LaunchWorkflows/{EntitySetId}", "D497A92F-AA0E-40BF-997F-D512FF0357CE" );// for Page:Workflow Launch
            RockMigrationHelper.UpdateBlockType( "Workflow Launch", "Block that enables previewing an entity set and then launching a workflow for each item within the set.", "~/Blocks/WorkFlow/WorkflowLaunch.ascx", "Workflow", "D7C15C1B-7487-42C3-A485-AD154F46558A" );
            
            // Add Block to Page: Workflow Launch Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DCD0E0A3-F115-4609-860F-B5BF678AA41E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D7C15C1B-7487-42C3-A485-AD154F46558A".AsGuid(), "Workflow Launch", "Main", @"", @"", 0, "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33" );
            
            // Attrib for BlockType: Workflow Launch:Default Number of Items to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Default Number of Items to Show", "DefaultNumberOfItemsToShow", "Default Number of Items to Show", @"The number of entities to list on screen before summarizing ('...and xx more').", 5, @"50", "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE" );
            // Attrib for BlockType: Workflow Launch:Workflow Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "Workflow Types", @"Only the selected workflow types will be shown. If left blank, any workflow type can be launched.", 1, @"", "EAD99415-02EA-4D67-BC8B-1B7E186D6057" );
            // Attrib for BlockType: Workflow Launch:Allow Multiple Workflow Launches
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Multiple Workflow Launches", "AllowMultipleWorkflowLaunches", "Allow Multiple Workflow Launches", @"If set to yes, allows launching multiple different types of workflows. After one is launched, the block will allow the individual to select another type to be launched. This will only show if more than one type is configured.", 2, @"True", "A9F130BF-F793-4755-BAC2-8B685E1176B0" );
            // Attrib for BlockType: Workflow Launch:Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The title to display in the block panel.", 3, @"Workflow Launch", "C291BDE3-5689-45E6-B092-87EEF0782ABE" );
            // Attrib for BlockType: Workflow Launch:Panel Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7C15C1B-7487-42C3-A485-AD154F46558A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title Icon CSS Class", "PanelIcon", "Panel Title Icon CSS Class", @"The icon to use before the panel title.", 4, @"fa fa-cog", "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89" );
            
            // Attrib Value for Block:Workflow Launch, Attribute:Default Number of Items to Show Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Workflow Types Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "EAD99415-02EA-4D67-BC8B-1B7E186D6057", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Allow Multiple Workflow Launches Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "A9F130BF-F793-4755-BAC2-8B685E1176B0", @"True" );
            // Attrib Value for Block:Workflow Launch, Attribute:Panel Title Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "C291BDE3-5689-45E6-B092-87EEF0782ABE", @"" );
            // Attrib Value for Block:Workflow Launch, Attribute:Panel Title Icon CSS Class Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33", "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89", @"" );
        }

        private void GridLaunchWorkflowsDown()
        {
            // Attrib for BlockType: Workflow Launch:Panel Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "8EF7E051-F5ED-4C6B-A314-04FE8D97CC89" );
            // Attrib for BlockType: Workflow Launch:Panel Title
            RockMigrationHelper.DeleteAttribute( "C291BDE3-5689-45E6-B092-87EEF0782ABE" );
            // Attrib for BlockType: Workflow Launch:Allow Multiple Workflow Launches
            RockMigrationHelper.DeleteAttribute( "A9F130BF-F793-4755-BAC2-8B685E1176B0" );
            // Attrib for BlockType: Workflow Launch:Workflow Types
            RockMigrationHelper.DeleteAttribute( "EAD99415-02EA-4D67-BC8B-1B7E186D6057" );
            // Attrib for BlockType: Workflow Launch:Default Number of Items to Show
            RockMigrationHelper.DeleteAttribute( "FF5A5181-F48E-4D75-B0FA-0CC626FDBBEE" );

            // Remove Block: Workflow Launch, from Page: Workflow Launch, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5AAAB4FD-6A07-47BD-A08E-F7DB68DD7F33" );
            RockMigrationHelper.DeleteBlockType( "D7C15C1B-7487-42C3-A485-AD154F46558A" ); // Workflow Launch
            RockMigrationHelper.DeletePage( "DCD0E0A3-F115-4609-860F-B5BF678AA41E" ); //  Page: Workflow Launch, Layout: Full Width, Site: Rock RMS
        }

        #endregion GridLaunchWorkflows

        #region AutomateStepsFromDataview

        private void AutomateStepsFromDataviewUp()
        {
            Sql( $@"
                INSERT INTO ServiceJob (
                    IsSystem,
                    IsActive,
                    Name,
                    Description,
                    Class,
                    Guid,
                    CreatedDateTime,
                    NotificationStatus,
                    CronExpression
                ) VALUES (
                    1, -- IsSystem
                    1, -- IsActive
                    'Steps Automation', -- Name
                    'Creates steps for people within a dataview', -- Description
                    'Rock.Jobs.StepsAutomation', -- Class
                    '{Rock.SystemGuid.ServiceJob.STEPS_AUTOMATION}', -- Guid
                    GETDATE(), -- Created
                    1, -- All notifications
                    '0 0 4 1/1 * ? *' -- At 4:00 am
                );" );

            RockMigrationHelper.UpdateEntityAttribute( 
                "Rock.Model.ServiceJob",
                fieldTypeGuid: Rock.SystemGuid.FieldType.INTEGER, 
                "Class", 
                "Rock.Jobs.StepsAutomation", 
                "Duplicate Prevention Day Range",
                description: "If duplicates are enabled above, this setting will keep steps from being added if a previous step was within the number of days provided.", 
                order: 1,
                defaultValue: "7", 
                guid: "87144EF1-F902-4293-A416-06E09D4963AE",
                key: "DuplicatePreventionDayRange",
                isRequired: false );
        }

        private void AutomateStepsFromDataviewDown()
        {
            RockMigrationHelper.DeleteAttribute( "87144EF1-F902-4293-A416-06E09D4963AE" );
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.STEPS_AUTOMATION}';" );
        }

        #endregion AutomateStepsFromDataview

    }
}
