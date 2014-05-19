// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
     
    /// <summary>
    ///
    /// </summary>
    public partial class BlocksPages02 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "Resident", "", "Default", "F98B0061-8327-4B96-8A5E-B3C58D899B31", "" );
            AddPage( "F98B0061-8327-4B96-8A5E-B3C58D899B31", "Resident Home", "", "Default", "826C0BFF-C831-4427-98F9-57FF462D82F5", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "Resident Competency Detail - Projects", "", "Default", "ADE663B9-386B-479C-ABD9-3349E1B4B827", "" );
            AddPage( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "Resident Project Detail", "", "Default", "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "" );
            AddPage( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "Resident Grade Request", "", "Default", "5D729D30-8E33-4913-A56F-98F803479C6D", "" );
            AddPage( "5D729D30-8E33-4913-A56F-98F803479C6D", "Resident Grade Project Detail", "", "Default", "A16C4B0F-66C6-4CF0-8B54-B232DDF553B9", "" );
            AddPage( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "Project Assessment", "", "Default", "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "" );
            AddPage( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "Point of Assessment", "", "Default", "4827C8D3-B0FA-4AA4-891F-1F27C7D76606", "" );
            AddPage( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "Resident Project Assessment Detail", "", "Default", "0DF59029-C17B-474D-8DD1-ED312B734202", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "Resident Competency Detail - Goals", "", "Default", "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "" );
            AddPage( "826C0BFF-C831-4427-98F9-57FF462D82F5", "Resident Competency Detail - Notes", "", "Default", "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "" );
            AddBlockType( "com_ccvonline - Residency - Resident Competency Detail", "", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyDetail.ascx", "536A0B29-B427-434D-82B6-C5CE6A8E07FE" );
            AddBlockType( "com_ccvonline - Residency - Resident Competency List", "", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyList.ascx", "2D404077-7723-4528-A893-800658BAEA4F" );
            AddBlockType( "com_ccvonline - Residency - Resident Project Point Of Assessment List", "", "~/Plugins/com_ccvonline/Residency/ResidentProjectPointOfAssessmentList.ascx", "BC3048EE-6964-4ABB-B710-4616136045BA" );
            AddBlockType( "com_ccvonline - Residency - Resident Project Detail", "", "~/Plugins/com_ccvonline/Residency/ResidentProjectDetail.ascx", "13A42E92-8D1A-407D-B2D1-2472BBC27D13" );
            AddBlockType( "com_ccvonline - Residency - Resident Grade Request", "", "~/Plugins/com_ccvonline/Residency/ResidentGradeRequest.ascx", "1AD0421D-8B24-4A5A-842F-AF37EACBE35E" );
            AddBlockType( "com_ccvonline - Residency - Resident Grade Detail", "", "~/Plugins/com_ccvonline/Residency/ResidentGradeDetail.ascx", "ABDDD216-80B8-427D-8689-0CF84C9C5646" );
            AddBlockType( "com_ccvonline - Residency - Competency Person Project Assessment Detail", "", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentDetail.ascx", "470EB28D-75A7-46C6-BB74-525A66BD114E" );
            AddBlockType( "com_ccvonline - Residency - Competency Person Project Assessment List", "", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentList.ascx", "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4" );
            AddBlockType( "com_ccvonline - Residency - Competency Person Project Assessment Point Of Assessment Detail", "", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentDetail.ascx", "B2F1D26F-0C4F-46F1-91A4-ACBBB50E4202" );
            AddBlockType( "com_ccvonline - Residency - Competency Person Project Assessment Point Of Assessment List", "", "~/Plugins/com_ccvonline/Residency/CompetencyPersonProjectAssessmentPointOfAssessmentList.ascx", "DE2FCBEA-F103-43BC-87E4-4235FA361B87" );
            AddBlockType( "com_ccvonline - Residency - Resident Project Assessment Detail", "", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentDetail.ascx", "D2835421-1D69-4D2E-80BC-836FF606ADDD" );
            AddBlockType( "com_ccvonline - Residency - Resident Project Assessment List", "", "~/Plugins/com_ccvonline/Residency/ResidentProjectAssessmentList.ascx", "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20" );
            AddBlockType( "com_ccvonline - Residency - Resident Competency Goals Detail", "", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyGoalsDetail.ascx", "4BD8E3F7-30D3-49C2-B3D6-B897174A9AB8" );
            AddBlockType( "com_ccvonline - Residency - Resident Competency Project List", "", "~/Plugins/com_ccvonline/Residency/ResidentCompetencyProjectList.ascx", "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9" );

            AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "5A880084-7237-449A-9855-3FA02B6BD79F", "Marketing Campaign Ads Xslt", "", "Content", 0, "D1AEB343-41A8-4501-9475-40103C7AEA0A" );

            AddBlock( "826C0BFF-C831-4427-98F9-57FF462D82F5", "2D404077-7723-4528-A893-800658BAEA4F", "Resident Competency List", "", "Content", 1, "EE97ABE8-A124-4437-B962-805C1D0C18D4" );

            AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "", "Content", 0, "0BC0C139-26FB-403C-A5ED-BAA1CC1231FD" );

            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "BC3048EE-6964-4ABB-B710-4616136045BA", "Resident Project Point Of Assessment List", "", "Content", 2, "C482389C-6CAC-4EB9-9C56-5FFCC0D17639" );

            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "13A42E92-8D1A-407D-B2D1-2472BBC27D13", "Resident Project Detail", "", "Content", 0, "402DB31D-7C84-4154-890E-D18AEE5FC0E2" );

            AddBlock( "5D729D30-8E33-4913-A56F-98F803479C6D", "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "Resident Grade Request", "", "Content", 0, "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B" );

            AddBlock( "A16C4B0F-66C6-4CF0-8B54-B232DDF553B9", "ABDDD216-80B8-427D-8689-0CF84C9C5646", "Resident Grade Detail", "", "Content", 0, "52B23A41-93BE-4A65-BF58-C98ED89E54B7" );

            AddBlock( "39661338-971E-45EA-86C3-7A8A5D2DEA54", "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4", "Competency Person Project Assessment List", "", "Content", 1, "C27D517E-128B-4A1B-B069-C367C7B59AAD" );

            AddBlock( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "470EB28D-75A7-46C6-BB74-525A66BD114E", "Competency Person Project Assessment Detail", "", "Content", 0, "76714DC6-A171-4E3A-8B61-EE7659968918" );

            AddBlock( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4", "DE2FCBEA-F103-43BC-87E4-4235FA361B87", "Competency Person Project Assessment Point Of Assessment List", "", "Content", 1, "5AC72357-5942-46D8-9BE8-27B1E5239DF3" );

            AddBlock( "4827C8D3-B0FA-4AA4-891F-1F27C7D76606", "B2F1D26F-0C4F-46F1-91A4-ACBBB50E4202", "Competency Person Project Assessment Point Of Assessment Detail", "", "Content", 0, "A4571853-BFF8-4599-8B7C-1A2402C06C05" );

            AddBlock( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0", "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "Resident Project Assessment List", "", "Content", 3, "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F" );

            AddBlock( "0DF59029-C17B-474D-8DD1-ED312B734202", "D2835421-1D69-4D2E-80BC-836FF606ADDD", "Resident Project Assessment Detail", "", "Content", 0, "0A1F651B-621F-4577-B35B-DD9E2FE22302" );

            AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "", "Content", 1, "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9" );

            AddBlock( "ADE663B9-386B-479C-ABD9-3349E1B4B827", "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9", "Resident Competency Project List", "", "Content", 2, "1843F88B-B17F-4F81-AED8-91B87C0A2816" );

            AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "", "Content", 0, "A3237301-8DAD-4E96-8871-1DE4FF5395A1" );

            AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "", "Content", 1, "4132D452-7672-4275-9D4E-B7D6A7DFE745" );

            AddBlock( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7", "4BD8E3F7-30D3-49C2-B3D6-B897174A9AB8", "Resident Competency Goals Detail", "", "Content", 2, "B48D7551-B398-431F-87EE-2786465C5A13" );

            AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "Resident Competency Detail", "", "Content", 0, "D4800BCD-B7F1-4D22-ACDF-5F04FB49E148" );

            AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "", "Content", 1, "55629043-F1BB-4B07-8AC7-A32D8B1F632C" );

            AddBlock( "130FA92D-9D5F-45D1-84AA-B399F2E868E6", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "", "Content", 2, "FDB0021C-485F-4B75-82D0-514CDBD59B7C" );


            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency List:Detail Page
            AddBlockTypeAttribute( "2D404077-7723-4528-A893-800658BAEA4F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "07B337A8-F3AE-4985-86BC-4B268257DF77" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Detail:Detail Page
            AddBlockTypeAttribute( "536A0B29-B427-434D-82B6-C5CE6A8E07FE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "E72925FC-288C-4FA9-A509-472870A231ED" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Detail:Grade Request Page
            AddBlockTypeAttribute( "13A42E92-8D1A-407D-B2D1-2472BBC27D13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Grade Request Page", "GradeRequestPage", "", "", 0, "", "0CAB3D65-0492-4CD1-BAEE-139A2BF9A7BA" );

            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment List:Detail Page
            AddBlockTypeAttribute( "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "3934ED43-CEAF-4C7E-8022-7F5668A431DE" );

            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment Point Of Assessment List:Detail Page
            AddBlockTypeAttribute( "DE2FCBEA-F103-43BC-87E4-4235FA361B87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "89638EF8-98DE-4852-BB12-AA35D3EBA238" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Assessment List:Detail Page
            AddBlockTypeAttribute( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Project List:Detail Page
            AddBlockTypeAttribute( "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "A7168B42-80A7-4077-8949-5012C6793DF1" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Resident Grade Detail Page
            AddBlockTypeAttribute( "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Resident Grade Detail Page", "ResidentGradeDetailPage", "", "", 0, "", "06B61C6B-84D9-4F3E-A191-F51531A2E905" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Detail:Person Project Detail Page
            AddBlockTypeAttribute( "ABDDD216-80B8-427D-8689-0CF84C9C5646", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Project Detail Page", "PersonProjectDetailPage", "", "", 0, "", "70602088-6F35-4C04-9959-6F21A4BC640F" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Residency Grader Security Role
            AddBlockTypeAttribute( "1AD0421D-8B24-4A5A-842F-AF37EACBE35E", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Residency Grader Security Role", "ResidencyGraderSecurityRole", "", "", 0, "", "DEDE968C-1347-4037-9BFC-A62C7BA59186" );

            // Attrib Value for Resident Competency List:Detail Page
            AddBlockAttributeValue( "EE97ABE8-A124-4437-B962-805C1D0C18D4", "07B337A8-F3AE-4985-86BC-4B268257DF77", "ade663b9-386b-479c-abd9-3349e1b4b827" );

            // Attrib Value for Resident Competency Detail:Detail Page
            AddBlockAttributeValue( "0BC0C139-26FB-403C-A5ED-BAA1CC1231FD", "E72925FC-288C-4FA9-A509-472870A231ED", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Resident Project Detail:Grade Request Page
            AddBlockAttributeValue( "402DB31D-7C84-4154-890E-D18AEE5FC0E2", "0CAB3D65-0492-4CD1-BAEE-139A2BF9A7BA", "5d729d30-8e33-4913-a56f-98f803479c6d" );

            // Attrib Value for Resident Grade Request:Residency Grader Security Role
            AddBlockAttributeValue( "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B", "DEDE968C-1347-4037-9BFC-A62C7BA59186", "31" );

            // Attrib Value for Resident Grade Request:Resident Grade Detail Page
            AddBlockAttributeValue( "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B", "06B61C6B-84D9-4F3E-A191-F51531A2E905", "a16c4b0f-66c6-4cf0-8b54-b232ddf553b9" );

            // Attrib Value for Resident Grade Detail:Person Project Detail Page
            AddBlockAttributeValue( "52B23A41-93BE-4A65-BF58-C98ED89E54B7", "70602088-6F35-4C04-9959-6F21A4BC640F", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Competency Person Project Assessment List:Detail Page
            AddBlockAttributeValue( "C27D517E-128B-4A1B-B069-C367C7B59AAD", "3934ED43-CEAF-4C7E-8022-7F5668A431DE", "a4be6749-0190-4655-b3f6-0ceec2ddd5c4" );

            // Attrib Value for Competency Person Project Assessment Point Of Assessment List:Detail Page
            AddBlockAttributeValue( "5AC72357-5942-46D8-9BE8-27B1E5239DF3", "89638EF8-98DE-4852-BB12-AA35D3EBA238", "4827c8d3-b0fa-4aa4-891f-1f27c7d76606" );

            // Attrib Value for Resident Project Assessment List:Detail Page
            AddBlockAttributeValue( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F", "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D", "0df59029-c17b-474d-8dd1-ed312b734202" );

            // Attrib Value for Resident Competency Project List:Detail Page
            AddBlockAttributeValue( "1843F88B-B17F-4F81-AED8-91B87C0A2816", "A7168B42-80A7-4077-8949-5012C6793DF1", "56f3e462-28ef-4ec5-a58c-c5fde48356e0" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Projects
            AddBlockAttributeValue( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Goals
            AddBlockAttributeValue( "4132D452-7672-4275-9D4E-B7D6A7DFE745", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "True" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "9909E07F-0E68-43B8-A151-24D03C795093", "3" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "55629043-F1BB-4B07-8AC7-A32D8B1F632C", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );

            // Breadcrumb and Page Title updates
            Sql( @"
-- Period Detail
update [Page] set [BreadCrumbDisplayName] = 1 where [Guid] = 'F8D8663B-FE4F-4F48-A359-DBE656AE69A2'

-- Project Assessment
update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] = 'A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4'

-- Resident..
update [Page] set [BreadCrumbDisplayName] = 0 where [Guid] in
 ('F98B0061-8327-4B96-8A5E-B3C58D899B31',
 'A16C4B0F-66C6-4CF0-8B54-B232DDF553B9',
'5D729D30-8E33-4913-A56F-98F803479C6D',
'56F3E462-28EF-4EC5-A58C-C5FDE48356E0',
'ADE663B9-386B-479C-ABD9-3349E1B4B827',
'130FA92D-9D5F-45D1-84AA-B399F2E868E6',
'83DBB422-38C5-44F3-9FDE-3737AC8CF2A7',
'0DF59029-C17B-474D-8DD1-ED312B734202',
'4827C8D3-B0FA-4AA4-891F-1F27C7D76606')

update [Page] set [Title] = 'Projects' where [Guid] = 'ADE663B9-386B-479C-ABD9-3349E1B4B827'
update [Page] set [Title] = 'Goals' where [Guid] = '83DBB422-38C5-44F3-9FDE-3737AC8CF2A7'
update [Page] set [Title] = 'Notes' where [Guid] = '130FA92D-9D5F-45D1-84AA-B399F2E868E6'

" );

            Sql( @"
INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])
     VALUES(1
           ,(select [Id] from [Page] where [Guid] = '130FA92D-9D5F-45D1-84AA-B399F2E868E6')
           ,'com.ccvonline.Residency.Model.CompetencyPerson'
           ,'competencyPersonId'
           ,SYSDATETIME()
           ,'13366F47-7AFD-430D-A0E5-9AF92347DAE1')
" );

            // Attrib Value for Block:Notes, Attribute:Context Entity Type, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "FDB0021C-485F-4B75-82D0-514CDBD59B7C", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", "com.ccvonline.Residency.Model.CompetencyPerson" );

            // Attrib Value for Block:Notes, Attribute:Note Type, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "FDB0021C-485F-4B75-82D0-514CDBD59B7C", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6", "Notes" );

            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "FDB0021C-485F-4B75-82D0-514CDBD59B7C", "20243A98-4802-48E2-AF61-83956056AC65", "False" );

            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "FDB0021C-485F-4B75-82D0-514CDBD59B7C", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", "False" );

            // Attrib Value for Block:Notes, Attribute:Show Security Button, Page:Resident Competency Detail - Notes
            AddBlockAttributeValue( "FDB0021C-485F-4B75-82D0-514CDBD59B7C", "00B6EBFF-786D-453E-8746-119D0B45CB3E", "False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DELETE FROM [dbo].[PageContext] where [Guid] = '6DBF9B0A-FE1D-42E0-B6B9-C194495FABA7'" );

            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Detail:Person Project Detail Page
            DeleteAttribute( "70602088-6F35-4C04-9959-6F21A4BC640F" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Resident Grade Detail Page
            DeleteAttribute( "06B61C6B-84D9-4F3E-A191-F51531A2E905" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Project List:Detail Page
            DeleteAttribute( "A7168B42-80A7-4077-8949-5012C6793DF1" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Assessment List:Detail Page
            DeleteAttribute( "B6ABAF24-2D2E-4A0E-A195-C66E98110D2D" );
            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment Point Of Assessment List:Detail Page
            DeleteAttribute( "89638EF8-98DE-4852-BB12-AA35D3EBA238" );
            // Attrib for BlockType: com _ccvonline - Residency - Competency Person Project Assessment List:Detail Page
            DeleteAttribute( "3934ED43-CEAF-4C7E-8022-7F5668A431DE" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Grade Request:Residency Grader Security Role
            DeleteAttribute( "DEDE968C-1347-4037-9BFC-A62C7BA59186" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Project Detail:Grade Request Page
            DeleteAttribute( "0CAB3D65-0492-4CD1-BAEE-139A2BF9A7BA" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency Detail:Detail Page
            DeleteAttribute( "E72925FC-288C-4FA9-A509-472870A231ED" );
            // Attrib for BlockType: com _ccvonline - Residency - Resident Competency List:Detail Page
            DeleteAttribute( "07B337A8-F3AE-4985-86BC-4B268257DF77" );

            DeleteBlock( "FDB0021C-485F-4B75-82D0-514CDBD59B7C" ); // Notes
            DeleteBlock( "55629043-F1BB-4B07-8AC7-A32D8B1F632C" ); // Page Xslt Transformation
            DeleteBlock( "D4800BCD-B7F1-4D22-ACDF-5F04FB49E148" ); // Resident Competency Detail
            DeleteBlock( "B48D7551-B398-431F-87EE-2786465C5A13" ); // Resident Competency Goals Detail
            DeleteBlock( "4132D452-7672-4275-9D4E-B7D6A7DFE745" ); // Page Xslt Transformation
            DeleteBlock( "A3237301-8DAD-4E96-8871-1DE4FF5395A1" ); // Resident Competency Detail
            DeleteBlock( "1843F88B-B17F-4F81-AED8-91B87C0A2816" ); // Resident Competency Project List
            DeleteBlock( "5116ABF3-4AE3-467E-B5DA-4E3920DF1CF9" ); // Page Xslt Transformation
            DeleteBlock( "0A1F651B-621F-4577-B35B-DD9E2FE22302" ); // Resident Project Assessment Detail
            DeleteBlock( "CA0F81CA-17B1-44EA-B8DF-C834DF8ED43F" ); // Resident Project Assessment List
            DeleteBlock( "A4571853-BFF8-4599-8B7C-1A2402C06C05" ); // Competency Person Project Assessment Point Of Assessment Detail
            DeleteBlock( "5AC72357-5942-46D8-9BE8-27B1E5239DF3" ); // Competency Person Project Assessment Point Of Assessment List
            DeleteBlock( "76714DC6-A171-4E3A-8B61-EE7659968918" ); // Competency Person Project Assessment Detail
            DeleteBlock( "C27D517E-128B-4A1B-B069-C367C7B59AAD" ); // Competency Person Project Assessment List
            DeleteBlock( "52B23A41-93BE-4A65-BF58-C98ED89E54B7" ); // Resident Grade Detail
            DeleteBlock( "142BDAEE-51CF-4DDF-AD74-E31B0D571E9B" ); // Resident Grade Request
            DeleteBlock( "402DB31D-7C84-4154-890E-D18AEE5FC0E2" ); // Resident Project Detail
            DeleteBlock( "C482389C-6CAC-4EB9-9C56-5FFCC0D17639" ); // Resident Project Point Of Assessment List
            DeleteBlock( "0BC0C139-26FB-403C-A5ED-BAA1CC1231FD" ); // Resident Competency Detail
            DeleteBlock( "EE97ABE8-A124-4437-B962-805C1D0C18D4" ); // Resident Competency List
            DeleteBlock( "D1AEB343-41A8-4501-9475-40103C7AEA0A" ); // Marketing Campaign Ads Xslt
            DeleteBlockType( "20CC5FAC-8EF5-4A9E-90E0-ECC8F746F7F9" ); // com _ccvonline - Residency - Resident Competency Project List
            DeleteBlockType( "4BD8E3F7-30D3-49C2-B3D6-B897174A9AB8" ); // com _ccvonline - Residency - Resident Competency Goals Detail
            DeleteBlockType( "BF27EBA9-EA1D-4B15-8B43-D97F0C1B0B20" ); // com _ccvonline - Residency - Resident Project Assessment List
            DeleteBlockType( "D2835421-1D69-4D2E-80BC-836FF606ADDD" ); // com _ccvonline - Residency - Resident Project Assessment Detail
            DeleteBlockType( "DE2FCBEA-F103-43BC-87E4-4235FA361B87" ); // com _ccvonline - Residency - Competency Person Project Assessment Point Of Assessment List
            DeleteBlockType( "B2F1D26F-0C4F-46F1-91A4-ACBBB50E4202" ); // com _ccvonline - Residency - Competency Person Project Assessment Point Of Assessment Detail
            DeleteBlockType( "21F10ADF-5C9F-45AE-AB04-BBC3866A8FE4" ); // com _ccvonline - Residency - Competency Person Project Assessment List
            DeleteBlockType( "470EB28D-75A7-46C6-BB74-525A66BD114E" ); // com _ccvonline - Residency - Competency Person Project Assessment Detail
            DeleteBlockType( "ABDDD216-80B8-427D-8689-0CF84C9C5646" ); // com _ccvonline - Residency - Resident Grade Detail
            DeleteBlockType( "1AD0421D-8B24-4A5A-842F-AF37EACBE35E" ); // com _ccvonline - Residency - Resident Grade Request
            DeleteBlockType( "13A42E92-8D1A-407D-B2D1-2472BBC27D13" ); // com _ccvonline - Residency - Resident Project Detail
            DeleteBlockType( "BC3048EE-6964-4ABB-B710-4616136045BA" ); // com _ccvonline - Residency - Resident Project Point Of Assessment List
            DeleteBlockType( "2D404077-7723-4528-A893-800658BAEA4F" ); // com _ccvonline - Residency - Resident Competency List
            DeleteBlockType( "536A0B29-B427-434D-82B6-C5CE6A8E07FE" ); // com _ccvonline - Residency - Resident Competency Detail
            DeleteBlockType( "D806B597-32C4-4151-8860-007AD1C37211" ); // com _ccvonline - Command Center - Recording List
            DeleteBlockType( "76DD4EF1-1DD8-44C3-B664-F96311AA0ADC" ); // com _ccvonline - Command Center - Recording Detail
            DeletePage( "130FA92D-9D5F-45D1-84AA-B399F2E868E6" ); // Resident Competency Detail - Notes
            DeletePage( "83DBB422-38C5-44F3-9FDE-3737AC8CF2A7" ); // Resident Competency Detail - Goals
            DeletePage( "0DF59029-C17B-474D-8DD1-ED312B734202" ); // Resident Project Assessment Detail
            DeletePage( "4827C8D3-B0FA-4AA4-891F-1F27C7D76606" ); // Point of Assessment
            DeletePage( "A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4" ); // Project Assessment
            DeletePage( "A16C4B0F-66C6-4CF0-8B54-B232DDF553B9" ); // Resident Grade Project Detail
            DeletePage( "5D729D30-8E33-4913-A56F-98F803479C6D" ); // Resident Grade Request
            DeletePage( "56F3E462-28EF-4EC5-A58C-C5FDE48356E0" ); // Resident Project Detail
            DeletePage( "ADE663B9-386B-479C-ABD9-3349E1B4B827" ); // Resident Competency Detail - Projects
            DeletePage( "826C0BFF-C831-4427-98F9-57FF462D82F5" ); // Resident Home
            DeletePage( "F98B0061-8327-4B96-8A5E-B3C58D899B31" ); // Resident
        }
    }
}
