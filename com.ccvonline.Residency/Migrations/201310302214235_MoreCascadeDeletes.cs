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
    public partial class MoreCascadeDeletes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo._com_ccvonline_Residency_Project", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId", "dbo._com_ccvonline_Residency_CompetencyPersonProject");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId", "dbo._com_ccvonline_Residency_ProjectPointOfAssessment");
            DropIndex("dbo._com_ccvonline_Residency_Project", new[] { "CompetencyId" });
            DropIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPerson", new[] { "CompetencyId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", new[] { "CompetencyPersonProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", new[] { "ProjectPointOfAssessmentId" });
            CreateIndex("dbo._com_ccvonline_Residency_Project", "CompetencyId");
            CreateIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId");
            AddForeignKey("dbo._com_ccvonline_Residency_Project", "CompetencyId", "dbo._com_ccvonline_Residency_Competency", "Id", cascadeDelete: true);
            AddForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId", "dbo._com_ccvonline_Residency_Project", "Id", cascadeDelete: true);
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId", "dbo._com_ccvonline_Residency_Competency", "Id", cascadeDelete: true);
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId", "dbo._com_ccvonline_Residency_Project", "Id", cascadeDelete: true);
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId", "dbo._com_ccvonline_Residency_CompetencyPersonProject", "Id", cascadeDelete: true);
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId", "dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "Id", cascadeDelete: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId", "dbo._com_ccvonline_Residency_ProjectPointOfAssessment");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId", "dbo._com_ccvonline_Residency_CompetencyPersonProject");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId", "dbo._com_ccvonline_Residency_Project");
            DropForeignKey("dbo._com_ccvonline_Residency_Project", "CompetencyId", "dbo._com_ccvonline_Residency_Competency");
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", new[] { "ProjectPointOfAssessmentId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", new[] { "CompetencyPersonProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_CompetencyPerson", new[] { "CompetencyId" });
            DropIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", new[] { "ProjectId" });
            DropIndex("dbo._com_ccvonline_Residency_Project", new[] { "CompetencyId" });
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId");
            CreateIndex("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId");
            CreateIndex("dbo._com_ccvonline_Residency_Project", "CompetencyId");
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment", "ProjectPointOfAssessmentId", "dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "Id");
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment", "CompetencyPersonProjectId", "dbo._com_ccvonline_Residency_CompetencyPersonProject", "Id");
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPersonProject", "ProjectId", "dbo._com_ccvonline_Residency_Project", "Id");
            AddForeignKey("dbo._com_ccvonline_Residency_CompetencyPerson", "CompetencyId", "dbo._com_ccvonline_Residency_Competency", "Id");
            AddForeignKey("dbo._com_ccvonline_Residency_ProjectPointOfAssessment", "ProjectId", "dbo._com_ccvonline_Residency_Project", "Id");
            AddForeignKey("dbo._com_ccvonline_Residency_Project", "CompetencyId", "dbo._com_ccvonline_Residency_Competency", "Id");
        }
    }
}
