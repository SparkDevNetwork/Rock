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
    public partial class GroupMemberDetailBreadCrumb : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"update [Page] set [BreadCrumbDisplayName] = 0 where Guid in ('3905C63F-4D57-40F0-9721-C60A2F681911','45899E6A-7CEC-44EC-8DBA-BD8850262C04')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"update [Page] set [BreadCrumbDisplayName] = 1 where Guid in ('3905C63F-4D57-40F0-9721-C60A2F681911','45899E6A-7CEC-44EC-8DBA-BD8850262C04')" );
        }
    }
}
