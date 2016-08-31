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
    public partial class PluginMigration : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PluginMigration",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PluginAssemblyName = c.String(nullable: false, maxLength: 512),
                        MigrationNumber = c.Int(nullable: false),
                        MigrationName = c.String(nullable: false, maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PluginMigration", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PluginMigration", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PluginMigration", new[] { "Guid" });
            DropIndex("dbo.PluginMigration", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PluginMigration", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.PluginMigration");
        }
    }
}
