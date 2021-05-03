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
    public partial class Rollup_08061 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            StepsPagesRename();
            UpdateOrderOfStepsPages();
            FixMobileBreadCrumbs();
            MobileBlockRename();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Code generated migrations for pages/block/attributes
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Text To Give Settings","Displays a person's Text To Give settings for editing.","~/Blocks/Finance/TextToGiveSettings.ascx","Finance","9069F894-FDA5-4546-93EB-CEC448B142AA");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3AE737D5-B022-4FFF-AB1D-BFC3E4619891", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "1333B9B5-171A-4A2D-9F1C-C4D731AF3368" );
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3AE737D5-B022-4FFF-AB1D-BFC3E4619891", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "1F348F1F-7D3D-46F5-BD62-12F25F2F91AE" );
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3AE737D5-B022-4FFF-AB1D-BFC3E4619891", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "1ECCB353-48DD-4CEC-8E49-3243331AB69C" );
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3AE737D5-B022-4FFF-AB1D-BFC3E4619891", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "B776BD06-61AB-4196-98F0-38A49C882B1C" );
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3AE737D5-B022-4FFF-AB1D-BFC3E4619891", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "B6A61034-B3F7-4946-A0E7-3A46B5290AE2" );
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5E2CF95-A01F-4963-81BA-68C19B968792", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "5837F6FD-74DC-400A-8CE2-047CA6C15FBC" );
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5E2CF95-A01F-4963-81BA-68C19B968792", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "7B9195A4-19A4-4FFD-9E78-D9E921B4AE28" );
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5E2CF95-A01F-4963-81BA-68C19B968792", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "2AE61C65-7D35-447B-ACE4-094868498F8A" );
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5E2CF95-A01F-4963-81BA-68C19B968792", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "8C7760F0-03DF-4231-864E-1B0C7E7C65B3" );
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34965833-83DC-4D59-8811-95EC961AF331", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "09C99635-F4BC-414D-9B75-0E79862EABE8" );
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34965833-83DC-4D59-8811-95EC961AF331", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "44BE838E-4768-470F-A9DA-79D8B95B2560" );
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34965833-83DC-4D59-8811-95EC961AF331", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Content", "InitialContent", "Initial Content", @"If the initial content should be static or dynamic.", 2, @"Static", "3E60E040-4416-4796-AC4F-0E607B56A982" );
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0D35665B-509C-4C6E-8D7A-D5394A3375DD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Image Url", "ImageUrl", "Image Url", @"The URL to use for displaying the image. <span class='tip tip-lava'></span>", 0, @"", "3A128F2D-8D79-435F-B00E-B017F962092D" );
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0D35665B-509C-4C6E-8D7A-D5394A3375DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "B2FDA73A-6A27-40C0-AC2C-1D1DB6C41427" );
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0D35665B-509C-4C6E-8D7A-D5394A3375DD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "3E7AD4A9-8A2B-4A9E-BED1-5743704E0657" );
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45AD0438-F601-49B1-B3D0-6A17C93787F5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "A2CBC1C0-66DF-4E4B-9A50-87DD66DAD554" );
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45AD0438-F601-49B1-B3D0-6A17C93787F5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "0789DA94-439C-446E-9273-189977B5CB3A" );
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45AD0438-F601-49B1-B3D0-6A17C93787F5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
  {
    ""Id"": 1,
    ""Title"": ""First Item""
  },
  {
    ""Id"": 2,
    ""Title"": ""Second Item""
  }
]", "2E708EFB-1E23-4FCD-A879-6E0D329A81C1" );
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "45AD0438-F601-49B1-B3D0-6A17C93787F5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Title}"" />
</StackLayout>", "04E630D5-EE48-4696-B0B4-62F19979BEF4" );
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A2F5FC7-558A-4DD6-BF20-78C1408615E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "067A48DC-FC28-42D5-B05F-3B09696E34F1" );
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A2F5FC7-558A-4DD6-BF20-78C1408615E2", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "5F8322E2-9D66-411B-A032-1123A11E7205" );
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "22F9D00C-01C8-4196-BE23-63D1E8BE5218" );
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "91FABF5A-5F07-41E6-8E4A-EE91A9C7BEBE" );
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "99274704-D6EC-44F7-9F4F-C86EACF9E032" );
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "EC70CF99-ED08-417F-9BDA-97899235386A" );
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "11CFE220-05CE-4845-9B70-D1C6AB44CCC0" );
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "294D67DF-61EF-4C39-96AA-B6FA51B6C03C" );
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "DD31431B-5F2B-48E0-A5DC-0373477318AA" );
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "43058D8F-2473-46B9-8E1B-BD4DE58CF65F" );
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "7E42C0B5-9E9B-410F-9ACD-3E9A72CE3648" );
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "DDFE5586-5E6E-4E33-BAD7-33F9ADF20A0B" );
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "1E513605-DE6C-4086-9150-2709478C8D9B" );
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F867098-ECDA-4EB5-820F-858495ECEBE1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "51EE0793-04BF-4A57-89B5-A49F97298F67" );
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "28FAFF27-CA05-40CB-80FE-02BE42654D4B" );
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "007818EC-67EC-4049-84F3-099CBFB6FDAE" );
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "E0797A43-7F45-4405-859F-109EB66693C7" );
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "B6FAD835-1699-4B21-8569-668DCB1199B3" );
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "19D379DA-812E-41A9-B718-ADFAEDED88A1" );
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "20D7D713-DD92-4CE4-9D49-273697C5BBEA" );
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "C9BCF43C-ED34-4455-851E-2CA2EC8D74B0" );
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "E9094931-3BBF-4E0E-8BC0-7C56CD61B640" );
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "04430AE2-2373-4F15-8E8D-C63ADE64E2CD" );
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A6ADF7C-4810-4B31-A5D6-8603742C4B8F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "68A5E0F4-76EB-46AC-8C3E-2E2575B0511D" );
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D7A093-E22F-45B8-8ADD-11D2B463A0FD", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "7FF99B71-BC22-4EE6-B890-2A46135B07FA" );
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D7A093-E22F-45B8-8ADD-11D2B463A0FD", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "DBE9E667-E0A6-44E4-9380-D65AC87816B2" );
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D7A093-E22F-45B8-8ADD-11D2B463A0FD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "430A6AE4-CF8F-4617-B6E8-3DD952A50237" );
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D7A093-E22F-45B8-8ADD-11D2B463A0FD", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "9567660B-DFB4-49AC-9520-3122B3FA20FF" );
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6D7A093-E22F-45B8-8ADD-11D2B463A0FD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "B02B8D30-7555-440C-8ECB-18DD7BE21B03" );
            // Attrib for BlockType: Text To Give Settings:Add Saved Account Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9069F894-FDA5-4546-93EB-CEC448B142AA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Saved Account Page", "AddSavedAccountPage", "Add Saved Account Page", @"Page that will be navigated to for the purpose of creating a new saved account.", 1, @"", "D61ED297-F1F3-4876-8352-F3D5E652C335" );
            // Attrib for BlockType: Text To Give Settings:Parent Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9069F894-FDA5-4546-93EB-CEC448B142AA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Parent Page", "ParentPage", "Parent Page", @"Page that will be navigated to when finished with this block.", 0, @"", "15B5B63E-F970-4FEB-8338-E4F63409E07B" );
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "B5CA6EE8-E60F-469E-ABD0-829CEA9DD240" );
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "D853E966-E953-4B8E-9BCF-3C315B33E547" );
            // Attrib for BlockType: Person Transaction Links:Text to Give Settings Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2BB707AC-F29A-44DF-A103-7454077509B4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Text to Give Settings Page", "TextToGiveSettingsPage", "Text to Give Settings Page", @"", 0, @"", "62CC40DA-C321-4406-9804-0C44B038E1C3" );
            // Attrib for BlockType: Transaction List:Source Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source Types", "SourceTypes", "Source Types", @"Optional list of financial source types to limit the list to (if none are selected all types will be included).", 11, @"", "65BCF5C4-4025-4FB5-B396-0BE6D92D0627" );
            // Attrib for BlockType: Transaction List:Show Future Transactions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Future Transactions", "ShowFutureTransactions", "Show Future Transactions", @"Should future transactions (transactions scheduled to be charged) be shown in this list?", 10, @"False", "7C455827-00ED-4800-BDBC-41FE04A58C2D" );

        }

        /// <summary>
        /// Code generated migrations for pages/block/attributes
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Transaction List:Show Future Transactions
            RockMigrationHelper.DeleteAttribute("7C455827-00ED-4800-BDBC-41FE04A58C2D");
            // Attrib for BlockType: Transaction List:Source Types
            RockMigrationHelper.DeleteAttribute("65BCF5C4-4025-4FB5-B396-0BE6D92D0627");
            // Attrib for BlockType: Person Transaction Links:Text to Give Settings Page
            RockMigrationHelper.DeleteAttribute("62CC40DA-C321-4406-9804-0C44B038E1C3");
            // Attrib for BlockType: Content Channel Item List:Check Item Security
            RockMigrationHelper.DeleteAttribute("D853E966-E953-4B8E-9BCF-3C315B33E547");
            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
            RockMigrationHelper.DeleteAttribute("B5CA6EE8-E60F-469E-ABD0-829CEA9DD240");
            // Attrib for BlockType: Text To Give Settings:Parent Page
            RockMigrationHelper.DeleteAttribute("15B5B63E-F970-4FEB-8338-E4F63409E07B");
            // Attrib for BlockType: Text To Give Settings:Add Saved Account Page
            RockMigrationHelper.DeleteAttribute("D61ED297-F1F3-4876-8352-F3D5E652C335");
            // Attrib for BlockType: Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("B02B8D30-7555-440C-8ECB-18DD7BE21B03");
            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("9567660B-DFB4-49AC-9520-3122B3FA20FF");
            // Attrib for BlockType: Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("430A6AE4-CF8F-4617-B6E8-3DD952A50237");
            // Attrib for BlockType: Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("DBE9E667-E0A6-44E4-9380-D65AC87816B2");
            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("7FF99B71-BC22-4EE6-B890-2A46135B07FA");
            // Attrib for BlockType: Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("68A5E0F4-76EB-46AC-8C3E-2E2575B0511D");
            // Attrib for BlockType: Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("04430AE2-2373-4F15-8E8D-C63ADE64E2CD");
            // Attrib for BlockType: Register:Email Required
            RockMigrationHelper.DeleteAttribute("E9094931-3BBF-4E0E-8BC0-7C56CD61B640");
            // Attrib for BlockType: Register:Email Show
            RockMigrationHelper.DeleteAttribute("C9BCF43C-ED34-4455-851E-2CA2EC8D74B0");
            // Attrib for BlockType: Register:Campus Required
            RockMigrationHelper.DeleteAttribute("20D7D713-DD92-4CE4-9D49-273697C5BBEA");
            // Attrib for BlockType: Register:Campus Show
            RockMigrationHelper.DeleteAttribute("19D379DA-812E-41A9-B718-ADFAEDED88A1");
            // Attrib for BlockType: Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("B6FAD835-1699-4B21-8569-668DCB1199B3");
            // Attrib for BlockType: Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("E0797A43-7F45-4405-859F-109EB66693C7");
            // Attrib for BlockType: Register:Record Status
            RockMigrationHelper.DeleteAttribute("007818EC-67EC-4049-84F3-099CBFB6FDAE");
            // Attrib for BlockType: Register:Connection Status
            RockMigrationHelper.DeleteAttribute("28FAFF27-CA05-40CB-80FE-02BE42654D4B");
            // Attrib for BlockType: Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("51EE0793-04BF-4A57-89B5-A49F97298F67");
            // Attrib for BlockType: Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("1E513605-DE6C-4086-9150-2709478C8D9B");
            // Attrib for BlockType: Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("DDFE5586-5E6E-4E33-BAD7-33F9ADF20A0B");
            // Attrib for BlockType: Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("7E42C0B5-9E9B-410F-9ACD-3E9A72CE3648");
            // Attrib for BlockType: Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("43058D8F-2473-46B9-8E1B-BD4DE58CF65F");
            // Attrib for BlockType: Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("DD31431B-5F2B-48E0-A5DC-0373477318AA");
            // Attrib for BlockType: Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("294D67DF-61EF-4C39-96AA-B6FA51B6C03C");
            // Attrib for BlockType: Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("11CFE220-05CE-4845-9B70-D1C6AB44CCC0");
            // Attrib for BlockType: Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("EC70CF99-ED08-417F-9BDA-97899235386A");
            // Attrib for BlockType: Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("99274704-D6EC-44F7-9F4F-C86EACF9E032");
            // Attrib for BlockType: Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("91FABF5A-5F07-41E6-8E4A-EE91A9C7BEBE");
            // Attrib for BlockType: Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("22F9D00C-01C8-4196-BE23-63D1E8BE5218");
            // Attrib for BlockType: Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("5F8322E2-9D66-411B-A032-1123A11E7205");
            // Attrib for BlockType: Login:Registration Page
            RockMigrationHelper.DeleteAttribute("067A48DC-FC28-42D5-B05F-3B09696E34F1");
            // Attrib for BlockType: Lava Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("04E630D5-EE48-4696-B0B4-62F19979BEF4");
            // Attrib for BlockType: Lava Item List:List Template
            RockMigrationHelper.DeleteAttribute("2E708EFB-1E23-4FCD-A879-6E0D329A81C1");
            // Attrib for BlockType: Lava Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("0789DA94-439C-446E-9273-189977B5CB3A");
            // Attrib for BlockType: Lava Item List:Page Size
            RockMigrationHelper.DeleteAttribute("A2CBC1C0-66DF-4E4B-9A50-87DD66DAD554");
            // Attrib for BlockType: Image:Cache Duration
            RockMigrationHelper.DeleteAttribute("3E7AD4A9-8A2B-4A9E-BED1-5743704E0657");
            // Attrib for BlockType: Image:Dynamic Content
            RockMigrationHelper.DeleteAttribute("B2FDA73A-6A27-40C0-AC2C-1D1DB6C41427");
            // Attrib for BlockType: Image:Image Url
            RockMigrationHelper.DeleteAttribute("3A128F2D-8D79-435F-B00E-B017F962092D");
            // Attrib for BlockType: Dynamic Content:Initial Content
            RockMigrationHelper.DeleteAttribute("3E60E040-4416-4796-AC4F-0E607B56A982");
            // Attrib for BlockType: Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("44BE838E-4768-470F-A9DA-79D8B95B2560");
            // Attrib for BlockType: Dynamic Content:Content
            RockMigrationHelper.DeleteAttribute("09C99635-F4BC-414D-9B75-0E79862EABE8");
            // Attrib for BlockType: Content Channel Item View:Log Interactions
            RockMigrationHelper.DeleteAttribute("8C7760F0-03DF-4231-864E-1B0C7E7C65B3");
            // Attrib for BlockType: Content Channel Item View:Content Channel
            RockMigrationHelper.DeleteAttribute("2AE61C65-7D35-447B-ACE4-094868498F8A");
            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("7B9195A4-19A4-4FFD-9E78-D9E921B4AE28");
            // Attrib for BlockType: Content Channel Item View:Content Template
            RockMigrationHelper.DeleteAttribute("5837F6FD-74DC-400A-8CE2-047CA6C15FBC");
            // Attrib for BlockType: Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("B6A61034-B3F7-4946-A0E7-3A46B5290AE2");
            // Attrib for BlockType: Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("B776BD06-61AB-4196-98F0-38A49C882B1C");
            // Attrib for BlockType: Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("1ECCB353-48DD-4CEC-8E49-3243331AB69C");
            // Attrib for BlockType: Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("1F348F1F-7D3D-46F5-BD62-12F25F2F91AE");
            // Attrib for BlockType: Content:Content
            RockMigrationHelper.DeleteAttribute("1333B9B5-171A-4A2D-9F1C-C4D731AF3368");
            // Attrib for BlockType: Lava Tester:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("5CF9CCE5-7F43-41BD-9F19-10974A4297FE");
            RockMigrationHelper.DeleteBlockType("9069F894-FDA5-4546-93EB-CEC448B142AA"); // Text To Give Settings
            RockMigrationHelper.DeleteBlockType("A6D7A093-E22F-45B8-8ADD-11D2B463A0FD"); // Workflow Entry
            RockMigrationHelper.DeleteBlockType("7A6ADF7C-4810-4B31-A5D6-8603742C4B8F"); // Register
            RockMigrationHelper.DeleteBlockType("1F867098-ECDA-4EB5-820F-858495ECEBE1"); // Profile Details
            RockMigrationHelper.DeleteBlockType("4A2F5FC7-558A-4DD6-BF20-78C1408615E2"); // Login
            RockMigrationHelper.DeleteBlockType("45AD0438-F601-49B1-B3D0-6A17C93787F5"); // Lava Item List
            RockMigrationHelper.DeleteBlockType("0D35665B-509C-4C6E-8D7A-D5394A3375DD"); // Image
            RockMigrationHelper.DeleteBlockType("34965833-83DC-4D59-8811-95EC961AF331"); // Dynamic Content
            RockMigrationHelper.DeleteBlockType("D5E2CF95-A01F-4963-81BA-68C19B968792"); // Content Channel Item View
            RockMigrationHelper.DeleteBlockType("3AE737D5-B022-4FFF-AB1D-BFC3E4619891"); // Content
            RockMigrationHelper.DeleteBlockType("D8439BF0-1022-4989-9028-F1E63929021C"); // Mobile Workflow Entry
            RockMigrationHelper.DeleteBlockType("7C2ACA30-849E-4868-86C0-E9D76C950EEF"); // Mobile Register
            RockMigrationHelper.DeleteBlockType("4B976C73-B98B-4C0F-8B0C-F62F094DF9EB"); // Mobile Profile Details
            RockMigrationHelper.DeleteBlockType("63CBF490-4D83-471E-90B7-AC489AB0DC03"); // Mobile Login
            RockMigrationHelper.DeleteBlockType("622F0737-947D-45B1-8DD1-CDB380F2AE35"); // Mobile Image
            RockMigrationHelper.DeleteBlockType("50FE6DD1-B09B-4FE7-B5FB-DF2552BDF100"); // Mobile Dynamic Content
            RockMigrationHelper.DeleteBlockType("17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE"); // Mobile Content
            RockMigrationHelper.DeleteBlockType("7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4"); // Content Channel Item List
        }

        /// <summary>
        /// JE: Steps Pages Rename
        /// Rename pages for the Steps features
        /// </summary>
        private void StepsPagesRename()
        {
            Sql( @"
                UPDATE[Page]
                SET
                 [BrowserTitle] = 'Step Program'
                 ,[InternalName] = 'Step Program'
                 ,[PageTitle] = 'Step Program'
                WHERE[Guid] = '6E46BC35-1FCB-4619-84F0-BB6926D2DDD5'

                UPDATE[Page]
                SET
                 [BrowserTitle] = 'Step Entry'
                 ,[InternalName] = 'Step Entry'
                 ,[PageTitle] = 'Step Entry'
                WHERE[Guid] = '2109228C-D828-4B58-9310-8D93D10B846E'

                UPDATE[Page]
                SET
                 [BrowserTitle] = 'Step Entry'
                 ,[InternalName] = 'Step Entry'
                 ,[PageTitle] = 'Step Entry'
                WHERE[Guid] = '7A04966A-8E4E-49EA-A03C-7DD4B52A7B28'" );
        }

        /// <summary>
        /// SK: Update Order of Steps Page
        /// </summary>
        private void UpdateOrderOfStepsPages()
        {
            Sql( @"
                DECLARE @PersonPagesId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303' )
                DECLARE @Order int = ISNULL( ( SELECT TOP 1 [Order] FROM [Page] WHERE [Guid] = '1C737278-4CBA-404B-B6B3-E3F0E05AB5FE' ), 0)
                IF @Order IS NOT NULL
                BEGIN
                    UPDATE [Page] SET [Order] = [Order] + 1 WHERE [ParentPageId] = @PersonPagesId AND [Order] > @Order
                    UPDATE [Page] SET [Order] = @Order + 1 WHERE [Guid] = 'CB9ABA3B-6962-4A42-BDA1-EA71B7309232' 
                END" );
        }

        /// <summary>
        /// SK: Fix Breadcrumb Behavior in Mobile App Pages 
        /// </summary>
        private void FixMobileBreadCrumbs()
        {
            Sql( @"
                UPDATE [Page]
                SET [BreadCrumbDisplayName]=0
                WHERE [Guid] IN ('A4B0BCBB-721D-439C-8566-24F604DD4A1C','37E21200-DF91-4426-89CC-7D067237A037','5583A55D-7398-48E9-971F-6A1EF8158943')" );
        }

        /// <summary>
        /// JE: Mobile Block Rename
        /// </summary>
        private void MobileBlockRename()
        {
            Sql( @"
                UPDATE [EntityType] SET
                    [Name] = REPLACE([Name], 'Mobile.Mobile', 'Mobile.'),
                    [AssemblyName] = REPLACE([AssemblyName], 'Mobile.Mobile', 'Mobile.'),
                    [FriendlyName] = REPLACE([FriendlyName], 'Mobile ', '')
                    WHERE [Name] LIKE 'Rock.Blocks.Types.Mobile.%'" );
        }
    }
}
