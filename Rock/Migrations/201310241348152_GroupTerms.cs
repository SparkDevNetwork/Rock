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
    public partial class GroupTerms : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [GroupType] SET [GroupTerm] = 'Group' WHERE [GroupTerm] IS NULL OR [GroupTerm] = ''
    UPDATE [GroupType] SET [GroupMemberTerm] = 'Member' WHERE [GroupMemberTerm] IS NULL OR [GroupMemberTerm] = ''
" );
            AlterColumn("dbo.GroupType", "GroupTerm", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.GroupType", "GroupMemberTerm", c => c.String(nullable: false, maxLength: 100));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.GroupType", "GroupMemberTerm", c => c.String(maxLength: 100));
            AlterColumn("dbo.GroupType", "GroupTerm", c => c.String(maxLength: 100));
        }
    }
}
