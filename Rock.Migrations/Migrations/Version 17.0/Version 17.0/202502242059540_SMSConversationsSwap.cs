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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SMSConversationsSwap : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SwapBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region KH: Swap blocks for v1.17.0.39

        private void SwapBlocksUp()
        {
            UpdateNoteTypeAttributeValues();
            SwapBlockTypesv17();
        }

        private void UpdateNoteTypeAttributeValues()
        {
            Sql( @"
        DECLARE @AttributeId INT;
        SELECT @AttributeId = [Id] 
        FROM [dbo].[Attribute] 
        WHERE [Guid] = '07AC139A-DF55-434F-80F4-A5131535F62E'

        DECLARE @ValidNoteTypes TABLE (Guid UNIQUEIDENTIFIER);
        INSERT INTO @ValidNoteTypes (Guid)
        SELECT LOWER(CAST(nt.[Guid] AS NVARCHAR(50)))
        FROM [dbo].[NoteType] nt
        INNER JOIN [EntityType] et ON et.[Id] = nt.[EntityTypeId]
        WHERE nt.[UserSelectable] = 1
        AND et.[Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'

        UPDATE av
        SET av.[Value] = STUFF((
            SELECT ',' + LOWER(CAST(vnt.Guid AS NVARCHAR(50)))
            FROM STRING_SPLIT(av.[Value], ',') AS s
            INNER JOIN @ValidNoteTypes vnt ON LOWER(s.value) = CAST(vnt.Guid AS NVARCHAR(50))
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')
        FROM [dbo].[AttributeValue] av
        WHERE av.[AttributeId] = @AttributeId" );
        }

        private void SwapBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Block Types - 1.17.0.39",
                blockTypeReplacements: new Dictionary<string, string> {
                    // Swap Block Types - 1.17.0.39
                    { "3497603B-3BE6-4262-B7E9-EC01FC7140EB", "3B052AC5-60DB-4490-BC47-C3471A2CA515" }, // SMS Conversations ( Communication )
                    // Swap Block Types - 1.17.0.33
                    { "B5EB66A1-7391-49D5-B613-5ED804A31E7B", "07BCB48D-746E-4364-80F3-C5BEB9075FC6" }, // Group Member Schedule Template Detail ( Group Scheduling )
                    { "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7", "2B8A5A3D-BF9D-4319-B7E5-06757FA44759" }, // Group Member Schedule Template List ( Group Scheduling )
                        
                    //Swap Block Types - 1.17.0.29
                    { "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" }, // Group Scheduler
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string>{
{ "37D43C21-1A4D-4B13-9555-EF0B7304EB8A",  "FutureWeeksToShow" }
            } );
        }

        #endregion
    }
}
