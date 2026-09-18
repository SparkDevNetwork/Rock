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
    public partial class CodeGenerated_20260520 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInConfigurationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.CheckInConfigurationList", "Check In Configuration List", "Rock.Blocks.CheckIn.Configuration.CheckInConfigurationList, Rock.Blocks, Version=20.0.2.0, Culture=neutral, PublicKeyToken=null", false, false, "C385BBCD-E0A1-4003-8CCE-487C6B845DED" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Configuration List
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInConfigurationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Configuration List", "Displays a list of check-in configurations.", "Rock.Blocks.CheckIn.Configuration.CheckInConfigurationList", "Check-in > Configuration", "41233A39-404A-478F-A7FC-536B644E6728" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8098DF5D-4B87-4FAF-BA65-E017C5A93353", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the entity type details.", 0, @"", "088DBD6D-BCD7-4CBE-B496-D9FE6C1F55A9" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8098DF5D-4B87-4FAF-BA65-E017C5A93353", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "14028048-695B-426D-94FB-79CE84AAAB83" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8098DF5D-4B87-4FAF-BA65-E017C5A93353", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "580CEFBA-0F26-46B6-8454-1D400B07251A" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Show Site Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Site Icon", "ShowSiteIcon", "Show Site Icon", @"Determines if the site icon should be shown.", 4, @"True", "62439B78-32EF-4F62-B8ED-7E43F79EE497" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Finance
            //   Attribute: Show Transaction Count Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transaction Count Column", "ShowTransactionCountColumn", "Show Transaction Count Column", @"Should the transaction count column be displayed.", 3, @"False", "D282C575-8173-4268-89DA-563AADED6EF7" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Finance
            //   Attribute: Append Suffix to Batch Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Append Suffix to Batch Name", "AppendSuffixToBatchName", "Append Suffix to Batch Name", @"When enabled, appends a suffix to the batch name for refund transactions. When disabled, uses the original batch name. Note: financial gateways that support settlement batches ignore this setting—all transactions process through the settlement batch regardless.", 4, @"True", "5A8E0530-8DF4-446B-AC2A-BFEE71E83F08" );

            // Attribute for BlockType
            //   BlockType: Attribute Categories
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FC50941-A883-47A2-ABE9-13528BCC4D1B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "05DE6049-850A-4D7E-B531-BF0DF47291DA" );

            // Attribute for BlockType
            //   BlockType: Attribute Categories
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FC50941-A883-47A2-ABE9-13528BCC4D1B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B530E52E-E4C3-4598-8D1B-2A7182598C70" );

            // Attribute for BlockType
            //   BlockType: HTML Content Approval
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "79E4D7D2-3F18-43A9-9A62-E02F09C6051C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FFAE14D4-93C8-4415-8262-B3AA7015B078" );

            // Attribute for BlockType
            //   BlockType: HTML Content Approval
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "79E4D7D2-3F18-43A9-9A62-E02F09C6051C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "07CD8666-C058-44E9-834F-DF861B195B1D" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 9, @"0", "8B3FA759-A3A3-4670-9B0D-7DB528CF9977" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 10, @"", "8E4F958C-C76D-4952-93ED-EC8E54C1CF8A" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 11, @"", "F7210130-9B97-4583-B72B-2DA1D7ECBE79" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 12, @"", "CAC4572F-1577-4135-A83C-D672AF4C10C7" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Scan Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Scan Mode", "ScanMode", "Scan Mode", @"", 13, @"0", "EA1725F9-670D-46D7-92C1-0CD2A9D29D27" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Scan Attribute
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Scan Attribute", "ScanAttribute", "Scan Attribute", @"", 14, @"", "784461AD-376F-4F97-B6AA-34E32AA4529F" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 5, @"", "4C1405BE-D967-48E2-B7DC-60EB2DDFE60A" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 6, @"", "8E1CC62E-CE04-4FA4-B2CD-8234673C66E2" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: List Item Details Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Item Details Template", "ListItemDetailsTemplate", "List Item Details Template", @"An optional lava template to appear next to each person in the list.", 8, @"{% comment %}
  This is the lava template for each attendance item in the GroupAttendanceDetail block
   Available Lava Fields:

   + Person (the person on the attendance record)
   + Attended (whether or not the attendance record is marked DidAttend = true)
   + GroupMembers (the member records for the Person)
   + Roles (the member role name(s) for the Person separated by ', ')
{% endcomment %}
<div class=""d-flex align-items-center h-100"" style=""gap: 8px;"">
    <img src=""{{ Person.PhotoUrl }}"" style=""border-radius: 48px; width: 48px; height: 48px"" />
    <div class=""checkbox-card-data"">
        {% assign activeGroupMembershipCount = GroupMembers | Where:'GroupMemberStatus','Active' | Size %}
        {% if activeGroupMembershipCount == 0 %}<span class=""label label-info align-self-end"">{{ GroupMembers | Select:'GroupMemberStatus' | Distinct | Join:', ' }}</span>{% endif %}
        <div>
        <strong>{{ Person.LastName }}, {{ Person.NickName }}</strong>
        {% if Roles %}<div class=""text-sm text-muted"">{{ Roles }}</div>{% endif %}
        </div>
        {% if activeGroupMembershipCount == 0 %}<span class=""label label-info invisible"">&nbsp;</span>{% endif %}
    </div>
</div>", "CD9A3DF8-33D4-4867-A7BD-FE64ABCAFD19" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Disable Long-List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Long-List", "DisableLongList", "Disable Long-List", @"Will disable the long-list feature which groups individuals by the first character of their last name. When enabled, this only shows when there are more than 50 individuals on the list.", 14, @"False", "A6EE238F-297D-42BE-B1F6-8F04F84CC2B8" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Disable Did Not Meet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Did Not Meet", "DisableDidNotMeet", "Disable Did Not Meet", @"Allows for hiding the flag that the group did not meet.", 15, @"False", "5B95730D-0F0F-40F2-9738-321F26172DC6" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Hide Back Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Back Button", "HideBackButton", "Hide Back Button", @"Will hide the back button from the bottom of the block.", 16, @"False", "2A0ACABE-63CD-4C40-955E-7FE20DDA3EC1" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Date Selection Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Date Selection Mode", "DateSelectionMode", "Date Selection Mode", @"'Date Picker' individual can pick any date. 'Current Date' locked to the current date. 'Pick From Schedule' drop down of dates from the schedule. This will need to be updated based on the location.", 17, @"1", "39CD7108-18FE-4880-A706-B054A7E9D211" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Number of Previous Days To Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Previous Days To Show", "NumberOfPreviousDaysToShow", "Number of Previous Days To Show", @"When the 'Pick From Schedule' option is used, this setting will control how many days back appear in the drop down list to choose from.", 18, @"14", "1C6839F2-60AC-4073-9D40-2979CB191740" );

            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C12257A6-E526-4518-971B-D4EF74D84202" );

            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B924B19D-3953-4228-AFB8-83DA2C3E9206" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Send Payment Reminder
            //   Category: Event
            //   Attribute: Registration Instance Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED56CD0A-0A8D-4758-A689-55B7BEC1B589", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "Registration Instance Page", @"The registration instance page to return to after reminders are sent.", 1, @"844DC54B-DAEC-47B3-A63A-712DD6D57793", "525047E3-1A73-4FA9-9516-44134C5811F8" );

            // Attribute for BlockType
            //   BlockType: Check-in Configuration Settings
            //   Category: Check-in > Configuration
            //   Attribute: Show Classic Check-in Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Classic Check-in Settings", "ShowClassicCheckInSettings", "Show Classic Check-in Settings", @"Enabling this will show Classic Check-in Settings for this configuration. Note: Trailblazer Mode must be enabled.", 1, @"False", "6DB40344-3B1F-4926-89FA-65BFB3B2EB28" );

            // Attribute for BlockType
            //   BlockType: Exception Occurrences
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3486885-FA88-4B67-88B6-472F1FE4E5E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F14D51A2-8B9C-4E10-9298-E3344798BC04" );

            // Attribute for BlockType
            //   BlockType: Exception Occurrences
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3486885-FA88-4B67-88B6-472F1FE4E5E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2D113ADE-262D-4D18-BC4F-7181566A824D" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: NCOA Process Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3997FE75-E069-4879-B8BA-C8B19C367CD3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "NCOA Process Page", "NcoaProcessPage", "NCOA Process Page", @"The page used to process NCOA data.", 0, @"56EDE500-CEE6-41F4-B724-E44E66A4432F,20A7BA14-BC22-48B2-AF82-063F428B66E4", "BBC1D97F-6E61-471B-8F09-EA59C27E9D26" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3997FE75-E069-4879-B8BA-C8B19C367CD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B65374BF-916B-4BA7-BBEA-87FD9B688D51" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3997FE75-E069-4879-B8BA-C8B19C367CD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "98407BC9-1885-4A26-A257-286CF7407121" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D7BD2F0E-383C-46D9-B55F-D557562BAF6E" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A56DE5AF-79BF-4B49-A477-410800DD9616" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDE31844-B024-472E-9B21-E094DFC40CAB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C8301EBB-15BA-45EF-8E51-242B86EF063B" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DDE31844-B024-472E-9B21-E094DFC40CAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7A021D94-6D0F-40DC-BF80-208C08384497" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Discount List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C8954BF-E221-4B2F-AC3B-612DC16BA27D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2D0A7168-0877-4761-838A-10FD5BCFFD63" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Discount List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C8954BF-E221-4B2F-AC3B-612DC16BA27D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F26B3D43-49EE-4C19-B273-0D38A2DFAD5F" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E877FDE1-DEE6-48F8-8150-4E28D5ABB694", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "368C08F7-B5A1-4FB5-8B69-D8C087EC950B" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E877FDE1-DEE6-48F8-8150-4E28D5ABB694", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "ED17C82A-D8D9-4361-9C9C-5D7D3024AA7E" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registration List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F42E7FD0-96B4-4398-B1CA-165949E8ABF1" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registration List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "18C39CE9-7CEA-47E3-B8C6-3A5710FA9F48" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "038E7B3D-EC33-46D0-8AC6-DE83D191FC72" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B04CA032-DB6F-4A9E-910C-77D2E56AA1D4" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6766EDA6-62CD-4986-B2CD-B5F3D1D4A03A" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "92B8804B-E8F5-47DF-9042-96D6887915CE" );

            // Attribute for BlockType
            //   BlockType: Reminder Types
            //   Category: Reminders
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4161700-6882-4A50-B362-8E4C8F37C79D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E29F427B-4AFA-4BBD-AF6B-40778B64C9DB" );

            // Attribute for BlockType
            //   BlockType: Reminder Types
            //   Category: Reminders
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E4161700-6882-4A50-B362-8E4C8F37C79D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "658829F5-0CFB-4E9F-8E86-593907E97B11" );

            // Attribute for BlockType
            //   BlockType: Snippet List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "32D3C53C-6C94-44CC-88F5-ED3AA2B39B08" );

            // Attribute for BlockType
            //   BlockType: Snippet List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EDAD934-6129-480B-9812-4BA7B9978AD2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F5949447-D35F-4FE7-B30B-718A9D031C24" );

            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "397583B2-0DC4-4D69-9169-C95B430AB336", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "163AD9AA-5460-4C58-AB74-FE82FE7DC9D7" );

            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "397583B2-0DC4-4D69-9169-C95B430AB336", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "584F7F1A-BC17-4B81-A169-C8EAB90A6F86" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Docked Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91A66C59-830E-49B5-A196-DCF93D0DDE92", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Docked Mode", "DockedMode", "Docked Mode", @"In Docked mode, the chat bot will appear as a docked panel on the page.", 1, @"False", "CC3B1300-2F27-45BF-A689-5059D84F46EF" );

            // Attribute for BlockType
            //   BlockType: MCP Server List
            //   Category: Core
            //   Attribute: Append API Key to URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "54B23A63-87C0-4955-B915-C91F23C36D48", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Append API Key to URL", "AppendApiKeyToUrl", "Append API Key to URL", @"When enabled, the individual's API key is appended to the MCP URL. Use this if the MCP server requires authentication via URL parameter rather than using OAuth. Note that API keys grant access based on the permissions of the individual they belong to — treat them as sensitive credentials and avoid sharing or exposing MCP URLs that contain them.", 0, @"False", "65775CE0-1C78-4849-B807-D4D0728A8D5C" );

            // Attribute for BlockType
            //   BlockType: Check-in Configuration List
            //   Category: Check-in > Configuration
            //   Attribute: Show Classic Label Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41233A39-404A-478F-A7FC-536B644E6728", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Classic Label Settings", "ShowClassicLabelSettings", "Show Classic Label Settings", @"Show the page link under Related Settings that allows the configuration of Classic Labels.", 0, @"False", "D6DE83E1-86A3-4529-859D-45FFC5B17271" );

            RockMigrationHelper.UpdateFieldType( "Schedule Builder", "", "Rock", "Rock.Field.Types.ScheduleBuilderFieldType", "09D6E619-E8BB-4CF4-8C25-296079A7C318" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "584F7F1A-BC17-4B81-A169-C8EAB90A6F86" );

            // Attribute for BlockType
            //   BlockType: Snippet Type List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "163AD9AA-5460-4C58-AB74-FE82FE7DC9D7" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Docked Mode
            RockMigrationHelper.DeleteAttribute( "CC3B1300-2F27-45BF-A689-5059D84F46EF" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B04CA032-DB6F-4A9E-910C-77D2E56AA1D4" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "038E7B3D-EC33-46D0-8AC6-DE83D191FC72" );

            // Attribute for BlockType
            //   BlockType: Check-in Configuration List
            //   Category: Check-in > Configuration
            //   Attribute: Show Classic Label Settings
            RockMigrationHelper.DeleteAttribute( "D6DE83E1-86A3-4529-859D-45FFC5B17271" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Scan Attribute
            RockMigrationHelper.DeleteAttribute( "784461AD-376F-4F97-B6AA-34E32AA4529F" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Scan Mode
            RockMigrationHelper.DeleteAttribute( "EA1725F9-670D-46D7-92C1-0CD2A9D29D27" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Redirect To Page
            RockMigrationHelper.DeleteAttribute( "CAC4572F-1577-4135-A83C-D672AF4C10C7" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "F7210130-9B97-4583-B72B-2DA1D7ECBE79" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Completion Xaml
            RockMigrationHelper.DeleteAttribute( "8E4F958C-C76D-4952-93ED-EC8E54C1CF8A" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Completion Action
            RockMigrationHelper.DeleteAttribute( "8B3FA759-A3A3-4670-9B0D-7DB528CF9977" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "7A021D94-6D0F-40DC-BF80-208C08384497" );

            // Attribute for BlockType
            //   BlockType: Streak Type List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C8301EBB-15BA-45EF-8E51-242B86EF063B" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "A56DE5AF-79BF-4B49-A477-410800DD9616" );

            // Attribute for BlockType
            //   BlockType: Streak Type Exclusion List
            //   Category: Streaks
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "D7BD2F0E-383C-46D9-B55F-D557562BAF6E" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Number of Previous Days To Show
            RockMigrationHelper.DeleteAttribute( "1C6839F2-60AC-4073-9D40-2979CB191740" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Date Selection Mode
            RockMigrationHelper.DeleteAttribute( "39CD7108-18FE-4880-A706-B054A7E9D211" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Hide Back Button
            RockMigrationHelper.DeleteAttribute( "2A0ACABE-63CD-4C40-955E-7FE20DDA3EC1" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Disable Did Not Meet
            RockMigrationHelper.DeleteAttribute( "5B95730D-0F0F-40F2-9738-321F26172DC6" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Disable Long-List
            RockMigrationHelper.DeleteAttribute( "A6EE238F-297D-42BE-B1F6-8F04F84CC2B8" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: List Item Details Template
            RockMigrationHelper.DeleteAttribute( "CD9A3DF8-33D4-4867-A7BD-FE64ABCAFD19" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "8E1CC62E-CE04-4FA4-B2CD-8234673C66E2" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "4C1405BE-D967-48E2-B7DC-60EB2DDFE60A" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Finance
            //   Attribute: Append Suffix to Batch Name
            RockMigrationHelper.DeleteAttribute( "5A8E0530-8DF4-446B-AC2A-BFEE71E83F08" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Finance
            //   Attribute: Show Transaction Count Column
            RockMigrationHelper.DeleteAttribute( "D282C575-8173-4268-89DA-563AADED6EF7" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "92B8804B-E8F5-47DF-9042-96D6887915CE" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "6766EDA6-62CD-4986-B2CD-B5F3D1D4A03A" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registration List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "18C39CE9-7CEA-47E3-B8C6-3A5710FA9F48" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registration List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F42E7FD0-96B4-4398-B1CA-165949E8ABF1" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "ED17C82A-D8D9-4361-9C9C-5D7D3024AA7E" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Linkage List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "368C08F7-B5A1-4FB5-8B69-D8C087EC950B" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Discount List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F26B3D43-49EE-4C19-B273-0D38A2DFAD5F" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Discount List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "2D0A7168-0877-4761-838A-10FD5BCFFD63" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Send Payment Reminder
            //   Category: Event
            //   Attribute: Registration Instance Page
            RockMigrationHelper.DeleteAttribute( "525047E3-1A73-4FA9-9516-44134C5811F8" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "98407BC9-1885-4A26-A257-286CF7407121" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B65374BF-916B-4BA7-BBEA-87FD9B688D51" );

            // Attribute for BlockType
            //   BlockType: NCOA Results
            //   Category: CRM
            //   Attribute: NCOA Process Page
            RockMigrationHelper.DeleteAttribute( "BBC1D97F-6E61-471B-8F09-EA59C27E9D26" );

            // Attribute for BlockType
            //   BlockType: MCP Server List
            //   Category: Core
            //   Attribute: Append API Key to URL
            RockMigrationHelper.DeleteAttribute( "65775CE0-1C78-4849-B807-D4D0728A8D5C" );

            // Attribute for BlockType
            //   BlockType: Reminder Types
            //   Category: Reminders
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "658829F5-0CFB-4E9F-8E86-593907E97B11" );

            // Attribute for BlockType
            //   BlockType: Reminder Types
            //   Category: Reminders
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "E29F427B-4AFA-4BBD-AF6B-40778B64C9DB" );

            // Attribute for BlockType
            //   BlockType: Attribute Categories
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B530E52E-E4C3-4598-8D1B-2A7182598C70" );

            // Attribute for BlockType
            //   BlockType: Attribute Categories
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "05DE6049-850A-4D7E-B531-BF0DF47291DA" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "580CEFBA-0F26-46B6-8454-1D400B07251A" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "14028048-695B-426D-94FB-79CE84AAAB83" );

            // Attribute for BlockType
            //   BlockType: Entity Types
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "088DBD6D-BCD7-4CBE-B496-D9FE6C1F55A9" );

            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "B924B19D-3953-4228-AFB8-83DA2C3E9206" );

            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C12257A6-E526-4518-971B-D4EF74D84202" );

            // Attribute for BlockType
            //   BlockType: Exception Occurrences
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2D113ADE-262D-4D18-BC4F-7181566A824D" );

            // Attribute for BlockType
            //   BlockType: Exception Occurrences
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "F14D51A2-8B9C-4E10-9298-E3344798BC04" );

            // Attribute for BlockType
            //   BlockType: Snippet List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F5949447-D35F-4FE7-B30B-718A9D031C24" );

            // Attribute for BlockType
            //   BlockType: Snippet List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "32D3C53C-6C94-44CC-88F5-ED3AA2B39B08" );

            // Attribute for BlockType
            //   BlockType: Site List
            //   Category: CMS
            //   Attribute: Show Site Icon
            RockMigrationHelper.DeleteAttribute( "62439B78-32EF-4F62-B8ED-7E43F79EE497" );

            // Attribute for BlockType
            //   BlockType: HTML Content Approval
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "07CD8666-C058-44E9-834F-DF861B195B1D" );

            // Attribute for BlockType
            //   BlockType: HTML Content Approval
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FFAE14D4-93C8-4415-8262-B3AA7015B078" );

            // Attribute for BlockType
            //   BlockType: Check-in Configuration Settings
            //   Category: Check-in > Configuration
            //   Attribute: Show Classic Check-in Settings
            RockMigrationHelper.DeleteAttribute( "6DB40344-3B1F-4926-89FA-65BFB3B2EB28" );

            // Delete BlockType 
            //   Name: Check-in Configuration List
            //   Category: Check-in > Configuration
            //   Path: -
            //   EntityType: Check In Configuration List
            RockMigrationHelper.DeleteBlockType( "41233A39-404A-478F-A7FC-536B644E6728" );
        }
    }
}
