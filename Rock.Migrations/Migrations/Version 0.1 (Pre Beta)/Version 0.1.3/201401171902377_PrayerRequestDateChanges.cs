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
    public partial class PrayerRequestDateChanges : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.PrayerRequest", "EnteredDate", "EnteredDateTime" );
            RenameColumn( "dbo.PrayerRequest", "ApprovedOnDate", "ApprovedOnDateTime" );

            AlterColumn( "dbo.PrayerRequest", "EnteredDateTime", c => c.DateTime( nullable: false ) );
            AlterColumn( "dbo.PrayerRequest", "ApprovedOnDateTime", c => c.DateTime() );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.PrayerRequest", "ApprovedOnDateTime", c => c.DateTime( storeType: "date" ) );
            AlterColumn( "dbo.PrayerRequest", "EnteredDateTime", c => c.DateTime( nullable: false, storeType: "date" ) );

            RenameColumn( "dbo.PrayerRequest", "EnteredDateTime", "EnteredDate" );
            RenameColumn( "dbo.PrayerRequest", "ApprovedOnDateTime", "ApprovedOnDate" );
        }
    }
}
