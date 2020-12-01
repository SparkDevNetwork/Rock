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
    public partial class Rollup_0929 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixNotesBlockAttributeTypo();
            RemoveContentChannelItemListMobileBlock();
            UpdateAttributeValues();
            BlockTemplateforMobileEventItemOccurrenceListByAudienceUp();
            FixFinancialPersonSavedAccount();
            DefaultApostropheForNameFields();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            BlockTemplateforMobileEventItemOccurrenceListByAudienceDown();
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            
            // Add/Update BlockType Connection Opportunity Select
            RockMigrationHelper.UpdateBlockType("Connection Opportunity Select","Block to display the connection opportunities that the user is authorized to view.","~/Blocks/Connection/ConnectionOpportunitySelect.ascx","Connection","CE9EBF79-FDFB-4769-9E71-80BA066B8BE6");

            // Add/Update BlockType Connection Request Board
            RockMigrationHelper.UpdateBlockType("Connection Request Board","Display the Connection Requests for a selected Connection Opportunity as a list or board view.","~/Blocks/Connection/ConnectionRequestBoard.ascx","Connection","5F2FE25A-9D94-4B81-8783-EC32DD062913");

            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "221FF0B2-5B7C-4E15-A85E-C9B884ABE0CA");

            // Add/Update Mobile Block Type:Communication View
            RockMigrationHelper.UpdateMobileBlockType("Communication View", "Displays a communication to the user.", "Rock.Blocks.Types.Mobile.Communication.CommunicationView", "Mobile > Communication", "9D694503-94A2-42C8-B355-FFA45894E95A");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "9D816E33-CA5A-49C8-82AF-204C2B68749E");

            // Add/Update Mobile Block Type:Event Item Occurrence List By Audience Lava
            RockMigrationHelper.UpdateMobileBlockType("Event Item Occurrence List By Audience Lava", "Block that takes an audience and displays calendar item occurrences for it using Lava.", "Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava", "Mobile > Events", "694745E8-4B1A-40E1-ABDF-09533C4BCD24");

            // Attribute for BlockType: Connection Opportunity Select:Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create connection opportunities.", 1, @"9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "C170AC54-47B3-4B25-A149-742627D254CE" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Opportunity Detail Page", "OpportunityDetailPage", "Opportunity Detail Page", @"Page to go to when an opportunity is selected.", 2, @"4FBCEB52-8892-4035-BDEA-112A494BE81F", "4CB57D26-4848-4F24-BB5B-2D3C44ED9434" );

            // Attribute for BlockType: Connection Opportunity Select:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 3, @"", "9D1A73B7-79A9-43FB-A465-DE9A0BD61A09" );

            // Attribute for BlockType: Connection Opportunity Select:Status Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "Status Template", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 4, @"
<div class='badge-legend expand-on-hover padding-r-md'>
    <span class='badge badge-info badge-circle js-legend-badge'>Assigned To You</span>
    <span class='badge badge-warning badge-circle js-legend-badge'>Unassigned Item</span>
    <span class='badge badge-critical badge-circle js-legend-badge'>Critical Status</span>
    <span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>
</div>", "DD58A091-CBB2-4A09-BC1A-19A5DA7FD6C1" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Summary Template", "OpportunitySummaryTemplate", "Opportunity Summary Template", @"Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary and ConnectionOpportunity.", 5, @"
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>
", "D042660B-00C2-440D-A700-B8B41398BD48" );

            // Attribute for BlockType: Connection Request Board:Max Cards per Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Cards per Column", "MaxCards", "Max Cards per Column", @"The maximum number of cards to display per column. This is to prevent performance issues caused by rendering too many cards at a time.", 0, @"100", "8977B16A-791A-452D-95BD-8DF7DF9A8040" );

            // Attribute for BlockType: Connection Request Board:Enable Request Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Request Security", "EnableRequestSecurity", "Enable Request Security", @"When enabled, the security column for request would be displayed.", 1, @"False", "3BBB0A22-4F17-4325-B3DE-AF37090F11E4" );

            // Attribute for BlockType: Connection Request Board:Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set a view profile button will show for each grid item.", 2, @"08DBD8A5-2C35-4146-B4A8-0F7652348B25", "5D76DCA5-8FD4-485D-B026-B046FF49D70A" );

            // Attribute for BlockType: Connection Request Board:Workflow Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "Workflow Detail Page", @"Page used to display details about a workflow.", 3, @"BA547EED-5537-49CF-BD4E-C583D760788C", "B39D65A0-7E24-4F09-8BA5-A7B8A20C50B2" );

            // Attribute for BlockType: Connection Request Board:Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 4, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "0A5C274B-0EFB-4FEC-99DA-AFE08B5D68D6" );

            // Attribute for BlockType: Connection Request Board:Status Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "Status Template", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 5, @"
<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>
    <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>
    <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
</div>", "D228323C-1283-4F53-B9B8-2BB77461AEA1" );

            // Attribute for BlockType: Connection Request Board:Connection Request Status Icons Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Connection Request Status Icons Template", "ConnectionRequestStatusIconsTemplate", "Connection Request Status Icons Template", @"Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.", 6, @"
<div class='board-card-pills'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='board-card-pill badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='board-card-pill badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='board-card-pill badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='board-card-pill badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
", "0CFEAEE1-533B-4DF8-ACD7-2C9642F261D6" );

            // Attribute for BlockType: Connection Request Board:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page used to display group details.", 8, @"4E237286-B715-4109-A578-C1445EC02707", "3C2787AE-EE14-425D-B600-73A3429334F8" );

            // Attribute for BlockType: Connection Request Board:SMS Link Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "SMS Link Page", "SmsLinkPage", "SMS Link Page", @"Page that will be linked for SMS enabled phones.", 9, @"2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "3CCC4AE5-D078-48C5-B134-77CFBC8C675C" );

            // Attribute for BlockType: Connection Request Board:Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The badges to display in this block.", 10, @"", "A79EBB5A-A1C8-4C98-91EC-B30CDB297A90" );

            // Attribute for BlockType: Connection Request Board:Lava Heading Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Heading Template", "LavaHeadingTemplate", "Lava Heading Template", @"The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 11, @"", "86142B15-47F2-4065-9B8A-3680B9FBBB20" );

            // Attribute for BlockType: Connection Request Board:Lava Badge Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Badge Bar", "LavaBadgeBar", "Lava Badge Bar", @"The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 12, @"", "42B332C4-6B38-4053-953D-EFCF740DF27E" );

            // Attribute for BlockType: Connection Request Board:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 13, @"", "689CFB59-A545-4C7A-8EBD-DD8B9671E89D" );

            // Attribute for BlockType: Connection Request Board:Limit to Assigned Connections
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Assigned Connections", "OnlyShowAssigned", "Limit to Assigned Connections", @"When enabled, only requests assigned to the current person will be shown.", 14, @"False", "3E99D945-0E2F-4AD5-BC7D-DC61EA5883A6" );

            // Attribute for BlockType: Data View Results:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F7418932-8570-41CA-84A1-37F93DD7A6A2" );

            // Attribute for BlockType: Data View Results:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DEB2DDD5-F4D8-4C6A-A989-AF05C90DDD0C" );

            // Attribute for BlockType: Communication View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D694503-94A2-42C8-B355-FFA45894E95A", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 0, @"39B8B16D-D213-46FD-9B8F-710453806193", "A2C3BF0B-3D23-4A77-B509-2BCA8F155011" );

            // Attribute for BlockType: Communication View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D694503-94A2-42C8-B355-FFA45894E95A", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 1, @"", "7CEFD97E-53A4-4692-85FC-865D1BCEA7EF" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D816E33-CA5A-49C8-82AF-204C2B68749E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "7DD08D0D-DBD8-474D-9F56-C725C6908EA7" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9D816E33-CA5A-49C8-82AF-204C2B68749E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "288E61FE-D831-49CB-941D-6832FC7172BE" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "List Title", @"The title to make available in the lava.", 0, @"Upcoming Events", "62A54080-911A-42B1-A5F1-9C60A97456E6" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "Audience", @"The audience to show calendar items for.", 1, @"", "05BC9D02-6F59-437F-9D18-CE7217F800A7" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"Filters the events by a specific calendar.", 2, @"", "7F2125BB-7226-442D-B19D-2E56C0CA8944" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "Campuses", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 3, @"", "1E1BD565-AB94-4182-94D8-FAE759DFA723" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "Use Campus Context", @"Determine if the campus should be read from the campus context of the page.", 4, @"False", "DA3C1176-0458-484B-9DCC-E2C783360126" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "Date Range", @"Optional date range to filter the occurrences on.", 5, @",", "771C3B0B-DC24-4097-8F2B-AE1D161740E9" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "Max Occurrences", @"The maximum number of occurrences to show.", 6, @"5", "9A4E69FC-F6A8-45BD-BCD2-333AFF4C36A8" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "DetailPage", "Event Detail Page", @"The page to use for showing event details.", 7, @"", "A031EFB9-3A9A-48B7-AB85-CB5CD9C0B813" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Lava Template", "LavaTemplate", "Lava Template", @"The template to use when rendering event items.", 8, @"", "6A75D685-CF28-4B50-9D70-AE0AB3B6E880" );

            // Attribute for BlockType: Event Item Occurrence List By Audience Lava:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694745E8-4B1A-40E1-ABDF-09533C4BCD24", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 9, @"", "6DA7C599-D10D-4BFE-8C84-52E80151AE8D" );

            // Attribute for BlockType: Connection Opportunity Select:Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9EBF79-FDFB-4769-9E71-80BA066B8BE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create connection opportunities.", 1, @"9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "349ACB1F-6595-4283-A6DB-BE70F9411F7F" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9EBF79-FDFB-4769-9E71-80BA066B8BE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Opportunity Detail Page", "OpportunityDetailPage", "Opportunity Detail Page", @"Page to go to when an opportunity is selected.", 2, @"4FBCEB52-8892-4035-BDEA-112A494BE81F", "33F976E9-8572-4050-99A9-B16D656D453A" );

            // Attribute for BlockType: Connection Opportunity Select:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9EBF79-FDFB-4769-9E71-80BA066B8BE6", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 3, @"", "273F0A70-7E2F-42FD-B8E5-09EE2A8B3CD3" );

            // Attribute for BlockType: Connection Opportunity Select:Status Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9EBF79-FDFB-4769-9E71-80BA066B8BE6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "Status Template", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 4, @"
<div class='badge-legend expand-on-hover padding-r-md'>
    <span class='badge badge-info badge-circle js-legend-badge'>Assigned To You</span>
    <span class='badge badge-warning badge-circle js-legend-badge'>Unassigned Item</span>
    <span class='badge badge-critical badge-circle js-legend-badge'>Critical Status</span>
    <span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>
</div>", "8B3C693A-857B-4413-9D06-377A52FE7CAB" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9EBF79-FDFB-4769-9E71-80BA066B8BE6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Summary Template", "OpportunitySummaryTemplate", "Opportunity Summary Template", @"Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary and ConnectionOpportunity.", 5, @"
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>
", "C4B99765-4FAF-41D2-9BA2-F5D991F5FAAF" );

            // Attribute for BlockType: Connection Request Board:Max Cards per Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Cards per Column", "MaxCards", "Max Cards per Column", @"The maximum number of cards to display per column. This is to prevent performance issues caused by rendering too many cards at a time.", 0, @"100", "95F85CCB-8C9C-4B5B-A1FC-CC58A22A5FFC" );

            // Attribute for BlockType: Connection Request Board:Enable Request Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Request Security", "EnableRequestSecurity", "Enable Request Security", @"When enabled, the security column for request would be displayed.", 1, @"False", "4269B7DA-A005-4E7F-9608-CA7E64AEA53C" );

            // Attribute for BlockType: Connection Request Board:Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set a view profile button will show for each grid item.", 2, @"08DBD8A5-2C35-4146-B4A8-0F7652348B25", "AFC1C820-733B-4332-9DCD-E1E0929E2354" );

            // Attribute for BlockType: Connection Request Board:Workflow Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "Workflow Detail Page", @"Page used to display details about a workflow.", 3, @"BA547EED-5537-49CF-BD4E-C583D760788C", "1190E07E-D511-48C3-A0CD-F2CCBBE8A9E1" );

            // Attribute for BlockType: Connection Request Board:Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 4, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "D04F65C0-5F75-427B-A554-2A71A454117B" );

            // Attribute for BlockType: Connection Request Board:Status Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "Status Template", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 5, @"
<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>
    <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>
    <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
</div>", "5ADA93AC-B48D-4A77-AC6F-F24C9BCFD401" );

            // Attribute for BlockType: Connection Request Board:Connection Request Status Icons Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Connection Request Status Icons Template", "ConnectionRequestStatusIconsTemplate", "Connection Request Status Icons Template", @"Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.", 6, @"
<div class='board-card-pills'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='board-card-pill badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='board-card-pill badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='board-card-pill badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='board-card-pill badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
", "34E65333-B15E-4737-AD4E-1E5285D3A5EB" );

            // Attribute for BlockType: Connection Request Board:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page used to display group details.", 8, @"4E237286-B715-4109-A578-C1445EC02707", "28F2CC2F-BC61-49E0-A878-90772A7CCE22" );

            // Attribute for BlockType: Connection Request Board:SMS Link Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "SMS Link Page", "SmsLinkPage", "SMS Link Page", @"Page that will be linked for SMS enabled phones.", 9, @"2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "34580BA2-B21D-4546-B09D-C639E077EC59" );

            // Attribute for BlockType: Connection Request Board:Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The badges to display in this block.", 10, @"", "A091457A-5480-4199-BD3B-5BFC9907AAB3" );

            // Attribute for BlockType: Connection Request Board:Lava Heading Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Heading Template", "LavaHeadingTemplate", "Lava Heading Template", @"The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 11, @"", "BEA14CC4-0D3E-4CE0-8099-0790DB8C5908" );

            // Attribute for BlockType: Connection Request Board:Lava Badge Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Badge Bar", "LavaBadgeBar", "Lava Badge Bar", @"The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 12, @"", "522E9BDD-C1C6-49F8-8FE0-E71442E69E92" );

            // Attribute for BlockType: Connection Request Board:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 13, @"", "8D374149-F3DE-4E59-9ECA-88084EC16407" );

            // Attribute for BlockType: Connection Request Board:Limit to Assigned Connections
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F2FE25A-9D94-4B81-8783-EC32DD062913", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Assigned Connections", "OnlyShowAssigned", "Limit to Assigned Connections", @"When enabled, only requests assigned to the current person will be shown.", 14, @"False", "DF967B3D-2122-4FD4-869E-0FA344C21D4E" );

            // Attribute for BlockType: Calendar Event List:Enable Campus Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Filtering", "EnableCampusFiltering", "Enable Campus Filtering", @"If enabled then events will be filtered by campus to the campus context of the page and user.", 4, @"False", "5AE3682B-7804-482B-8552-F1D1069162C9" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Enable Campus Filtering Attribute for BlockType: Calendar Event List
            RockMigrationHelper.DeleteAttribute("5AE3682B-7804-482B-8552-F1D1069162C9");

            // Limit to Assigned Connections Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("DF967B3D-2122-4FD4-869E-0FA344C21D4E");

            // Connection Types Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("8D374149-F3DE-4E59-9ECA-88084EC16407");

            // Lava Badge Bar Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("522E9BDD-C1C6-49F8-8FE0-E71442E69E92");

            // Lava Heading Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("BEA14CC4-0D3E-4CE0-8099-0790DB8C5908");

            // Badges Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("A091457A-5480-4199-BD3B-5BFC9907AAB3");

            // SMS Link Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("34580BA2-B21D-4546-B09D-C639E077EC59");

            // Group Detail Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("28F2CC2F-BC61-49E0-A878-90772A7CCE22");

            // Connection Request Status Icons Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("34E65333-B15E-4737-AD4E-1E5285D3A5EB");

            // Status Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("5ADA93AC-B48D-4A77-AC6F-F24C9BCFD401");

            // Workflow Entry Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("D04F65C0-5F75-427B-A554-2A71A454117B");

            // Workflow Detail Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("1190E07E-D511-48C3-A0CD-F2CCBBE8A9E1");

            // Person Profile Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("AFC1C820-733B-4332-9DCD-E1E0929E2354");

            // Enable Request Security Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("4269B7DA-A005-4E7F-9608-CA7E64AEA53C");

            // Max Cards per Column Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("95F85CCB-8C9C-4B5B-A1FC-CC58A22A5FFC");

            // Opportunity Summary Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("C4B99765-4FAF-41D2-9BA2-F5D991F5FAAF");

            // Status Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("8B3C693A-857B-4413-9D06-377A52FE7CAB");

            // Connection Types Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("273F0A70-7E2F-42FD-B8E5-09EE2A8B3CD3");

            // Opportunity Detail Page Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("33F976E9-8572-4050-99A9-B16D656D453A");

            // Configuration Page Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("349ACB1F-6595-4283-A6DB-BE70F9411F7F");

            // Enabled Lava Commands Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6DA7C599-D10D-4BFE-8C84-52E80151AE8D");

            // Lava Template Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("6A75D685-CF28-4B50-9D70-AE0AB3B6E880");

            // Event Detail Page Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("A031EFB9-3A9A-48B7-AB85-CB5CD9C0B813");

            // Max Occurrences Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("9A4E69FC-F6A8-45BD-BCD2-333AFF4C36A8");

            // Date Range Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("771C3B0B-DC24-4097-8F2B-AE1D161740E9");

            // Use Campus Context Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("DA3C1176-0458-484B-9DCC-E2C783360126");

            // Campuses Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("1E1BD565-AB94-4182-94D8-FAE759DFA723");

            // Calendar Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("7F2125BB-7226-442D-B19D-2E56C0CA8944");

            // Audience Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("05BC9D02-6F59-437F-9D18-CE7217F800A7");

            // List Title Attribute for BlockType: Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteAttribute("62A54080-911A-42B1-A5F1-9C60A97456E6");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("288E61FE-D831-49CB-941D-6832FC7172BE");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("7DD08D0D-DBD8-474D-9F56-C725C6908EA7");

            // Enabled Lava Commands Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("7CEFD97E-53A4-4692-85FC-865D1BCEA7EF");

            // Template Attribute for BlockType: Communication View
            RockMigrationHelper.DeleteAttribute("A2C3BF0B-3D23-4A77-B509-2BCA8F155011");

            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute("DEB2DDD5-F4D8-4C6A-A989-AF05C90DDD0C");

            // core.CustomActionsConfigs Attribute for BlockType: Data View Results
            RockMigrationHelper.DeleteAttribute("F7418932-8570-41CA-84A1-37F93DD7A6A2");

            // Limit to Assigned Connections Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("3E99D945-0E2F-4AD5-BC7D-DC61EA5883A6");

            // Connection Types Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("689CFB59-A545-4C7A-8EBD-DD8B9671E89D");

            // Lava Badge Bar Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("42B332C4-6B38-4053-953D-EFCF740DF27E");

            // Lava Heading Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("86142B15-47F2-4065-9B8A-3680B9FBBB20");

            // Badges Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("A79EBB5A-A1C8-4C98-91EC-B30CDB297A90");

            // SMS Link Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("3CCC4AE5-D078-48C5-B134-77CFBC8C675C");

            // Group Detail Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("3C2787AE-EE14-425D-B600-73A3429334F8");

            // Connection Request Status Icons Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("0CFEAEE1-533B-4DF8-ACD7-2C9642F261D6");

            // Status Template Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("D228323C-1283-4F53-B9B8-2BB77461AEA1");

            // Workflow Entry Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("0A5C274B-0EFB-4FEC-99DA-AFE08B5D68D6");

            // Workflow Detail Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("B39D65A0-7E24-4F09-8BA5-A7B8A20C50B2");

            // Person Profile Page Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("5D76DCA5-8FD4-485D-B026-B046FF49D70A");

            // Enable Request Security Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("3BBB0A22-4F17-4325-B3DE-AF37090F11E4");

            // Max Cards per Column Attribute for BlockType: Connection Request Board
            RockMigrationHelper.DeleteAttribute("8977B16A-791A-452D-95BD-8DF7DF9A8040");

            // Opportunity Summary Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("D042660B-00C2-440D-A700-B8B41398BD48");

            // Status Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("DD58A091-CBB2-4A09-BC1A-19A5DA7FD6C1");

            // Connection Types Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("9D1A73B7-79A9-43FB-A465-DE9A0BD61A09");

            // Opportunity Detail Page Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("4CB57D26-4848-4F24-BB5B-2D3C44ED9434");

            // Configuration Page Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute("C170AC54-47B3-4B25-A149-742627D254CE");

            // Delete BlockType Connection Request Board
            RockMigrationHelper.DeleteBlockType("5F2FE25A-9D94-4B81-8783-EC32DD062913"); // Connection Request Board

            // Delete BlockType Connection Opportunity Select
            RockMigrationHelper.DeleteBlockType("CE9EBF79-FDFB-4769-9E71-80BA066B8BE6"); // Connection Opportunity Select

            // Delete BlockType Event Item Occurrence List By Audience Lava
            RockMigrationHelper.DeleteBlockType("694745E8-4B1A-40E1-ABDF-09533C4BCD24"); // Event Item Occurrence List By Audience Lava

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("9D816E33-CA5A-49C8-82AF-204C2B68749E"); // Calendar Event Item Occurrence View

            // Delete BlockType Communication View
            RockMigrationHelper.DeleteBlockType("9D694503-94A2-42C8-B355-FFA45894E95A"); // Communication View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("221FF0B2-5B7C-4E15-A85E-C9B884ABE0CA"); // Structured Content View

        }
    
        /// <summary>
        /// SK: Fixed Typo on Notes Block Attribute
        /// </summary>
        private void FixNotesBlockAttributeTypo()
        {
            
            Sql( @"
                UPDATE 
                    [Attribute]
                SET [Description]='Should replies be automatically expanded?'
                WHERE
	                [Guid]='84E53A88-32D2-432C-8BB5-600BDBA10949'" );
        }

        /// <summary>
        /// ED: Remove the Content Channel Item List Mobile block
        /// </summary>
        private void RemoveContentChannelItemListMobileBlock()
        {
            Sql( @"DELETE FROM [dbo].[BlockType] WHERE [Guid] = '5A06FF57-DE19-423A-9E8A-CB71B69DD4FC'" );
        }

        /// <summary>
        /// MB: Update PublicApplicationRoot attribute.
        /// </summary>
        private void UpdateAttributeValues()
        {
            var currentValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}/assessments?{{ Person.ImpersonationParameter }}";

            var newValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}assessments?{{ Person.ImpersonationParameter }}";

            UpdateTableColumn( "AttributeValue", "Value", currentValue, newValue, 
                $"AND EXISTS(SELECT 1 FROM [Attribute] WHERE [AttributeValue].[AttributeId] = [Attribute].[Id] AND [Attribute].[Guid] = '{SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_BODY}')" );
        }

        /// <summary>
        /// MB: Update PublicApplicationRoot attribute.
        /// Supports the UpdateAttributeValues method
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="additionalWhere">The additional where.</param>
        private void UpdateTableColumn( string tableName, string columnName, string currentValue, string newValue, string additionalWhere = "" )
        {
            var normalizedColumn = RockMigrationHelper.NormalizeColumnCRLF( columnName );

            Sql( $@"UPDATE [{tableName}]
                    SET [{columnName}] = REPLACE({normalizedColumn}, '{currentValue}', '{newValue}')
                    WHERE {normalizedColumn} LIKE '%{currentValue}%'
                    {additionalWhere}" );
        }

        /// <summary>
        /// JE: New Block Template for Mobile Event Item Occurrence List By Audience
        /// </summary>
        private void BlockTemplateforMobileEventItemOccurrenceListByAudienceUp()
        {
            string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.AddOrUpdateTemplateBlock( Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE,
                "Mobile Event Item Occurrence List By Audience",
                string.Empty );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate( "323D1996-C27F-4B48-B0C7-82FDA440D950",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE,
                "Default",
                @"<StackLayout>
    <Label StyleClass=""h1,mb-4"" Text=""{{ ListTitle | Escape }}"" />
    {% for occurrence in EventItemOccurrences %}
    <Frame HasShadow=""false"" StyleClass=""calendar-event"">
        <StackLayout Spacing=""0"">
            <Label StyleClass=""calendar-event-title"" Text=""{{ occurrence.EventItem.Name | Escape }}"" />
            <Label StyleClass=""calendar-event-text text-sm"" Text=""({{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'M/d ' }})"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    
        {% if occurrence.EventItem.DetailsUrl != '' %}
            <Frame.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding OpenExternalBrowser}"" CommandParameter=""{{ occurrence.EventItem.DetailsUrl | Escape }}"" />
            </Frame.GestureRecognizers>
        {% elseif EventDetailPage != null %}
            <Frame.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ EventDetailPage }}?EventOccurrenceId={{ occurrence.Id }}"" />
            </Frame.GestureRecognizers>
        {% endif %}
    </Frame>
    {% endfor %}
</StackLayout>",
    STANDARD_ICON_SVG,
    "standard-template.svg",
    "image/svg+xml" );
        }
    
        /// <summary>
        /// JE: New Block Template for Mobile Event Item Occurrence List By Audience
        /// </summary>
        private void BlockTemplateforMobileEventItemOccurrenceListByAudienceDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "323D1996-C27F-4B48-B0C7-82FDA440D950" );
            RockMigrationHelper.DeleteTemplateBlock( Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE );
        }

        /// <summary>
        /// MP: Fix invalid FinancialPersonSavedAccount
        /// </summary>
        private void FixFinancialPersonSavedAccount()
        {
            // Delete NMI FinancialPersonSavedAccounts that don't have a ReferenceNumer or a GatewayPersonIdentifier
            // These would not be usable and would cause a 'The ccnumber field is required..' error 
            Sql( @"
                DELETE
                FROM [dbo].[FinancialPersonSavedAccount]
                WHERE [FinancialGatewayId] in (
	                SELECT [Id]
	                FROM [dbo].[FinancialGateway] fg
	                WHERE [EntityTypeId] IN (SELECT [Id] FROM [dbo].[EntityType] WHERE [Name] = 'Rock.NMI.Gateway')
	                )
	                AND ISNULL([ReferenceNumber], '') = '' AND ISNULL([GatewayPersonIdentifier], '') = ''" );
        }

        /// <summary>
        /// SK: Default apostrophe for name fields
        /// </summary>
        private void DefaultApostropheForNameFields()
        {
            Sql( @"
                UPDATE [Person]
                SET [FirstName]=Replace(FirstName,'’',''''),
	                [LastName]=Replace(LastName,'’',''''),
	                [NickName]=Replace(NickName,'’','''')" );
        }
    }
}
