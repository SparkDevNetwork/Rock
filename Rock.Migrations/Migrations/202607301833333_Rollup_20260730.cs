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
    public partial class Rollup_20260730 : Rock.Migrations.RockMigration
    {
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
