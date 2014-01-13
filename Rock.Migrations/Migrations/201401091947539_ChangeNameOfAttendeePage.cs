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
    public partial class ChangeNameOfAttendeePage : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"UPDATE [Page]
                    SET [Name] = 'Manage', [Title] = 'Manage'
                     WHERE [Guid] = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"UPDATE [Page]
                    SET [Name] = 'Attendees', [Title] = 'Attendees'
                     WHERE [Guid] = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701'");
        }
    }
}
