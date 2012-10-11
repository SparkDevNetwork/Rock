namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class FixGroupTypeRoleKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey( "groupsGroupTypeRole", "GroupTypeId", "groupsGroupRole" );
            DropForeignKey( "groupsGroupTypeRole", "GroupRoleId", "groupsGroupType" );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole", "Id", cascadeDelete: true );
        }
        
        public override void Down()
        {
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType" );
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole" );
            AddForeignKey( "groupsGroupTypeRole", "GroupTypeId", "groupsGroupRole", "Id", cascadeDelete: true );
            AddForeignKey( "groupsGroupTypeRole", "GroupRoleId", "groupsGroupType", "Id", cascadeDelete: true );
        }
    }
}
