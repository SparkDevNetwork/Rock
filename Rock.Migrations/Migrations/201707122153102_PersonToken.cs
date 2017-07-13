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
    public partial class PersonToken : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonToken",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        Token = c.String(maxLength: 32),
                        ExpireDateTime = c.DateTime(),
                        TimesUsed = c.Int(nullable: false),
                        UsageLimit = c.Int(),
                        LastUsedDateTime = c.DateTime(),
                        PageId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => t.PersonAliasId)
                .Index(t => t.Token, unique: true)
                .Index(t => t.PageId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonToken", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonToken", "PageId", "dbo.Page");
            DropIndex("dbo.PersonToken", new[] { "Guid" });
            DropIndex("dbo.PersonToken", new[] { "PageId" });
            DropIndex("dbo.PersonToken", new[] { "Token" });
            DropIndex("dbo.PersonToken", new[] { "PersonAliasId" });
            DropTable("dbo.PersonToken");
        }
    }
}
