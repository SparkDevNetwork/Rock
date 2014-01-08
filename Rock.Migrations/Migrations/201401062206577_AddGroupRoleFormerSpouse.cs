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
    public partial class AddGroupRoleFormerSpouse : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
                INSERT INTO [GroupTypeRole]
                   ([IsSystem]
                   ,[GroupTypeId]
                   ,[Name]
                   ,[Description]
                   ,[Order]
                   ,[IsLeader]
                   ,[Guid])
                VALUES
                   (0
                   ,11
                   ,'Former Spouse'
                   ,'Role to identify former spouses after divorce.'
                   ,12
                   ,0
                   ,'60C6142E-8E00-4678-BC2F-983BB7BDE80B')");

            Sql(@"INSERT INTO [AttributeValue] 
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                    SELECT 0
                        ,540
                        ,[Id]
                        ,0
                        ,'60c6142e-8e00-4678-bc2f-983bb7bde80b'
                        ,'150069D7-079B-46B5-BE74-98167BA42CC4'
	                FROM [GROUPTypeRole]
	                WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"DELETE FROM [AttributeValue] WHERE [Guid] = '150069D7-079B-46B5-BE74-98167BA42CC4'");
            Sql(@"DELETE FROM [GroupTypeRole] WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
    }
}
