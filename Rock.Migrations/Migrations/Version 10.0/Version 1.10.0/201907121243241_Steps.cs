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
    public partial class Steps : Rock.Migrations.RockMigration
    {
        private const string StepNoteTypeGuidString = "2678D220-2852-49B7-963F-CA36BD1B6DBB";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelsUps();
            PagesAndBlocksUp();
            TagUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            TagDown();
            PagesAndBlocksDown();
            ModelsDown();
        }

        private void ModelsUps()
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

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Step", SystemGuid.EntityType.STEP, true, true );

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

            DropIndex( "dbo.StepProgram", new[] { "CategoryId" } );
            AddColumn( "dbo.StepProgram", "StepTerm", c => c.String( nullable: false, maxLength: 100 ) );
            AlterColumn( "dbo.StepProgram", "CategoryId", c => c.Int() );
            CreateIndex( "dbo.StepProgram", "CategoryId" );
        }

        private void ModelsDown()
        {
            DropIndex( "dbo.StepProgram", new[] { "CategoryId" } );
            AlterColumn( "dbo.StepProgram", "CategoryId", c => c.Int( nullable: false ) );
            DropColumn( "dbo.StepProgram", "StepTerm" );
            CreateIndex( "dbo.StepProgram", "CategoryId" );

            Sql( string.Format( "DELETE FROM NoteType WHERE Guid = '{0}';", StepNoteTypeGuidString ) );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.STEP );
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

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Steps", "", "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program", "", "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Type", "", "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Steps", "", "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step", "", "2109228C-D828-4B58-9310-8D93D10B846E", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step", "", "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "Steps", "4E4280B8-0A10-401A-9D69-687CA66A7B76" );// for Page:Steps
            RockMigrationHelper.AddPageRoute( "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "Steps/Program/{ProgramId}", "0B796F9D-1294-40E7-B264-D460D62B4F2F" );// for Page:Program
            RockMigrationHelper.AddPageRoute( "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "Steps/Type/{StepTypeId}", "74DF0B98-B980-4EF7-B879-7A028535C3FA" );// for Page:Type
            RockMigrationHelper.AddPageRoute( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "Person/{PersonId}/Steps", "181A8246-0F80-44BE-A448-DADF680E6F73" );// for Page:Steps
            RockMigrationHelper.AddPageRoute( "2109228C-D828-4B58-9310-8D93D10B846E", "Steps/Record/{StepId}", "C72F337F-4320-4CED-B5FF-20A443268123" );// for Page:Step
            RockMigrationHelper.AddPageRoute( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "Person/{PersonId}/Steps/{StepTypeId}/{StepId}", "6BA3B394-C827-4548-94AE-CA9AD585CF3A" );// for Page:Step
            RockMigrationHelper.UpdateBlockType( "Steps", "Displays step records for a person in a step program.", "~/Blocks/Steps/PersonProgramStepList.ascx", "Steps", "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" );
            RockMigrationHelper.UpdateBlockType( "Step Entry", "Displays a form to add or edit a step.", "~/Blocks/Steps/StepEntry.ascx", "Steps", "8D78BC55-6E67-40AB-B453-994D69503838" );
            RockMigrationHelper.UpdateBlockType( "Step Participant List", "Lists all the participants in a Step.", "~/Blocks/Steps/StepParticipantList.ascx", "Steps", "2E4A1578-145E-4052-9B56-1739F7366827" );
            RockMigrationHelper.UpdateBlockType( "Step Program Detail", "Displays the details of the given Step Program for editing.", "~/Blocks/Steps/StepProgramDetail.ascx", "Steps", "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" );
            RockMigrationHelper.UpdateBlockType( "Step Program List", "Shows a list of all step programs.", "~/Blocks/Steps/StepProgramList.ascx", "Steps", "429A817E-1379-4BCC-AEFE-01D9C75273E5" );
            RockMigrationHelper.UpdateBlockType( "Step Type Detail", "Displays the details of the given Step Type for editing.", "~/Blocks/Steps/StepTypeDetail.ascx", "Steps", "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD" );
            RockMigrationHelper.UpdateBlockType( "Step Type List", "Shows a list of all step types for a program.", "~/Blocks/Steps/StepTypeList.ascx", "Steps", "3EFB4302-9AB4-420F-A818-48B1B06AD109" );
            // Add Block to Page: Steps Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F5E8A369-4856-42E5-B187-276DFCEB1F3F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "429A817E-1379-4BCC-AEFE-01D9C75273E5".AsGuid(), "Step Program List", "Main", @"", @"", 0, "6AD9C580-387D-413F-9791-EB0DF4D382FC" );
            // Add Block to Page: Program Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CF372F6E-7131-4FF7-8BCD-6053DBB67D34".AsGuid(), "Step Program Detail", "Main", @"", @"", 0, "84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93" );
            // Add Block to Page: Program Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3EFB4302-9AB4-420F-A818-48B1B06AD109".AsGuid(), "Step Type List", "Main", @"", @"", 1, "B7DFAB79-858E-4D44-BD74-38B273BA1EBB" );
            // Add Block to Page: Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E4A1578-145E-4052-9B56-1739F7366827".AsGuid(), "Step Participant List", "Main", @"", @"", 1, "9F149FB6-95BA-4B4F-B98B-20A29892D03B" );
            // Add Block to Page: Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD".AsGuid(), "Step Type Detail", "Main", @"", @"", 0, "B572C7D7-3989-4BEA-AC63-3447B5CF7ED8" );
            // Add Block to Page: Steps Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CB9ABA3B-6962-4A42-BDA1-EA71B7309232".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4".AsGuid(), "Steps", "SectionC1", @"", @"", 0, "46E5C15A-44A5-4FB3-8CE8-572FB0D85367" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2109228C-D828-4B58-9310-8D93D10B846E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D78BC55-6E67-40AB-B453-994D69503838".AsGuid(), "Step Entry", "Main", @"", @"", 0, "74E22668-FE00-4238-AC40-6A2DACD48F56" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D78BC55-6E67-40AB-B453-994D69503838".AsGuid(), "Step Entry", "Main", @"", @"", 0, "826E3498-AEC5-45DF-A454-F3AD19573714" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93'" );  // Page: Program,  Zone: Main,  Block: Step Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'B572C7D7-3989-4BEA-AC63-3447B5CF7ED8'" );  // Page: Type,  Zone: Main,  Block: Step Type Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '9F149FB6-95BA-4B4F-B98B-20A29892D03B'" );  // Page: Type,  Zone: Main,  Block: Step Participant List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B7DFAB79-858E-4D44-BD74-38B273BA1EBB'" );  // Page: Program,  Zone: Main,  Block: Step Type List
            // Attrib for BlockType: Step Participant List:Show Note Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Note Column", "ShowNoteColumn", "", @"Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?", 3, @"False", "FDEAB18C-637C-4B7F-A742-437AAB53C0C4" );
            // Attrib for BlockType: Step Program Detail:Show Chart
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "Show Chart", "", @"", 0, @"true", "ECF644EF-C74F-4182-82EB-56BFC9C63630" );
            // Attrib for BlockType: Step Type Detail:Show Chart
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "ShowChart", "", @"", 0, @"true", "793E9E60-B1DA-4D76-A7B5-8C933E87B574" );
            // Attrib for BlockType: Steps:Steps Per Row
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Steps Per Row", "StepsPerRow", "", @"The number of step cards that should be shown on a row", 3, @"6", "FBAB162B-556B-44DA-B830-D629529C0542" );
            // Attrib for BlockType: Steps:Steps Per Row Mobile
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Steps Per Row Mobile", "StepsPerRowMobile", "", @"The number of step cards that should be shown on a row on a mobile screen size", 4, @"2", "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75" );
            // Attrib for BlockType: Step Entry:Step Type Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "8D78BC55-6E67-40AB-B453-994D69503838", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Step Type Id", "StepType", "", @"The step type to use to add a new step. Leave blank to use the query string: StepTypeId. The type of the step, if step id is specified, overrides this setting.", 1, @"", "BB5B26FD-1056-4C2E-85F2-A87F3FA45D68" );
            // Attrib for BlockType: Steps:Step Entry Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Step Entry Page", "StepPage", "", @"The page where step records can be edited or added", 2, @"", "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500" );
            // Attrib for BlockType: Step Entry:Success Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "8D78BC55-6E67-40AB-B453-994D69503838", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Success Page", "SuccessPage", "", @"The page to navigate to once the add or edit has completed. Leave blank to navigate to the parent page.", 2, @"", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB" );
            // Attrib for BlockType: Step Participant List:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", @"Page used for viewing a person's profile. If set a view profile button will show for each participant.", 2, @"", "D9ECF014-7936-4D87-809D-56303AB15C7D" );
            // Attrib for BlockType: Step Participant List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "2D657072-4288-4AC4-98AE-79833C84AB11" );
            // Attrib for BlockType: Step Program List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 2, @"", "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6" );
            // Attrib for BlockType: Step Type List:Bulk Entry
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Entry", "BulkEntryPage", "", @"Linked page that allows for bulk entry of steps for a step type.", 3, @"", "A5791226-ABCC-4BBA-BDE9-CE605B8AC2DD" );
            // Attrib for BlockType: Step Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 2, @"", "09478006-E619-4210-9A5B-8442576259B5" );
            // Attrib for BlockType: Step Program Detail:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "Chart Style", "", @"", 1, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "6910B0DD-3F05-42F9-9183-928E076A82F3" );
            // Attrib for BlockType: Step Type Detail:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", @"", 1, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "7B950595-6CDF-4A6C-A418-974849F3FC2D" );
            // Attrib for BlockType: Step Program List:Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Categories", "Categories", "", @"If block should only display Step Programs from specific categories, select the categories here.", 1, @"", "CF2E1C24-EEE8-4375-9282-257A3B72A996" );
            // Attrib for BlockType: Step Type Detail:Data View Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Data View Categories", "DataViewCategories", "", @"The categories from which the Audience and Autocomplete data view options can be selected. If empty, all data views will be available.", 7, @"", "81BAADD8-F855-495C-BC07-FF97CE7AB8DF" );
            // Attrib for BlockType: Step Program Detail:Default Chart Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "", @"", 2, @"Current||Year||", "6DF66E4A-2F7E-4A7C-9A90-B12EC711D4A2" );
            // Attrib for BlockType: Step Type Detail:Default Chart Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "", @"", 2, @"Current||Year||", "ACBEFEFC-9605-4645-90A9-0E50506D19C8" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "StepProgram", "", @"The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.", 1, @"", "625E9A05-AF25-4886-9961-4F00263EBC82" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "Programs", "", @"Display Step Types from a specified program. If none selected, the block will display the program from the current context.", 1, @"", "3728C201-1D50-456C-8CF2-29A380558F67" );
            // Attrib Value for Block:Step Program List, Attribute:Detail Page Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AD9C580-387D-413F-9791-EB0DF4D382FC", "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6", @"6e46bc35-1fcb-4619-84f0-bb6926d2ddd5,0b796f9d-1294-40e7-b264-d460d62b4f2f" );
            // Attrib Value for Block:Step Type List, Attribute:Detail Page Page: Program, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B7DFAB79-858E-4D44-BD74-38B273BA1EBB", "09478006-E619-4210-9A5B-8442576259B5", @"8e78f9dc-657d-41bf-be0f-56916b6bf92f,74df0b98-b980-4ef7-b879-7a028535c3fa" );
            // Attrib Value for Block:Step Participant List, Attribute:Person Profile Page Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "D9ECF014-7936-4D87-809D-56303AB15C7D", @"cb9aba3b-6962-4a42-bda1-ea71b7309232,181a8246-0f80-44be-a448-dadf680e6f73" );
            // Attrib Value for Block:Step Participant List, Attribute:Detail Page Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "2D657072-4288-4AC4-98AE-79833C84AB11", @"2109228c-d828-4b58-9310-8d93d10b846e,c72f337f-4320-4ced-b5ff-20a443268123" );
            // Attrib Value for Block:Step Participant List, Attribute:Show Note Column Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "FDEAB18C-637C-4B7F-A742-437AAB53C0C4", @"False" );
            // Attrib Value for Block:Step Participant List, Attribute:core.CustomGridEnableStickyHeaders Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "9120E7A5-658B-4EA5-8517-A072ED5B0C20", @"False" );
            // Attrib Value for Block:Steps, Attribute:Steps Per Row Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "FBAB162B-556B-44DA-B830-D629529C0542", @"6" );
            // Attrib Value for Block:Steps, Attribute:Steps Per Row Mobile Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75", @"2" );
            // Attrib Value for Block:Steps, Attribute:Step Entry Page Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500", @"7a04966a-8e4e-49ea-a03c-7dd4b52a7b28,6ba3b394-c827-4548-94ae-ca9ad585cf3a" );
            // Attrib Value for Block:Step Entry, Attribute:Success Page Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "74E22668-FE00-4238-AC40-6A2DACD48F56", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB", @"8e78f9dc-657d-41bf-be0f-56916b6bf92f,74df0b98-b980-4ef7-b879-7a028535c3fa" );
            // Attrib Value for Block:Step Entry, Attribute:Success Page Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "826E3498-AEC5-45DF-A454-F3AD19573714", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB", @"cb9aba3b-6962-4a42-bda1-ea71b7309232,181a8246-0f80-44be-a448-dadf680e6f73" );
            RockMigrationHelper.UpdateFieldType( "Registry Entry", "", "Rock", "Rock.Field.Types.RegistryEntryFieldType", "D98E1D88-2240-4248-B93B-0512BD3BB61A" );
            RockMigrationHelper.UpdateFieldType( "Step Program", "", "Rock", "Rock.Field.Types.StepProgramFieldType", "33875369-7D2B-4CD7-BB89-ABC29906CCAE" );
            // Add/Update PageContext for Page:Steps, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "Rock.Model.Person", "PersonId", "A14075A7-8A09-424D-AC71-D81535194EB7" );

            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2109228C-D828-4B58-9310-8D93D10B846E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3".AsGuid(), "Notes", "Main", @"", @"", 1, "9410BE4C-134E-47D1-93D0-62248280B67F" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3".AsGuid(), "Notes", "Main", @"", @"", 1, "457489AD-D236-4994-8A0C-6E89423FBE3B" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '74E22668-FE00-4238-AC40-6A2DACD48F56'" );  // Page: Step,  Zone: Main,  Block: Step Entry
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '826E3498-AEC5-45DF-A454-F3AD19573714'" );  // Page: Step,  Zone: Main,  Block: Step Entry
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '457489AD-D236-4994-8A0C-6E89423FBE3B'" );  // Page: Step,  Zone: Main,  Block: Notes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '9410BE4C-134E-47D1-93D0-62248280B67F'" );  // Page: Step,  Zone: Main,  Block: Notes
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "Programs", "", @"Display Step Types from a specified program. If none selected, the block will display the program from the current context.", 1, @"", "C46D400B-277B-4B9C-B4E2-FB767F73D88F" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "StepProgram", "", @"The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.", 1, @"", "B4E07CC9-53E0-47CF-AC22-10F2085547A3" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"8eadb0dc-17f4-4541-a46e-53f89e21a622" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-quote-left" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2678D220-2852-49B7-963F-CA36BD1B6DBB" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2678D220-2852-49B7-963F-CA36BD1B6DBB" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-quote-left" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"8eadb0dc-17f4-4541-a46e-53f89e21a622" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Add/Update PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.UpdatePageContext( "2109228C-D828-4B58-9310-8D93D10B846E", "Rock.Model.Step", "StepId", "C73B4EC9-4572-44F4-8E10-A7069FCD5233" );
            // Add/Update PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.UpdatePageContext( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "Rock.Model.Step", "StepId", "B6C2659F-4C2F-4942-B476-427CFB49B979" );
        }

        private void PagesAndBlocksDown()
        {
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.DeleteAttribute( "B4E07CC9-53E0-47CF-AC22-10F2085547A3" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.DeleteAttribute( "C46D400B-277B-4B9C-B4E2-FB767F73D88F" );
            // Remove Block: Notes, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "457489AD-D236-4994-8A0C-6E89423FBE3B" );
            // Remove Block: Notes, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9410BE4C-134E-47D1-93D0-62248280B67F" );
            // Delete PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.DeletePageContext( "C73B4EC9-4572-44F4-8E10-A7069FCD5233" );
            // Delete PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.DeletePageContext( "B6C2659F-4C2F-4942-B476-427CFB49B979" );

            // Attrib for BlockType: Step Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "09478006-E619-4210-9A5B-8442576259B5" );
            // Attrib for BlockType: Step Type List:Bulk Entry
            RockMigrationHelper.DeleteAttribute( "A5791226-ABCC-4BBA-BDE9-CE605B8AC2DD" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.DeleteAttribute( "3728C201-1D50-456C-8CF2-29A380558F67" );
            // Attrib for BlockType: Step Type Detail:Chart Style
            RockMigrationHelper.DeleteAttribute( "7B950595-6CDF-4A6C-A418-974849F3FC2D" );
            // Attrib for BlockType: Step Type Detail:Show Chart
            RockMigrationHelper.DeleteAttribute( "793E9E60-B1DA-4D76-A7B5-8C933E87B574" );
            // Attrib for BlockType: Step Type Detail:Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "ACBEFEFC-9605-4645-90A9-0E50506D19C8" );
            // Attrib for BlockType: Step Type Detail:Data View Categories
            RockMigrationHelper.DeleteAttribute( "81BAADD8-F855-495C-BC07-FF97CE7AB8DF" );
            // Attrib for BlockType: Step Program List:Detail Page
            RockMigrationHelper.DeleteAttribute( "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6" );
            // Attrib for BlockType: Step Program List:Categories
            RockMigrationHelper.DeleteAttribute( "CF2E1C24-EEE8-4375-9282-257A3B72A996" );
            // Attrib for BlockType: Step Program Detail:Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "6DF66E4A-2F7E-4A7C-9A90-B12EC711D4A2" );
            // Attrib for BlockType: Step Program Detail:Chart Style
            RockMigrationHelper.DeleteAttribute( "6910B0DD-3F05-42F9-9183-928E076A82F3" );
            // Attrib for BlockType: Step Program Detail:Show Chart
            RockMigrationHelper.DeleteAttribute( "ECF644EF-C74F-4182-82EB-56BFC9C63630" );
            // Attrib for BlockType: Step Participant List:Show Note Column
            RockMigrationHelper.DeleteAttribute( "FDEAB18C-637C-4B7F-A742-437AAB53C0C4" );
            // Attrib for BlockType: Step Participant List:Detail Page
            RockMigrationHelper.DeleteAttribute( "2D657072-4288-4AC4-98AE-79833C84AB11" );
            // Attrib for BlockType: Step Participant List:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "D9ECF014-7936-4D87-809D-56303AB15C7D" );
            // Attrib for BlockType: Step Entry:Success Page
            RockMigrationHelper.DeleteAttribute( "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB" );
            // Attrib for BlockType: Step Entry:Step Type Id
            RockMigrationHelper.DeleteAttribute( "BB5B26FD-1056-4C2E-85F2-A87F3FA45D68" );
            // Attrib for BlockType: Steps:Step Entry Page
            RockMigrationHelper.DeleteAttribute( "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500" );
            // Attrib for BlockType: Steps:Steps Per Row Mobile
            RockMigrationHelper.DeleteAttribute( "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75" );
            // Attrib for BlockType: Steps:Steps Per Row
            RockMigrationHelper.DeleteAttribute( "FBAB162B-556B-44DA-B830-D629529C0542" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.DeleteAttribute( "625E9A05-AF25-4886-9961-4F00263EBC82" );
            // Remove Block: Step Entry, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "826E3498-AEC5-45DF-A454-F3AD19573714" );
            // Remove Block: Step Entry, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "74E22668-FE00-4238-AC40-6A2DACD48F56" );
            // Remove Block: Steps, from Page: Steps, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367" );
            // Remove Block: Step Participant List, from Page: Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9F149FB6-95BA-4B4F-B98B-20A29892D03B" );
            // Remove Block: Step Type Detail, from Page: Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B572C7D7-3989-4BEA-AC63-3447B5CF7ED8" );
            // Remove Block: Step Type List, from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B7DFAB79-858E-4D44-BD74-38B273BA1EBB" );
            // Remove Block: Step Program Detail, from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93" );
            // Remove Block: Step Program List, from Page: Steps, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6AD9C580-387D-413F-9791-EB0DF4D382FC" );
            RockMigrationHelper.DeleteBlockType( "3EFB4302-9AB4-420F-A818-48B1B06AD109" ); // Step Type List
            RockMigrationHelper.DeleteBlockType( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD" ); // Step Type Detail
            RockMigrationHelper.DeleteBlockType( "429A817E-1379-4BCC-AEFE-01D9C75273E5" ); // Step Program List
            RockMigrationHelper.DeleteBlockType( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" ); // Step Program Detail
            RockMigrationHelper.DeleteBlockType( "2E4A1578-145E-4052-9B56-1739F7366827" ); // Step Participant List
            RockMigrationHelper.DeleteBlockType( "8D78BC55-6E67-40AB-B453-994D69503838" ); // Step Entry
            RockMigrationHelper.DeleteBlockType( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" ); // Steps
            RockMigrationHelper.DeletePage( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28" ); //  Page: Step, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "2109228C-D828-4B58-9310-8D93D10B846E" ); //  Page: Step, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232" ); //  Page: Steps, Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.DeletePage( "8E78F9DC-657D-41BF-BE0F-56916B6BF92F" ); //  Page: Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5" ); //  Page: Program, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F5E8A369-4856-42E5-B187-276DFCEB1F3F" ); //  Page: Steps, Layout: Full Width, Site: Rock RMS
            // Delete PageContext for Page:Steps, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "A14075A7-8A09-424D-AC71-D81535194EB7" );
        }

        private void TagUp()
        {
            AddColumn( "dbo.Tag", "IconCssClass", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.Tag", "BackgroundColor", c => c.String( maxLength: 100 ) );
            Sql( @"UPDATE
                    [dbo].[Tag]
                  SET
                    [BackgroundColor]  = '#bababa'
                  WHERE
                    [OwnerPersonAliasId] IS NULL
                  UPDATE
                    [dbo].[Tag]
                  SET
                    [BackgroundColor]  = '#e0e0e0'
                  WHERE
                    [OwnerPersonAliasId] IS NOT NULL" );
        }

        private void TagDown()
        {
            DropColumn( "dbo.Tag", "BackgroundColor" );
            DropColumn( "dbo.Tag", "IconCssClass" );
        }
    }
}