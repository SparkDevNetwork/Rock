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
    public partial class CodeGenerated_20211005 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
           // Add/Update BlockType 
           //   Name: Personalized Communication History
           //   Category: Communication
           //   Path: ~/Blocks/Communication/PersonalizedCommunicationHistory.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Personalized Communication History","Lists the communications sent to a specific individual","~/Blocks/Communication/PersonalizedCommunicationHistory.ascx","Communication","3F294916-A02D-48D5-8FE4-E8D7B98F61F7");

            // Attribute for BlockType
            //   BlockType: Giving Analytics
            //   Category: Finance
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 0, @"180", "5EC22115-5159-4C26-9308-A627FF5A0431" );

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F294916-A02D-48D5-8FE4-E8D7B98F61F7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Detail Page", "CommunicationDetailPage", "Communication Detail Page", @"", 0, @"", "1210C9FB-A6EF-4C10-868B-1D8F054B2356" );

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication List Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F294916-A02D-48D5-8FE4-E8D7B98F61F7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication List Detail Page", "CommunicationListDetailPage", "Communication List Detail Page", @"", 0, @"", "530DC2A7-DC47-447B-92CE-CF46612488A4" );

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Segment Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F294916-A02D-48D5-8FE4-E8D7B98F61F7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Segment Detail Page", "CommunicationSegmentDetailPage", "Communication Segment Detail Page", @"", 0, @"", "4523F7E5-1356-42DB-9F18-F3FB33F15ADD" );

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Template Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F294916-A02D-48D5-8FE4-E8D7B98F61F7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Template Detail Page", "CommunicationTemplateDetailPage", "Communication Template Detail Page", @"", 0, @"", "FD364362-2BC0-41AA-8E76-FEE888A9560E" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Confirmation Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationWebPage", "Confirmation Page", @"Web page on a public site for user to confirm their account (if not set then no confirmation e-mail will be sent).", 2, @"", "741ADC29-B726-4E98-B4FB-DE7C2B4E157A" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Confirm Account Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account Template", "ConfirmAccountTemplate", "Confirm Account Template", @"The system communication to use when generating the confirm account e-mail.", 3, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "A425BCFB-F882-4094-B41B-66A79FA4C902" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Return Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Return Page", "ReturnPage", "Return Page", @"The page to return to after the individual has successfully logged in, defaults to the home page (requires shell v3).", 4, @"", "9038418B-3BA9-4DEC-8B10-AD591A17ABAB" );

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Cancel Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Cancel Page", "CancelPage", "Cancel Page", @"The page to return to after pressing the cancel button, defaults to the home page (requires shell v3).", 5, @"", "A9EFA01A-D90F-4AFE-A3E4-31C231F5CB9C" );

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Check For Duplicates
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check For Duplicates", "CheckForDuplicates", "Check For Duplicates", @"If enabled and a duplicate is found then it will be used instead of creating a new person record. You must also configure the Confirmation Page and Confirm Account Template settings.", 0, @"True", "ECC43369-9E44-4570-81C5-8E670BDBFE50" );

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Confirmation Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationWebPage", "Confirmation Page", @"Web page on a public site for user to confirm their account (if not set then no confirmation e-mail will be sent).", 1, @"", "06FC8FA6-1177-47E7-9B2C-E9F190D8D72C" );

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Confirm Account Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Confirm Account Template", "ConfirmAccountTemplate", "Confirm Account Template", @"The system communication to use when generating the confirm account e-mail.", 2, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "4B3D4D4D-CA87-4B9F-85AD-CAB5AE5E6C32" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The Lava template that lays out the view of the prayer requests.", 0, @"757935E7-AB6D-47B6-A6C4-1CA5920C922E", "A8B76BEB-0F6F-4F0B-968C-49CB3CEBB77F" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Title Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Title Content", "TitleContent", "Title Content", @"The XAML content to show below the campus picker and above the prayer requests.", 1, @"", "CD77FAA9-1852-407E-BE14-C8F92A1E7111" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Hide Campus When Known
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Campus When Known", "HideCampusWhenKnown", "Hide Campus When Known", @"Will hide the campus picker when a campus is known from either the Current Person's campus or passed in CampusGuid page parameter.", 2, @"False", "26CD1F8C-ABA4-48CC-812E-C43520C546B4" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"A top level category. This controls which categories are shown when starting a prayer session.", 3, @"", "4B794B17-84AB-4D83-9CBF-6ACB327297F2" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public prayer requests will be excluded.", 4, @"True", "1BACC328-1913-4C1B-AA05-18CD56F357F8" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Order", "PrayerOrder", "Order", @"The order that requests should be displayed.", 5, @"0", "6D362614-769B-4C0E-A305-B567FFE50F7E" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Allows selecting which campus types to filter campuses by.", 6, @"", "8BE3F2C3-517E-449F-871B-4C683C647D61" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This allows selecting which campus statuses to filter campuses by.", 7, @"", "759F4397-6C81-4EF9-AD7A-48829DA37293" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Max Requests
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Requests", "MaxRequests", "Max Requests", @"The maximum number of requests to display. Leave blank for all.", 8, @"", "8F216637-A193-4131-A1AE-9251F885AFC0" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Load Last Prayed Collection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Load Last Prayed Collection", "LoadLastPrayedCollection", "Load Last Prayed Collection", @"Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.", 9, @"False", "A39D70B2-2AF9-42B8-8C18-CC671E299CE0" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Prayed Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Prayed Workflow", "PrayedWorkflow", "Prayed Workflow", @"The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic ""Entity"" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonId.", 10, @"", "7F9EB953-AAA6-45EB-9B11-7C122E8F7618" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Prayer
            //   Attribute: Load Last Prayed Collection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FEE129E-E46A-4805-AF5A-6F98E1DA7A16", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Load Last Prayed Collection", "LoadLastPrayedCollection", "Load Last Prayed Collection", @"Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.", 13, @"False", "B83C0F29-0ADB-4013-B426-705B8EECC561" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Prayer
            //   Attribute: Load Last Prayed Collection
            RockMigrationHelper.DeleteAttribute("B83C0F29-0ADB-4013-B426-705B8EECC561");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Prayed Workflow
            RockMigrationHelper.DeleteAttribute("7F9EB953-AAA6-45EB-9B11-7C122E8F7618");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Load Last Prayed Collection
            RockMigrationHelper.DeleteAttribute("A39D70B2-2AF9-42B8-8C18-CC671E299CE0");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Max Requests
            RockMigrationHelper.DeleteAttribute("8F216637-A193-4131-A1AE-9251F885AFC0");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute("759F4397-6C81-4EF9-AD7A-48829DA37293");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute("8BE3F2C3-517E-449F-871B-4C683C647D61");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Order
            RockMigrationHelper.DeleteAttribute("6D362614-769B-4C0E-A305-B567FFE50F7E");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute("1BACC328-1913-4C1B-AA05-18CD56F357F8");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Category
            RockMigrationHelper.DeleteAttribute("4B794B17-84AB-4D83-9CBF-6ACB327297F2");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Hide Campus When Known
            RockMigrationHelper.DeleteAttribute("26CD1F8C-ABA4-48CC-812E-C43520C546B4");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Title Content
            RockMigrationHelper.DeleteAttribute("CD77FAA9-1852-407E-BE14-C8F92A1E7111");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Template
            RockMigrationHelper.DeleteAttribute("A8B76BEB-0F6F-4F0B-968C-49CB3CEBB77F");

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Confirm Account Template
            RockMigrationHelper.DeleteAttribute("4B3D4D4D-CA87-4B9F-85AD-CAB5AE5E6C32");

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Confirmation Page
            RockMigrationHelper.DeleteAttribute("06FC8FA6-1177-47E7-9B2C-E9F190D8D72C");

            // Attribute for BlockType
            //   BlockType: Register
            //   Category: Mobile > Cms
            //   Attribute: Check For Duplicates
            RockMigrationHelper.DeleteAttribute("ECC43369-9E44-4570-81C5-8E670BDBFE50");

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Cancel Page
            RockMigrationHelper.DeleteAttribute("A9EFA01A-D90F-4AFE-A3E4-31C231F5CB9C");

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Return Page
            RockMigrationHelper.DeleteAttribute("9038418B-3BA9-4DEC-8B10-AD591A17ABAB");

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Confirm Account Template
            RockMigrationHelper.DeleteAttribute("A425BCFB-F882-4094-B41B-66A79FA4C902");

            // Attribute for BlockType
            //   BlockType: Login
            //   Category: Mobile > Cms
            //   Attribute: Confirmation Page
            RockMigrationHelper.DeleteAttribute("741ADC29-B726-4E98-B4FB-DE7C2B4E157A");

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Template Detail Page
            RockMigrationHelper.DeleteAttribute("FD364362-2BC0-41AA-8E76-FEE888A9560E");

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Segment Detail Page
            RockMigrationHelper.DeleteAttribute("4523F7E5-1356-42DB-9F18-F3FB33F15ADD");

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication List Detail Page
            RockMigrationHelper.DeleteAttribute("530DC2A7-DC47-447B-92CE-CF46612488A4");

            // Attribute for BlockType
            //   BlockType: Personalized Communication History
            //   Category: Communication
            //   Attribute: Communication Detail Page
            RockMigrationHelper.DeleteAttribute("1210C9FB-A6EF-4C10-868B-1D8F054B2356");

            // Attribute for BlockType
            //   BlockType: Giving Analytics
            //   Category: Finance
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute("5EC22115-5159-4C26-9308-A627FF5A0431");

            // Delete BlockType 
            //   Name: Personalized Communication History
            //   Category: Communication
            //   Path: ~/Blocks/Communication/PersonalizedCommunicationHistory.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("3F294916-A02D-48D5-8FE4-E8D7B98F61F7");

            // Delete BlockType 
            //   Name: Field Type Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Field Type Gallery
            RockMigrationHelper.DeleteBlockType("0D5B1C68-C907-43E8-9386-D7F8BD2373BE");

            // Delete BlockType 
            //   Name: Control Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Control Gallery
            RockMigrationHelper.DeleteBlockType("09478215-BC13-4067-A8A9-32459FDA5653");

            // Delete BlockType 
            //   Name: Widget List
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Widgets List
            RockMigrationHelper.DeleteBlockType("B7D69FAB-4D08-4E3F-BEF2-DB9109D8DC5C");

            // Delete BlockType 
            //   Name: Context Group
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Group
            RockMigrationHelper.DeleteBlockType("6EF05FC3-D433-4165-B455-C67A6DED74A8");

            // Delete BlockType 
            //   Name: Context Entities
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Entities
            RockMigrationHelper.DeleteBlockType("03C26704-2F8A-4028-BB04-6B1123700C29");
        }
    }
}
