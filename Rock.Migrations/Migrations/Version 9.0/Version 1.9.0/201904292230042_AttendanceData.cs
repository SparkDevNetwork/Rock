// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AttendanceData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.AttendanceLabelData", "Id", "dbo.Attendance");
            DropIndex("dbo.AttendanceLabelData", new[] { "Id" });
            CreateTable(
                "dbo.AttendanceData",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LabelData = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attendance", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
            DropTable("dbo.AttendanceLabelData");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.AttendanceLabelData",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.AttendanceData", "Id", "dbo.Attendance");
            DropIndex("dbo.AttendanceData", new[] { "Id" });
            DropTable("dbo.AttendanceData");
            CreateIndex("dbo.AttendanceLabelData", "Id");
            AddForeignKey("dbo.AttendanceLabelData", "Id", "dbo.Attendance", "Id", cascadeDelete: true);
        }
    }
}
