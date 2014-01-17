//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PrayerRequestDateChanges : Rock.Migrations.RockMigration
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
