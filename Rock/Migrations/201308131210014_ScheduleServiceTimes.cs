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
            UpdateEntityType("Rock.Model.Schedule", "0B2C38A7-D79C-4F85-9757-F1B045D32C8A");
            Sql( string.Format( @"

    DECLARE @EntityTypeId int
    SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A')

    IF NOT EXISTS (SELECT [Id] FROM [Category] WHERE [Guid] = '{0}')
        INSERT INTO [Category] ([IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid])
            VALUES (1, @EntityTypeId, '', '', 'Service Times', '{0}')
", Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES ) );
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
