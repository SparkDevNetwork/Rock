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
    public partial class Rollup_0512 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixMarriedDefinedValue();
            AddOnlineWatcherKnownRelationshipType();
            UpdateCSSClassesInMobileTemplates();
            MobileEventDetailBlock();
            UpdateCardLavaTemplateInStepType();
            AzureCloudStorageEntityType();
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
            RockMigrationHelper.UpdateBlockType("Attendance Self Entry","Allows quick self service attendance recording.","~/Blocks/CheckIn/AttendanceSelfEntry.ascx","Check-in","DAFB3699-3AC3-4930-A7F9-424C562F6474");
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "D41AB62B-1CE4-431B-9998-AC6E4507B1ED");
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "E27B9630-EE7D-4D0B-9BC6-5008E43A13DD");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E27B9630-EE7D-4D0B-9BC6-5008E43A13DD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "13B48510-DCEB-443F-A8C0-1D074BDEC41F" );
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E27B9630-EE7D-4D0B-9BC6-5008E43A13DD", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "15193ECD-1C6C-47C0-9E52-585B9F35B900" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Configuration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Configuration", "CheckinConfiguration", "Check-in Configuration", @"This will be the group type that we will use to determine where to check them in.", 0, @"77713830-AE5E-4B1A-94FA-E145DFF85035", "9496A9DF-9CD7-4508-825F-5CF50C948056" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Shown
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Shown", "PrimaryPersonBirthdayShown", "Primary Person Birthday Shown", @"Should birthday be displayed for primary person?", 1, @"True", "FF073A60-11BD-46CC-BBDF-8471A8246457" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Required", "PrimaryPersonBirthdayRequired", "Primary Person Birthday Required", @"Determine if birthday for primary person is required.", 2, @"False", "41FAE0E7-8FEB-44E5-AD8C-0264F78D18BB" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Shown
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Shown", "PrimaryPersonAddressShown", "Primary Person Address Shown", @"Should address be displayed for primary person?", 3, @"True", "8D6A8779-3B2A-48D8-BC8E-3827FC018290" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Required", "PrimaryPersonAddressRequired", "Primary Person Address Required", @"Determine if address for primary person is required.", 4, @"False", "5138BF6C-D726-443E-A4B6-01D28142E57E" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Shown
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Shown", "PrimaryPersonMobilePhoneShown", "Primary Person Mobile Phone Shown", @"Should mobile phone be displayed for primary person?", 5, @"True", "3B2AEED2-BC85-4865-9D9B-5F94C7B900C7" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Required", "PrimaryPersonMobilePhoneRequired", "Primary Person Mobile Phone Required", @"Determine if mobile phone for primary person is required.", 6, @"False", "20A68EAE-9297-41CB-A64A-95F9508C129C" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Shown
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Shown", "OtherPersonBirthdayShown", "Other Person Birthday Shown", @"Should birthday be displayed for other person?", 7, @"True", "78EC58D8-832E-4F62-8ABF-5823977446C3" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Required", "OtherPersonBirthdayRequired", "Other Person Birthday Required", @"Determine if birthday for other person is required.", 8, @"False", "2A8CC5A3-0989-49B3-9126-C8EFF6B722F1" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Shown
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Shown", "OtherPersonMobilePhoneShown", "Other Person Mobile Phone Shown", @"Should mobile phone be displayed for other person?", 9, @"True", "003CB484-2C4D-42FE-A2A2-4DBACA1F4586" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Required", "OtherPersonMobilePhoneRequired", "Other Person Mobile Phone Required", @"Determine if mobile phone for other person is required.", 10, @"False", "69936C72-157B-4313-9417-7079082B6393" );
            // Attrib for BlockType: Attendance Self Entry:Known Relationship Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Known Relationship Types", "KnownRelationshipTypes", "Known Relationship Types", @"A checkbox list of Known Relationship types that should be included in the Relation dropdown.", 11, @"6b05f48e-5235-45de-970e-fe145bd28e1d", "33FDEE08-7DE2-4691-AF49-97D286C9EB79" );
            // Attrib for BlockType: Attendance Self Entry:Redirect URL
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Redirect URL", "RedirectURL", "Redirect URL", @"The URL to redirect the individual to when they check-in.", 12, @"", "C177D017-C4D6-482E-91D1-BCC1CD764FF2" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Button Text", "CheckinButtonText", "Check-in Button Text", @"The text that should be shown on the check-in button.", 13, @"Check-in", "CF157EE1-F198-4A44-8335-025A1492B3B0" );
            // Attrib for BlockType: Attendance Self Entry:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"The optional workflow type to launch when a person is checked in. The primary person will be passed to the workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: GroupId, LocationId (if found), ScheduleId (if found), CheckedInPersonIds. (NOTE: If you want a workflow 'form' type of workflow use the Redirect URL setting instead.)", 14, @"", "EBFC4C83-9247-45A3-823B-C5DEB69E686A" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 1 Title", "UnknownIndividualPanel1Title", "Unknown Individual Panel 1 Title", @"The title to display on the primary watcher panel.", 15, @"Tell Us a Little About You...", "B24BD6F9-A84C-45B3-B243-53A6BA459E43" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Intro Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 1 Intro Text", "UnknownIndividualPanel1IntroText", "Unknown Individual Panel 1 Intro Text", @"The intro text to display on the primary watcher panel.", 16, @" We love to learn a little about you so that we can best serve you and your family.", "E546CD5B-F449-4DDD-90D2-F55F110B73F9" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 2 Title", "UnknownIndividualPanel2Title", "Unknown Individual Panel 2 Title", @"The title to display on the other watcher panel.", 17, @"Who Else Is Joining You?", "01CA755D-1DA1-4DC1-B1E6-2AF1CAB7EA10" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Intro Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 2 Intro Text", "UnknownIndividualPanel2IntroText", "Unknown Individual Panel 2 Intro Text", @"The intro text to display on the other watcher panel.", 18, @"We'd love to know more about others watching with you.", "D3503C92-B713-46BD-87F3-DB49ABE1F2A8" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 3 Title", "UnknownIndividualPanel3Title", "Unknown Individual Panel 3 Title", @"The title to display on the account panel.", 19, @"Would You Like to Create An Account?", "0383CC27-F0BC-4F17-8B90-3E50B3DF90FF" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Intro Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 3 Intro Text", "UnknownIndividualPanel3IntroText", "Unknown Individual Panel 3 Intro Text", @"The intro text to display on the account panel.", 20, @"Creating an account will help you to connect on our website.", "577B364A-281D-41DE-83C4-1170EF7766F7" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 1 Title", "KnownIndividualPanel1Title", "Known Individual Panel 1 Title", @"The title to display on the known individual panel.", 21, @"Great to see you {{ CurrentPerson.NickName }}!", "92EAE8A1-181E-4A1F-8851-748F5711D61C" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Intro Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 1 Intro Text", "KnownIndividualPanel1IntroText", "Known Individual Panel 1 Intro Text", @"The intro text to display on the known individual panel.", 22, @"We'd love to know who is watching with you today.", "BC546B14-7110-46E9-A82C-9C6F87B7FB97" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 2 Title", "KnownIndividualPanel2Title", "Known Individual Panel 2 Title", @"The title to display on the success panel.", 23, @"Thanks for Connecting!", "286F2A1C-6713-4B62-B93D-852A99E42F19" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Intro Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DAFB3699-3AC3-4930-A7F9-424C562F6474", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 2 Intro Text", "KnownIndividualPanel2IntroText", "Known Individual Panel 2 Intro Text", @"The intro text to display on the success panel.", 24, @"Thank you for connecting with us today. We hope that your enjoy the service!", "E229A866-065F-4127-A82D-BE81EA9BE887" );
            // Attrib for BlockType: Workflow Entry:Scan Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Scan Mode", "ScanMode", "Scan Mode", @"", 5, @"0", "370F3617-CE26-4FA8-96CA-26B82E4D4F15" );
            // Attrib for BlockType: Workflow Entry:Scan Attribute
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Scan Attribute", "ScanAttribute", "Scan Attribute", @"", 6, @"", "C87284BE-5BDE-4F14-B450-5FF50E976047" );
            // Attrib for BlockType: Prayer Session Setup:Title Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A3B0D13-FC32-4354-A224-9D450F860BE9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Text", "TitleText", "Title Text", @"The title to display at the top of the block. Leave blank to hide.", 0, @"Let's Pray", "3D65A824-5C8C-4580-9E9B-5224A03D82FD" );
            // Attrib for BlockType: Prayer Session Setup:Instruction Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A3B0D13-FC32-4354-A224-9D450F860BE9", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Instruction Text", "InstructionText", "Instruction Text", @"Instructions to help the individual know how to use the block.", 1, @"Praying for others is part of what transforms a community into a family. Please select the categories of prayer you would like to pray for.", "010BF18D-FA2E-4110-B393-96FBB6DFF02F" );
            // Attrib for BlockType: Bulk Update:Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Batch Size", "BatchSize", "Batch Size", @"The maximum number of items in each processing batch. If not specified, this value will be automatically determined.", 4, @"0", "CF361018-1F2B-40F9-AD5D-4D34D989B70C" );
            // Attrib for BlockType: Personal Step List:Show Campus Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Column", "ShowCampusColumn", "Show Campus Column", @"Should the campus should be shown on the grid and card display?", 5, @"True", "5297FCCB-722B-4D96-82F8-480B4D938E89" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8C1A5EE4-296B-45F4-B998-186B418D1EA9" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "783745C2-FF68-4BF9-AA14-95872C2A004C" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E4B2F442-8207-4949-A334-D9293C310EC2" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E1D9DE73-C066-4677-B633-70A25F796760" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("E1D9DE73-C066-4677-B633-70A25F796760");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("E4B2F442-8207-4949-A334-D9293C310EC2");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("783745C2-FF68-4BF9-AA14-95872C2A004C");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("8C1A5EE4-296B-45F4-B998-186B418D1EA9");
            // Attrib for BlockType: Personal Step List:Show Campus Column
            RockMigrationHelper.DeleteAttribute("5297FCCB-722B-4D96-82F8-480B4D938E89");
            // Attrib for BlockType: Bulk Update:Batch Size
            RockMigrationHelper.DeleteAttribute("CF361018-1F2B-40F9-AD5D-4D34D989B70C");
            // Attrib for BlockType: Prayer Session Setup:Instruction Text
            RockMigrationHelper.DeleteAttribute("010BF18D-FA2E-4110-B393-96FBB6DFF02F");
            // Attrib for BlockType: Prayer Session Setup:Title Text
            RockMigrationHelper.DeleteAttribute("3D65A824-5C8C-4580-9E9B-5224A03D82FD");
            // Attrib for BlockType: Workflow Entry:Scan Attribute
            RockMigrationHelper.DeleteAttribute("C87284BE-5BDE-4F14-B450-5FF50E976047");
            // Attrib for BlockType: Workflow Entry:Scan Mode
            RockMigrationHelper.DeleteAttribute("370F3617-CE26-4FA8-96CA-26B82E4D4F15");
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Intro Text
            RockMigrationHelper.DeleteAttribute("E229A866-065F-4127-A82D-BE81EA9BE887");
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Title
            RockMigrationHelper.DeleteAttribute("286F2A1C-6713-4B62-B93D-852A99E42F19");
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Intro Text
            RockMigrationHelper.DeleteAttribute("BC546B14-7110-46E9-A82C-9C6F87B7FB97");
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Title
            RockMigrationHelper.DeleteAttribute("92EAE8A1-181E-4A1F-8851-748F5711D61C");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Intro Text
            RockMigrationHelper.DeleteAttribute("577B364A-281D-41DE-83C4-1170EF7766F7");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Title
            RockMigrationHelper.DeleteAttribute("0383CC27-F0BC-4F17-8B90-3E50B3DF90FF");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Intro Text
            RockMigrationHelper.DeleteAttribute("D3503C92-B713-46BD-87F3-DB49ABE1F2A8");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Title
            RockMigrationHelper.DeleteAttribute("01CA755D-1DA1-4DC1-B1E6-2AF1CAB7EA10");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Intro Text
            RockMigrationHelper.DeleteAttribute("E546CD5B-F449-4DDD-90D2-F55F110B73F9");
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Title
            RockMigrationHelper.DeleteAttribute("B24BD6F9-A84C-45B3-B243-53A6BA459E43");
            // Attrib for BlockType: Attendance Self Entry:Workflow
            RockMigrationHelper.DeleteAttribute("EBFC4C83-9247-45A3-823B-C5DEB69E686A");
            // Attrib for BlockType: Attendance Self Entry:Check-in Button Text
            RockMigrationHelper.DeleteAttribute("CF157EE1-F198-4A44-8335-025A1492B3B0");
            // Attrib for BlockType: Attendance Self Entry:Redirect URL
            RockMigrationHelper.DeleteAttribute("C177D017-C4D6-482E-91D1-BCC1CD764FF2");
            // Attrib for BlockType: Attendance Self Entry:Known Relationship Types
            RockMigrationHelper.DeleteAttribute("33FDEE08-7DE2-4691-AF49-97D286C9EB79");
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("69936C72-157B-4313-9417-7079082B6393");
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Shown
            RockMigrationHelper.DeleteAttribute("003CB484-2C4D-42FE-A2A2-4DBACA1F4586");
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Required
            RockMigrationHelper.DeleteAttribute("2A8CC5A3-0989-49B3-9126-C8EFF6B722F1");
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Shown
            RockMigrationHelper.DeleteAttribute("78EC58D8-832E-4F62-8ABF-5823977446C3");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("20A68EAE-9297-41CB-A64A-95F9508C129C");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Shown
            RockMigrationHelper.DeleteAttribute("3B2AEED2-BC85-4865-9D9B-5F94C7B900C7");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Required
            RockMigrationHelper.DeleteAttribute("5138BF6C-D726-443E-A4B6-01D28142E57E");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Shown
            RockMigrationHelper.DeleteAttribute("8D6A8779-3B2A-48D8-BC8E-3827FC018290");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Required
            RockMigrationHelper.DeleteAttribute("41FAE0E7-8FEB-44E5-AD8C-0264F78D18BB");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Shown
            RockMigrationHelper.DeleteAttribute("FF073A60-11BD-46CC-BBDF-8471A8246457");
            // Attrib for BlockType: Attendance Self Entry:Check-in Configuration
            RockMigrationHelper.DeleteAttribute("9496A9DF-9CD7-4508-825F-5CF50C948056");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.DeleteAttribute("15193ECD-1C6C-47C0-9E52-585B9F35B900");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.DeleteAttribute("13B48510-DCEB-443F-A8C0-1D074BDEC41F");
            RockMigrationHelper.DeleteBlockType("DAFB3699-3AC3-4930-A7F9-424C562F6474"); // Attendance Self Entry
            RockMigrationHelper.DeleteBlockType("E27B9630-EE7D-4D0B-9BC6-5008E43A13DD"); // Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("D41AB62B-1CE4-431B-9998-AC6E4507B1ED"); // Structured Content View
        }
    
        /// <summary>
        /// GJ: Married Defined Value Typo
        /// </summary>
        private void FixMarriedDefinedValue()
        {
            Sql( @"
                UPDATE [DefinedValue]
                SET [Description]=N'Used when an individual is married.'
                WHERE ([Description] = 'Used with an individual is married.'
                    AND [Guid] = '5FE5A540-7D9F-433E-B47E-4229D1472248')" );
        }

        /// <summary>
        /// Items needed for the Attendance Self Entry block.
        /// </summary>
        private void AddOnlineWatcherKnownRelationshipType()
        {
            RockMigrationHelper.AddGroupTypeRole( "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF", "Online Watcher", "", 0, null, null, "6B05F48E-5235-45DE-970E-FE145BD28E1D", true );

            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Attendance Self Entry", "", "7863E418-A2C9-450B-943A-C3F34994C28E", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Attendance Self Entry", "Allows quick self service attendance recording.", "~/Blocks/CheckIn/AttendanceSelfEntry.ascx", "Check -in", "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3" );
            // Add Block to Page: Attendance Self Entry Site: External Website  
            RockMigrationHelper.AddBlock( true, "7863E418-A2C9-450B-943A-C3F34994C28E".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3".AsGuid(), "Attendance Self Entry","Main",@"",@"",0,"C7A5EC8D-6BDB-4E60-ACE4-AAB8BD77166F");
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Shown", "PrimaryPersonAddressShown", "Primary Person Address Shown", @"Should address be displayed for primary person?", 3, @"True", "0395939D-1621-4C8C-A12F-CE42589EE50C" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Configuration            
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Configuration", "CheckinConfiguration", "Check-in Configuration", @"This will be the group type that we will use to determine where to check them in.", 0, @"77713830-AE5E-4B1A-94FA-E145DFF85035", "F63AD083-BAA7-4CB2-A3F0-DD3B0060921F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Shown      
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Shown", "PrimaryPersonBirthdayShown", "Primary Person Birthday Shown", @"Should birthday be displayed for primary person?", 1, @"True", "A81792AB-15B1-400F-905F-61D6A34BBF7B" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Birthday Required      
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Birthday Required", "PrimaryPersonBirthdayRequired", "Primary Person Birthday Required", @"Determine if birthday for primary person is required.", 2, @"False", "8528B4F1-85F5-40BE-8C42-81DB755387C1" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 2 Intro Text", "KnownIndividualPanel2IntroText", "Known Individual Panel 2 Intro Text", @"The intro text to display on the success panel.", 24, @"Thank you for connecting with us today. We hope that your enjoy the service!", "51ABBB3A-DD00-4112-B8EE-D097226207F0" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Address Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Address Required", "PrimaryPersonAddressRequired", "Primary Person Address Required", @"Determine if address for primary person is required.", 4, @"False", "AA1D4B29-2EF5-4085-85C5-B7DDF15250DD" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Shown", "OtherPersonBirthdayShown", "Other Person Birthday Shown", @"Should birthday be displayed for other person?", 7, @"True", "38F58816-17C2-4337-8A80-5B48768614B1" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Birthday Required 
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Birthday Required", "OtherPersonBirthdayRequired", "Other Person Birthday Required", @"Determine if birthday for other person is required.", 8, @"False", "DDDF9334-75CD-46C2-BBC0-67363BDEDE40" );
            // Attrib for BlockType: Attendance Self Entry:Known Relationship Types    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Known Relationship Types", "KnownRelationshipTypes", "Known Relationship Types", @"A checkbox list of Known Relationship types that should be included in the Relation dropdown.", 11, @"6b05f48e-5235-45de-970e-fe145bd28e1d", "08AA04A0-0657-4FE6-9FA1-12664A290111" );
            // Attrib for BlockType: Attendance Self Entry:Redirect URL    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Redirect URL", "RedirectURL", "Redirect URL", @"The URL to redirect the individual to when they check-in. The merge fields that are available includes 'PersonAliasGuid'.", 12, @"", "33A5C4FF-11E6-43EE-B0F3-C516DB0FC9BC" );
            // Attrib for BlockType: Attendance Self Entry:Check-in Button Text  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Button Text", "CheckinButtonText", "Check-in Button Text", @"The text that should be shown on the check-in button.", 13, @"Check-in", "BF5ABAD7-A4DC-4DD0-B10A-B126D23C6F73" );
            // Attrib for BlockType: Attendance Self Entry:Workflow     
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"The optional workflow type to launch when a person is checked in. The primary person will be passed to the workflow as the entity. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FamilyPersonIds, OtherPersonIds.", 14, @"", "4E099838-6487-4E7A-9592-F5C0A6B81CFA" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 1 Title", "UnknownIndividualPanel1Title", "Unknown Individual Panel 1 Title", @"The title to display on the primary watcher panel.", 15, @"Tell Us a Little About You...", "418D0AF2-EA3B-4EC2-BAC6-CB378E7354FE" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 1 Intro Text     
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 1 Intro Text", "UnknownIndividualPanel1IntroText", "Unknown Individual Panel 1 Intro Text", @"The intro text to display on the primary watcher panel.", 16, @" We love to learn a little about you so that we can best serve you and your family.", "E196E82E-51B7-41BA-B2E9-8BAFD153ED0A" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 2 Title", "UnknownIndividualPanel2Title", "Unknown Individual Panel 2 Title", @"The title to display on the other watcher panel.", 17, @"Who Else Is Joining You?", "33E12BF7-8765-43CB-BF4F-402DACA43F22" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 2 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 2 Intro Text", "UnknownIndividualPanel2IntroText", "Unknown Individual Panel 2 Intro Text", @"The intro text to display on the other watcher panel.", 18, @"We'd love to know more about others watching with you.", "5719012E-5818-47B1-A382-F7637058720F" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Title  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Unknown Individual Panel 3 Title", "UnknownIndividualPanel3Title", "Unknown Individual Panel 3 Title", @"The title to display on the account panel.", 19, @"Would You Like to Create An Account?", "1DA99754-75C3-4A75-9BBA-FFD841A1DB63" );
            // Attrib for BlockType: Attendance Self Entry:Unknown Individual Panel 3 Intro Text   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Unknown Individual Panel 3 Intro Text", "UnknownIndividualPanel3IntroText", "Unknown Individual Panel 3 Intro Text", @"The intro text to display on the account panel.", 20, @"Creating an account will help you to connect on our website.", "3DA4F419-1999-477D-A420-AF8A3367D1E6" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Title   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 1 Title", "KnownIndividualPanel1Title", "Known Individual Panel 1 Title", @"The title to display on the known individual panel.", 21, @"Great to see you {{ CurrentPerson.NickName }}!", "E4CF976C-BE6B-4CB4-A781-578ACE8B7621" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 1 Intro Text  
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Known Individual Panel 1 Intro Text", "KnownIndividualPanel1IntroText", "Known Individual Panel 1 Intro Text", @"The intro text to display on the known individual panel.", 22, @"We'd love to know who is watching with you today.", "664F339B-C142-40A7-BD1B-EE609B01637A" );
            // Attrib for BlockType: Attendance Self Entry:Known Individual Panel 2 Title    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Known Individual Panel 2 Title", "KnownIndividualPanel2Title", "Known Individual Panel 2 Title", @"The title to display on the success panel.", 23, @"Thanks for Connecting!", "60B08B90-0F55-4DF8-A2A2-525EE964CD6F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Shown    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Shown", "PrimaryPersonMobilePhoneShown", "Primary Person Mobile Phone Shown", @"Should mobile phone be displayed for primary person?", 5, @"True", "5FB63D9A-D9ED-45C5-9520-6258EFF49B8F" );
            // Attrib for BlockType: Attendance Self Entry:Primary Person Mobile Phone Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Primary Person Mobile Phone Required", "PrimaryPersonMobilePhoneRequired", "Primary Person Mobile Phone Required", @"Determine if mobile phone for primary person is required.", 6, @"False", "36305F0D-A8D4-480C-87F5-6F3FD064B79C" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Shown        
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Shown", "OtherPersonMobilePhoneShown", "Other Person Mobile Phone Shown", @"Should mobile phone be displayed for other person?", 9, @"True", "636540DB-AFA2-4D36-A867-2CC2A33B2C2F" );
            // Attrib for BlockType: Attendance Self Entry:Other Person Mobile Phone Required    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Other Person Mobile Phone Required", "OtherPersonMobilePhoneRequired", "Other Person Mobile Phone Required", @"Determine if mobile phone for other person is required.", 10, @"False", "49957315-CE82-4BB0-AFCD-743E3FC56708" );  
        }

        /// <summary>
        /// JE: Update CSS Classes in Mobile Templates
        /// </summary>
        private void UpdateCSSClassesInMobileTemplates()
        {
            Sql( @"
                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading1""', 'StyleClass=""h1""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')
    
                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading2""', 'StyleClass=""h2""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading3""', 'StyleClass=""h3""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading4""', 'StyleClass=""h4""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading5""', 'StyleClass=""h5""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')

                UPDATE [DefinedValue]
                SET [Description] = REPLACE([Description], 'StyleClass=""heading6""', 'StyleClass=""h6""')
                WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = 'a6e267e2-66a4-44d7-a5c9-9399666cbf95')" );
        }

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// JE/DH: Mobile Event Detail Block
        /// </summary>
        private void MobileEventDetailBlock()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Calendar Event Item Occurrence View",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6593D4EB-2B7A-4C24-8D30-A02991D26BC0",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW,
                "Default",
                @"<StackLayout>
    <Label Text=""{{ Event.Name | Escape }}"" />
    <Button Text=""Register"" Command=""{Binding OpenExternalBrowser}"">
        <Button.CommandParameter>
            <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                <Rock:Parameter Name=""RegistrationInstanceId"" Value=""0"" />
            </Rock:OpenExternalBrowserParameters>
        </Button.CommandParameter>
    </Button>
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.UpdateNoteType(
                "Structured Content User Value",
                "Rock.Model.ContentChannelItem",
                false,
                Rock.SystemGuid.NoteType.CONTENT_CHANNEL_ITEM_STRUCTURED_CONTENT_USER_VALUE );
        }

        /// <summary>
        /// SK: Update Card Lava Template in Step Type
        /// </summary>
        private void UpdateCardLavaTemplateInStepType()
        {
            string lavaTemplate = @"{% if LatestStepStatus %}
            <span class=""label"" style=""background-color: {{ LatestStepStatus.StatusColor }};"">{{ LatestStepStatus.Name }}</span>
        {% endif %}";

            string newLavaTemplate = @"{% if LatestStepStatus %}
            <span class=""label"" style=""background-color: {{ LatestStepStatus.StatusColor }};"">{{ LatestStepStatus.Name }}</span>
        {% endif %}
        {% if ShowCampus and LatestStep and LatestStep.Campus != '' %}
            <span class=""label label-campus"">{{ LatestStep.Campus.Name }}</span>
        {% endif %}";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "CardLavaTemplate" );

            Sql( $@"
                UPDATE
	                [dbo].[StepType]
                SET [CardLavaTemplate] = REPLACE({targetColumn}, '{lavaTemplate}', '{newLavaTemplate}')
                WHERE [CardLavaTemplate] IS NOT NULL and {targetColumn} NOT LIKE '%{newLavaTemplate}%'"
                );
        }

        /// <summary>
        /// SK: Add Azure Cloud Storage Component Entity Type
        /// </summary>
        private void AzureCloudStorageEntityType()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Storage.AssetStorage.AzureCloudStorageComponent", "Azure Cloud Storage Component", "Rock.Storage.AssetStorage.AzureCloudStorageComponent, Rock, Version=1.11.0.14, Culture=neutral, PublicKeyToken=null", false, true, "1576800F-BFD2-4309-A2C9-AE6DF6C0A1A5" );
        }
    }
}
