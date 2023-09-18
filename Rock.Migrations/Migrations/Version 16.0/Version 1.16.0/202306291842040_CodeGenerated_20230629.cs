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
    public partial class CodeGenerated_20230629 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type              
            //   EntityType:Rock.Blocks.Reporting.ServiceMetricsEntry              
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.ServiceMetricsEntry", "Service Metrics Entry", "Rock.Blocks.Reporting.ServiceMetricsEntry, Rock.Blocks, Version=1.16.0.6, Culture=neutral, PublicKeyToken=null", false, false, "46199E9D-59CC-4CBC-BC05-83F6FF193147" );

            // Add/Update Obsidian Block Type              
            //   Name:Service Metrics Entry              
            //   Category:Reporting              
            //   EntityType:Rock.Blocks.Reporting.ServiceMetricsEntry              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Service Metrics Entry", "Block for easily adding/editing metric values for any metric that has partitions of campus and service time.", "Rock.Blocks.Reporting.ServiceMetricsEntry", "Reporting", "E6144C7A-2E95-431B-AB75-C588D151ACA4" );

            // Add Block               
            //  Block Name: Membership              
            //  Page Name: Extended Attributes V1              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "DC6AC8B3-00E3-457B-8996-4E395E80801E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "61661506-0DB1-4300-8078-D58DBD8408C6" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Insert 0 for Blank Items              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Insert 0 for Blank Items", "DefaultToZero", "Insert 0 for Blank Items", @"If enabled, a zero will be added to any metrics that are left empty when entering data. This will override ""Remove Values When Deleted"".", 5, @"False", "7086E2B8-EBE5-4E1D-9144-8D87816DDFC1" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Remove Values When Deleted              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove Values When Deleted", "RemoveValuesWhenDeleted", "Remove Values When Deleted", @"If enabled, metrics that are left empty will be deleted. This will have no effect when ""Insert 0 for Blank Items"" is enabled.", 6, @"False", "DB4D9EFC-BCC8-467E-B203-89353801B797" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Limit Campus Selection to Campus Team Membership              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Campus Selection to Campus Team Membership", "LimitCampusByCampusTeam", "Limit Campus Selection to Campus Team Membership", @"If enabled, this would limit the campuses shown to only those where the individual was on the Campus Team.", 8, @"False", "4DDB4F13-F868-4526-9102-31B55A79E416" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Filter Schedules by Campus              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Schedules by Campus", "FilterByCampus", "Filter Schedules by Campus", @"When enabled, only schedules that are included in the Campus Schedules will be included.", 11, @"False", "D0675C42-E7DB-411A-9310-34086A929D52" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Show Metric Category Subtotals              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Metric Category Subtotals", "ShowMetricCategorySubtotals", "Show Metric Category Subtotals", @"When enabled, shows the metric category subtotals.", 12, @"False", "C2418BBF-CE14-4F71-87D4-19B28203926B" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Include Duplicate Metrics in Category Subtotals              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Duplicate Metrics in Category Subtotals", "IncludeDuplicateMetricsInCategorySubtotals", "Include Duplicate Metrics in Category Subtotals", @"When enabled, category subtotals will include the same metric multiple times if that metric is in multiple subcategories.", 14, @"True", "6282EE8B-0462-49A5-A773-66751DFDAE42" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "97C712FC-D005-4F8E-87DE-64BFB499FEF1" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Metric Date Determined By              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Metric Date Determined By", "MetricDateDeterminedBy", "Metric Date Determined By", @"This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.", 7, @"0", "C8ADFE88-D369-4600-8341-96130A901567" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Weeks Back              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Weeks Back", "WeeksBack", "Weeks Back", @"The number of weeks back to display in the 'Week of' selection.", 1, @"8", "1B8C6391-3E3A-4D66-AC48-91FF187FFA10" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Weeks Ahead              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Weeks Ahead", "WeeksAhead", "Weeks Ahead", @"The number of weeks ahead to display in the 'Week of' selection.", 2, @"0", "D776BDCB-E6DE-4595-8C5B-5AE58BEC237B" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Roll-up Category Depth              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Roll-up Category Depth", "RollupCategoryDepth", "Roll-up Category Depth", @"Determines how many levels of parent categories to show. (1 = parent, 2 = grandparent, etc.)", 13, @"0", "78C2DD99-F7E4-4D7C-95FA-BA238C47AE47" );

            // Attribute for BlockType              
            //   BlockType: SMS Conversation List              
            //   Category: Mobile > Communication              
            //   Attribute: Person Search Stopped Typing Behavior Threshold              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E16DC868-101F-4944-BE6C-29D858D9821D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Search Stopped Typing Behavior Threshold", "StoppedTypingBehaviorThreshold", "Person Search Stopped Typing Behavior Threshold", @"Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.", 7, @"200", "44B566F0-FE23-4810-B1A7-406CBB0C92AE" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campus Types              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Note: setting this can override the selected Campuses block setting.", 9, @"", "42857BA5-262F-4833-A33D-69BD8D4A1633" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campus Status              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Status", "CampusStatus", "Campus Status", @"Note: setting this can override the selected Campuses block setting.", 10, @"", "2C7DE3AC-F1B6-4427-A549-442910900105" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campuses              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"Select the campuses you want to limit this block to.", 4, @"", "59B18417-A8B0-4706-8159-C8F801C01AF1" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Schedule Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Schedule Category", "ScheduleCategory", "Schedule Category", @"The schedule category to use for list of service times.", 0, @"", "9CCE7A4E-695D-42C8-BE98-D042117A8885" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Metric Categories              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6144C7A-2E95-431B-AB75-C588D151ACA4", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metric Categories", "MetricCategories", "Metric Categories", @"Select the metric categories to display (note: only metrics in those categories with a campus and schedule partition will displayed).", 3, @"", "A77ABDC5-D6A8-4426-AE17-C570E7992E6E" );

            // Add Block Attribute Value              
            //   Block: Membership              
            //   BlockType: Attribute Values              
            //   Category: CRM > Person Detail              
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS              
            //   Attribute: Category              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */              
            RockMigrationHelper.AddBlockAttributeValue( "61661506-0DB1-4300-8078-D58DBD8408C6", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "97C712FC-D005-4F8E-87DE-64BFB499FEF1" );

            // Attribute for BlockType              
            //   BlockType: SMS Conversation List              
            //   Category: Mobile > Communication              
            //   Attribute: Person Search Stopped Typing Behavior Threshold              
            RockMigrationHelper.DeleteAttribute( "44B566F0-FE23-4810-B1A7-406CBB0C92AE" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Include Duplicate Metrics in Category Subtotals              
            RockMigrationHelper.DeleteAttribute( "6282EE8B-0462-49A5-A773-66751DFDAE42" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Roll-up Category Depth              
            RockMigrationHelper.DeleteAttribute( "78C2DD99-F7E4-4D7C-95FA-BA238C47AE47" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Show Metric Category Subtotals              
            RockMigrationHelper.DeleteAttribute( "C2418BBF-CE14-4F71-87D4-19B28203926B" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Filter Schedules by Campus              
            RockMigrationHelper.DeleteAttribute( "D0675C42-E7DB-411A-9310-34086A929D52" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campus Status              
            RockMigrationHelper.DeleteAttribute( "2C7DE3AC-F1B6-4427-A549-442910900105" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campus Types              
            RockMigrationHelper.DeleteAttribute( "42857BA5-262F-4833-A33D-69BD8D4A1633" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Limit Campus Selection to Campus Team Membership              
            RockMigrationHelper.DeleteAttribute( "4DDB4F13-F868-4526-9102-31B55A79E416" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Metric Date Determined By              
            RockMigrationHelper.DeleteAttribute( "C8ADFE88-D369-4600-8341-96130A901567" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Remove Values When Deleted              
            RockMigrationHelper.DeleteAttribute( "DB4D9EFC-BCC8-467E-B203-89353801B797" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Insert 0 for Blank Items              
            RockMigrationHelper.DeleteAttribute( "7086E2B8-EBE5-4E1D-9144-8D87816DDFC1" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Campuses              
            RockMigrationHelper.DeleteAttribute( "59B18417-A8B0-4706-8159-C8F801C01AF1" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Metric Categories              
            RockMigrationHelper.DeleteAttribute( "A77ABDC5-D6A8-4426-AE17-C570E7992E6E" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Weeks Ahead              
            RockMigrationHelper.DeleteAttribute( "D776BDCB-E6DE-4595-8C5B-5AE58BEC237B" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Weeks Back              
            RockMigrationHelper.DeleteAttribute( "1B8C6391-3E3A-4D66-AC48-91FF187FFA10" );

            // Attribute for BlockType              
            //   BlockType: Service Metrics Entry              
            //   Category: Reporting              
            //   Attribute: Schedule Category              
            RockMigrationHelper.DeleteAttribute( "9CCE7A4E-695D-42C8-BE98-D042117A8885" );

            // Remove Block              
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS              
            //  from Page: Extended Attributes V1, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "61661506-0DB1-4300-8078-D58DBD8408C6" );

            // Delete BlockType               
            //   Name: Service Metrics Entry              
            //   Category: Reporting              
            //   Path: -              
            //   EntityType: Service Metrics Entry              
            RockMigrationHelper.DeleteBlockType( "E6144C7A-2E95-431B-AB75-C588D151ACA4" );
        }
    }
}
