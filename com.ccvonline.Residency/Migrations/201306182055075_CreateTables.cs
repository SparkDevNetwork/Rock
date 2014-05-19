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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CreateTables : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo._com_ccvonline_Residency_Competency",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TrackId = c.Int(nullable: false),
                        TeacherOfRecordPersonId = c.Int(),
                        FacilitatorPersonId = c.Int(),
                        Goals = c.String(),
                        CreditHours = c.Int(),
                        SupervisionHours = c.Int(),
                        ImplementationHours = c.Int(),
                        CompetencyTypeValueId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_Track", t => t.TrackId)
                .ForeignKey("dbo.Person", t => t.TeacherOfRecordPersonId)
                .ForeignKey("dbo.Person", t => t.FacilitatorPersonId)
                .ForeignKey("dbo.DefinedValue", t => t.CompetencyTypeValueId)
                .Index(t => t.TrackId)
                .Index(t => t.TeacherOfRecordPersonId)
                .Index(t => t.FacilitatorPersonId)
                .Index(t => t.CompetencyTypeValueId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_Competency", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_Track",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_Period", t => t.PeriodId)
                .Index(t => t.PeriodId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_Track", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_Period",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo._com_ccvonline_Residency_Period", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_Project",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyId = c.Int(nullable: false),
                        MinAssignmentCountDefault = c.Int(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_Competency", t => t.CompetencyId)
                .Index(t => t.CompetencyId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_Project", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_ProjectPointOfAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(nullable: false),
                        AssessmentOrder = c.Int(nullable: false),
                        AssessmentText = c.String(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_Project", t => t.ProjectId)
                .Index(t => t.ProjectId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_CompetencyPerson",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyId = c.Int(nullable: false),
                        PersonId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_Competency", t => t.CompetencyId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.CompetencyId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_CompetencyPerson", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_CompetencyPersonProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyPersonId = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                        MinAssignmentCount = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", t => t.CompetencyPersonId)
                .ForeignKey("dbo._com_ccvonline_Residency_Project", t => t.ProjectId)
                .Index(t => t.CompetencyPersonId)
                .Index(t => t.ProjectId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_CompetencyPersonProject", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyPersonProjectId = c.Int(nullable: false),
                        AssessorPersonId = c.Int(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", t => t.CompetencyPersonProjectId)
                .ForeignKey("dbo.Person", t => t.AssessorPersonId)
                .Index(t => t.CompetencyPersonProjectId)
                .Index(t => t.AssessorPersonId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyPersonProjectAssignmentId = c.Int(nullable: false),
                        AssessmentDateTime = c.DateTime(),
                        Rating = c.Int(),
                        RatingNotes = c.String(),
                        ResidentComments = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", t => t.CompetencyPersonProjectAssignmentId)
                .Index(t => t.CompetencyPersonProjectAssignmentId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "Guid", true );
            CreateTable(
                "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompetencyPersonProjectAssignmentAssessmentId = c.Int(nullable: false),
                        ProjectPointOfAssessmentId = c.Int(nullable: false),
                        Rating = c.Int(),
                        RatingNotes = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", t => t.CompetencyPersonProjectAssignmentAssessmentId)
                .ForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", t => t.ProjectPointOfAssessmentId)
                .Index(t => t.CompetencyPersonProjectAssignmentAssessmentId)
                .Index(t => t.ProjectPointOfAssessmentId);
            
            CreateIndex( "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment", "Guid", true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment", "ProjectPointOfAssessmentId", "dbo._com_ccvonline_Residency_ProjectPointOfAssessment");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment", "CompetencyPersonProjectAssignmentAssessmentId", "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "CompetencyPersonProjectAssignmentId", "dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", "AssessorPersonId", "dbo.Person");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", "CompetencyPersonProjectId", "dbo._com_ccvonline_Residency_CompetencyPersonProject");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "CompetencyPersonId", "dbo._com_ccvonline_Residency_CompetencyPerson");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "PersonId", "dbo.Person");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropForeignKey("dbo._com_ccvonline_Residency_Competency", "CompetencyTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_Project", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropForeignKey("dbo._com_ccvonline_Residency_Competency", "FacilitatorPersonId", "dbo.Person");
            DropForeignKey("dbo._com_ccvonline_Residency_Competency", "TeacherOfRecordPersonId", "dbo.Person");
            DropForeignKey("dbo._com_ccvonline_Residency_Competency", "TrackId", "dbo._com_ccvonline_Residency_Track");
            DropForeignKey("dbo._com_ccvonline_Residency_Track", "PeriodId", "dbo._com_ccvonline_Residency_Period");
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment", new[] { "ProjectPointOfAssessmentId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment", new[] { "CompetencyPersonProjectAssignmentAssessmentId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", new[] { "CompetencyPersonProjectAssignmentId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", new[] { "AssessorPersonId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment", new[] { "CompetencyPersonProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", new[] { "CompetencyPersonId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPerson", new[] { "PersonId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPerson", new[] { "CompetencyId" });
            DropIndex("dbo._com_ccvonline_Residency_Competency", new[] { "CompetencyTypeValueId" });
            DropIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_Project", new[] { "CompetencyId" });
            DropIndex("dbo._com_ccvonline_Residency_Competency", new[] { "FacilitatorPersonId" });
            DropIndex("dbo._com_ccvonline_Residency_Competency", new[] { "TeacherOfRecordPersonId" });
            DropIndex("dbo._com_ccvonline_Residency_Competency", new[] { "TrackId" });
            DropIndex("dbo._com_ccvonline_Residency_Track", new[] { "PeriodId" });
            DropTable("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessmentPointOfAssessment");
            DropTable("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment");
            DropTable("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignment");
            DropTable("dbo._com_ccvonline_Residency_CompetencyPersonProject");
            DropTable("dbo._com_ccvonline_Residency_CompetencyPerson");
            DropTable("dbo._com_ccvonline_Residency_ProjectPointOfAssessment");
            DropTable("dbo._com_ccvonline_Residency_Project");
            DropTable("dbo._com_ccvonline_Residency_Period");
            DropTable("dbo._com_ccvonline_Residency_Track");
            DropTable("dbo._com_ccvonline_Residency_Competency");
        }
    }
}

