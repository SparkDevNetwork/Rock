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
    public partial class Rollup_0904 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            CreateNonCashAssetTypes();
            UpdateWorkflowActionRunLava();
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
            RockMigrationHelper.UpdateBlockType("Persisted Dataset Detail","Edit details of a Persisted Dataset","~/Blocks/Cms/PersistedDatasetDetail.ascx","CMS","ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F");
            RockMigrationHelper.UpdateBlockType("Persisted Dataset List","Lists Persisted Datasets","~/Blocks/Cms/PersistedDatasetList.ascx","CMS","50ADE904-BB5C-40F9-A97D-ED8FF530B5A6");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AFBB966-3F41-4081-895E-A0C58210BB75", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "39B1BA8A-4EDB-4E76-BC3F-8F7ED98A2949" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AFBB966-3F41-4081-895E-A0C58210BB75", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "F9C73B8C-A2EE-4A3D-98CB-9D405620F41C" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AFBB966-3F41-4081-895E-A0C58210BB75", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "5E1A3CD6-4D3B-4653-ABCB-12090809F9C6" );
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AFBB966-3F41-4081-895E-A0C58210BB75", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "5F042523-9DD6-4C5D-A087-EA9728A6108F" );
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9AFBB966-3F41-4081-895E-A0C58210BB75", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "83B1311D-C3AB-4B2D-8832-623591C261C7" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "669FC130-C620-4DD5-9EB0-B04568F02EA2" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "89BA2D96-9DDD-4A4E-8D43-B67717FB971F" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "C2192E49-AAA7-4B98-BF6B-2B09F217E9FF" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "EB9F2F93-2C19-49CB-8A59-F74643F2685A" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "52DC839F-741A-46E5-AFF1-6DD00AB83C67" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "3B9478FD-DCB8-4E06-9932-BE5C72F7500B" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "E32644C3-681D-4557-88C1-ACC3D2A27D52" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "C8A1B867-2F1D-4B10-ACA0-BE3E0A07D77A" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "093FEE0E-78F1-4D48-85BD-6B38A4628E3A" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "350594D6-19CB-4BAA-9831-1046E279494B" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
</StackLayout>", "C1D452F0-D99B-425C-9ABA-C1FF52FF53EF" );
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DB613CF-5732-4515-81B5-46CDC42109E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "C54D2964-3BD5-4305-9391-ADBA39166907" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D8F7C7-9D5E-4A02-9216-05B73BCE42BF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "DA54EC14-0FE3-47CC-AB5C-5A57FC1114A0" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D8F7C7-9D5E-4A02-9216-05B73BCE42BF", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "3B02E02F-E90D-423C-BC55-208D7E9EB656" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D8F7C7-9D5E-4A02-9216-05B73BCE42BF", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "099C4B0A-3B19-45BF-BE52-F332F97291D3" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56D8F7C7-9D5E-4A02-9216-05B73BCE42BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "635686C9-9C9F-4745-980A-3D5EB45050EA" );
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B45710-616F-4A35-9941-5560992ABBCB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "1746D9EE-9877-4CFF-84B4-185779A5200D" );
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B45710-616F-4A35-9941-5560992ABBCB", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "844E5167-13EF-4826-9CBD-C4837AA659BC" );
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39B45710-616F-4A35-9941-5560992ABBCB", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Content", "InitialContent", "Initial Content", @"If the initial content should be static or dynamic.", 2, @"Static", "FFD1F06A-CE6E-438A-B56D-8AD5F4F00487" );
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3C6C9FF-288F-4499-924C-3451C7048BDD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Image Url", "ImageUrl", "Image Url", @"The URL to use for displaying the image. <span class='tip tip-lava'></span>", 0, @"", "8BA82F56-1606-4415-A531-76F85E904369" );
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3C6C9FF-288F-4499-924C-3451C7048BDD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "69EA900E-311C-466D-B81A-C5796810DB49" );
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3C6C9FF-288F-4499-924C-3451C7048BDD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "05935D2E-2C85-4F34-9A9B-3FABC8DCF48A" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE189BF0-61A4-4E0A-8B6A-880D04996CC8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "EECDB7D4-3150-482B-9BFA-6DC906DF3E01" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE189BF0-61A4-4E0A-8B6A-880D04996CC8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "EA642CDE-065C-4948-8A95-3E0729DDC48B" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE189BF0-61A4-4E0A-8B6A-880D04996CC8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "A06CB4FF-7167-4513-BE51-C87A61F1A32D" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE189BF0-61A4-4E0A-8B6A-880D04996CC8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "6D6B5870-9688-40E5-8E61-27225EEEDC91" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9333B74-B537-439E-9AC4-C33ECE5AB341", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "CDC1EE41-A875-40A6-A41C-7F2951CB24B0" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9333B74-B537-439E-9AC4-C33ECE5AB341", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "6FABBF6E-F974-4376-82DD-B58003187F3E" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "E2DF8CEF-CA55-4C0A-A061-DA5287D8A309" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "0D2F525D-CBA2-4F90-B0AF-2823D395F352" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "B04EC327-461C-43B8-A2A0-1ADC235E719B" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "9597075B-1727-4F88-8B84-C09245ADEA73" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "95AA7A82-F987-429A-BC22-C57CC06772FE" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "EE072625-AF72-45A8-9A64-DC333505AE60" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "5D15FACA-D5EB-4485-A4C6-DC56414CD9B0" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "DFD76261-884F-4E53-AE2B-C86C3B098BC6" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "5E90BA6A-5B4F-4776-96A6-E1F7D52730CE" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "561462FC-273A-481E-B5C1-F27822267363" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "5F0D5180-5E44-4AEE-8852-023CC982EDA6" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8664A6FE-0366-4F47-B33A-44523F1EC43F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "D5AA29DE-C9E3-4734-9C20-B5CFC00E79E6" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "06C9C7AB-D4B1-4043-A299-492CDF3E24A6" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "540DFAE6-C3FD-46EF-9D77-FB727EA8ABEF" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "F6D43EA8-53A0-4ED4-A8F4-AE335D4EF135" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "3D8BBF0E-7C98-4858-BA2B-7FB24541FFFF" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "363353CF-1B62-44BC-838B-9F2EC55D1D78" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "147FE149-C9F0-4E7B-B727-D0268010A25B" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "FD67B645-A363-4D48-8743-C2BFAC5BF634" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "8032CC03-C135-4DE3-B79B-F3776A98E7AB" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "D22B8615-FE9B-4F27-9A23-0A4C84807E93" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "8FBD0A26-0A11-4853-9325-D194C0A1375F" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC975C40-BE7C-4589-AF2E-42F8558C6B63", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "B86D5A17-1859-44AF-B8F0-98B0551BBF59" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC975C40-BE7C-4589-AF2E-42F8558C6B63", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "114E6004-B58D-4450-A319-C49DF1816BA9" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC975C40-BE7C-4589-AF2E-42F8558C6B63", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "309585C5-9703-4FF1-9A3E-984D5BD1FAC3" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC975C40-BE7C-4589-AF2E-42F8558C6B63", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "110EC73E-1208-4A83-9D43-008899898616" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DC975C40-BE7C-4589-AF2E-42F8558C6B63", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "9F9850FD-A134-4ACA-9086-4B0875CBD1C0" );
            // Attrib for BlockType: Persisted Dataset List:Max Preview Size (MB)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Max Preview Size (MB)", "MaxPreviewSizeMB", "Max Preview Size (MB)", @"If the JSON data is large, it could cause the browser to timeout.", 2, @"1", "CFA5BA3D-E6B9-4602-A1B2-CF54A2C75400" );
            // Attrib for BlockType: Persisted Dataset List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "C6F5517D-9902-4A72-87D6-513604DD1819" );
            // Attrib for BlockType: Step Bulk Entry:Step Program and Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6535FA22-9630-49A3-B8FF-A672CD91B8EE", "F8E85355-2780-4772-9B21-30B84741E6D1", "Step Program and Status", "StepProgramStepStatus", "Step Program and Status", @"The step program and step status to use to add a new step. Leave this empty to allow the user to choose.", 2, @"", "F6696635-126C-4F00-97B6-81CC8CA156A6" );
            // Attrib for BlockType: Step Bulk Entry:Step Program and Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6535FA22-9630-49A3-B8FF-A672CD91B8EE", "B00149C7-08D6-448C-AF21-948BF453DF7E", "Step Program and Type", "StepProgramStepType", "Step Program and Type", @"The step program and step type to use to add a new step. Leave this empty to allow the user to choose.", 1, @"", "B7763BD0-1A69-4B8A-9847-8F6B9A8C7A6B" );
            RockMigrationHelper.UpdateFieldType("Persisted Dataset","","Rock","Rock.Field.Types.PersistedDatasetFieldType","2A7817FD-4C38-40AE-A0A9-73976DA4E3A3");

        }

        private void CodeGenMigrationsDown()
        {
                        // Attrib for BlockType: Step Bulk Entry:Step Program and Type
            RockMigrationHelper.DeleteAttribute("B7763BD0-1A69-4B8A-9847-8F6B9A8C7A6B");
            // Attrib for BlockType: Step Bulk Entry:Step Program and Status
            RockMigrationHelper.DeleteAttribute("F6696635-126C-4F00-97B6-81CC8CA156A6");
            // Attrib for BlockType: Persisted Dataset List:Detail Page
            RockMigrationHelper.DeleteAttribute("C6F5517D-9902-4A72-87D6-513604DD1819");
            // Attrib for BlockType: Persisted Dataset List:Max Preview Size (MB)
            RockMigrationHelper.DeleteAttribute("CFA5BA3D-E6B9-4602-A1B2-CF54A2C75400");
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("9F9850FD-A134-4ACA-9086-4B0875CBD1C0");
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("110EC73E-1208-4A83-9D43-008899898616");
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("309585C5-9703-4FF1-9A3E-984D5BD1FAC3");
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("114E6004-B58D-4450-A319-C49DF1816BA9");
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("B86D5A17-1859-44AF-B8F0-98B0551BBF59");
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("8FBD0A26-0A11-4853-9325-D194C0A1375F");
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("D22B8615-FE9B-4F27-9A23-0A4C84807E93");
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute("8032CC03-C135-4DE3-B79B-F3776A98E7AB");
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute("FD67B645-A363-4D48-8743-C2BFAC5BF634");
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute("147FE149-C9F0-4E7B-B727-D0268010A25B");
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute("363353CF-1B62-44BC-838B-9F2EC55D1D78");
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("3D8BBF0E-7C98-4858-BA2B-7FB24541FFFF");
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("F6D43EA8-53A0-4ED4-A8F4-AE335D4EF135");
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute("540DFAE6-C3FD-46EF-9D77-FB727EA8ABEF");
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute("06C9C7AB-D4B1-4043-A299-492CDF3E24A6");
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("D5AA29DE-C9E3-4734-9C20-B5CFC00E79E6");
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("5F0D5180-5E44-4AEE-8852-023CC982EDA6");
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("561462FC-273A-481E-B5C1-F27822267363");
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("5E90BA6A-5B4F-4776-96A6-E1F7D52730CE");
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("DFD76261-884F-4E53-AE2B-C86C3B098BC6");
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("5D15FACA-D5EB-4485-A4C6-DC56414CD9B0");
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("EE072625-AF72-45A8-9A64-DC333505AE60");
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("95AA7A82-F987-429A-BC22-C57CC06772FE");
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("9597075B-1727-4F88-8B84-C09245ADEA73");
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("B04EC327-461C-43B8-A2A0-1ADC235E719B");
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("0D2F525D-CBA2-4F90-B0AF-2823D395F352");
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("E2DF8CEF-CA55-4C0A-A061-DA5287D8A309");
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("6FABBF6E-F974-4376-82DD-B58003187F3E");
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute("CDC1EE41-A875-40A6-A41C-7F2951CB24B0");
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("6D6B5870-9688-40E5-8E61-27225EEEDC91");
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute("A06CB4FF-7167-4513-BE51-C87A61F1A32D");
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("EA642CDE-065C-4948-8A95-3E0729DDC48B");
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute("EECDB7D4-3150-482B-9BFA-6DC906DF3E01");
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.DeleteAttribute("05935D2E-2C85-4F34-9A9B-3FABC8DCF48A");
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.DeleteAttribute("69EA900E-311C-466D-B81A-C5796810DB49");
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.DeleteAttribute("8BA82F56-1606-4415-A531-76F85E904369");
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.DeleteAttribute("FFD1F06A-CE6E-438A-B56D-8AD5F4F00487");
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("844E5167-13EF-4826-9CBD-C4837AA659BC");
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.DeleteAttribute("1746D9EE-9877-4CFF-84B4-185779A5200D");
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute("635686C9-9C9F-4745-980A-3D5EB45050EA");
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute("099C4B0A-3B19-45BF-BE52-F332F97291D3");
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("3B02E02F-E90D-423C-BC55-208D7E9EB656");
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute("DA54EC14-0FE3-47CC-AB5C-5A57FC1114A0");
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.DeleteAttribute("C54D2964-3BD5-4305-9391-ADBA39166907");
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("C1D452F0-D99B-425C-9ABA-C1FF52FF53EF");
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute("350594D6-19CB-4BAA-9831-1046E279494B");
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute("093FEE0E-78F1-4D48-85BD-6B38A4628E3A");
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute("C8A1B867-2F1D-4B10-ACA0-BE3E0A07D77A");
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute("E32644C3-681D-4557-88C1-ACC3D2A27D52");
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute("3B9478FD-DCB8-4E06-9932-BE5C72F7500B");
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute("52DC839F-741A-46E5-AFF1-6DD00AB83C67");
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("EB9F2F93-2C19-49CB-8A59-F74643F2685A");
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute("C2192E49-AAA7-4B98-BF6B-2B09F217E9FF");
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute("89BA2D96-9DDD-4A4E-8D43-B67717FB971F");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute("669FC130-C620-4DD5-9EB0-B04568F02EA2");
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("83B1311D-C3AB-4B2D-8832-623591C261C7");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("5F042523-9DD6-4C5D-A087-EA9728A6108F");
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("5E1A3CD6-4D3B-4653-ABCB-12090809F9C6");
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("F9C73B8C-A2EE-4A3D-98CB-9D405620F41C");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute("39B1BA8A-4EDB-4E76-BC3F-8F7ED98A2949");
            RockMigrationHelper.DeleteBlockType("50ADE904-BB5C-40F9-A97D-ED8FF530B5A6"); // Persisted Dataset List
            RockMigrationHelper.DeleteBlockType("ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F"); // Persisted Dataset Detail
            RockMigrationHelper.DeleteBlockType("DC975C40-BE7C-4589-AF2E-42F8558C6B63"); // Workflow Entry
            RockMigrationHelper.DeleteBlockType("EAC6881E-1C5A-4632-8ECF-2E596F3F5AD4"); // Register
            RockMigrationHelper.DeleteBlockType("8664A6FE-0366-4F47-B33A-44523F1EC43F"); // Profile Details
            RockMigrationHelper.DeleteBlockType("F9333B74-B537-439E-9AC4-C33ECE5AB341"); // Login
            RockMigrationHelper.DeleteBlockType("EE189BF0-61A4-4E0A-8B6A-880D04996CC8"); // Lava Item List
            RockMigrationHelper.DeleteBlockType("C3C6C9FF-288F-4499-924C-3451C7048BDD"); // Image
            RockMigrationHelper.DeleteBlockType("39B45710-616F-4A35-9941-5560992ABBCB"); // Dynamic Content
            RockMigrationHelper.DeleteBlockType("56D8F7C7-9D5E-4A02-9216-05B73BCE42BF"); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType("1DB613CF-5732-4515-81B5-46CDC42109E6"); // Content Channel Item List
            RockMigrationHelper.DeleteBlockType("9AFBB966-3F41-4081-895E-A0C58210BB75"); // Content
        }

        /// <summary>
        /// SC: Pushpay Plugin v6.0/Non-Cash Giving Project.
        /// </summary>
        private void CreateNonCashAssetTypes()
        {
            RockMigrationHelper.AddDefinedType(
                category: "Financial",
                name: "Non-Cash Asset Types",
                description: "Asset types that describe various kinds of Non-Cash transactions.",
                guid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Property",
                description: "Non-Cash Asset Type: Property.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_PROPERTY );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Stocks And Bonds",
                description: "Non-Cash Asset Type: Stocks And Bonds.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_STOCKSANDBONDS );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Vehicles",
                description: "Non-Cash Asset Type: Vehicles.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_VEHICLES );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Other",
                description: "Non-Cash Asset Type: Other.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_OTHER );
        }

        /// <summary>
        /// JE: Update Friendly Name of the Lava Run Workflow Action
        /// </summary>
        private void UpdateWorkflowActionRunLava()
        {
            Sql( @"
                UPDATE [EntityType] 
                SET [FriendlyName] = 'Lava Run' 
                WHERE [Name] = 'Rock.Workflow.Action.RunLava'" );
        }
    }
}
