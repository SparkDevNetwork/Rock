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
    public partial class HideSystemGroupTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [GroupType] SET [ShowInNavigation] = 0 WHERE [Guid] IN ('E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF', '8C0E5852-F08F-4327-9AA5-87800A6AB53E')
    UPDATE [GroupType] SET [ShowInGroupList] = 1 WHERE [Guid] NOT IN ('790E3215-3B10-442B-AF69-616C0DCB998E', 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF', '8C0E5852-F08F-4327-9AA5-87800A6AB53E')
" );
        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Nothing (Types should have initially been created with these settings)
        }
    }
}
