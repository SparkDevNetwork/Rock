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
    public partial class AssessmentOverallRatingColumn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "OverallRating", c => c.Decimal(precision: 2, scale: 1));
            DropColumn("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "Rating");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "Rating", c => c.Int());
            DropColumn("dbo._com_ccvonline_Residency_CompetencyPersonProjectAssignmentAssessment", "OverallRating");
        }
    }
}
/* Skipped Operations for tables that are not part of ResidencyContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<ResidencyContext>(false); */

// Up()...
// DropForeignKeyOperation for TableName PersonAccount, column PersonId.
// DropIndexOperation for TableName PersonAccount, column PersonId.
// DropTableOperation for TableName PersonAccount.

// Down()...
// CreateTableOperation for TableName PersonAccount.
// CreateIndexOperation for TableName PersonAccount, column PersonId.
// AddForeignKeyOperation for TableName PersonAccount, column PersonId.
