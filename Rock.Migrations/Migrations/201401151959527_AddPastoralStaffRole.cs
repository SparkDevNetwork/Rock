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
    public partial class AddPastoralStaffRole : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"INSERT INTO [Group]
                       ([IsSystem]
                       ,[ParentGroupId]
                       ,[GroupTypeId]
                       ,[CampusId]
                       ,[Name]
                       ,[Description]
                       ,[IsSecurityRole]
                       ,[IsActive]
                       ,[Order]
                       ,[Guid])
                 VALUES
                       (0
                       ,null
                       ,1
                       ,null
                       ,'Pastoral Staff'
                       ,'Group of individuals how can access information limited to just pastors on staff.'
                       ,1
                       ,1
                       ,0
                       ,'26E7148C-2059-4F45-BCFE-32230A12F0DC')");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"DELETE FROM [Group] 
                    WHERE [Guid] = '26E7148C-2059-4F45-BCFE-32230A12F0DC'");
        }
    }
}
