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

using System.Collections.Generic;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 201, "1.16.4" )]
    public class MigrationRollupsForV17_0_8 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            AddWarningMessagesForActiveObsoletedBlock();
            ChopDISCBlock();
            UpdateCmsLayout();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// PA: Add warning message in the Pre-HTML of obsoleted blocks and an entry to the Admin Checklist to remove the obsoleted blocks if they are present.
        /// </summary>
        private void AddWarningMessagesForActiveObsoletedBlock()
        {
            var sqlObsoletePersonProfile = $@"
DECLARE @ObsoletePersonBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Please replace it with either the Person Profile or the Person Recent Attendances blocks.</div>')
WHERE [BlockTypeId] = @ObsoletePersonBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoletePersonBlockId)
BEGIN
-- Insert the entry to the Admin CheckList to delete the occurrences of the obsolete Person block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+1
    , 'Obsoleted Person Profile block will be removed in v17.0. Please replace it with the Person Profile or the Person Recent Attendances blocks.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Person Profile (Obsolete) block. This block will be removed in v17.0. Please replace the instances with either the Person Profile or the Person Recent Attendances blocks.</div>'
    , '80343B7C-7731-473F-9947-15A3D74ABB02'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteLocation = $@"
DECLARE @ObsoleteLocationBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '00FC1DEA-FE34-41E3-BC0A-2EE9138091EC');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use Roster, Live Metrics, or Room Settings blocks instead.</div>')
WHERE [BlockTypeId] = @ObsoleteLocationBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteLocationBlockId)
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Location from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+2
    , 'Obsoleted Locations block will be removed in v17.0. Please replace it with Roster or Live Metrics or Room Settings Blocks.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Locations block. This block will be removed in v17.0. Please replace the instances with the Roster or the Live Metrics or the Room Settings blocks.</div>'
    , '84D3F58A-B427-4348-98B9-D1121376FB30'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteContributionStatementLava = $@"
DECLARE @ObsoleteContributionStatementLavaBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'AF986B72-ADD9-4E05-971F-1DE4EBED8667');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use the Contribution Statement Generator block instead.</div>')
WHERE [BlockTypeId] = @ObsoleteContributionStatementLavaBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteContributionStatementLavaBlockId)
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Contribution Statement Lava block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+3
    , 'Obsoleted Contribution Statement Generator block will be removed in v17.0. Please replace it with Contribution Statement Generator block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Contribution Statement Lava block. This block will be removed in v17.0. Please replace the instances with Contribution Statement Generator blocks.</div>'
    , '85A4AE6F-8B52-484B-B594-C9A15EEDEBE1'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteSystemEmailListBlock = $@"
DECLARE @ObsoleteSystemEmailList INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '2645A264-D5E5-43E8-8FE2-D351F3D5435B');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use System Communication List instead.</div>')
WHERE [BlockTypeId] = @ObsoleteSystemEmailList
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteSystemEmailList AND [Guid] <> '68F10E30-BD74-49F5-B63F-DA671E31DA90')
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Person block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+4
    , 'Obsoleted System Email List block will be removed in v17.0. Please replace it System Communication List block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted System Email List block. This block will be removed in v17.0. Please replace the instances with the System Communication List block.</div>'
    , '6D0A5968-4BF3-43B0-866C-11D9779165F6'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteSystemEmailDetail = $@"
DECLARE @ObsoleteSystemEmailDetail INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '82B00455-B8CF-4673-ACF5-641B961DF59F');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use System Communication Detail block instead.</div>')
WHERE [BlockTypeId] = @ObsoleteSystemEmailDetail
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteSystemEmailDetail AND [Guid] <> '707A99EB-C24A-46BB-9230-8607E674246C')
BEGIN
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+5
    , 'Obsoleted System Email Detail block will be removed in v17.0. Please replace it with the System Communication Detail block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted System Email Detail Block. This block will be removed in v17.0. Please replaces the instances with the System Communication Detail block.</div>'
    , 'EF040A12-DEAD-43D5-84D2-8FEE0C9CF365'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            Sql( $@"
DECLARE @AdminChecklistDefinedTypeId INT = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D');
DECLARE @Order INT
SELECT @Order = ISNULL(MAX([order]) + 1, 0) FROM [DefinedValue] WHERE [DefinedTypeId] = @AdminChecklistDefinedTypeId
-- Add entry for obsoleted Person Profile block
{sqlObsoletePersonProfile}
-- Add entry for obsoleted Location block
{sqlObsoleteLocation}
-- Add entry for obsoleted Contribution Statement Lava block
{sqlObsoleteContributionStatementLava}
-- Add entry for obsoleted System Email List block
{sqlObsoleteSystemEmailListBlock}
-- Add entry for obsoleted System Email Detail block
{sqlObsoleteSystemEmailDetail}
" );
        }

        /// <summary>
        /// PA: Chop Legacy DISC Results Block with DISC block
        /// </summary>
        private void ChopDISCBlock()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop DISC block",
                blockTypeReplacements:
            new Dictionary<string, string> {
            { "0549519D-4048-4B28-89CC-94493B29BBD4", Rock.SystemGuid.BlockType.DISC }
                },
                migrationStrategy:
            "Chop",
                jobGuid:
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_DISC_BLOCK,
                blockAttributeKeysToIgnore: null );
        }

        /// <summary>
        /// KH: Fix CMS Lava Layout
        /// </summary>
        private void UpdateCmsLayout()
        {
            RockMigrationHelper.AddBlockAttributeValue( "BEDFF750-3EB8-4EE7-A8B4-23863FB0315D", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            RockMigrationHelper.DeletePage( "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Website Configuration Section
            RockMigrationHelper.DeletePage( "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channels Section
            RockMigrationHelper.DeletePage( "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Personalization Section
            RockMigrationHelper.DeletePage( "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Content Platform Section
            RockMigrationHelper.DeletePage( "82726ACD-3480-4514-A920-FE920A71C046" ); // Digital Media Applications Section
        }
    }
}
