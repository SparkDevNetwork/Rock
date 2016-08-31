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
    public partial class PersonDuplicate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonDuplicate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        DuplicatePersonAliasId = c.Int(nullable: false),
                        IsConfirmedAsNotDuplicate = c.Boolean(nullable: false),
                        Score = c.Int(),
                        ScoreDetail = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.DuplicatePersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => new { t.PersonAliasId, t.DuplicatePersonAliasId }, unique: true)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateIndex("dbo.Person", "Email");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonDuplicate", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonDuplicate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonDuplicate", "DuplicatePersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonDuplicate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonDuplicate", new[] { "Guid" });
            DropIndex("dbo.PersonDuplicate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonDuplicate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonDuplicate", new[] { "PersonAliasId", "DuplicatePersonAliasId" });
            DropIndex("dbo.Person", new[] { "Email" });
            DropTable("dbo.PersonDuplicate");
        }
    }
}
