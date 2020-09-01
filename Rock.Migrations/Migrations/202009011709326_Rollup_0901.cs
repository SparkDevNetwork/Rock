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
    public partial class Rollup_0901 : Rock.Migrations.RockMigration
    {
        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";
        
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RockMobileCommunicationPushUpdates();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            
            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "AF21AFB0-4E38-4340-AFF4-D833BFFF0BB8");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "FD0D47E8-5D13-4F53-BD78-9BEC08B5380A");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "3BC8D4E5-65BC-4EC5-81C2-D072AB37682F");

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD0D47E8-5D13-4F53-BD78-9BEC08B5380A", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "8A6E36C4-C5E6-430B-980A-5FF07E4BA222" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD0D47E8-5D13-4F53-BD78-9BEC08B5380A", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "E3A6433F-6FDB-4B7D-9B9E-70770B326DF1" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3BC8D4E5-65BC-4EC5-81C2-D072AB37682F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "5E262B70-ECDB-49F6-8A24-A5652D6CEC15" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3BC8D4E5-65BC-4EC5-81C2-D072AB37682F", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "5FC6667D-5F6E-4660-BA2B-0C9E6C7418A1" );

            // Attribute for BlockType: OpenID Connect Claims:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "142BE80B-5FB2-459D-AE5C-E371C79538F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FAB6ED9D-0445-45B4-B491-A8C05E2CC496" );

            // Attribute for BlockType: OpenID Connect Claims:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "142BE80B-5FB2-459D-AE5C-E371C79538F6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6DD7A22C-9C1E-4E61-85FD-00AFC9DEF4B4" );

            // Attribute for BlockType: OpenID Connect Clients:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "616D1A98-067D-43B8-B7F5-41FB12FB894E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "226A3940-A61F-442C-87F7-057C2A261EC9" );

            // Attribute for BlockType: OpenID Connect Clients:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "616D1A98-067D-43B8-B7F5-41FB12FB894E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "555D96E1-6F10-4E0A-92A4-1FB3EA0C529D" );

            // Attribute for BlockType: OpenID Connect Scopes:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E407FC8-B5B9-488E-81E4-8EA5F7CFCAB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "08CCB296-F8C4-407D-96CF-F7EF27B3391F" );

            // Attribute for BlockType: OpenID Connect Scopes:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E407FC8-B5B9-488E-81E4-8EA5F7CFCAB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F6746DB4-4358-4539-BB79-B37E16A70741" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: OpenID Connect Scopes
            RockMigrationHelper.DeleteAttribute("F6746DB4-4358-4539-BB79-B37E16A70741");

            // core.CustomActionsConfigs Attribute for BlockType: OpenID Connect Scopes
            RockMigrationHelper.DeleteAttribute("08CCB296-F8C4-407D-96CF-F7EF27B3391F");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: OpenID Connect Clients
            RockMigrationHelper.DeleteAttribute("555D96E1-6F10-4E0A-92A4-1FB3EA0C529D");

            // core.CustomActionsConfigs Attribute for BlockType: OpenID Connect Clients
            RockMigrationHelper.DeleteAttribute("226A3940-A61F-442C-87F7-057C2A261EC9");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: OpenID Connect Claims
            RockMigrationHelper.DeleteAttribute("6DD7A22C-9C1E-4E61-85FD-00AFC9DEF4B4");

            // core.CustomActionsConfigs Attribute for BlockType: OpenID Connect Claims
            RockMigrationHelper.DeleteAttribute("FAB6ED9D-0445-45B4-B491-A8C05E2CC496");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("5FC6667D-5F6E-4660-BA2B-0C9E6C7418A1");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("5E262B70-ECDB-49F6-8A24-A5652D6CEC15");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("E3A6433F-6FDB-4B7D-9B9E-70770B326DF1");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("8A6E36C4-C5E6-430B-980A-5FF07E4BA222");

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("3BC8D4E5-65BC-4EC5-81C2-D072AB37682F"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("FD0D47E8-5D13-4F53-BD78-9BEC08B5380A"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("AF21AFB0-4E38-4340-AFF4-D833BFFF0BB8"); // Structured Content View

        }
    
        /// <summary>
        /// JE: Rock Mobile Communication Push Updates
        /// </summary>
        private void RockMobileCommunicationPushUpdates()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlock(
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW,
                "Mobile Communication View",
                string.Empty );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "39B8B16D-D213-46FD-9B8F-710453806193",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW,
                "Default",
                @"<StackLayout>
    <Label Text=""{{ Communication.PushTitle | Escape }}"" StyleClass=""h1"" />
    <Rock:Html>
        <![CDATA[{{ Communication.PushOpenMessage }}]]>
    </Rock:Html>
</StackLayout> ",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }
    }
}
