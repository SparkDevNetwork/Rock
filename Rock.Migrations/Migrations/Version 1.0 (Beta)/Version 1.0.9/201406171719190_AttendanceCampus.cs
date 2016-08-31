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
    public partial class AttendanceCampus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // clean up any duplicate AttributeQualifiers
            Sql( @"
delete from AttributeQualifier where Id in
(
select a.Id from
(
select max(aq.id) [Id], aq.[AttributeId], aq.[Key], count(*) [Count] from AttributeQualifier aq group by aq.[AttributeId], aq.[Key]
) [a]
where a.Count > 1
)
" );
            
            DropIndex("dbo.AttributeQualifier", new[] { "AttributeId" });
            AddColumn("dbo.Attendance", "CampusId", c => c.Int());
            CreateIndex("dbo.Attendance", "CampusId");
            CreateIndex("dbo.AttributeQualifier", new[] { "AttributeId", "Key" }, unique: true, name: "IX_AttributeIdKey");
            AddForeignKey("dbo.Attendance", "CampusId", "dbo.Campus", "Id", cascadeDelete: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Attendance", "CampusId", "dbo.Campus");
            DropIndex("dbo.AttributeQualifier", "IX_AttributeIdKey");
            DropIndex("dbo.Attendance", new[] { "CampusId" });
            DropColumn("dbo.Attendance", "CampusId");
            CreateIndex("dbo.AttributeQualifier", "AttributeId");
        }
    }
}
