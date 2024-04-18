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
    public partial class CodeGenerated_20240411 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the namespace for the Mobile.Core.SmartSearch so the UpdateEntityType will correctly find it.
            Sql( "UPDATE [dbo].[EntityType] SET [Name] = 'Rock.Blocks.Types.Mobile.Core.SmartSearch' where [Guid] = '45BE4816-3F5B-4AD1-BA89-819325D7E8CF'" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Core.SmartSearch
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Core.SmartSearch", "Smart Search", "Rock.Blocks.Types.Mobile.Core.SmartSearch, Rock, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "45BE4816-3F5B-4AD1-BA89-819325D7E8CF" );

            // Add/Update Mobile Block Type
            //   Name:Smart Search
            //   Category:Mobile > Core
            //   EntityType:Rock.Blocks.Types.Mobile.Core.SmartSearch
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Smart Search", "Performs a search using the configured search components and displays the results.", "Rock.Blocks.Types.Mobile.Core.SmartSearch", "Mobile > Core", "9AA64485-9641-4A06-9450-B5244BC1464A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageRouteList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageRouteList", "Page Route List", "Rock.Blocks.Cms.PageRouteList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "ADF87B5B-9A9D-464E-8A2F-110E80B3D8BB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AuditList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AuditList", "Audit List", "Rock.Blocks.Core.AuditList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "8D4A9E56-30F1-4A2D-BD00-7803D7D51909" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DefinedValueList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DefinedValueList", "Defined Value List", "Rock.Blocks.Core.DefinedValueList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "710916BD-4BC1-4D05-B088-381394351B53" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.RestControllerList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.RestControllerList", "Rest Controller List", "Rock.Blocks.Core.RestControllerList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "98008055-F00F-4F6C-BA1D-2414D6DFF7AA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduledJobList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduledJobList", "Scheduled Job List", "Rock.Blocks.Core.ScheduledJobList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "D72E22CA-040D-4DE9-B2E0-438BA70BA91A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PersonMergeRequestList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PersonMergeRequestList", "Person Merge Request List", "Rock.Blocks.Crm.PersonMergeRequestList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "9C1A70F8-3177-49C9-97C6-AC3E52FC36B1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.AchievementTypeList", "Achievement Type List", "Rock.Blocks.Engagement.AchievementTypeList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "E9E67424-1FD8-4A85-9E7B-C919117BDE1A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.ConnectionTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.ConnectionTypeList", "Connection Type List", "Rock.Blocks.Engagement.ConnectionTypeList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "7D78F300-3DF7-4ED7-BC2B-813D4F866220" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BusinessContactList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BusinessContactList", "Business Contact List", "Rock.Blocks.Finance.BusinessContactList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "49EF69C9-B893-4684-BE71-8D8BC8905B06" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.SavedAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.SavedAccountList", "Saved Account List", "Rock.Blocks.Finance.SavedAccountList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "AD9C4AAC-54BB-498D-9BD3-47D8F21B9549" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Fundraising.FundraisingDonationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Fundraising.FundraisingDonationList", "Fundraising Donation List", "Rock.Blocks.Fundraising.FundraisingDonationList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "B80410E7-53D7-4AB1-8B17-39FF8B3E708F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MergeTemplateList", "Merge Template List", "Rock.Blocks.Reporting.MergeTemplateList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "EDAAAF0C-BA30-40C9-8E7C-9D1118FEFD87" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.ReportList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.ReportList", "Report List", "Rock.Blocks.Reporting.ReportList, Rock.Blocks, Version=1.16.4.4, Culture=neutral, PublicKeyToken=null", false, false, "084E3594-E399-4639-8461-88333399CBA2" );

            // Add/Update Obsidian Block Type
            //   Name:Route List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PageRouteList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Route List", "Displays a list of page routes.", "Rock.Blocks.Cms.PageRouteList", "CMS", "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7" );

            // Add/Update Obsidian Block Type
            //   Name:Audit List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AuditList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Audit List", "Displays a list of audits.", "Rock.Blocks.Core.AuditList", "Core", "120552E2-5C36-4220-9A73-FBBBD75B0964" );

            // Add/Update Obsidian Block Type
            //   Name:Defined Value List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DefinedValueList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Defined Value List", "Block for viewing values for a defined type.", "Rock.Blocks.Core.DefinedValueList", "Core", "F431F950-F007-493E-81C8-16559FE4C0F0" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Controller List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.RestControllerList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Controller List", "Displays a list of rest controllers.", "Rock.Blocks.Core.RestControllerList", "Core", "A6D8BFD9-0C3D-4F1E-AE0D-325A9C70B4C8" );

            // Add/Update Obsidian Block Type
            //   Name:Scheduled Job List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduledJobList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job List", "Lists all scheduled jobs.", "Rock.Blocks.Core.ScheduledJobList", "Core", "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20" );

            // Add/Update Obsidian Block Type
            //   Name:Person Merge Request List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.PersonMergeRequestList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Merge Request List", "Lists Person Merge Requests", "Rock.Blocks.Crm.PersonMergeRequestList", "CRM", "B2CF80F1-5588-46D5-8198-8C5816290E98" );

            // Add/Update Obsidian Block Type
            //   Name:Achievement Type List
            //   Category:Streaks
            //   EntityType:Rock.Blocks.Engagement.AchievementTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Achievement Type List", "Shows a list of all achievement types.", "Rock.Blocks.Engagement.AchievementTypeList", "Streaks", "4ACFBF3F-3D49-4AE3-B468-529F79DA9898" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Type List
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.ConnectionTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Type List", "Displays a list of connection types.", "Rock.Blocks.Engagement.ConnectionTypeList", "Engagement", "45F30EA2-F93B-4A63-806F-7CD375DAACAB" );

            // Add/Update Obsidian Block Type
            //   Name:Business Contact List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BusinessContactList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Business Contact List", "Displays the list of contacts for a business.", "Rock.Blocks.Finance.BusinessContactList", "Finance", "5E72C18D-F459-4226-820B-B47F88EFEB0F" );

            // Add/Update Obsidian Block Type
            //   Name:Saved Account List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.SavedAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Saved Account List", "List of a person's saved accounts that can be used to delete an account.", "Rock.Blocks.Finance.SavedAccountList", "Finance", "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702" );

            // Add/Update Obsidian Block Type
            //   Name:Fundraising Donation List
            //   Category:Fundraising
            //   EntityType:Rock.Blocks.Fundraising.FundraisingDonationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Fundraising Donation List", "Lists donations in a grid for the current fundraising opportunity or participant.", "Rock.Blocks.Fundraising.FundraisingDonationList", "Fundraising", "054A8469-A838-4708-B18F-9F2819346298" );

            // Add/Update Obsidian Block Type
            //   Name:Merge Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Merge Template List", "Displays a list of all merge templates.", "Rock.Blocks.Reporting.MergeTemplateList", "Core", "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52" );

            // Add/Update Obsidian Block Type
            //   Name:Report List
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.ReportList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Report List", "Lists all reports under a specified report category.", "Rock.Blocks.Reporting.ReportList", "Reporting", "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the page route details.", 0, @"", "417BAE85-9AE4-4808-930C-C9842CEE13F1" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "3A96BEB8-6F57-49AF-AA4B-77126B916716" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "937B6777-6EC9-4852-AA44-73825A8D276B" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "120552E2-5C36-4220-9A73-FBBBD75B0964", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9DBEF79B-1B79-4C4B-B8CA-3963ED00BABF" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "120552E2-5C36-4220-9A73-FBBBD75B0964", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7FE27268-DAA5-403C-A2EB-90B53CF2B2D4" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: Defined Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F431F950-F007-493E-81C8-16559FE4C0F0", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "Defined Type", @"If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).", 0, @"", "DF88E1B7-FC9A-42DC-BB1F-2CAA4CCD6152" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F431F950-F007-493E-81C8-16559FE4C0F0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "1A9D3344-8665-4BA2-AF7E-B8FDF19E9CBD" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F431F950-F007-493E-81C8-16559FE4C0F0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A89BF02D-7ECB-47C5-A203-E3ACFF8B8D84" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D8BFD9-0C3D-4F1E-AE0D-325A9C70B4C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the rest controller details.", 0, @"", "49BBE14C-3794-4516-97E2-187576C7ED6F" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D8BFD9-0C3D-4F1E-AE0D-325A9C70B4C8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FEF857C1-8736-427D-94EA-F3F1844FA85D" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D8BFD9-0C3D-4F1E-AE0D-325A9C70B4C8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "757E9FC2-2A7C-4A58-B66D-E658CA957B88" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the service job details.", 0, @"", "DF2A7B20-54DA-44DA-A3BE-695CCCCAD6B6" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: History Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "History Page", "HistoryPage", "History Page", @"The page to display group history.", 0, @"", "B45EC31E-A1DF-41EE-B2C3-201A8A3C9BF9" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C6B47E32-CA90-4F7B-83D1-E163BE89AB02" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DA72C7D5-B0CC-4147-828A-5DF79F88D621" );

            // Attribute for BlockType
            //   BlockType: Person Merge Request List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2CF80F1-5588-46D5-8198-8C5816290E98", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DFDB84FD-A370-4161-9F00-629C5B0060B1" );

            // Attribute for BlockType
            //   BlockType: Person Merge Request List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2CF80F1-5588-46D5-8198-8C5816290E98", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A3D08902-0AA8-4717-96EC-FAEAC5102630" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the achievement type details.", 0, @"", "59F4B591-3921-4AFB-BF51-5E429B2A72D7" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8DF9D395-BFE8-4CFB-8340-B4AB516BC49D" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B998A4BF-9A2A-4486-972A-C497FB3DE022" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45F30EA2-F93B-4A63-806F-7CD375DAACAB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the connection type details.", 0, @"", "397EA71E-7D39-40FE-AD6C-28DAB1276C63" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45F30EA2-F93B-4A63-806F-7CD375DAACAB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F3A43658-C52A-4EF9-922D-7D2686A48A13" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45F30EA2-F93B-4A63-806F-7CD375DAACAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B88A62DD-E273-4E42-8690-DA5C573481C0" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E72C18D-F459-4226-820B-B47F88EFEB0F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"The page used to view the details of a business contact.", 0, @"", "0CDDB2DD-B992-49A2-94F2-010DFC5D8307" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E72C18D-F459-4226-820B-B47F88EFEB0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B6541846-7B6A-4A4A-8D19-90EB0B456391" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E72C18D-F459-4226-820B-B47F88EFEB0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E7493B52-9DE0-48E5-9EB5-66E398186910" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "94FFEDC7-188F-4E09-A41D-76A974D8BA37" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A5AF7CAE-B742-4F28-BF5D-B84139CC4E5D" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "AFB0BBFD-79A2-4CF8-8667-C2D913AAE681" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Hide Grid Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Grid Columns", "HideGridColumns", "Hide Grid Columns", @"The grid columns that should be hidden from the user.", 0, @"", "E3183EA4-137C-4F23-95B4-4E1E5ACB85F6" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Hide Grid Actions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Grid Actions", "HideGridActions", "Hide Grid Actions", @"The grid actions that should be hidden from the user.", 1, @"", "985D27BA-A4CC-4F60-8149-C244781A016A" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Donor Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Donor Column", "DonorColumn", "Donor Column", @"The value that should be displayed for the Donor column. <span class='tip tip-lava'></span>", 2, @"<a href=""/Person/{{ Donor.Id }}"">{{ Donor.FullName }}</a>", "566E657A-DB31-4FD5-B713-47890E832A2E" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Participant Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Participant Column", "ParticipantColumn", "Participant Column", @"The value that should be displayed for the Participant column. <span class='tip tip-lava'></span>", 3, @"<a href=""/Person/{{ Participant.PersonId }}"" class=""pull-right margin-l-sm btn btn-sm btn-default"">
    <i class=""fa fa-user""></i>
</a>
<a href=""/GroupMember/{{ Participant.Id }}"">{{ Participant.Person.FullName }}</a>", "18C7F5E0-3B25-41EB-A9F7-1474B46801E0" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "18A9566F-9F10-4561-97C9-A9DA452D773F" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "054A8469-A838-4708-B18F-9F2819346298", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7202527F-92EC-4545-9073-E58A2B345E7C" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the merge template details.", 0, @"", "71E56082-AABE-47AD-95A6-85A13218DA6D" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Merge Templates Ownership
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "Merge Templates Ownership", @"Set this to limit to merge templates depending on ownership type.", 0, @"1", "0845BD0A-8A74-49EA-95AD-84EE380A1F97" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FE1D374F-2FB2-49D2-B4A6-02DA844ACB85" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1E1BCA3D-0F6B-49E4-B698-0A8AFC899BD8" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the report details.", 0, @"", "1C2D47F1-5F7C-4FFD-9588-9EB87DC6E6E8" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: Report Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Report Category", "ReportCategory", "Report Category", @"Category to use to list reports for.", 0, @"89e54497-5e98-4f1b-b83a-95bfb685da91", "FB82A48F-CDBE-40B8-A592-5630EBBE215E" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5840A5DD-C4A2-44B1-8151-2AE1BA6D3EB5" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "596C5077-5F33-41A2-AA5E-6843915AF294" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Search Component(s)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "039E2E97-3682-4B29-8748-7132287A2059", "Search Component(s)", "SearchComponents", "Search Component(s)", @"The search components to offer for searches.", 0, @"", "85BF947D-9964-4322-B895-F34C7EB635E2" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Header Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Content", "HeaderContent", "Header Content", @"The content to display for the header.", 1, @"", "45FC9713-CE6A-45ED-878A-A4009AE44DBD" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Footer Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Footer Content", "FooterContent", "Footer Content", @"The content to display for the header.", 2, @"", "0C996FE1-94E9-42ED-976A-9D077F7BF2AB" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Result Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Result Size", "ResultSize", "Result Size", @"The amount of results to initially return and with each sequential load (as you scroll down).", 3, @"20", "ED2BA5A3-A7DB-4D94-9A9E-B8E8FC77C342" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Auto Focus Keyboard
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Auto Focus Keyboard", "AutoFocusKeyboard", "Auto Focus Keyboard", @"Determines if the keyboard should auto-focus into the search field when the page is attached.", 4, @"True", "C2C878F9-9640-4480-8561-F5CE0623B774" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Stopped Typing Behavior Threshold
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Stopped Typing Behavior Threshold", "StoppedTypingBehaviorThreshold", "Stopped Typing Behavior Threshold", @"Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.", 5, @"200", "8AD62CAB-1781-4A87-9CE6-7E9FAA191A31" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Birthdate
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birthdate", "ShowBirthdate", "Show Birthdate", @"Determines if the person's birthdate should be displayed in the search results.", 6, @"False", "223C8EEA-8FA7-4DA5-ABDD-A6BAA701C6F4" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "Show Age", @"Determines if the person's age should be displayed in the search results.", 11, @"True", "CCB51963-0CB6-47C1-B900-917F52B574BE" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Spouse
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Spouse", "ShowSpouse", "Show Spouse", @"Determines if the person's spouse should be displayed in the search results.", 8, @"True", "C7678386-5B0A-4707-BB56-021F11AECC8E" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Phone Number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Phone Number", "ShowPhoneNumber", "Show Phone Number", @"Determines if the person's phone number should be displayed in the search results.", 9, @"True", "05FD1EC3-3516-42D9-A92A-5015C29B827D" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Address", "ShowAddress", "Show Address", @"Determines if the person's address should be displayed in the search results.", 10, @"True", "71583D34-9877-465A-BFB1-11671EE061A1" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Person Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "Person Detail Page", @"Page to link to when a person taps on a Person search result. 'PersonGuid' is passed as the query string.", 12, @"", "1A9BD4D3-2802-4C4D-A205-3FB7EE8452D2" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Person Highlight Indicators
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "F739BF5D-3FDC-45EC-A03C-1AE7C47E3883", "Person Highlight Indicators", "PersonDataViewIcons", "Person Highlight Indicators", @"Select one or more Data Views for Person search result icons. Note: More selections increase processing time.", 13, @"", "650B2E12-9179-454D-AA53-41561B416EDF" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page to link to when a person taps on a Group search result. 'GroupGuid' is passed as the query string.", 14, @"", "105235EC-FD8C-48A7-9752-6A565A065571" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Group Highlight Indicators
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AA64485-9641-4A06-9450-B5244BC1464A", "F739BF5D-3FDC-45EC-A03C-1AE7C47E3883", "Group Highlight Indicators", "GroupDataViewIcons", "Group Highlight Indicators", @"Select one or more Data Views for Group search result icons. Note: More selections increase processing time.", 15, @"", "E7CAE99B-97E7-4D28-BB6F-915BF42152F5" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Group Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Header Template", "GroupHeaderTemplate", "Group Header Template", @"The lava template to use to render the group headers. This will display above each content collection source.", 0, @"<Grid StyleClass=""px-8, mt-8""
      MarginBottom=""-8""
      HorizontalOptions=""Center""
      ColumnDefinitions=""Auto, *"">
    
    <Rock:Icon IconClass=""{{ SourceEntity.IconCssClass }}""
               StyleClass=""mr-8, title2, text-primary-strong""
               Grid.Column=""0""
               VerticalOptions=""Center"" />

    <Label Text=""{{ SourceName | Escape }}""
           StyleClass=""title2, text-interface-stronger, bold""
           Grid.Column=""1"" />
</Grid>", "B09185B1-5533-42F4-B5A5-0863BB8AC661" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Include Unapproved
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Unapproved", "IncludeUnapproved", "Include Unapproved", @"If selected, all unapproved prayer requests will be included.", 6, @"False", "B50FF66E-423C-4BF9-8953-4DB381754748" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Include Unapproved
            RockMigrationHelper.DeleteAttribute( "B50FF66E-423C-4BF9-8953-4DB381754748" );

            // Attribute for BlockType
            //   BlockType: Content Collection View
            //   Category: CMS
            //   Attribute: Group Header Template
            RockMigrationHelper.DeleteAttribute( "B09185B1-5533-42F4-B5A5-0863BB8AC661" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Group Highlight Indicators
            RockMigrationHelper.DeleteAttribute( "E7CAE99B-97E7-4D28-BB6F-915BF42152F5" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "105235EC-FD8C-48A7-9752-6A565A065571" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Person Highlight Indicators
            RockMigrationHelper.DeleteAttribute( "650B2E12-9179-454D-AA53-41561B416EDF" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Person Detail Page
            RockMigrationHelper.DeleteAttribute( "1A9BD4D3-2802-4C4D-A205-3FB7EE8452D2" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Address
            RockMigrationHelper.DeleteAttribute( "71583D34-9877-465A-BFB1-11671EE061A1" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Phone Number
            RockMigrationHelper.DeleteAttribute( "05FD1EC3-3516-42D9-A92A-5015C29B827D" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Spouse
            RockMigrationHelper.DeleteAttribute( "C7678386-5B0A-4707-BB56-021F11AECC8E" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Age
            RockMigrationHelper.DeleteAttribute( "CCB51963-0CB6-47C1-B900-917F52B574BE" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Show Birthdate
            RockMigrationHelper.DeleteAttribute( "223C8EEA-8FA7-4DA5-ABDD-A6BAA701C6F4" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Stopped Typing Behavior Threshold
            RockMigrationHelper.DeleteAttribute( "8AD62CAB-1781-4A87-9CE6-7E9FAA191A31" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Auto Focus Keyboard
            RockMigrationHelper.DeleteAttribute( "C2C878F9-9640-4480-8561-F5CE0623B774" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Result Size
            RockMigrationHelper.DeleteAttribute( "ED2BA5A3-A7DB-4D94-9A9E-B8E8FC77C342" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Footer Content
            RockMigrationHelper.DeleteAttribute( "0C996FE1-94E9-42ED-976A-9D077F7BF2AB" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Header Content
            RockMigrationHelper.DeleteAttribute( "45FC9713-CE6A-45ED-878A-A4009AE44DBD" );

            // Attribute for BlockType
            //   BlockType: Smart Search
            //   Category: Mobile > Core
            //   Attribute: Search Component(s)
            RockMigrationHelper.DeleteAttribute( "85BF947D-9964-4322-B895-F34C7EB635E2" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "596C5077-5F33-41A2-AA5E-6843915AF294" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "5840A5DD-C4A2-44B1-8151-2AE1BA6D3EB5" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: Report Category
            RockMigrationHelper.DeleteAttribute( "FB82A48F-CDBE-40B8-A592-5630EBBE215E" );

            // Attribute for BlockType
            //   BlockType: Report List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "1C2D47F1-5F7C-4FFD-9588-9EB87DC6E6E8" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "1E1BCA3D-0F6B-49E4-B698-0A8AFC899BD8" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FE1D374F-2FB2-49D2-B4A6-02DA844ACB85" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Merge Templates Ownership
            RockMigrationHelper.DeleteAttribute( "0845BD0A-8A74-49EA-95AD-84EE380A1F97" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "71E56082-AABE-47AD-95A6-85A13218DA6D" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "7202527F-92EC-4545-9073-E58A2B345E7C" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "18A9566F-9F10-4561-97C9-A9DA452D773F" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Participant Column
            RockMigrationHelper.DeleteAttribute( "18C7F5E0-3B25-41EB-A9F7-1474B46801E0" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Donor Column
            RockMigrationHelper.DeleteAttribute( "566E657A-DB31-4FD5-B713-47890E832A2E" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Hide Grid Actions
            RockMigrationHelper.DeleteAttribute( "985D27BA-A4CC-4F60-8149-C244781A016A" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Hide Grid Columns
            RockMigrationHelper.DeleteAttribute( "E3183EA4-137C-4F23-95B4-4E1E5ACB85F6" );

            // Attribute for BlockType
            //   BlockType: Fundraising Donation List
            //   Category: Fundraising
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "AFB0BBFD-79A2-4CF8-8667-C2D913AAE681" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A5AF7CAE-B742-4F28-BF5D-B84139CC4E5D" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "94FFEDC7-188F-4E09-A41D-76A974D8BA37" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "E7493B52-9DE0-48E5-9EB5-66E398186910" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B6541846-7B6A-4A4A-8D19-90EB0B456391" );

            // Attribute for BlockType
            //   BlockType: Business Contact List
            //   Category: Finance
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute( "0CDDB2DD-B992-49A2-94F2-010DFC5D8307" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B88A62DD-E273-4E42-8690-DA5C573481C0" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F3A43658-C52A-4EF9-922D-7D2686A48A13" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "397EA71E-7D39-40FE-AD6C-28DAB1276C63" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B998A4BF-9A2A-4486-972A-C497FB3DE022" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "8DF9D395-BFE8-4CFB-8340-B4AB516BC49D" );

            // Attribute for BlockType
            //   BlockType: Achievement Type List
            //   Category: Streaks
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "59F4B591-3921-4AFB-BF51-5E429B2A72D7" );

            // Attribute for BlockType
            //   BlockType: Person Merge Request List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A3D08902-0AA8-4717-96EC-FAEAC5102630" );

            // Attribute for BlockType
            //   BlockType: Person Merge Request List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "DFDB84FD-A370-4161-9F00-629C5B0060B1" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "DA72C7D5-B0CC-4147-828A-5DF79F88D621" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C6B47E32-CA90-4F7B-83D1-E163BE89AB02" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: History Page
            RockMigrationHelper.DeleteAttribute( "B45EC31E-A1DF-41EE-B2C3-201A8A3C9BF9" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "DF2A7B20-54DA-44DA-A3BE-695CCCCAD6B6" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "757E9FC2-2A7C-4A58-B66D-E658CA957B88" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FEF857C1-8736-427D-94EA-F3F1844FA85D" );

            // Attribute for BlockType
            //   BlockType: Rest Controller List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "49BBE14C-3794-4516-97E2-187576C7ED6F" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A89BF02D-7ECB-47C5-A203-E3ACFF8B8D84" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "1A9D3344-8665-4BA2-AF7E-B8FDF19E9CBD" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: Defined Type
            RockMigrationHelper.DeleteAttribute( "DF88E1B7-FC9A-42DC-BB1F-2CAA4CCD6152" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "7FE27268-DAA5-403C-A2EB-90B53CF2B2D4" );

            // Attribute for BlockType
            //   BlockType: Audit List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9DBEF79B-1B79-4C4B-B8CA-3963ED00BABF" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "937B6777-6EC9-4852-AA44-73825A8D276B" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "3A96BEB8-6F57-49AF-AA4B-77126B916716" );

            // Attribute for BlockType
            //   BlockType: Route List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "417BAE85-9AE4-4808-930C-C9842CEE13F1" );

            // Delete BlockType 
            //   Name: Smart Search
            //   Category: Mobile > Core
            //   Path: -
            //   EntityType: Smart Search
            RockMigrationHelper.DeleteBlockType( "9AA64485-9641-4A06-9450-B5244BC1464A" );

            // Delete BlockType 
            //   Name: Report List
            //   Category: Reporting
            //   Path: -
            //   EntityType: Report List
            RockMigrationHelper.DeleteBlockType( "7C01525C-2FCC-4F0B-A9B5-25E8146AF0D7" );

            // Delete BlockType 
            //   Name: Merge Template List
            //   Category: Core
            //   Path: -
            //   EntityType: Merge Template List
            RockMigrationHelper.DeleteBlockType( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52" );

            // Delete BlockType 
            //   Name: Fundraising Donation List
            //   Category: Fundraising
            //   Path: -
            //   EntityType: Fundraising Donation List
            RockMigrationHelper.DeleteBlockType( "054A8469-A838-4708-B18F-9F2819346298" );

            // Delete BlockType 
            //   Name: Saved Account List
            //   Category: Finance
            //   Path: -
            //   EntityType: Saved Account List
            RockMigrationHelper.DeleteBlockType( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702" );

            // Delete BlockType 
            //   Name: Business Contact List
            //   Category: Finance
            //   Path: -
            //   EntityType: Business Contact List
            RockMigrationHelper.DeleteBlockType( "5E72C18D-F459-4226-820B-B47F88EFEB0F" );

            // Delete BlockType 
            //   Name: Connection Type List
            //   Category: Engagement
            //   Path: -
            //   EntityType: Connection Type List
            RockMigrationHelper.DeleteBlockType( "45F30EA2-F93B-4A63-806F-7CD375DAACAB" );

            // Delete BlockType 
            //   Name: Achievement Type List
            //   Category: Streaks
            //   Path: -
            //   EntityType: Achievement Type List
            RockMigrationHelper.DeleteBlockType( "4ACFBF3F-3D49-4AE3-B468-529F79DA9898" );

            // Delete BlockType 
            //   Name: Person Merge Request List
            //   Category: CRM
            //   Path: -
            //   EntityType: Person Merge Request List
            RockMigrationHelper.DeleteBlockType( "B2CF80F1-5588-46D5-8198-8C5816290E98" );

            // Delete BlockType 
            //   Name: Scheduled Job List
            //   Category: Core
            //   Path: -
            //   EntityType: Scheduled Job List
            RockMigrationHelper.DeleteBlockType( "9B90F2D1-0C7B-4F08-A808-8BA4C9A70A20" );

            // Delete BlockType 
            //   Name: Rest Controller List
            //   Category: Core
            //   Path: -
            //   EntityType: Rest Controller List
            RockMigrationHelper.DeleteBlockType( "A6D8BFD9-0C3D-4F1E-AE0D-325A9C70B4C8" );

            // Delete BlockType 
            //   Name: Defined Value List
            //   Category: Core
            //   Path: -
            //   EntityType: Defined Value List
            RockMigrationHelper.DeleteBlockType( "F431F950-F007-493E-81C8-16559FE4C0F0" );

            // Delete BlockType 
            //   Name: Audit List
            //   Category: Core
            //   Path: -
            //   EntityType: Audit List
            RockMigrationHelper.DeleteBlockType( "120552E2-5C36-4220-9A73-FBBBD75B0964" );

            // Delete BlockType 
            //   Name: Route List
            //   Category: CMS
            //   Path: -
            //   EntityType: Page Route List
            RockMigrationHelper.DeleteBlockType( "D7F40E20-89AF-4CD1-AA64-C6D84C81DCA7" );

        }
    }
}
