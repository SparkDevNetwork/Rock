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
    /// <summary>
    /// Restores BlockType and Block records that may have been removed due to a prior migration issue.
    /// </summary>
    public partial class RestoreDeletedBlockTypesAndBlocksFromPriorMigration : Rock.Migrations.RockMigration
    {
        private const string SnippetDetailBlockTypeGuid = "8B0F3048-99BA-4ED1-8DE6-6A34F498F556";
        private const string SnippetDetailPageGuid = "E315FCD1-3942-415E-BED2-E30428928955";
        private const string SnippetDetailSnippetTypeAttributeGuid = "48AE0214-5AB4-4307-ADD8-78BFB30462E0";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RestoreSnippetDetailBlockType();
            InsertSnippetDetailBlock();
            RestoreOutreachDashboardEntityAndBlockType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        #region Private Methods

        private void RestoreSnippetDetailBlockType()
        {
            /*
                A previous migration inadvertently removed the Snippet Detail BlockType

                While the original migration has since been corrected, this logic ensures that environments 
                affected by the issue are properly restored.
            */
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Snippet Detail", "Displays the details of a particular Snippet.", "Rock.Blocks.Communication.SnippetDetail", "Communication", SnippetDetailBlockTypeGuid );

            /*
               The original issue occurred because the Snippet Detail BlockType was initially seeded ( 2023 )
               with an empty-string Path value instead of NULL.

               When RockMigrationHelper.UpdateBlockTypeByGuid() was called incorrectly with a null Path parameter, 
               the underlying SQL resolved the value to an empty string. This caused the Snippet Detail BlockType
               to be deleted.

               DELETE FROM [BlockType] WHERE [Path] = ''
            
               ... that happened.

               To prevent similar issues in environments that did not run the faulty migration, 
               normalize the Path value to NULL.
           */
            Sql( $@"
                UPDATE [BlockType]
                SET [Path] = NULL
                WHERE [Path] = ''
                AND [Guid] = '{SnippetDetailBlockTypeGuid}'
            " );
        }

        private void InsertSnippetDetailBlock()
        {
            /*
                Restore a Snippet Detail block instance and default attribute on the Snippet Detail page if it was removed.
            */
            Sql( $@"
                DECLARE @SnippetDetailPageId INT = (SELECT TOP (1) [Id] FROM [Page] WHERE [Guid] = '{SnippetDetailPageGuid}');
                DECLARE @SnippetDetailBlockTypeId INT = (SELECT TOP (1) [Id] FROM [BlockType] WHERE [Guid] = '{SnippetDetailBlockTypeGuid}');
                DECLARE @SnippetDetailSnippetTypeAttributeId INT = (SELECT TOP (1) [Id] FROM [Attribute] WHERE [Guid] = '{SnippetDetailSnippetTypeAttributeGuid}');

                IF @SnippetDetailPageId IS NOT NULL AND @SnippetDetailBlockTypeId IS NOT NULL
                BEGIN
                    IF @SnippetDetailSnippetTypeAttributeId IS NOT NULL
                    BEGIN
                        UPDATE [Attribute]
                        SET [EntityTypeQualifierValue] = CAST(@SnippetDetailBlockTypeId AS VARCHAR(100))
                        WHERE [Id] = @SnippetDetailSnippetTypeAttributeId;
                    END

                    IF NOT EXISTS (
                        SELECT 1
                        FROM [Block]
                        WHERE [PageId] = @SnippetDetailPageId
                          AND [BlockTypeId] = @SnippetDetailBlockTypeId
                    )
                    BEGIN
                        INSERT INTO [Block] (
                            [IsSystem], [PageId], [LayoutId], [SiteId], [BlockTypeId], [Zone],
                            [Order], [Name], [PreHtml], [PostHtml], [OutputCacheDuration], [Guid]
                        )
                        VALUES (
                            1, @SnippetDetailPageId, NULL, NULL, @SnippetDetailBlockTypeId, 'Main',
                            0, 'Snippet Detail', '', '', 0, '757B79A1-CFE5-4952-80D8-80CD36DB172B'
                        );

                        DECLARE @SnippetDetailBlockId INT = (SELECT TOP (1) [Id] FROM [Block] WHERE [Guid] = '757B79A1-CFE5-4952-80D8-80CD36DB172B');

                        IF @SnippetDetailBlockId IS NOT NULL AND @SnippetDetailSnippetTypeAttributeId IS NOT NULL
                        BEGIN
                            INSERT INTO [AttributeValue] (
                                [IsSystem], [AttributeId], [EntityId], [Value], [Guid]
                            )
                            VALUES (
                                1, @SnippetDetailSnippetTypeAttributeId, @SnippetDetailBlockId, '{SystemGuid.SnippetType.SMS}', NEWID()
                            );
                        END
                    END
                END
            " );
        }

        private void RestoreOutreachDashboardEntityAndBlockType()
        {
            /*
                In affected databases that ran the faulty migration, the Outreach Dashboard BlockType Path value was incorrectly set to an empty string. Normalize
                this value to NULL to align with expected conventions.
            */
            Sql( $@"
                UPDATE [BlockType]
                SET [Path] = NULL
                WHERE [Path] = ''
                AND [Guid] = '{SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_BEACON_DASHBOARD}'
            " );

            /*
                Additionally, the associated EntityType record was not updated correctly during the faulty migration. Ensure the
                EntityType is updated properly.
            */
            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.MOBILE_OUTREACH_OUTREACH_DASHBOARD_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Engagement.OutreachDashboard", "Outreach Dashboard", "Rock.Blocks.Types.Mobile.Engagement.OutreachDashboard, Rock, Version=19.0.5.0, Culture=neutral, PublicKeyToken=null", false, false );
        }

        #endregion Private Methods
    }
}
