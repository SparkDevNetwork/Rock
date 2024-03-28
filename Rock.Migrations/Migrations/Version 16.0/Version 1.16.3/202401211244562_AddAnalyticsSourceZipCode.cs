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
    public partial class AddAnalyticsSourceZipCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AnalyticsSourceZipCode",
                c => new
                {
                    ZipCode = c.String( nullable: false, maxLength: 50 ),
                    GeoFence = c.Geography(),
                    State = c.String( maxLength: 2 ),
                    City = c.String( maxLength: 50 ),
                    SquareMiles = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    HouseholdsTotal = c.Int(),
                    FamiliesTotal = c.Int(),
                    FamiliesUnder10kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families10kTo14kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families15kTo24kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families25kTo34kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families35kTo49kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families50kTo74kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families75kTo99kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families100kTo149kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Families150kTo199kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    FamiliesMore200kPercent = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    FamiliesMedianIncome = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    FamiliesMedianIncomeMarginOfError = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    FamiliesMeanIncome = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    FamiliesMeanIncomeMarginOfError = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    MarriedCouplesTotal = c.Int(),
                    NonFamilyHouseholdsTotal = c.Int(),
                    LastUpdate = c.Int(),
                } )
                .PrimaryKey( t => t.ZipCode );

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.UpdateAnalyticsSourceZipCode'
                                AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_ZIPCODE}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[HistoryCount]
                    ,[Guid]
                ) VALUES (
                    0
                    ,1
                    ,'Update Analytics Source ZipCode'
                    ,'Job to update the AnalyticsSourceZipCode table with geographical and census data.'
                    ,'Rock.Jobs.UpdateAnalyticsSourceZipCode'
                    ,'0 20 1 1/1 * ? *'
                    ,1
                    ,500 
                    ,'{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_ZIPCODE}'
                );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropTable( "dbo.AnalyticsSourceZipCode" );
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_ZIPCODE}';" );
        }
    }
}
