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
    public partial class StatementGeneratorDefaultTemplateUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddOrUpdateRockDefaultTemplate();
        }

        private void AddOrUpdateRockDefaultTemplate()
        {
            // updated template for this migration 202106042044568_StatementGeneratorDefaultTemplateUpdate
            var defaultTemplate = MigrationSQL._202106042044568_StatementGeneratorDefaultTemplateUpdate_RockDefaultTemplate.Replace( "'", "''" );
            var defaultTemplateName = "Rock Default";
            var defaultTemplateDescription = "The default statement generator lava template. It includes a transaction list, account summary, non-cash contributions section and a pledges section. Use this as a starting point for making a custom template. A logo size of 240 x 80px works best for this template.";

            // use the ReportSettings and FooterSettings from the previous
            // 202105280103566_StatementGenerator migration since they haven't changed
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

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
