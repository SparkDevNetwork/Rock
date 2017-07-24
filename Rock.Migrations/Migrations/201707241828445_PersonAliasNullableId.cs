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
    public partial class PersonAliasNullableId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.PersonAlias", new[] { "AliasPersonId" });
            AddColumn("dbo.GroupType", "ShowMaritalStatus", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PersonAlias", "AliasPersonId", c => c.Int());
            AlterColumn("dbo.PersonAlias", "AliasPersonGuid", c => c.Guid());
            CreateIndex("dbo.PersonAlias", "AliasPersonId", unique: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.PersonAlias", new[] { "AliasPersonId" });
            AlterColumn("dbo.PersonAlias", "AliasPersonGuid", c => c.Guid(nullable: false));
            AlterColumn("dbo.PersonAlias", "AliasPersonId", c => c.Int(nullable: false));
            DropColumn("dbo.GroupType", "ShowMaritalStatus");
            CreateIndex("dbo.PersonAlias", "AliasPersonId", unique: true);
        }
    }
}
