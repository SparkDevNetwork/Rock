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
    public partial class Rollup_1001 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RemoveChecklistItem();
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
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Content","Content","Rock.Blocks.Types.Mobile.Content, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_CONTENT_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.ContentChannelItemList","Content Channel Item List","Rock.Blocks.Types.Mobile.ContentChannelItemList, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_CONTENT_CHANNEL_ITEM_LIST_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.ContentChannelItemView","Content Channel Item View","Rock.Blocks.Types.Mobile.ContentChannelItemView, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_CONTENT_CHANNEL_ITEM_VIEW_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.LavaItemList","Lava Item List","Rock.Blocks.Types.Mobile.LavaItemList, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_LAVA_ITEM_LIST_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Login","Login","Rock.Blocks.Types.Mobile.Login, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_LOGIN_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.ProfileDetails","Profile Details","Rock.Blocks.Types.Mobile.ProfileDetails, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_PROFILE_DETAILS_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.Register","Register","Rock.Blocks.Types.Mobile.Register, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_REGISTER_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Types.Mobile.WorkflowEntry","Workflow Entry","Rock.Blocks.Types.Mobile.WorkflowEntry, Rock, Version=1.10.0.7, Culture=neutral, PublicKeyToken=null",false,false, Rock.SystemGuid.EntityType.MOBILE_WORKFLOW_ENTRY_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType("Content", "Displays custom XAML content on the page.", "Rock.Blocks.Types.Mobile.Content", "7258A210-E936-4260-B573-9FA1193AD9E2");
            RockMigrationHelper.UpdateMobileBlockType("Content Channel Item List", "Lists content channel items for a given channel.", "Rock.Blocks.Types.Mobile.ContentChannelItemList", "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC");
            RockMigrationHelper.UpdateMobileBlockType("Content Channel Item View", "Displays a content channel item by formatting it with XAML.", "Rock.Blocks.Types.Mobile.ContentChannelItemView", "B76B5F10-D2D6-4C60-B6FB-F913A62442E0");
            RockMigrationHelper.UpdateMobileBlockType("Lava Item List", "List items genreated by Lava.", "Rock.Blocks.Types.Mobile.LavaItemList", "42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D");
            RockMigrationHelper.UpdateMobileBlockType("Login", "Allows the user to login on a mobile application.", "Rock.Blocks.Types.Mobile.Login", "6006FE32-DC01-4B1C-A9B8-EE172451F4C5");
            RockMigrationHelper.UpdateMobileBlockType("Profile Details", "Allows the user to edit their account on a mobile application.", "Rock.Blocks.Types.Mobile.ProfileDetails", "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C");
            RockMigrationHelper.UpdateMobileBlockType("Register", "Allows the user to register a new account on a mobile application.", "Rock.Blocks.Types.Mobile.Register", "2A71FDA2-5204-418F-858E-693A1F4E9A49");
            RockMigrationHelper.UpdateMobileBlockType("Workflow Entry", "Allows for filling out workflows from a mobile application.", "Rock.Blocks.Types.Mobile.WorkflowEntry", "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED");
            
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "5682EDBF-68DA-4B43-A593-6C2B936C2839" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "24516448-3F1F-4F27-97A1-CFB4F8B277B5" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "B31D29A0-3725-4AEB-8360-7D91B9CDFE47" );
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "061A6328-C9EC-4190-A117-F90C4EAE4E28" );
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "FE62F5A5-DC26-4D32-B643-EAA66567144E" );
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7258A210-E936-4260-B573-9FA1193AD9E2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Callback Logic", "CallbackLogic", "Callback Logic", @"If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>", 0, @"", "2725F971-243A-4B60-83EB-527BA8C08737" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "DCD6442E-9473-40C4-9537-6DE55960EB1B" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "8442D333-A417-4297-8B9D-51D2E520DC9C" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "D838934D-21B1-4CAB-AEFB-7157B60DE6FE" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "0DE5AA37-0340-403D-899A-AE53678E7FBA" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "1341250F-F955-4266-9B4A-B9C3C51A3A14" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "B98CB843-5D3D-4635-B57C-5102080999E4" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "E4EDAFF1-818B-4C1B-9380-6D9D04B136C3" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "EE9AEEC7-A8E0-4BD0-BE60-6D990A8F38DD" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "1879B5C4-D1AB-4B81-B6A0-2C359A8084C7" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "A14F5FE3-013E-4C28-93F9-1E1D041D20E3" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
</StackLayout>", "E7A8E965-2BCD-40E2-BEFD-BF64D6229AB4" );
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A06FF57-DE19-423A-9E8A-CB71B69DD4FC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "AE960559-187C-4C9E-B90A-B6856D475E01" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B76B5F10-D2D6-4C60-B6FB-F913A62442E0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "D80CF7C7-F6F4-4E77-97A8-B0842E4AF7FB" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B76B5F10-D2D6-4C60-B6FB-F913A62442E0", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "45EC896A-6A9F-495E-8F88-1BC800612B2D" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B76B5F10-D2D6-4C60-B6FB-F913A62442E0", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "49913217-BF13-4270-8023-C56BDA52C790" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B76B5F10-D2D6-4C60-B6FB-F913A62442E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "616351D9-41FD-4E84-9378-78140BE30605" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "AFFD987C-99DF-43F5-B95E-636EBFB4F463" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "A644F995-DDF5-438B-8E11-B382874C872B" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "2B736AB8-107E-412D-87CC-271F9C813911" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "E92051A2-E18B-4599-B9DD-4F43B6D578D5" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "61B98E57-B508-4384-9606-8A4D6E827658" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "0036807C-7742-48DE-BAD4-E025DE37A215" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "E4709745-E420-425D-82FB-E7EA9B8C89E2" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "13AED2C1-BC58-4B5C-B711-CEA71A52ECC4" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "F8A627F5-D23B-4F09-BF68-C2E7D5279C4D" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "98A9C8D3-777D-4744-9346-811A9829CB47" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "E35DCBCE-A2F6-46BD-87D5-04FF6A59BAB8" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "22289290-D87F-418F-997C-5FDF986379A1" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "FE8F714E-3077-4EB6-87A8-001AF221DA1E" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "BE3AAA2A-5D08-46CF-B7DE-DB8F19502463" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "1FAA53DE-7F6E-481A-A8C0-D977271F0B6E" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "7DAC89DE-BF2E-47E7-9D83-A4056A681D9B" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "CC6CC403-423B-48CA-9761-6114C34C7FDE" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "1759420A-18DA-44B0-98B9-54F55CD644B6" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "AF36BCE7-62F7-461F-80FC-77343925FE3E" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "BC9E9B0F-0097-4032-B348-8EA1C5B56E6D" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "70992EE5-D8AF-420F-AA19-E5A0743F679F" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "7C30E10D-3254-43C5-B0FF-36CB5012B708" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "9410EFC3-43DE-48C7-92BC-09ECD4F8E63F" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "B9F4F69B-86FE-47BE-8AC4-5C90C939F93D" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "B6045ACD-F384-4CEE-93C7-EA6C87024601" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "104CE646-73E5-46FA-B007-C4BF0FCC68B3" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "65C84E5A-4D7E-4207-B019-75AA3458487F" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A71FDA2-5204-418F-858E-693A1F4E9A49", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "A5EE3A3C-0455-4966-90FF-5DC1000EDC67" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "D77299F8-37F8-4F3C-8747-A9F1C7C5CEF1" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "87BAB537-0EB1-4894-B72B-D70472C802D7" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "46BB8051-EE66-4128-B47E-75130EA855F1" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "5DB854C5-8F95-4EDF-9B2D-5DBDC8FC3D29" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "1338F56A-0F7F-4484-A665-6E7C564F25D0" );
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "0E2B924A-C1AC-4A7C-AD77-A036581552D4", "Content Channels Filter", "ContentChannelsFilter", "Content Channels Filter", @"Select the content channels you would like displayed. This setting will override the Content Channel Types Include/Exclude settings.", 4, @"", "F45513EB-2693-4377-B910-3D998F4ED115" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.DeleteAttribute("F45513EB-2693-4377-B910-3D998F4ED115");
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("1338F56A-0F7F-4484-A665-6E7C564F25D0");
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("5DB854C5-8F95-4EDF-9B2D-5DBDC8FC3D29");
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("46BB8051-EE66-4128-B47E-75130EA855F1");
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("87BAB537-0EB1-4894-B72B-D70472C802D7");
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("D77299F8-37F8-4F3C-8747-A9F1C7C5CEF1");
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("A5EE3A3C-0455-4966-90FF-5DC1000EDC67");
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("65C84E5A-4D7E-4207-B019-75AA3458487F");
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute("104CE646-73E5-46FA-B007-C4BF0FCC68B3");
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute("B6045ACD-F384-4CEE-93C7-EA6C87024601");
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute("B9F4F69B-86FE-47BE-8AC4-5C90C939F93D");
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute("9410EFC3-43DE-48C7-92BC-09ECD4F8E63F");
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("7C30E10D-3254-43C5-B0FF-36CB5012B708");
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("70992EE5-D8AF-420F-AA19-E5A0743F679F");
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute("BC9E9B0F-0097-4032-B348-8EA1C5B56E6D");
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute("AF36BCE7-62F7-461F-80FC-77343925FE3E");
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("1759420A-18DA-44B0-98B9-54F55CD644B6");
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("CC6CC403-423B-48CA-9761-6114C34C7FDE");
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("7DAC89DE-BF2E-47E7-9D83-A4056A681D9B");
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("1FAA53DE-7F6E-481A-A8C0-D977271F0B6E");
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("BE3AAA2A-5D08-46CF-B7DE-DB8F19502463");
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("FE8F714E-3077-4EB6-87A8-001AF221DA1E");
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("22289290-D87F-418F-997C-5FDF986379A1");
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("E35DCBCE-A2F6-46BD-87D5-04FF6A59BAB8");
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("98A9C8D3-777D-4744-9346-811A9829CB47");
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("F8A627F5-D23B-4F09-BF68-C2E7D5279C4D");
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("13AED2C1-BC58-4B5C-B711-CEA71A52ECC4");
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("E4709745-E420-425D-82FB-E7EA9B8C89E2");
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("0036807C-7742-48DE-BAD4-E025DE37A215");
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute("61B98E57-B508-4384-9606-8A4D6E827658");
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("E92051A2-E18B-4599-B9DD-4F43B6D578D5");
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute("2B736AB8-107E-412D-87CC-271F9C813911");
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("A644F995-DDF5-438B-8E11-B382874C872B");
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute("AFFD987C-99DF-43F5-B95E-636EBFB4F463");
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute("616351D9-41FD-4E84-9378-78140BE30605");
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute("49913217-BF13-4270-8023-C56BDA52C790");
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("45EC896A-6A9F-495E-8F88-1BC800612B2D");
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute("D80CF7C7-F6F4-4E77-97A8-B0842E4AF7FB");
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.DeleteAttribute("AE960559-187C-4C9E-B90A-B6856D475E01");
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("E7A8E965-2BCD-40E2-BEFD-BF64D6229AB4");
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute("A14F5FE3-013E-4C28-93F9-1E1D041D20E3");
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute("1879B5C4-D1AB-4B81-B6A0-2C359A8084C7");
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute("EE9AEEC7-A8E0-4BD0-BE60-6D990A8F38DD");
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute("E4EDAFF1-818B-4C1B-9380-6D9D04B136C3");
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute("B98CB843-5D3D-4635-B57C-5102080999E4");
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute("1341250F-F955-4266-9B4A-B9C3C51A3A14");
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("0DE5AA37-0340-403D-899A-AE53678E7FBA");
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute("D838934D-21B1-4CAB-AEFB-7157B60DE6FE");
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute("8442D333-A417-4297-8B9D-51D2E520DC9C");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute("DCD6442E-9473-40C4-9537-6DE55960EB1B");
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.DeleteAttribute("2725F971-243A-4B60-83EB-527BA8C08737");
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("FE62F5A5-DC26-4D32-B643-EAA66567144E");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("061A6328-C9EC-4190-A117-F90C4EAE4E28");
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("B31D29A0-3725-4AEB-8360-7D91B9CDFE47");
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("24516448-3F1F-4F27-97A1-CFB4F8B277B5");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute("5682EDBF-68DA-4B43-A593-6C2B936C2839");
            RockMigrationHelper.DeleteBlockType("9116AAD8-CF16-4BCE-B0CF-5B4D565710ED"); // Workflow Entry
            RockMigrationHelper.DeleteBlockType("2A71FDA2-5204-418F-858E-693A1F4E9A49"); // Register
            RockMigrationHelper.DeleteBlockType("66B2B513-1C71-4E6B-B4BE-C4EF90E1899C"); // Profile Details
            RockMigrationHelper.DeleteBlockType("6006FE32-DC01-4B1C-A9B8-EE172451F4C5"); // Login
            RockMigrationHelper.DeleteBlockType("42B9ADBA-AE3E-4AC6-BE4C-7D3714ADF48D"); // Lava Item List
            RockMigrationHelper.DeleteBlockType("B76B5F10-D2D6-4C60-B6FB-F913A62442E0"); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType("5A06FF57-DE19-423A-9E8A-CB71B69DD4FC"); // Content Channel Item List
            RockMigrationHelper.DeleteBlockType("7258A210-E936-4260-B573-9FA1193AD9E2"); // Content

            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_CONTENT_BLOCK_TYPE ); // Content
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_CONTENT_CHANNEL_ITEM_LIST_BLOCK_TYPE ); // Content Channel Item List
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_CONTENT_CHANNEL_ITEM_VIEW_BLOCK_TYPE ); // Content Channel Item View
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_LAVA_ITEM_LIST_BLOCK_TYPE ); // Lava Item List
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_LOGIN_BLOCK_TYPE ); // Login
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_PROFILE_DETAILS_BLOCK_TYPE ); // Profile Details
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_REGISTER_BLOCK_TYPE ); // Register
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_WORKFLOW_ENTRY_BLOCK_TYPE ); // Workflow Entry
        }

        /// <summary>
        /// JE: Remove Old Admin Checklist Item
        /// </summary>
        private void RemoveChecklistItem()
        {
            Sql( @"
                DELETE
                FROM [DefinedValue]
                WHERE [Guid] = '62a45da7-8c68-4283-b9aa-2e1be38610af'" );
        }
    }
}
