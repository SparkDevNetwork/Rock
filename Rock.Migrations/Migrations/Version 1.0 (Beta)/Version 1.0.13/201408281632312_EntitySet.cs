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
    public partial class EntitySet : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.EntitySet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentEntitySetId = c.Int(),
                        Name = c.String(maxLength: 100),
                        EntityTypeId = c.Int(),
                        ExpireDateTime = c.DateTime(),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.EntitySet", t => t.ParentEntitySetId)
                .Index(t => t.ParentEntitySetId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.EntitySetItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntitySetId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntitySet", t => t.EntitySetId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntitySetId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            Sql( @"
    UPDATE [BinaryFileType] SET 
         [Name] = N'Transaction Image'
        ,[Description] = N'Scanned image of a check or envelope'
    WHERE [Guid] = N'6d18a9c4-34ab-444a-b95b-c644019465ac'

    UPDATE [Attribute] SET [IsGridColumn] = 1 
    WHERE [Guid] IN ('04712FBD-715D-412E-96C3-10C748482D6E', 'E0D71111-05F1-4CD3-959E-1D246613942E', '9D165825-21F2-4B5F-9A64-790A6EBD51AC')

    UPDATE [EntityType] SET 
         [Name] = 'Rock.Workflow.Action.AssignActivityFromAttributeValue'
        ,[AssemblyName] = 'Rock.Workflow.Action.AssignActivityFromAttributeValue, Rock, Version=1.0.12.0, Culture=neutral, PublicKeyToken=null'
    WHERE [Name] = 'Rock.Workflow.Action.AssignActivityToAttributeValue'

    UPDATE [EntityType] SET 
         [Name] = 'Rock.Workflow.Action.SetWorkflowName'
	    ,[AssemblyName] = 'Rock.Workflow.Action.SetWorkflowName, Rock, Version=1.0.12.0, Culture=neutral, PublicKeyToken=null'
    WHERE [Name] = 'Rock.Workflow.Action.SetName'

    UPDATE [EntityType] SET 
         [Name] = 'Rock.Workflow.Action.SetAttributeFromPerson'
	    ,[AssemblyName] = 'Rock.Workflow.Action.SetAttributeFromPerson, Rock, Version=1.0.12.0, Culture=neutral, PublicKeyToken=null'
    WHERE [Name] = 'Rock.Workflow.Action.SetPersonAttributeValue'
" );
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.EntitySet", "ParentEntitySetId", "dbo.EntitySet");
            DropForeignKey("dbo.EntitySet", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntitySetItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntitySetItem", "EntitySetId", "dbo.EntitySet");
            DropForeignKey("dbo.EntitySetItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EntitySet", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.EntitySet", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.EntitySetItem", new[] { "Guid" });
            DropIndex("dbo.EntitySetItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EntitySetItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EntitySetItem", new[] { "EntitySetId" });
            DropIndex("dbo.EntitySet", new[] { "Guid" });
            DropIndex("dbo.EntitySet", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EntitySet", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EntitySet", new[] { "EntityTypeId" });
            DropIndex("dbo.EntitySet", new[] { "ParentEntitySetId" });
            DropTable("dbo.EntitySetItem");
            DropTable("dbo.EntitySet");
        }
    }
}
