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
    public partial class UpdateAnalyticsSourceZipCodeAndCampusSchema : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable( name: "dbo.AnalyticsSourceZipCode", newName: "AnalyticsSourcePostalCode" );
            RenameColumn( table: "dbo.AnalyticsSourcePostalCode", name: "ZipCode", newName: "PostalCode" );
            AddColumn( "dbo.AnalyticsSourcePostalCode", "CountryCode", c => c.String( maxLength: 3 ) );
            AddColumn( "dbo.Campus", "OpenedDate", c => c.DateTime( storeType: "date" ) );
            AddColumn( "dbo.Campus", "ClosedDate", c => c.DateTime( storeType: "date" ) );
            AddColumn( "dbo.Campus", "TitheMetric", c => c.Decimal( precision: 8, scale: 2 ) );

            Sql( $@"
	UPDATE ServiceJob
	SET [Class] = 'Rock.Jobs.UpdateAnalyticsSourcePostalCode',
	[Description] = 'Job to update the AnalyticsSourcePostalCode table with geographical and census data.',
	[Name] = 'Update Analytics Source Postal Code',
    [IsSystem] = 1
	WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_POSTAL_CODE}'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.AnalyticsSourcePostalCode", "CountryCode" );
            DropColumn( "dbo.Campus", "OpenedDate" );
            DropColumn( "dbo.Campus", "ClosedDate" );
            DropColumn( "dbo.Campus", "TitheMetric" );
            RenameColumn( table: "dbo.AnalyticsSourcePostalCode", name: "PostalCode", newName: "ZipCode" );
            RenameTable( name: "dbo.AnalyticsSourcePostalCode", newName: "AnalyticsSourceZipCode" );

            Sql( $@"
	UPDATE ServiceJob
	SET [Class] = 'Rock.Jobs.UpdateAnalyticsSourceZipCode',
	[Description] = 'Job to update the AnalyticsSourceZipCode table with geographical and census data.',
	[Name] = 'Update Analytics Source Zip Code'
	WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_ANALYTICS_SOURCE_POSTAL_CODE}'
" );
        }
    }
}
