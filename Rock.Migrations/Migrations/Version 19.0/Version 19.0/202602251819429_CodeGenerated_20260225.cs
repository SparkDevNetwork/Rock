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
    /// Code generated EF migration from running the /Dev Tools/Sql/CodeGen_PagesBlocksAttributesMigration.sql
    /// </summary>
    public partial class CodeGenerated_20260225 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Group Type Detail
            //   Category: Groups
            //   Attribute: Enable Group View Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "78B8EE69-71A7-43C1-B00B-ED13828FE104", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group View Lava Template", "EnableGroupViewLavaTemplate", "Enable Group View Lava Template", @"This Lava template will be used by the Group Details block when viewing a group. This allows you to customize the layout of a group base on its type.", 0, @"False", "B4E640E4-EA28-4977-BBA6-E0D43BFA0E69" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Detail
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "132D18F3-D169-4260-94E0-84F42A40B356", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "094C830E-7D83-4689-A16F-BAAD2F318DA2" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Detail
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "132D18F3-D169-4260-94E0-84F42A40B356", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F2A194FF-9D0B-4684-B530-F34BDA38D4A5" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Summary
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8DDB0616-E986-4904-9A1D-8D6694021536" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Summary
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DAF15F9-E237-4B2B-8309-F335456F8FE4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BFD90E7B-F423-45AD-BD77-1E8792045562" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25F0658-3038-45B0-A6AA-DFFC4053EE13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "37C2015F-696E-4A73-9A45-3CDE7C2C99DC" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D25F0658-3038-45B0-A6AA-DFFC4053EE13", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "687889A5-DEFC-4A80-8A44-9344860FD013" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Details
            //   Category: Mobile > Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 3, @"", "413F62E2-43D5-4C94-AC94-1829FA6A586D" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Enable Mark All as Present
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Mark All as Present", "EnableMarkAllAsPresentButton", "Enable Mark All as Present", @"Controls whether a 'Mark All as Present' button appears in the 'Checked-in' view, allowing all rostered individuals to be marked as present at once.", 10, @"False", "B66692F8-FEC6-4F78-9A18-1F9CF07AE74F" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Data View Alert Icons
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "F739BF5D-3FDC-45EC-A03C-1AE7C47E3883", "Data View Alert Icons", "DataViewAlertIcons", "Data View Alert Icons", @"The data views to use for alert icons on individuals. The data view must be a persisted data view for it to be used.", 12, @"", "73829664-3317-4886-BD70-F359E14462B5" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable Missing Field Diagnostics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0252E237-0684-4426-9E5C-D454A13E152A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Missing Field Diagnostics", "EnableMissingFieldDiagnostics", "Enable Missing Field Diagnostics", @"When enabled, special checks will be performed during registration to identify missing required form fields and logged to the exception log. This should only be enabled at the request of the core team.", 0, @"False", "C4BAFCA0-1B7D-4EE6-B889-FE051F1CAD7A" );

            // Attribute for BlockType
            //   BlockType: Content Collection List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F305FE35-2EFA-4653-AA1A-87AE990EAFEB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4D5B7CB4-6B2C-44B4-AAC1-5F720C4304FD" );

            // Attribute for BlockType
            //   BlockType: Content Collection List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F305FE35-2EFA-4653-AA1A-87AE990EAFEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "74749B6E-915E-4CBD-A09A-8AE83E7812F5" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Toolbox Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Toolbox Name", "ToolboxName", "Toolbox Name", @"The name that you want to call this tool.", 3, @"Beacon", "2E379904-14DB-4EA1-9E61-C7A56D7421A4" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Toolbox subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Toolbox subtitle", "ToolboxSubtitle", "Toolbox subtitle", @"The subtitle appears below the Toolbox name.", 4, @"Small actions with eternal impact.", "2DF07436-AE4C-48FB-8A78-B75FA4196081" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Completion Lookback Period
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Completion Lookback Period", "CompletionLookbackPeriod", "Completion Lookback Period", @"Number of days to look back when calculating on-time completion", 5, @"30", "6B2FDE5C-313E-454A-93D8-B661717EB91B" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: Toolbox Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F1E3C4B-6D7E-4A8F-9C2B-3D4E5F6A7B8C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Toolbox Name", "ToolboxName", "Toolbox Name", @"The name that you want to call this tool.", 2, @"Beacon", "B9DEC356-AA18-44CB-A136-5367E4CF7857" );

            // Attribute for BlockType
            //   BlockType: Snippet Detail
            //   Category: Communication
            //   Attribute: Snippet Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B0F3048-99BA-4ED1-8DE6-6A34F498F556", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Snippet Type", "SnippetType", "Snippet Type", @"Determines what type of snippet to filter on. This is required (only one type can be displayed at a time).", 0, @"", "FCBCEEF6-617B-4CA0-8368-BDD9A3EEC343" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3364AABF-0C5B-4BFB-8CF3-B1A80FD3ED10", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Should inactive campuses be listed as well?", 4, @"false", "272533DD-603D-4D27-9A62-9BE3487367C5" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Default Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3364AABF-0C5B-4BFB-8CF3-B1A80FD3ED10", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Default Campus", "DefaultCampus", "Default Campus", @"When there is no campus value, what campus should be displayed?", 5, @"", "441F59C6-3FAE-45B7-9949-DEE8199B38F6" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3364AABF-0C5B-4BFB-8CF3-B1A80FD3ED10", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the campus drop-down.", 6, @"", "794D83B5-12C5-45AA-9B52-9DFB943B8FA9" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3364AABF-0C5B-4BFB-8CF3-B1A80FD3ED10", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the campus drop-down.", 7, @"", "400AA79B-7474-4DF0-8132-47CDA5B194EE" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Person Viewed Summary
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "BFD90E7B-F423-45AD-BD77-1E8792045562" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Summary
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "8DDB0616-E986-4904-9A1D-8D6694021536" );

            // Attribute for BlockType
            //   BlockType: Group Type Detail
            //   Category: Groups
            //   Attribute: Enable Group View Lava Template
            RockMigrationHelper.DeleteAttribute( "B4E640E4-EA28-4977-BBA6-E0D43BFA0E69" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "687889A5-DEFC-4A80-8A44-9344860FD013" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Engagement
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "37C2015F-696E-4A73-9A45-3CDE7C2C99DC" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "400AA79B-7474-4DF0-8132-47CDA5B194EE" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "794D83B5-12C5-45AA-9B52-9DFB943B8FA9" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Default Campus
            RockMigrationHelper.DeleteAttribute( "441F59C6-3FAE-45B7-9949-DEE8199B38F6" );

            // Attribute for BlockType
            //   BlockType: Check-in Context Setter
            //   Category: Check-in > Manager
            //   Attribute: Include Inactive Campuses
            RockMigrationHelper.DeleteAttribute( "272533DD-603D-4D27-9A62-9BE3487367C5" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Data View Alert Icons
            RockMigrationHelper.DeleteAttribute( "73829664-3317-4886-BD70-F359E14462B5" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Enable Mark All as Present
            RockMigrationHelper.DeleteAttribute( "B66692F8-FEC6-4F78-9A18-1F9CF07AE74F" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Details
            //   Category: Mobile > Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "413F62E2-43D5-4C94-AC94-1829FA6A586D" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: Toolbox Name
            RockMigrationHelper.DeleteAttribute( "B9DEC356-AA18-44CB-A136-5367E4CF7857" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Completion Lookback Period
            RockMigrationHelper.DeleteAttribute( "6B2FDE5C-313E-454A-93D8-B661717EB91B" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Toolbox subtitle
            RockMigrationHelper.DeleteAttribute( "2DF07436-AE4C-48FB-8A78-B75FA4196081" );

            // Attribute for BlockType
            //   BlockType: Outreach Dashboard
            //   Category: Engagement
            //   Attribute: Toolbox Name
            RockMigrationHelper.DeleteAttribute( "2E379904-14DB-4EA1-9E61-C7A56D7421A4" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Detail
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F2A194FF-9D0B-4684-B530-F34BDA38D4A5" );

            // Attribute for BlockType
            //   BlockType: Person Viewed Detail
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "094C830E-7D83-4689-A16F-BAAD2F318DA2" );

            // Attribute for BlockType
            //   BlockType: Registration Entry
            //   Category: Event
            //   Attribute: Enable Missing Field Diagnostics
            RockMigrationHelper.DeleteAttribute( "C4BAFCA0-1B7D-4EE6-B889-FE051F1CAD7A" );

            // Attribute for BlockType
            //   BlockType: Snippet Detail
            //   Category: Communication
            //   Attribute: Snippet Type
            RockMigrationHelper.DeleteAttribute( "FCBCEEF6-617B-4CA0-8368-BDD9A3EEC343" );

            // Attribute for BlockType
            //   BlockType: Content Collection List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "74749B6E-915E-4CBD-A09A-8AE83E7812F5" );

            // Attribute for BlockType
            //   BlockType: Content Collection List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "4D5B7CB4-6B2C-44B4-AAC1-5F720C4304FD" );
        }
    }
}
