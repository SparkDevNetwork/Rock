namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupRoles : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole");
            DropForeignKey("dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType");
            DropIndex("groupsGroupTypeRole", new[] { "GroupRoleId" });
            DropIndex("groupsGroupTypeRole", new[] { "GroupTypeId" });

            CreateTable(
                "dbo.groupGroupLocation",
                c => new
                    {
                        GroupId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        LocationTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupId, t.LocationId })
                .ForeignKey("dbo.groupsGroup", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.crmLocation", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.LocationId);
            
            AddColumn("dbo.groupsGroup", "CampusId", c => c.Int());
            AddColumn("dbo.groupsGroupRole", "GroupTypeId", c => c.Int(nullable: true));

			Sql( @"

				INSERT INTO [groupsGroupTypeRole] ([GroupRoleId], [GroupTypeId])
					SELECT 
						 R.[Id]
						,CASE R.[Guid]
							WHEN '00F3AC1C-71B9-4EE5-A30E-4C48C8A0BF1F' THEN
								(SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = 'AECE949F-704C-483E-A4FB-93D5E4720C4C')
							WHEN 'FAF28845-3F76-404E-9613-507C9FF8E135' THEN
								(SELECT [Id] FROM [groupsGroupType] WHERE [Guid] = 'C663546D-AC65-48BF-BE3A-8BFC3F69CEB3')
						END
					FROM [groupsGroupRole] R
					LEFT OUTER JOIN [groupsGroupTypeRole] TR
						ON TR.[GroupRoleId] = R.[Id]
					WHERE R.[Guid] IN ('00F3AC1C-71B9-4EE5-A30E-4C48C8A0BF1F', 'FAF28845-3F76-404E-9613-507C9FF8E135')
					AND TR.[GroupRoleId] IS NULL

				;WITH CTE
				AS
				(
					SELECT 
						 MIN([GroupTypeId]) AS [GroupTypeId]
						,[GroupRoleId]
					FROM [groupsGroupTypeRole]
					GROUP BY [GroupRoleId]
				)
				UPDATE R
					SET [GroupTypeId] = CTE.[GroupTypeId]
				FROM CTE
				INNER JOIN [groupsGroupRole] R
					ON R.[Id] = CTE.[GroupRoleId]
" );
			
			AddForeignKey( "dbo.groupsGroup", "CampusId", "dbo.crmCampus", "Id" );
            AddForeignKey("dbo.groupsGroupRole", "GroupTypeId", "dbo.groupsGroupType", "Id", cascadeDelete: true);
            CreateIndex("dbo.groupsGroup", "CampusId");
            CreateIndex("dbo.groupsGroupRole", "GroupTypeId");

			DropTable("dbo.groupsGroupTypeRole");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.groupsGroupTypeRole",
                c => new
                    {
                        GroupRoleId = c.Int(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupRoleId, t.GroupTypeId });

			Sql( @"
				INSERT INTO [groupsGroupTypeRole] ([GroupRoleId], [GroupTypeId])
					SELECT [Id], [GroupTypeId] FROM [groupsGroupRole]
" );
			
			DropIndex( "dbo.groupsGroupRole", new[] { "GroupTypeId" } );
            DropIndex("dbo.groupGroupLocation", new[] { "LocationId" });
            DropIndex("dbo.groupGroupLocation", new[] { "GroupId" });
            DropIndex("dbo.groupsGroup", new[] { "CampusId" });
            DropForeignKey("dbo.groupsGroupRole", "GroupTypeId", "dbo.groupsGroupType");
            DropForeignKey("dbo.groupGroupLocation", "LocationId", "dbo.crmLocation");
            DropForeignKey("dbo.groupGroupLocation", "GroupId", "dbo.groupsGroup");
            DropForeignKey("dbo.groupsGroup", "CampusId", "dbo.crmCampus");
            DropColumn("dbo.groupsGroupRole", "GroupTypeId");
            DropColumn("dbo.groupsGroup", "CampusId");
            DropTable("dbo.groupGroupLocation");
            CreateIndex("groupsGroupTypeRole", "GroupTypeId");
            CreateIndex("groupsGroupTypeRole", "GroupRoleId");
            AddForeignKey("dbo.groupsGroupTypeRole", "GroupTypeId", "dbo.groupsGroupType", "Id", cascadeDelete: true);
            AddForeignKey("dbo.groupsGroupTypeRole", "GroupRoleId", "dbo.groupsGroupRole", "Id", cascadeDelete: true);
        }
    }
}
