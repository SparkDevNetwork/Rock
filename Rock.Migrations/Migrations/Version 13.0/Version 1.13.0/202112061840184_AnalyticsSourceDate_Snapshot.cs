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
    public partial class AnalyticsSourceDate_Snapshot : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ETD: Need to get the snapshot up-to-date since adding the [DatabaseGenerated(DatabaseGeneratedOption.Computed)] attributes to these fields.
            // Can't acutally run the AlterColumn that EF generates in the DB because they are already computed.
            //AlterColumn("dbo.AnalyticsSourceDate", "CalendarMonthNameAbbrevated", c => c.String());
            //AlterColumn("dbo.AnalyticsSourceDate", "FiscalMonthAbbrevated", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //AlterColumn("dbo.AnalyticsSourceDate", "FiscalMonthAbbrevated", c => c.String());
            //AlterColumn("dbo.AnalyticsSourceDate", "CalendarMonthNameAbbrevated", c => c.String());
        }
    }
}
