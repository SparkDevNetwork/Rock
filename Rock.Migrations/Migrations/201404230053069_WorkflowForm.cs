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
    public partial class WorkflowForm : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.WorkflowActionFormAttribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowActionFormId = c.Int(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        IsVisible = c.Boolean(nullable: false),
                        IsReadOnly = c.Boolean(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.WorkflowActionForm", t => t.WorkflowActionFormId, cascadeDelete: true)
                .Index(t => t.WorkflowActionFormId)
                .Index(t => t.AttributeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.WorkflowActionForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Header = c.String(),
                        Footer = c.String(),
                        InactiveMessage = c.String(),
                        Actions = c.String(maxLength: 300),
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
            
            AddColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 20));
            AddColumn("dbo.WorkflowActionType", "WorkflowFormId", c => c.Int());
            CreateIndex("dbo.WorkflowActionType", "WorkflowFormId");
            AddForeignKey("dbo.WorkflowActionType", "WorkflowFormId", "dbo.WorkflowActionForm", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowActionType", "WorkflowFormId", "dbo.WorkflowActionForm");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "WorkflowActionFormId", "dbo.WorkflowActionForm");
            DropForeignKey("dbo.WorkflowActionForm", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionForm", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "AttributeId", "dbo.Attribute");
            DropIndex("dbo.WorkflowActionType", new[] { "WorkflowFormId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "Guid" });
            DropIndex("dbo.WorkflowActionForm", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "Guid" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "AttributeId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "WorkflowActionFormId" });
            DropColumn("dbo.WorkflowActionType", "WorkflowFormId");
            DropColumn("dbo.WorkflowAction", "FormAction");
            DropTable("dbo.WorkflowActionForm");
            DropTable("dbo.WorkflowActionFormAttribute");
        }
    }
}
