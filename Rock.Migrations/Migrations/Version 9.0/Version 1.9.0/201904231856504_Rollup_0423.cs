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
    public partial class Rollup_0423 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixRockInstanceIDs();
            Post90DataMigrations();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Transaction Matching:Prompt to Edit Payment Detail Attributes
            RockMigrationHelper.UpdateBlockTypeAttribute("1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Prompt to Edit Payment Detail Attributes","DisplayPaymentDetailAttributeControls","",@"If Transaction Payment Detail has attributes configured, this will prompt to edit the values for those.",6,@"False","8542FD63-18D6-4DE2-BB42-D1A87C4B3BCF");
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable Credit Card
            RockMigrationHelper.UpdateBlockTypeAttribute("F1ADF375-7442-4B30-BAC3-C387EA9B6C18","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Credit Card","EnableCreditCard","",@"",2,@"False","144BC046-13DE-4B14-98BF-8B5520B3F977");
            // Attrib for BlockType: Transaction Entry (V2):Enable Credit Card
            RockMigrationHelper.UpdateBlockTypeAttribute("6316D801-40C0-4EED-A2AD-55C13870664D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Credit Card","EnableCreditCard","",@"",2,@"False","D3E95A83-4A84-4260-95BB-0458EDE78298");

        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Transaction Matching:Prompt to Edit Payment Detail Attributes
            RockMigrationHelper.DeleteAttribute("8542FD63-18D6-4DE2-BB42-D1A87C4B3BCF");
            // Attrib for BlockType: Transaction Entry (V2):Enable Credit Card
            RockMigrationHelper.DeleteAttribute("D3E95A83-4A84-4260-95BB-0458EDE78298");
            // Attrib for BlockType: Scheduled Transaction Edit (V2):Enable Credit Card
            RockMigrationHelper.DeleteAttribute("144BC046-13DE-4B14-98BF-8B5520B3F977");
        }

        /// <summary>
        /// ED: Fix Rock Instance IDs F8ABA66C-C73D-441F-9A28-233D803EFB0D
        /// </summary>
        private void FixRockInstanceIDs()
        {
            Sql( @"
                -- Update to a unique ID
                UPDATE [dbo].[Attribute]
                SET [Guid] = NEWID()
                WHERE [EntityTypeQualifierColumn] = 'systemsetting'
                 AND [Key] = 'RockInstanceId'
                 AND [Guid] = 'F8ABA66C-C73D-441F-9A28-233D803EFB0D'" );
        }

        /// <summary>
        /// MP: Post90DataMigrations
        /// </summary>
        private void Post90DataMigrations()
        {
            Sql( MigrationSQL._201904231856504_Rollup_0423_CreatePost90DataMigrationServiceJob );
        }
    }
}
