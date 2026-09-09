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
    /// Migrates the legacy "Promo List Lava" WebForms block configuration to the converted
    /// Obsidian "Promo List" block by replacing the template-based layout selection with the
    /// new "Display Style" block setting. Blocks whose "Lava Template" referenced the rotator
    /// template are switched to the "Rotator" display style so they keep their existing layout.
    /// </summary>
    public partial class PromoListDisplayStyleByTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// The block type Guid shared by the legacy "Promo List Lava" WebForms block and the
        /// converted Obsidian "Promo List" block.
        /// </summary>
        private const string PromoListBlockTypeGuid = "B8F1B648-8C5F-4529-8F8B-B564C2A19061";

        /// <summary>
        /// The Guid for the "Display Style" block setting on the converted Obsidian block.
        /// </summary>
        private const string DisplayStyleAttributeGuid = "0B8E6E2A-6F3D-4A1C-9B7E-3C5D8F2A1E4B";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                PromoListBlockTypeGuid,
                Rock.SystemGuid.FieldType.SINGLE_SELECT,
                "Display Style",
                "DisplayStyle",
                "",
                "Determines how the promotions are rendered: a card list or a rotator.",
                0,
                "Card List",
                DisplayStyleAttributeGuid );

            // Set "Display Style" to "Rotator" for any block whose legacy "Lava Template" setting
            // referenced the Promo Rotator template, so the converted block keeps the rotator layout.
            // "Display Style" was just registered above, so no value rows exist for it yet - a single
            // guarded INSERT is enough (the NOT EXISTS keeps it safe to re-run).
            Sql( $@"
                    DECLARE @BlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{PromoListBlockTypeGuid}' );
                    DECLARE @LavaTemplateAttributeId INT = (
                        SELECT [Id] FROM [Attribute]
                        WHERE [EntityTypeQualifierColumn] = 'BlockTypeId'
                            AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR( 20 ) )
                            AND [Key] = 'LavaTemplate' );
                    DECLARE @DisplayStyleAttributeId INT = (
                        SELECT [Id] FROM [Attribute]
                        WHERE [EntityTypeQualifierColumn] = 'BlockTypeId'
                            AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR( 20 ) )
                            AND [Key] = 'DisplayStyle' );

                    IF @LavaTemplateAttributeId IS NOT NULL AND @DisplayStyleAttributeId IS NOT NULL
                    BEGIN
                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
                        SELECT 0,
                            @DisplayStyleAttributeId,
                            [lava].[EntityId],
                            'Rotator',
                            NEWID()
                        FROM [AttributeValue] AS [lava]
                        WHERE [lava].[AttributeId] = @LavaTemplateAttributeId
                            AND [lava].[Value] LIKE '%PromoRotator%'
                            AND NOT EXISTS (
                                SELECT 1 FROM [AttributeValue] AS [av]
                                WHERE [av].[AttributeId] = @DisplayStyleAttributeId
                                    AND [av].[EntityId] = [lava].[EntityId] );
                    END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /*
                6/9/26 - CLAUDE

                Intentionally left empty. This is a one-way data migration that derives the new
                "Display Style" value from the legacy "Lava Template" setting. The legacy template
                values are left untouched by Up(), so no data is lost and there is nothing to
                restore. The "Display Style" attribute itself is owned by the Obsidian block code
                and is re-registered on the next block type verification regardless.
            */
        }
    }
}
