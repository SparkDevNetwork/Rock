//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class Workflow : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.coreCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        ParentCategoryId = c.Int(),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Name = c.String(maxLength: 100),
                        FileId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.coreCategory", t => t.ParentCategoryId)
                .ForeignKey("dbo.coreEntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.cmsFile", t => t.FileId)
                .Index(t => t.ParentCategoryId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.FileId);
            
            CreateIndex( "dbo.coreCategory", "Guid", true );
            CreateTable(
                "dbo.coreEntityTypeWorkflowTrigger",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        WorkflowTypeId = c.Int(nullable: false),
                        EntityTriggerType = c.Int(nullable: false),
                        WorkflowName = c.String(nullable: false, maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.coreEntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.utilWorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.coreEntityTypeWorkflowTrigger", "Guid", true );
            CreateTable(
                "dbo.utilWorkflowType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CategoryId = c.Int(),
                        Order = c.Int(nullable: false),
                        FileId = c.Int(),
                        WorkTerm = c.String(nullable: false, maxLength: 100),
                        EntryActivityTypeId = c.Int(),
                        ProcessingIntervalSeconds = c.Int(),
                        IsPersisted = c.Boolean(nullable: false),
                        LoggingLevel = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.coreCategory", t => t.CategoryId)
                .ForeignKey("dbo.cmsFile", t => t.FileId)
                .ForeignKey("dbo.utilActivityType", t => t.EntryActivityTypeId)
                .Index(t => t.CategoryId)
                .Index(t => t.FileId)
                .Index(t => t.EntryActivityTypeId);
            
            CreateIndex( "dbo.utilWorkflowType", "Guid", true );
            CreateTable(
                "dbo.utilActivityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(),
                        WorkflowTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActivatedWithWorkflow = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilWorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.utilActivityType", "Guid", true );
            CreateTable(
                "dbo.utilActionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        IsActionCompletedOnSuccess = c.Boolean(nullable: false),
                        IsActivityCompletedOnSuccess = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilActivityType", t => t.ActivityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.coreEntityType", t => t.EntityTypeId, cascadeDelete: true)
                .Index(t => t.ActivityTypeId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.utilActionType", "Guid", true );
            CreateTable(
                "dbo.utilAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityId = c.Int(nullable: false),
                        ActionTypeId = c.Int(nullable: false),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilActivity", t => t.ActivityId, cascadeDelete: true)
                .ForeignKey("dbo.utilActionType", t => t.ActionTypeId)
                .Index(t => t.ActivityId)
                .Index(t => t.ActionTypeId);
            
            CreateIndex( "dbo.utilAction", "Guid", true );
            CreateTable(
                "dbo.utilActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowId = c.Int(nullable: false),
                        ActivityTypeId = c.Int(nullable: false),
                        ActivatedDateTime = c.DateTime(),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilWorkflow", t => t.WorkflowId, cascadeDelete: true)
                .ForeignKey("dbo.utilActivityType", t => t.ActivityTypeId)
                .Index(t => t.WorkflowId)
                .Index(t => t.ActivityTypeId);
            
            CreateIndex( "dbo.utilActivity", "Guid", true );
            CreateTable(
                "dbo.utilWorkflow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Status = c.String(nullable: false, maxLength: 100),
                        IsProcessing = c.Boolean(nullable: false),
                        ActivatedDateTime = c.DateTime(),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilWorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.utilWorkflow", "Guid", true );
            CreateTable(
                "dbo.utilWorkflowLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowId = c.Int(nullable: false),
                        LogDateTime = c.DateTime(nullable: false),
                        LogText = c.String(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.utilWorkflow", t => t.WorkflowId, cascadeDelete: true)
                .Index(t => t.WorkflowId);
            
            CreateIndex( "dbo.utilWorkflowLog", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.utilWorkflowLog", new[] { "WorkflowId" });
            DropIndex("dbo.utilWorkflow", new[] { "WorkflowTypeId" });
            DropIndex("dbo.utilActivity", new[] { "ActivityTypeId" });
            DropIndex("dbo.utilActivity", new[] { "WorkflowId" });
            DropIndex("dbo.utilAction", new[] { "ActionTypeId" });
            DropIndex("dbo.utilAction", new[] { "ActivityId" });
            DropIndex("dbo.utilActionType", new[] { "EntityTypeId" });
            DropIndex("dbo.utilActionType", new[] { "ActivityTypeId" });
            DropIndex("dbo.utilActivityType", new[] { "WorkflowTypeId" });
            DropIndex("dbo.utilWorkflowType", new[] { "EntryActivityTypeId" });
            DropIndex("dbo.utilWorkflowType", new[] { "FileId" });
            DropIndex("dbo.utilWorkflowType", new[] { "CategoryId" });
            DropIndex("dbo.coreEntityTypeWorkflowTrigger", new[] { "WorkflowTypeId" });
            DropIndex("dbo.coreEntityTypeWorkflowTrigger", new[] { "EntityTypeId" });
            DropIndex("dbo.coreCategory", new[] { "FileId" });
            DropIndex("dbo.coreCategory", new[] { "EntityTypeId" });
            DropIndex("dbo.coreCategory", new[] { "ParentCategoryId" });
            DropForeignKey("dbo.utilWorkflowLog", "WorkflowId", "dbo.utilWorkflow");
            DropForeignKey("dbo.utilWorkflow", "WorkflowTypeId", "dbo.utilWorkflowType");
            DropForeignKey("dbo.utilActivity", "ActivityTypeId", "dbo.utilActivityType");
            DropForeignKey("dbo.utilActivity", "WorkflowId", "dbo.utilWorkflow");
            DropForeignKey("dbo.utilAction", "ActionTypeId", "dbo.utilActionType");
            DropForeignKey("dbo.utilAction", "ActivityId", "dbo.utilActivity");
            DropForeignKey("dbo.utilActionType", "EntityTypeId", "dbo.coreEntityType");
            DropForeignKey("dbo.utilActionType", "ActivityTypeId", "dbo.utilActivityType");
            DropForeignKey("dbo.utilActivityType", "WorkflowTypeId", "dbo.utilWorkflowType");
            DropForeignKey("dbo.utilWorkflowType", "EntryActivityTypeId", "dbo.utilActivityType");
            DropForeignKey("dbo.utilWorkflowType", "FileId", "dbo.cmsFile");
            DropForeignKey("dbo.utilWorkflowType", "CategoryId", "dbo.coreCategory");
            DropForeignKey("dbo.coreEntityTypeWorkflowTrigger", "WorkflowTypeId", "dbo.utilWorkflowType");
            DropForeignKey("dbo.coreEntityTypeWorkflowTrigger", "EntityTypeId", "dbo.coreEntityType");
            DropForeignKey("dbo.coreCategory", "FileId", "dbo.cmsFile");
            DropForeignKey("dbo.coreCategory", "EntityTypeId", "dbo.coreEntityType");
            DropForeignKey("dbo.coreCategory", "ParentCategoryId", "dbo.coreCategory");
            DropTable("dbo.utilWorkflowLog");
            DropTable("dbo.utilWorkflow");
            DropTable("dbo.utilActivity");
            DropTable("dbo.utilAction");
            DropTable("dbo.utilActionType");
            DropTable("dbo.utilActivityType");
            DropTable("dbo.utilWorkflowType");
            DropTable("dbo.coreEntityTypeWorkflowTrigger");
            DropTable("dbo.coreCategory");
        }
    }
}
