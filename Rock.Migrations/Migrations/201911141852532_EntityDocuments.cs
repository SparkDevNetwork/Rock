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
    public partial class EntityDocuments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
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
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
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
    }
}
