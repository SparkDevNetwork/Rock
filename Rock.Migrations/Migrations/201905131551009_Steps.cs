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
using Rock.SystemGuid;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class Steps : RockMigration
    {
        private const string StepNoteTypeGuidString = "2678D220-2852-49B7-963F-CA36BD1B6DBB";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// Add sample pages and blocks: https://gist.github.com/bjwiley2/a800176a96fbcda22a8759cf20f250da
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.StepType",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    StepProgramId = c.Int( nullable: false ),
                    Name = c.String( nullable: false, maxLength: 250 ),
                    Description = c.String(),
                    IconCssClass = c.String( maxLength: 100 ),
                    AllowMultiple = c.Boolean( nullable: false ),
                    HasEndDate = c.Boolean( nullable: false ),
                    AudienceDataViewId = c.Int(),
                    ShowCountOnBadge = c.Boolean( nullable: false ),
                    AutoCompleteDataViewId = c.Int(),
                    AllowManualEditing = c.Boolean( nullable: false ),
                    HighlightColor = c.String( maxLength: 100 ),
                    CardLavaTemplate = c.String(),
                    MergeTemplateId = c.Int(),
                    MergeTemplateDescriptor = c.String( maxLength: 50 ),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.DataView", t => t.AudienceDataViewId )
                .ForeignKey( "dbo.DataView", t => t.AutoCompleteDataViewId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.MergeTemplate", t => t.MergeTemplateId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.StepProgram", t => t.StepProgramId, cascadeDelete: true )
                .Index( t => t.StepProgramId )
                .Index( t => t.AudienceDataViewId )
                .Index( t => t.AutoCompleteDataViewId )
                .Index( t => t.MergeTemplateId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.StepProgram",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 250 ),
                    Description = c.String(),
                    IconCssClass = c.String( maxLength: 100 ),
                    CategoryId = c.Int( nullable: false ),
                    DefaultListView = c.Int( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Category", t => t.CategoryId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CategoryId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.StepStatus",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    StepProgramId = c.Int( nullable: false ),
                    IsCompleteStatus = c.Boolean( nullable: false ),
                    StatusColor = c.String( maxLength: 100 ),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.StepProgram", t => t.StepProgramId, cascadeDelete: true )
                .Index( t => t.StepProgramId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.Step",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    StepTypeId = c.Int( nullable: false ),
                    StepStatusId = c.Int(),
                    PersonAliasId = c.Int( nullable: false ),
                    CampusId = c.Int(),
                    CompletedDateTime = c.DateTime(),
                    StartDateTime = c.DateTime(),
                    EndDateTime = c.DateTime(),
                    Note = c.String(),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Campus", t => t.CampusId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true )
                .ForeignKey( "dbo.StepStatus", t => t.StepStatusId )
                .ForeignKey( "dbo.StepType", t => t.StepTypeId, cascadeDelete: true )
                .Index( t => t.StepTypeId )
                .Index( t => t.StepStatusId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.CampusId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.StepWorkflow",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    StepWorkflowTriggerId = c.Int( nullable: false ),
                    WorkflowId = c.Int( nullable: false ),
                    StepId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Step", t => t.StepId, cascadeDelete: true )
                .ForeignKey( "dbo.StepWorkflowTrigger", t => t.StepWorkflowTriggerId, cascadeDelete: true )
                .ForeignKey( "dbo.Workflow", t => t.WorkflowId, cascadeDelete: true )
                .Index( t => t.StepWorkflowTriggerId )
                .Index( t => t.WorkflowId )
                .Index( t => t.StepId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.StepWorkflowTrigger",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    StepProgramId = c.Int(),
                    StepTypeId = c.Int(),
                    WorkflowTypeId = c.Int( nullable: false ),
                    TriggerType = c.Int( nullable: false ),
                    TypeQualifier = c.String( maxLength: 200 ),
                    WorkflowName = c.String( maxLength: 100 ),
                    IsActive = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.StepProgram", t => t.StepProgramId )
                .ForeignKey( "dbo.StepType", t => t.StepTypeId )
                .ForeignKey( "dbo.WorkflowType", t => t.WorkflowTypeId )
                .Index( t => t.StepProgramId )
                .Index( t => t.StepTypeId )
                .Index( t => t.WorkflowTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.StepTypePrerequisite",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    StepTypeId = c.Int( nullable: false ),
                    PrerequisiteStepTypeId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.StepType", t => t.PrerequisiteStepTypeId )
                .ForeignKey( "dbo.StepType", t => t.StepTypeId, cascadeDelete: true )
                .Index( t => t.StepTypeId )
                .Index( t => t.PrerequisiteStepTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.EntityType", "AttributesSupportShowOnBulk", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Attribute", "ShowOnBulk", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.AttendanceOccurrence", "StepTypeId", c => c.Int() );
            CreateIndex( "dbo.AttendanceOccurrence", "StepTypeId" );
            AddForeignKey( "dbo.AttendanceOccurrence", "StepTypeId", "dbo.StepType", "Id", cascadeDelete: true );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Step", EntityType.STEP, true, true );

            Sql( string.Format( @"
If NOT EXISTS (SELECT * FROM NoteType WHERE Guid = '{0}')
BEGIN
	INSERT[dbo].[NoteType] (
		[IsSystem], 
		[EntityTypeId], 
		[Name],
		[Guid], [IconCssClass], 
		[Order],
        [UserSelectable]
	) VALUES (
		1, -- IsSystem
		(SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Step'), -- EntityTypeId
		N'Step Note', -- Name
		N'{0}', -- Guid
		N'fa fa-quote-left', -- IconCssClass
		0, -- Order
        1 -- UserSelectable
	);
END", StepNoteTypeGuidString ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( string.Format( "DELETE FROM NoteType WHERE Guid = '{0}';", StepNoteTypeGuidString ) );
            RockMigrationHelper.DeleteEntityType( EntityType.STEP );
            DropForeignKey( "dbo.AttendanceOccurrence", "StepTypeId", "dbo.StepType" );
            DropForeignKey( "dbo.StepTypePrerequisite", "StepTypeId", "dbo.StepType" );
            DropForeignKey( "dbo.StepTypePrerequisite", "PrerequisiteStepTypeId", "dbo.StepType" );
            DropForeignKey( "dbo.StepTypePrerequisite", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepTypePrerequisite", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepType", "StepProgramId", "dbo.StepProgram" );
            DropForeignKey( "dbo.StepWorkflow", "WorkflowId", "dbo.Workflow" );
            DropForeignKey( "dbo.StepWorkflow", "StepWorkflowTriggerId", "dbo.StepWorkflowTrigger" );
            DropForeignKey( "dbo.StepWorkflowTrigger", "WorkflowTypeId", "dbo.WorkflowType" );
            DropForeignKey( "dbo.StepWorkflowTrigger", "StepTypeId", "dbo.StepType" );
            DropForeignKey( "dbo.StepWorkflowTrigger", "StepProgramId", "dbo.StepProgram" );
            DropForeignKey( "dbo.StepWorkflowTrigger", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepWorkflowTrigger", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepWorkflow", "StepId", "dbo.Step" );
            DropForeignKey( "dbo.StepWorkflow", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepWorkflow", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Step", "StepTypeId", "dbo.StepType" );
            DropForeignKey( "dbo.Step", "StepStatusId", "dbo.StepStatus" );
            DropForeignKey( "dbo.Step", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Step", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Step", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Step", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.StepStatus", "StepProgramId", "dbo.StepProgram" );
            DropForeignKey( "dbo.StepStatus", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepStatus", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepProgram", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepProgram", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepProgram", "CategoryId", "dbo.Category" );
            DropForeignKey( "dbo.StepType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepType", "MergeTemplateId", "dbo.MergeTemplate" );
            DropForeignKey( "dbo.StepType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.StepType", "AutoCompleteDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.StepType", "AudienceDataViewId", "dbo.DataView" );
            DropIndex( "dbo.StepTypePrerequisite", new[] { "Guid" } );
            DropIndex( "dbo.StepTypePrerequisite", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepTypePrerequisite", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepTypePrerequisite", new[] { "PrerequisiteStepTypeId" } );
            DropIndex( "dbo.StepTypePrerequisite", new[] { "StepTypeId" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "Guid" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "WorkflowTypeId" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "StepTypeId" } );
            DropIndex( "dbo.StepWorkflowTrigger", new[] { "StepProgramId" } );
            DropIndex( "dbo.StepWorkflow", new[] { "Guid" } );
            DropIndex( "dbo.StepWorkflow", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepWorkflow", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepWorkflow", new[] { "StepId" } );
            DropIndex( "dbo.StepWorkflow", new[] { "WorkflowId" } );
            DropIndex( "dbo.StepWorkflow", new[] { "StepWorkflowTriggerId" } );
            DropIndex( "dbo.Step", new[] { "Guid" } );
            DropIndex( "dbo.Step", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Step", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Step", new[] { "CampusId" } );
            DropIndex( "dbo.Step", new[] { "PersonAliasId" } );
            DropIndex( "dbo.Step", new[] { "StepStatusId" } );
            DropIndex( "dbo.Step", new[] { "StepTypeId" } );
            DropIndex( "dbo.StepStatus", new[] { "Guid" } );
            DropIndex( "dbo.StepStatus", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepStatus", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepStatus", new[] { "StepProgramId" } );
            DropIndex( "dbo.StepProgram", new[] { "Guid" } );
            DropIndex( "dbo.StepProgram", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepProgram", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepProgram", new[] { "CategoryId" } );
            DropIndex( "dbo.StepType", new[] { "Guid" } );
            DropIndex( "dbo.StepType", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.StepType", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.StepType", new[] { "MergeTemplateId" } );
            DropIndex( "dbo.StepType", new[] { "AutoCompleteDataViewId" } );
            DropIndex( "dbo.StepType", new[] { "AudienceDataViewId" } );
            DropIndex( "dbo.StepType", new[] { "StepProgramId" } );
            DropIndex( "dbo.AttendanceOccurrence", new[] { "StepTypeId" } );
            DropColumn( "dbo.AttendanceOccurrence", "StepTypeId" );
            DropColumn( "dbo.Attribute", "ShowOnBulk" );
            DropColumn( "dbo.EntityType", "AttributesSupportShowOnBulk" );
            DropTable( "dbo.StepTypePrerequisite" );
            DropTable( "dbo.StepWorkflowTrigger" );
            DropTable( "dbo.StepWorkflow" );
            DropTable( "dbo.Step" );
            DropTable( "dbo.StepStatus" );
            DropTable( "dbo.StepProgram" );
            DropTable( "dbo.StepType" );
        }
    }
}