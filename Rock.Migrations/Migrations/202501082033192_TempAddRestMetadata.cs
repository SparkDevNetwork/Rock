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
    public partial class TempAddRestMetadata : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //CreateTable(
            //    "dbo.EntitySearch",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(nullable: false, maxLength: 100),
            //            EntityTypeId = c.Int(nullable: false),
            //            Key = c.String(nullable: false, maxLength: 50),
            //            Description = c.String(),
            //            IsActive = c.Boolean(nullable: false),
            //            WhereExpression = c.String(),
            //            GroupByExpression = c.String(),
            //            SelectExpression = c.String(),
            //            SelectManyExpression = c.String(),
            //            SortExpression = c.String(),
            //            MaximumResultsPerQuery = c.Int(),
            //            IsEntitySecurityEnabled = c.Boolean(nullable: false),
            //            IncludePaths = c.String(maxLength: 200),
            //            IsRefinementAllowed = c.Boolean(nullable: false),
            //            CreatedDateTime = c.DateTime(),
            //            ModifiedDateTime = c.DateTime(),
            //            CreatedByPersonAliasId = c.Int(),
            //            ModifiedByPersonAliasId = c.Int(),
            //            Guid = c.Guid(nullable: false),
            //            ForeignId = c.Int(),
            //            ForeignGuid = c.Guid(),
            //            ForeignKey = c.String(maxLength: 100),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
            //    .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
            //    .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
            //    .Index(t => t.EntityTypeId)
            //    .Index(t => t.CreatedByPersonAliasId)
            //    .Index(t => t.ModifiedByPersonAliasId)
            //    .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.RestAction", "AdditionalSettingsJson", c => c.String());
            AddColumn("dbo.RestController", "AdditionalSettingsJson", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //DropForeignKey("dbo.EntitySearch", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            //DropForeignKey("dbo.EntitySearch", "EntityTypeId", "dbo.EntityType");
            //DropForeignKey("dbo.EntitySearch", "CreatedByPersonAliasId", "dbo.PersonAlias");
            //DropIndex("dbo.EntitySearch", new[] { "Guid" });
            //DropIndex("dbo.EntitySearch", new[] { "ModifiedByPersonAliasId" });
            //DropIndex("dbo.EntitySearch", new[] { "CreatedByPersonAliasId" });
            //DropIndex("dbo.EntitySearch", new[] { "EntityTypeId" });
            DropColumn("dbo.RestController", "AdditionalSettingsJson");
            DropColumn("dbo.RestAction", "AdditionalSettingsJson");
            //DropTable("dbo.EntitySearch");
        }
    }
}
