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
    public partial class SundayOfTheYear : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AnalyticsSourceDate", "WeekOfYear", c => c.Int(nullable: false));
            AddColumn("dbo.AnalyticsSourceDate", "WeekCounter", c => c.Int(nullable: false));
            AddColumn("dbo.AnalyticsSourceDate", "LeapYearIndicator", c => c.Boolean(nullable: false));
            AddColumn("dbo.AnalyticsSourceDate", "SundayDateYear", c => c.Int(nullable: false));
            CreateIndex("dbo.AnalyticsSourceDate", "WeekOfYear");
            CreateIndex("dbo.AnalyticsSourceDate", "SundayDateYear");

            // Run the SQL to update all of the values for these new columns in the analytics table
            Sql( Rock.Model.AnalyticsSourceDate.SundayOfTheYearSql );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.AnalyticsSourceDate", new[] { "WeekOfYear" });
            DropIndex("dbo.AnalyticsSourceDate", new[] { "SundayDateYear" });
            DropColumn("dbo.AnalyticsSourceDate", "SundayDateYear");
            DropColumn("dbo.AnalyticsSourceDate", "LeapYearIndicator");
            DropColumn("dbo.AnalyticsSourceDate", "WeekCounter");
            DropColumn("dbo.AnalyticsSourceDate", "WeekOfYear");
        }
    }
}
