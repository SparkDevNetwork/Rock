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
    public partial class UpdateAuthAuditLog : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AuthAuditLog", "PersonAliasId", c => c.Int());
            CreateIndex("dbo.AuthAuditLog", "PersonAliasId");
            AddForeignKey("dbo.AuthAuditLog", "PersonAliasId", "dbo.PersonAlias", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AuthAuditLog", "PersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.AuthAuditLog", new[] { "PersonAliasId" });
            DropColumn("dbo.AuthAuditLog", "PersonAliasId");
        }
    }
}
