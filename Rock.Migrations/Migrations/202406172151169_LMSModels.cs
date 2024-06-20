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

    /// <summary>
    ///
    /// </summary>
    public partial class LMSModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.LearningActivityCompletion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningActivityId = c.Int(nullable: false),
                        StudentId = c.Int(nullable: false),
                        CompletedByPersonAliasId = c.Int(),
                        ActivityComponentCompletionJson = c.String(),
                        AvailableDateTime = c.DateTime(),
                        DueDate = c.DateTime(storeType: "date"),
                        CompletedDateTime = c.DateTime(),
                        FacilitatorComment = c.String(),
                        StudentComment = c.String(),
                        PointsEarned = c.Int(nullable: false),
                        IsStudentCompleted = c.Boolean(nullable: false),
                        IsFacilitatorCompleted = c.Boolean(nullable: false),
                        WasCompletedOnTime = c.Boolean(nullable: false),
                        NotificationCommunicationId = c.Int(),
                        BinaryFileId = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CompletedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.LearningActivity", t => t.LearningActivityId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.SystemCommunication", t => t.NotificationCommunicationId)
                .ForeignKey("dbo.LearningParticipant", t => t.StudentId)
                .Index(t => t.LearningActivityId)
                .Index(t => t.StudentId)
                .Index(t => t.CompletedByPersonAliasId)
                .Index(t => t.NotificationCommunicationId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningClassId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Order = c.Int(nullable: false),
                        ActivityComponentId = c.Int(nullable: false),
                        ActivityComponentSettingsJson = c.String(),
                        AssignTo = c.Int(nullable: false),
                        DueDateCalculationMethod = c.Int(nullable: false),
                        DueDateDefault = c.DateTime(storeType: "date"),
                        DueDateOffset = c.Int(),
                        AvailableDateCalculationMethod = c.Int(nullable: false),
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
                .ForeignKey("dbo.LearningClass", t => t.LearningClassId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningClassId)
                .Index(t => t.CompletionWorkflowTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningClassAnnouncement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 250),
                        Summary = c.String(maxLength: 500),
                        Description = c.String(),
                        DetailsUrl = c.String(),
                        LearningClassId = c.Int(nullable: false),
                        CommunicationMode = c.Int(nullable: false),
                        PublishDateTime = c.DateTime(nullable: false),
                        CommunicationSent = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.LearningClass", t => t.LearningClassId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningClassId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningClassContentPage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningClassId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 250),
                        Content = c.String(),
                        StartDateTime = c.DateTime(),
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
                .ForeignKey("dbo.LearningClass", t => t.LearningClassId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningClassId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningCourse",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        PublicName = c.String(nullable: false, maxLength: 100),
                        Summary = c.String(maxLength: 500),
                        Description = c.String(),
                        ImageBinaryFileId = c.Int(),
                        LearningProgramId = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        CourseCode = c.String(),
                        MaxStudents = c.Int(),
                        Credits = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        CompletionWorkflowTypeId = c.Int(),
                        EnableAnnouncements = c.Boolean(nullable: false),
                        AllowHistoricalAccess = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.WorkflowType", t => t.CompletionWorkflowTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.ImageBinaryFileId)
                .ForeignKey("dbo.LearningProgram", t => t.LearningProgramId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ImageBinaryFileId)
                .Index(t => t.LearningProgramId)
                .Index(t => t.CategoryId)
                .Index(t => t.CompletionWorkflowTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningCourseRequirement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningCourseId = c.Int(nullable: false),
                        RequiredLearningCourseId = c.Int(nullable: false),
                        RequirementType = c.Int(nullable: false),
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
                .ForeignKey("dbo.LearningCourse", t => t.LearningCourseId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.LearningCourse", t => t.RequiredLearningCourseId)
                .Index(t => t.LearningCourseId)
                .Index(t => t.RequiredLearningCourseId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningProgram",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        PublicName = c.String(nullable: false, maxLength: 100),
                        Summary = c.String(maxLength: 500),
                        Description = c.String(),
                        IconCssClass = c.String(maxLength: 100),
                        HighlightColor = c.String(maxLength: 50),
                        ImageBinaryFileId = c.Int(),
                        ConfigurationMode = c.Int(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        CategoryId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        IsCompletionStatusTracked = c.Boolean(nullable: false),
                        SystemCommunicationId = c.Int(nullable: false),
                        CompletionWorkflowTypeId = c.Int(),
                        AbsencesWarningCount = c.Int(),
                        AbsencesCriticalCount = c.Int(),
                        AdditionalSettingsJson = c.String(),
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
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.WorkflowType", t => t.CompletionWorkflowTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.ImageBinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.SystemCommunication", t => t.SystemCommunicationId)
                .Index(t => t.ImageBinaryFileId)
                .Index(t => t.CategoryId)
                .Index(t => t.SystemCommunicationId)
                .Index(t => t.CompletionWorkflowTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningProgramCompletion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LearningProgramId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        CampusId = c.Int(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        StartDateKey = c.Int(nullable: false),
                        EndDateKey = c.Int(),
                        CompletionStatus = c.Int(nullable: false),
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
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.LearningProgram", t => t.LearningProgramId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.LearningProgramId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CampusId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningSemester",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        LearningProgramId = c.Int(nullable: false),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        EnrollmentCloseDate = c.DateTime(storeType: "date"),
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
                .ForeignKey("dbo.LearningProgram", t => t.LearningProgramId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningProgramId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningGradingSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
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
            
            CreateTable(
                "dbo.LearningGradingSystemScale",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        ThresholdPercentage = c.Decimal(precision: 8, scale: 3),
                        IsPassing = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        LearningGradingSystemId = c.Int(nullable: false),
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
                .ForeignKey("dbo.LearningGradingSystem", t => t.LearningGradingSystemId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LearningGradingSystemId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.LearningParticipant",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LearningCompletionStatus = c.Int(nullable: false),
                        LearningCompletionDateTime = c.DateTime(),
                        LearningGradingSystemScaleId = c.Int(),
                        LearningGradePercent = c.Decimal(nullable: false, precision: 18, scale: 3),
                        LearningProgramCompletionId = c.Int(),
                        LearningClassId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupMember", t => t.Id)
                .ForeignKey("dbo.LearningGradingSystemScale", t => t.LearningGradingSystemScaleId)
                .ForeignKey("dbo.LearningProgramCompletion", t => t.LearningProgramCompletionId)
                .ForeignKey("dbo.LearningClass", t => t.LearningClassId)
                .Index(t => t.Id)
                .Index(t => t.LearningGradingSystemScaleId)
                .Index(t => t.LearningProgramCompletionId)
                .Index(t => t.LearningClassId);
            
            CreateTable(
                "dbo.LearningClass",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LearningCourseId = c.Int(nullable: false),
                        LearningSemesterId = c.Int(),
                        LearningGradingSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.Id)
                .ForeignKey("dbo.LearningCourse", t => t.LearningCourseId, cascadeDelete: true)
                .ForeignKey("dbo.LearningSemester", t => t.LearningSemesterId)
                .ForeignKey("dbo.LearningGradingSystem", t => t.LearningGradingSystemId)
                .Index(t => t.Id)
                .Index(t => t.LearningCourseId)
                .Index(t => t.LearningSemesterId)
                .Index(t => t.LearningGradingSystemId);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.LearningClass", "LearningGradingSystemId", "dbo.LearningGradingSystem");
            DropForeignKey("dbo.LearningClass", "LearningSemesterId", "dbo.LearningSemester");
            DropForeignKey("dbo.LearningClass", "LearningCourseId", "dbo.LearningCourse");
            DropForeignKey("dbo.LearningClass", "Id", "dbo.Group");
            DropForeignKey("dbo.LearningParticipant", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningParticipant", "LearningProgramCompletionId", "dbo.LearningProgramCompletion");
            DropForeignKey("dbo.LearningParticipant", "LearningGradingSystemScaleId", "dbo.LearningGradingSystemScale");
            DropForeignKey("dbo.LearningParticipant", "Id", "dbo.GroupMember");
            DropForeignKey("dbo.LearningActivityCompletion", "StudentId", "dbo.LearningParticipant");
            DropForeignKey("dbo.LearningActivityCompletion", "NotificationCommunicationId", "dbo.SystemCommunication");
            DropForeignKey("dbo.LearningActivityCompletion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningActivityCompletion", "LearningActivityId", "dbo.LearningActivity");
            DropForeignKey("dbo.LearningActivity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningActivity", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningGradingSystem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningGradingSystemScale", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningGradingSystemScale", "LearningGradingSystemId", "dbo.LearningGradingSystem");
            DropForeignKey("dbo.LearningGradingSystemScale", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningGradingSystem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningCourse", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningCourse", "LearningProgramId", "dbo.LearningProgram");
            DropForeignKey("dbo.LearningProgram", "SystemCommunicationId", "dbo.SystemCommunication");
            DropForeignKey("dbo.LearningProgram", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningSemester", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningSemester", "LearningProgramId", "dbo.LearningProgram");
            DropForeignKey("dbo.LearningSemester", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningProgramCompletion", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningProgramCompletion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningProgramCompletion", "LearningProgramId", "dbo.LearningProgram");
            DropForeignKey("dbo.LearningProgramCompletion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningProgramCompletion", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.LearningProgram", "ImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.LearningProgram", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningProgram", "CompletionWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.LearningProgram", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.LearningCourseRequirement", "RequiredLearningCourseId", "dbo.LearningCourse");
            DropForeignKey("dbo.LearningCourseRequirement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningCourseRequirement", "LearningCourseId", "dbo.LearningCourse");
            DropForeignKey("dbo.LearningCourseRequirement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningCourse", "ImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.LearningCourse", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningCourse", "CompletionWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.LearningCourse", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.LearningClassContentPage", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningClassContentPage", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningClassContentPage", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningClassAnnouncement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningClassAnnouncement", "LearningClassId", "dbo.LearningClass");
            DropForeignKey("dbo.LearningClassAnnouncement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningActivity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningActivity", "CompletionWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.LearningActivityCompletion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LearningActivityCompletion", "CompletedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.LearningClass", new[] { "LearningGradingSystemId" });
            DropIndex("dbo.LearningClass", new[] { "LearningSemesterId" });
            DropIndex("dbo.LearningClass", new[] { "LearningCourseId" });
            DropIndex("dbo.LearningClass", new[] { "Id" });
            DropIndex("dbo.LearningParticipant", new[] { "LearningClassId" });
            DropIndex("dbo.LearningParticipant", new[] { "LearningProgramCompletionId" });
            DropIndex("dbo.LearningParticipant", new[] { "LearningGradingSystemScaleId" });
            DropIndex("dbo.LearningParticipant", new[] { "Id" });
            DropIndex("dbo.LearningGradingSystemScale", new[] { "Guid" });
            DropIndex("dbo.LearningGradingSystemScale", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningGradingSystemScale", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningGradingSystemScale", new[] { "LearningGradingSystemId" });
            DropIndex("dbo.LearningGradingSystem", new[] { "Guid" });
            DropIndex("dbo.LearningGradingSystem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningGradingSystem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningSemester", new[] { "Guid" });
            DropIndex("dbo.LearningSemester", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningSemester", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningSemester", new[] { "LearningProgramId" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "Guid" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "CampusId" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "PersonAliasId" });
            DropIndex("dbo.LearningProgramCompletion", new[] { "LearningProgramId" });
            DropIndex("dbo.LearningProgram", new[] { "Guid" });
            DropIndex("dbo.LearningProgram", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningProgram", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningProgram", new[] { "CompletionWorkflowTypeId" });
            DropIndex("dbo.LearningProgram", new[] { "SystemCommunicationId" });
            DropIndex("dbo.LearningProgram", new[] { "CategoryId" });
            DropIndex("dbo.LearningProgram", new[] { "ImageBinaryFileId" });
            DropIndex("dbo.LearningCourseRequirement", new[] { "Guid" });
            DropIndex("dbo.LearningCourseRequirement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningCourseRequirement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningCourseRequirement", new[] { "RequiredLearningCourseId" });
            DropIndex("dbo.LearningCourseRequirement", new[] { "LearningCourseId" });
            DropIndex("dbo.LearningCourse", new[] { "Guid" });
            DropIndex("dbo.LearningCourse", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningCourse", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningCourse", new[] { "CompletionWorkflowTypeId" });
            DropIndex("dbo.LearningCourse", new[] { "CategoryId" });
            DropIndex("dbo.LearningCourse", new[] { "LearningProgramId" });
            DropIndex("dbo.LearningCourse", new[] { "ImageBinaryFileId" });
            DropIndex("dbo.LearningClassContentPage", new[] { "Guid" });
            DropIndex("dbo.LearningClassContentPage", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningClassContentPage", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningClassContentPage", new[] { "LearningClassId" });
            DropIndex("dbo.LearningClassAnnouncement", new[] { "Guid" });
            DropIndex("dbo.LearningClassAnnouncement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningClassAnnouncement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningClassAnnouncement", new[] { "LearningClassId" });
            DropIndex("dbo.LearningActivity", new[] { "Guid" });
            DropIndex("dbo.LearningActivity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningActivity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningActivity", new[] { "CompletionWorkflowTypeId" });
            DropIndex("dbo.LearningActivity", new[] { "LearningClassId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "Guid" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "NotificationCommunicationId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "CompletedByPersonAliasId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "StudentId" });
            DropIndex("dbo.LearningActivityCompletion", new[] { "LearningActivityId" });
            DropTable("dbo.LearningClass");
            DropTable("dbo.LearningParticipant");
            DropTable("dbo.LearningGradingSystemScale");
            DropTable("dbo.LearningGradingSystem");
            DropTable("dbo.LearningSemester");
            DropTable("dbo.LearningProgramCompletion");
            DropTable("dbo.LearningProgram");
            DropTable("dbo.LearningCourseRequirement");
            DropTable("dbo.LearningCourse");
            DropTable("dbo.LearningClassContentPage");
            DropTable("dbo.LearningClassAnnouncement");
            DropTable("dbo.LearningActivity");
            DropTable("dbo.LearningActivityCompletion");
        }
    }
}
