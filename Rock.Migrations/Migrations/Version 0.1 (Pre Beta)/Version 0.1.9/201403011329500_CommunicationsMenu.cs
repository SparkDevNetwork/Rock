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
    public partial class CommunicationsMenu : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    --EXEC sp_rename '[dbo].[SystemEmail].[PK_dbo.EmailTemplate]', 'PK_dbo.SystemEmail';
    EXEC sp_rename '[FK_dbo.EmailTemplate_dbo.Person_PersonId]', 'FK_dbo.SystemEmail_dbo.Person_PersonId';
    EXEC sp_rename '[FK_dbo.EmailTemplate_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId';
    EXEC sp_rename '[FK_dbo.EmailTemplate_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId';
" );
            DropForeignKey( "dbo.SystemEmail", "PersonId", "dbo.Person" );
            DropIndex( "dbo.SystemEmail", new[] { "PersonId" } );
            DropColumn( "dbo.SystemEmail", "PersonId" );

            AddPage( "98163C8B-5C91-4A68-BB79-6AD948A604CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communications", "", "7F79E512-B9DB-4780-9887-AD6D63A39050", "" ); // Site:Rock RMS
            AddPage( "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Templates", "", "EA611245-7A5E-4995-A3C6-EB97C6FD7C8D", "" ); // Site:Rock RMS
            AddPage( "EA611245-7A5E-4995-A3C6-EB97C6FD7C8D", "195BCD57-1C10-4969-886F-7324B6287B75", "Template Detail", "", "753D62FD-A06F-43A3-B9D2-0A728FF2809A", "" ); // Site:Rock RMS

            AddBlockType( "Template Detail", "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people.", "~/Blocks/Communication/TemplateDetail.ascx", "Communication", "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D" );
            AddBlockType( "Template List", "Lists the available communication templates that can used when creating new communications.", "~/Blocks/Communication/TemplateList.ascx", "Communication", "EACDBBD4-C355-4D38-B604-779BC55D3876" );

            // Add Block to Page: Communication Templates, Site: Rock RMS
            AddBlock( "EA611245-7A5E-4995-A3C6-EB97C6FD7C8D", "", "EACDBBD4-C355-4D38-B604-779BC55D3876", "Template List", "Main", "", "", 0, "8B080D88-D088-4D09-9D74-576B485549A2" );
            // Attrib for BlockType: Template List:Detail Page
            AddBlockTypeAttribute( "EACDBBD4-C355-4D38-B604-779BC55D3876", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "08C596A3-CDC5-42B8-9D2B-382254DDBCE5" );
            // Attrib Value for Block:Template List, Attribute:Detail Page Page: Communication Templates, Site: Rock RMS
            AddBlockAttributeValue( "8B080D88-D088-4D09-9D74-576B485549A2", "08C596A3-CDC5-42B8-9D2B-382254DDBCE5", @"753d62fd-a06f-43a3-b9d2-0a728ff2809a" );

            // Add Block to Page: Template Detail, Site: Rock RMS
            AddBlock( "753D62FD-A06F-43A3-B9D2-0A728FF2809A", "", "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D", "Template Detail", "Main", "", "", 0, "425C325E-1054-4A52-A162-DECEB377E178" );

            Sql( @"
    -- Communications
    UPDATE [Page] SET 
        [MenuDisplayChildPages] = 1,
        [MenuDisplayDescription] = 1,
        [BreadCrumbDisplayName] = 0,
        [Order] = 0
    WHERE [Guid] = '7F79E512-B9DB-4780-9887-AD6D63A39050'
    
    -- Reporting
    UPDATE [Page] SET 
        [Order] = 1
    WHERE [Guid] = 'BB0ACD18-24FB-42BA-B89A-2FFD80472F5B'

    -- Webiste
    UPDATE [Page] SET 
        [InternalName] = 'Website',
        [PageTitle] = 'Website',
        [BrowserTitle] = 'Website',
        [Order] = 2
    WHERE [Guid] = 'F7105BFE-B28C-41B6-9CE6-F1018D77DD8F'

    DECLARE @ParentPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '7F79E512-B9DB-4780-9887-AD6D63A39050')

    -- New Communications
    UPDATE [Page] SET 
        [BreadCrumbDisplayName] = 0,
        [Order] = 0,
        [ParentPageId] = @ParentPageId
    WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'    

    -- Communication List
    UPDATE [Page] SET 
        [Order] = 1,
        [ParentPageId] = @ParentPageId
    WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79'
    
    -- Communication Template
    UPDATE [Page] SET 
        [Order] = 2,
        [ParentPageId] = @ParentPageId
    WHERE [Guid] = 'EA611245-7A5E-4995-A3C6-EB97C6FD7C8D'

    -- Communication Template Detail
    UPDATE [Page] SET 
        [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '753D62FD-A06F-43A3-B9D2-0A728FF2809A'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    -- Website
    UPDATE [Page] SET 
        [InternalName] = 'Website & Communications',
        [PageTitle] = 'Website & Communications',
        [BrowserTitle] = 'Website & Communications',
        [Order] = 0
    WHERE [Guid] = 'F7105BFE-B28C-41B6-9CE6-F1018D77DD8F'

    DECLARE @ParentPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F7105BFE-B28C-41B6-9CE6-F1018D77DD8F')

    -- New Communications
    UPDATE [Page] SET 
        [Order] = 6,
        [ParentPageId] = @ParentPageId
    WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'    

    -- Communication List
    UPDATE [Page] SET 
        [Order] = 7,
        [ParentPageId] = @ParentPageId
    WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79'
    
" );

            // Attrib for BlockType: Template List:Detail Page
            DeleteAttribute( "08C596A3-CDC5-42B8-9D2B-382254DDBCE5" );

            // Remove Block: Template Detail, from Page: Template Detail, Site: Rock RMS
            DeleteBlock( "425C325E-1054-4A52-A162-DECEB377E178" );
            // Remove Block: Template List, from Page: Communication Templates, Site: Rock RMS
            DeleteBlock( "8B080D88-D088-4D09-9D74-576B485549A2" );

            DeleteBlockType( "EACDBBD4-C355-4D38-B604-779BC55D3876" ); // Template List
            DeleteBlockType( "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D" ); // Template Detail

            DeletePage( "753D62FD-A06F-43A3-B9D2-0A728FF2809A" ); // Page: Template DetailLayout: Full Width Panel, Site: Rock RMS
            DeletePage( "EA611245-7A5E-4995-A3C6-EB97C6FD7C8D" ); // Page: Communication TemplatesLayout: Full Width, Site: Rock RMS
            DeletePage( "7F79E512-B9DB-4780-9887-AD6D63A39050" ); // Page: CommunicationsLayout: Full Width, Site: Rock RMS

            AddColumn( "dbo.SystemEmail", "PersonId", c => c.Int() );
            CreateIndex( "dbo.SystemEmail", "PersonId" );
            AddForeignKey( "dbo.SystemEmail", "PersonId", "dbo.Person", "Id", cascadeDelete: true );

            Sql( @"
    EXEC sp_rename '[dbo].[SystemEmail].[PK_dbo.SystemEmail]', 'PK_dbo.EmailTemplate';
    EXEC sp_rename '[FK_dbo.SystemEmail_dbo.Person_PersonId]', 'FK_dbo.EmailTemplate_dbo.Person_PersonId';
    EXEC sp_rename '[FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId]', 'FK_dbo.EmailTemplate_dbo.PersonAlias_CreatedByPersonAliasId';
    EXEC sp_rename '[FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId]', 'FK_dbo.EmailTemplate_dbo.PersonAlias_ModifiedByPersonAliasId';
" );
        }
    }
}
