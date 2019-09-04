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
    public partial class Rollup_0820 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateGroupTypeForGroupConnections();
            LavaGroupBadge();
            MobileSiteListShowDelete();
            PendingGroupMembersNotificationEmail();
            UpdateStepProgramBlockSetting();
            ConnectionStatusChangeReportPageDescription();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CFC3BD9-F793-487A-871F-28D06C8465C3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "E94CD1F9-FF21-4B28-A417-0FC8E409842E" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CFC3BD9-F793-487A-871F-28D06C8465C3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "63DD5B5F-F11C-4514-BCC7-EF40037C210B" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CFC3BD9-F793-487A-871F-28D06C8465C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "AD29E029-981A-4676-8154-4A5E1EAF894B" );
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CFC3BD9-F793-487A-871F-28D06C8465C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "33A1CC70-B6AF-453F-85B6-F8E3707BF3CC" );
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0CFC3BD9-F793-487A-871F-28D06C8465C3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "B7409204-A1B9-4C8F-ABB9-C964551EFA7F" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "E333A0E9-AE3E-4857-B357-B688E93F2D7A" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "801A6B7D-C9A6-42F4-808A-D726FF92EADC" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "D24B239E-7895-4525-BACC-C4CECBB3374D" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "EAAFF41F-5F03-4942-AF0B-73EFEF881C77" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "B51BCB0B-9917-49E6-BEE7-49197A87D053" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "5345114D-C1E9-496D-A34D-AFEB6F5125F8" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "0BB2B9C2-90A2-4196-B4D8-733BB48DBA94" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "B0E38CD4-74B5-444F-BF24-E660B44D6CF8" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "F537BD20-0C3E-4BAC-9B73-79EB32522951" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "DD86338E-D751-43D0-9078-8CAC2A46C99E" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
</StackLayout>", "D9FB5F69-7412-414F-801F-D57FC6C2D196" );
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "060EB050-73FE-4DA2-9392-4EBEBE8876D7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "C6E96495-88CB-45E9-B67F-8308EAF14913" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "86B85B79-31A2-4D8A-B8ED-114CE44DCEA8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "B79BEAB9-A9B8-45BA-B03B-6FEC012C4688" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "86B85B79-31A2-4D8A-B8ED-114CE44DCEA8", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "F1358265-B444-41DD-9CF5-1D18E1F81E3A" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "86B85B79-31A2-4D8A-B8ED-114CE44DCEA8", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "9F8C29B1-7F8F-4D1E-B1A1-34E2FAE517DE" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "86B85B79-31A2-4D8A-B8ED-114CE44DCEA8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "CB0C1B83-156B-4E9E-B6C5-80B37000A46C" );
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CF81258-C6C4-4C04-BA60-06DC1CC0EE94", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "0215B1D0-E32E-4B74-9FF5-10804AC25867" );
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CF81258-C6C4-4C04-BA60-06DC1CC0EE94", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "D83265EE-7F0D-4D6A-B276-52DF3C2EEA26" );
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CF81258-C6C4-4C04-BA60-06DC1CC0EE94", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Content", "InitialContent", "Initial Content", @"If the initial content should be static or dynamic.", 2, @"Static", "8202EF1D-835A-472C-A2CF-F999ACE16D96" );
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3230C783-5A07-421A-AFA0-638831B7E1FB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Image Url", "ImageUrl", "Image Url", @"The URL to use for displaying the image. <span class='tip tip-lava'></span>", 0, @"", "FA0B06D0-22CC-437A-995D-456DCB5FDCBA" );
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3230C783-5A07-421A-AFA0-638831B7E1FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "BE674B6D-F8A0-4080-85F7-3F4E2550FABC" );
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3230C783-5A07-421A-AFA0-638831B7E1FB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "F99A364D-24B2-4C7C-914A-D4855D4F3652" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F439410F-82EE-4AC6-B946-F8D593B5C389", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "485212A4-347A-4AE6-A7F0-9504AD9E0006" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F439410F-82EE-4AC6-B946-F8D593B5C389", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "52CD03A4-0DD8-4E8A-A964-0D402DD3C12C" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F439410F-82EE-4AC6-B946-F8D593B5C389", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "30F49F07-C60C-42E5-9E0D-AAF6F35E6383" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F439410F-82EE-4AC6-B946-F8D593B5C389", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "31D146F5-F5BB-4A9C-8415-F7E7DDEA0F28" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D53E183-91E3-4247-81AA-881107939F9F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "C0B0B1F0-5608-4170-8EB7-B9FF25AA7A81" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D53E183-91E3-4247-81AA-881107939F9F", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "5AACEE1D-873B-412E-85E5-449C16B64F19" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "8DE098AA-2CF0-4FF3-BA1B-5F204C8E7112" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "13675D8C-D401-4407-8D0E-D0347DDAA739" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "9BC3BE23-F477-4F29-9506-FEB697A57E57" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "AE803F1B-6C2B-4A8A-AA3B-EE390E8DA03A" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "3D1BAD7D-6343-4C9C-8FE2-8E9F1BE36A3C" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "6813782E-323C-4887-A589-7969918F9C8D" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "394D9F77-F746-45BA-88E1-833D206D656B" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "E0427393-874A-417A-8C02-507C67EB36C7" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "ACD0B9D4-5CFB-4318-8C12-144A466DBA29" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "BD8936FD-5E75-4F3C-B00C-48DEC3164731" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "9D468B99-BE24-4374-A42D-80FC4974F739" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EA7AA4-2930-473E-8893-503535B1E405", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "5F7F5DBF-27E7-45F6-815F-58910F79658A" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "9CAB7C1B-126D-4C07-A8BA-B663EAA90A36" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "79CC29FA-1677-4E93-B075-A56F1F2CC16A" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "B82CB625-E41F-42C4-8DD1-41E4C2F68041" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "D931A697-7815-46B8-BFD6-58D067ABE8D1" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "81D39DA5-91C8-4FE5-B7CE-FFB8F035178D" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "202DED32-71BA-4C50-B467-C1311F18EA31" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "E90847D4-8272-4958-95F4-2AD769140A74" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "C4BF40BB-A94D-4DF4-A52B-F2C509D72F88" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "6D0468C9-2C0A-407C-AD1B-C43E04174A48" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "929D75A9-628C-4FCF-85BF-17FC3457814B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "C8E2CD9F-40A6-4C0A-B655-1C0AB10586BE" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E5BC3AE-4123-4F61-9557-0C52806B3C60", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "56A0B9A1-9054-4E25-9EC3-C826BEF98577" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E5BC3AE-4123-4F61-9557-0C52806B3C60", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "B295F2CD-CF22-4283-90F0-6D95952FC55E" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E5BC3AE-4123-4F61-9557-0C52806B3C60", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "2A995452-82EB-4CC7-A2F4-041C4EEC2AFA" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E5BC3AE-4123-4F61-9557-0C52806B3C60", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "26DB8672-FA74-42A1-A6AD-39A02179BB3B" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8E5BC3AE-4123-4F61-9557-0C52806B3C60", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "A0C1395F-F634-4D3A-9C52-8FB4FBE6AD93" );
            // Attrib for BlockType: Site List:Show Delete Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete Column", "ShowDeleteColumn", "Show Delete Column", @"Determines if the delete column should be shown.", 5, @"False", "952C470D-8A97-46E8-894F-C758580E0584" );
            // Attrib for BlockType: Communication Detail:Series Colors
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Series Colors", "SeriesColors", "Series Colors", @"A comma-delimited list of colors that the Clients chart will use.", 1, @"#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766", "38BC43B3-394D-4BE9-A24F-AA38D063243B" );
            // Attrib for BlockType: Step Bulk Entry:Step Program and Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6535FA22-9630-49A3-B8FF-A672CD91B8EE", "F8E85355-2780-4772-9B21-30B84741E6D1", "Step Program and Status", "StepProgramStepStatus", "Step Program and Status", @"The step program and step status to use to add a new step. Leave this empty to allow the user to choose.", 2, @"", "E87F6675-CB51-42E5-9544-73BBBB71AFFB" );
            // Attrib for BlockType: Step Bulk Entry:Step Program and Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6535FA22-9630-49A3-B8FF-A672CD91B8EE", "B00149C7-08D6-448C-AF21-948BF453DF7E", "Step Program and Type", "StepProgramStepType", "Step Program and Type", @"The step program and step type to use to add a new step. Leave this empty to allow the user to choose.", 1, @"", "9E0D5B6D-6622-4AD9-9FEE-B22D10FBC2BD" );
            RockMigrationHelper.UpdateFieldType("Step Program Step Status","","Rock","Rock.Field.Types.StepProgramStepStatusFieldType","F8E85355-2780-4772-9B21-30B84741E6D1");
            RockMigrationHelper.UpdateFieldType("Step Program Step Type","","Rock","Rock.Field.Types.StepProgramStepTypeFieldType","B00149C7-08D6-448C-AF21-948BF453DF7E");

        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Step Bulk Entry:Step Program and Type
            RockMigrationHelper.DeleteAttribute("9E0D5B6D-6622-4AD9-9FEE-B22D10FBC2BD");
            // Attrib for BlockType: Step Bulk Entry:Step Program and Status
            RockMigrationHelper.DeleteAttribute("E87F6675-CB51-42E5-9544-73BBBB71AFFB");
            // Attrib for BlockType: Communication Detail:Series Colors
            RockMigrationHelper.DeleteAttribute("38BC43B3-394D-4BE9-A24F-AA38D063243B");
            // Attrib for BlockType: Site List:Show Delete Column
            RockMigrationHelper.DeleteAttribute("952C470D-8A97-46E8-894F-C758580E0584");
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("A0C1395F-F634-4D3A-9C52-8FB4FBE6AD93");
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("26DB8672-FA74-42A1-A6AD-39A02179BB3B");
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("2A995452-82EB-4CC7-A2F4-041C4EEC2AFA");
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("B295F2CD-CF22-4283-90F0-6D95952FC55E");
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("56A0B9A1-9054-4E25-9EC3-C826BEF98577");
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("C8E2CD9F-40A6-4C0A-B655-1C0AB10586BE");
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("6D0468C9-2C0A-407C-AD1B-C43E04174A48");
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute("C4BF40BB-A94D-4DF4-A52B-F2C509D72F88");
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute("E90847D4-8272-4958-95F4-2AD769140A74");
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute("202DED32-71BA-4C50-B467-C1311F18EA31");
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute("81D39DA5-91C8-4FE5-B7CE-FFB8F035178D");
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("D931A697-7815-46B8-BFD6-58D067ABE8D1");
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("B82CB625-E41F-42C4-8DD1-41E4C2F68041");
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute("79CC29FA-1677-4E93-B075-A56F1F2CC16A");
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute("9CAB7C1B-126D-4C07-A8BA-B663EAA90A36");
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("5F7F5DBF-27E7-45F6-815F-58910F79658A");
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("9D468B99-BE24-4374-A42D-80FC4974F739");
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("BD8936FD-5E75-4F3C-B00C-48DEC3164731");
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("ACD0B9D4-5CFB-4318-8C12-144A466DBA29");
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("E0427393-874A-417A-8C02-507C67EB36C7");
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("394D9F77-F746-45BA-88E1-833D206D656B");
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("6813782E-323C-4887-A589-7969918F9C8D");
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("3D1BAD7D-6343-4C9C-8FE2-8E9F1BE36A3C");
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("AE803F1B-6C2B-4A8A-AA3B-EE390E8DA03A");
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("9BC3BE23-F477-4F29-9506-FEB697A57E57");
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("13675D8C-D401-4407-8D0E-D0347DDAA739");
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("8DE098AA-2CF0-4FF3-BA1B-5F204C8E7112");
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("5AACEE1D-873B-412E-85E5-449C16B64F19");
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute("C0B0B1F0-5608-4170-8EB7-B9FF25AA7A81");
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("31D146F5-F5BB-4A9C-8415-F7E7DDEA0F28");
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute("30F49F07-C60C-42E5-9E0D-AAF6F35E6383");
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("52CD03A4-0DD8-4E8A-A964-0D402DD3C12C");
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute("485212A4-347A-4AE6-A7F0-9504AD9E0006");
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.DeleteAttribute("F99A364D-24B2-4C7C-914A-D4855D4F3652");
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.DeleteAttribute("BE674B6D-F8A0-4080-85F7-3F4E2550FABC");
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.DeleteAttribute("FA0B06D0-22CC-437A-995D-456DCB5FDCBA");
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.DeleteAttribute("8202EF1D-835A-472C-A2CF-F999ACE16D96");
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("D83265EE-7F0D-4D6A-B276-52DF3C2EEA26");
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.DeleteAttribute("0215B1D0-E32E-4B74-9FF5-10804AC25867");
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute("CB0C1B83-156B-4E9E-B6C5-80B37000A46C");
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute("9F8C29B1-7F8F-4D1E-B1A1-34E2FAE517DE");
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("F1358265-B444-41DD-9CF5-1D18E1F81E3A");
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute("B79BEAB9-A9B8-45BA-B03B-6FEC012C4688");
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.DeleteAttribute("C6E96495-88CB-45E9-B67F-8308EAF14913");
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("D9FB5F69-7412-414F-801F-D57FC6C2D196");
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute("DD86338E-D751-43D0-9078-8CAC2A46C99E");
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute("F537BD20-0C3E-4BAC-9B73-79EB32522951");
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute("B0E38CD4-74B5-444F-BF24-E660B44D6CF8");
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute("0BB2B9C2-90A2-4196-B4D8-733BB48DBA94");
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute("5345114D-C1E9-496D-A34D-AFEB6F5125F8");
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute("B51BCB0B-9917-49E6-BEE7-49197A87D053");
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("EAAFF41F-5F03-4942-AF0B-73EFEF881C77");
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute("D24B239E-7895-4525-BACC-C4CECBB3374D");
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute("801A6B7D-C9A6-42F4-808A-D726FF92EADC");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute("E333A0E9-AE3E-4857-B357-B688E93F2D7A");
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("B7409204-A1B9-4C8F-ABB9-C964551EFA7F");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("33A1CC70-B6AF-453F-85B6-F8E3707BF3CC");
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("AD29E029-981A-4676-8154-4A5E1EAF894B");
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("63DD5B5F-F11C-4514-BCC7-EF40037C210B");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute("E94CD1F9-FF21-4B28-A417-0FC8E409842E");
            RockMigrationHelper.DeleteBlockType("8E5BC3AE-4123-4F61-9557-0C52806B3C60"); // Workflow Entry
            RockMigrationHelper.DeleteBlockType("929D75A9-628C-4FCF-85BF-17FC3457814B"); // Register
            RockMigrationHelper.DeleteBlockType("C8EA7AA4-2930-473E-8893-503535B1E405"); // Profile Details
            RockMigrationHelper.DeleteBlockType("4D53E183-91E3-4247-81AA-881107939F9F"); // Login
            RockMigrationHelper.DeleteBlockType("F439410F-82EE-4AC6-B946-F8D593B5C389"); // Lava Item List
            RockMigrationHelper.DeleteBlockType("3230C783-5A07-421A-AFA0-638831B7E1FB"); // Image
            RockMigrationHelper.DeleteBlockType("1CF81258-C6C4-4C04-BA60-06DC1CC0EE94"); // Dynamic Content
            RockMigrationHelper.DeleteBlockType("86B85B79-31A2-4D8A-B8ED-114CE44DCEA8"); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType("060EB050-73FE-4DA2-9392-4EBEBE8876D7"); // Content Channel Item List
            RockMigrationHelper.DeleteBlockType("0CFC3BD9-F793-487A-871F-28D06C8465C3"); // Content
        }

        /// <summary>
        /// JE: Group Connections Project
        /// </summary>
        private void UpdateGroupTypeForGroupConnections()
        {
            Sql( @"
                UPDATE [GroupType]
                SET [AllowSpecificGroupMemberAttributes] = 1
                WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49'
                    AND [AllowSpecificGroupMemberAttributes] = 0" );
        }

        /// <summary>
        /// GJ: Lava Group Badge
        /// </summary>
        private void LavaGroupBadge()
        {
            Sql( MigrationSQL._201908202115162_Rollup_0820_GroupRequirementsBadge );
        }

        /// <summary>
        /// SK: Enabled Show Delete Column for Mobile Application Site List
        /// </summary>
        private void MobileSiteListShowDelete()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete Column", "ShowDeleteColumn", "Show Delete Column", @"Determines if the delete column should be shown.", 5, @"False", "72FAC7B5-AA38-491B-83AF-7945D02F6740" );
            RockMigrationHelper.AddBlockAttributeValue("BD30CBF7-3296-43B6-A98E-6EF6F2F12E51","72FAC7B5-AA38-491B-83AF-7945D02F6740",@"True");  
        }

        /// <summary>
        /// SK: Fixed Pending Group Members Notification system email
        /// </summary>
        private void PendingGroupMembersNotificationEmail()
        {
            string lavaTemplate = @"We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group.";

            string newLavaTemplate = @"We wanted to make you aware of additional individuals who have taken the next step to connect with your {{ Group.Name }} group.";

            lavaTemplate = lavaTemplate.Replace( "'", "''" );
            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );

            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"
            UPDATE [SystemEmail]
            SET [Body] = REPLACE( {targetColumn}
                ,'{lavaTemplate}'
                ,'{newLavaTemplate}' )
            WHERE [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986' AND {targetColumn} NOT LIKE '%{newLavaTemplate}%'"
            );
        }

        /// <summary>
        /// SK:  Updated Step Program Block Setting
        /// </summary>
        private void UpdateStepProgramBlockSetting()
        {
            RockMigrationHelper.AddBlockAttributeValue("46E5C15A-44A5-4FB3-8CE8-572FB0D85367","B4E07CC9-53E0-47CF-AC22-10F2085547A3",@"A31AE259-885E-4ACE-B8B6-56000F58EA3B");
            RockMigrationHelper.AddBlockAttributeValue("46E5C15A-44A5-4FB3-8CE8-572FB0D85367","FBAB162B-556B-44DA-B830-D629529C0542",@"5");  
        }

        /// <summary>
        /// JE: Cleared Page Description for Connection Status Change Report Page
        /// </summary>
        private void ConnectionStatusChangeReportPageDescription()
        {
            Sql( @"
                UPDATE [Page] 
                SET [Description] = ''
                WHERE [Guid] = '97624123-900C-4442-B42E-19CF95877E04'" );
        }
    }
}
