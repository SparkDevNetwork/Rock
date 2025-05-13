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
    ///
    /// </summary>
    public partial class ModerationFlagsDataType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn( "dbo.PrayerRequest", "ModerationFlags", c => c.Int( nullable: false ) );
            MergeDuplicateSentimentEmotions();
            UpdateAIAutomationsParentEntityType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.PrayerRequest", "ModerationFlags", c => c.Long(nullable: false));
        }

        private void MergeDuplicateSentimentEmotions()
        {
            Sql( @"
IF OBJECT_ID('tempdb..#dupesToKeep') IS NOT NULL
	DROP TABLE #dupesToKeep

-- Get the duplicate values for the DefinedType keeping the
-- MIN(DefinedValue.Id) - deleting any others for the DefinedType and Value.
SELECT MIN(v.Id) IdToKeep, t.Id DefinedTypeId, v.[Value]
INTO #dupesToKeep
FROM [dbo].[DefinedValue] v
JOIN [dbo].[DefinedType] t ON t.[Id] = v.[DefinedTypeId]
WHERE t.[Guid] = 'C9751C20-DA81-4521-81DE-0099D6F598BA'
GROUP BY t.Id, v.[Value]

-- Update any assignments where the PrayerRequest
-- uses a value that will soon be deleted.
UPDATE pr SET
	SentimentEmotionValueId = d.IdToKeep
FROM PrayerRequest pr
JOIN [dbo].[DefinedValue] dv ON dv.Id = pr.SentimentEmotionValueId
JOIN #dupesToKeep d ON d.[Value] = dv.[Value]
	AND d.IdToKeep <> dv.Id

-- Now delete the duplicates that we don't intend to keep.
DELETE dv
FROM #dupesToKeep d
JOIN [dbo].[DefinedValue] dv ON dv.DefinedTypeId = d.DefinedTypeId
	AND dv.[Value] = d.[Value]
	AND dv.Id <> d.IdToKeep

DROP TABLE #dupesToKeep
" );
        }

        private void UpdateAIAutomationsParentEntityType()
        {
            Sql( @"
DECLARE @aiAutomationsCategoryGuid NVARCHAR(40) = '571B1191-7F6A-4C9F-8953-1C5B14274F3F';
DECLARE @attributeEntityTypeId INT = (SELECT Id FROM EntityType WHERE [Guid] = '5997C8D3-8840-4591-99A5-552919F90CBD');
UPDATE Category SET
	EntityTypeId = @attributeEntityTypeId
WHERE [Guid] = @aiAutomationsCategoryGuid
" );
        }
    }
}
