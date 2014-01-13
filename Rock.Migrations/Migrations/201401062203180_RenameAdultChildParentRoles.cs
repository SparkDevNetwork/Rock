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
    public partial class RenameAdultChildParentRoles : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Child'
                WHERE 
                    [Guid] = 'F87DF00F-E86D-4771-A3AE-DBF79B78CF5D'");

            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Parent'
                WHERE 
                    [Guid] = '6F3FADC4-6320-4B54-9CF6-02EF9586A660'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Adult Child'
                WHERE 
                    [Guid] = 'F87DF00F-E86D-4771-A3AE-DBF79B78CF5D'");

            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Adult Parent Of'
                WHERE 
                    [Guid] = '6F3FADC4-6320-4B54-9CF6-02EF9586A660'");
        }
    }
}
