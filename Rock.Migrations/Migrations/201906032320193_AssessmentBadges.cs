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
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
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
    }
}
