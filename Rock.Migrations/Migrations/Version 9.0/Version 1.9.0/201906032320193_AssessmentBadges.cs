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

namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AssessmentBadges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.AssessmentType", "IconCssClass", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.AssessmentType", "BadgeColor", c => c.String( maxLength: 7 ) );
            AddColumn( "dbo.AssessmentType", "BadgeSummaryLava", c => c.String() );

            UpdateAssessmentTypeFields();
            AddAssessmentBadgeUp();
            ReplaceDiscBadgeWithAssessmentBadgeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            ReplaceDiscBadgeWithAssessmentBadgeDown();
            AddAssessmentBadgeDown();

            DropColumn( "dbo.AssessmentType", "BadgeSummaryLava" );
            DropColumn( "dbo.AssessmentType", "BadgeColor" );
            DropColumn( "dbo.AssessmentType", "IconCssClass" );

        }

        /// <summary>
        /// Updates the assessment type fields used for badges.
        /// </summary>
        private void UpdateAssessmentTypeFields()
        {
            Sql( $"UPDATE [AssessmentType] SET IconCssClass = 'fa fa-chart-bar', BadgeColor = '#4E79A7' WHERE [Guid] = '{SystemGuid.AssessmentType.DISC}'" );
            Sql( $"UPDATE [AssessmentType] SET IconCssClass = 'fa fa-gift', BadgeColor = '#8CD17D' WHERE [Guid] = '{SystemGuid.AssessmentType.GIFTS}' " );
            Sql( $"UPDATE [AssessmentType] SET IconCssClass = 'fa fa-handshake', BadgeColor = '#E15759' WHERE [Guid] = '{SystemGuid.AssessmentType.CONFLICT}'" );
            Sql( $"UPDATE [AssessmentType] SET IconCssClass = 'fa fa-theater-masks', BadgeColor = '#499894' WHERE [Guid] = '{SystemGuid.AssessmentType.EQ}'" );
            Sql( $"UPDATE [AssessmentType] SET IconCssClass = 'fa fa-key ', BadgeColor = '#F28E2B' WHERE [Guid] = '{SystemGuid.AssessmentType.MOTIVATORS}'" );
        }

        private void AddAssessmentBadgeUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.Assessment", "C10B68B3-A13C-4B1A-9C56-91F0630AED90", false, true );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.UpdatePersonBadge( "Assessments", "Shows the person's Personality Assessments.", "Rock.PersonProfile.Badge.Assessment", 0, "CCE09793-89F6-4042-A98A-ED38392BCFCC" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void AddAssessmentBadgeDown()
        {
            Sql( @"DELETE FROM [PersonBadge] WHERE [Guid] = 'CCE09793-89F6-4042-A98A-ED38392BCFCC'" );
            RockMigrationHelper.DeleteEntityType( "C10B68B3-A13C-4B1A-9C56-91F0630AED90" );
        }


        private void ReplaceDiscBadgeWithAssessmentBadgeUp()
        {
            Sql( @"
                -- REMOVE DISC
                UPDATE [AttributeValue]
                SET [Value] = REPLACE([Value], '6C491A10-E942-4CA5-8D13-ACBC28511714', '') 
                WHERE [Guid] = 'BC1B2142-5B24-4918-833D-E2F0BE833DFD'

                -- ADD Assessments
                UPDATE [AttributeValue]
                SET [Value] = [Value] + ',CCE09793-89F6-4042-A98A-ED38392BCFCC'
                WHERE [Guid] = 'BC1B2142-5B24-4918-833D-E2F0BE833DFD'" );
        }

        private void ReplaceDiscBadgeWithAssessmentBadgeDown()
        {
            Sql( @"
                -- REMOVE Assessments
                UPDATE [AttributeValue]
                SET [Value] = REPLACE([Value], 'CCE09793-89F6-4042-A98A-ED38392BCFCC', '') 
                WHERE [Guid] = 'BC1B2142-5B24-4918-833D-E2F0BE833DFD'

                -- ADD DISC
                UPDATE [AttributeValue]
                SET [Value] = [Value] + ',6C491A10-E942-4CA5-8D13-ACBC28511714'
                WHERE [Guid] = 'BC1B2142-5B24-4918-833D-E2F0BE833DFD'" );
        }
    }
}
