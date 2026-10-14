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

    using Rock.Model;
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20261014 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20):
            // Note: These are ordered by the Rock version they are expected to
            //       run in.
            // ----------------------------------------------------------------
            // v17.8; 324_AddAllowedRedirectDomainsToSite.cs
            JPH_AddAllowedRedirectDomainsToSite_Up();

            // v17.8; 325_AddVersionSecurityNoticeBlock.cs
            DH_AddVersionSecurityNoticeBlock_Up();

            // v17.8; 326_FixCheckScannerTransactionImages.cs
            DH_AddFinancialTransactionImageEditAuth_Up();
            DH_AddFixTransactionImageMimeTypesJob_Up();

            // v17.8; 327_CleanSocialMediaAccountValues.cs
            DH_AddCleanSocialMediaAccountValuesJob_Up();

            // v17.8; 328_FixPersonImageMimeTypes.cs
            CH_AddFixPersonImageMimeTypesJob_Up();

            // v19.5; 315_FixConnectionWorkflowActivityQualifier6986.cs
            MSE_FixActivityAddedQualifierGuids_6986_Up();

            // v19.5; 317_FixPickerShortcodeApostropheTruncation7003.cs
            NA_FixPickerShortcodeApostropheTruncation7003_Up();

            // v19.5; 318_FixDuplicateGroupChatChannelKeys7007.cs
            NA_FixDuplicateGroupChatChannelKeys7007_Up();

            // v20.0; 316_AddPostV20JobToRemoveOrphanedAnonymousVisitorAliases.cs
            NA_AddPostUpdateJobToRemoveOrphanedAnonymousVisitorAliases_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // v17.8; 324_AddAllowedRedirectDomainsToSite.cs
            JPH_AddAllowedRedirectDomainsToSite_Down();
        }

        #region 324_AddAllowedRedirectDomainsToSite

        /// <summary>
        /// JPH: Add an Allowed Redirect Domains attribute to Site - up.
        /// </summary>
        private void JPH_AddAllowedRedirectDomainsToSite_Up()
        {
            RockMigrationHelper.AddOrUpdateEntityAttribute(
                entityTypeName: "Rock.Model.Site",
                fieldTypeGuid: Rock.SystemGuid.FieldType.VALUE_LIST,
                entityTypeQualifierColumn: string.Empty,
                entityTypeQualifierValue: string.Empty,
                name: "Allowed Redirect Domain(s)",
                abbreviatedName: "Allowed Redirect Domain(s)",
                description: "The external domains that individuals may be sent to after actions like logging in. The current domain, the organization's website and the public application root are always allowed. Add each domain as its own value (e.g. example.com); use *.example.com to allow any subdomain.",
                order: 0,
                defaultValue: string.Empty,
                guid: Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS,
                key: "AllowedRedirectDomains"
            );

            /*
                Seed from Allowed Frame Domains so sites that already redirect to those domains keep working.

                1. Replace line breaks (CRLF pairs, then any lone CR or LF) with spaces.
                2. Replace commas with spaces.
                3. Trim the result.
                4. Replace spaces with pipes.
                5. Replace double pipes with single pipes (twice to catch excessive delimiters).

                If we're left over with multiple pipes, that's okay;
                the ValueList field type will handle that when it parses the values.
             */
            Sql( $@"
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS}');

INSERT INTO [AttributeValue]
(
    [IsSystem]
    , [AttributeId]
    , [EntityId]
    , [Value]
    , [Guid]
    , [IsPersistedValueDirty]
)
SELECT
    0
    , @AttributeId
    , [s].[Id]
    , REPLACE(
        REPLACE(
            REPLACE(
                LTRIM(RTRIM(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE([s].[AllowedFrameDomains]
                                , CHAR(13) + CHAR(10), ' ')
                            , CHAR(13), ' ')
                        , CHAR(10), ' ')
                    , ',', ' ')
                ))
            , ' ', '|')
        , '||', '|')
      , '||', '|')
    , NEWID()
    , 1
FROM [Site] AS [s]
WHERE LTRIM(RTRIM(ISNULL([s].[AllowedFrameDomains], ''))) <> ''
    AND NOT EXISTS (
        SELECT 1
        FROM [AttributeValue] AS [av]
        WHERE [av].[AttributeId] = @AttributeId
            AND [av].[EntityId] = [s].[Id]
    );" );
        }

        /// <summary>
        /// JPH: Add an Allowed Redirect Domains attribute to Site - down.
        /// </summary>
        private void JPH_AddAllowedRedirectDomainsToSite_Down()
        {
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.SITE_ALLOWED_REDIRECT_DOMAINS );
        }

        #endregion 324_AddAllowedRedirectDomainsToSite

        #region 325_AddVersionSecurityNoticeBlock

        /// <summary>
        /// DH: Registers and adds the new version security notice block instance
        /// to the homepage.
        /// </summary>
        private void DH_AddVersionSecurityNoticeBlock_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.SecurityVersionNotice
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.SecurityVersionNotice",
                "Security Version Notice",
                "Rock.Blocks.Administration.SecurityVersionNotice, Rock.Blocks, Version=17.8.0.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                "31922DA8-A172-4B85-8FC7-BB07BCBBCDE5" );

            // Add/Update Obsidian Block Type
            //   Name:Security Version Notice
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.SecurityVersionNotice
            RockMigrationHelper.UpdateMobileBlockType( "Security Version Notice",
                "Displays a notice to administrators when the running version of Rock has known security vulnerabilities.",
                "Rock.Blocks.Administration.SecurityVersionNotice",
                "Administration",
                "82BA1CB3-7220-46D1-9B5B-BC07F6D9005A" );

            // Move all blocks in the Main zone of the Homepage down one to make room
            // for the new Security Version Notice block at the top of the zone. Only
            // move blocks if the new block does not already exist.
            Sql( @"
DECLARE @SecurityVersionNoticeBlockId INT = ( SELECT [Id] FROM [Block] WHERE [Guid] = '0BCD1E06-ECC3-4867-8B62-B3308B9EF710' )
IF @SecurityVersionNoticeBlockId IS NULL
BEGIN
    DECLARE @HomepagePageId INT = ( SELECT [Id] FROM [Page] WHERE [Guid] = '" + SystemGuid.Page.INTERNAL_HOMEPAGE + @"' )
    UPDATE [Block]
    SET [Order] = [Order] + 1
    WHERE [PageId] = @HomepagePageId AND [Zone] = 'Main'
END" );

            // Add Block
            //  Block Name: Security Version Notice
            //  Page Name: Homepage
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                SystemGuid.Page.INTERNAL_HOMEPAGE.AsGuid(),
                null,
                null,
                "82BA1CB3-7220-46D1-9B5B-BC07F6D9005A".AsGuid(),
                "Security Version Notice",
                "Main",
                @"",
                @"",
                0,
                "0BCD1E06-ECC3-4867-8B62-B3308B9EF710" );

            // Make the block only visible to RSR - Rock Administration group.
            RockMigrationHelper.AddSecurityAuthForBlock( "0BCD1E06-ECC3-4867-8B62-B3308B9EF710",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "EDC80490-A153-4C07-9566-C5FE1CF57B21" );

            RockMigrationHelper.AddSecurityAuthForBlock( "0BCD1E06-ECC3-4867-8B62-B3308B9EF710",
                1,
                Authorization.VIEW,
                false,
                null,
                SpecialRole.AllUsers,
                "EDC80490-A153-4C07-9566-C5FE1CF57B21" );
        }

        #endregion 325_AddVersionSecurityNoticeBlock

        #region 326_FixCheckScannerTransactionImages

        /// <summary>
        /// DH: Grants EDIT on FinancialTransactionImage to the finance security roles. Inserts made
        /// through the v1 REST API now require EDIT on the new entity, and unlike FinancialTransaction,
        /// FinancialBatch and FinancialPaymentDetail, this entity type never had EDIT rules for
        /// these roles.
        /// </summary>
        private void DH_AddFinancialTransactionImageEditAuth_Up()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionImage",
                0,
                Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "003703EF-7BF7-4F3A-BCB6-8C79B14ECB01" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionImage",
                1,
                Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                ( int ) SpecialRole.None,
                "8733BF9D-8F5C-46B0-A78A-0F8A0E72C3C5" );
        }

        /// <summary>
        /// DH: Adds a post update job to set the mime type of check images uploaded by the Check
        /// Scanner to image/png. The scanner uploads PNG files as application/octet-stream,
        /// which GetImage.ashx refuses to serve. This is done in a job rather than in the
        /// migration because large databases can have hundreds of thousands of these records.
        /// </summary>
        private void DH_AddFixTransactionImageMimeTypesJob_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.10 - Fix Transaction Image Mime Types",
                description: "This job will set the mime type of check images uploaded by the Check Scanner to image/png so that they can be displayed.",
                jobType: "Rock.Jobs.PostV1710FixTransactionImageMimeTypes",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_1710_FIX_TRANSACTION_IMAGE_MIME_TYPES );
        }

        #endregion 326_FixCheckScannerTransactionImages

        #region 327_CleanSocialMediaAccountValues

        /// <summary>
        /// DH: Adds a post update job to clean up Social Media Account attribute values.
        /// This is done in a job rather than in the migration because the values
        /// need to be checked in C# and large databases can have many of them.
        /// </summary>
        private void DH_AddCleanSocialMediaAccountValuesJob_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.11 - Clean Social Media Account Values",
                description: "This job will clean up Social Media Account attribute values that are not safe to display.",
                jobType: "Rock.Jobs.PostV1711CleanSocialMediaAccountValues",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_1711_CLEAN_SOCIAL_MEDIA_ACCOUNT_VALUES );
        }

        #endregion 327_CleanSocialMediaAccountValues

        #region 328_FixPersonImageMimeTypes

        /// <summary>
        /// CH: Adds a post update job to set the mime type of person photos that
        /// were saved as "octet/stream", which GetImage.ashx refuses to serve.
        /// This is done in a job rather than in the migration because the
        /// type has to be read from each file's contents, which may not be
        /// stored in the database.
        /// </summary>
        private void CH_AddFixPersonImageMimeTypesJob_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.11 - Fix Person Image Mime Types",
                description: "This job will set the mime type of person photos uploaded from mobile devices so that they can be displayed.",
                jobType: "Rock.Jobs.PostV1711FixPersonImageMimeTypes",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_1711_FIX_PERSON_IMAGE_MIME_TYPES );
        }

        #endregion 328_FixPersonImageMimeTypes

        #region 315_FixConnectionWorkflowActivityQualifier6986

        /// <summary>
        /// MSE: Rewrites broken "Activity Added" qualifier values from "|Guid||" to "|Id||".
        /// </summary>
        private void MSE_FixActivityAddedQualifierGuids_6986_Up()
        {
            /*
                8/20/2026 - MSE

                The Obsidian Connection Opportunity Detail block saved the Activity Added
                qualifier as "|<ActivityTypeGuid>||" instead of "|<ActivityTypeId>||", so
                those triggers never launched their workflow. This rebuilds the exact broken
                string for each activity type and replaces it with the correct Id form by
                whole-value equality, so no other value can match and re-running is a no-op.
                LOWER() is needed because casting a uniqueidentifier to varchar produces
                uppercase while the saved values are lowercase.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6986
            */

            const int activityAddedTriggerType = ( int ) ConnectionWorkflowTriggerType.ActivityAdded;

            Sql( $@"
UPDATE cw
SET cw.[QualifierValue] = CONCAT( '|', cat.[Id], '||' )
FROM [ConnectionWorkflow] cw
INNER JOIN [ConnectionActivityType] cat
    ON cw.[QualifierValue] = CONCAT( '|', LOWER( CAST( cat.[Guid] AS varchar(36) ) ), '||' )
WHERE cw.[TriggerType] = {activityAddedTriggerType}
" );
        }

        #endregion 315_FixConnectionWorkflowActivityQualifier6986

        #region 317_FixPickerShortcodeApostropheTruncation7003

        /// <summary>
        /// NA: Escapes the display text emitted by the Defined Value Picker and Campus Picker
        /// Lava shortcodes so values containing an apostrophe are no longer truncated.
        /// </summary>
        private void NA_FixPickerShortcodeApostropheTruncation7003_Up()
        {
            /*
                8/31/2026 - NA

                The Defined Value Picker and Campus Picker shortcodes build their options by emitting
                a nested [[ item text:'{{ value }}' ]] element, where the display text is wrapped in
                single quotes. The shortcode engine re-parses those child parameters with the pattern
                :'[^']+', which stops at the first apostrophe, so a value like "Men's Ministry" was
                truncated to "Men". Our general solution used elsewhere (for example the KPI shortcode)
                is to pipe such values through the Escape filter, which converts the apostrophe to
                &#39; - the parser no longer treats it as a delimiter and the browser renders it back
                as an apostrophe, matching how the equivalent C# controls behave.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/7003
            */

            // Escape the Defined Value Picker display text (both the Value and Description variants,
            // used across the checkbox-list and dropdown branches).
            Sql( @"
UPDATE [LavaShortcode]
SET [Markup] = REPLACE(
        REPLACE([Markup], '{{ definedvalue.Value }}', '{{ definedvalue.Value | Escape }}'),
        '{{ definedvalue.Description }}', '{{ definedvalue.Description | Escape }}'
        )
WHERE [Guid] = 'E2FC377F-EDCE-4FD3-B734-D06939E65210'
" );

            // Escape the Campus Picker display text (used across the checkbox-list and dropdown branches).
            Sql( @"
UPDATE [LavaShortcode]
SET [Markup] = REPLACE([Markup],
    '{{ campus.Name }}', '{{ campus.Name | Escape }}')
WHERE [Guid] = 'E787B188-2E0F-479E-A855-0E4ABA75C91B'
" );
        }

        #endregion 317_FixPickerShortcodeApostropheTruncation7003

        #region 318_FixDuplicateGroupChatChannelKeys7007

        /// <summary>
        /// NA: Clears the chat channel key on groups that inherited another group's key via the
        /// Copy Group action, so each affected group can be assigned its own unique chat channel.
        /// </summary>
        private void NA_FixDuplicateGroupChatChannelKeys7007_Up()
        {
            /*
                8/31/2026 - NA

                The "Copy Group" button previously cloned the source group's ChatChannelKey onto the new
                group, so copies shared the source group's external chat channel. That caused the Chat Sync
                job to add members to the wrong group's roster and to throw "An item with the same key has
                already been added". The code fix (GroupService.GenerateGroupCopy) stops new copies from
                getting the same key, but existing databases still hold the duplicated keys, and there is
                no UI to correct them.

                A Rock-originated key encodes the group's OWN Id in the form rock-group-{Id}. Any group whose
                key is rock-group-{n} with n <> its own Id received that key from the source group during a
                copy. This migration nulls those copies so the next Chat Sync assigns each its own
                rock-group-{ownId} key and channel. Within a shared-key set, the true owner (Id = embedded
                Id) is left untouched, keeping its existing channel and message history.

                We intentionally limit this to keys that are CURRENTLY shared by more than one group. A
                group with a mismatched key that is NOT shared with a live group is syncing correctly
                against its existing channel; nulling it would migrate it to a new empty channel and cause
                the old channel (and its message history) to be deleted as an orphan by the Chat-to-Rock
                sync. Those lone survivors are left alone.

                Non-integer suffixes (channels that originated in the external chat system) are excluded by
                TRY_CONVERT, so only true Rock-origin keys with a mismatched Id are cleared.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/7007
            */

            Sql( @"
;WITH Parsed AS (
    SELECT
        [Id],
        [ChatChannelKey],
        TRY_CONVERT( INT, SUBSTRING( [ChatChannelKey], LEN( 'rock-group-' ) + 1, 50 ) ) AS [EmbeddedId]
    FROM [Group]
    WHERE [ChatChannelKey] LIKE 'rock-group-%'
        AND TRY_CONVERT( INT, SUBSTRING( [ChatChannelKey], LEN( 'rock-group-' ) + 1, 50 ) ) IS NOT NULL
),
DuplicateKeys AS (
    SELECT [ChatChannelKey]
    FROM Parsed
    GROUP BY [ChatChannelKey]
    HAVING COUNT(*) > 1
)
UPDATE g
SET g.[ChatChannelKey] = NULL
FROM [Group] g
INNER JOIN Parsed p ON p.[Id] = g.[Id]
INNER JOIN DuplicateKeys d ON d.[ChatChannelKey] = p.[ChatChannelKey]
WHERE p.[EmbeddedId] <> g.[Id];
" );
        }

        #endregion 318_FixDuplicateGroupChatChannelKeys7007

        #region 316_AddPostV20JobToRemoveOrphanedAnonymousVisitorAliases

        /*
            8/24/2026 - CLAUDE

            The deletion itself is deliberately not done here. Plugin migrations
            run during the startup migration phase, so removing a backlog that
            reaches six figures on a large site, with a per-record retry for any
            foreign key violation, would directly extend the restart after an
            install.

            Registering the work as a post-update job costs one ServiceJob insert
            at migration time. DataMigrationsStartup then starts the job inside a
            Task.Run, so the deleting happens in the background.

            Reason: Keep a potentially long cleanup off the startup path.
        */

        /// <summary>
        /// NA: Add the run-once, post update job that removes Anonymous Visitor person alias
        /// records with no interactions. These accumulated because the crawler check ran
        /// after the alias had already been committed. This job will be added to the
        /// ServiceJob table during the post update process of the v20.0 update, and its
        /// guid is also added to startup so that it runs shortly after start.
        /// See: Rock.Migrations.RockStartup.DataMigrationsStartup />
        /// </summary>
        private void NA_AddPostUpdateJobToRemoveOrphanedAnonymousVisitorAliases_Up()
        {
            // Note: This cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v20.0 - Remove Orphaned Anonymous Visitor Aliases",
                description: "This job removes Anonymous Visitor PersonAlias records that were created for bot traffic whose page view interaction was subsequently discarded as a crawler. After all the operations are done, this job will delete itself.",
                jobType: typeof( Rock.Jobs.PostV20RemoveOrphanedAnonymousVisitorAliases ).FullName,
                cronExpression: "0 0 20 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_200_REMOVE_ORPHANED_ANONYMOUS_VISITOR_ALIASES );
        }

        #endregion 316_AddPostV20JobToRemoveOrphanedAnonymousVisitorAliases
    }
}
