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
    public partial class DigitalSignatures : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.SignatureDocumentTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        ProviderEntityTypeId = c.Int(),
                        ProviderTemplateKey = c.String(maxLength: 100),
                        BinaryFileTypeId = c.Int(),
                        RequestEmailTemplateFromName = c.String(),
                        RequestEmailTemplateFromAddress = c.String(),
                        RequestEmailTemplateSubject = c.String(),
                        RequestEmailTemplateBody = c.String(),
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
                .ForeignKey("dbo.BinaryFileType", t => t.BinaryFileTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.ProviderEntityTypeId)
                .Index(t => t.ProviderEntityTypeId)
                .Index(t => t.BinaryFileTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            CreateTable(
                "dbo.SignatureDocument",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SignatureDocumentTemplateId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 250),
                        DocumentKey = c.String(maxLength: 200),
                        RequestDate = c.DateTime(),
                        AppliesToPersonAliasId = c.Int(),
                        AssignedToPersonAliasId = c.Int(),
                        Status = c.Int(nullable: false),
                        LastStatusDate = c.DateTime(),
                        BinaryFileId = c.Int(),
                        SignedByPersonAliasId = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.AppliesToPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.AssignedToPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.SignatureDocumentTemplate", t => t.SignatureDocumentTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.SignedByPersonAliasId)
                .Index(t => t.SignatureDocumentTemplateId)
                .Index(t => t.AppliesToPersonAliasId)
                .Index(t => t.AssignedToPersonAliasId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.SignedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            AddColumn("dbo.Group", "RequiredSignatureDocumentTemplateId", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "RequiredSignatureDocumentTemplateId", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "SignatureDocumentAction", c => c.Int(nullable: false));
            CreateIndex("dbo.Group", "RequiredSignatureDocumentTemplateId");
            CreateIndex("dbo.RegistrationTemplate", "RequiredSignatureDocumentTemplateId");
            AddForeignKey("dbo.RegistrationTemplate", "RequiredSignatureDocumentTemplateId", "dbo.SignatureDocumentTemplate", "Id");
            AddForeignKey("dbo.Group", "RequiredSignatureDocumentTemplateId", "dbo.SignatureDocumentTemplate", "Id");

            RockMigrationHelper.UpdateBinaryFileType( "0AA42802-04FD-4AEC-B011-FEB127FC85CD", "Digitally Signed Documents", "Documents that are digitally signed", "", "40871411-4E2D-45C2-9E21-D9FCBA5FC340", false, true );

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Signature Documents", "", "CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2", "fa fa-pencil-square-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Document Type", "", "7096FA12-07A5-489C-83B0-EE55494A3484", "fa fa-file-text-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Signature Document Providers", "", "FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3", "fa fa-pencil-square-o", "53F1B7D9-806A-4541-93BC-4CCF5DFF90B3" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "7096FA12-07A5-489C-83B0-EE55494A3484", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Document", "", "52DD4262-D55E-4749-9249-9A919BBB2309", "fa fa-file-text-o" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Signature Document Detail", "Displays the details of a given signature document.", "~/Blocks/Core/SignatureDocumentDetail.ascx", "Core", "01D23E86-51DC-496D-BB3E-0CEF5094F304" );
            RockMigrationHelper.UpdateBlockType( "Signature Document List", "Block for viewing values for a signature document type.", "~/Blocks/Core/SignatureDocumentList.ascx", "Core", "256F6FDB-B241-4DE6-9C38-0E9DA0270A22" );
            RockMigrationHelper.UpdateBlockType( "Signature Document Type Detail", "Displays the details of the given signature document type.", "~/Blocks/Core/SignatureDocumentTemplateDetail.ascx", "Core", "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06" );
            RockMigrationHelper.UpdateBlockType( "Signature Document Type List", "Lists all the signature document types and allows for managing them.", "~/Blocks/Core/SignatureDocumentTemplateList.ascx", "Core", "2E413152-B790-4EC2-84A9-9B48D2717D63" );

            // Add Block to Page: Signature Documents, Site: Rock RMS
            RockMigrationHelper.AddBlock( "CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2", "", "2E413152-B790-4EC2-84A9-9B48D2717D63", "Signature Document Type List", "Main", "", "", 0, "29D8EF57-A485-4996-931C-6289256BE1E5" );
            // Add Block to Page: Document Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7096FA12-07A5-489C-83B0-EE55494A3484", "", "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "Signature Document Type Detail", "Main", "", "", 0, "A112EF63-77C5-4570-A52E-5B68BD27EA84" );
            // Add Block to Page: Signature Document Providers, Site: Rock RMS
            RockMigrationHelper.AddBlock( "FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3", "", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Components", "Main", "", "", 0, "8690831C-D48A-48A7-BBA7-5BC496E493F2" );
            // Add Block to Page: Document Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7096FA12-07A5-489C-83B0-EE55494A3484", "", "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "Signature Document List", "SectionA", "", "", 0, "F4A322B0-C362-4049-8237-8AC6C4A565F2" );
            // Add Block to Page: Document, Site: Rock RMS
            RockMigrationHelper.AddBlock( "52DD4262-D55E-4749-9249-9A919BBB2309", "", "01D23E86-51DC-496D-BB3E-0CEF5094F304", "Signature Document Detail", "Main", "", "", 0, "3F329DB8-ACD6-415D-8486-10CF55FDB6D4" );

            // Attrib for BlockType: Signature Document Type List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "2E413152-B790-4EC2-84A9-9B48D2717D63", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C73666B5-947C-47E1-B5F6-214BFD17E002" );
            // Attrib for BlockType: Signature Document Type Detail:Default File Type
            RockMigrationHelper.AddBlockTypeAttribute( "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Default File Type", "DefaultFileType", "", "The default file type to use when creating new document types.", 0, @"40871411-4E2D-45C2-9E21-D9FCBA5FC340", "967C18D0-822B-469F-A3EB-98F0952F4782" );
            // Attrib for BlockType: Signature Document List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "5E982756-72C0-4397-BB16-5D1C8D0DA85D" );

            // Attrib Value for Block:Signature Document Type List, Attribute:Detail Page Page: Signature Documents, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29D8EF57-A485-4996-931C-6289256BE1E5", "C73666B5-947C-47E1-B5F6-214BFD17E002", @"7096fa12-07a5-489c-83b0-ee55494a3484" );
            // Attrib Value for Block:Signature Document Type Detail, Attribute:Default File Type: Document, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A112EF63-77C5-4570-A52E-5B68BD27EA84", "967C18D0-822B-469F-A3EB-98F0952F4782", @"40871411-4e2d-45c2-9e21-d9fcba5fc340" );
            // Attrib Value for Block:Components, Attribute:Component Container Page: Signature Document Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8690831C-D48A-48A7-BBA7-5BC496E493F2", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.Security.DigitalSignatureContainer, Rock" );
            // Attrib Value for Block:Components, Attribute:Support Ordering Page: Signature Document Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8690831C-D48A-48A7-BBA7-5BC496E493F2", "A4889D7B-87AA-419D-846C-3E618E79D875", @"True" );
            // Attrib Value for Block:Signature Document List, Attribute:Detail Page Page: Document Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F4A322B0-C362-4049-8237-8AC6C4A565F2", "5E982756-72C0-4397-BB16-5D1C8D0DA85D", @"52dd4262-d55e-4749-9249-9a919bbb2309" );

            Sql( @"
    DECLARE @BinaryFileEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '62AF597F-F193-412B-94EA-291CF713327D' )
    DECLARE @SignedDocumentFileTypeId int = (SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = '40871411-4E2D-45C2-9E21-D9FCBA5FC340')
    DECLARE @AdminRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
	DECLARE @StaffRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' )
	DECLARE @StaffLikeRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )
    DECLARE @SafetyRoleId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB')

	INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	VALUES ( @BinaryFileEntityTypeId, @SignedDocumentFileTypeId, 0, 'View', 'A', 0, @AdminRoleId, NEWID() )

	INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	VALUES ( @BinaryFileEntityTypeId, @SignedDocumentFileTypeId, 1, 'View', 'A', 0, @StaffRoleId, NEWID() )

	INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	VALUES ( @BinaryFileEntityTypeId, @SignedDocumentFileTypeId, 2, 'View', 'A', 0, @StaffLikeRoleId, NEWID() )

	INSERT INTO [dbo].[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [Guid] )
	VALUES ( @BinaryFileEntityTypeId, @SignedDocumentFileTypeId, 3, 'View', 'D', 1, NEWID() )
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Signature Document List:Detail Page
            RockMigrationHelper.DeleteAttribute( "5E982756 -72C0-4397-BB16-5D1C8D0DA85D" );
            // Attrib for BlockType: Signature Document Type Detail:Default File Type
            RockMigrationHelper.DeleteAttribute( "967C18D0-822B-469F-A3EB-98F0952F4782" );
            // Attrib for BlockType: Signature Document Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "C73666B5-947C-47E1-B5F6-214BFD17E002" );

            // Remove Block: Signature Document Detail, from Page: Document, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3F329DB8-ACD6-415D-8486-10CF55FDB6D4" );
            // Remove Block: Signature Document List, from Page: Document Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F4A322B0-C362-4049-8237-8AC6C4A565F2" );
            // Remove Block: Components, from Page: Signature Document Providers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8690831C-D48A-48A7-BBA7-5BC496E493F2" );
            // Remove Block: Signature Document Type Detail, from Page: Document Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A112EF63-77C5-4570-A52E-5B68BD27EA84" );
            // Remove Block: Signature Document Type List, from Page: Signature Documents, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "29D8EF57-A485-4996-931C-6289256BE1E5" );

            RockMigrationHelper.DeleteBlockType( "2E413152-B790-4EC2-84A9-9B48D2717D63" ); // Signature Document Type List
            RockMigrationHelper.DeleteBlockType( "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06" ); // Signature Document Type Detail
            RockMigrationHelper.DeleteBlockType( "256F6FDB-B241-4DE6-9C38-0E9DA0270A22" ); // Signature Document List
            RockMigrationHelper.DeleteBlockType( "01D23E86-51DC-496D-BB3E-0CEF5094F304" ); // Signature Document Detail

            RockMigrationHelper.DeletePage( "52DD4262-D55E-4749-9249-9A919BBB2309" ); //  Page: Document, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3" ); //  Page: Signature Document Providers, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "7096FA12-07A5-489C-83B0-EE55494A3484" ); //  Page: Document Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2" ); //  Page: Signature Documents, Layout: Full Width, Site: Rock RMS

            DropForeignKey( "dbo.Group", "RequiredSignatureDocumentTemplateId", "dbo.SignatureDocumentTemplate");
            DropForeignKey("dbo.RegistrationTemplate", "RequiredSignatureDocumentTemplateId", "dbo.SignatureDocumentTemplate");
            DropForeignKey("dbo.SignatureDocumentTemplate", "ProviderEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.SignatureDocumentTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocument", "SignedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocument", "SignatureDocumentTemplateId", "dbo.SignatureDocumentTemplate");
            DropForeignKey("dbo.SignatureDocument", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocument", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocument", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.SignatureDocument", "AssignedToPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocument", "AppliesToPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocumentTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignatureDocumentTemplate", "BinaryFileTypeId", "dbo.BinaryFileType");
            DropIndex("dbo.SignatureDocument", new[] { "ForeignKey" });
            DropIndex("dbo.SignatureDocument", new[] { "ForeignGuid" });
            DropIndex("dbo.SignatureDocument", new[] { "ForeignId" });
            DropIndex("dbo.SignatureDocument", new[] { "Guid" });
            DropIndex("dbo.SignatureDocument", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SignatureDocument", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SignatureDocument", new[] { "SignedByPersonAliasId" });
            DropIndex("dbo.SignatureDocument", new[] { "BinaryFileId" });
            DropIndex("dbo.SignatureDocument", new[] { "AssignedToPersonAliasId" });
            DropIndex("dbo.SignatureDocument", new[] { "AppliesToPersonAliasId" });
            DropIndex("dbo.SignatureDocument", new[] { "SignatureDocumentTemplateId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "ForeignKey" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "ForeignGuid" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "ForeignId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "Guid" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "BinaryFileTypeId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "ProviderEntityTypeId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "RequiredSignatureDocumentTemplateId" });
            DropIndex("dbo.Group", new[] { "RequiredSignatureDocumentTemplateId" });
            DropColumn("dbo.RegistrationTemplate", "SignatureDocumentAction");
            DropColumn("dbo.RegistrationTemplate", "RequiredSignatureDocumentTemplateId");
            DropColumn("dbo.Group", "RequiredSignatureDocumentTemplateId");
            DropTable("dbo.SignatureDocument");
            DropTable("dbo.SignatureDocumentTemplate");
        }
    }
}
