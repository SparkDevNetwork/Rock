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
    public partial class CodeGenerated_20221214 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.AddBlock( true, "BDEE4E84-3A6F-4914-B5E8-F113C4889E1F".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"2D5E8A91-171F-4536-BE26-9F8AC032432B"); 

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "AttributeCategories", "Attribute Categories", @"The Attribute Categories to display attributes from.", 26, @"", "0F16D496-A242-464C-8490-304EABB8272C" );

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow Race to be optionally selected.", 4, @"Hide", "DB2B00CA-FB15-4735-B821-D82CF83106FF" );

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 5, @"Hide", "C821807E-9E64-432D-A863-A0391CEBE54B" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow race to be optionally selected.", 8, @"Hide", "3A523CCE-439C-4441-9919-6F397FDE09F0" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 9, @"Hide", "D8612BD5-F03D-4616-8AA7-E7BBF727D083" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow Race to be optionally selected.", 28, @"Hide", "453708FA-2298-4C85-A5F1-1203F3E81DCA" );

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 29, @"Hide", "5AC781BE-D60D-4BAA-901A-AA46BFBA33DD" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow race to be optionally selected.", 8, @"Hide", "82A2CD69-B974-4805-89E4-84BF2F95501E" );

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 9, @"Hide", "24D861FE-0046-4B1B-8698-8566B460B08D" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "RaceOption", "Race", @"Allow Race to be optionally selected.", 13, @"Hide", "0486C28B-5541-48CF-8470-7F4731BE6112" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "EthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 14, @"Hide", "02C50651-DE02-43BE-ACF1-0A720D6D3B4E" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Race", "ChildRaceOption", "Race", @"Allow Race to be optionally selected.", 9, @"Hide", "D5B9A502-91B8-4442-9E67-D289F9FCD0C8" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Ethnicity", "ChildEthnicityOption", "Ethnicity", @"Allow Ethnicity to be optionally selected.", 10, @"Hide", "03EC5379-9AB3-4507-B679-9E162052CFF8" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Core
            //   Attribute: View Reminders Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC59B6D6-5CA1-4367-9109-CDDC92357D35", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Reminders Page", "ViewRemindersPage", "View Reminders Page", @"The page where a person can view their reminders.", 0, @"E1736347-1D4F-42A6-8EC4-7595286054A6", "90AA9A0B-E958-40A8-B7FA-2D641E7DEF62" );

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Core
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC59B6D6-5CA1-4367-9109-CDDC92357D35", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Reminder Page", "EditReminderPage", "Edit Reminder Page", @"The page where a person can edit a reminder.", 1, @"2640FF34-DCC9-4604-9C5B-9E2DA590D5A7", "A200CB6E-BFD1-450C-9073-32C7BBF37B04" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Edit Reminder Page", "EditReminderPage", "Edit Reminder Page", @"The page where a person can edit a reminder.", 1, @"2640FF34-DCC9-4604-9C5B-9E2DA590D5A7", "7D2AC833-E871-45FA-B89D-52FE2E0CF671" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "92BAF1CF-E729-4502-A269-71B40CDC8580" );

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8DC018-C702-4A23-81BA-DF9DD6008CB6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D7708003-E0C9-4547-9CFB-FF6C434D540D" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("2D5E8A91-171F-4536-BE26-9F8AC032432B","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
            RockMigrationHelper.UpdateFieldType("Reminder Type","","Rock","Rock.Field.Types.ReminderTypeFieldType","94A5DF3C-A7E0-451E-9DBA-86A6CFD5DF70");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Attribute Categories
            RockMigrationHelper.DeleteAttribute("0F16D496-A242-464C-8490-304EABB8272C");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("D8612BD5-F03D-4616-8AA7-E7BBF727D083");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("3A523CCE-439C-4441-9919-6F397FDE09F0");

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("C821807E-9E64-432D-A863-A0391CEBE54B");

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("DB2B00CA-FB15-4735-B821-D82CF83106FF");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("03EC5379-9AB3-4507-B679-9E162052CFF8");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("D5B9A502-91B8-4442-9E67-D289F9FCD0C8");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("02C50651-DE02-43BE-ACF1-0A720D6D3B4E");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("0486C28B-5541-48CF-8470-7F4731BE6112");

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("D7708003-E0C9-4547-9CFB-FF6C434D540D");

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("92BAF1CF-E729-4502-A269-71B40CDC8580");

            // Attribute for BlockType
            //   BlockType: Reminder List
            //   Category: Core
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.DeleteAttribute("7D2AC833-E871-45FA-B89D-52FE2E0CF671");

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("5AC781BE-D60D-4BAA-901A-AA46BFBA33DD");

            // Attribute for BlockType
            //   BlockType: Public Profile Edit
            //   Category: CMS
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("453708FA-2298-4C85-A5F1-1203F3E81DCA");

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Ethnicity
            RockMigrationHelper.DeleteAttribute("24D861FE-0046-4B1B-8698-8566B460B08D");

            // Attribute for BlockType
            //   BlockType: Rapid Attendance Entry
            //   Category: Check-in
            //   Attribute: Race
            RockMigrationHelper.DeleteAttribute("82A2CD69-B974-4805-89E4-84BF2F95501E");

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Core
            //   Attribute: Edit Reminder Page
            RockMigrationHelper.DeleteAttribute("A200CB6E-BFD1-450C-9073-32C7BBF37B04");

            // Attribute for BlockType
            //   BlockType: Reminder Links
            //   Category: Core
            //   Attribute: View Reminders Page
            RockMigrationHelper.DeleteAttribute("90AA9A0B-E958-40A8-B7FA-2D641E7DEF62");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("2D5E8A91-171F-4536-BE26-9F8AC032432B");
        }
    }
}
