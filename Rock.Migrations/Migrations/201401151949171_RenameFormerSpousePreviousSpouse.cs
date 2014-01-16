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
    public partial class RenameFormerSpousePreviousSpouse : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"UPDATE [GroupTypeRole]
                     SET [Name] = 'Previous Spouse'
                     WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"UPDATE [GroupTypeRole]
                     SET [Name] = 'Former Spouse'
                     WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
    }
}
