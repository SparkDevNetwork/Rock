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
    public partial class Rollup_1204 : Rock.Migrations.RockMigration
    {
        
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixDQSocialMediaLinks();
            DisableInteractionSessionsListPageSearchIndex();
            ChartJsLavaShortCode();
            FixDefinedTypeDescriptionFormatting();
            MobileCategories();
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
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5D79FCA-1700-4B3B-87AA-82922C0FDA42", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "34B26D1E-07F0-4E81-8658-0BEA9976732C" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5D79FCA-1700-4B3B-87AA-82922C0FDA42", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "E0AEA691-D787-4A18-A49A-9F0CA4AD8F48" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5D79FCA-1700-4B3B-87AA-82922C0FDA42", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server on each page (taking cache duration into account), otherwise the content will remain static.", 2, @"False", "4BADFA20-C0F0-4EFE-ACC5-7191DB04CFF3" );
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5D79FCA-1700-4B3B-87AA-82922C0FDA42", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Callback Logic", "CallbackLogic", "Callback Logic", @"If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>", 0, @"", "C3B230AB-F824-4220-9113-8E0BC16535AB" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "1F5D2D3A-F7CF-413A-BA17-20247F439EBA" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "7F670787-6271-4A88-B99D-353B08809E92" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "AEBEF472-6D49-422E-BE4D-A3B8D532A332" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "BC68D240-8E7A-4448-A9DC-F40053A056DB" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "F58B6604-AEF2-48E8-A62E-E5DA1473B196" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "C341A412-D9FF-4B9A-9929-B6923D1D87CB" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "4F8CD94F-3A4C-4BAA-9871-8070196131FA" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "92BC7E67-EFFF-4825-A733-5034558F67A0" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "2D282BB7-40CE-4343-A036-428C8877A854" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "621FC2A5-C6D2-48B4-8E40-FF1D1E98BCC3" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
</StackLayout>", "F48B064B-7CE5-4BD7-BD47-131387BC43D3" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56CC8D73-CDF2-46B9-9777-601DFCFDD6D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "031A4C54-5647-40EA-ADCE-7E452EFAE4E7" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56CC8D73-CDF2-46B9-9777-601DFCFDD6D2", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "081E7B66-3CCB-48E3-84E4-1B5CF3D70007" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56CC8D73-CDF2-46B9-9777-601DFCFDD6D2", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "FF162EA4-517C-4800-955D-1C632E1A9903" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56CC8D73-CDF2-46B9-9777-601DFCFDD6D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "3A8D7441-DD0B-4837-86B0-146DB34C06EB" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "471DB3E0-1C45-4098-BADD-D0A70DE087D5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "FD2B6E3B-58D9-478C-9F1D-6B79F672AAF3" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "471DB3E0-1C45-4098-BADD-D0A70DE087D5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "3004AAA7-1277-4911-BCB2-BBC566CD1B53" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "471DB3E0-1C45-4098-BADD-D0A70DE087D5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "6786F1B4-A98C-463B-A6C4-D9544591E5A1" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "471DB3E0-1C45-4098-BADD-D0A70DE087D5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "6F0EC707-2198-4185-AE59-B34E0C4F481A" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "15EFBC65-A18C-4BA6-847B-6981B1FE37CC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "9CBA18CB-FC6A-4EDC-A4E3-C4916AA134C4" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "15EFBC65-A18C-4BA6-847B-6981B1FE37CC", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "9292F26E-5A31-45C0-94E5-F8E9573547FD" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "4C56DFB3-3E77-435C-A971-93F64E19408A" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "BBB950CA-0486-418E-8473-027FAAF3F322" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "1F7E218C-E2FF-415D-9ABD-5002D40FC857" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "53249F89-5FD8-4A5A-B11C-CFE660C36405" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "AA350F64-8358-4C85-8689-B4CFF6FED302" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "1E10F1D2-7242-41FD-8B88-F20FD0D144DD" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "8424DF5A-ECC7-4E3B-8097-4128CBD50D79" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "F7FD3009-7009-44BA-9089-B07D156E3C32" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "D0569DF4-07B6-4B00-827F-81F2963E666A" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "EC7EF859-0B0C-405A-912D-EEAAE075A239" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "11CC4E71-0BEC-47B8-B6E0-D6E0FE16A034" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "36DFF585-1AD8-43AF-804A-46A40BC62816", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "C691A642-68CE-430E-87A9-D89F1B7F3122" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "65980998-9667-40B5-8951-33ABA1931C31" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "7F199BA9-C97B-41DB-95CC-7906EAC86784" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "7A59BAA3-81D7-4718-80EE-13B705E9E436" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "0595BD35-60B6-4341-B2E8-ED3D64AAA601" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "934E903E-B417-46B3-8921-72547839F569" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "1C909594-963A-4832-990F-83A3B567CFD5" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "BAAD30CD-2339-487A-A348-EC23C3564675" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "FEF3FEF1-D63F-4E5F-9A7F-541869292C9E" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "A22DB679-4C29-4ADA-B314-17264FD09FC4" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E65DBC8C-B8DF-4961-824F-643623AFF262", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "B7238A1A-D4C2-46D0-8616-95F6F6BFFD22" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "0128649B-1738-402A-9EF2-95A12998C72A" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "8699F784-A9A7-4E67-9406-B7FEEF3F048F" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "46AEF3CC-3130-47CE-B274-E5AE5914D81F" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "8F85702D-299D-4D7C-948C-53763AE3FB82" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "6AEBA1E7-D2BF-4C5E-868C-F75D2276753C" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "7AFDEC25-68AE-4AD1-A792-707F79707421" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "0B3A812D-B6D7-48F4-BE8C-D93B8FCC7446" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "34A02AB8-9AFE-4736-AA67-79A36847A1A8" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "4015B0A6-B3C8-417E-9947-EA407FBC4A23" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "9EFB0FD4-B7D0-4CD1-8A59-E318CFF8BD05" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "BDE82E60-0EEB-42F7-9B7A-5F387AC30467" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "5615F612-7888-49F7-B953-B51D05E329CA" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "8A2221F6-D8CA-4770-B273-0AA00DB8369B" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "CBB13468-B2CD-408B-A839-946B85EF958C" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "76FDDA7E-B949-41A0-9E63-C158BFE2621B" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "59B63E59-566B-4B74-8469-D96682956F93" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "F7CED533-D6CA-440C-A735-257DE46CE85F" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "3396939D-545E-466D-A932-B8931D1AE501" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50E1866B-0674-4A13-BE9E-32C656621DAB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "0C3085A3-A780-4B07-9553-02725FD6DACB" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C455663-4E6F-47BF-AB80-5C72415DFE85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "960425B1-B396-4804-B206-87F0B1A39553" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C455663-4E6F-47BF-AB80-5C72415DFE85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "915400F1-CC10-4B17-9BF3-C3EC163DD360" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C455663-4E6F-47BF-AB80-5C72415DFE85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "7717FBBA-6C70-47E8-A26E-4300493C9EF2" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C455663-4E6F-47BF-AB80-5C72415DFE85", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "9B0D7749-4B21-4C96-AF25-35288FFE145D" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C455663-4E6F-47BF-AB80-5C72415DFE85", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "BB4A149D-9273-4210-BCEC-AF9C945EADCD" );
            // Attrib for BlockType: Group List:Root Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Root Group", "RootGroup", "Root Group", @"Select the root group to use as a starting point for the tree view.", 17, @"", "AC22E9D0-B37C-4F3E-9EB6-CA94CBCA3873" );
            // Attrib for BlockType: Group List:Group Picker Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Picker Type", "GroupPickerType", "Group Picker Type", @"Used to control which kind of picker is used when adding a person to a group.", 16, @"Dropdown", "77307E9F-CE10-4285-A0A5-418D324A4576" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Group List:Group Picker Type
            RockMigrationHelper.DeleteAttribute( "77307E9F-CE10-4285-A0A5-418D324A4576" );
            // Attrib for BlockType: Group List:Root Group
            RockMigrationHelper.DeleteAttribute( "AC22E9D0-B37C-4F3E-9EB6-CA94CBCA3873" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute( "BB4A149D-9273-4210-BCEC-AF9C945EADCD" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute( "9B0D7749-4B21-4C96-AF25-35288FFE145D" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute( "7717FBBA-6C70-47E8-A26E-4300493C9EF2" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute( "915400F1-CC10-4B17-9BF3-C3EC163DD360" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute( "960425B1-B396-4804-B206-87F0B1A39553" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "0C3085A3-A780-4B07-9553-02725FD6DACB" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute( "3396939D-545E-466D-A932-B8931D1AE501" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute( "F7CED533-D6CA-440C-A735-257DE46CE85F" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute( "59B63E59-566B-4B74-8469-D96682956F93" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute( "76FDDA7E-B949-41A0-9E63-C158BFE2621B" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute( "CBB13468-B2CD-408B-A839-946B85EF958C" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute( "8A2221F6-D8CA-4770-B273-0AA00DB8369B" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute( "5615F612-7888-49F7-B953-B51D05E329CA" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute( "BDE82E60-0EEB-42F7-9B7A-5F387AC30467" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute( "9EFB0FD4-B7D0-4CD1-8A59-E318CFF8BD05" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute( "4015B0A6-B3C8-417E-9947-EA407FBC4A23" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute( "34A02AB8-9AFE-4736-AA67-79A36847A1A8" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute( "0B3A812D-B6D7-48F4-BE8C-D93B8FCC7446" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute( "7AFDEC25-68AE-4AD1-A792-707F79707421" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute( "6AEBA1E7-D2BF-4C5E-868C-F75D2276753C" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "8F85702D-299D-4D7C-948C-53763AE3FB82" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute( "46AEF3CC-3130-47CE-B274-E5AE5914D81F" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute( "8699F784-A9A7-4E67-9406-B7FEEF3F048F" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute( "0128649B-1738-402A-9EF2-95A12998C72A" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute( "B7238A1A-D4C2-46D0-8616-95F6F6BFFD22" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute( "A22DB679-4C29-4ADA-B314-17264FD09FC4" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute( "FEF3FEF1-D63F-4E5F-9A7F-541869292C9E" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute( "BAAD30CD-2339-487A-A348-EC23C3564675" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute( "1C909594-963A-4832-990F-83A3B567CFD5" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute( "934E903E-B417-46B3-8921-72547839F569" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute( "0595BD35-60B6-4341-B2E8-ED3D64AAA601" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute( "7A59BAA3-81D7-4718-80EE-13B705E9E436" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute( "7F199BA9-C97B-41DB-95CC-7906EAC86784" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute( "65980998-9667-40B5-8951-33ABA1931C31" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute( "C691A642-68CE-430E-87A9-D89F1B7F3122" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute( "11CC4E71-0BEC-47B8-B6E0-D6E0FE16A034" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute( "EC7EF859-0B0C-405A-912D-EEAAE075A239" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute( "D0569DF4-07B6-4B00-827F-81F2963E666A" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute( "F7FD3009-7009-44BA-9089-B07D156E3C32" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute( "8424DF5A-ECC7-4E3B-8097-4128CBD50D79" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute( "1E10F1D2-7242-41FD-8B88-F20FD0D144DD" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute( "AA350F64-8358-4C85-8689-B4CFF6FED302" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute( "53249F89-5FD8-4A5A-B11C-CFE660C36405" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute( "1F7E218C-E2FF-415D-9ABD-5002D40FC857" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute( "BBB950CA-0486-418E-8473-027FAAF3F322" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute( "4C56DFB3-3E77-435C-A971-93F64E19408A" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute( "9292F26E-5A31-45C0-94E5-F8E9573547FD" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute( "9CBA18CB-FC6A-4EDC-A4E3-C4916AA134C4" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute( "6F0EC707-2198-4185-AE59-B34E0C4F481A" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute( "6786F1B4-A98C-463B-A6C4-D9544591E5A1" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute( "3004AAA7-1277-4911-BCB2-BBC566CD1B53" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute( "FD2B6E3B-58D9-478C-9F1D-6B79F672AAF3" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute( "3A8D7441-DD0B-4837-86B0-146DB34C06EB" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute( "FF162EA4-517C-4800-955D-1C632E1A9903" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "081E7B66-3CCB-48E3-84E4-1B5CF3D70007" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute( "031A4C54-5647-40EA-ADCE-7E452EFAE4E7" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute( "F48B064B-7CE5-4BD7-BD47-131387BC43D3" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute( "621FC2A5-C6D2-48B4-8E40-FF1D1E98BCC3" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute( "2D282BB7-40CE-4343-A036-428C8877A854" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute( "92BC7E67-EFFF-4825-A733-5034558F67A0" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute( "4F8CD94F-3A4C-4BAA-9871-8070196131FA" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute( "C341A412-D9FF-4B9A-9929-B6923D1D87CB" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute( "F58B6604-AEF2-48E8-A62E-E5DA1473B196" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute( "BC68D240-8E7A-4448-A9DC-F40053A056DB" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute( "AEBEF472-6D49-422E-BE4D-A3B8D532A332" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute( "7F670787-6271-4A88-B99D-353B08809E92" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute( "1F5D2D3A-F7CF-413A-BA17-20247F439EBA" );
            // Attrib for BlockType: Content:Callback Logic
            RockMigrationHelper.DeleteAttribute( "C3B230AB-F824-4220-9113-8E0BC16535AB" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute( "4BADFA20-C0F0-4EFE-ACC5-7191DB04CFF3" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "E0AEA691-D787-4A18-A49A-9F0CA4AD8F48" );
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute( "34B26D1E-07F0-4E81-8658-0BEA9976732C" );
            RockMigrationHelper.DeleteBlockType( "2C455663-4E6F-47BF-AB80-5C72415DFE85" ); // Group Member Edit
            RockMigrationHelper.DeleteBlockType( "50E1866B-0674-4A13-BE9E-32C656621DAB" ); // Group Edit
            RockMigrationHelper.DeleteBlockType( "72EA2519-2CE4-4D16-93DF-AFA2AF70A730" ); // Workflow Entry
            RockMigrationHelper.DeleteBlockType( "E65DBC8C-B8DF-4961-824F-643623AFF262" ); // Register
            RockMigrationHelper.DeleteBlockType( "36DFF585-1AD8-43AF-804A-46A40BC62816" ); // Profile Details
            RockMigrationHelper.DeleteBlockType( "15EFBC65-A18C-4BA6-847B-6981B1FE37CC" ); // Login
            RockMigrationHelper.DeleteBlockType( "471DB3E0-1C45-4098-BADD-D0A70DE087D5" ); // Lava Item List
            RockMigrationHelper.DeleteBlockType( "56CC8D73-CDF2-46B9-9777-601DFCFDD6D2" ); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType( "C21B4F1B-09C4-4DCD-85A2-FD3F07024ECF" ); // Content Channel Item List
            RockMigrationHelper.DeleteBlockType( "A5D79FCA-1700-4B3B-87AA-82922C0FDA42" ); // Content
        }

        /// <summary>
        /// GJ: Fix double quoted Social Media 
        /// </summary>
        private void FixDQSocialMediaLinks()
        {
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "BC8F9FEF-59D6-4BC4-84B3-BC6EC52CECED" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "6FFF488B-C7A8-410A-ADC2-3D9D21706511" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "02820F4F-476A-448F-A869-14206625670C" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "7B3650EF-8F42-40DF-A729-9BEF19941DD8" );
        }

        /// <summary>
        /// ED: Don't allow indexing on by default on Interaction Sessions List page
        /// </summary>
        private void DisableInteractionSessionsListPageSearchIndex()
        {
            Sql( @" UPDATE [Page] SET AllowIndexing = 0 WHERE [Guid] = '756D37B7-7BE2-497D-8D37-CC273FE29659'" );
        }

        /// <summary>
        /// GJ: Fix ChartJS Lava Shortcode
        /// </summary>
        private void ChartJsLavaShortCode()
        {
            Sql( MigrationSQL._201912042320379_Rollup_1204_chartjsfix );
        }

        /// <summary>
        /// GJ: Fix Defined Type Description formatting
        /// </summary>
        private void FixDefinedTypeDescriptionFormatting()
        {
            Sql( @"
            UPDATE [DefinedType] SET [Description]=N'By default, Rock does not share saved login information across domains. For example if a user logs in from <i>http://<strong>www</strong>.rocksolidchurch.com</i>, they would also have to login at <i>http://<strong>admin</strong>.rocksolidchurch.com</i>. You can override this behavior so that all hosts of common domain share their login status. So in the case above, if <i>rocksolidchurchdemo.com</i> was entered below, logging into the <strong>www</strong> site would also auto log you into the <strong>admin</strong> site.' WHERE ([Guid]='6CE00E1B-FE09-45FE-BD9D-56C57A11BE1A')
            UPDATE [DefinedType] SET [Description]=N'Lists the external domains that are authorized to access the REST API through ""cross-origin resource sharing"" (CORS).' WHERE ([Guid]='DF7C8DF7-49F9-4858-9E5D-20842AF65AD8')
            UPDATE [DefinedType] SET [Description]=N'Defines preset colors shown inside of Color Picker controls.' WHERE ([Guid]='CC1400B3-E161-45E3-BF49-49825D3D6467')
            UPDATE [DefinedType] SET [Description]=N'Used by Rock''s Conflict Profile Assessment to hold all Conflict Themes.' WHERE ([Guid]='EE7E089E-DF81-4407-8BFA-AD865FA5427A')" );
        }

        /// <summary>
        /// DH/JE: Mobile Categories
        /// </summary>
        private void MobileCategories()
        {
            Sql( @"
                UPDATE [EntityType]
                SET [Name] = REPLACE([Name], 'Rock.Blocks.Types.Mobile.', 'Rock.Blocks.Types.Mobile.Cms.'),
                    [AssemblyName] = REPLACE([AssemblyName], 'Rock.Blocks.Types.Mobile.', 'Rock.Blocks.Types.Mobile.Cms.')
                WHERE [Name] IN ('Rock.Blocks.Types.Mobile.Content',
                    'Rock.Blocks.Types.Mobile.ContentChannelItemList',
                    'Rock.Blocks.Types.Mobile.ContentChannelItemView',
                    'Rock.Blocks.Types.Mobile.LavaItemList',
                    'Rock.Blocks.Types.Mobile.Login',
                    'Rock.Blocks.Types.Mobile.ProfileDetails',
                    'Rock.Blocks.Types.Mobile.Register',
                    'Rock.Blocks.Types.Mobile.WorkflowEntry')

                UPDATE [BT]
                SET [BT].[Category] = 'Mobile > Cms'
                FROM [BlockType] AS [BT]
                INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [BT].[EntityTypeId]
                WHERE [ET].[Name] IN ('Rock.Blocks.Types.Mobile.Cms.Content',
                    'Rock.Blocks.Types.Mobile.Cms.ContentChannelItemList',
                    'Rock.Blocks.Types.Mobile.Cms.ContentChannelItemView',
                    'Rock.Blocks.Types.Mobile.Cms.LavaItemList',
                    'Rock.Blocks.Types.Mobile.Cms.Login',
                    'Rock.Blocks.Types.Mobile.Cms.ProfileDetails',
                    'Rock.Blocks.Types.Mobile.Cms.Register',
                    'Rock.Blocks.Types.Mobile.Cms.WorkflowEntry')" );
        }

    }
}
