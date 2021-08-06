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
    public partial class AchievementType : Rock.Migrations.RockMigration
    {
        // <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelChanges_Up();

            Sql( MigrationSQL._202107141741515_AchievementType_AddMedalBinaryFile );
            Sql( MigrationSQL._202107141741515_AchievementType_AddTrophyBinaryFile );
            Sql( MigrationSQL._202107141741515_AchievementType_AddWeeklyAttendanceStreakType );
            Sql( MigrationSQL._202107141741515_AchievementType_AddAchievementTypes );

            AddOrUpdateAchievementTypeAttributes();

            AddOrUpdateAchievementTypeAttributeValues();
        }

        private void ModelChanges_Up()
        {
            AddColumn( "dbo.AchievementType", "IsPublic", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.AchievementType", "ImageBinaryFileId", c => c.Int() );
            AddColumn( "dbo.AchievementType", "CustomSummaryLavaTemplate", c => c.String() );
            CreateIndex( "dbo.AchievementType", "ImageBinaryFileId" );
            AddForeignKey( "dbo.AchievementType", "ImageBinaryFileId", "dbo.BinaryFile", "Id" );

            // We want the "AchievementTypes" GroupType attribute to only apply to GroupType's with a GroupTypePurpose of 'Checkin Template'
            // So, we need to get the checkInTemplatePurposeValueId for that so we can use it as the EntityTypeQualifierValue
            var checkInTemplatePurposeValueId = SqlScalar( $" SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE}'" ) as int?;

            // Add the "AchievementTypes" group type attribute with a EntityTypeQualifier of GroupTypePurposeValueId
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.TEXT, "GroupTypePurposeValueId", checkInTemplatePurposeValueId?.ToString(), "Achievement Types", "", 0, "", "EECDA094-E5E2-4A47-804D-65701590F2A1", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ACHIEVEMENT_TYPES );

            // Add the "AchievementTypes" group type attribute with a EntityTypeQualifier of GroupTypePurposeValueId
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.TEXT, "GroupTypePurposeValueId", checkInTemplatePurposeValueId?.ToString(), "Success Template Display Mode", "", 0, "", "B30236D5-F25F-4CC6-8ED1-76E02E71F042", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_SUCCESS_LAVA_TEMPLATE_OVERRIDE_DISPLAY_MODE );
        }

        private void AddOrUpdateAchievementTypeAttributeValues()
        {
            RockMigrationHelper.AddAchievementTypeAttributeValue( Rock.SystemGuid.AchievementType.TWENTY_WEEKS_IN_A_YEAR, Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_STREAK_TYPE, Rock.SystemGuid.StreakType.WEEKLY_ATTENDANCE );
            RockMigrationHelper.AddAchievementTypeAttributeValue( Rock.SystemGuid.AchievementType.TWENTY_WEEKS_IN_A_YEAR, Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_NUMBER_TO_ACCUMULATE, "20" );
            RockMigrationHelper.AddAchievementTypeAttributeValue( Rock.SystemGuid.AchievementType.TWENTY_WEEKS_IN_A_YEAR, Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_TIME_SPAN_IN_DAYS, "365" );

            RockMigrationHelper.AddAchievementTypeAttributeValue( Rock.SystemGuid.AchievementType.TEN_WEEKS_IN_A_ROW, Rock.SystemGuid.Attribute.STREAK_ACHIEVEMENT_STREAK_TYPE, Rock.SystemGuid.StreakType.WEEKLY_ATTENDANCE );
            RockMigrationHelper.AddAchievementTypeAttributeValue( Rock.SystemGuid.AchievementType.TEN_WEEKS_IN_A_ROW, Rock.SystemGuid.Attribute.STREAK_ACHIEVEMENT_NUMBER_TO_ACHIEVE, "10" );
        }

        private void AddOrUpdateAchievementTypeAttributes()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.AccumulativeAchievement", Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.StreakAchievement", Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.StepProgramAchievement", Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement", Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, false, true );


            // Rock.Achievement.Component.AccumulativeAchievement - Active
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", @"Should Service be used?", 0, @"False", "D85AF19B-0B04-4C27-B2B8-62A72640D562" );

            // Rock.Achievement.Component.AccumulativeAchievement - Order
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", @"The order that this service should be used (priority)", 0, @"", "69BD0909-E1A1-40CE-9FDE-08ED4B26411B" );

            // Rock.Achievement.Component.AccumulativeAchievement - StreakType
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "F1411F4A-BD4B-4F80-9A83-94026C009F4D", "Streak Type", "StreakType", @"The source streak type from which achievements are earned.", 0, @"", Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_STREAK_TYPE );

            // Rock.Achievement.Component.AccumulativeAchievement - NumberToAccumulate
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number to Accumulate", "NumberToAccumulate", @"The number of engagements required to earn this achievement.", 1, @"", Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_NUMBER_TO_ACCUMULATE );

            // Rock.Achievement.Component.AccumulativeAchievement - TimespanInDays
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timespan in Days", "TimespanInDays", @"The sliding window of days in which the engagements must occur.", 2, @"", Rock.SystemGuid.Attribute.ACCUMULATIVE_ACHIEVEMENT_TIME_SPAN_IN_DAYS );

            // Rock.Achievement.Component.AccumulativeAchievement - StartDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDateTime", @"The date that defines when the engagements must occur on or after.", 3, @"", "BF61C4E7-D7B6-4C0C-9612-437D56C0641F" );

            // Rock.Achievement.Component.AccumulativeAchievement - EndDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.ACCUMULATIVE_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDateTime", @"The date that defines when the engagements must occur on or before.", 4, @"", "0071C77A-AC16-47D2-BCF8-F13E37ED11CD" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - Active
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", @"Should Service be used?", 0, @"False", "E1B78EB9-036E-43A9-91F8-51827408C26A" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - InteractionChannelComponent
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "299F8444-BB47-4B6C-B523-235156BF96DC", "Interaction Channel and Component", "InteractionChannelComponent", @"The source interaction channel and component from which achievements are earned.", 0, @"", "94157241-73D7-4133-85F4-19DFD3E15858" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - Order
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", @"The order that this service should be used (priority)", 0, @"", "A8FCEC67-25F6-4D74-84FE-163E597C110E" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - NumberToAccumulate
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number to Accumulate", "NumberToAccumulate", @"The number of interactions required to earn this achievement.", 1, @"", "C3090588-DB56-4E22-8B55-45915CA9A947" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - StartDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDateTime", @"The date that defines when the interactions must occur on or after.", 2, @"", "579D8692-AC40-4A19-A4AA-0B19A62CF235" );

            // Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement - EndDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDateTime", @"The date that defines when the interactions must occur on or before.", 3, @"", "56D14278-EFDE-4BE5-8F27-8D7667F3F626" );

            // Rock.Achievement.Component.StepProgramAchievement - Active
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", @"Should Service be used?", 0, @"False", "20E9AC6D-9486-45A4-B579-3B529D63435E" );

            // Rock.Achievement.Component.StepProgramAchievement - Order
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", @"The order that this service should be used (priority)", 0, @"", "DD11D0F1-11E4-4949-8F4E-AE0A7B90785C" );

            // Rock.Achievement.Component.StepProgramAchievement - StepProgram
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "StepProgram", @"The step program from which the achievement is earned.", 0, @"", "155529BA-D28A-4B41-AF4F-C282AB2ECD3D" );

            // Rock.Achievement.Component.StepProgramAchievement - StartDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDateTime", @"The date that defines when the program must be completed on or after.", 2, @"", "9A985BB9-DBFC-4A31-80DB-F259A354345F" );

            // Rock.Achievement.Component.StepProgramAchievement - EndDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STEP_PROGRAM_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDateTime", @"The date that defines when the program must be completed on or before.", 3, @"", "6BDEF165-C25C-45E7-B4C3-35B64419626B" );

            // Rock.Achievement.Component.StreakAchievement - Active
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", @"Should Service be used?", 0, @"False", "7F15E4ED-74DB-4C46-B17F-CBC6B0927970" );

            // Rock.Achievement.Component.StreakAchievement - Order
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", @"The order that this service should be used (priority)", 0, @"", "DEBA6250-9241-4B9E-ADE3-1B341E74F003" );



            // Rock.Achievement.Component.StreakAchievement - NumberToAchieve
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number to Achieve", "NumberToAchieve", @"The number of engagements in a row required to earn this achievement.", 1, @"", Rock.SystemGuid.Attribute.STREAK_ACHIEVEMENT_NUMBER_TO_ACHIEVE );

            // Rock.Achievement.Component.StreakAchievement - TimespanInDays
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timespan in Days", "TimespanInDays", @"The sliding window of days in which the engagements must occur.", 2, @"", Rock.SystemGuid.Attribute.STREAK_ACHIEVEMENT_TIME_SPAN_IN_DAYS );

            // Rock.Achievement.Component.StreakAchievement - StartDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDateTime", @"The date that defines when the engagements must occur on or after.", 3, @"", "CCED1AD6-9052-4A84-9228-242FC517C7E9" );

            // Rock.Achievement.Component.StreakAchievement - EndDateTime
            RockMigrationHelper.AddOrUpdateAchievementTypeAttribute( Rock.SystemGuid.EntityType.STREAK_ACHIEVEMENT_COMPONENT, "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDateTime", @"The date that defines when the engagements must occur on or before.", 4, @"", "3E0EF75B-FDA2-437A-B732-F62D8E6D065D" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ModelChanges_Down();
        }

        private void ModelChanges_Down()
        {
            DropForeignKey( "dbo.AchievementType", "ImageBinaryFileId", "dbo.BinaryFile" );
            DropIndex( "dbo.AchievementType", new[] { "ImageBinaryFileId" } );
            DropColumn( "dbo.AchievementType", "CustomSummaryLavaTemplate" );
            DropColumn( "dbo.AchievementType", "ImageBinaryFileId" );
            DropColumn( "dbo.AchievementType", "IsPublic" );
        }
    }
}
