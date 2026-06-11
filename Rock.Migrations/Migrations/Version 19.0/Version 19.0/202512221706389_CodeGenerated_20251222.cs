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
    public partial class CodeGenerated_20251222 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C4FAFAE-41D1-4FF2-B6DC-FF99CD4DABBE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D7B93E80-8C72-44A4-BDDC-956C47D2171B" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C4FAFAE-41D1-4FF2-B6DC-FF99CD4DABBE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "72FBC523-D3A8-4DDB-8D09-6FCFB9EC615B" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics", "ShowDemographics", "Show Demographics", @"When enabled the Demographics panel will be displayed.", 0, @"true", "622600CF-D020-4570-A051-983466C33C7B" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Connection Status", "ShowConnectionStatus", "Show Demographics > Connection Status", @"When enabled the Connection Status chart will be displayed on the Demographics panel.", 1, @"true", "CF0A2008-91B2-4057-87E6-2F84F7D24342" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Age", "ShowAge", "Show Demographics > Age", @"When enabled the Age chart will be displayed on the Demographics panel.", 2, @"true", "82E3D217-9A85-4443-A004-6034E6B78CED" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Marital Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Marital Status", "ShowMaritalStatus", "Show Demographics > Marital Status", @"When enabled the Marital Status chart will be displayed on the Demographics panel.", 3, @"true", "9E05920A-0720-48F3-9E33-C2E8E1B1901E" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Gender", "ShowGender", "Show Demographics > Gender", @"When enabled the Gender chart will be displayed on the Demographics panel.", 4, @"true", "EEF4B759-8E1C-4546-8F45-C1D3D08197AF" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics", "ShowInformationStatistics", "Show Information Statistics", @"When enabled the Information Statistics panel will be displayed.", 7, @"true", "228F1561-F821-4662-9555-3B96F6C2F2BC" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Information Completeness
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > Information Completeness", "ShowInformationCompleteness", "Show Information Statistics > Information Completeness", @"When enabled the Information Completeness chart will be displayed on the Information Statistics panel.", 8, @"true", "1215D4D0-5A92-43F8-8848-4249564DADD0" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > % of Active Individuals with Assessments
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > % of Active Individuals with Assessments", "ShowPercentageOfActiveIndividualsWithAssessments", "Show Information Statistics > % of Active Individuals with Assessments", @"When enabled the % of Active Individuals with Assessments chart will be displayed on the Information Statistics panel.", 9, @"true", "EA250D2E-E0A0-42C5-8F5B-6A4B7067A07D" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Record Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B215F5FA-410C-4674-8C47-43DC40AF9F67", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > Record Statuses", "ShowRecordStatuses", "Show Information Statistics > Record Statuses", @"When enabled the Record Statuses chart will be displayed on the Information Statistics panel.", 10, @"true", "8EA9BB7E-45BB-4992-A2A0-4997E0218DA6" );

            // Attribute for BlockType
            //   BlockType: Page Search
            //   Category: CMS
            //   Attribute: Additional Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A279A88E-D4E0-4867-A108-2AA743B3CFD0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Content Template", "AdditionalContentTemplate", "Additional Content Template", @"Lava template to use to display additional content on the page. This will be displayed in the sidebar below the section list.", 0, @"", "CBDC5F56-987D-4458-9688-79CE25DAC490" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "72FBC523-D3A8-4DDB-8D09-6FCFB9EC615B" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "D7B93E80-8C72-44A4-BDDC-956C47D2171B" );

            // Attribute for BlockType
            //   BlockType: Page Search
            //   Category: CMS
            //   Attribute: Additional Content Template
            RockMigrationHelper.DeleteAttribute( "CBDC5F56-987D-4458-9688-79CE25DAC490" );
        }
    }
}
