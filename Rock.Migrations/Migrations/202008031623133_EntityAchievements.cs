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
    using Rock.Data;

    /// <summary>
    ///
    /// </summary>
    public partial class EntityAchievements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Rock.Plugin.HotFixes.AchievementPagesAndBlocks.UpStatic( RockMigrationHelper, Sql );
            EntityTypeChangesUp();
            TableChangesUp();
            DataMigrationUp();
            PagesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesDown();
            TableChangesDown();
            EntityTypeChangesDown();
            Rock.Plugin.HotFixes.AchievementPagesAndBlocks.DownStatic( RockMigrationHelper );
        }

        private void PagesUp()
        {
            RockMigrationHelper.MovePage( SystemGuid.Page.ACHIEVEMENT_TYPES, SystemGuid.Page.ENGAGEMENT );
            RockMigrationHelper.RenamePage( SystemGuid.Page.ACHIEVEMENT_TYPES, "Achievements" );
        }

        private void PagesDown()
        {
            RockMigrationHelper.MovePage( SystemGuid.Page.ACHIEVEMENT_TYPES, SystemGuid.Page.STREAK_TYPES );
        }

        private void DataMigrationUp()
        {
            // Set defaults on achievement type knowing all existing records are related to streaks
            Sql(
@"UPDATE [AchievementType]
SET
    [SourceEntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Streak'),
    [AchieverEntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonAlias')" );

            // Transform the streak id value stored in AchieverEntityId (because of field rename) to a person alias id value
            Sql(
@"UPDATE aa
SET aa.[AchieverEntityId] = s.[PersonAliasId]
FROM 
	[AchievementAttempt] aa
	JOIN [Streak] s ON s.Id = aa.[AchieverEntityId]" );

            // Fix qualifier column values because of renamed fields
            Sql(
$@"UPDATE a
SET a.[EntityTypeQualifierColumn] = 'ComponentEntityTypeId'
FROM
    [Attribute] a
    JOIN [EntityType] et ON et.Id = a.EntityTypeId
WHERE
    a.[EntityTypeQualifierColumn] = 'AchievementEntityTypeId' AND
    et.[Guid] = '{SystemGuid.EntityType.ACHIEVEMENT_TYPE}'" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Streak", SystemGuid.EntityType.STREAK, true, true );
            Sql( $"UPDATE [EntityType] SET [IsAchievementsEnabled] = 1 WHERE [Guid] = '{ SystemGuid.EntityType.STREAK }';" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Interaction", SystemGuid.EntityType.INTERACTION, true, true );
            Sql( $"UPDATE [EntityType] SET [IsAchievementsEnabled] = 1 WHERE [Guid] = '{ SystemGuid.EntityType.INTERACTION }';" );

            RockMigrationHelper.AddEntityAttribute(
                "Rock.Model.AchievementType",
                "F1411F4A-BD4B-4F80-9A83-94026C009F4D",
                "ComponentEntityTypeId",
                "' + (SELECT Id FROM EntityType WHERE Guid = '174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD') + '",
                "Streak Type",
                string.Empty,
                "The source streak type from which achievements are earned.",
                4,
                string.Empty,
                "E926DAAE-980A-4BEE-9CF8-C3BF52F28D9D" );

            RockMigrationHelper.AddEntityAttribute(
                "Rock.Model.AchievementType",
                "F1411F4A-BD4B-4F80-9A83-94026C009F4D",
                "ComponentEntityTypeId",
                "' + (SELECT Id FROM EntityType WHERE Guid = '05D8CD17-E07D-4927-B9C4-5018F7C4B715') + '",
                "Streak Type",
                string.Empty,
                "The source streak type from which achievements are earned.",
                4,
                string.Empty,
                "BEDD14D0-450E-475C-8D9F-404DDE350530" );

            Sql(
$@"INSERT INTO AttributeValue (
	AttributeId,
	EntityId,
	Value,
	Guid,
	CreatedDateTime,
	ForeignKey,
	IsSystem
) SELECT
	a.Id,
	at.Id,
	st.Guid,
	NEWID(),
	GETDATE(),
	'Migrated from streak attempts',
	0
FROM
	AchievementType at
	JOIN StreakType st ON st.Id = at.StreakTypeId
	JOIN EntityType et ON et.Id = at.ComponentEntityTypeId
	JOIN Attribute a ON 
		a.EntityTypeQualifierColumn = 'ComponentEntityTypeId' 
		AND a.[Key] = 'StreakType'
		AND a.EntityTypeQualifierValue = at.ComponentEntityTypeId" );

            Sql(
@"UPDATE at
SET at.ComponentConfigJson = '{""StreakType"":""' + CONVERT(varchar(100), st.Guid) + '""}'
FROM 
	AchievementType at
	JOIN StreakType st ON st.Id = at.StreakTypeId" );

            DropColumn( "dbo.AchievementType", "StreakTypeId" );
        }

        private void EntityTypeChangesUp()
        {
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_ATTEMPT, "Rock.Model.StreakAchievementAttempt", "Rock.Model.AchievementAttempt", "Achievement Attempt" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE, "Rock.Model.StreakTypeAchievementType", "Rock.Model.AchievementType", "Achievement Type" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE_PREREQUISITE, "Rock.Model.StreakTypeAchievementTypePrerequisite", "Rock.Model.AchievementTypePrerequisite", "Achievement Type Prerequisite" );

            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.AccumulativeAchievement", "05D8CD17-E07D-4927-B9C4-5018F7C4B715", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.StreakAchievement", "174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD", false, true );
        }

        private void EntityTypeChangesDown()
        {
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_ATTEMPT, "Rock.Model.AchievementAttempt", "Rock.Model.StreakAchievementAttempt", "Streak Achievement Attempt" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE, "Rock.Model.AchievementType", "Rock.Model.StreakTypeAchievementType", "Streak Type Achievement Type" );
            RenameEntity( SystemGuid.EntityType.ACHIEVEMENT_TYPE_PREREQUISITE, "Rock.Model.AchievementTypePrerequisite", "Rock.Model.StreakTypeAchievementTypePrerequisite", "Streak Type Achievement Type Prerequisite" );
        }

        private void RenameEntity( string guidString, string oldName, string newName, string friendlyName )
        {
            RockMigrationHelper.UpdateEntityType( oldName, guidString, true, true );
            RockMigrationHelper.RenameEntityType( guidString, newName, friendlyName, newName + ", Rock, Version=1.11.0.20, Culture=neutral, PublicKeyToken=null", true, true );
        }

        private void TableChangesUp()
        {
            DropForeignKey( "dbo.StreakTypeAchievementType", "StreakTypeId", "dbo.StreakType" );
            DropIndex( "dbo.StreakTypeAchievementType", new[] { "StreakTypeId" } );
            RenameTable( name: "dbo.StreakTypeAchievementType", newName: "AchievementType" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "PrerequisiteStreakTypeAchievementTypeId", newName: "PrerequisiteAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "StreakTypeAchievementTypeId", newName: "AchievementTypeId" );
            RenameColumn( table: "dbo.StreakAchievementAttempt", name: "StreakTypeAchievementTypeId", newName: "AchievementTypeId" );
            RenameIndex( table: "dbo.StreakAchievementAttempt", name: "IX_StreakTypeAchievementTypeId", newName: "IX_AchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_StreakTypeAchievementTypeId", newName: "IX_AchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_PrerequisiteStreakTypeAchievementTypeId", newName: "IX_PrerequisiteAchievementTypeId" );
            AddColumn( "dbo.EntityType", "IsAchievementsEnabled", c => c.Boolean( nullable: false ) );

            DropForeignKey( "dbo.StreakAchievementAttempt", "StreakId", "dbo.Streak" );
            RenameTable( name: "dbo.StreakAchievementAttempt", newName: "AchievementAttempt" );
            RenameTable( name: "dbo.StreakTypeAchievementTypePrerequisite", newName: "AchievementTypePrerequisite" );
            DropIndex( "dbo.AchievementAttempt", new[] { "StreakId" } );
            RenameColumn( table: "dbo.AchievementType", name: "AchievementEntityTypeId", newName: "ComponentEntityTypeId" );
            RenameIndex( table: "dbo.AchievementType", name: "IX_AchievementEntityTypeId", newName: "IX_ComponentEntityTypeId" );
            AddColumn( "dbo.AchievementType", "SourceEntityTypeId", c => c.Int() );
            AddColumn( "dbo.AchievementType", "AchieverEntityTypeId", c => c.Int( nullable: false ) );
            RenameColumn( table: "dbo.AchievementAttempt", name: "StreakId", newName: "AchieverEntityId" );

            AddColumn( "dbo.AchievementType", "ComponentConfigJson", c => c.String() );
        }

        private void TableChangesDown()
        {
            DropColumn( "dbo.AchievementType", "ComponentConfigJson" );

            RenameColumn( table: "dbo.AchievementAttempt", name: "AchieverEntityId", newName: "StreakId" );
            DropColumn( "dbo.AchievementType", "AchieverEntityTypeId" );
            DropColumn( "dbo.AchievementType", "SourceEntityTypeId" );
            RenameIndex( table: "dbo.AchievementType", name: "IX_ComponentEntityTypeId", newName: "IX_AchievementEntityTypeId" );
            RenameColumn( table: "dbo.AchievementType", name: "ComponentEntityTypeId", newName: "AchievementEntityTypeId" );
            CreateIndex( "dbo.AchievementAttempt", "StreakId" );
            AddForeignKey( "dbo.StreakAchievementAttempt", "StreakId", "dbo.Streak", "Id" );
            RenameTable( name: "dbo.AchievementTypePrerequisite", newName: "StreakTypeAchievementTypePrerequisite" );
            RenameTable( name: "dbo.AchievementAttempt", newName: "StreakAchievementAttempt" );

            DropColumn( "dbo.EntityType", "IsAchievementsEnabled" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_PrerequisiteAchievementTypeId", newName: "IX_PrerequisiteStreakTypeAchievementTypeId" );
            RenameIndex( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "IX_AchievementTypeId", newName: "IX_StreakTypeAchievementTypeId" );
            RenameIndex( table: "dbo.StreakAchievementAttempt", name: "IX_AchievementTypeId", newName: "IX_StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakAchievementAttempt", name: "AchievementTypeId", newName: "StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "AchievementTypeId", newName: "StreakTypeAchievementTypeId" );
            RenameColumn( table: "dbo.StreakTypeAchievementTypePrerequisite", name: "PrerequisiteAchievementTypeId", newName: "PrerequisiteStreakTypeAchievementTypeId" );
            CreateIndex( "dbo.AchievementType", "StreakTypeId" );
            RenameTable( name: "dbo.AchievementType", newName: "StreakTypeAchievementType" );
            AddForeignKey( "dbo.StreakTypeAchievementType", "StreakTypeId", "dbo.StreakType", "Id", cascadeDelete: true );
        }
    }
}