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
    public partial class AttributeMatrix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttributeMatrix",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeMatrixTemplateId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttributeMatrixTemplate", t => t.AttributeMatrixTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.AttributeMatrixTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AttributeMatrixItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeMatrixId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttributeMatrix", t => t.AttributeMatrixId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.AttributeMatrixId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AttributeMatrixTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        MinimumRows = c.Int(),
                        MaximumRows = c.Int(),
                        FormattedLava = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.UpdateFieldType("Matrix", "", "Rock", "Rock.Field.Types.MatrixFieldType", "24FF4AEA-693D-4B25-A4E0-347DD670D2BA");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.AttributeMatrix", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrix", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrix", "AttributeMatrixTemplateId", "dbo.AttributeMatrixTemplate");
            DropForeignKey("dbo.AttributeMatrixTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AttributeMatrixItem", "AttributeMatrixId", "dbo.AttributeMatrix");
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrixItem", new[] { "AttributeMatrixId" });
            DropIndex("dbo.AttributeMatrix", new[] { "Guid" });
            DropIndex("dbo.AttributeMatrix", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrix", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.AttributeMatrix", new[] { "AttributeMatrixTemplateId" });
            DropTable("dbo.AttributeMatrixTemplate");
            DropTable("dbo.AttributeMatrixItem");
            DropTable("dbo.AttributeMatrix");
        }
    }
}
