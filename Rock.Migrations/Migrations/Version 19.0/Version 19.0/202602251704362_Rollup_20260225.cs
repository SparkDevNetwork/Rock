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
    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20260225 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // v19.0.6 Remove blocks
            NA_RemoveDynamicHeatMapBlock_Up();

            // DH: Replace legacy check-in manager layout blocks.
            DH_ReplaceLegacyCheckInManagerLayoutBlocksUp();

            // v18.3 Hotfix rollups.
            // 277
            JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersUp_20260204();

            // 278
            NA_AddPostUpdateJobToFixBrokenAchievementTypes();

            //279
            NA_Fix_HtmlContent_AlertWarningMarkup_Up();

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DH_ReplaceLegacyCheckInManagerLayoutBlocksDown();

            // 277
            JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersDown_20260204();
        }

        private void NA_RemoveDynamicHeatMapBlock_Up()
        {
            Sql( @"
DECLARE @DynamicHeatMapBlockTypeGuid UNIQUEIDENTIFIER = 'FAFBB883-D0B4-498E-91EE-CAC5652E5095';

/* -------------------------------------------------------------------
   Delete Block instances for the targeted BlockType
------------------------------------------------------------------- */
DELETE [Block]
FROM [Block]
JOIN [BlockType]
    ON [Block].[BlockTypeId] = [BlockType].[Id]
WHERE [BlockType].[Guid] IN (
      @DynamicHeatMapBlockTypeGuid 
);

/* -------------------------------------------------------------------
   Delete the targeted BlockType
------------------------------------------------------------------- */
DELETE [BlockType]
FROM [BlockType]
WHERE [BlockType].[Guid] IN (
      @DynamicHeatMapBlockTypeGuid 
);" );
        }

        private void DH_ReplaceLegacyCheckInManagerLayoutBlocksUp()
        {
            // Delete back button from Check-in Manager Left Sidebar layout.
            RockMigrationHelper.DeleteBlock( "A9A5FF01-2263-4CE3-82EB-326528BAAD98" );

            // Delete old campus context setter block from Check-in Manager Left Sidebar layout.
            RockMigrationHelper.DeleteBlock( "EC16F292-8FF8-44A4-84A3-9F64991C3BEB" );

            // Add the Check-in Context Setter block to the Header zone of the
            // Left Sidebar layout.
            RockMigrationHelper.AddBlock(
                string.Empty,
                "2669A579-48A5-4160-88EA-C3A10024E1E1",
                "3364aabf-0c5b-4bfb-8cf3-b1a80fd3ed10",
                "Check-in Context Setter",
                "Header",
                string.Empty,
                string.Empty,
                0,
                "e8a2bd74-d03b-4cbc-a597-919419433066" );

            // Add the Login Status block to the Login zone of the Left Sidebar layout.
            RockMigrationHelper.AddBlock(
                string.Empty,
                "2669A579-48A5-4160-88EA-C3A10024E1E1",
                "04712F3D-9667-4901-A49D-4507573EF7AD",
                "Login Status",
                "Login",
                string.Empty,
                string.Empty,
                0,
                "534d2859-4b6c-4098-87b5-43c45a869a17" );

            RockMigrationHelper.AddBlockAttributeValue( "534d2859-4b6c-4098-87b5-43c45a869a17",
                "ac52cf41-07d4-4382-a195-2805863de3c4",
                "1" );
        }

        private void DH_ReplaceLegacyCheckInManagerLayoutBlocksDown()
        {
            RockMigrationHelper.DeleteBlock( "534d2859-4b6c-4098-87b5-43c45a869a17" );
            RockMigrationHelper.DeleteBlock( "e8a2bd74-d03b-4cbc-a597-919419433066" );
        }

        /// <summary>
        /// JPH - spCommunication_SynchronizeListRecipients - Exclude Archived and Duplicate List Members - Up.
        /// </summary>
        private void JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersUp_20260204()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( RockMigrationSQL._202602251704362_Rollup_20260225_277_ExcludeArchivedAndDuplicateListMembers_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// JPH - spCommunication_SynchronizeListRecipients - Exclude Archived and Duplicate List Members - Down.
        /// </summary>
        private void JPH_spCommunication_SynchronizeListRecipients_ExcludeArchivedAndDuplicateListMembersDown_20260204()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( RockMigrationSQL._202602251704362_Rollup_20260225_268_ImproveSyncListRecipientsPerformance_spCommunication_SynchronizeListRecipients);

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// Add Run-Once, post update job for fixing broken achievement types that were caused by a bug in v18.0.
        /// This job will be added to the ServiceJob table during the post update process of the v18.3 update,
        /// but also note that this job guid will also be added to the startup so that it runs immediate after start.
        /// See: Rock.Migrations.RockStartup.DataMigrationsStartup />
        /// </summary>
        private void NA_AddPostUpdateJobToFixBrokenAchievementTypes()
        {
            // Note: This cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.3 - Fix Broken Achievement Types",
                description: "This job fixes broken achievement types that could exist as a result of adding new Achievement Types using the new Obsidian block (introduced in v18.0) which had a bug that failed to save the SourceEntityTypeId.",
                jobType: typeof( Rock.Jobs.PostUpdateJobs.PostV183UpdateAchievementTypes ).FullName,
                cronExpression: "0 0 20 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_183_FIX_BROKEN_ACHIEVEMENT_TYPES );
        }

        /// <summary>
        /// Updates the HTML content for a specific record to fix issue 6682.
        /// https://github.com/SparkDevNetwork/Rock/issues/6682
        /// </summary>
        /// <remarks>
        /// This migration locates the well-known <c>HtmlContent</c> record by its known <c>Guid</c>
        /// (18dbda15-5ed7-4fe8-bc30-da872f6a3c22) and replaces its <c>Content</c> field
        /// with updated HTML markup.
        /// </remarks>
        private void NA_Fix_HtmlContent_AlertWarningMarkup_Up()
        {
            Sql( @"
DECLARE @Guid UNIQUEIDENTIFIER = '18dbda15-5ed7-4fe8-bc30-da872f6a3c22';

UPDATE [HtmlContent]
SET [Content] =
N'<div class=""alert alert-danger"">
    <strong>Warning!</strong>

    <p>
        Running SQL commands directly against the database while powerful, can be extremely dangerous.
        The difference is all in your hands.
    </p>

    <p>If you are unsure of the SQL you are about to run <strong>DO NOT</strong> proceed.</p>
</div>'
WHERE [Guid] = @Guid;
" );

        }
    }
}
