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
    public partial class ScheduleServiceTimes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( string.Format( @"
    IF NOT EXISTS (SELECT [Id] FROM [Category] WHERE [Guid] = '{0}')
        INSERT INTO [Category] ([IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid])
            VALUES (1, {1}, '', '', 'Service Times', '{0}')
", Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES, Rock.Web.Cache.EntityTypeCache.Read( "Rock.Model.Schedule" ).Id ) );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No need to delete system category on down-grade since up-grade checks for existence
        }
    }
}
