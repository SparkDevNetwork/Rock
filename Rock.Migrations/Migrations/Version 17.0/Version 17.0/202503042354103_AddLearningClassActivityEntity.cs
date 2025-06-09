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
    public partial class AddLearningClassActivityEntity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Overview:
            //
            // We are going to rename the LearningActivityCompletion table to
            // LearningClassActivityCompletion as the model was renamed.
            //
            // We renamed the LearningActivity model to LearningClassActivity. EF
            // is going to create a new table for LearningClassActivity and then
            // drop the old columns from the LearningActivity table, which will then
            // represent the new LearningActivity model.

            // Before we start, delete any activity related data so we don't
            // chance into a SQL error.
            Sql( "DELETE FROM [dbo].[LearningActivityCompletion]" );
            Sql( "DELETE FROM [dbo].[LearningActivity]" );

            // Drop foreign keys and indexes for columns that will be removed later.
            DropForeignKey("dbo.LearningActivity", "CompletionWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.LearningActivity", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningActivityCompletion", "LearningActivityId", "dbo.LearningActivity");
            DropIndex("dbo.LearningActivityCompletion", new[] { "LearningActivityId" });
            DropIndex("dbo.LearningActivity", new[] { "LearningClassId" });
            DropIndex("dbo.LearningActivity", new[] { "CompletionWorkflowTypeId" });

            // Rename LearningActivityCompletion to LearningClassActivityCompletion.
            RenameTable( name: "dbo.LearningActivityCompletion", newName: "LearningClassActivityCompletion" );

            // Create the LearningClassActivity table.
            CreateTable(
                "dbo.LearningClassActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningClassId = c.Int(nullable: false),
                        LearningActivityId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        AssignTo = c.Int(nullable: false),
                        DueDateCriteria = c.Int(nullable: false),
                        DueDateDefault = c.DateTime(storeType: "date"),
                        DueDateOffset = c.Int(),
                        AvailabilityCriteria = c.Int(nullable: false),
                        AvailableDateDefault = c.DateTime(storeType: "date"),
                        AvailableDateOffset = c.Int(),
                        TaskBinaryFileId = c.Int(),
                        Points = c.Int(nullable: false),
                        IsStudentCommentingEnabled = c.Boolean(nullable: false),
                        SendNotificationCommunication = c.Boolean(nullable: false),
                        CompletionWorkflowTypeId = c.Int(),
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
                .ForeignKey("dbo.WorkflowType", t => t.CompletionWorkflowTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.LearningActivity", t => t.LearningActivityId, cascadeDelete: true)
                .ForeignKey("dbo.LearningClass", t => t.LearningClassId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningClassId)
                .Index(t => t.LearningActivityId)
                .Index(t => t.CompletionWorkflowTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            // Add the new columns to existing tables.
            AddColumn("dbo.LearningClassActivityCompletion", "LearningClassActivityId", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "IsShared", c => c.Boolean(nullable: false));

            // Add new foreign keys and indexes for the new column on
            // LearningClassActivityCompletion and the existing column
            // on LearningActivity that should have referenced EntityType.
            CreateIndex("dbo.LearningActivity", "ActivityComponentId");
            CreateIndex("dbo.LearningClassActivityCompletion", "LearningClassActivityId");
            AddForeignKey("dbo.LearningActivity", "ActivityComponentId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.LearningClassActivityCompletion", "LearningClassActivityId", "dbo.LearningClassActivity", "Id", cascadeDelete: true);

            // Drop all the excess columns that are not needed now.
            DropColumn("dbo.LearningClassActivityCompletion", "LearningActivityId");
            DropColumn("dbo.LearningActivity", "LearningClassId");
            DropColumn("dbo.LearningActivity", "Order");
            DropColumn("dbo.LearningActivity", "AssignTo");
            DropColumn("dbo.LearningActivity", "DueDateCriteria");
            DropColumn("dbo.LearningActivity", "DueDateDefault");
            DropColumn("dbo.LearningActivity", "DueDateOffset");
            DropColumn("dbo.LearningActivity", "AvailabilityCriteria");
            DropColumn("dbo.LearningActivity", "AvailableDateDefault");
            DropColumn("dbo.LearningActivity", "AvailableDateOffset");
            DropColumn("dbo.LearningActivity", "TaskBinaryFileId");
            DropColumn("dbo.LearningActivity", "Points");
            DropColumn("dbo.LearningActivity", "IsStudentCommentingEnabled");
            DropColumn("dbo.LearningActivity", "SendNotificationCommunication");
            DropColumn("dbo.LearningActivity", "CompletionWorkflowTypeId");

            // Now that the schema is correct, update all the other Rock data
            // to match the new entity names.

            // Process the entity type changes.
            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY,
                "Rock.Model.LearningClassActivity",
                "Learning Class Activity",
                "Rock.Model.LearningClassActivity, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true );

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.LearningActivity",
                SystemGuid.EntityType.LEARNING_ACTIVITY,
                true,
                true );

            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY_COMPLETION,
                "Rock.Model.LearningClassActivityCompletion",
                "Learning Class Activity Completion",
                "Rock.Model.LearningClassActivityCompletion, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true );

            // Process the renamed data view filters.
            RockMigrationHelper.RenameEntityType( "fcb26028-6b54-4c2b-a47c-3b6f49a472c6",
                "Rock.Reporting.DataFilter.Person.HasCompletedClassActivityFilter",
                "Has Completed Class Activity Filter",
                "Rock.Reporting.DataFilter.Person.HasCompletedClassActivityFilter, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                true );

            // Process the renamed block types.
            RockMigrationHelper.RenameEntityType( "19474eb0-eeda-4fcb-b1ea-a35e23e6f691",
                "Rock.Blocks.Lms.LearningClassActivityCompletionDetail",
                "Learning Class Activity Completion Detail",
                "Rock.Blocks.Lms.LearningClassActivityCompletionDetail, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Activity Completion Detail",
                "Displays the details of a particular learning class activity completion.",
                "Rock.Blocks.Lms.LearningClassActivityCompletionDetail",
                "LMS",
                "4569f28d-1efb-4b95-a506-0d9043c24775" );


            RockMigrationHelper.RenameEntityType( "152fea00-5721-4cb2-897f-1b6829f4b7c4",
                "Rock.Blocks.Lms.LearningClassActivityCompletionList",
                "Learning Class Activity Completion List",
                "Rock.Blocks.Lms.LearningClassActivityCompletionList, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Activity Completion List",
                "Displays a list of learning class activity completions.",
                "Rock.Blocks.Lms.LearningClassActivityCompletionList",
                "LMS",
                "ef1a5cdd-6769-4ffc-b826-55c194b01897" );


            RockMigrationHelper.RenameEntityType( "fe13bfef-6266-4667-b51f-01af8e6c5b89",
                "Rock.Blocks.Lms.LearningClassActivityDetail",
                "Learning Class Activity Detail",
                "Rock.Blocks.Lms.LearningClassActivityDetail, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Activity Detail",
                "Displays the details of a particular learning class activity.",
                "Rock.Blocks.Lms.LearningClassActivityDetail",
                "LMS",
                "4b18bf0d-d91b-4934-ac2d-a7188b15b893" );

            // Update page routes to match new block page parameters.
            RockMigrationHelper.UpdatePageRoute( "e2581432-c9d8-4324-97e2-bcfe6bbd0f57",
                "d72dcbc4-c57f-4028-b503-1954925edc7d",
                "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningClassActivityId}" );

            RockMigrationHelper.UpdatePageRoute( "8c40ae8d-60c6-49de-b7de-be46d8a64aa6",
                "e0f2e4f1-ed10-49f6-b053-ac6807994204",
                "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningClassActivityId}/completions/{LearningClassActivityCompletionId}" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Before we start, delete any activity related data so we don't
            // chance into a SQL error.
            Sql( "DELETE FROM [dbo].[LearningClassActivityCompletion]" );
            Sql( "DELETE FROM [dbo].[LearningClassActivity]" );

            RockMigrationHelper.UpdatePageRoute( "e2581432-c9d8-4324-97e2-bcfe6bbd0f57",
                "d72dcbc4-c57f-4028-b503-1954925edc7d",
                "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}" );

            RockMigrationHelper.UpdatePageRoute( "8c40ae8d-60c6-49de-b7de-be46d8a64aa6",
                "e0f2e4f1-ed10-49f6-b053-ac6807994204",
                "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}/completions/{LearningActivityCompletionId}" );


            RockMigrationHelper.RenameEntityType( "fe13bfef-6266-4667-b51f-01af8e6c5b89",
                "Rock.Blocks.Lms.LearningActivityDetail",
                "Learning Activity Detail",
                "Rock.Blocks.Lms.LearningActivityDetail, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Detail",
                "Displays the details of a particular learning activity.",
                "Rock.Blocks.Lms.LearningActivityDetail",
                "LMS",
                "4b18bf0d-d91b-4934-ac2d-a7188b15b893" );


            RockMigrationHelper.RenameEntityType( "152fea00-5721-4cb2-897f-1b6829f4b7c4",
                "Rock.Blocks.Lms.LearningActivityCompletionList",
                "Learning Activity Completion List",
                "Rock.Blocks.Lms.LearningActivityCompletionList, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion List",
                "Displays a list of learning activity completions.",
                "Rock.Blocks.Lms.LearningActivityCompletionList",
                "LMS",
                "ef1a5cdd-6769-4ffc-b826-55c194b01897" );


            RockMigrationHelper.RenameEntityType( "19474eb0-eeda-4fcb-b1ea-a35e23e6f691",
                "Rock.Blocks.Lms.LearningActivityCompletionDetail",
                "Learning Activity Completion Detail",
                "Rock.Blocks.Lms.LearningActivityCompletionDetail, Rock.Blocks, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion Detail",
                "Displays the details of a particular learning activity completion.",
                "Rock.Blocks.Lms.LearningActivityCompletionDetail",
                "LMS",
                "4569f28d-1efb-4b95-a506-0d9043c24775" );


            RockMigrationHelper.RenameEntityType( "fcb26028-6b54-4c2b-a47c-3b6f49a472c6",
                "Rock.Reporting.DataFilter.Person.HasCompletedActivityFilter",
                "Has Completed Activity Filter",
                "Rock.Reporting.DataFilter.Person.HasCompletedActivityFilter, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                true );


            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY_COMPLETION,
                "Rock.Model.LearningActivityCompletion",
                "Learning Activity Completion",
                "Rock.Model.LearningActivityCompletion, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true );

            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.LEARNING_ACTIVITY );

            RockMigrationHelper.RenameEntityType( SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY,
                "Rock.Model.LearningActivity",
                "Learning Activity",
                "Rock.Model.LearningActivity, Rock, Version=18.0.1.0, Culture=neutral, PublicKeyToken=null",
                true,
                true );


            AddColumn( "dbo.LearningActivity", "CompletionWorkflowTypeId", c => c.Int());
            AddColumn("dbo.LearningActivity", "SendNotificationCommunication", c => c.Boolean(nullable: false));
            AddColumn("dbo.LearningActivity", "IsStudentCommentingEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.LearningActivity", "Points", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "TaskBinaryFileId", c => c.Int());
            AddColumn("dbo.LearningActivity", "AvailableDateOffset", c => c.Int());
            AddColumn("dbo.LearningActivity", "AvailableDateDefault", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.LearningActivity", "AvailabilityCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "DueDateOffset", c => c.Int());
            AddColumn("dbo.LearningActivity", "DueDateDefault", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.LearningActivity", "DueDateCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "AssignTo", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.LearningActivity", "LearningClassId", c => c.Int(nullable: false));
            AddColumn("dbo.LearningClassActivityCompletion", "LearningActivityId", c => c.Int(nullable: false));
            DropForeignKey("dbo.LearningClassActivity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningClassActivityCompletion", "LearningClassActivityId", "dbo.LearningClassActivity");
            DropForeignKey("dbo.LearningClassActivity", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningClassActivity", "LearningActivityId", "dbo.LearningActivity");
            DropForeignKey("dbo.LearningActivity", "ActivityComponentId", "dbo.EntityType");
            DropForeignKey("dbo.LearningClassActivity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningClassActivity", "CompletionWorkflowTypeId", "dbo.WorkflowType");
            DropIndex("dbo.LearningClassActivityCompletion", new[] { "LearningClassActivityId" });
            DropIndex("dbo.LearningActivity", new[] { "ActivityComponentId" });
            DropIndex("dbo.LearningClassActivity", new[] { "Guid" });
            DropIndex("dbo.LearningClassActivity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningClassActivity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningClassActivity", new[] { "CompletionWorkflowTypeId" });
            DropIndex("dbo.LearningClassActivity", new[] { "LearningActivityId" });
            DropIndex("dbo.LearningClassActivity", new[] { "LearningClassId" });
            DropColumn("dbo.LearningActivity", "IsShared");
            DropColumn("dbo.LearningClassActivityCompletion", "LearningClassActivityId");
            DropTable("dbo.LearningClassActivity");
            RenameTable( name: "dbo.LearningClassActivityCompletion", newName: "LearningActivityCompletion" );
            CreateIndex( "dbo.LearningActivity", "CompletionWorkflowTypeId");
            CreateIndex("dbo.LearningActivity", "LearningClassId");
            CreateIndex("dbo.LearningActivityCompletion", "LearningActivityId");
            AddForeignKey("dbo.LearningActivityCompletion", "LearningActivityId", "dbo.LearningActivity", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LearningActivity", "LearningClassId", "dbo.LearningClass", "Id");
            AddForeignKey("dbo.LearningActivity", "CompletionWorkflowTypeId", "dbo.WorkflowType", "Id");
        }
    }
}
