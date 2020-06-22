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
    public partial class AddPropertyToAttendance : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attendance", "PresentDateTime", c => c.DateTime());
            AddColumn("dbo.Attendance", "PresentByPersonAliasId", c => c.Int());
            AddColumn("dbo.Attendance", "CheckedOutByPersonAliasId", c => c.Int());
            CreateIndex("dbo.Attendance", "PresentByPersonAliasId");
            CreateIndex("dbo.Attendance", "CheckedOutByPersonAliasId");
            AddForeignKey("dbo.Attendance", "CheckedOutByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.Attendance", "PresentByPersonAliasId", "dbo.PersonAlias", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Attendance", "PresentByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Attendance", "CheckedOutByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Attendance", new[] { "CheckedOutByPersonAliasId" });
            DropIndex("dbo.Attendance", new[] { "PresentByPersonAliasId" });
            DropColumn("dbo.Attendance", "CheckedOutByPersonAliasId");
            DropColumn("dbo.Attendance", "PresentByPersonAliasId");
            DropColumn("dbo.Attendance", "PresentDateTime");
        }
    }
}
