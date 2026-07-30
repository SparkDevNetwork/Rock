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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20260730 : Rock.Migrations.RockMigration
    {
        #region 304_FixGivingJourneyStageAttributeValues6913 Guids

        /// <summary>
        /// The Guid of the "values" AttributeQualifier row for the CurrentJourneyGivingStage person attribute.
        /// </summary>
        private const string CurrentGivingJourneyStageValuesQualifierGuid = "1A9213FC-B567-4793-AF57-89F4C443FF02";

        /// <summary>
        /// The Guid of the "values" AttributeQualifier row for the PreviousJourneyGivingStage person attribute.
        /// </summary>
        private const string PreviousGivingJourneyStageValuesQualifierGuid = "4B61627F-3B3A-4150-9F79-01CB023439FC";

        #endregion

        #region 307_SunsetProtectMyMinistry Guids

        /// <summary>
        /// The full type name of the (now-removed) Protect My Ministry v1 component. This is
        /// hard-coded here because the class no longer exists in the assembly at the time this
        /// migration runs on an instance.
        /// </summary>
        private const string PmmComponentTypeName = "Rock.Security.BackgroundCheck.ProtectMyMinistry";

        /// <summary>
        /// Well-known Guids for the artifacts being removed.
        /// </summary>
        private const string PmmEntityTypeGuid = "C16856F4-3C6B-4AFB-A0B8-88A303508206";
        private const string PmmBlockGuid = "63AA839B-B6A1-4A57-A0DC-2F5B6DDA71BE";
        private const string PmmBlockTypeGuid = "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663";
        private const string PmmPageGuid = "E7F4B733-60FF-4FA3-AB17-0832E123F6F2";
        private const string PmmPageRouteGuid = "2BB14E39-6AEE-4379-8B92-ACB5EF3F700B";
        private const string PmmMvrJurisdictionDefinedTypeGuid = "2F8821E8-05B9-4CD5-9FA4-303662AAC85D";
        private const string PmmWorkflowTypeGuid = "16D12EF7-C546-4039-9036-B73D118EDC90";

        /// <summary>
        /// The system setting key that holds the currently-configured default background check provider.
        /// </summary>
        private const string DefaultBackgroundCheckProviderKey = "core_DefaultBackgroundCheckProvider";

        #endregion

        /// <summary>
        /// The page that hosts a Content Channel Item Detail block with a malformed breadcrumb setting.
        /// </summary>
        private const string ContentChannelItemDetailPageGuid = "6DFA80C3-E2A4-479F-ADDF-98EAC31169E0";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20/develop):
            // ----------------------------------------------------------------

            // v19.3; 304_FixGivingJourneyStageAttributeValues6913.cs
            ME_FixGivingJourneyStageAttributeValues6913_Up();

            // v19.3; 305_FixFinancialTransactionDetailAuth6886.cs
            NA_FixFinancialTransactionDetailAuth6886_Up();

            // v19.3; 306_FixEraFamilyAnalyticsWeekBoundaries6902.cs
            NA_FixEraFamilyAnalyticsWeekBoundaries6902_Up();

            // v20.0; 307_SunsetProtectMyMinistry.cs
            NA_SunsetProtectMyMinistry_Up();

            // ----------------------------------------------------------------
            // Rollup Migrations for v20.0.6
            // ----------------------------------------------------------------
            NA_RetireAppleTvPageListBlockTypes_Up();
            MSE_UpdateContentChannelItemBreadcrumbPageSetting_Up();
            NA_RemoveStaleWebFormsToObsidianSwapJobs_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // v20.0.6
            MSE_UpdateContentChannelItemBreadcrumbPageSetting_Down();
        }

        /// <summary>
        /// Corrects the "values" qualifier on the Current/Previous Giving Journey Stage person
        /// attributes so the option list matches the <c>GivingJourneyStage</c> enum. Fix for issue #6913.
        /// </summary>
        private void ME_FixGivingJourneyStageAttributeValues6913_Up()
        {
            /*
                7/8/26 - ME

                The giving automation overhaul (commit 16c48bfd304ae3d28d8326699988d4911d3c2061)
                changed the GivingJourneyStage enum ordering (2 = Consistent, 3 = Occasional) but did
                not update the "values" qualifier on the CurrentJourneyGivingStage and
                PreviousJourneyGivingStage person attributes, so Lava renders options 2 and 3 swapped.
                This corrects those two qualifier rows, guarded on the known-bad string so a value an
                administrator has customized is left untouched.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6913
            */
            Sql( $@"
UPDATE [AttributeQualifier]
SET [Value] = '0^Non-Giver, 1^New Giver, 2^Consistent Giver, 3^Occasional Giver, 4^Lapsed Giver, 5^Former Giver'
WHERE [Guid] IN ( '{CurrentGivingJourneyStageValuesQualifierGuid}', '{PreviousGivingJourneyStageValuesQualifierGuid}' )
    AND [Value] = '0^Non-Giver, 1^New Giver, 2^Occasional Giver, 3^Consistent Giver, 4^Lapsed Giver, 5^Former Giver';
" );
        }

        /// <summary>
        /// Corrects the security (Auth) records for the FinancialTransactionDetail model to allow
        /// Financial Admins and Financial Workers to match the FinancialTransaction model so that
        /// they can use the ModifyEntity Lava commands. Fix for issue #6886.
        /// </summary>
        private void NA_FixFinancialTransactionDetailAuth6886_Up()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionDetail", 1, "Edit", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "20FF56BB-D406-4779-AFDD-7886CD85EAE0" ); // EntityType:Rock.Model.FinancialTransactionDetail Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ),
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionDetail", 0, "Edit", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", 0, "049908E0-50DE-4137-9000-C0DBA9B86A5D" ); // EntityType:Rock.Model.FinancialTransactionDetail Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ),
        }

        /// <summary>
        /// Updates the [spCrm_FamilyAnalyticsEraDataset], [spCrm_FamilyAnalyticsGiving],
        /// and [spCrm_FamilyAnalyticsAttendance] stored procedures so that the
        /// weekly evaluation window uses the SundayDate column for its boundary
        /// comparisons. Previously the boundaries compared Attendance.StartDateTime
        /// (which includes a time-of-day) against a Sunday-midnight variable, which
        /// caused check-ins on the final Sunday of the window to be excluded and
        /// caused check-ins on the starting boundary Sunday to be incorrectly
        /// included. The mis-count could complete an eRA Core Step for people who
        /// were still actively attending and could distort the First/Last CheckIn,
        /// First/Last Gift, and gift/attendance count attributes. Fix for issue #6902.
        /// </summary>
        private void NA_FixEraFamilyAnalyticsWeekBoundaries6902_Up()
        {
            Sql( RockMigrationSQL._202607301833333_Rollup_20260730_306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsEraDataset );
            Sql( RockMigrationSQL._202607301833333_Rollup_20260730_306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsGiving );
            Sql( RockMigrationSQL._202607301833333_Rollup_20260730_306_FixEraFamilyAnalyticsWeekBoundaries6902_spCrm_FamilyAnalyticsAttendance );
        }

        /// <summary>
        /// Sunsets the original Protect My Ministry (v1) background check component in Rock v20.
        /// Removes the PMM component's EntityType, its admin Page/PageRoute/Block/BlockType, its
        /// DVR Jurisdiction Codes DefinedType, its component-configuration Attributes and
        /// AttributeValues, and marks the original "Background Check" WorkflowType inactive
        /// (renamed to "Background Check (PMM Legacy)"). If the Rock instance still has PMM set
        /// as the default background check provider when this migration runs, an entry is written
        /// to the ExceptionLog so operators can see a breadcrumb of what was removed.
        ///
        /// The WorkflowType itself is intentionally NOT deleted because historical Workflow rows
        /// reference it; the deactivation-and-rename approach preserves audit history while making
        /// it obvious the workflow is legacy. Downstream code paths (BackgroundCheckFieldType,
        /// BackgroundCheckDocument) still recognize the PMM EntityType GUID so that any legacy
        /// background-check documents stored in the single-Guid format continue to render.
        /// </summary>
        private void NA_SunsetProtectMyMinistry_Up()
        {
            NA_LogExceptionIfPmmIsStillTheDefaultProvider();
            NA_ClearDefaultBackgroundCheckProviderIfPmm();
            NA_DeactivateAndRenamePmmWorkflowType();
            NA_DeletePmmComponentAttributesAndValues();
            NA_DeletePmmAdminPageAndBlocks();
            NA_DeletePmmMvrJurisdictionDefinedType();
            NA_DeletePmmEntityType();
        }

        /// <summary>
        /// Writes a row to <c>[ExceptionLog]</c> when the PMM v1 component is still configured
        /// as the default background check provider. This gives operators a breadcrumb that
        /// their PMM configuration became inoperable when this migration ran.
        /// </summary>
        private void NA_LogExceptionIfPmmIsStillTheDefaultProvider()
        {
            Sql( $@"
IF EXISTS (
    SELECT 1
    FROM [Attribute]
    WHERE [EntityTypeId] IS NULL
      AND [EntityTypeQualifierColumn] = 'SystemSetting'
      AND [Key] = '{DefaultBackgroundCheckProviderKey}'
      AND [DefaultValue] = '{PmmComponentTypeName}'
)
BEGIN
    INSERT INTO [ExceptionLog]
        ( [HasInnerException], [ExceptionType], [Description], [Source],
          [Guid], [CreatedDateTime], [ModifiedDateTime] )
    VALUES
        ( 0,
          'System.Exception',
          'The legacy Protect My Ministry (v1) background check provider was removed by the Rock v20 sunset migration while it was still configured as the default background check provider. Configure a supported provider (Checkr, other, etc.) under Admin Tools > System Settings > Background Check.',
          'Rock.Plugin.HotFixes.SunsetProtectMyMinistry',
          NEWID(),
          SYSDATETIME(),
          SYSDATETIME() );
END
" );
        }

        /// <summary>
        /// Blanks the <c>core_DefaultBackgroundCheckProvider</c> SystemSetting if (and only if)
        /// it currently points at the removed PMM v1 type. Other provider selections are left
        /// alone.
        /// </summary>
        private void NA_ClearDefaultBackgroundCheckProviderIfPmm()
        {
            Sql( $@"
UPDATE [Attribute]
SET [DefaultValue] = ''
WHERE [EntityTypeId] IS NULL
  AND [EntityTypeQualifierColumn] = 'SystemSetting'
  AND [Key] = '{DefaultBackgroundCheckProviderKey}'
  AND [DefaultValue] = '{PmmComponentTypeName}';
" );
        }

        /// <summary>
        /// The original "Background Check" WorkflowType was authored for PMM v1. Historical
        /// Workflow instances reference it, so it cannot be safely deleted. This renames it to
        /// "Background Check (PMM Legacy)" (matching Checkr's existing rename behavior) and
        /// deactivates it so it no longer surfaces as an active workflow type.
        /// </summary>
        private void NA_DeactivateAndRenamePmmWorkflowType()
        {
            Sql( $@"
UPDATE [WorkflowType]
SET [Name] = 'Background Check (PMM Legacy)',
    [IsActive] = 0
WHERE [Guid] = '{PmmWorkflowTypeGuid}';
" );
        }

        /// <summary>
        /// Deletes the PMM component's configuration attributes (UserName, Password, Active,
        /// Order, TestMode, RequestURL, ReturnURL) and their values, plus the container-side
        /// componentized attributes qualified by the PMM EntityType Id. Uses joins on the
        /// EntityType Guid so it works regardless of Id churn between installs.
        /// </summary>
        private void NA_DeletePmmComponentAttributesAndValues()
        {
            Sql( $@"
DECLARE @PmmEntityTypeId INT =
    ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{PmmEntityTypeGuid}' );

IF @PmmEntityTypeId IS NOT NULL
BEGIN
    -- Values for attributes owned by the PMM component EntityType.
    DELETE av
    FROM [AttributeValue] av
    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeId] = @PmmEntityTypeId;

    DELETE FROM [Attribute]
    WHERE [EntityTypeId] = @PmmEntityTypeId;

    -- Container-side componentized attributes (""Active"", ""Order"") that live on the
    -- BackgroundCheckContainer service and are qualified by the PMM EntityType Id.
    DECLARE @PmmEntityTypeIdText NVARCHAR(50) = CAST( @PmmEntityTypeId AS NVARCHAR(50) );

    DELETE av
    FROM [AttributeValue] av
    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeQualifierColumn] = 'EntityTypeId'
      AND a.[EntityTypeQualifierValue] = @PmmEntityTypeIdText;

    DELETE FROM [Attribute]
    WHERE [EntityTypeQualifierColumn] = 'EntityTypeId'
      AND [EntityTypeQualifierValue] = @PmmEntityTypeIdText;
END
" );
        }

        /// <summary>
        /// Deletes the placed PMM Settings block, the PMM Settings block type, the PMM admin
        /// page route, and the PMM admin page itself. Uses <see cref="RockMigrationHelper"/>
        /// helpers so any related Auth rows are cleaned up too.
        /// </summary>
        private void NA_DeletePmmAdminPageAndBlocks()
        {
            // Block placed on the PMM admin page.
            RockMigrationHelper.DeleteBlock( PmmBlockGuid );

            // BlockType for ~/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx.
            RockMigrationHelper.DeleteBlockType( PmmBlockTypeGuid );

            // Route: /admin/system/protect-my-ministry.
            RockMigrationHelper.DeletePageRoute( PmmPageRouteGuid );

            // Page: Protect My Ministry (under Admin Tools > System Settings).
            RockMigrationHelper.DeletePage( PmmPageGuid );
        }

        /// <summary>
        /// The "Protect My Ministry DVR Jurisdiction Codes" DefinedType and its DefinedValues
        /// were only used by PMM v1's MVR search feature; no other provider references them.
        /// </summary>
        private void NA_DeletePmmMvrJurisdictionDefinedType()
        {
            RockMigrationHelper.DeleteDefinedType( PmmMvrJurisdictionDefinedTypeGuid );
        }

        /// <summary>
        /// Removes the PMM component EntityType row itself. All attributes/values pointing at
        /// it were removed by <see cref="NA_DeletePmmComponentAttributesAndValues"/> above.
        /// </summary>
        private void NA_DeletePmmEntityType()
        {
            RockMigrationHelper.DeleteEntityType( PmmEntityTypeGuid );
        }

        /// <summary>
        /// Finalizes the retirement of the obsolete Apple TV Page List blocks (both the
        /// WebForms and the short-lived Obsidian variant), which are superseded by
        /// Rock.Blocks.Tv.TvPageList (added in v1.16.7).
        /// </summary>
        private void NA_RetireAppleTvPageListBlockTypes_Up()
        {
            /*
                7/20/26 - NA

                The WebForms Apple TV Page List (BlockType 7BD1B79C-...) was chopped into an
                Obsidian variant (a759218b-...) by the "Chop Block Types 17.1 (18.0.6)"
                post-update job registered in 202505131801097_Rollup_20250513.cs. Both are now
                obsolete, replaced by the general-purpose TvPageList block. Plugin migration
                288_MigrationRollupsForV20_0_2 originally tried to delete the Obsidian block
                outright, but that raced with the chop job on fresh dev / PreAlpha DBs (chop
                job's cron had not yet fired, so the Obsidian block was still the mapping
                target). We removed the pair from the chop job's dictionary in
                202505131801097_Rollup_20250513.cs to prevent the race, and this method
                finishes the cleanup on any DB that still has these BlockTypes around.

                Reason: Finalize retirement of the obsolete AppleTvPageList block types
                       (WebForms + Obsolete Obsidian).
            */

            // Delete any leftover Block instances of either BlockType first, since the FK on
            // Block.BlockTypeId prevents deleting the BlockType rows until their instances are
            // gone. On production DBs the chop job's own ChopBlock() step already removed the
            // WebForms instances; on fresh dev / PreAlpha DBs the WebForms instances may still
            // be sitting on the standard Apple TV admin pages (the v1.16.7 migration placed a
            // TvPageList instance on those pages alongside them, so removing the old ones does
            // not leave the pages without a list block).
            Sql( @"
        DELETE FROM [Block]
        WHERE [BlockTypeId] IN (
            SELECT [Id] FROM [BlockType]
            WHERE [Guid] IN (
                '7BD1B79C-BF27-42C6-8359-F80EC7FEE397',
                'a759218b-1c72-446c-8994-8559ba72941e'
            )
        );
    " );

            // BlockType: AppleTvPageList (WebForms). Idempotent no-op on any DB where the chop
            // job already deleted it.
            RockMigrationHelper.DeleteBlockType( "7BD1B79C-BF27-42C6-8359-F80EC7FEE397" );

            // BlockType: AppleTvPageList (Obsolete Obsidian).
            RockMigrationHelper.DeleteBlockType( "a759218b-1c72-446c-8994-8559ba72941e" );

            // EntityType: AppleTvPageList (Obsolete Obsidian).
            RockMigrationHelper.DeleteEntityType( "4e89a96e-88a2-4ca4-a86b-b9ffdcacf49f" );
        }

        /// <summary>
        /// MSE: Updates the Content Channel Item Detail page breadcrumb setting - up.
        /// </summary>
        private void MSE_UpdateContentChannelItemBreadcrumbPageSetting_Up()
        {
            // Turn off "Show Name in Breadcrumb" so the page stops adding an extra
            // crumb after the Content Channel Item Detail block's own trail.
            Sql( $@"
        UPDATE [Page]
        SET [BreadCrumbDisplayName] = 0
        WHERE [Guid] = '{ContentChannelItemDetailPageGuid}';" );
        }

        /// <summary>
        /// MSE: Updates the Content Channel Item Detail page breadcrumb setting - down.
        /// </summary>
        private void MSE_UpdateContentChannelItemBreadcrumbPageSetting_Down()
        {
            // Restore the original "Show Name in Breadcrumb" value.
            Sql( $@"
        UPDATE [Page]
        SET [BreadCrumbDisplayName] = 1
        WHERE [Guid] = '{ContentChannelItemDetailPageGuid}';" );
        }

        /// <summary>
        /// Removes three single-pair WebForms->Obsidian swap/chop service jobs whose Obsidian
        /// target BlockType.Guid values were later reassigned to their WebForms counterparts
        /// in code (April 2026). Only fresh dev / PreAlpha databases carry these jobs at this
        /// point; on any normally-upgraded environment they completed their work years ago
        /// and no longer exist.
        /// </summary>
        private void NA_RemoveStaleWebFormsToObsidianSwapJobs_Up()
        {
            /*
                7/20/26 - NA

                On a normally-upgraded Rock instance (one that progressed through v15/v16/v17
                as releases shipped over time), these three swap jobs were registered by their
                original EF migrations, fired on their original cron, successfully migrated the
                WebForms block instances into the pre-reassignment Obsidian BlockTypes, and
                then self-deleted (or were manually cleaned up long ago). By the time the
                April 2026 guid reassignment happened, the jobs were already gone -- there was
                never anything to fail.

                Fresh dev databases and PreAlpha refreshes are different: they get seeded from
                CreateDatabase and then run the accumulated migration set in one sitting,
                including the older ones that (re)register these jobs. But by that point the
                current code has already reassigned each Obsidian BlockType.Guid to match its
                WebForms counterpart, so the target guid the job was told to migrate INTO
                (308DBA32-... / F1950524-... / D87B84DC-...) no longer resolves. The job's
                cron then fails every night with
                    "BlockType could not be found for guid ... for the new block".

                The reassignment already collapsed each WebForms/Obsidian BlockType pair into
                a single row and the existing Block instances are already schema-aligned with
                the Obsidian block (verified via attribute-key diff on a fresh DB), so the
                jobs have no useful work left to do. Deleting them is a no-op on any
                normally-upgraded DB (WHERE clause matches nothing) and it cleans up the
                error spam on fresh dev / PreAlpha DBs.

                Reason: Delete the (only-on-fresh-DBs) broken WebForms->Obsidian swap jobs
                whose Obsidian target guids were reassigned out from under them.
            */
            Sql( $@"
        -- v15 job: Group Attendance Detail
        --   Registered by 202303231713546_Rollup_20230323.cs
        --   Pair FC6B5DC8-... -> 308DBA32-...; target 308DBA32-... was reassigned to FC6B5DC8-...
        DELETE FROM [ServiceJob]
        WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}';

        -- v16.1 job: Financial Batch List
        --   Registered by 202311021741313_Rollup_20231101.cs
        --   Pair AB345CE7-... -> F1950524-...; target F1950524-... was reassigned to AB345CE7-...
        DELETE FROM [ServiceJob]
        WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_161_SWAP_FINANCIAL_BATCH_LIST}';

        -- v16.0 job: Notes
        --   Registered by 202308112104015_ReplaceWeformsBlocksWithObsidianBlocks.cs
        --   Pair 2E9F32D4-... -> D87B84DC-...; target D87B84DC-... was reassigned to 2E9F32D4-...
        DELETE FROM [ServiceJob]
        WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_SWAP_NOTES_BLOCK}';
    " );
        }
    }
}
