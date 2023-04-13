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
    public partial class CodeGenerated_20221004 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0F2294B8-31BE-4962-854F-2A7CAB476BE1".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"0A2955B6-6958-46E2-9453-84FB0B97AF08"); 

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Requirements Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Requirements Header Lava Template", "RequirementsHeaderLavaTemplate", "Requirements Header Lava Template", @"Lava template for requirements header.", 4, @"{% include '~~/Assets/Lava/FundraisingParticipantRequirementsHeader.lava' %}", "ED16D169-AD0A-4F07-9F3F-C212338AC0DD" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Contributions Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contributions Header", "ContributionsHeader", "Contributions Header", @"The title for the Contributions header.", 10, @"Contributions", "FDE33A15-ECC5-49FC-B8E2-6EA1862AAF31" );

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 13, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "594E7FC9-FD03-4F06-A6EE-7E876C944902" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Are Requirements publicly hidden?
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Are Requirements publicly hidden?", "AreRequirementsPubliclyHidden", "Are Requirements publicly hidden?", @"Set to true to publicly show the group member's requirements.", 2, @"False", "924FFC5A-FF18-4EC1-ADE6-E5E9BCD3EBA4" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Is Requirement Summary Hidden?
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Requirement Summary Hidden?", "IsSummaryHidden", "Is Requirement Summary Hidden?", @"Set to true to hide the ""Summary"" field.", 3, @"False", "562D04DB-744C-48CC-8738-BA094F6FEA26" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Are Requirements refreshed when block is loaded?
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Are Requirements refreshed when block is loaded?", "AreRequirementsRefreshedOnLoad", "Are Requirements refreshed when block is loaded?", @"Set to true to refresh group member requirements when the block is loaded.", 4, @"False", "78A53B17-B4BA-4345-B984-6172E03F9B0E" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 5, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "75C4FE0F-58E1-4BE2-896B-9ADA0A0D4D4F" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Enable Communications
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Communications", "EnableCommunications", "Enable Communications", @"Enables the capability to send quick communications from the block.", 6, @"True", "9C78478B-A1D9-4F62-BB36-EB9F32AA3035" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Enable SMS
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable SMS", "EnableSMS", "Enable SMS", @"Allows SMS to be able to be sent from the communications if the individual has SMS enabled. Otherwise only email will be an option.", 7, @"True", "D657F24D-565F-4F19-B6D8-CB0D9A3F3121" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Append Organization Email Header/Footer
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Append Organization Email Header/Footer", "AppendHeaderFooter", "Append Organization Email Header/Footer", @"Will append the organization’s email header and footer to the email message.", 8, @"True", "A0513BD2-3A68-40A4-94F0-063AEF476048" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Allow Selecting 'From'
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Selecting 'From'", "AllowSelectingFrom", "Allow Selecting 'From'", @"Allows the 'from' of the communication to be changed to a different person.", 9, @"True", "65FCFD8F-0BD9-4285-AC2B-6CCB6654EC20" );

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 10, @"", "CEB4DE41-B112-4363-982C-A6B49CD0CD73" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Width
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Chart Width", "ChartWidth", "Chart Width", @"How many pixels wide should the chart be?", 3, @"1200", "3EC0DED2-AEBC-4540-B14A-D2364AF99759" );

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Footer Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B4E0128-BCDF-48BF-AEC9-85001169DA3E", "27718256-C1EB-4B1F-9B4B-AC53249F78DF", "Chart Footer Lava Template", "FooterLavaTemplate", "Chart Footer Lava Template", @"Format the labels for each legend item at the foot of the chart using Lava.", 5, @"<div class=""flow-legend"">
{% for stepItem in Steps %}
    <div class=""flow-key"">
        <span class=""color"" style=""background-color:{{stepItem.Color}};""></span>
        <span>{{forloop.index}}. {{stepItem.StepName}}</span>
    </div>
{% endfor %}
</div>", "84601E4F-26C2-4168-B158-64ADF9344777" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Show Medium Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Medium Preference", "ShowMediumPreference", "Show Medium Preference", @"If enabled then the medium preference will be shown.", 2, @"False", "44172A95-55AC-4FC0-8A2B-831FFBC68BE4" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Show Push Notification As Medium Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Push Notification As Medium Preference", "ShowPushNotificationsAsMediumPreference", "Show Push Notification As Medium Preference", @"If enabled then the push notification medium preference will be shown. Irrelevant if Show Medium Preference is disabled.", 3, @"False", "A904547F-C7A2-4C61-848F-07AA32C6ECE6" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Prayed For in Last x Minutes Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Prayed For in Last x Minutes Filter", "MinutesToFilter", "Prayed For in Last x Minutes Filter", @"An integer (minutes) that you can use to filter out recently prayed for items. Uses interaction data, so 'Create Interactions for Prayers' must be enabled. 0 to disable.", 9, @"0", "30356A40-E205-4963-A545-EC9AF37807AA" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Prayed For in Last x Minutes Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Prayed For in Last x Minutes Filter", "MinutesToFilter", "Prayed For in Last x Minutes Filter", @"An integer (minutes) that you can use to filter out recently prayed for items. Uses interaction data. 0 to disable.", 13, @"0", "99F00427-5638-4121-88A6-D349268DC1CB" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("0A2955B6-6958-46E2-9453-84FB0B97AF08","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Prayed For in Last x Minutes Filter
            RockMigrationHelper.DeleteAttribute("99F00427-5638-4121-88A6-D349268DC1CB");

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Prayed For in Last x Minutes Filter
            RockMigrationHelper.DeleteAttribute("30356A40-E205-4963-A545-EC9AF37807AA");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Show Push Notification As Medium Preference
            RockMigrationHelper.DeleteAttribute("A904547F-C7A2-4C61-848F-07AA32C6ECE6");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Show Medium Preference
            RockMigrationHelper.DeleteAttribute("44172A95-55AC-4FC0-8A2B-831FFBC68BE4");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Footer Lava Template
            RockMigrationHelper.DeleteAttribute("84601E4F-26C2-4168-B158-64ADF9344777");

            // Attribute for BlockType
            //   BlockType: Step Flow
            //   Category: Steps
            //   Attribute: Chart Width
            RockMigrationHelper.DeleteAttribute("3EC0DED2-AEBC-4540-B14A-D2364AF99759");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute("CEB4DE41-B112-4363-982C-A6B49CD0CD73");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Allow Selecting 'From'
            RockMigrationHelper.DeleteAttribute("65FCFD8F-0BD9-4285-AC2B-6CCB6654EC20");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Append Organization Email Header/Footer
            RockMigrationHelper.DeleteAttribute("A0513BD2-3A68-40A4-94F0-063AEF476048");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Enable SMS
            RockMigrationHelper.DeleteAttribute("D657F24D-565F-4F19-B6D8-CB0D9A3F3121");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Enable Communications
            RockMigrationHelper.DeleteAttribute("9C78478B-A1D9-4F62-BB36-EB9F32AA3035");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.DeleteAttribute("75C4FE0F-58E1-4BE2-896B-9ADA0A0D4D4F");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Are Requirements refreshed when block is loaded?
            RockMigrationHelper.DeleteAttribute("78A53B17-B4BA-4345-B984-6172E03F9B0E");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Is Requirement Summary Hidden?
            RockMigrationHelper.DeleteAttribute("562D04DB-744C-48CC-8738-BA094F6FEA26");

            // Attribute for BlockType
            //   BlockType: Group Member Detail
            //   Category: Groups
            //   Attribute: Are Requirements publicly hidden?
            RockMigrationHelper.DeleteAttribute("924FFC5A-FF18-4EC1-ADE6-E5E9BCD3EBA4");

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.DeleteAttribute("594E7FC9-FD03-4F06-A6EE-7E876C944902");

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Contributions Header
            RockMigrationHelper.DeleteAttribute("FDE33A15-ECC5-49FC-B8E2-6EA1862AAF31");

            // Attribute for BlockType
            //   BlockType: Fundraising Opportunity Participant
            //   Category: Fundraising
            //   Attribute: Requirements Header Lava Template
            RockMigrationHelper.DeleteAttribute("ED16D169-AD0A-4F07-9F3F-C212338AC0DD");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("0A2955B6-6958-46E2-9453-84FB0B97AF08");
        }
    }
}
