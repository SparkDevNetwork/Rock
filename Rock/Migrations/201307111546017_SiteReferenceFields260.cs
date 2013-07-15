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
    public partial class SiteReferenceFields260 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.Site", "LoginPageReference", c => c.String(maxLength: 260));
            AlterColumn("dbo.Site", "RegistrationPageReference", c => c.String(maxLength: 260));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.Site", "RegistrationPageReference", c => c.String(maxLength: 10));
            AlterColumn("dbo.Site", "LoginPageReference", c => c.String(maxLength: 10));
        }
    }
}
