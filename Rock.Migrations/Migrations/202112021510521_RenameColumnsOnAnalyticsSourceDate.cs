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
    public partial class RenameColumnsOnAnalyticsSourceDate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rename column
            RenameColumn("dbo.AnalyticsSourceDate", "CalendarMonthNameAbbrevated", "CalendarMonthNameAbbreviated");
            RenameColumn("dbo.AnalyticsSourceDate", "FiscalMonthAbbrevated", "FiscalMonthAbbreviated");

            // Add new column with old misspelled name as computed column which returns the value of the column with the correctly spelled column
            Sql("ALTER TABLE dbo.AnalyticsSourceDate ADD CalendarMonthNameAbbrevated AS CalendarMonthNameAbbreviated");
            Sql("ALTER TABLE dbo.AnalyticsSourceDate ADD FiscalMonthAbbrevated AS FiscalMonthAbbreviated");
        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Drop computed columns with misspelled names
            DropColumn("dbo.AnalyticsSourceDate", "FiscalMonthAbbrevated");
            DropColumn("dbo.AnalyticsSourceDate", "CalendarMonthNameAbbrevated");

            // Rename column to old misspelled names
            RenameColumn("dbo.AnalyticsSourceDate", "CalendarMonthNameAbbreviated", "CalendarMonthNameAbbrevated");
            RenameColumn("dbo.AnalyticsSourceDate", "FiscalMonthAbbreviated", "FiscalMonthAbbrevated");
        }
    }
}
