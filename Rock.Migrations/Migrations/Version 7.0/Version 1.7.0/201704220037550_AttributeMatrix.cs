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
    public partial class AttributeMatrix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttributeMatrix",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeMatrixTemplateId = c.Int(nullable: false),
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
                .ForeignKey("dbo.AttributeMatrixTemplate", t => t.AttributeMatrixTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.AttributeMatrixTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AttributeMatrixItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeMatrixId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
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
                .ForeignKey("dbo.AttributeMatrix", t => t.AttributeMatrixId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.AttributeMatrixId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AttributeMatrixTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        MinimumRows = c.Int(),
                        MaximumRows = c.Int(),
                        FormattedLava = c.String(),
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

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attribute Matrix Templates", "", "6C43A9B6-EADC-4E32-854A-B40376CF8CAF", "fa fa-list-alt" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "6C43A9B6-EADC-4E32-854A-B40376CF8CAF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attribute Matrix Template Detail", "", "601DE4F6-2290-4A5A-AC96-32FB6A133C28", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Attribute Matrix Template List", "Shows a list of all attribute matrix templates", "~/Blocks/Core/AttributeMatrixTemplateList.ascx", "Core", "069554B7-983E-4653-9A28-BA39659C6D63" );
            
            RockMigrationHelper.UpdateBlockType( "Attribute Matrix Template Detail", "Displays the details of an attribute matrix template.", "~/Blocks/Core/AttributeMatrixTemplateDetail.ascx", "Core", "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5" );
            // Add Block to Page: Attribute Matrix Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6C43A9B6-EADC-4E32-854A-B40376CF8CAF", "", "069554B7-983E-4653-9A28-BA39659C6D63", "Attribute Matrix Template List", "Main", @"", @"", 0, "3F8E9247-EE5F-4137-A0E3-52B821129CB2" );
            // Add Block to Page: Attribute Matrix Template Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "601DE4F6-2290-4A5A-AC96-32FB6A133C28", "", "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "Attribute Matrix Template Detail", "Main", @"", @"", 0, "D33FF55F-3401-476A-9BB4-4608FECAA26C" );
                        
            // Attrib for BlockType: Attribute Matrix Template List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "069554B7-983E-4653-9A28-BA39659C6D63", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C6377B38-5B39-4C3D-BE2E-642C44AAD3F3" );
            // Attrib Value for Block:Attribute Matrix Template List, Attribute:Detail Page Page: Attribute Matrix Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3F8E9247-EE5F-4137-A0E3-52B821129CB2", "C6377B38-5B39-4C3D-BE2E-642C44AAD3F3", @"601de4f6-2290-4a5a-ac96-32fb6a133c28" );

            RockMigrationHelper.UpdateFieldType( "Matrix", "", "Rock", "Rock.Field.Types.MatrixFieldType", "F16FC460-DC1E-4821-9012-5F21F974C677" );

            // Move Calendar Dim page to System Settings
            // 2660D554-D161-44A1-9763-A73C60559B50 'Calendar Dimension Settings' page
            // C831428A-6ACD-4D49-9B2D-046D399E3123 system settings
            Sql( @"DECLARE @SystemSettingsPageId INT = (
		SELECT TOP 1 Id
		FROM [Page]
		WHERE [Guid] = 'C831428A-6ACD-4D49-9B2D-046D399E3123'
		)

UPDATE [Page]
SET ParentPageId = @SystemSettingsPageId
	,[Order] = (
		SELECT max([order]) + 1
		FROM [Page]
		WHERE ParentPageId = @SystemSettingsPageId
		)
WHERE [Guid] = '2660D554-D161-44A1-9763-A73C60559B50'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Attribute Matrix Template Detail, from Page: Attribute Matrix Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D33FF55F-3401-476A-9BB4-4608FECAA26C" );
            // Remove Block: Attribute Matrix Template List, from Page: Attribute Matrix Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3F8E9247-EE5F-4137-A0E3-52B821129CB2" );
            RockMigrationHelper.DeleteBlockType( "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5" ); // Attribute Matrix Template Detail
            RockMigrationHelper.DeleteBlockType( "069554B7-983E-4653-9A28-BA39659C6D63" ); // Attribute Matrix Template List
            RockMigrationHelper.DeletePage( "601DE4F6-2290-4A5A-AC96-32FB6A133C28" ); //  Page: Attribute Matrix Template Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6C43A9B6-EADC-4E32-854A-B40376CF8CAF" ); //  Page: Attribute Matrix Templates, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.AttributeMatrix", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrix", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrix", "AttributeMatrixTemplateId", "dbo.AttributeMatrixTemplate");
            DropForeignKey("dbo.AttributeMatrixTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "AttributeMatrixId", "dbo.AttributeMatrix");
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "AttributeMatrixId" });
            DropIndex("dbo.AttributeMatrix", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrix", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrix", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrix", new[] { "AttributeMatrixTemplateId" });
            DropTable("dbo.AttributeMatrixTemplate");
            DropTable("dbo.AttributeMatrixItem");
            DropTable("dbo.AttributeMatrix");
        }
    }
}
