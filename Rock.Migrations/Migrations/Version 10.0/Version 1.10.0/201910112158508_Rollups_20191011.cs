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
    public partial class Rollups_20191011 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "TeamGroupId", c => c.Int());
            AddColumn("dbo.AttendanceOccurrence", "Name", c => c.String(maxLength: 250));
            AddColumn("dbo.Site", "EnableExclusiveRoutes", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Campus", "TeamGroupId");
            AddForeignKey("dbo.Campus", "TeamGroupId", "dbo.Group", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Campus", "TeamGroupId", "dbo.Group");
            DropIndex("dbo.Campus", new[] { "TeamGroupId" });
            DropColumn("dbo.Site", "EnableExclusiveRoutes");
            DropColumn("dbo.AttendanceOccurrence", "Name");
            DropColumn("dbo.Campus", "TeamGroupId");
        }
    }
}
