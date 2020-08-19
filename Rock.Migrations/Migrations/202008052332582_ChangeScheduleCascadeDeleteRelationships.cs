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
    public partial class ChangeScheduleCascadeDeleteRelationships : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.GroupLocationScheduleConfig", "GroupLocationId", "dbo.GroupLocation");
            DropForeignKey("dbo.GroupLocationScheduleConfig", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.AttendanceOccurrence", "ScheduleId", "dbo.Schedule");
            AddForeignKey("dbo.GroupLocationScheduleConfig", "GroupLocationId", "dbo.GroupLocation", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GroupLocationScheduleConfig", "ScheduleId", "dbo.Schedule", "Id", cascadeDelete: true);

            // Instead of the scaffolded AddForeignKey("dbo.AttendanceOccurrence", "ScheduleId", "dbo.Schedule", "Id", cascadeDelete: true);
            // we want a ON DELETE NULL (cascade null).
            Sql( @"ALTER TABLE [dbo].[AttendanceOccurrence]
ADD CONSTRAINT [FK_dbo.AttendanceOccurrence_dbo.Schedule_ScheduleId] FOREIGN KEY ([ScheduleId])
REFERENCES [dbo].[Schedule] ([Id])
ON DELETE SET NULL;" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AttendanceOccurrence", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.GroupLocationScheduleConfig", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.GroupLocationScheduleConfig", "GroupLocationId", "dbo.GroupLocation");
            AddForeignKey("dbo.AttendanceOccurrence", "ScheduleId", "dbo.Schedule", "Id", cascadeDelete: true);
            AddForeignKey("dbo.GroupLocationScheduleConfig", "ScheduleId", "dbo.Schedule", "Id");
            AddForeignKey("dbo.GroupLocationScheduleConfig", "GroupLocationId", "dbo.GroupLocation", "Id");
        }
    }
}
