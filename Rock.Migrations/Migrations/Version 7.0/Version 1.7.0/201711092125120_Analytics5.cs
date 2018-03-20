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
    public partial class Analytics5 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AnalyticsSourceCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CampusId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        ShortCode = c.String(maxLength: 50),
                        Url = c.String(),
                        LocationId = c.Int(),
                        PhoneNumber = c.String(),
                        LeaderPersonAliasId = c.Int(),
                        ServiceTimes = c.String(maxLength: 500),
                        Order = c.Int(nullable: false),
                        Count = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.Guid, unique: true);

            Sql( MigrationSQL._201711092125120_Analytics5_AnalyticsDimAttendanceLocation );
            Sql( MigrationSQL._201711092125120_Analytics5_AnalyticsDimCampus );
            Sql( MigrationSQL._201711092125120_Analytics5_AnalyticsDimFamilyHistorical );

            Sql( @"exec sp_refreshview [AnalyticsDimFamilyCurrent]" );

            Sql( MigrationSQL._201711092125120_Analytics5_AnalyticsDimPersonHistorical );
            Sql( MigrationSQL._201711092125120_Analytics5_spAnalytics_ETL_Campus );

            // refresh the view definitions that include a * clause in the view definition for underlying tables that have changed
            // ORDER is important in cases where a VIEW selects from another VIEW that needs an update
            Sql( @"exec sp_refreshview [AnalyticsDimAttendanceDate]" );

            Sql( @"exec sp_refreshview [AnalyticsDimPersonHistorical]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonCurrent]" );

            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHistorical]" );

            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHeadOfHousehold]" );
            Sql( @"exec sp_refreshview [AnalyticsDimFamilyHeadOfHouseholdBirthDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimFinancialTransactionDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonCurrentBirthDate]" );
            Sql( @"exec sp_refreshview [AnalyticsDimPersonHistoricalBirthDate]" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.AnalyticsSourceCampus", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceCampus", new[] { "Name" });
            DropTable( "dbo.AnalyticsSourceCampus" );
        }
    }
}
