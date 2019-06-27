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
    public partial class Rollup_0219 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            SetSiteWithRockOriginalTheme();
            UpdateCheckScannerURL();
            UpdateDefinedType_COMMUNICATION_SMS_FROM_Up();
            FixGroupAdministratorLavaTemplate();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            UpdateCheckScannerURLDown();
        }

        /// <summary>
        /// JM:Update Migration Pre-Alpha reload of block and filed types
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Account Entry:Show Campus Selector
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Selector", "ShowCampusSelector", "", @"Allows selection of primary campus.", 20, @"False", "5034A501-F43C-459F-A755-FCD94C93DD90" );
            // Attrib for BlockType: Page Menu:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this content channel item block.", 1, @"", "EF10B2F9-93E5-426F-8D43-8C020224670F" );
            // Attrib for BlockType: Public Profile Edit:Phone Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types", "PhoneTypes", "", @"The types of phone numbers to display / edit.", 6, @"AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303", "71DC48B6-2C3D-4A61-B13A-A53F403A21B9" );
            // Attrib for BlockType: Public Profile Edit:Required Adult Phone Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Required Adult Phone Types", "RequiredAdultPhoneTypes", "", @"The phone numbers that are required when editing an adult record.", 7, @"", "E8BA51BB-3ADE-4C05-847B-B8869C3B863E" );
            // Attrib for BlockType: Public Profile Edit:Show Phone Numbers
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Phone Numbers", "ShowPhoneNumbers", "", @"Allows hiding the phone numbers.", 5, @"False", "18ED0255-7D50-49FE-98C1-EF7BD5D8D725" );
            // Attrib for BlockType: Public Profile Edit:Require Adult Email Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Adult Email Address", "RequireAdultEmailAddress", "", @"Require an email address on adult records", 8, @"True", "2FB10621-DCE2-4006-97AB-9BA51404C1DA" );
            // Attrib for BlockType: Public Profile Edit:Show Campus Selector
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Selector", "ShowCampusSelector", "", @"Allows selection of primary campus.", 12, @"False", "276A3B84-7C2F-433C-8F4D-B8906C8E47A5" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Navigation Pane
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Navigation Pane", "ShowNavPane", "", @"Determines whether the navigation pane in the embedded report should be shown.", 0, @"True", "76D53429-896C-470A-86C0-0C6975C4E69B" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Fullsize Button
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Fullsize Button", "ShowFullsizeBtn", "", @"Determines whether the fullsize button should be shown.", 0, @"True", "66C3016D-CF2D-45DB-96C2-7DFBD6C2DF2F" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Right Pane
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Right Pane", "ShowRightPane", "", @"Determines whether the right pane in the embedded report should be shown.", 0, @"True", "2B9DAC59-2B0D-4501-98A7-C29C6DBFBA36" );
        }

        /// <summary>
        /// JM:Update Migration Pre-Alpha reload of block and filed types
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Account Entry:Show Campus Selector
            RockMigrationHelper.DeleteAttribute( "5034A501-F43C-459F-A755-FCD94C93DD90" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Right Pane
            RockMigrationHelper.DeleteAttribute( "2B9DAC59-2B0D-4501-98A7-C29C6DBFBA36" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Fullsize Button
            RockMigrationHelper.DeleteAttribute( "66C3016D-CF2D-45DB-96C2-7DFBD6C2DF2F" );
            // Attrib for BlockType: Power Bi Report Viewer:Show Navigation Pane
            RockMigrationHelper.DeleteAttribute( "76D53429-896C-470A-86C0-0C6975C4E69B" );
            // Attrib for BlockType: Public Profile Edit:Show Campus Selector
            RockMigrationHelper.DeleteAttribute( "276A3B84-7C2F-433C-8F4D-B8906C8E47A5" );
            // Attrib for BlockType: Public Profile Edit:Require Adult Email Address
            RockMigrationHelper.DeleteAttribute( "2FB10621-DCE2-4006-97AB-9BA51404C1DA" );
            // Attrib for BlockType: Public Profile Edit:Required Adult Phone Types
            RockMigrationHelper.DeleteAttribute( "E8BA51BB-3ADE-4C05-847B-B8869C3B863E" );
            // Attrib for BlockType: Public Profile Edit:Phone Types
            RockMigrationHelper.DeleteAttribute( "71DC48B6-2C3D-4A61-B13A-A53F403A21B9" );
            // Attrib for BlockType: Public Profile Edit:Show Phone Numbers
            RockMigrationHelper.DeleteAttribute( "18ED0255-7D50-49FE-98C1-EF7BD5D8D725" );
            // Attrib for BlockType: Page Menu:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "EF10B2F9-93E5-426F-8D43-8C020224670F" );
        }

        /// <summary>
        /// SK: Updated Site with RockOriginal Theme to Rock
        /// </summary>
        private void SetSiteWithRockOriginalTheme()
        {
            Sql( @"UPDATE
	[Site]
SET 
	[Theme]='Rock'
WHERE
	[Theme]='RockOriginal'" );
        }

        /// <summary>
        /// NA: Update the CheckScanner download link under External Applications
        /// </summary>
        private void UpdateCheckScannerURL()
        {
            Sql( @"UPDATE[AttributeValue]
    SET[Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.9.0/checkscanner.exe'
    WHERE[Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
        }

        /// <summary>
        /// SK: Updated Site with RockOriginal Theme to Rock
        /// </summary>
        private void UpdateCheckScannerURLDown()
        {
            Sql( @"    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.8.0/checkscanner.exe' 
    WHERE [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
        }

        /// <summary>
        /// ED:Updates the description for the SMS Phone number defined type
        /// </summary>
        private void UpdateDefinedType_COMMUNICATION_SMS_FROM_Up()
        {
            RockMigrationHelper.AddDefinedType(
                   "Communication",
                   "SMS Phone Numbers",
                   "SMS numbers that can be used to send text messages from. This is usually a phone number or short code that has been set up with your SMS provider. Providing a response recipient will send replies straight to the individual's mobile phone if 'Enable Response Recipient Forwarding' is set to Yes.",
                   Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM );
        }

        /// <summary>
        /// NA:Fixes the Group Administrator feature's Lava templates that were missed with the original migration
        /// due to CRLF issues.
        /// </summary>
        public void FixGroupAdministratorLavaTemplate()
        {
            string lavaTemplate = @"{% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}";

            string newLavaTemplate = @"{% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}
            {% if Group.GroupType.ShowAdministrator and Group.GroupAdministratorPersonAlias != null and Group.GroupAdministratorPersonAlias != '' %}

            <dt> {{ Group.GroupType.AdministratorTerm }}</dt>

            <dd>{{ Group.GroupAdministratorPersonAlias.Person.FullName }}</dd>
            {% endif %}";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "GroupViewLavaTemplate" );

            Sql( $@"
            UPDATE [GroupType] 
            SET [GroupViewLavaTemplate] = REPLACE( {targetColumn}
                ,'{lavaTemplate}'
                ,'{newLavaTemplate}' )
            WHERE {targetColumn} NOT LIKE '%{newLavaTemplate}%'"
            );
        }
    }
}
