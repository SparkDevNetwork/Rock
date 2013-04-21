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
    public partial class AttendanceFields : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attendance", "DeviceId", c => c.Int());
            AddColumn("dbo.Attendance", "SearchTypeValueId", c => c.Int());
            AddForeignKey("dbo.Attendance", "DeviceId", "dbo.Device", "Id");
            AddForeignKey("dbo.Attendance", "SearchTypeValueId", "dbo.DefinedValue", "Id");
            CreateIndex("dbo.Attendance", "DeviceId");
            CreateIndex("dbo.Attendance", "SearchTypeValueId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.Attendance", new[] { "SearchTypeValueId" });
            DropIndex("dbo.Attendance", new[] { "DeviceId" });
            DropForeignKey("dbo.Attendance", "SearchTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Attendance", "DeviceId", "dbo.Device");
            DropColumn("dbo.Attendance", "SearchTypeValueId");
            DropColumn("dbo.Attendance", "DeviceId");
        }
    }
}
