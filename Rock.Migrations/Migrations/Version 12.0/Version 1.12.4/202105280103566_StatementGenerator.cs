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
    public partial class StatementGenerator : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PagesBlocks_Up();

            BlockUpdates_Up();
            RenameLegacyStatementGeneratedDefinedType();
            AddRockDefaultTemplateLogo();
            AddRockDefaultTemplate();

            UpdateRestSecurity();
        }

        private bool BlockHasModifiedAttributeValues( Guid blockGuid )
        {
            var modifiedCount = this.SqlScalar( $@"SELECT count(*)
FROM AttributeValue av
JOIN Attribute a ON av.AttributeId = a.Id
JOIN BlockType bt ON bt.Id = TRY_CAST(EntityTypeQualifierValue AS INT)
	AND a.EntityTypeQualifierColumn = 'BlockTypeId'
WHERE EntityId IN (
		SELECT Id
		FROM [Block] b
		WHERE b.[Guid] = '{blockGuid}'
		)
	AND av.ModifiedDateTime IS NOT NULL" ) as int? ?? 0;

            return modifiedCount > 0;
        }

        private void BlockUpdates_Up()
        {
            var internalLegacyStatementLavaBlockGuid = "680D8BC7-9F39-45AA-A89E-D542BC7AC57D";
            var externalLegacyStatementLavaBlockGuid = "A1C9B68E-BD41-43E1-A40F-BAD33EBD4124";

            if ( !BlockHasModifiedAttributeValues( internalLegacyStatementLavaBlockGuid.AsGuid() ) )
            {
                // internal Contribution Statement Lava (Legacy)
                RockMigrationHelper.DeleteBlock( internalLegacyStatementLavaBlockGuid );

                // Add Block Contribution Statement Generator to Page: Contribution Statement, Site: Rock RMS (Internal)
                RockMigrationHelper.AddBlock( true, "98EBADAF-CCA9-4893-9DD3-D8201D8BD7FA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E0A699C3-61AA-4522-9067-1FE56FA80972".AsGuid(), "Contribution Statement Generator", "Main", @"", @"", 1, "BCC85FF4-88A5-4526-996C-DB102339B71F" );

                // Add Block Attribute Value
                //   Block: Contribution Statement Generator
                //   BlockType: Contribution Statement Generator
                //   Block Location: Page=Contribution Statement, Site=Rock RMS
                //   Attribute: Allow Person QueryString
                //   Attribute Value: True
                RockMigrationHelper.AddBlockAttributeValue( "BCC85FF4-88A5-4526-996C-DB102339B71F", "40180E49-CAC2-47A0-9234-04E3415DAA7D", @"True" );
            }

            if ( !BlockHasModifiedAttributeValues( externalLegacyStatementLavaBlockGuid.AsGuid() ) )
            {
                // external Contribution Statement Lava (Legacy)
                RockMigrationHelper.DeleteBlock( externalLegacyStatementLavaBlockGuid );

                // Add Block Contribution Statement Generator to Page: Contribution Statement, Site: External Website
                RockMigrationHelper.AddBlock( true, "FC44FC7F-5EA2-4F0E-8182-D8D6C9C75E28".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "E0A699C3-61AA-4522-9067-1FE56FA80972".AsGuid(), "Contribution Statement Generator", "Main", @"", @"", 1, "C8C9A67B-2E23-4A2B-ADA8-8137E360D203" );
            }

            // 
#pragma warning disable CS0618 // Type or member is obsolete
            Sql( $"UPDATE [BlockType] SET [Name] = 'Contribution Statement Lava ( Legacy )' WHERE [Guid] = '{Rock.SystemGuid.BlockType.CONTRIBUTION_STATEMENT_LAVA_LEGACY}'" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Updates the rest security.
        /// </summary>
        private void UpdateRestSecurity()
        {
            Sql( @"IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.StatementGenerator.Rest.FinancialGivingStatementController') 
	INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
		VALUES ( 'FinancialGivingStatement', 'Rock.StatementGenerator.Rest.FinancialGivingStatementController', NEWID() )
" );

            Sql( "DELETE FROM [Auth] WHERE [Guid] IN ('72E3DC93-50F9-4A2F-A95B-774D6A19621F', '0B8A3A47-FFBB-4B17-9FF1-3832BD8F69D8', 'B7BD3533-745D-4F0E-B745-0E0BB68606ED', '93A2D2B7-3F41-422C-A599-307485239071')" );

            Sql( @"
INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
	VALUES (
		(SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'), 
		(SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.StatementGenerator.Rest.FinancialGivingStatementController'), 
		0, 'View', 'A', 0, 
		(SELECT [Id] FROM [Group] WHERE [Guid] = '6246A7EF-B7A3-4C8C-B1E4-3FF114B84559'), 
		'72E3DC93-50F9-4A2F-A95B-774D6A19621F')


INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
	VALUES (
		(SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'), 
		(SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.StatementGenerator.Rest.FinancialGivingStatementController'), 
		1, 'View', 'A', 0, 
		(SELECT [Id] FROM [Group] WHERE [Guid] = '2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9'), 
		'0B8A3A47-FFBB-4B17-9FF1-3832BD8F69D8')


INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
	VALUES (
		(SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'), 
		(SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.StatementGenerator.Rest.FinancialGivingStatementController'), 
		2, 'View', 'A', 0, 
		(SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'), 
		'B7BD3533-745D-4F0E-B745-0E0BB68606ED')


INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
	VALUES (
		(SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D'), 
		(SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.StatementGenerator.Rest.FinancialGivingStatementController'), 
		3, 'View', 'D', 1, 
		NULL, 
		'93A2D2B7-3F41-422C-A599-307485239071')
" );

        }

        private void RenameLegacyStatementGeneratedDefinedType()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Sql( $@"UPDATE [DefinedType] SET  [Name] = 'Statement Generator Lava Template (Legacy)' WHERE [Guid] = '{Rock.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE_LEGACY}'" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void AddRockDefaultTemplateLogo()
        {
            Sql( $@"DECLARE
	@BinaryFileId int
	,@BinaryFileTypeIdDefault int = (SELECT TOP 1 Id from [BinaryFileType] where [Guid] = '{Rock.SystemGuid.BinaryFiletype.DEFAULT}')
	,@StorageEntityTypeIdDatabase int = (SELECT TOP 1 Id FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE}')

-- Add logo.jpg
IF NOT EXISTS (SELECT * FROM [BinaryFile] WHERE [Guid] = '{Rock.SystemGuid.BinaryFile.FINANCIAL_STATEMENT_TEMPLATE_ROCK_DEFAULT_LOGO}' )
BEGIN
INSERT INTO [BinaryFile] ([IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [MimeType], [StorageEntityTypeId], [Guid])
			VALUES (0,0, @BinaryFileTypeIdDefault, 'logo.jpg', 'image/jpeg', @StorageEntityTypeIdDatabase, '{Rock.SystemGuid.BinaryFile.FINANCIAL_STATEMENT_TEMPLATE_ROCK_DEFAULT_LOGO}')

SET @BinaryFileId = SCOPE_IDENTITY()

INSERT INTO [BinaryFileData] ([Id], [Content], [Guid])
  VALUES ( 
    @BinaryFileId
    ,{MigrationSQL._202105280103566_StatementGenerator_RockDefaultTemplateLogo}
    ,'{Rock.SystemGuid.BinaryFile.FINANCIAL_STATEMENT_TEMPLATE_ROCK_DEFAULT_LOGO}'
    )
END
" );
        }

        private void AddRockDefaultTemplate()
        {
            var defaultTemplate = MigrationSQL._202105280103566_StatementGenerator_RockDefaultTemplate.Replace( "'", "''" );
            var defaultTemplateName = "Rock Default";
            var defaultTemplateDescription = "The default statement generator lava template. It includes a transaction list, account summary, non-cash contributions section and a pledges section. Use this as a starting point for making a custom template. A logo size of 240 x 80px works best for this template.";

            var reportSettingsJson = MigrationSQL._202105280103566_StatementGenerator_RockDefaultTemplateReportSettings.Replace( "'", "''" );
            var footerSettingsJson = MigrationSQL._202105280103566_StatementGenerator_RockDefaultTemplateFooter.Replace( "'", "''" );

            var defaultTemplateSQL = $@"DECLARE @rockDefaultTemplateGuid UNIQUEIDENTIFIER = '{Rock.SystemGuid.FinancialStatementTemplate.ROCK_DEFAULT}'
	,@logoBinaryFileId INT = (
		SELECT TOP 1 Id
		FROM BinaryFile
		WHERE [Guid] = '{Rock.SystemGuid.BinaryFile.FINANCIAL_STATEMENT_TEMPLATE_ROCK_DEFAULT_LOGO}'
		)
	,@defaultTemplate NVARCHAR(max) = '{defaultTemplate}'
	
	,@reportSettingsJson NVARCHAR(max) = '{reportSettingsJson}'
	,@footerSettingsJson NVARCHAR(max) = '{footerSettingsJson}'

IF NOT EXISTS (
		SELECT 1
		FROM [FinancialStatementTemplate]
		WHERE [Guid] = @rockDefaultTemplateGuid
		)
BEGIN
	INSERT INTO [dbo].[FinancialStatementTemplate] (
		[Name]
		,[Description]
		,[IsActive]
		,[ReportTemplate]
		,[LogoBinaryFileId]
		,[ReportSettingsJson]
		,[FooterSettingsJson]
		,[Guid]
		)
	VALUES (
		'{defaultTemplateName}'
		,'{defaultTemplateDescription}'
		,1
		,@defaultTemplate
		,@logoBinaryFileId
		,@reportSettingsJson
		,@footerSettingsJson
		,@rockDefaultTemplateGuid
		)
END
ELSE
BEGIN
	UPDATE [dbo].[FinancialStatementTemplate]
	SET [Name] = '{defaultTemplateName}'
		,[Description] = '{defaultTemplateDescription}'
		,[ReportTemplate] = @defaultTemplate
		,[LogoBinaryFileId] = @logoBinaryFileId
		,[ReportSettingsJson] = @reportSettingsJson
		,[FooterSettingsJSON] = @footerSettingsJson
	WHERE [Guid] = @rockDefaultTemplateGuid
END";

            Sql( defaultTemplateSQL );
        }

        private void PagesBlocks_Up()
        {
            // Add Page Statement Templates to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Statement Templates", "", "B59F9E88-C6A1-463E-8FA0-DB381C617C89", "" );

            // Add Page Statement Template Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B59F9E88-C6A1-463E-8FA0-DB381C617C89", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Statement Template Detail", "", "DE55FD8A-D24C-4C01-9B7A-BD4E74E362BF", "" );

            // Add/Update BlockType Financial Statement Template Detail
            RockMigrationHelper.UpdateBlockType( "Financial Statement Template Detail", "Displays the details of the statement template.", "~/Blocks/Finance/FinancialStatementTemplateDetail.ascx", "Finance", "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA" );

            // Add/Update BlockType Financial Statement Template List
            RockMigrationHelper.UpdateBlockType( "Financial Statement Template List", "Block used to list statement templates.", "~/Blocks/Finance/FinancialStatementTemplateList.ascx", "Finance", "65057F07-85D5-4795-91A1-86D8F67A65DC" );

            // Add Block Financial Statement Template List to Page: Statement Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B59F9E88-C6A1-463E-8FA0-DB381C617C89".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "65057F07-85D5-4795-91A1-86D8F67A65DC".AsGuid(), "Financial Statement Template List", "Main", @"", @"", 0, "A2022D1F-041C-4DA6-AD96-0C1FB3076703" );

            // Add Block Financial Statement Template Detail to Page: Statement Template Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DE55FD8A-D24C-4C01-9B7A-BD4E74E362BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA".AsGuid(), "Financial Statement Template Detail", "Main", @"", @"", 0, "43EAEEC3-F2AB-4A0D-A125-E9C7943F7157" );

            // Attribute for BlockType: Financial Statement Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "65057F07-85D5-4795-91A1-86D8F67A65DC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "10A1EEC9-9853-4742-9046-52C40F8D102D" );

            // Attribute for BlockType: Financial Statement Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "65057F07-85D5-4795-91A1-86D8F67A65DC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AFD65005-BD37-400F-8574-47CAC2EFFFF7" );

            // Attribute for BlockType: Financial Statement Template List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "65057F07-85D5-4795-91A1-86D8F67A65DC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "3D04EE40-9B9B-4848-B1D8-9831058028A2" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Statement Templates, Site=Rock RMS
            //   Attribute: Detail Page
            //   Attribute Value: de55fd8a-d24c-4c01-9b7a-bd4e74e362bf
            RockMigrationHelper.AddBlockAttributeValue( "A2022D1F-041C-4DA6-AD96-0C1FB3076703", "3D04EE40-9B9B-4848-B1D8-9831058028A2", @"de55fd8a-d24c-4c01-9b7a-bd4e74e362bf" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Statement Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "A2022D1F-041C-4DA6-AD96-0C1FB3076703", "B6F2C56A-CFE0-4901-867E-51E658F39B6B", @"False" );

            // Add Block Attribute Value
            //   Block: Financial Statement Template List
            //   BlockType: Financial Statement Template List
            //   Block Location: Page=Statement Templates, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            //   Attribute Value: True
            RockMigrationHelper.AddBlockAttributeValue( "A2022D1F-041C-4DA6-AD96-0C1FB3076703", "AFD65005-BD37-400F-8574-47CAC2EFFFF7", @"True" );


            // Add/Update BlockType Contribution Statement Generator
            RockMigrationHelper.UpdateBlockType( "Contribution Statement Generator", "Block for generating a Contribution Statement", "~/Blocks/Finance/ContributionStatementGenerator.ascx", "Finance", "E0A699C3-61AA-4522-9067-1FE56FA80972" );

            // Attribute for BlockType: Contribution Statement Generator:Allow Person QueryString
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E0A699C3-61AA-4522-9067-1FE56FA80972", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Person QueryString", "AllowPersonQueryString", "Allow Person QueryString", @"Determines if any person other than the currently logged in person is allowed to be passed through the query string. For security reasons this is not allowed by default.", 0, @"False", "40180E49-CAC2-47A0-9234-04E3415DAA7D" );

            // Attribute for BlockType: Contribution Statement Generator:Statement Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E0A699C3-61AA-4522-9067-1FE56FA80972", "9E0CD807-D69F-4888-A9BE-BCD11DD083FE", "Statement Template", "FinancialStatementTemplate", "Statement Template", @"", 1, @"4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0", "0D158AF4-F98B-410B-B227-D91166903CD6" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesBlocks_Down();
        }

        private void PagesBlocks_Down()
        {
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute( "AFD65005-BD37-400F-8574-47CAC2EFFFF7" );

            // core.CustomActionsConfigs Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute( "10A1EEC9-9853-4742-9046-52C40F8D102D" );

            // Detail Page Attribute for BlockType: Financial Statement Template List
            RockMigrationHelper.DeleteAttribute( "3D04EE40-9B9B-4848-B1D8-9831058028A2" );

            // Remove Block: Financial Statement Template Detail, from Page: Statement Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "43EAEEC3-F2AB-4A0D-A125-E9C7943F7157" );

            // Remove Block: Financial Statement Template List, from Page: Statement Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A2022D1F-041C-4DA6-AD96-0C1FB3076703" );

            // Delete BlockType Financial Statement Template List
            RockMigrationHelper.DeleteBlockType( "65057F07-85D5-4795-91A1-86D8F67A65DC" ); // Financial Statement Template List

            // Delete BlockType Financial Statement Template Detail
            RockMigrationHelper.DeleteBlockType( "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA" ); // Financial Statement Template Detail

            // Delete Page Statement Template Detail from Site:Rock RMS
            RockMigrationHelper.DeletePage( "DE55FD8A-D24C-4C01-9B7A-BD4E74E362BF" ); //  Page: Statement Template Detail, Layout: Full Width, Site: Rock RMS

            // Delete Page Statement Templates from Site:Rock RMS
            RockMigrationHelper.DeletePage( "B59F9E88-C6A1-463E-8FA0-DB381C617C89" ); //  Page: Statement Templates, Layout: Full Width, Site: Rock RMS

            // Statement Template Attribute for BlockType: Contribution Statement Generator
            RockMigrationHelper.DeleteAttribute( "0D158AF4-F98B-410B-B227-D91166903CD6" );

            // Allow Person QueryString Attribute for BlockType: Contribution Statement Generator
            RockMigrationHelper.DeleteAttribute( "40180E49-CAC2-47A0-9234-04E3415DAA7D" );

            // Remove Block: Contribution Statement Generator, from Page: Contribution Statement, Site: External Website
            RockMigrationHelper.DeleteBlock( "C8C9A67B-2E23-4A2B-ADA8-8137E360D203" );

            // Remove Block: Contribution Statement Generator, from Page: Contribution Statement, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BCC85FF4-88A5-4526-996C-DB102339B71F" );

            // Delete BlockType Contribution Statement Generator
            RockMigrationHelper.DeleteBlockType( "E0A699C3-61AA-4522-9067-1FE56FA80972" ); // Contribution Statement Generator
        }
    }
}
