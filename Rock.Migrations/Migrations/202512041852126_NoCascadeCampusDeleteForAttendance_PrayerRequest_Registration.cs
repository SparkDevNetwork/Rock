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
    public partial class NoCascadeCampusDeleteForAttendance_PrayerRequest_Registration : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.Registration", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.Attendance", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.PrayerRequest", "CampusId", "dbo.Campus" );

            // Instead of the scaffolded AddForeignKey("dbo.Registration", "CampusId", "dbo.Campus", "Id");
            // we want a ON DELETE NULL (cascade null).
            Sql( @"
ALTER TABLE [dbo].[Registration]
ADD CONSTRAINT [FK_dbo.Registration_dbo.Campus_CampusId] FOREIGN KEY ([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL;" );

            // Instead of scaffolded AddForeignKey("dbo.Attendance", "CampusId", "dbo.Campus", "Id");
            Sql( @"
ALTER TABLE [dbo].[Attendance]
ADD CONSTRAINT [FK_dbo.Attendance_dbo.Campus_CampusId] FOREIGN KEY ([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL;" );

            // Instead of scaffolded AddForeignKey("dbo.PrayerRequest", "CampusId", "dbo.Campus", "Id");
            Sql( @"
ALTER TABLE [dbo].[PrayerRequest]
ADD CONSTRAINT [FK_dbo.PrayerRequest_dbo.Campus_CampusId] FOREIGN KEY ([CampusId])
REFERENCES [dbo].[Campus] ([Id])
ON DELETE SET NULL;" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.PrayerRequest", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.Attendance", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.Registration", "CampusId", "dbo.Campus" );
            AddForeignKey( "dbo.PrayerRequest", "CampusId", "dbo.Campus", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Attendance", "CampusId", "dbo.Campus", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Registration", "CampusId", "dbo.Campus", "Id", cascadeDelete: true );
        }
    }
}
