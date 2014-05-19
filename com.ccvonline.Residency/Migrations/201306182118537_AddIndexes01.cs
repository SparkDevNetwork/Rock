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
    public partial class AddIndexes01 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "_com_ccvonline_Residency_CompetencyPerson", new string[] { "CompetencyId", "PersonId" }, true );
            CreateIndex( "_com_ccvonline_Residency_CompetencyPersonProject", new string[] { "CompetencyPersonId", "ProjectId" }, true );
            CreateIndex( "_com_ccvonline_Residency_ProjectPointOfAssessment", new string[] { "ProjectId", "AssessmentOrder" }, true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "_com_ccvonline_Residency_CompetencyPerson", new string[] { "CompetencyId", "PersonId" } );
            DropIndex( "_com_ccvonline_Residency_CompetencyPersonProject", new string[] { "CompetencyPersonId", "ProjectId" } );
            DropIndex( "_com_ccvonline_Residency_ProjectPointOfAssessment", new string[] { "ProjectId", "AssessmentOrder" } );
        }
    }
}
