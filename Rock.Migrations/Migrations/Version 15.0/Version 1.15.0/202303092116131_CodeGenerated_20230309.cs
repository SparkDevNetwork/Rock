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
    public partial class CodeGenerated_20230309 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.CommunicationEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Communication.CommunicationEntry", "Communication Entry", "Rock.Blocks.Types.Mobile.Communication.CommunicationEntry, Rock, Version=1.15.0.13, Culture=neutral, PublicKeyToken=null", false, false, "9A952F9F-F619-4063-B1BB-CFB2E6983C01" );

            // Add/Update Mobile Block Type
            //   Name:Communication Entry
            //   Category:Mobile > Communication
            //   EntityType:Rock.Blocks.Types.Mobile.Communication.CommunicationEntry
            RockMigrationHelper.UpdateMobileBlockType( "Communication Entry", "Allows you to send communications to a set of recipients.", "Rock.Blocks.Types.Mobile.Communication.CommunicationEntry", "Mobile > Communication", "B0182DA2-82F7-4798-A48E-88EBE61F2109" );

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "72C6E10C-DB12-4FD5-8F11-3CC92F9F6D66".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "5373DCE8-0E4D-49BB-95DE-B724BCE33474" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Pledge Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pledge Term", "PledgeTerm", "Pledge Term", @"The Text to display as the pledge term on the pledge amount input label.", 12, @"Pledge", "924F375B-BA83-4D1E-8267-A8166FC60DB1" );

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 29, @"False", "8D615A5D-FDEA-4957-B050-04DA93DA9C83" );

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Allow Communication Preference Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Communication Preference Change", "AllowCommunicationPreferenceChange", "Allow Communication Preference Change", @"", 3, @"True", "D0556559-51DE-41EB-89DF-6CDAD937E4A6" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Include Inactive Members Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Include Inactive Members Filter", "ShowInactiveMembersFilter", "Show Include Inactive Members Filter", @"If enabled then the 'Include Inactive' filter option will be shown.", 4, @"False", "FB51E7CE-C987-4269-B22B-B48CE0F765AB" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Group Role Type Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Role Type Filter", "ShowGroupRoleTypeFilter", "Show Group Role Type Filter", @"If enabled then the 'Group Type Role' filter option will be shown.", 5, @"False", "F98CF893-5C82-46AA-A044-8CAAFB0DBD56" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Group Role Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Role Filter", "ShowGroupRoleFilter", "Show Group Role Filter", @"If enabled then the 'Group Role' filter option will be shown.", 6, @"True", "0CBCDB7C-CC81-4BFD-BEF8-1F18291D8B0E" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Gender Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender Filter", "ShowGenderFilter", "Show Gender Filter", @"If enabled then the 'Gender' filter option will be shown.", 7, @"False", "93F3D6EF-97F7-42B0-833F-A9CB49EBD2F3" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Child Groups Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Child Groups Filter", "ShowChildGroupsFilter", "Show Child Groups Filter", @"If enabled then the 'Child Groups' filter option will be shown.", 8, @"False", "FA15A633-0BF0-45E0-A602-AAC2E222E048" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Attendance Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Attendance Filter", "ShowAttendanceFilter", "Show Attendance Filter", @"If enabled then the 'Attendance' filter option will be shown.", 9, @"False", "78DD984C-A7F6-4754-9483-03225375C98E" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Attendance Filter Short Week Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Attendance Filter Short Week Range", "AttendanceFilterShortWeekRange", "Attendance Filter Short Week Range", @"Displays a filter option that gives a variety of different options for attendance based on x number of weeks.", 10, @"3", "7240E7E6-8502-4127-B3BE-8EB8365A1AAD" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Attendance Filter Long Week Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Attendance Filter Long Week Range", "AttendanceFilterLongWeekRange", "Attendance Filter Long Week Range", @"Displays a filter option that gives a variety of different options for attendance based on x number of weeks.", 11, @"12", "69586657-1240-4D83-A3E5-3E1AAC448B3A" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page to link to when user taps on a person listed in the 'Failed to Deliver' section. PersonGuid is passed in the query string.", 9, @"", "ACBB495B-3587-4D23-BCAC-B31BC94AD6A9" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ", 6, @"", "5F2744E5-214F-4621-B6C2-115187054311" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Enable Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Email", "EnableEmail", "Enable Email", @"When enabled, show email as a selectable communication transport.", 0, @"True", "74EB53DA-9692-4682-9DF7-D7A94AB1FDC7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Enable SMS
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable SMS", "EnableSms", "Enable SMS", @"When enabled, show SMS as a selectable communication transport.", 1, @"True", "3C23A843-8A2F-4DD4-933C-011CF642E582" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show From Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show From Name", "ShowFromName", "Show From Name", @"When enabled, a field will be shown to input From Name (email transport only).", 2, @"False", "E6788477-B6B2-46A7-B79E-26655A7C1117" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Reply To
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reply To", "ShowReplyTo", "Show Reply To", @"When enabled, a field will be shown to input Reply To (email transport only).", 3, @"False", "98AD50B6-5992-40EE-B422-790E8235C1D6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Send To Parents
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Send To Parents", "ShowSendToParents", "Show Send To Parents", @"When enabled, a toggle will show to enable an individual with the Age Classification of 'Child' to have the communication sent to their parents as well.", 4, @"False", "EF51D613-5AD8-4F1B-8410-0F7D0846AF18" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Is Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Bulk", "IsBulk", "Is Bulk", @"When enabled, the communication will be flagged as a bulk communication.", 5, @"False", "DF32B595-FD04-48A2-90FC-D2D86D933060" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show only personal SMS number", "ShowOnlyPersonalSmsNumber", "Show only personal SMS number", @"Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.", 7, @"False", "DED7D46C-106E-4C0D-8741-316B4A74336B" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide personal SMS numbers", "HidePersonalSmsNumbers", "Hide personal SMS numbers", @"Only SMS Numbers that are not associated with a person. The numbers without an Assigned To Person value.", 8, @"False", "92B3B83F-3A23-40C9-8A62-8FBE91F99CD1" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "5373DCE8-0E4D-49BB-95DE-B724BCE33474", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Transaction Entry (V2)
            //   Category: Finance
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "8D615A5D-FDEA-4957-B050-04DA93DA9C83" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Attendance Filter Long Week Range
            RockMigrationHelper.DeleteAttribute( "69586657-1240-4D83-A3E5-3E1AAC448B3A" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Attendance Filter Short Week Range
            RockMigrationHelper.DeleteAttribute( "7240E7E6-8502-4127-B3BE-8EB8365A1AAD" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Attendance Filter
            RockMigrationHelper.DeleteAttribute( "78DD984C-A7F6-4754-9483-03225375C98E" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Child Groups Filter
            RockMigrationHelper.DeleteAttribute( "FA15A633-0BF0-45E0-A602-AAC2E222E048" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Gender Filter
            RockMigrationHelper.DeleteAttribute( "93F3D6EF-97F7-42B0-833F-A9CB49EBD2F3" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Group Role Filter
            RockMigrationHelper.DeleteAttribute( "0CBCDB7C-CC81-4BFD-BEF8-1F18291D8B0E" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Group Role Type Filter
            RockMigrationHelper.DeleteAttribute( "F98CF893-5C82-46AA-A044-8CAAFB0DBD56" );

            // Attribute for BlockType
            //   BlockType: Group Member List
            //   Category: Mobile > Groups
            //   Attribute: Show Include Inactive Members Filter
            RockMigrationHelper.DeleteAttribute( "FB51E7CE-C987-4269-B22B-B48CE0F765AB" );

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Allow Communication Preference Change
            RockMigrationHelper.DeleteAttribute( "D0556559-51DE-41EB-89DF-6CDAD937E4A6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute( "ACBB495B-3587-4D23-BCAC-B31BC94AD6A9" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Hide personal SMS numbers
            RockMigrationHelper.DeleteAttribute( "92B3B83F-3A23-40C9-8A62-8FBE91F99CD1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show only personal SMS number
            RockMigrationHelper.DeleteAttribute( "DED7D46C-106E-4C0D-8741-316B4A74336B" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.DeleteAttribute( "5F2744E5-214F-4621-B6C2-115187054311" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Is Bulk
            RockMigrationHelper.DeleteAttribute( "DF32B595-FD04-48A2-90FC-D2D86D933060" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Send To Parents
            RockMigrationHelper.DeleteAttribute( "EF51D613-5AD8-4F1B-8410-0F7D0846AF18" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show Reply To
            RockMigrationHelper.DeleteAttribute( "98AD50B6-5992-40EE-B422-790E8235C1D6" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Show From Name
            RockMigrationHelper.DeleteAttribute( "E6788477-B6B2-46A7-B79E-26655A7C1117" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Enable SMS
            RockMigrationHelper.DeleteAttribute( "3C23A843-8A2F-4DD4-933C-011CF642E582" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: Enable Email
            RockMigrationHelper.DeleteAttribute( "74EB53DA-9692-4682-9DF7-D7A94AB1FDC7" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Pledge Term
            RockMigrationHelper.DeleteAttribute( "924F375B-BA83-4D1E-8267-A8166FC60DB1" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5373DCE8-0E4D-49BB-95DE-B724BCE33474" );

            // Delete BlockType 
            //   Name: Communication Entry
            //   Category: Mobile > Communication
            //   Path: -
            //   EntityType: Communication Entry
            RockMigrationHelper.DeleteBlockType( "B0182DA2-82F7-4798-A48E-88EBE61F2109" );
        }
    }
}
