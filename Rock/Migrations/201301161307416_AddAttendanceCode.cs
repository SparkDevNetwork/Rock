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
    public partial class AddAttendanceCode : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttendanceCode",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IssueDateTime = c.DateTime(nullable: false),
                        Code = c.String(maxLength: 10),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.AttendanceCode", "Code", true );
            CreateIndex( "dbo.AttendanceCode", "Guid", true );
            AddColumn("dbo.Attendance", "AttendanceCodeId", c => c.Int());
            AddForeignKey("dbo.Attendance", "AttendanceCodeId", "dbo.AttendanceCode", "Id");
            CreateIndex("dbo.Attendance", "AttendanceCodeId");
            DropColumn("dbo.Attendance", "SecurityCode");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Attendance", "SecurityCode", c => c.String(maxLength: 10));
            DropIndex("dbo.Attendance", new[] { "AttendanceCodeId" });
            DropForeignKey("dbo.Attendance", "AttendanceCodeId", "dbo.AttendanceCode");
            DropColumn("dbo.Attendance", "AttendanceCodeId");
            DropTable("dbo.AttendanceCode");
        }
    }
}
