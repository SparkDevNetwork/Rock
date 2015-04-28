// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class PersonPreviousName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonPreviousName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        LastName = c.String(maxLength: 50),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.LastName, name: "IDX_LastName")
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.GroupMember", "DateTimeAdded", c => c.DateTime());

            Sql( @"UPDATE [GroupMember] SET [DateTimeAdded] = [CreatedDateTime] where [DateTimeAdded] is null" );

            AddColumn("dbo.Attribute", "AllowSearch", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonPreviousName", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonPreviousName", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonPreviousName", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonPreviousName", new[] { "ForeignId" });
            DropIndex("dbo.PersonPreviousName", new[] { "Guid" });
            DropIndex("dbo.PersonPreviousName", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonPreviousName", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonPreviousName", "IDX_LastName");
            DropIndex("dbo.PersonPreviousName", new[] { "PersonAliasId" });
            DropColumn("dbo.Attribute", "AllowSearch");
            DropColumn("dbo.GroupMember", "DateTimeAdded");
            DropTable("dbo.PersonPreviousName");
        }
    }
}
