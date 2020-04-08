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
    public partial class Rollup_0724 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            AddNewAbsenceGroupMemberNotificationEmailTemplate();
            FixGooglestaticmapShortcode();
            FixGoogleMapTypeo();
            FixAccordianTypeo();
            FixWordCloudTypeo();
            FixParallaxTypeo();
            ConnectionStatusChangesUp();
            HideBreadcrumbsForStepsPages();
            UpdateMotivatorAssessmentColors();
            FixChartJS();
            AddAssessmentAdminPagesUp();
            ClearMigrationModelsForPreviousVersion();
            ConnectionActivityTypesToActive();
            RenameAttributeValuesBlockAttibutes();
            SetPageRoutesToSystemRoutes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddAssessmentAdminPagesDown();
            ConnectionStatusChangesDown();
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Up migrations created by CodeGen_PagesBlocksAttributesMigration.sql 
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Assessment Type Detail","Displays the details of the given Assessment Type for editing.","~/Blocks/Assessments/AssessmentTypeDetail.ascx","Assessments","F24E1102-FA8D-4FA5-8DF1-832EF6621A62");
            RockMigrationHelper.UpdateBlockType("Assessment Type List","Shows a list of all Assessment Types.","~/Blocks/Assessments/AssessmentTypeList.ascx","Assessments","D7270081-3A75-4FFE-97CD-330AEAB621B4");
            RockMigrationHelper.UpdateBlockType("Connection Status Changes","Shows changes of Connection Status for people within a specific period.","~/Blocks/Crm/ConnectionStatusChangeReport.ascx","Connection","401079C6-01E3-4F72-9FFD-7ABC8741FBBE");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "D64C1B9D-6CED-429E-9212-E7AE77646E54" );
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "7428F31A-EE60-408C-BE6D-9E18BCB26663" );
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "10C9C7F0-92E8-49A5-AE22-63345E4712BA" );
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "FA421A8C-EF58-4695-8152-EFE94A6EB81B" );
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "C8C01BE7-552D-4E6D-82B3-28D1315DBACD" );
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "054CEE96-C0A2-45AD-A43F-0EF13D1E6FE3" );
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "FADC568E-1C7A-4589-B153-B26F931D5409" );
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 8, @"", "4C033E8A-491E-43BE-AA33-67A4DAEE07A3" );
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding [Content]}"" />
</StackLayout>", "7B48C696-9856-4BEC-B61B-AB7EBDD24135" );
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A9B4A68-E9F8-4CAF-9E9C-7C2115E941C4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "F64D1CDB-B878-4B74-8A78-C67DF44265D3" );
            // Attrib for BlockType: Mobile Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "FC03F001-ECAD-42A2-8590-24CC75025BD7" );
            // Attrib for BlockType: Mobile Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "F572205C-59A9-46B8-971D-1749ADF1907A" );
            // Attrib for BlockType: Mobile Content:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "6F708113-0998-4AC1-BE88-FE6BBE63D0F5" );
            // Attrib for BlockType: Mobile Content:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "0F31D639-C5B9-4346-BAB8-EA391BF628C9" );
            // Attrib for BlockType: Mobile Content:Lava Render Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "17F6F3C5-C7DD-4681-B548-6D7B7A8B6FAE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "31524A3A-BD70-4012-8D4E-0CBC4E39D412" );
            // Attrib for BlockType: Mobile Dynamic Content:Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50FE6DD1-B09B-4FE7-B5FB-DF2552BDF100", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "8E637749-DFB1-46CC-984B-1D5E5F176AC4" );
            // Attrib for BlockType: Mobile Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50FE6DD1-B09B-4FE7-B5FB-DF2552BDF100", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 1, @"", "39276522-4072-44C9-9009-9FB4FC065612" );
            // Attrib for BlockType: Mobile Dynamic Content:Initial Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50FE6DD1-B09B-4FE7-B5FB-DF2552BDF100", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Initial Content", "InitialContent", "Initial Content", @"If the initial content should be static or dynamic.", 2, @"Static", "7EA66B54-ABAD-4A56-B202-F86ED43558F4" );
            // Attrib for BlockType: Mobile Image:Image Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "622F0737-947D-45B1-8DD1-CDB380F2AE35", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Image Url", "ImageUrl", "Image Url", @"The URL to use for displaying the image. <span class='tip tip-lava'></span>", 0, @"", "76BD1AF1-2882-42C6-A851-F4C0478A2695" );
            // Attrib for BlockType: Mobile Image:Dynamic Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "622F0737-947D-45B1-8DD1-CDB380F2AE35", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "4B21CA12-CCFB-4F32-98C7-767A5F766320" );
            // Attrib for BlockType: Mobile Image:Cache Duration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "622F0737-947D-45B1-8DD1-CDB380F2AE35", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "C994AFE9-D863-468B-8BFB-826E2D58250D" );
            // Attrib for BlockType: Mobile Login:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63CBF490-4D83-471E-90B7-AC489AB0DC03", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "D75EF452-D6FF-4188-B6BC-2D57D79FFF43" );
            // Attrib for BlockType: Mobile Login:Forgot Password Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63CBF490-4D83-471E-90B7-AC489AB0DC03", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "C1910E67-5312-40DA-8F95-83E1246DB4BA" );
            // Attrib for BlockType: Mobile Profile Details:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "4CC04E29-B654-41DF-BDF0-A3EFA859800D" );
            // Attrib for BlockType: Mobile Profile Details:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "0C0F0161-FC0A-48C8-A49D-B5480D0BC7B8" );
            // Attrib for BlockType: Mobile Profile Details:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "E753ACA8-4716-404A-9C85-D1400D8B3D67" );
            // Attrib for BlockType: Mobile Profile Details:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "9F7AB52F-99F3-4768-A9BC-B06CE4E224AE" );
            // Attrib for BlockType: Mobile Profile Details:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "475DF918-A00F-4153-8AB0-473993943807" );
            // Attrib for BlockType: Mobile Profile Details:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "D87BC79C-A25A-490E-A00E-489AFAA50F67" );
            // Attrib for BlockType: Mobile Profile Details:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "8A3EEC4F-2759-4267-BF51-8ECBE1986085" );
            // Attrib for BlockType: Mobile Profile Details:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "428229EA-633E-45D0-A351-A2DCE28BB419" );
            // Attrib for BlockType: Mobile Profile Details:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "40796DA3-F176-4E22-ADCE-4DDDC9EDBA28" );
            // Attrib for BlockType: Mobile Profile Details:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "FA67DAEE-898B-41C7-89BF-5CDDE928C331" );
            // Attrib for BlockType: Mobile Profile Details:Address Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "8448EBDE-3C90-45A6-8993-6E5F03D89E56" );
            // Attrib for BlockType: Mobile Profile Details:Address Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4B976C73-B98B-4C0F-8B0C-F62F094DF9EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "82265474-7F24-40B2-9741-5B15F62ADC88" );
            // Attrib for BlockType: Mobile Register:Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "3ACDB156-1C0D-4E6D-8E48-553FF1EEA4E4" );
            // Attrib for BlockType: Mobile Register:Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "24D70BA7-4F63-4FD1-9DB9-125C0C514EA7" );
            // Attrib for BlockType: Mobile Register:Birthdate Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "E39D7FE8-DB75-4523-A527-3A3861E2BC56" );
            // Attrib for BlockType: Mobile Register:BirthDate Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "C30546D9-870B-49A9-83B3-6B1B8E4D7805" );
            // Attrib for BlockType: Mobile Register:Campus Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "BBC5C2A2-36BC-4CC7-8AF5-E5DA7CCD66F4" );
            // Attrib for BlockType: Mobile Register:Campus Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "5FE12396-9207-4985-B3C4-BACACAB2175B" );
            // Attrib for BlockType: Mobile Register:Email Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "3C83839C-9AAA-4AC8-8621-52D6AF081FE0" );
            // Attrib for BlockType: Mobile Register:Email Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "CC19585B-4714-4D1D-B195-FB0D5BEBDDEB" );
            // Attrib for BlockType: Mobile Register:Mobile Phone Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "0110F13B-81FA-4EB9-8AAE-F7524FFE1078" );
            // Attrib for BlockType: Mobile Register:Mobile Phone Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7C2ACA30-849E-4868-86C0-E9D76C950EEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "7FE8EE7F-EC29-4ED4-9B0E-615C81D31186" );
            // Attrib for BlockType: Mobile Workflow Entry:Workflow Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8439BF0-1022-4989-9028-F1E63929021C", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "3F92DA27-699D-4E6F-ABF9-F009A7BAC8E3" );
            // Attrib for BlockType: Mobile Workflow Entry:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8439BF0-1022-4989-9028-F1E63929021C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "F4EDE738-001C-47F0-B26F-6981BCBC8CEC" );
            // Attrib for BlockType: Mobile Workflow Entry:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8439BF0-1022-4989-9028-F1E63929021C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "40C72CAF-05F3-4859-A170-4D186B9B771A" );
            // Attrib for BlockType: Mobile Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8439BF0-1022-4989-9028-F1E63929021C", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "051B1F23-175A-4F1D-8B4F-B594A7521E0D" );
            // Attrib for BlockType: Mobile Workflow Entry:Redirect To Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8439BF0-1022-4989-9028-F1E63929021C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "D5027E7D-50B6-4D31-AF03-3173779902CA" );
            // Attrib for BlockType: Assessment Type List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7270081-3A75-4FFE-97CD-330AEAB621B4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 1, @"", "5A8F3347-8315-4EF4-93B7-40E6462CCC85" );
            // Attrib for BlockType: Connection Status Changes:Person Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "401079C6-01E3-4F72-9FFD-7ABC8741FBBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "Person Detail Page", @"", 0, @"", "FC3699E7-6260-4A04-A268-C54C56E9D183" );
            // Attrib for BlockType: Site List:Block Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Icon CSS Class", "BlockIcon", "Block Icon CSS Class", @"The icon CSS class for the block.", 3, @"fa fa-desktop", "B1188B79-3BC4-4F11-8165-CA4E5858FADA" );
            // Attrib for BlockType: Site List:Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "Block Title", @"The title for the block.", 2, @"Site List", "8D88C763-B904-48F1-9FEE-31CC4D5086FD" );
            // Attrib for BlockType: Communication Entry Wizard:Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 11, @"False", "CC52F07D-D99A-4C8C-82C2-A2531FC90FEB" );
            // Attrib for BlockType: Family Pre Registration:Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Email", "ChildEmail", "Email", @"How should Email be displayed for children?  Be sure to seek legal guidance when collecting email addresses on minors.", 4, @"Hide", "BA8C1486-AB4F-443C-8E3A-5DD0AB11ACD8" );
            // Attrib for BlockType: Add Group:Detect Groups already at the Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Detect Groups already at the Address", "DetectGroupsAlreadyAtTheAddress", "Detect Groups already at the Address", @"If enabled, a prompt to select an existing group will be displayed if there are existing groups that have the same address as the new group.", 32, @"True", "0FC3E89E-E950-421A-8D6A-665AC1EBE712" );
            // Attrib for BlockType: Add Group:Max Groups at Address to Detect
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Groups at Address to Detect", "MaxGroupsAtAddressToDetect", "Max Groups at Address to Detect", @"", 33, @"10", "6D3B445B-1B4E-4715-84B5-DED54B83AFD3" );
            // Attrib for BlockType: Attribute Values:Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title", "BlockTitle", "Block Title", @"The text to display as the heading.", 3, @"", "6AA30A6D-D5E7-482E-A207-33026070DB42" );
            // Attrib for BlockType: Attribute Values:Block Icon
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Icon", "BlockIcon", "Block Icon", @"The css class name to use for the heading icon.", 4, @"", "E7704D69-2B48-490A-AC40-80D112E6EE0A" );
            // Attrib for BlockType: Step Entry:Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8D78BC55-6E67-40AB-B453-994D69503838", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 3, @"", "F0DC03C5-5C43-41B2-B7CB-33D0216CA3CB" );
            RockMigrationHelper.UpdateFieldType("Badges","","Rock","Rock.Field.Types.BadgesFieldType","602F273B-7EC2-42E6-9AA7-A36A268192A3");
            RockMigrationHelper.UpdateFieldType("Sequence","","Rock","Rock.Field.Types.SequenceFieldType","45598C08-8575-4B23-9869-4D993F8AC734");
        }

        /// <summary>
        /// Down migrations created by CodeGen_PagesBlocksAttributesMigration.sql 
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Step Entry:Workflow Entry Page
            RockMigrationHelper.DeleteAttribute("F0DC03C5-5C43-41B2-B7CB-33D0216CA3CB");
            // Attrib for BlockType: Attribute Values:Block Icon
            RockMigrationHelper.DeleteAttribute("E7704D69-2B48-490A-AC40-80D112E6EE0A");
            // Attrib for BlockType: Attribute Values:Block Title
            RockMigrationHelper.DeleteAttribute("6AA30A6D-D5E7-482E-A207-33026070DB42");
            // Attrib for BlockType: Add Group:Max Groups at Address to Detect
            RockMigrationHelper.DeleteAttribute("6D3B445B-1B4E-4715-84B5-DED54B83AFD3");
            // Attrib for BlockType: Add Group:Detect Groups already at the Address
            RockMigrationHelper.DeleteAttribute("0FC3E89E-E950-421A-8D6A-665AC1EBE712");
            // Attrib for BlockType: Family Pre Registration:Email
            RockMigrationHelper.DeleteAttribute("BA8C1486-AB4F-443C-8E3A-5DD0AB11ACD8");
            // Attrib for BlockType: Communication Entry Wizard:Show Duplicate Prevention Option
            RockMigrationHelper.DeleteAttribute("CC52F07D-D99A-4C8C-82C2-A2531FC90FEB");
            // Attrib for BlockType: Site List:Block Title
            RockMigrationHelper.DeleteAttribute("8D88C763-B904-48F1-9FEE-31CC4D5086FD");
            // Attrib for BlockType: Site List:Block Icon CSS Class
            RockMigrationHelper.DeleteAttribute("B1188B79-3BC4-4F11-8165-CA4E5858FADA");
            // Attrib for BlockType: Connection Status Changes:Person Detail Page
            RockMigrationHelper.DeleteAttribute("FC3699E7-6260-4A04-A268-C54C56E9D183");
            // Attrib for BlockType: Assessment Type List:Detail Page
            RockMigrationHelper.DeleteAttribute("5A8F3347-8315-4EF4-93B7-40E6462CCC85");
            // Attrib for BlockType: Mobile Workflow Entry:Redirect To Page
            RockMigrationHelper.DeleteAttribute("D5027E7D-50B6-4D31-AF03-3173779902CA");
            // Attrib for BlockType: Mobile Workflow Entry:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("051B1F23-175A-4F1D-8B4F-B594A7521E0D");
            // Attrib for BlockType: Mobile Workflow Entry:Completion Xaml
            RockMigrationHelper.DeleteAttribute("40C72CAF-05F3-4859-A170-4D186B9B771A");
            // Attrib for BlockType: Mobile Workflow Entry:Completion Action
            RockMigrationHelper.DeleteAttribute("F4EDE738-001C-47F0-B26F-6981BCBC8CEC");
            // Attrib for BlockType: Mobile Workflow Entry:Workflow Type
            RockMigrationHelper.DeleteAttribute("3F92DA27-699D-4E6F-ABF9-F009A7BAC8E3");
            // Attrib for BlockType: Mobile Register:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("7FE8EE7F-EC29-4ED4-9B0E-615C81D31186");
            // Attrib for BlockType: Mobile Register:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("0110F13B-81FA-4EB9-8AAE-F7524FFE1078");
            // Attrib for BlockType: Mobile Register:Email Required
            RockMigrationHelper.DeleteAttribute("CC19585B-4714-4D1D-B195-FB0D5BEBDDEB");
            // Attrib for BlockType: Mobile Register:Email Show
            RockMigrationHelper.DeleteAttribute("3C83839C-9AAA-4AC8-8621-52D6AF081FE0");
            // Attrib for BlockType: Mobile Register:Campus Required
            RockMigrationHelper.DeleteAttribute("5FE12396-9207-4985-B3C4-BACACAB2175B");
            // Attrib for BlockType: Mobile Register:Campus Show
            RockMigrationHelper.DeleteAttribute("BBC5C2A2-36BC-4CC7-8AF5-E5DA7CCD66F4");
            // Attrib for BlockType: Mobile Register:BirthDate Required
            RockMigrationHelper.DeleteAttribute("C30546D9-870B-49A9-83B3-6B1B8E4D7805");
            // Attrib for BlockType: Mobile Register:Birthdate Show
            RockMigrationHelper.DeleteAttribute("E39D7FE8-DB75-4523-A527-3A3861E2BC56");
            // Attrib for BlockType: Mobile Register:Record Status
            RockMigrationHelper.DeleteAttribute("24D70BA7-4F63-4FD1-9DB9-125C0C514EA7");
            // Attrib for BlockType: Mobile Register:Connection Status
            RockMigrationHelper.DeleteAttribute("3ACDB156-1C0D-4E6D-8E48-553FF1EEA4E4");
            // Attrib for BlockType: Mobile Profile Details:Address Required
            RockMigrationHelper.DeleteAttribute("82265474-7F24-40B2-9741-5B15F62ADC88");
            // Attrib for BlockType: Mobile Profile Details:Address Show
            RockMigrationHelper.DeleteAttribute("8448EBDE-3C90-45A6-8993-6E5F03D89E56");
            // Attrib for BlockType: Mobile Profile Details:Mobile Phone Required
            RockMigrationHelper.DeleteAttribute("FA67DAEE-898B-41C7-89BF-5CDDE928C331");
            // Attrib for BlockType: Mobile Profile Details:Mobile Phone Show
            RockMigrationHelper.DeleteAttribute("40796DA3-F176-4E22-ADCE-4DDDC9EDBA28");
            // Attrib for BlockType: Mobile Profile Details:Email Required
            RockMigrationHelper.DeleteAttribute("428229EA-633E-45D0-A351-A2DCE28BB419");
            // Attrib for BlockType: Mobile Profile Details:Email Show
            RockMigrationHelper.DeleteAttribute("8A3EEC4F-2759-4267-BF51-8ECBE1986085");
            // Attrib for BlockType: Mobile Profile Details:Campus Required
            RockMigrationHelper.DeleteAttribute("D87BC79C-A25A-490E-A00E-489AFAA50F67");
            // Attrib for BlockType: Mobile Profile Details:Campus Show
            RockMigrationHelper.DeleteAttribute("475DF918-A00F-4153-8AB0-473993943807");
            // Attrib for BlockType: Mobile Profile Details:BirthDate Required
            RockMigrationHelper.DeleteAttribute("9F7AB52F-99F3-4768-A9BC-B06CE4E224AE");
            // Attrib for BlockType: Mobile Profile Details:Birthdate Show
            RockMigrationHelper.DeleteAttribute("E753ACA8-4716-404A-9C85-D1400D8B3D67");
            // Attrib for BlockType: Mobile Profile Details:Record Status
            RockMigrationHelper.DeleteAttribute("0C0F0161-FC0A-48C8-A49D-B5480D0BC7B8");
            // Attrib for BlockType: Mobile Profile Details:Connection Status
            RockMigrationHelper.DeleteAttribute("4CC04E29-B654-41DF-BDF0-A3EFA859800D");
            // Attrib for BlockType: Mobile Login:Forgot Password Url
            RockMigrationHelper.DeleteAttribute("C1910E67-5312-40DA-8F95-83E1246DB4BA");
            // Attrib for BlockType: Mobile Login:Registration Page
            RockMigrationHelper.DeleteAttribute("D75EF452-D6FF-4188-B6BC-2D57D79FFF43");
            // Attrib for BlockType: Mobile Image:Cache Duration
            RockMigrationHelper.DeleteAttribute("C994AFE9-D863-468B-8BFB-826E2D58250D");
            // Attrib for BlockType: Mobile Image:Dynamic Content
            RockMigrationHelper.DeleteAttribute("4B21CA12-CCFB-4F32-98C7-767A5F766320");
            // Attrib for BlockType: Mobile Image:Image Url
            RockMigrationHelper.DeleteAttribute("76BD1AF1-2882-42C6-A851-F4C0478A2695");
            // Attrib for BlockType: Mobile Dynamic Content:Initial Content
            RockMigrationHelper.DeleteAttribute("7EA66B54-ABAD-4A56-B202-F86ED43558F4");
            // Attrib for BlockType: Mobile Dynamic Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("39276522-4072-44C9-9009-9FB4FC065612");
            // Attrib for BlockType: Mobile Dynamic Content:Content
            RockMigrationHelper.DeleteAttribute("8E637749-DFB1-46CC-984B-1D5E5F176AC4");
            // Attrib for BlockType: Mobile Content:Lava Render Location
            RockMigrationHelper.DeleteAttribute("31524A3A-BD70-4012-8D4E-0CBC4E39D412");
            // Attrib for BlockType: Mobile Content:Cache Duration
            RockMigrationHelper.DeleteAttribute("0F31D639-C5B9-4346-BAB8-EA391BF628C9");
            // Attrib for BlockType: Mobile Content:Dynamic Content
            RockMigrationHelper.DeleteAttribute("6F708113-0998-4AC1-BE88-FE6BBE63D0F5");
            // Attrib for BlockType: Mobile Content:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute("F572205C-59A9-46B8-971D-1749ADF1907A");
            // Attrib for BlockType: Mobile Content:Content
            RockMigrationHelper.DeleteAttribute("FC03F001-ECAD-42A2-8590-24CC75025BD7");
            // Attrib for BlockType: Content Channel Item List:Cache Duration
            RockMigrationHelper.DeleteAttribute("F64D1CDB-B878-4B74-8A78-C67DF44265D3");
            // Attrib for BlockType: Content Channel Item List:List Data Template
            RockMigrationHelper.DeleteAttribute("7B48C696-9856-4BEC-B61B-AB7EBDD24135");
            // Attrib for BlockType: Content Channel Item List:Order
            RockMigrationHelper.DeleteAttribute("4C033E8A-491E-43BE-AA33-67A4DAEE07A3");
            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            RockMigrationHelper.DeleteAttribute("FADC568E-1C7A-4589-B153-B26F931D5409");
            // Attrib for BlockType: Content Channel Item List:Filter Id
            RockMigrationHelper.DeleteAttribute("054CEE96-C0A2-45AD-A43F-0EF13D1E6FE3");
            // Attrib for BlockType: Content Channel Item List:Field Settings
            RockMigrationHelper.DeleteAttribute("C8C01BE7-552D-4E6D-82B3-28D1315DBACD");
            // Attrib for BlockType: Content Channel Item List:Detail Page
            RockMigrationHelper.DeleteAttribute("FA421A8C-EF58-4695-8152-EFE94A6EB81B");
            // Attrib for BlockType: Content Channel Item List:Include Following
            RockMigrationHelper.DeleteAttribute("10C9C7F0-92E8-49A5-AE22-63345E4712BA");
            // Attrib for BlockType: Content Channel Item List:Page Size
            RockMigrationHelper.DeleteAttribute("7428F31A-EE60-408C-BE6D-9E18BCB26663");
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.DeleteAttribute("D64C1B9D-6CED-429E-9212-E7AE77646E54");
            RockMigrationHelper.DeleteBlockType("401079C6-01E3-4F72-9FFD-7ABC8741FBBE"); // Connection Status Changes
            RockMigrationHelper.DeleteBlockType("D7270081-3A75-4FFE-97CD-330AEAB621B4"); // Assessment Type List
            RockMigrationHelper.DeleteBlockType("F24E1102-FA8D-4FA5-8DF1-832EF6621A62"); // Assessment Type Detail
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
        /// SK: Added new Absence Group Member Notification Email Template
        /// </summary>
        private void AddNewAbsenceGroupMemberNotificationEmailTemplate()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Absence Group Member Notification", "", "", "", "", "", "Absent Group Members | {{ 'Global'  | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}  <p>  {{ Person.NickName }}, </p>  <p>  We wanted to make you aware of additional individuals who have missed the last few times that your {{ Group.Name }} group met. The individuals' names and contact information can be found below. Our goal is to find out if there’s a situation the church should know about, or if perhaps they’ve found another group to join. </p>   <table cellpadding=""25"">  {% for absentMember in AbsentMembers %}   <tr><td>   <strong>{{ absentMember.FullName }}</strong><br />   {%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}   {%- assign homePhone = absentMember.PhoneNumbers | Where:'NumberTypeValueId', 13 | Select:'NumberFormatted' -%}   {%- assign homeAddress = absentMember | Address:'Home' -%}      {%- if mobilePhone != empty -%}   Mobile Phone: {{ mobilePhone }}<br />   {%- endif -%}      {%- if homePhone != empty -%}   Home Phone: {{ homePhone }}<br />   {%- endif -%}      {%- if absentMember.Email != empty -%}   {{ absentMember.Email }}<br />   {%- endif -%}      <p>   {%- if homeAddress != empty -%}   Home Address <br />   {{ homeAddress }}   {%- endif -%}   </p>      </td></tr>  {% endfor %}  </table>    <p>   Once you have connected with these individuals, please update their status in your group if appropriate, or let us know if there’s anything the church needs to be aware of in their lives.  </p>   <p>   Thank you for your ongoing commitment to {{ 'Global' | Attribute:'OrganizationName' }}.  </p>   {{ 'Global' | Attribute:'EmailFooter' }}", "8747131E-3EDA-4FB0-A484-C2D2BE3918BA" );
        }

        /// <summary>
        /// GJ: Fix typo on googlestaticmap shortcode
        /// </summary>
        private void FixGooglestaticmapShortcode()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    This shortcode returns a static map (think Google Map as an image) based on parameters you provide. Let''s look at a simple example.
</p>

<pre>{[ googlestaticmap center:''10451 W Palmeras Dr Sun City, AZ 85373-2000'' zoom:''12'' ]}
{[ endgooglestaticmap ]}</pre>

<p>
    Here''s we''re showing a simple map centered around the address we provided at a reasonable zoom level. Note that you could 
    also provide the center point as a lat/long if you prefer.
</p>

<p>
So, what about markers, yep you can do that too. Notice when you use makers you don''t have to provide a center point or zoom.
</p>

<pre>{[ googlestaticmap ]}
    [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
    [[ marker location:''1 Cardinals Dr, Glendale, AZ 85305'' ]] [[ endmarker ]]
{[ endgooglestaticmap ]}</pre>

<p>
    Note here we provided both a lat/long and a street address. Either is just fine. With the markers, you can also 
    provide it other options. Here are your options:
</p>
<ul>
    <li><strong>location</strong> – The location (address) of the pin.</li>
    <li><strong>color</strong> – The color of the pin, can be a basic color from the list below or a 24-bit color in the pattern of 0xFFFFFF.</li>
        (black, brown, green, purple, yellow, blue, gray, orange, red, white).
    <li><strong>size</strong> – The size of the pin (valid values include: tiny,mid,small)</li>
    <li><strong>label</strong> - The letter or number that you want on the pin.</li>
    <li><strong>icon</strong> – The url to the png file to use for the pin icon.</li>
</ul>
<p>
    Now that we have the pins taken care of let''s look at the other options you can place on the shortcut:
</p>

<ul>
    <li><strong>returnurl</strong> (false) – By default the returned map will be in an image tag with a div to wrap it. Setting returnurl to ''true'' 
        will return just the URL (say for adding it as a background image).</li>
    <li><strong>width</strong> (100%) – The image will be added to a responsive div. The setting allows you to set the width of the div to a percent or px.</li>
    <li><strong>imagesize</strong> – The size of the image to get from the Google API. If you are using the free API you are limited to 640x640, though you can actually double 
        that if you set the scale = 2. If you buy the premium plan that size goes up to 2048x2048.</li>
    <li><strong>maptype</strong> (roadmap) – This determines the type of map to return. Valid values include roadmap, satellite, hybrid, and terrain.</li>
    <li><strong>scale</strong> (1) – This effects the number of pixels that are returned (usually used for creating high DPI images (aka retina). You can use this though to 
        double the size of the 640px limit. Valid values are 1 or 2 or 4 (4 requires the premium plan).</li>
    <li><strong>zoom</strong> (optional if used with markers) – The zoom level of the map. This must be value between 1-20. Below is a rough idea of the scales:
        <ul>
            <li>1 = world</li>
            <li>5 = continent</li>
            <li>10 = city</li>
            <li>15 = streets</li>
            <li>20 = buildings</li>
        </ul>
        </li>
    <li><strong>center</strong> (optional if used with markers) – The center point of the map. Can be either a street address or lat/long.</li>
    <li><strong>format</strong> (png8) – The format type of the image that is returned. Valid values include png8, png32, gif, jpg, jpg-baseline.</li>
    <li><strong>style</strong> (optional) – The returned map can be styled in an infinite (or close to) number of ways. See the Google Static Map documentation for 
        details (https://developers.google.com/maps/documentation/static-maps/styling).</li>
</ul>' WHERE ([Guid]='2DD53FE6-6EB2-4EC8-A965-3F71054F7983')" );
        }

        /// <summary>
        /// GJ: Fix Google Map Typo (Sever)
        /// </summary>
        private void FixGoogleMapTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    Adding a Google map to your page always starts out sounding easy… until… you get to the details. Soon the whole day is wasted and you don''t have much to 
    show. This shortcode makes it easy to add responsive Google Maps to your site. Let''s start with a simple example and work our way to more complex use cases.
</p>

<p>
    Note: Do to this javascript requirements of this shortcode you will need to do a full page reload before changes to the 
    shortcode appear on your page.
</p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    In the example above we mapped a single point to our map. Pretty easy, but not very helpful. We can add additional points by providing more markers.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    Ok… we''re getting there, but what if we wanted titles and information windows for our markers, oh and custom markers 
    too? You can provide optional parameters for each marker as shown below.
</p>

<pre>[[ marker location:''latitude,longitude'' title:''My Title'' icon:''icon url'' ]] info window content [[ endmarker ]]</pre>

<p><strong>Example:</strong></p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' title:''Spark Global Headquarters'']]
        &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;
        It''s not as grand as it sounds.&lt;br&gt;
        &lt;img src=""https://rockrms.blob.core.windows.net/misc/spark-logo.png"" width=""179"" height=""47""&gt;          
    [[ endmarker ]]
    [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
{[ endgooglemap ]}</pre>

<p></p><p>
    Note: A list of great resources for custom map markers is below:
</p>

<ul>
    <li><a href=""http://map-icons.com/"">Map Icons</a></li>
    <li><a href=""https://mapicons.mapsmarker.com/"">Map Icons Collection</a></li>
    <li><a href=""https://github.com/Concept211/Google-Maps-Markers"">Google Maps Markers</a></li>
</ul>

<p>
    There are several other parameters for you to use to control the options on your map. They include:
</p>

<ul>
    <li><strong>height</strong> (600px) – The height of the map.</li>
    <li><strong>width</strong> (100%) – The responsive width of the map.</li>
    <li><strong>zoom</strong> (optional) – The zoom level of the map. Note when two or more points are provided the map will auto zoom to place all of the points on the map. The range of the zoom scale is 1 (the furthest out, largest) to 20 (the closest, smallest). The approximate zoom levels are: 
    <ul>
        <li>1 = world</li>
        <li>5 = continent</li>
        <li>10 = city</li>
        <li>15 = streets</li>
        <li>20 = buildings</li>
    </ul>
    </li>
    <li><strong>center</strong> (optional) – The center point on the map. If you do not provide a center a default will be calculated based on the points given.</li>
    <li><strong>maptype</strong> (roadmap) – The type of map to display. The options are ‘roadmap'', ‘hybrid'', ‘satellite'' or ‘terrain''.</li>
    <li><strong>showzoom</strong> (true) – Should the zoom control be displayed.</li>
    <li><strong>showstreetview</strong> (false) – Should he StreetView control be displayed.</li>
    <li><strong>showfullscreen</strong> (true) – Should the control to show the map full screen be displayed.</li>
    <li><strong>showmapttype</strong> (false) – Should the control to change the map type be shown.</li>
    <li><strong>markeranimation</strong> (none) – The marker animation type. Options include: ''none'', ‘bounce'' (markers bounce continuously) or ''drop'' (markers drop in).</li>
    <li><strong>scrollwheel</strong> (true) – Determines if the scroll wheel should control the zoom level when the mouse is over the map.</li>
    <li><strong>draggable</strong> (true) – Determines if the mouse should be allowed to drag the center point of the map (allow the map to be moved).</li>
    <li><strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ‘cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.</li>
</ul>

<p>
    As you can see there are a lot of options in working with your map. You can also style your map by changing the colors. You do this by providing 
    the styling information in a separate [[ style ]] section. The styling settings for Google Maps is not pretty to look at or configure for that matter. Luckily, there 
    are several sites that allow you to download preconfigured map styles. Two of the best are called <a href=""https://snazzymaps.com"">SnazzyMaps</a> and 
    <a href=""https://mapstyle.withgoogle.com"">Map Style</a>. Below is an example showing how to add styling to your maps.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]] 
     [[ style ]]
        [{""featureType"":""all"",""elementType"":""all"",""stylers"":[{""visibility"":""on""}]},{""featureType"":""all"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""},{""saturation"":""-100""}]},{""featureType"":""all"",""elementType"":""labels.text.fill"",""stylers"":[{""saturation"":36},{""color"":""#000000""},{""lightness"":40},{""visibility"":""off""}]},{""featureType"":""all"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""},{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""all"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""administrative"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""administrative"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#000000""},{""lightness"":17},{""weight"":1.2}]},{""featureType"":""landscape"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""landscape"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape.natural"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry"",""stylers"":[{""lightness"":21}]},{""featureType"":""poi"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""road"",""elementType"":""geometry"",""stylers"":[{""visibility"":""on""},{""color"":""#7f8d89""}]},{""featureType"":""road"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.highway"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":17}]},{""featureType"":""road.highway"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":29},{""weight"":0.2}]},{""featureType"":""road.arterial"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":18}]},{""featureType"":""road.arterial"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.arterial"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""road.local"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""transit"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":19}]},{""featureType"":""water"",""elementType"":""all"",""stylers"":[{""color"":""#2b3638""},{""visibility"":""on""}]},{""featureType"":""water"",""elementType"":""geometry"",""stylers"":[{""color"":""#2b3638""},{""lightness"":17}]},{""featureType"":""water"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.fill"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]}]
    [[ endstyle]]
{[ endgooglemap ]}</pre>

<p>
    Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply copy and pasted straight from SnazzyMaps!
</p>'
                WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')" );
        }

        /// <summary>
        /// GJ: Fix Accordion Typo (Simplifies)
        /// </summary>
        private void FixAccordianTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    <a href=""https://getbootstrap.com/docs/3.3/javascript/#collapse-example-accordion"">Bootstrap accordions</a> are a clean way of displaying a large 
    amount of structured content on a page. While they''re not incredibly difficult to make using just HTML this shortcode simplifies the markup 
    quite a bit. The example below shows an accordion with three different sections.
</p>

<pre>{[ accordion ]}

    [[ item title:''Lorem Ipsum'' ]]
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut pretium tortor et orci ornare 
        tincidunt. In hac habitasse platea dictumst. Aliquam blandit dictum fringilla. 
    [[ enditem ]]
    
    [[ item title:''In Commodo Dolor'' ]]
        In commodo dolor vel ante porttitor tempor. Ut ac convallis mauris. Sed viverra magna nulla, quis 
        elementum diam ullamcorper et. 
    [[ enditem ]]
    
    [[ item title:''Vivamus Sollicitudin'' ]]
        Vivamus sollicitudin, leo quis pulvinar venenatis, lorem sem aliquet nibh, sit amet condimentum
        ligula ex a risus. Curabitur condimentum enim elit, nec auctor massa interdum in.
    [[ enditem ]]

{[ endaccordion ]}</pre>

<p>
    The base control has the following options:
</p>

<ul>
    <li><strong>paneltype</strong> (default) - This is the CSS panel class type to use (e.g. ''default'', ''block'', ''primary'', ''success'', etc.)</li>
    <li><strong>firstopen</strong> (true) - Determines is the first accordion section should be open when the page loads. Setting this to false will
        cause all sections to be closed.</li>
</ul>

<p>
    The [[ item ]] block configuration has the following options:
</p>
<ul>
    <li><strong>title</strong> - The title of the section.</li>
</ul>'
                WHERE ([Guid]='18F87671-A848-4509-8058-C95682E7BAD4')" );
        }

        /// <summary>
        /// GJ: Fix Wordcloud Typo
        /// </summary>
        private void FixWordCloudTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode] SET [Documentation]=N'<p>
    This short cut takes a large amount of text and converts it to a word cloud of the most popular terms. It''s 
    smart enough to take out common words like ''the'', ''and'', etc. Below are the various options for this shortcode. If 
    you would like to play with the settings in real-time head over <a href=""https://www.jasondavies.com/wordcloud/"">the Javascript''s homepage</a>.
</p>

<pre>{[ wordcloud ]}
    ... out a lot of text here ...
{[ endwordcloud ]}
</pre>

<ul>
    <li><strong>width</strong> (960) – The width in pixels of the word cloud control.</li>
    <li><strong>height</strong> (420) - The height in pixels of the control.</li>
    <li><strong>fontname</strong> (Impact) – The font to use for the rendering.</li>
    <li><strong>maxwords</strong> (255) – The maximum number of words to use in the word cloud.</li>
    <li><strong>scalename</strong> (log) – The type of scaling algorithm to use. Options are ''log'', '' sqrt'' or ''linear''.</li>
    <li><strong>spiralname</strong> (archimedean) – The type of spiral used for positioning words. Options are ''archimedean'' or ''rectangular''.</li>
    <li><strong>colors</strong> (#0193B9,#F2C852,#1DB82B,#2B515D,#ED3223) – A comma delimited list of colors to use for the words.</li>
    <li><strong>anglecount</strong> (6) – The maximum number of angles to place words on in the cloud.</li>
    <li><strong>anglemin</strong> (-90) – The minimum angle to use when drawing the cloud.</li>
    <li><strong>anglemax</strong> (90) – The maximum angle to use when drawing the cloud.</li>
</ul>'
                WHERE ([Guid]='CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4')" );
        }

        /// <summary>
        /// GJ: Parallax Typos
        /// </summary>
        private void FixParallaxTypeo()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>
<pre>{[ parallax image:''http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'' contentpadding:''20px'' ]}
    &lt;h1&gt;Hello World&lt;/h1&gt;
{[ endparallax ]}</pre>

<p>  
    This shortcode takes the content you provide it and places it into a div with a parallax background using the image you provide in the ''image'' 
    parameter. As always, there are several parameters.
</p>
    
<ul>
    <li><strong>image</strong> (required) – A valid URL to the image that should be used as the background.</li><li><b>height</b> (200px) – The minimum height of the content. This is useful if you want your section to not have any 
    content, but instead be just the parallax image.</li>
    <li><strong>videourl</strong> - This is the URL to use if you''d like a video background.</li>
    <li><strong>speed</strong> (50) – the speed that the background should scroll. The value of 0 means the image will be fixed in place, the value of 100 would make the background scroll quick up as the page scrolls down, while the value of -100 would scroll quickly in the opposite direction.</li>
    <li><strong>zindex</strong> (1) – The z-index of the background image. Depending on your design you may need to adjust the z-index of the parallax image. </li>
    <li><strong>position</strong> (center center) - This is analogous to the background-position css property. Specify coordinates as top, bottom, right, left, center, or pixel values (e.g. -10px 0px). The parallax image will be positioned as close to these values as possible while still covering the target element.</li>
    <li><strong>contentpadding</strong> (0) – The amount of padding you’d like to have around your content. You can provide any valid CSS padding value. For example, the value ‘200px 20px’ would give you 200px top and bottom and 20px left and right.</li>
    <li><strong>contentcolor</strong> (#fff = white) – The font color you’d like to use for your content. This simplifies the styling of your content.</li>
    <li><strong>contentalign</strong> (center) – The alignment of your content inside of the section. </li>
    <li><strong>noios</strong> (false) – Disables the effect on iOS devices.</li>
    <li><strong>noandriod</strong> (center) – Disables the effect on Android devices.</li>
</ul>
<p>Note: Due to the javascript requirements of this shortcode, you will need to do a full page reload before changes to the shortcode appear on your page.</p>'
                WHERE ([Guid]='4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4')" );
        }

        /// <summary>
        /// DL: Add Connection Status Changes Pages and Blocks
        /// </summary>
        private void ConnectionStatusChangesUp()
        {
            // Add Page to Site: Tools/Data Integrity/Connection Status Changes
            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Status Changes", "A summary of Connection Status changes for people, filtered by date and Campus.", "97624123-900C-4442-B42E-19CF95877E04", "fa fa-exchange-alt" ); // Site:Rock RMS

            // Add Block Type: Connection Status Changes
            RockMigrationHelper.UpdateBlockType( "Connection Status Changes", "Shows a summary of Connection Status changes for people, filtered by date and Campus.", "~/Blocks/Crm/ConnectionStatusChangeReport.ascx", "CRM", "FE50DDE5-3D8C-47EC-817D-21348717AD38" );

            // Add Block to Page: Connection Status Changes Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "97624123-900C-4442-B42E-19CF95877E04".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "FE50DDE5-3D8C-47EC-817D-21348717AD38".AsGuid(), "Connection Status Changes", "Main", @"", @"", 0, "69D20D1C-1065-48CB-8BD0-1DD4A305F688" );

            // Attrib for BlockType: Connection Status Changes:Person Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE50DDE5-3D8C-47EC-817D-21348717AD38", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", @"", @"", 0, @"", "25812515-76B0-4C02-9732-C298765BA0EA" );

            // Attrib Value for Block:Connection Status Changes, Attribute:Person Detail Page Page: Connection Status Changes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69D20D1C-1065-48CB-8BD0-1DD4A305F688", "25812515-76B0-4C02-9732-C298765BA0EA", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
        }

        /// <summary>
        /// DL: Add Connection Status Changes Pages and Blocks
        /// </summary>
        private void ConnectionStatusChangesDown()
        {
            // Remove Attrib for BlockType: Connection Status Changes:Person Detail Page
            RockMigrationHelper.DeleteAttribute( "25812515-76B0-4C02-9732-C298765BA0EA" );
            // Remove Block: Connection Status Changes, from Page: Connection Status Changes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "69D20D1C-1065-48CB-8BD0-1DD4A305F688" );
            // Remove Block Type: Connection Status Changes
            RockMigrationHelper.DeleteBlockType( "FE50DDE5-3D8C-47EC-817D-21348717AD38" );
            // Remove Page: //  Page: Connection Status Changes, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "97624123-900C-4442-B42E-19CF95877E04" );
        }

        /// <summary>
        /// DL: Hide Breadcrumbs for Steps Pages
        /// </summary>
        private void HideBreadcrumbsForStepsPages()
        {
            // Switch off breadcrumb display for Step Program and Step Type pages.
            Sql( @"update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] in ('6E46BC35-1FCB-4619-84F0-BB6926D2DDD5','8E78F9DC-657D-41BF-BE0F-56916B6BF92F')" );
        }

        /// <summary>
        /// ED: Updates to Motivator Assessment Colors
        /// </summary>
        private void UpdateMotivatorAssessmentColors()
        {
            // Updates for Motivator Theme colros
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f26863" );
            // Updates for Motivator graph colors
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
        }

        /// <summary>
        /// GJ: ChartJS Fix Migration
        /// </summary>
        private void FixChartJS()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Parameters]=N'fillcolor^rgba(5,155,255,.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|yaxislabels^#777|fontfamily^sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5,155,255,.6)|pointhoverbordercolor^rgba(5,155,255,.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear|yaxislabels^|yaxismin^|yaxismax^|yaxisstepsize^'
                WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        /// <summary>
        /// DL: Add Assessments Admin Pages and Blocks
        /// </summary>
        private void AddAssessmentAdminPagesUp()
        {
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Assessment Types", "", "CC59F2B4-16B4-47BE-B8A0-E417EABA068F", "fa fa-directions" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CC59F2B4-16B4-47BE-B8A0-E417EABA068F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Assessment Type Detail", "", "F3C96663-1079-4F20-BABA-3F3203AFCFF3", "fa fa-directions" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Assessment Type List", "Shows a list of all Assessment Types.", "~/Blocks/Assessments/AssessmentTypeList.ascx", "Assessments", "00A86827-1E0C-4F47-8A6F-82581FA75CED" );
            RockMigrationHelper.UpdateBlockType( "Assessment Type Detail", "Displays the details of the given Assessment Type for editing.", "~/Blocks/Assessments/AssessmentTypeDetail.ascx", "Assessments", "A81AB554-B438-4C7F-9C45-1A9AE2F889C5" );

            // Add Block to Page: Assessment Type Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F3C96663-1079-4F20-BABA-3F3203AFCFF3".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A81AB554-B438-4C7F-9C45-1A9AE2F889C5".AsGuid(), "Assessment Type Detail", "Main", @"", @"", 0, "8918560C-B8E0-4ED6-9379-BCC191A57B65" );
            // Add Block to Page: Assessment Types Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CC59F2B4-16B4-47BE-B8A0-E417EABA068F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "00A86827-1E0C-4F47-8A6F-82581FA75CED".AsGuid(), "Assessment Type List", "Main", @"", @"", 0, "8D486E88-EB00-40C7-8C66-90B9B92E8823" );

            // Attrib for BlockType: Assessment Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "00A86827-1E0C-4F47-8A6F-82581FA75CED", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "A0DFC7E8-6E63-403F-9D1B-5E3D3684AD8D" );

            // Attrib Value for Block:Assessment Type List, Attribute:Detail Page Page: Assessment Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D486E88-EB00-40C7-8C66-90B9B92E8823", "A0DFC7E8-6E63-403F-9D1B-5E3D3684AD8D", @"f3c96663-1079-4f20-baba-3f3203afcff3" );
        }

        /// <summary>
        /// DL: Add Assessments Admin Pages and Blocks
        /// </summary>
        private void AddAssessmentAdminPagesDown()
        {
            // Attrib for BlockType: Assessment Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "A0DFC7E8-6E63-403F-9D1B-5E3D3684AD8D" );

            // Remove Block: Assessment Type List, from Page: Assessment Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8D486E88-EB00-40C7-8C66-90B9B92E8823" );
            // Remove Block: Assessment Type Detail, from Page: Assessment Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8918560C-B8E0-4ED6-9379-BCC191A57B65" );

            RockMigrationHelper.DeleteBlockType( "A81AB554-B438-4C7F-9C45-1A9AE2F889C5" ); // Assessment Type Detail
            RockMigrationHelper.DeleteBlockType( "00A86827-1E0C-4F47-8A6F-82581FA75CED" ); // Assessment Type List

            RockMigrationHelper.DeletePage( "F3C96663-1079-4F20-BABA-3F3203AFCFF3" ); //  Page: Assessment Type Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "CC59F2B4-16B4-47BE-B8A0-E417EABA068F" ); //  Page: Assessment Types, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// MP: NULL out migrations models in __MigrationHistory
        /// </summary>
        private void ClearMigrationModelsForPreviousVersion()
        {
            Sql( @"
                UPDATE [__MigrationHistory]
                SET [Model] = 0x
                WHERE MigrationId < '201907112025023_CampusStatus'" );
        }

        /// <summary>
        /// NA: Set all ConnectionActivityTypes to Active
        /// </summary>
        private void ConnectionActivityTypesToActive()
        {
            Sql( @"UPDATE [ConnectionActivityType] SET [IsActive] = 1 WHERE [IsActive] = 0" );
        }

        /// <summary>
        /// SK: Renamed Attribute Values block Setting Key
        /// </summary>
        private void RenameAttributeValuesBlockAttibutes()
        {
            Sql( @"
                DECLARE @BlockTypeId INT = (SELECT [Id] from [BlockType] where [Guid]='D70A59DC-16BE-43BE-9880-59598FA7A94C')
                IF @BlockTypeId IS NOT NULL
	                BEGIN
	                UPDATE
		                [Attribute]
	                SET [Name]='Block Title',
		                [Key] = 'BlockTitle',
		                [AbbreviatedName]='Block Title'
	                WHERE
		                [EntityTypeQualifierColumn]='BlockTypeId' AND EntityTypeQualifierValue=@BlockTypeId AND [Key]='SetPageTitle'

	                UPDATE
		                [Attribute]
	                SET [Name]='Block Icon',
	                [Key] = 'BlockIcon',
	                [AbbreviatedName]='Block Icon'
	                WHERE
		                [EntityTypeQualifierColumn]='BlockTypeId' AND EntityTypeQualifierValue=@BlockTypeId AND [Key]='SetPageIcon'
	                END" );
        }

        /// <summary>
        /// GJ: Set PageRoutes to system Routes
        /// </summary>
        private void SetPageRoutesToSystemRoutes()
        {
            Sql( @"
                UPDATE [PageRoute]
                SET [IsSystem]= '1'
                WHERE (
                    [Guid]='B4E63C6F-AAC1-411C-B28B-E1A3113A87EF' OR
                    [Guid]='0C9DD175-1089-4D1B-979B-80ABDF28B665' OR
                    [Guid]='F4FD6B34-AABA-4DCD-9073-70833ED37589' OR
                    [Guid]='150FEBDB-01FA-4048-A10E-DFFB36821E84' OR
                    [Guid]='543FA0BB-8E7A-4F63-81FB-144D60A01A6D' OR
                    [Guid]='796D5B39-FF89-49E1-878C-D338FDD4D82C')" );
        }
    }
}
