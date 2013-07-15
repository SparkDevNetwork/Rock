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
    public partial class AddAttendedCheckInSite : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                DECLARE @PageId int
                SELECT @PageId = [ID] from [Page] where [Guid] = '32A132A6-63A2-4840-B4A5-23D80994CCBD'
                DECLARE @SiteId int
                IF @PageId IS NOT NULL BEGIN
                    INSERT INTO [Site] (IsSystem, Name, Description, Theme, DefaultPageId, Guid)
                    VALUES (1, 'Attended Checkin', 'The Rock default attended checkin site.', 'CheckinPark', @PageId, '30FB46F7-4814-4691-852A-04FB56CC07F0' )
                    SET @SiteId = SCOPE_IDENTITY()

                    UPDATE [Page]
                    SET [SiteId] = @SiteId where [ParentPageId] = @PageId    
                END 
            ");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                DECLARE @SiteId int
                SELECT @SiteId = [Id] from [Site] where [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4'
                DECLARE @PageId int
                SELECT @PageId = [ID] from [Page] where [Guid] = '32A132A6-63A2-4840-B4A5-23D80994CCBD'
                IF @PageId IS NOT NULL BEGIN
                    UPDATE [Page] SET [SiteId] = @SiteId where [ParentPageId] = @PageId
                END

                DELETE [Site] where [Guid] = '30FB46F7-4814-4691-852A-04FB56CC07F0'
            " );
        }
    }
}
