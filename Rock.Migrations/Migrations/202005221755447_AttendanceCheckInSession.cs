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
    public partial class AttendanceCheckInSession : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttendanceCheckInSession",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceId = c.Int(),
                        ClientIpAddress = c.String(maxLength: 45),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Device", t => t.DeviceId)
                .Index(t => t.DeviceId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Attendance", "AttendanceCheckInSessionId", c => c.Int());
            CreateIndex("dbo.Attendance", "AttendanceCheckInSessionId");

            // instead of the scaffolded AddForeignKey("dbo.Attendance", "AttendanceCheckInSessionId", "dbo.AttendanceCheckInSession", "Id", cascadeDelete: true);
            // we want a ON DELETE NULL (cascade null)
            
            Sql( @"ALTER TABLE [dbo].[Attendance] ADD CONSTRAINT [FK_dbo.Attendance_dbo.AttendanceCheckInSession_AttendanceCheckInSessionId] 
FOREIGN KEY ([AttendanceCheckInSessionId]) REFERENCES [dbo].[AttendanceCheckInSession] ([Id])
ON DELETE SET NULL" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AttendanceCheckInSession", "DeviceId", "dbo.Device");
            DropForeignKey("dbo.Attendance", "AttendanceCheckInSessionId", "dbo.AttendanceCheckInSession");
            DropIndex("dbo.AttendanceCheckInSession", new[] { "Guid" });
            DropIndex("dbo.AttendanceCheckInSession", new[] { "DeviceId" });
            DropIndex("dbo.Attendance", new[] { "AttendanceCheckInSessionId" });
            DropColumn("dbo.Attendance", "AttendanceCheckInSessionId");
            DropTable("dbo.AttendanceCheckInSession");
        }
    }
}
