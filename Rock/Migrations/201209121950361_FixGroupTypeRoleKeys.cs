namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class FixGroupTypeRoleKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "groupsGroupRole" );
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "groupsGroupType" );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole", "Id", cascadeDelete: true );
        }
        
        public override void Down()
        {
            Sql( @"DELETE groupsGroupTypeRole" );

            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType" );
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole" );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "groupsGroupRole", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "groupsGroupType", "Id", cascadeDelete: true );
        }
    }
}
