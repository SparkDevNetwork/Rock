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
    public partial class CodeGenerated_20240314 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Sign-Ups Finder
            //   Category: Engagement > Sign-Up
            //   Attribute: Hide Campuses with no Sign-Up Opportunities
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Campuses with no Sign-Up Opportunities", "HideCampusesWithNoOpportunities", "Hide Campuses with no Sign-Up Opportunities", @"Determines if campuses should be excluded from the filter list if they don't have any sign-up opportunities. This setting will be ignored if ""Display Campus Filter"" is disabled.", 0, @"False", "68D5F2DB-5D92-460C-9CC5-C5D56214B5DF" );

            // Attribute for BlockType
            //   BlockType: Sign-Ups Finder
            //   Category: Engagement > Sign-Up
            //   Attribute: Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Campuses", "Campuses", "Campuses", @"The specific campuses to include in the filter list if ""Display Campus Filter"" is enabled. If the filter is not displayed, these are the campuses the results should be automatically filtered by.", 0, @"", "80DBAFD6-AA3F-4BB0-B046-5F2EF935E626" );

            // Add Block Attribute Value
            //   Block: Login
            //   BlockType: Login
            //   Category: Security
            //   Block Location: Page=Log In, Site=External Website
            //   Attribute: Remote Authorization Prompt Message
            /*   Attribute Value: Log in with social account */
            RockMigrationHelper.AddBlockAttributeValue( "B7A76E19-3A40-4B03-9769-CE041832EB0C", "2DE2A188-6541-4C6F-B21E-9564E75F45FE", "" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Sign-Ups Finder
            //   Category: Engagement > Sign-Up
            //   Attribute: Campuses
            RockMigrationHelper.DeleteAttribute( "80DBAFD6-AA3F-4BB0-B046-5F2EF935E626" );

            // Attribute for BlockType
            //   BlockType: Sign-Ups Finder
            //   Category: Engagement > Sign-Up
            //   Attribute: Hide Campuses with no Sign-Up Opportunities
            RockMigrationHelper.DeleteAttribute( "68D5F2DB-5D92-460C-9CC5-C5D56214B5DF" );
        }
    }
}
