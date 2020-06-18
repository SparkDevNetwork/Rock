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
    public partial class Rollup_0917 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixMotivatorsPositionalSummaryText();
            RemoveEmailAnalyticsPageUp();
            UpdateStepTypePageTitle();
            RemoveMobileDynamicContentBlock();
            RemoveMobileImageBlock();
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
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "59B2A109-E266-42C5-838E-42AFD15AACC0" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "6D7108BD-9CCA-4886-9F9A-D0A01DB3FDC3" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "E50E5ADB-6CF0-4CBE-BA83-E7380BCD5AB5" );
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "1E4C1A4E-952D-43A3-97EF-9B747CDBCA70" );
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "D0879626-3C5E-4B5D-9DEA-56563F615B85" );
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Callback Logic", "CallbackLogic", "Callback Logic", @"If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>", 0, @"", "B7EF8FD2-83F2-468D-9E7B-8FFC337B2964" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "F1E59A6B-A64E-4217-930B-B90EFE1CB3D5" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "9695DCA1-86C2-4F4C-B9F5-BF90E2178F4D" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "A8AD9B03-15BB-4338-B0DD-5EE4F9BDD1E0" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "307EAF97-77FB-43DE-A809-D12E348DB907" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "CBD2A1FD-6DAB-4134-8C01-76C72C50B267" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "D11DE08F-F1F0-429F-8342-6F25FFB3292A" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "9A395CB7-65F8-49C1-8D68-9DF0C04DF3C3" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "15C21D62-4F6D-4F86-B2D0-163FD5D14C34" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "34DB685B-B52A-4D1C-A467-44B8DB628956" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "E5532E34-519E-40C6-8EE2-860E81C84C52" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
</StackLayout>", "8C5D0BFB-017E-4885-B23C-F1CC96198077" );
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02FE499D-DD0A-4B45-9BC0-26CC9826506A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "063F28EC-2D6D-46DC-8857-8E4D98A160B6" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09014CD6-FF97-4453-A884-6D9ED23CFE2C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "87972F2A-F8A6-4554-8CAA-120F532A3CD1" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09014CD6-FF97-4453-A884-6D9ED23CFE2C", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "5CEE2B50-F127-456B-A032-2784815A683C" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09014CD6-FF97-4453-A884-6D9ED23CFE2C", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "86E30229-9E57-4855-99CA-A3E4D7C86330" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09014CD6-FF97-4453-A884-6D9ED23CFE2C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "BE7704BD-FA7B-4391-A207-09E3BFAA3D90" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC3CBE8C-613B-4DCE-9004-55E208441621", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "D185962A-04F2-45F5-8AC7-DBDB7DDB3268" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC3CBE8C-613B-4DCE-9004-55E208441621", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "77AA46A8-9C2E-41C4-B563-5D87103A4E09" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC3CBE8C-613B-4DCE-9004-55E208441621", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "8AD08DA4-75F2-41D9-955A-32070AF7FAA6" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC3CBE8C-613B-4DCE-9004-55E208441621", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "1236BD2C-1B77-4B93-A842-8AA9A9EB1BE5" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B07CDBC0-5EC7-4E4B-A02D-6262C154AC73", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "F483FB69-D612-4D9C-9DD4-F324AF97BA46" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B07CDBC0-5EC7-4E4B-A02D-6262C154AC73", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "93322508-E254-47CA-942D-F759D4C0CF86" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "388B35A5-3D9D-416B-859F-40DE989A8DF5" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "20960B25-C25A-4706-9022-21709E5B2830" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "1FE9EAD1-45AC-44DA-B04C-8188F5C186B4" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "BCC11604-3377-4D03-AF01-D1E961A6302B" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "B7944CA9-F79D-4262-AA49-5DA5C60627ED" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "BA707427-292B-4C4C-AB33-74C98757BC37" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "4B1BDA4A-4464-4073-BE2F-99CFDFD6E5C1" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "8ACA0A59-6D35-47D1-92D1-0CAAD64C295C" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "A61D792E-CC7F-4921-89FE-F272A7D60BD9" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "6BA5C12E-7582-4B2F-95D0-7B49555D6C93" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "317DED94-0A70-44B7-89DB-87203246F8D9" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63ED5175-4CE6-4A4C-B04E-6FC4FF596917", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "9D9925C0-F981-4D59-A864-E9AF3809DF6C" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "389736F5-FBC4-4129-8890-20B8DC063239" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "BD69A7E7-D3FE-4247-9BE1-AC0093470235" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "FDEBD079-D84C-4B2E-8F56-D4DB6CF4AD16" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "634A1905-D272-4783-9FB3-F37E831E2886" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "5D24EB96-88D9-4798-88D8-DA8C71156292" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "9CA36068-B1AB-4984-AF7C-4458D2251707" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "C564AD6A-30E8-445F-9277-2C4091AEE85F" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "FFB7F296-BCBD-4EBF-8531-5F148C50E344" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "901A72CF-D850-492F-A612-EF70200EC04D" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1FE9C03A-E528-46E1-8056-42546C4D929F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "2A51F1D0-398C-47C1-9E0A-03F329837F3B" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C60DF85-FF00-4A4B-A792-7D1E94009237", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "52511203-376F-47F5-A38C-8498B8C46A36" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C60DF85-FF00-4A4B-A792-7D1E94009237", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "68D623C3-608F-45CF-BF48-5D9ADCD3ECCF" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C60DF85-FF00-4A4B-A792-7D1E94009237", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "9A17FF68-D86D-44B4-9210-FA19CA6BFA05" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C60DF85-FF00-4A4B-A792-7D1E94009237", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "D3633D8D-AF88-4C22-9172-5A8926E7EF06" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C60DF85-FF00-4A4B-A792-7D1E94009237", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "A3BA96F8-F716-4B5C-A8B6-9881BD7C4045" );
            // Attrib for BlockType: Content Channel Navigation:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "Detail Page", "Detail Page", @"Page used to view a content item.", 1, @"", "A743F941-3C17-47E4-820B-F645DD3FCF1B" );
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "0E2B924A-C1AC-4A7C-AD77-A036581552D4", "Content Channels Filter", "ContentChannelsFilter", "Content Channels Filter", @"Select the content channels you would like displayed. This setting will override the Content Channel Types Include/Exclude settings.", 4, @"", "477C2574-3DD1-4819-B45F-9B64110B34D2" );
            // Attrib for BlockType: Group Detail:Group RSVP List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group RSVP List Page", "GroupRSVPPage", "Group RSVP List Page", @"The page to manage RSVPs for this group.", 17, @"69285A6B-4DBB-43BB-8B0D-08DEBB860AEA", "06CEB3ED-92F3-47E5-922F-FFDB60D8B78D" );
            // Attrib for BlockType: RSVP Detail:Decline Reasons Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2BC5CC6B-3618-4848-BCD9-1796AA35E7FD", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Decline Reasons Type", "DeclineReasonsType", "Decline Reasons Type", @"", 0, @"1E339D24-3DF3-4628-91C3-DA9300D21ACE", "210D1694-A2F0-4A9E-BBE0-A7E03A6F8C68" );
            // Attrib for BlockType: RSVP List:RSVP Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CE8B41-FD1B-43F2-8C8E-4E878470E8BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "RSVP Detail Page", "RSVPDetailPage", "RSVP Detail Page", @"The Page to displays RSVP Details", 0, @"40E60703-CF52-4742-BDA6-65FB0CF198CB", "A7C6A1AF-CBD2-4932-9C92-E71D16EC61A5" );
            // Attrib for BlockType: RSVP Response:Default Accept Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Default Accept Message", "DefaultAcceptMessage", "Default Accept Message", @"The default message displayed when an RSVP is accepted.", 4, @"We have received your response. Thanks, and we’ll see you soon!", "8B925C04-1070-4C1D-8125-74AAF00E3172" );
            // Attrib for BlockType: RSVP Response:Default Decline Reasons
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Decline Reasons", "DefaultDeclineReasons", "Default Decline Reasons", @"Default Decline Reasons to be displayed.  Setting decline reasons on the Attendance Occurrence will override these.", 6, @"", "625CE6DE-EE30-45CB-9CE6-BDE8F0DE8B48" );
            // Attrib for BlockType: RSVP Response:Multigroup Mode RSVP Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Multigroup Mode RSVP Title", "MultigroupModeRSVPTitle", "Multigroup Mode RSVP Title", @"The page title when a user is RSVPing for multiple groups.", 8, @"RSVP For Events", "8B31A44C-BE48-4E1F-8C66-D1085CF5277B" );
            // Attrib for BlockType: RSVP Response:Multigroup Accept Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Multigroup Accept Message", "MultigroupAcceptMessage", "Multigroup Accept Message", @"The message displayed when one or more RSVPs are accepted in Multigroup mode.  Will include a list of accepted events with the key ""AcceptedRsvps"".", 9, @"", "A2761DC2-112C-421E-B837-A75C6D36FC25" );
            // Attrib for BlockType: RSVP Response:Decline Button Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Decline Button Label", "DeclineButtonLabel", "Decline Button Label", @"The label for the Decline button.", 3, @"Decline", "A14F38CA-437B-4A17-8D7C-86149A7E790C" );
            // Attrib for BlockType: RSVP Response:Default Decline Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Default Decline Message", "DefaultDeclineMessage", "Default Decline Message", @"The default message displayed when an RSVP is declined.", 5, @"Sorry to hear you won’t make it, but hopefully we’ll see you again soon!", "0BDCE48C-4B7E-4188-9A75-05FE42E6DAEF" );
            // Attrib for BlockType: RSVP Response:Display Form When Signed In
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Form When Signed In", "DisplayFormWhenSignedIn", "Display Form When Signed In", @"If signed in and Display Form When Signed In is disabled, only the accept and decline buttons are shown.", 0, @"True", "AC2DE0DC-A360-406E-B3CF-99C3F7037CCE" );
            // Attrib for BlockType: RSVP Response:Accept Button Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EEFD83FB-6EE1-44F4-A012-7569F979CD6B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Accept Button Label", "AcceptButtonLabel", "Accept Button Label", @"The label for the Accept button.", 2, @"Accept", "1E6122A2-B4CA-4C99-A0A7-8374DD6D7D74" );
            RockMigrationHelper.UpdateFieldType("Content Channels","","Rock","Rock.Field.Types.ContentChannelsFieldType","0E2B924A-C1AC-4A7C-AD77-A036581552D4");

        }

        private void CodeGenMigrationsDown()
        {
                        // Attrib for BlockType: RSVP Response:Accept Button Label
            RockMigrationHelper.DeleteAttribute("1E6122A2-B4CA-4C99-A0A7-8374DD6D7D74");
            // Attrib for BlockType: RSVP Response:Display Form When Signed In
            RockMigrationHelper.DeleteAttribute("AC2DE0DC-A360-406E-B3CF-99C3F7037CCE");
            // Attrib for BlockType: RSVP Response:Default Decline Message
            RockMigrationHelper.DeleteAttribute("0BDCE48C-4B7E-4188-9A75-05FE42E6DAEF");
            // Attrib for BlockType: RSVP Response:Decline Button Label
            RockMigrationHelper.DeleteAttribute("A14F38CA-437B-4A17-8D7C-86149A7E790C");
            // Attrib for BlockType: RSVP Response:Multigroup Accept Message
            RockMigrationHelper.DeleteAttribute("A2761DC2-112C-421E-B837-A75C6D36FC25");
            // Attrib for BlockType: RSVP Response:Multigroup Mode RSVP Title
            RockMigrationHelper.DeleteAttribute("8B31A44C-BE48-4E1F-8C66-D1085CF5277B");
            // Attrib for BlockType: RSVP Response:Default Decline Reasons
            RockMigrationHelper.DeleteAttribute("625CE6DE-EE30-45CB-9CE6-BDE8F0DE8B48");
            // Attrib for BlockType: RSVP Response:Default Accept Message
            RockMigrationHelper.DeleteAttribute("8B925C04-1070-4C1D-8125-74AAF00E3172");
            // Attrib for BlockType: RSVP List:RSVP Detail Page
            RockMigrationHelper.DeleteAttribute("A7C6A1AF-CBD2-4932-9C92-E71D16EC61A5");
            // Attrib for BlockType: RSVP Detail:Decline Reasons Type
            RockMigrationHelper.DeleteAttribute("210D1694-A2F0-4A9E-BBE0-A7E03A6F8C68");
            // Attrib for BlockType: Group Detail:Group RSVP List Page
            RockMigrationHelper.DeleteAttribute("06CEB3ED-92F3-47E5-922F-FFDB60D8B78D");
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.DeleteAttribute("477C2574-3DD1-4819-B45F-9B64110B34D2");
            // Attrib for BlockType: Content Channel Navigation:Detail Page
            RockMigrationHelper.DeleteAttribute("A743F941-3C17-47E4-820B-F645DD3FCF1B");
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("A3BA96F8-F716-4B5C-A8B6-9881BD7C4045");
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("D3633D8D-AF88-4C22-9172-5A8926E7EF06");
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("9A17FF68-D86D-44B4-9210-FA19CA6BFA05");
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("68D623C3-608F-45CF-BF48-5D9ADCD3ECCF");
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("52511203-376F-47F5-A38C-8498B8C46A36");
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("2A51F1D0-398C-47C1-9E0A-03F329837F3B");
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("901A72CF-D850-492F-A612-EF70200EC04D");
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute("FFB7F296-BCBD-4EBF-8531-5F148C50E344");
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute("C564AD6A-30E8-445F-9277-2C4091AEE85F");
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute("9CA36068-B1AB-4984-AF7C-4458D2251707");
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute("5D24EB96-88D9-4798-88D8-DA8C71156292");
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("634A1905-D272-4783-9FB3-F37E831E2886");
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("FDEBD079-D84C-4B2E-8F56-D4DB6CF4AD16");
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute("BD69A7E7-D3FE-4247-9BE1-AC0093470235");
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute("389736F5-FBC4-4129-8890-20B8DC063239");
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("9D9925C0-F981-4D59-A864-E9AF3809DF6C");
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("317DED94-0A70-44B7-89DB-87203246F8D9");
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("6BA5C12E-7582-4B2F-95D0-7B49555D6C93");
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("A61D792E-CC7F-4921-89FE-F272A7D60BD9");
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("8ACA0A59-6D35-47D1-92D1-0CAAD64C295C");
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("4B1BDA4A-4464-4073-BE2F-99CFDFD6E5C1");
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("BA707427-292B-4C4C-AB33-74C98757BC37");
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("B7944CA9-F79D-4262-AA49-5DA5C60627ED");
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("BCC11604-3377-4D03-AF01-D1E961A6302B");
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("1FE9EAD1-45AC-44DA-B04C-8188F5C186B4");
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("20960B25-C25A-4706-9022-21709E5B2830");
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("388B35A5-3D9D-416B-859F-40DE989A8DF5");
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("93322508-E254-47CA-942D-F759D4C0CF86");
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute("F483FB69-D612-4D9C-9DD4-F324AF97BA46");
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("1236BD2C-1B77-4B93-A842-8AA9A9EB1BE5");
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute("8AD08DA4-75F2-41D9-955A-32070AF7FAA6");
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("77AA46A8-9C2E-41C4-B563-5D87103A4E09");
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute("D185962A-04F2-45F5-8AC7-DBDB7DDB3268");
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute("BE7704BD-FA7B-4391-A207-09E3BFAA3D90");
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute("86E30229-9E57-4855-99CA-A3E4D7C86330");
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("5CEE2B50-F127-456B-A032-2784815A683C");
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute("87972F2A-F8A6-4554-8CAA-120F532A3CD1");
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.DeleteAttribute("063F28EC-2D6D-46DC-8857-8E4D98A160B6");
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("8C5D0BFB-017E-4885-B23C-F1CC96198077");
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute("E5532E34-519E-40C6-8EE2-860E81C84C52");
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute("34DB685B-B52A-4D1C-A467-44B8DB628956");
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute("15C21D62-4F6D-4F86-B2D0-163FD5D14C34");
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute("9A395CB7-65F8-49C1-8D68-9DF0C04DF3C3");
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute("D11DE08F-F1F0-429F-8342-6F25FFB3292A");
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute("CBD2A1FD-6DAB-4134-8C01-76C72C50B267");
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("307EAF97-77FB-43DE-A809-D12E348DB907");
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute("A8AD9B03-15BB-4338-B0DD-5EE4F9BDD1E0");
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute("9695DCA1-86C2-4F4C-B9F5-BF90E2178F4D");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute("F1E59A6B-A64E-4217-930B-B90EFE1CB3D5");
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.DeleteAttribute("B7EF8FD2-83F2-468D-9E7B-8FFC337B2964");
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("D0879626-3C5E-4B5D-9DEA-56563F615B85");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("1E4C1A4E-952D-43A3-97EF-9B747CDBCA70");
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("E50E5ADB-6CF0-4CBE-BA83-E7380BCD5AB5");
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("6D7108BD-9CCA-4886-9F9A-D0A01DB3FDC3");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute("59B2A109-E266-42C5-838E-42AFD15AACC0");
            RockMigrationHelper.DeleteBlockType("3C60DF85-FF00-4A4B-A792-7D1E94009237"); // Workflow Entry
            RockMigrationHelper.DeleteBlockType("1FE9C03A-E528-46E1-8056-42546C4D929F"); // Register
            RockMigrationHelper.DeleteBlockType("63ED5175-4CE6-4A4C-B04E-6FC4FF596917"); // Profile Details
            RockMigrationHelper.DeleteBlockType("B07CDBC0-5EC7-4E4B-A02D-6262C154AC73"); // Login
            RockMigrationHelper.DeleteBlockType("DC3CBE8C-613B-4DCE-9004-55E208441621"); // Lava Item List
            RockMigrationHelper.DeleteBlockType("09014CD6-FF97-4453-A884-6D9ED23CFE2C"); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType("02FE499D-DD0A-4B45-9BC0-26CC9826506A"); // Content Channel Item List
            RockMigrationHelper.DeleteBlockType("F5EBD7D8-7BED-4B11-8842-2E1542E9B3A1"); // Content
        }

        /// <summary>
        /// NA: Fix Typo in the Summary text (attribute) of the Motivators Theme (DefinedType) for Positional (DefinedValue)
        /// </summary>
        private void FixMotivatorsPositionalSummaryText()
        {
            Sql( @"
                DECLARE @DefinedValueId int
                SET @DefinedValueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '84322020-4E27-44EF-88F2-EAFDB7286A01')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '07E85FA1-8F86-4414-8DC3-43D303C55457')

                IF @DefinedValueId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN

                    UPDATE [AttributeValue] 
                    SET [Value] = REPLACE( [Value],'motivators int this', 'motivators in this' ) 
                    WHERE [AttributeId] = @AttributeId AND [EntityId] = @DefinedValueId

                END" );
        }

        /// <summary>
        /// DL: Remove Email Analytics Page
        /// Remove the Email Analytics page and associated links and settings.
        /// The EmailAnalytics block is marked as obsolete in v10, but will not be removed until a later version.
        /// </summary>
        private void RemoveEmailAnalyticsPageUp()
        {
	        const string emailAnalyticsPageGuid = "DF014200-72A3-48A0-A953-E594E5410E36";
	        const string emailAnalyticsBlockGuid = "DC951B4F-0F07-47C3-A279-D1AFA1C50549";
	        const string emailAnalyticsBlockSeriesColorAttributeGuid = "0EF39AAC-E6EA-426B-802A-3212CE52F245";
	        const string communicationsListEmailAnalyticsLinkedPageAttributeGuid = "0B28D804-634A-40CF-AF8B-BD37E1E7A7C6";

	        // Delete Page: Email Analytics
	        RockMigrationHelper.DeletePage( emailAnalyticsPageGuid );

	        // Delete Communications List Linked Page Attribute: Communications List/Email Analytics
	        RockMigrationHelper.DeleteAttribute( communicationsListEmailAnalyticsLinkedPageAttributeGuid );

	        // Delete Block instance Attribute Values: Email Analytics/Series Colors
	        RockMigrationHelper.DeleteBlockAttributeValue( emailAnalyticsBlockGuid, emailAnalyticsBlockSeriesColorAttributeGuid );
        }

        /// <summary>
        /// JE: Update Step Type page title
        /// </summary>
        private void UpdateStepTypePageTitle()
        {
            Sql( @"
                UPDATE [Page] 
                SET [PageTitle] = 'Step Type', [BrowserTitle] = 'Step Type', [InternalName] = 'Step Type'
                WHERE [Guid] = '8E78F9DC-657D-41BF-BE0F-56916B6BF92F'" );
        }

        /// <summary>
        /// JE: Remove Mobile Dynamic Content Block
        /// </summary>
        private void RemoveMobileDynamicContentBlock()
        {
            Sql( @"
                DELETE B
                FROM [Block] AS B
                INNER JOIN [BlockType] AS BT ON BT.[Id] = B.[BlockTypeId]
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.DynamicContent'

                DELETE A
                FROM [Attribute] AS A
                INNER JOIN [BlockType] AS BT ON BT.[Id] = TRY_CAST(A.[EntityTypeQualifierValue] AS [int])
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
                  AND ET.[Name] = 'Rock.Blocks.Types.Mobile.DynamicContent'

                DELETE BT
                FROM [BlockType] AS BT
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.DynamicContent'

                DELETE ET
                FROM [EntityType] AS ET
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.DynamicContent'" );
        }

        /// <summary>
        /// JE: Delete the Mobile Image Block
        /// </summary>
        private void RemoveMobileImageBlock()
        {
            Sql( @"
                DELETE B
                FROM [Block] AS B
                INNER JOIN [BlockType] AS BT ON BT.[Id] = B.[BlockTypeId]
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.Image'

                DELETE A
                FROM [Attribute] AS A
                INNER JOIN [BlockType] AS BT ON BT.[Id] = TRY_CAST(A.[EntityTypeQualifierValue] AS [int])
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
                  AND ET.[Name] = 'Rock.Blocks.Types.Mobile.Image'

                DELETE BT
                FROM [BlockType] AS BT
                INNER JOIN [EntityType] AS ET ON ET.[Id] = BT.[EntityTypeId]
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.Image'

                DELETE ET
                FROM [EntityType] AS ET
                WHERE ET.[Name] = 'Rock.Blocks.Types.Mobile.Image'" );
        }
    }
}
