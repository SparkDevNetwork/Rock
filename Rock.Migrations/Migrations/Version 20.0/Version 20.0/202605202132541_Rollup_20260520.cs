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
    public partial class Rollup_20260520 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_SetPledgeDetailHistoryLogBlockRoleToContent_Up();
            JPH_ClearBadNumberOptOutFlag_Up();
            NA_RemoveLegacyTextToGiveSettingsBlockTypeForV20_Up();
            JPH_PerformAdditionalCheckInTypeDetailChopSteps_Up();
            JPH_UpdateCheckInScheduleBuilderBlockTypeNameAndCategory_Up();

            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20/develop):
            // ----------------------------------------------------------------

            // v20.0; 283_MigrationRollupsForV20_0_1.cs
            JPH_AddExceptionListIndex_20260407_Up();

            // v20.0; 284_RemoveLegacyThemeAndProfilePagesForV20.cs
            NA_RemoveLegacyThemeAndProfilePagesForV20_Up();

            // v20.0; 285_ConvertContributionStatementLava_Blocks_to_ContributionStatementGenerator.cs
            NA_ConvertContributionStatementLavaBlocksToContributionStatementGeneratorUp();

            // v20.0; 286_ConnectionsBoardAndGridUpdates.cs
            UpdateBlockAndAttributeNamingUp();
            DeleteRedundantBoardPageAttributesUp();
            RemoveLegacyConnectionRequestBoardUp();

            // v19.0; 287_FixItemsUsingFontAwesomeIssue6766ForV19_1.cs
            JE_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up();

            // v19.0; 288_MigrationRollupsForV20_0_2.cs
            /*
                4/29/2026 - NA

                We ran into a race/timing issue with this block deletion that affects developers
                who create a fresh database and the pre-alpha deployment process (which starts
                from a v15 database or earlier). In these situations, this deletion interferes with the
                "Chop Block Types 17.1 (18.0.6)" chop job that runs in v18.0 via
                202505131801097_Rollup_20250513.cs because, by the time that job runs, the block has been
                deleted by this data migration.

                Reason: Deferring this deletion until the next migration squish, at which point
                there is no chance the block will still exist.
            */
            //NA_RemoveObsoleteAppleTVPageListObsidianBlock_Up();
            NA_RenameChoppedBlocksForV20_Up();
            NA_ReCleanupUnusedPluginManagerBlockType_Up();

            // v19.1; 289_FixCommunicationTemplateCategoriesBreadcrumb.cs
            MW_FixCommunicationTemplateCategoriesBreadcrumb_Up();

            // v17.7; 290_SetDefaultDocumentTypeAuthFromBinaryFileType.cs
            NA_SetDefaultDocumentTypeAuthFromBinaryFileType_Up();

            // v17.8; 291_HardenCoreWorkflowSecurity.cs
            AddSanitizeSqlToWorkflowSqlQueryLavaFix_Up();
            RestrictViewOnCoreWorkflowTypes_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_PerformAdditionalCheckInTypeDetailChopSteps_Down();
            JPH_UpdateCheckInScheduleBuilderBlockTypeNameAndCategory_Down();
        }

        /// <summary>
        /// Updates the role of the History Log block on the Financial Pledge Detail page to the Content role to ensure
        /// it remains visible during automatic editing transitions.
        /// </summary>
        private void NA_SetPledgeDetailHistoryLogBlockRoleToContent_Up()
        {
            Sql( $@"
        -- Set the History Log block on the Financial Pledge Detail page to Content role since we don't want it to vanish because the PledgeDetail block goes straight to autoEdit=true from the Pledge List block.
        UPDATE [Block]
         SET [Role] = {Enums.Cms.BlockRole.Content.ConvertToInt()}
        WHERE [Guid] = '2008404E-6E55-46FD-803C-7A52B626AA11' -- Added via AddHistoryLogToPledgeDetailUp() in 202406061735165_Rollup_20240606.cs
    " );
        }

        /// <summary>
        /// Fix for issue https://github.com/SparkDevNetwork/Rock/issues/6816.
        ///
        /// Prior to the companion code change (linked below), the Twilio transport set [IsMessagingOptedOut] = 1
        /// on any phone number that hit a "disable SMS" Twilio error code (landline, invalid mobile, unreachable
        /// carrier, etc.), without setting [MessagingOptedOutDateTime]. That conflated bad-number errors with
        /// genuine recipient opt-outs and caused the danger icon to persist on person profiles even after staff
        /// corrected the number.
        ///
        /// Genuine recipient opt-outs (inbound STOP keyword via SmsActionService, or - after the linked fix - the
        /// Twilio 21610 unsubscribed code) always set [MessagingOptedOutDateTime] alongside [IsMessagingOptedOut],
        /// so a NULL date paired with [IsMessagingOptedOut] = 1 is the unique fingerprint of a Twilio bad-number
        /// false positive in core Rock code paths. Clearing only those rows preserves real opt-outs and is the
        /// narrowest possible cleanup.
        ///
        /// https://github.com/SparkDevNetwork/Rock/commit/f32925769a6154ba741f3c12bd31bc4f34a5032e
        /// </summary>
        private void JPH_ClearBadNumberOptOutFlag_Up()
        {
            Sql( @"
UPDATE [PhoneNumber]
SET [IsMessagingOptedOut] = 0
WHERE [IsMessagingOptedOut] = 1
    AND [MessagingOptedOutDateTime] IS NULL;" );
        }

        private void NA_RemoveLegacyTextToGiveSettingsBlockTypeForV20_Up()
        {
            RockMigrationHelper.DeleteBlockType( "9069F894-FDA5-4546-93EB-CEC448B142AA" ); // Text To Give Settings
        }

        /// <summary>
        /// JPH: Performs the additional steps to properly chop the Check-in Type Detail block - up.
        /// </summary>
        private void JPH_PerformAdditionalCheckInTypeDetailChopSteps_Up()
        {
            // Update the preexisting CheckInTypeDetail block type(s) and any instances to reflect the new name and
            // description values. If the admins have changed the name of any instances from the previous default,
            // leave their names as-is.
            Sql( @"
DECLARE @NewBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '6CB1416A-3B25-41FD-8E60-1B94F4A64AE6');
DECLARE @LegacyObsBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '7ea2e093-2f33-4213-a33e-9e9a7a760181');

IF @NewBlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Check-in Configuration Settings'
        , [Description] = 'Displays the settings for a check-in configuration.'
    WHERE [Id] = @NewBlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Check-in Configuration Settings'
    WHERE [BlockTypeId] IN (@NewBlockTypeId, @LegacyObsBlockTypeId)
        AND [Name] = 'Check-in Type Detail';
END" );

            // A v14 migration (202206101556191_Rollup_0610.AllowCheckoutAtKiosk) split this `core_checkin_AllowCheckout`
            // attribute into `core_checkin_AllowCheckout_Kiosk` and `core_checkin_AllowCheckout_Manager`, copying every
            // existing AttributeValue into both new attributes verbatim, but it never deleted the original.
            RockMigrationHelper.DeleteAttribute( "37EB8C83-A5DC-4A9B-8816-D93F07B2A7C5" );
        }

        /// <summary>
        /// JPH: Performs the additional steps to properly chop the Check-in Type Detail block - down.
        /// </summary>
        private void JPH_PerformAdditionalCheckInTypeDetailChopSteps_Down()
        {
            // Revert the block type name and description that were changed in the up migration.
            Sql( @"
DECLARE @NewBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '6CB1416A-3B25-41FD-8E60-1B94F4A64AE6');
DECLARE @LegacyObsBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '7ea2e093-2f33-4213-a33e-9e9a7a760181');

IF @NewBlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Check-in Type Detail'
        , [Description] = 'Displays the details of a particular Check-in Type.'
    WHERE [Id] = @NewBlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Check-in Type Detail'
    WHERE [BlockTypeId] IN (@NewBlockTypeId, @LegacyObsBlockTypeId)
        AND [Name] = 'Check-in Configuration Settings';
END" );

            // There's no need to re-add the `core_checkin_AllowCheckout` attribute that was deleted in the up migration.
        }

        /// <summary>
        /// JPH: Updates the Check-in Schedule Builder block type name and category - up.
        /// </summary>
        private void JPH_UpdateCheckInScheduleBuilderBlockTypeNameAndCategory_Up()
        {
            // Update the preexisting CheckInScheduleBuilder block type(s) and any instances to reflect the new name
            // and category values. If the admins have changed the name of any instances from the previous default,
            // leave their names as-is.
            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Check-in Schedule Builder'
        , [Category] = 'Check-in > Configuration'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Check-in Schedule Builder'
    WHERE [BlockTypeId] = BlockTypeId
        AND [Name] = 'Schedule Builder';
END" );
        }

        /// <summary>
        /// JPH: Updates the Check-in Schedule Builder block type name and category - down.
        /// </summary>
        private void JPH_UpdateCheckInScheduleBuilderBlockTypeNameAndCategory_Down()
        {
            // Revert the block type name and category that were changed in the up migration:
            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Schedule Builder'
        , [Category] = 'Check-in'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Schedule Builder'
    WHERE [BlockTypeId] = BlockTypeId
        AND [Name] = 'Check-in Schedule Builder';
END" );
        }

        /// <summary>
        /// JPH: Add a post update job to add an Exception Log index to improve performance of the Exception List block.
        /// </summary>
        private void JPH_AddExceptionListIndex_20260407_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v20.0 - Add Exception Log Index for the Exception List Block",
                description: "This job will add an Exception Log index to improve performance of the Exception List block.",
                jobType: "Rock.Jobs.PostV20AddExceptionListIndex",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_200_ADD_EXCEPTION_LIST_INDEX );
        }

        /// <summary>
        /// 
        /// </summary>
        private void NA_RemoveLegacyThemeAndProfilePagesForV20_Up()
        {
            // Delete the Legacy "Rock" theme.
            Sql( @"DELETE FROM [Theme] WHERE [Name] = 'Rock' AND [RootPath] = '/Themes/Rock'" );

            Sql( @"
-- Find the parent 'People' page by its known Guid.
DECLARE @PeopleParentPageId INT =
(
    SELECT [Id]
    FROM [Page]
    WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B'
);

-- Find the specific legacy starting page under that parent.
DECLARE @LegacyPersonPagesStartingPageId INT =
(
    SELECT [Id]
    FROM [Page]
    WHERE [InternalName] = 'Person Pages V1'
      AND [IsSystem] = 1
      AND [ParentPageId] = @PeopleParentPageId
);

-- Build the full descendant tree under ""Person Pages V1"".
-- Depth 0 = direct children of the starting page.
-- Higher depth = deeper descendants.
;WITH PageTree AS
(
    SELECT
        p.[Id],
        p.[ParentPageId],
        0 AS [Depth]
    FROM [Page] p
    WHERE p.[ParentPageId] = @LegacyPersonPagesStartingPageId

    UNION ALL

    SELECT
        c.[Id],
        c.[ParentPageId],
        pt.[Depth] + 1
    FROM [Page] c
    INNER JOIN PageTree pt
        ON c.[ParentPageId] = pt.[Id]
)
SELECT
    [Id],
    [Depth]
INTO #PagesToDelete
FROM PageTree;

-- Start at the deepest level so children are deleted before parents.
DECLARE @Depth INT = (SELECT MAX([Depth]) FROM #PagesToDelete);

WHILE @Depth IS NOT NULL AND @Depth >= 0
BEGIN
    -- Delete only the pages at the current depth.
    DELETE p
    FROM [Page] p
    INNER JOIN #PagesToDelete d
        ON d.[Id] = p.[Id]
    WHERE d.[Depth] = @Depth;

    SET @Depth = @Depth - 1;
END

-- After all descendants are removed, delete the starting page itself.
DELETE FROM [Page]
WHERE [Id] = @LegacyPersonPagesStartingPageId;

-- Clean up the temp table.
DROP TABLE #PagesToDelete;
" );

            RockMigrationHelper.DeleteBlock( "19C2140D-498A-4675-B8A2-18B281736F6E" ); // "Login Status"; BlockType: Login Status
            RockMigrationHelper.DeleteBlock( "148E5996-00DE-4341-8541-20CB3FFB7C74" ); // "Menu"; BlockType: Page Menu
            RockMigrationHelper.DeleteBlock( "AE29A24E-6F85-4BC8-8C14-A8BF97A5D263" ); // "Footer Content"; BlockType: HTML Content
            RockMigrationHelper.DeleteBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" ); // "Bio"; BlockType: Person Bio (V1)
            RockMigrationHelper.DeleteBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" ); // "Family Members"; BlockType: Group Members (V1)
            RockMigrationHelper.DeleteBlock( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" ); // "Sub Page Menu"; BlockType: Page Menu
            RockMigrationHelper.DeleteBlock( "98A30DD7-8665-4C6D-B1BB-A8380E862A04" ); // "Badges 1"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "AA588E23-D34C-433A-BA3D-B0B82797A22F" ); // "Badges 2"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B" ); // "Badges 3"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "B01F3EF1-ECB9-4C7E-AB92-B45C4C29D5C0" ); // "Smart Search"; BlockType: Smart Search

            Sql( @"
    -- Identify the legacy person detail layout that is being retired.
    DECLARE @LegacyPersonDetailLayoutId INT =
    (
        SELECT [Id]
        FROM [Layout]
        WHERE [Guid] = 'F66758C6-3E3D-4598-AF4C-B317047B5987'
    );

    -- Identify the replacement layout for any pages still pointing to the legacy layout.
    DECLARE @ReplacementPersonDetailLayoutId INT =
    (
        SELECT [Id]
        FROM [Layout]
        WHERE [Guid] = '6AD84AFC-B3A1-4E30-B53B-C6E57B513839'
    );

    -- Clear the LayoutId from any remaining blocks that still reference the legacy layout.
    UPDATE [Block]
    SET [LayoutId] = NULL
    WHERE [LayoutId] = @LegacyPersonDetailLayoutId;

    -- Re-point any remaining pages from the legacy layout to the replacement layout. (not likely to be any)
    UPDATE [Page]
    SET [LayoutId] = @ReplacementPersonDetailLayoutId
    WHERE [LayoutId] = @LegacyPersonDetailLayoutId;
" );

            RockMigrationHelper.DeleteLayout( "F66758C6-3E3D-4598-AF4C-B317047B5987" ); // "PersonDetail"

            RockMigrationHelper.DeleteBlockType( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" ); // BlockType: Crm/PersonDetail/BioV1.ascx
            RockMigrationHelper.DeleteBlockType( "FC137BDA-4F05-4ECE-9899-A249C90D11FC" ); // BlockType: Crm/PersonDetail/GroupMembersV1.ascx
        }

        /// <summary>
        /// Converts legacy Contribution Statement Lava block instances to Contribution Statement Generator instances.
        /// </summary>
        private void NA_ConvertContributionStatementLavaBlocksToContributionStatementGeneratorUp()
        {
            Sql( @"
DECLARE @Now DATETIME = GETDATE();
DECLARE @BlockEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65' );
DECLARE @ContributionStatementLavaBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = 'AF986B72-ADD9-4E05-971F-1DE4EBED8667' );
DECLARE @ContributionStatementGeneratorBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E0A699C3-61AA-4522-9067-1FE56FA80972' );
DECLARE @DefaultFinancialStatementTemplateId INT = ( SELECT [Id] FROM [FinancialStatementTemplate] WHERE [Guid] = '4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0' );

DECLARE @LegacyAllowPersonQuerystringAttributeId INT = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST( @ContributionStatementLavaBlockTypeId AS NVARCHAR(200) )
        AND [Key] = 'AllowPersonQuerystring'
);

DECLARE @LegacyDisplayPledgesAttributeId INT = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST( @ContributionStatementLavaBlockTypeId AS NVARCHAR(200) )
        AND [Key] = 'DisplayPledges'
);

DECLARE @LegacyAccountsAttributeId INT = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST( @ContributionStatementLavaBlockTypeId AS NVARCHAR(200) )
        AND [Key] = 'Accounts'
);

DECLARE @GeneratorAllowPersonQueryStringAttributeId INT = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST( @ContributionStatementGeneratorBlockTypeId AS NVARCHAR(200) )
        AND [Key] = 'AllowPersonQueryString'
);

DECLARE @GeneratorFinancialStatementTemplateAttributeId INT = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = CAST( @ContributionStatementGeneratorBlockTypeId AS NVARCHAR(200) )
        AND [Key] = 'FinancialStatementTemplate'
);

IF @BlockEntityTypeId IS NULL
    OR @ContributionStatementLavaBlockTypeId IS NULL
    OR @ContributionStatementGeneratorBlockTypeId IS NULL
    OR @DefaultFinancialStatementTemplateId IS NULL
    OR @GeneratorAllowPersonQueryStringAttributeId IS NULL
    OR @GeneratorFinancialStatementTemplateAttributeId IS NULL
BEGIN
    RETURN;
END;

IF OBJECT_ID( 'tempdb..#LegacyBlocks' ) IS NOT NULL
BEGIN
    DROP TABLE #LegacyBlocks;
END;

SELECT
    b.[Id],
    b.[Name]
INTO #LegacyBlocks
FROM [Block] b
WHERE b.[BlockTypeId] = @ContributionStatementLavaBlockTypeId;

IF NOT EXISTS ( SELECT 1 FROM #LegacyBlocks )
BEGIN
    RETURN;
END;

IF OBJECT_ID( 'tempdb..#LegacyBlockSettings' ) IS NOT NULL
BEGIN
    DROP TABLE #LegacyBlockSettings;
END;

SELECT
    lb.[Id] AS [BlockId],
    dp.[Value] AS [DisplayPledgesValue],
    ac.[Value] AS [AccountsValue]
INTO #LegacyBlockSettings
FROM #LegacyBlocks lb
OUTER APPLY
(
    SELECT TOP 1 av.[Value]
    FROM [AttributeValue] av
    WHERE av.[EntityId] = lb.[Id]
        AND av.[AttributeId] = @LegacyDisplayPledgesAttributeId
) dp
OUTER APPLY
(
    SELECT TOP 1 av.[Value]
    FROM [AttributeValue] av
    WHERE av.[EntityId] = lb.[Id]
        AND av.[AttributeId] = @LegacyAccountsAttributeId
) ac;

IF OBJECT_ID( 'tempdb..#BlockTemplateMap' ) IS NOT NULL
BEGIN
    DROP TABLE #BlockTemplateMap;
END;

CREATE TABLE #BlockTemplateMap
(
    [BlockId] INT NOT NULL,
    [FinancialStatementTemplateId] INT NOT NULL,
    [FinancialStatementTemplateGuid] UNIQUEIDENTIFIER NOT NULL
);

MERGE [FinancialStatementTemplate] AS target
USING
(
    SELECT
        lb.[Id] AS [BlockId],
        lb.[Name] AS [BlockName],
        fst.[IsActive],
        fst.[ReportTemplate],
        fst.[LogoBinaryFileId],
        fst.[CreatedByPersonAliasId],
        fst.[ModifiedByPersonAliasId],
        fst.[ForeignId],
        fst.[ForeignGuid],
        fst.[ForeignKey],
        fst.[ReportSettingsJson],
        fst.[FooterSettingsJson]
    FROM #LegacyBlocks lb
    CROSS JOIN [FinancialStatementTemplate] fst
    WHERE fst.[Id] = @DefaultFinancialStatementTemplateId
) AS src
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT
    (
        [Name],
        [Description],
        [IsActive],
        [ReportTemplate],
        [LogoBinaryFileId],
        [CreatedDateTime],
        [ModifiedDateTime],
        [CreatedByPersonAliasId],
        [ModifiedByPersonAliasId],
        [Guid],
        [ForeignId],
        [ForeignGuid],
        [ForeignKey],
        [ReportSettingsJson],
        [FooterSettingsJson]
    )
    VALUES
    (
        LEFT( '(' + CAST( src.[BlockId] AS NVARCHAR(20) ) + ') ' + ISNULL( src.[BlockName], '' ), 50 ),
        'Created by conversion of the legacy ContributionStatementLava block',
        src.[IsActive],
        src.[ReportTemplate],
        src.[LogoBinaryFileId],
        @Now,
        @Now,
        src.[CreatedByPersonAliasId],
        src.[ModifiedByPersonAliasId],
        NEWID(),
        src.[ForeignId],
        src.[ForeignGuid],
        src.[ForeignKey],
        src.[ReportSettingsJson],
        src.[FooterSettingsJson]
    )
OUTPUT
    src.[BlockId],
    inserted.[Id],
    inserted.[Guid]
INTO #BlockTemplateMap ( [BlockId], [FinancialStatementTemplateId], [FinancialStatementTemplateGuid] );

DELETE av
FROM [AttributeValue] av
INNER JOIN #LegacyBlocks lb
    ON lb.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @GeneratorAllowPersonQueryStringAttributeId;

UPDATE av
SET
    av.[AttributeId] = @GeneratorAllowPersonQueryStringAttributeId,
    av.[IsPersistedValueDirty] = 1,
    av.[ValueAsBoolean] = CASE
        WHEN LOWER( LTRIM( RTRIM( ISNULL( av.[Value], '' ) ) ) ) = 'true' THEN 1
        WHEN LOWER( LTRIM( RTRIM( ISNULL( av.[Value], '' ) ) ) ) = 'false' THEN 0
        ELSE av.[ValueAsBoolean]
    END,
    av.[ModifiedDateTime] = @Now
FROM [AttributeValue] av
INNER JOIN #LegacyBlocks lb
    ON lb.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @LegacyAllowPersonQuerystringAttributeId;

UPDATE fst
SET fst.[ReportSettingsJson] = JSON_MODIFY(
    fst.[ReportSettingsJson],
    '$.PledgeSettings.AccountIds',
    JSON_QUERY(
        CASE
            WHEN LOWER( LTRIM( RTRIM( ISNULL( lbs.[DisplayPledgesValue], '' ) ) ) ) = 'true' THEN
                (
                    SELECT
                        ISNULL( '[' + STRING_AGG( CAST( fa.[Id] AS NVARCHAR(20) ), ',' ) + ']', '[]' )
                    FROM [FinancialAccount] fa
                    WHERE fa.[IsActive] = 1
                )
            ELSE '[]'
        END
    )
)
FROM [FinancialStatementTemplate] fst
INNER JOIN #BlockTemplateMap btm
    ON btm.[FinancialStatementTemplateId] = fst.[Id]
INNER JOIN #LegacyBlockSettings lbs
    ON lbs.[BlockId] = btm.[BlockId]
WHERE lbs.[DisplayPledgesValue] IS NOT NULL;

;WITH AccountSelections AS
(
    SELECT
        btm.[FinancialStatementTemplateId],
        '[' + STRING_AGG( CAST( fa.[Id] AS NVARCHAR(20) ), ',' ) + ']' AS [SelectedAccountIdsJson]
    FROM #BlockTemplateMap btm
    INNER JOIN #LegacyBlockSettings lbs
        ON lbs.[BlockId] = btm.[BlockId]
    CROSS APPLY STRING_SPLIT( ISNULL( lbs.[AccountsValue], '' ), ',' ) s
    INNER JOIN [FinancialAccount] fa
        ON fa.[Guid] = TRY_CONVERT( UNIQUEIDENTIFIER, LTRIM( RTRIM( s.[value] ) ) )
    WHERE NULLIF( LTRIM( RTRIM( ISNULL( lbs.[AccountsValue], '' ) ) ), '' ) IS NOT NULL
    GROUP BY btm.[FinancialStatementTemplateId]
)
UPDATE fst
SET fst.[ReportSettingsJson] = JSON_MODIFY(
        JSON_MODIFY( fst.[ReportSettingsJson], '$.TransactionSettings.AccountSelectionOption', 1 ),
        '$.TransactionSettings.SelectedAccountIds',
        JSON_QUERY( ISNULL( ac.[SelectedAccountIdsJson], '[]' ) )
    )
FROM [FinancialStatementTemplate] fst
INNER JOIN #BlockTemplateMap btm
    ON btm.[FinancialStatementTemplateId] = fst.[Id]
INNER JOIN #LegacyBlockSettings lbs
    ON lbs.[BlockId] = btm.[BlockId]
LEFT JOIN AccountSelections ac
    ON ac.[FinancialStatementTemplateId] = fst.[Id]
WHERE NULLIF( LTRIM( RTRIM( ISNULL( lbs.[AccountsValue], '' ) ) ), '' ) IS NOT NULL;

UPDATE av
SET
    av.[Value] = CONVERT( NVARCHAR(50), btm.[FinancialStatementTemplateGuid] ),
    av.[IsPersistedValueDirty] = 1,
    av.[ModifiedDateTime] = @Now
FROM [AttributeValue] av
INNER JOIN #BlockTemplateMap btm
    ON btm.[BlockId] = av.[EntityId]
WHERE av.[AttributeId] = @GeneratorFinancialStatementTemplateAttributeId;

INSERT INTO [AttributeValue]
(
    [IsSystem],
    [AttributeId],
    [EntityId],
    [Value],
    [Guid],
    [CreatedDateTime],
    [ModifiedDateTime],
    [IsPersistedValueDirty]
)
SELECT
    0 AS [IsSystem],
    @GeneratorFinancialStatementTemplateAttributeId AS [AttributeId],
    btm.[BlockId] AS [EntityId],
    CONVERT( NVARCHAR(50), btm.[FinancialStatementTemplateGuid] ) AS [Value],
    NEWID() AS [Guid],
    @Now AS [CreatedDateTime],
    @Now AS [ModifiedDateTime],
    1 AS [IsPersistedValueDirty]
FROM #BlockTemplateMap btm
WHERE NOT EXISTS
(
    SELECT 1
    FROM [AttributeValue] av
    WHERE av.[AttributeId] = @GeneratorFinancialStatementTemplateAttributeId
        AND av.[EntityId] = btm.[BlockId]
);

UPDATE b
SET
    b.[BlockTypeId] = @ContributionStatementGeneratorBlockTypeId,
    b.[ModifiedDateTime] = @Now
FROM [Block] b
INNER JOIN #LegacyBlocks lb
    ON lb.[Id] = b.[Id]
WHERE b.[BlockTypeId] = @ContributionStatementLavaBlockTypeId;

DELETE bt
FROM [BlockType] bt
WHERE bt.[Guid] = 'AF986B72-ADD9-4E05-971F-1DE4EBED8667'
    AND NOT EXISTS
    (
        SELECT 1
        FROM [Block] b
        WHERE b.[BlockTypeId] = bt.[Id]
    );
" );
        }

        /// <summary>
        /// KH: Renames the "Connections List" page, route, block, and related
        /// block type attributes to "Connections Hub", and removes the redundant
        /// "Connection Board Page" block settings.
        /// </summary>
        private void UpdateBlockAndAttributeNamingUp()
        {
            // ----------------------------------------------------------------
            // 1. Rename the "Connections List Page" attribute to
            //    "Connections Hub Page" on the Connection Type Navigation block.
            //    Guard on the old Key so re-runs are safe.
            Sql( @"
UPDATE [Attribute]
SET [Name] = 'Connections Hub Page'
    , [AbbreviatedName] = 'Connections Hub Page'
    , [Key] = 'ConnectionsHubPage'
    , [Description] = 'Select the page that the list, board, and grid buttons should open to view the connections hub.'
WHERE [Guid] = 'A783108E-D015-49B1-AA86-B7F18F438BCA'
    AND [Key] = 'ConnectionsListPage';" );

            // Rename the same attribute on the Connection Opportunity Navigation block.
            Sql( @"
UPDATE [Attribute]
SET [Name] = 'Connections Hub Page'
    , [AbbreviatedName] = 'Connections Hub Page'
    , [Key] = 'ConnectionsHubPage'
    , [Description] = 'Select the page that the ""View Requests"", list, board, and grid buttons should open to view the connections hub.'
WHERE [Guid] = 'D43E5BE5-3375-44E9-9FCC-93D5B7A5C7CC'
    AND [Key] = 'ConnectionsListPage';" );

            // ----------------------------------------------------------------
            // 2. Rename the "Connections List" block instance to "Connections Hub".
            //    Only rename if it still matches the original default name so we
            //    don't overwrite a customized name.
            Sql( @"
UPDATE [Block]
SET [Name] = 'Connections Hub'
WHERE [Guid] = '1422636F-548F-4F50-BF2A-D494FB936A5C'
    AND [Name] = 'Connections List';" );

            // ----------------------------------------------------------------
            // 3. Rename the CONNECTIONS_HUB page, update the page layout, and its route.
            Sql( $@"
UPDATE [Page]
SET [InternalName] = 'Connections Hub'
    , [PageTitle] = 'Connections Hub'
    , [BrowserTitle] = 'Connections Hub'
WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_HUB}'
    AND [InternalName] = 'Connections List';" );

            RockMigrationHelper.UpdatePageLayout( Rock.SystemGuid.Page.CONNECTIONS_HUB, "C2467799-BB45-4251-8EE6-F0BF27201535" );

            Sql( @"
UPDATE [PageRoute]
SET [Route] = 'people/connections/hub'
WHERE [Guid] = '565DFC73-E223-4C52-9174-11BB65700B7B'
    AND [Route] = 'people/connections/list';" );
        }

        /// <summary>
        /// KH: Removes the now-redundant "Connection Board Page" block type
        /// attributes. The unified Connections Hub page handles both list and
        /// board views, so a separate board page setting is no longer needed.
        /// DeleteAttribute also cascade-removes the associated AttributeValue rows.
        /// </summary>
        private void DeleteRedundantBoardPageAttributesUp()
        {
            // Connection Board Page attribute for BlockType: Connection Type Navigation.
            RockMigrationHelper.DeleteAttribute( "30415B17-54DD-4632-A8DD-96BB218E938C" );

            // Connection Board Page attribute for BlockType: Connection Opportunity Navigation.
            RockMigrationHelper.DeleteAttribute( "294BC369-5706-4179-AA62-9DFB68070667" );
        }

        /// <summary>
        /// KH: Removes the legacy WebForms "Connection Request Board" block type,
        /// the Connection Board page, its "Connection Request History" child page (and history block).
        /// The new Obsidian Connections Hub replaces all of this.
        /// </summary>
        private void RemoveLegacyConnectionRequestBoardUp()
        {
            // Block: History Log on Page: Connection Request History.
            RockMigrationHelper.DeleteBlock( "5667A5A7-DFE1-4BD4-93FF-21B71E3A07EA" );

            // Delete the WebForms BlockType: Connection Request Board.
            RockMigrationHelper.DeleteBlockType( "28DBE708-E99B-4879-A64D-656C030D25B5" );

            // Child Page: Connection Request History.
            RockMigrationHelper.DeletePage( "26F4DCE0-E638-4BC5-8AAB-11E75116E1FB" );

            // Page: Connection Board (CONNECTIONS_BOARD).
            RockMigrationHelper.DeletePage( "4FBCEB52-8892-4035-BDEA-112A494BE81F" );
        }

        /// <summary>
        /// Fix for issue https://github.com/SparkDevNetwork/Rock/issues/6766
        /// </summary>
        private void JE_SetDefaultImageForTemplateDefinedTypeIconAttribute_Up()
        {
            Sql( @"
UPDATE [__IconTransition]
SET [TablerClass] = 'ti-map-search', [TablerFull] = 'ti ti-map-search'
WHERE [FontAwesomeFull] = 'fa fa-search-location'

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-openid', 'fa fa-openid', 'ti-cloud-lock', 'ti ti-cloud-lock')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-video-camera', 'fa fa-video-camera', 'ti-video', 'ti ti-video')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-file-search', 'fa fa-file-search', 'ti-file-search', 'ti ti-file-search')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-warning', 'fa fa-warning', 'ti-alert-triangle', 'ti ti-alert-triangle')

INSERT INTO [__IconTransition]
([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-diamond', 'fa fa-diamond', 'ti-diamond', 'ti ti-diamond')

UPDATE [Page]
SET [IconCssClass] = 'ti ti-file-search'
WHERE [IconCssClass] = 'fa-file-search'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-map-search'
WHERE [IconCssClass] = 'fa fa-search-location'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-cloud-lock'
WHERE [IconCssClass] = 'fa fa-openid'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'

UPDATE [NoteType]
SET [IconCssClass] = 'ti ti-settings'
WHERE [IconCssClass] = 'fa fa-gogs'

UPDATE [NoteType]
SET [IconCssClass] = 'ti ti-alert-triangle'
WHERE [IconCssClass] = 'fa fa-warning'

UPDATE [Page]
SET [IconCssClass] = 'ti ti-api'
WHERE [Guid] IN ('C132F1D5-9F43-4AEB-9172-CD45138B4CEA', '32551448-8602-4200-9F69-BD4C04770F9F') 

UPDATE [ContentChannel]
SET [IconCssClass] = 'ti ti-video'
WHERE [IconCssClass] = 'fa fa-video-camera'
" );
        }
        private void NA_RenameChoppedBlocksForV20_Up()
        {
            Sql( @"
                -- Correct chopped Financial Batch List block type
                UPDATE [BlockType]
                SET [Name] = 'Financial Batch List',
                    [Description] = 'Displays a list of financial batches.'   
                WHERE [Guid] = 'AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25'

                -- Correct chopped Group Attendance Detail block type
                UPDATE [BlockType]
                SET [Name] = 'Group Attendance Detail',
                    [Description] = 'Lists the group members for a specific occurrence date time and allows selecting if they attended or not.'   
                WHERE [Guid] = 'FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B'

                -- Correct chopped Notes block type
                UPDATE [BlockType]
                SET [Name] = 'Notes',
                    [Description] = 'Context aware block for adding notes to an entity.'   
                WHERE [Guid] = '2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3'
            " );
        }

        private void NA_ReCleanupUnusedPluginManagerBlockType_Up()
        {
            // For any pre-alpha systems that already ran 202602041840490_Rollup_20260204, this will delete the blocktype
            // This goes along with the deletion of the block via commit 7a115db90ed8c60d2d6ad4088ecc181524813d2e
            Sql( @"
            -- Delete old, very obsolete (v0.1) PluginManager block
            DECLARE @PluginManagerBlockTypeId INT = ( SELECT TOP (1) [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Core/PluginManager.ascx' AND [Guid] = 'F80268E6-2625-4565-AA2E-790C5E40A119' );

            IF @PluginManagerBlockTypeId IS NOT NULL
            BEGIN
                DELETE FROM [Block]
                WHERE [BlockTypeId] = @PluginManagerBlockTypeId;

                DELETE FROM [BlockType]
                WHERE [Id] = @PluginManagerBlockTypeId;
            END
            " );
        }

        private const string CommunicationTemplateCategoriesPageGuid = "4D6DEAB3-46A0-4B27-B67B-71383EFE1171";
        private void MW_FixCommunicationTemplateCategoriesBreadcrumb_Up()
        {
            Sql( $@"
IF EXISTS ( SELECT 1 FROM [Page] WHERE [Guid] = '{CommunicationTemplateCategoriesPageGuid}' )
BEGIN
    UPDATE [Page]
    SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] = '{CommunicationTemplateCategoriesPageGuid}'
END
" );
        }

        private void NA_SetDefaultDocumentTypeAuthFromBinaryFileType_Up()
        {
            Sql( @"
DECLARE @BinaryFileEntityTypeId   INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '62AF597F-F193-412B-94EA-291CF713327D' );
DECLARE @DocumentTypeEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '18CF366F-46B6-49CA-B557-BCABD6BBD175' );

IF @BinaryFileEntityTypeId IS NOT NULL AND @DocumentTypeEntityTypeId IS NOT NULL
BEGIN
    /*
        For every DocumentType (in the Guid list below) that points to a BinaryFileType,
        copy that BinaryFileType's Auth rows onto the DocumentType -- but only when the
        DocumentType currently has no Auth rows of its own. The NOT EXISTS
        guard is correlated per DocumentType so any DocumentType that an
        administrator has already secured is left entirely untouched, and the
        statement is safely re-runnable.
    */
    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT
        @DocumentTypeEntityTypeId,
        [dt].[Id],
        [srcAuth].[Order],
        [srcAuth].[Action],
        [srcAuth].[AllowOrDeny],
        [srcAuth].[SpecialRole],
        [srcAuth].[GroupId],
        [srcAuth].[PersonAliasId],
        NEWID(),
        GETDATE(),
        GETDATE()
    FROM [DocumentType] AS [dt]
    INNER JOIN [Auth] AS [srcAuth]
        ON  [srcAuth].[EntityTypeId] = @BinaryFileEntityTypeId
        AND [srcAuth].[EntityId]     = [dt].[BinaryFileTypeId]
    WHERE [dt].[BinaryFileTypeId] IS NOT NULL
      AND [dt].[Guid] IN (
          '2FACE26D-FC22-4041-AA76-81BE4A914B5E', -- General Person Document
          'E8513F11-165D-4EDB-AC27-9204B84FB016'  -- Giving Statement
      )
      AND NOT EXISTS (
          SELECT 1
          FROM [Auth] AS [targetAuth]
          WHERE [targetAuth].[EntityTypeId] = @DocumentTypeEntityTypeId
            AND [targetAuth].[EntityId]     = [dt].[Id]
      );
END
" );
        }

        /// <summary>
        /// Adds the <c>| SanitizeSql</c> Lava filter to any 
        /// <c>{{ Workflow | Attribute:... }}</c> expression in the
        /// <c>SQLQuery</c> attribute value for of out-of-the-box Workflow Type's
        /// Action rows.
        /// </summary>
        private void AddSanitizeSqlToWorkflowSqlQueryLavaFix_Up()
        {
            /*
                5/18/26 - NA

                T-SQL has no regular-expression, so we narrow to the tiny
                set of candidate AttributeValue rows by AttributeId before
                doing any string work, then walk each row's Value one {{ ... }}
                segment at a time. Within each segment, we only insert
                " | SanitizeSql " when it (a) contains both "Workflow" and
                "Attribute:" and (b) does not already contain "SanitizeSql". This
                preserves any segment that is already correct -- including rows
                where some segments are fixed and others are not.

                Reason: AttributeValue can contain ~1B rows on large Rock
                installs; touching only the AttributeId-indexed candidate set
                keeps this migration bounded to dozens of rows regardless of
                table size.
            */
            Sql( @"
DECLARE @WorkflowActionTypeEntityTypeId INT =
    ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '23E3273A-B137-48A3-9AFF-C8DC832DDCA6' );

IF @WorkflowActionTypeEntityTypeId IS NOT NULL
BEGIN
    -- Candidate Attribute Ids: SQLQuery key on WorkflowActionType.
    DECLARE @SqlQueryAttributeIds TABLE ( [Id] INT PRIMARY KEY );
    INSERT INTO @SqlQueryAttributeIds ( [Id] )
    SELECT [Id]
    FROM   [Attribute]
    WHERE  [EntityTypeId] = @WorkflowActionTypeEntityTypeId
      AND  [Key] = 'SQLQuery';

    IF EXISTS ( SELECT 1 FROM @SqlQueryAttributeIds )
    BEGIN
        DECLARE @Id INT;
        DECLARE @Value NVARCHAR(MAX);
        DECLARE @NewValue NVARCHAR(MAX);
        DECLARE @Remaining NVARCHAR(MAX);
        DECLARE @OpenIdx INT;
        DECLARE @CloseIdx INT;
        DECLARE @Segment NVARCHAR(MAX);
        DECLARE @Inner NVARCHAR(MAX);

        -- Coarse pre-filter on Value happens AFTER AttributeId narrowing,
        -- so the LIKE only ever scans the small candidate set.
        DECLARE [WorkflowSqlQueryCursor] CURSOR LOCAL FAST_FORWARD FOR
            SELECT [av].[Id], [av].[Value]
            FROM   [AttributeValue] AS [av]
            WHERE  [av].[AttributeId] IN ( SELECT [Id] FROM @SqlQueryAttributeIds )
              AND  [av].[IsSystem] = 1
              AND  [av].[Value] LIKE '%{{%Workflow%Attribute:%}}%';

        OPEN [WorkflowSqlQueryCursor];
        FETCH NEXT FROM [WorkflowSqlQueryCursor] INTO @Id, @Value;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @NewValue = N'';
            SET @Remaining = @Value;

            -- Walk every {{ ... }} segment exactly once.
            WHILE 1 = 1
            BEGIN
                SET @OpenIdx = CHARINDEX( N'{{', @Remaining );
                IF @OpenIdx = 0 BREAK;

                SET @CloseIdx = CHARINDEX( N'}}', @Remaining, @OpenIdx );
                IF @CloseIdx = 0 BREAK;

                SET @Segment = SUBSTRING( @Remaining, @OpenIdx, @CloseIdx - @OpenIdx + 2 );

                -- Rewrite only segments that are Workflow Attribute lookups
                -- and have not already been sanitized. Whitespace around the
                -- pipe(s) inside the segment is irrelevant to these LIKE
                -- checks, which is what we want.
                IF @Segment LIKE N'%Workflow%Attribute:%'
                    AND @Segment NOT LIKE N'%SanitizeSql%'
                BEGIN
                    SET @Inner = LEFT( @Segment, LEN( @Segment ) - 2 );  -- drop trailing }}
                    SET @Inner = RTRIM( @Inner );                        -- normalize spacing before pipe
                    SET @Segment = @Inner + N' | SanitizeSql }}';
                END

                SET @NewValue = @NewValue + SUBSTRING( @Remaining, 1, @OpenIdx - 1 ) + @Segment;
                SET @Remaining = SUBSTRING( @Remaining, @CloseIdx + 2, LEN( @Remaining ) );
            END

            SET @NewValue = @NewValue + @Remaining;

            -- Only update when something actually changed. This is what
            -- makes the migration cheap to re-run.
            IF @NewValue <> @Value
            BEGIN
                UPDATE [AttributeValue]
                SET    [Value] = @NewValue,
                       [IsPersistedValueDirty] = 1,
                       [ModifiedDateTime] = GETDATE()
                WHERE  [Id] = @Id;
            END

            FETCH NEXT FROM [WorkflowSqlQueryCursor] INTO @Id, @Value;
        END

        CLOSE [WorkflowSqlQueryCursor];
        DEALLOCATE [WorkflowSqlQueryCursor];
    END
END
" );
        }

        /// <summary>
        /// Inserts the standard "Staff + Staff Like + Rock Administration
        /// Allow, All Users Deny" set of <c>View</c> Auth rows for the listed
        /// core Workflow Types, but only when the target workflow type has no
        /// existing <c>View</c> Auth rows of its own. Any where an administrator
        /// has already configured <c>View</c> security is left untouched.
        /// </summary>
        private void RestrictViewOnCoreWorkflowTypes_Up()
        {
            /*
                5/18/26 - NA

                Several core workflow types ship with no [Auth] rows for the
                'View' action, which means they fall through to the global default
                "everyone can view" behavior.

                Only insert defaults when the workflow type currently has
                NO 'View' Auth rows at all (NOT EXISTS guard). If an
                administrator has already configured 'View' security on a
                given workflow type -- even partially -- we leave it alone.
                This keeps the migration idempotent and respectful of
                Rock Admin customizations.
            */
            Sql( @"
DECLARE @WorkflowTypeEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'C9F3C4A5-1526-474D-803F-D6C7A45CBBAE' ); -- Rock.Model.WorkflowType

DECLARE @StaffWorkersGroupId            INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' ); -- RSR - Staff Workers
DECLARE @StaffLikeWorkersGroupId        INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' ); -- RSR - Staff Like Workers
DECLARE @RSRRockAdministrationGroupId   INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' ); -- RSR - Rock Administration

IF  @WorkflowTypeEntityTypeId IS NOT NULL
AND @StaffWorkersGroupId IS NOT NULL
AND @StaffLikeWorkersGroupId IS NOT NULL
AND @RSRRockAdministrationGroupId IS NOT NULL
BEGIN
    -- The workflow types we are securing.
    DECLARE @WorkflowTypeGuidsToSecure TABLE ( [Guid] UNIQUEIDENTIFIER PRIMARY KEY );
    INSERT INTO @WorkflowTypeGuidsToSecure ( [Guid] ) VALUES
        ( '885CBA61-44EA-4B4A-B6E1-289041B6A195' ), -- DISC Request
        ( '221BF486-A82C-40A7-85B7-BB44DA45582F' ), -- Person Data Error
        ( '417D8016-92DC-4F25-ACFF-A071B591FA4F' ), -- Facilities Request
        ( '51FE9641-FB8F-41BF-B09E-235900C3E53E' ), -- IT Support
        ( '036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2' ), -- Photo Request
        ( '655BE2A4-2735-4CF9-AEC8-7EF5BE92724C' ), -- Position Approval
        ( '31DDC001-C91A-4418-B375-CAB1475F7A62' ); -- Request Assessment

    DECLARE @Now DATETIME = GETDATE();

    -- Only target workflow types that currently have zero 'View' Auth rows.
    -- Anything an admin has already touched is left alone.
    ;WITH [TargetWorkflowTypes] AS
    (
        SELECT [wt].[Id]
        FROM   [WorkflowType] AS [wt]
        INNER JOIN @WorkflowTypeGuidsToSecure AS [g]
            ON [g].[Guid] = [wt].[Guid]
        WHERE  NOT EXISTS
        (
            SELECT 1
            FROM   [Auth] AS [a]
            WHERE  [a].[EntityTypeId] = @WorkflowTypeEntityTypeId
              AND  [a].[EntityId]     = [wt].[Id]
              AND  [a].[Action]       = 'View'
        )
    ),
    -- One row per (workflow type, Auth slot) combination.
    [AuthSlotsToInsert] AS
    (
        SELECT
            [t].[Id] AS [EntityId]
            ,0        AS [Order]
            ,'A'      AS [AllowOrDeny]
            ,0        AS [SpecialRole]
            ,@StaffWorkersGroupId AS [GroupId]
        FROM [TargetWorkflowTypes] AS [t]

        UNION ALL

        SELECT
            [t].[Id]
            ,1
            ,'A'
            ,0
            ,@StaffLikeWorkersGroupId
        FROM [TargetWorkflowTypes] AS [t]

        UNION ALL

        SELECT
            [t].[Id]
            ,2
            ,'A'
            ,0
            ,@RSRRockAdministrationGroupId
        FROM [TargetWorkflowTypes] AS [t]

        UNION ALL

        SELECT
            [t].[Id]
            ,3
            ,'D'
            ,1
            ,NULL
        FROM [TargetWorkflowTypes] AS [t]
    )
    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT
        @WorkflowTypeEntityTypeId,
        [s].[EntityId],
        [s].[Order],
        'View',
        [s].[AllowOrDeny],
        [s].[SpecialRole],
        [s].[GroupId],
        NULL,
        NEWID(),
        @Now,
        @Now
    FROM [AuthSlotsToInsert] AS [s];
END
" );
        }

    } // end class Rollup_20260520
}
