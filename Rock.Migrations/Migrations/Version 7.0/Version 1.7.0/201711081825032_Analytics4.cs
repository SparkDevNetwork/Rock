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
    using System.Data.Entity.Spatial;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Analytics4 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.AnalyticsSourceDate", "Count", c => c.Int( nullable: false ) );
            Sql( @"UPDATE [AnalyticsSourceDate] SET [Count] = 1" );
            AddColumn( "dbo.AnalyticsSourceFamilyHistorical", "Count", c => c.Int( nullable: false ) );
            Sql( @"UPDATE [AnalyticsSourceFamilyHistorical] SET [Count] = 1" );
            AddColumn( "dbo.AnalyticsSourcePersonHistorical", "Count", c => c.Int( nullable: false ) );
            Sql( @"UPDATE [AnalyticsSourcePersonHistorical] SET [Count] = 1" );

            // Also add a [Count] field to the rest of the DIM Views
            Sql( MigrationSQL._201711081825032_Analytics4_AnalyticsDimAttendanceLocation );
            Sql( MigrationSQL._201711081825032_Analytics4_AnalyticsDimFinancialAccount );
            Sql( MigrationSQL._201711081825032_Analytics4_AnalyticsDimFinancialBatch );
            Sql( MigrationSQL._201711081825032_Analytics4_AnalyticsDimPersonBirthDate );
            Sql( MigrationSQL._201711081825032_Analytics4_AnalyticsDimPersonHistorical );

            // refresh the view definitions that include a * clause in the view definition for underlying tables that have changed
            // ORDER is important in cases where a VIEW selects from another VIEW that needs an update
            Sql( @"exec sp_refreshview [AnalyticsDimAttendanceDate]" );

            Sql( @"exec sp_refreshview [AnalyticsDimPersonHistorical]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonCurrent]" );

            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHistorical]" );
            Sql( @"exec sp_refreshview [AnalyticsDimFamilyCurrent]" );

            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHeadOfHousehold]" );
            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHeadOfHouseholdBirthDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimFinancialTransactionDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonCurrentBirthDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonHistoricalBirthDate]" );

            // Update spAnalytics_ETL_Family to populate the [Count] field
            Sql( MigrationSQL._201711081825032_Analytics4_spAnalytics_ETL_Family );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.AnalyticsSourceDate", "Count" );
            DropColumn( "dbo.AnalyticsSourcePersonHistorical", "Count" );
            DropColumn( "dbo.AnalyticsSourceFamilyHistorical", "Count" );
        }
    }
}
