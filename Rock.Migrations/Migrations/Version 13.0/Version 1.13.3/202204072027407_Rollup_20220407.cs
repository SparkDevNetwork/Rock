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
    public partial class Rollup_20220407 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateFeeCoverageMessageDefaultValue();
            UpdatespCrm_FamilyAnalyticsEraDataset_AddIsEraAttributes();
            RemoveBackgroundCheckResultDuplicateAuth();
            FixDuplicateBlockTypes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// KA: Migration for updating FeeCoverageMessage DefaultValue
        /// </summary>
        private void UpdateFeeCoverageMessageDefaultValue()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Fee Coverage Message", "FeeCoverageMessage", "Fee Coverage Message", @"The Lava template to use to provide the cover the fees prompt to the individual. <span class='tip tip-lava'></span>", 28, @"Make my gift go further. Please increase my gift by {{ Percentage }}% ({{ AmountHTML }}) to help cover the electronic transaction fees.", "6A757838-D924-4966-BF58-6542FC78FBA2" );
        }

        /// <summary>
        /// KA: Migration for updating spCrm_FamilyAnalyticsEraDataset to add IsEra Attributes
        /// </summary>
        private void UpdatespCrm_FamilyAnalyticsEraDataset_AddIsEraAttributes()
        {
            Sql( MigrationSQL._202204072027407_Rollup_20220407_spCrm_FamilyAnalyticsEraDataset );
        }

        /// <summary>
        /// SK: Removed Background Check Result Duplicate Auth
        /// </summary>
        private void RemoveBackgroundCheckResultDuplicateAuth()
        {
            RockMigrationHelper.DeleteSecurityAuth( "397CEE3C-CE72-41EF-B2BC-F0031A6A29E4" );
            RockMigrationHelper.DeleteSecurityAuth( "878F5776-1498-4E61-B2AA-4B2FE7A6EE1D" );
        }

        /// <summary>
        /// ED: Fix duplicate BlockTypes for ConnectionRequestBoard & ConnectionOpportunitySelect
        /// </summary>
        private void FixDuplicateBlockTypes()
        {
            Sql( MigrationSQL._202204072027407_Rollup_20220407_FixDuplicateConnectionOpportunitySelectBlock );
            Sql( MigrationSQL._202204072027407_Rollup_20220407_FixDuplicateConnectionRequestBoardBlock );
        }
    }
}
