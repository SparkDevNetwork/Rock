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
    public partial class StepPagesAndBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Steps", "", "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program", "", "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Type", "", "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Steps", "", "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step", "", "2109228C-D828-4B58-9310-8D93D10B846E", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step", "", "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "F5E8A369-4856-42E5-B187-276DFCEB1F3F", "Steps", "4E4280B8-0A10-401A-9D69-687CA66A7B76" );// for Page:Steps
            RockMigrationHelper.AddPageRoute( "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5", "Steps/Program/{ProgramId}", "0B796F9D-1294-40E7-B264-D460D62B4F2F" );// for Page:Program
            RockMigrationHelper.AddPageRoute( "8E78F9DC-657D-41BF-BE0F-56916B6BF92F", "Steps/Type/{StepTypeId}", "74DF0B98-B980-4EF7-B879-7A028535C3FA" );// for Page:Type
            RockMigrationHelper.AddPageRoute( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "Person/{PersonId}/Steps", "181A8246-0F80-44BE-A448-DADF680E6F73" );// for Page:Steps
            RockMigrationHelper.AddPageRoute( "2109228C-D828-4B58-9310-8D93D10B846E", "Steps/Record/{StepId}", "C72F337F-4320-4CED-B5FF-20A443268123" );// for Page:Step
            RockMigrationHelper.AddPageRoute( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "Person/{PersonId}/Steps/{StepTypeId}/{StepId}", "6BA3B394-C827-4548-94AE-CA9AD585CF3A" );// for Page:Step
            RockMigrationHelper.UpdateBlockType( "Steps", "Displays step records for a person in a step program.", "~/Blocks/Steps/PersonProgramStepList.ascx", "Steps", "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" );
            RockMigrationHelper.UpdateBlockType( "Step Entry", "Displays a form to add or edit a step.", "~/Blocks/Steps/StepEntry.ascx", "Steps", "8D78BC55-6E67-40AB-B453-994D69503838" );
            RockMigrationHelper.UpdateBlockType( "Step Participant List", "Lists all the participants in a Step.", "~/Blocks/Steps/StepParticipantList.ascx", "Steps", "2E4A1578-145E-4052-9B56-1739F7366827" );
            RockMigrationHelper.UpdateBlockType( "Step Program Detail", "Displays the details of the given Step Program for editing.", "~/Blocks/Steps/StepProgramDetail.ascx", "Steps", "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" );
            RockMigrationHelper.UpdateBlockType( "Step Program List", "Shows a list of all step programs.", "~/Blocks/Steps/StepProgramList.ascx", "Steps", "429A817E-1379-4BCC-AEFE-01D9C75273E5" );
            RockMigrationHelper.UpdateBlockType( "Step Type Detail", "Displays the details of the given Step Type for editing.", "~/Blocks/Steps/StepTypeDetail.ascx", "Steps", "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD" );
            RockMigrationHelper.UpdateBlockType( "Step Type List", "Shows a list of all step types for a program.", "~/Blocks/Steps/StepTypeList.ascx", "Steps", "3EFB4302-9AB4-420F-A818-48B1B06AD109" );
            // Add Block to Page: Steps Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F5E8A369-4856-42E5-B187-276DFCEB1F3F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "429A817E-1379-4BCC-AEFE-01D9C75273E5".AsGuid(), "Step Program List", "Main", @"", @"", 0, "6AD9C580-387D-413F-9791-EB0DF4D382FC" );
            // Add Block to Page: Program Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CF372F6E-7131-4FF7-8BCD-6053DBB67D34".AsGuid(), "Step Program Detail", "Main", @"", @"", 0, "84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93" );
            // Add Block to Page: Program Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3EFB4302-9AB4-420F-A818-48B1B06AD109".AsGuid(), "Step Type List", "Main", @"", @"", 1, "B7DFAB79-858E-4D44-BD74-38B273BA1EBB" );
            // Add Block to Page: Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E4A1578-145E-4052-9B56-1739F7366827".AsGuid(), "Step Participant List", "Main", @"", @"", 1, "9F149FB6-95BA-4B4F-B98B-20A29892D03B" );
            // Add Block to Page: Type Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8E78F9DC-657D-41BF-BE0F-56916B6BF92F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD".AsGuid(), "Step Type Detail", "Main", @"", @"", 0, "B572C7D7-3989-4BEA-AC63-3447B5CF7ED8" );
            // Add Block to Page: Steps Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CB9ABA3B-6962-4A42-BDA1-EA71B7309232".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4".AsGuid(), "Steps", "SectionC1", @"", @"", 0, "46E5C15A-44A5-4FB3-8CE8-572FB0D85367" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2109228C-D828-4B58-9310-8D93D10B846E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D78BC55-6E67-40AB-B453-994D69503838".AsGuid(), "Step Entry", "Main", @"", @"", 0, "74E22668-FE00-4238-AC40-6A2DACD48F56" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D78BC55-6E67-40AB-B453-994D69503838".AsGuid(), "Step Entry", "Main", @"", @"", 0, "826E3498-AEC5-45DF-A454-F3AD19573714" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93'" );  // Page: Program,  Zone: Main,  Block: Step Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'B572C7D7-3989-4BEA-AC63-3447B5CF7ED8'" );  // Page: Type,  Zone: Main,  Block: Step Type Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '9F149FB6-95BA-4B4F-B98B-20A29892D03B'" );  // Page: Type,  Zone: Main,  Block: Step Participant List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B7DFAB79-858E-4D44-BD74-38B273BA1EBB'" );  // Page: Program,  Zone: Main,  Block: Step Type List
            // Attrib for BlockType: Step Participant List:Show Note Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Note Column", "ShowNoteColumn", "", @"Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?", 3, @"False", "FDEAB18C-637C-4B7F-A742-437AAB53C0C4" );
            // Attrib for BlockType: Step Program Detail:Show Chart
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "Show Chart", "", @"", 0, @"true", "ECF644EF-C74F-4182-82EB-56BFC9C63630" );
            // Attrib for BlockType: Step Type Detail:Show Chart
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "ShowChart", "", @"", 0, @"true", "793E9E60-B1DA-4D76-A7B5-8C933E87B574" );
            // Attrib for BlockType: Steps:Steps Per Row
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Steps Per Row", "StepsPerRow", "", @"The number of step cards that should be shown on a row", 3, @"6", "FBAB162B-556B-44DA-B830-D629529C0542" );
            // Attrib for BlockType: Steps:Steps Per Row Mobile
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Steps Per Row Mobile", "StepsPerRowMobile", "", @"The number of step cards that should be shown on a row on a mobile screen size", 4, @"2", "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75" );
            // Attrib for BlockType: Step Entry:Step Type Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "8D78BC55-6E67-40AB-B453-994D69503838", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Step Type Id", "StepType", "", @"The step type to use to add a new step. Leave blank to use the query string: StepTypeId. The type of the step, if step id is specified, overrides this setting.", 1, @"", "BB5B26FD-1056-4C2E-85F2-A87F3FA45D68" );
            // Attrib for BlockType: Steps:Step Entry Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Step Entry Page", "StepPage", "", @"The page where step records can be edited or added", 2, @"", "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500" );
            // Attrib for BlockType: Step Entry:Success Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "8D78BC55-6E67-40AB-B453-994D69503838", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Success Page", "SuccessPage", "", @"The page to navigate to once the add or edit has completed. Leave blank to navigate to the parent page.", 2, @"", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB" );
            // Attrib for BlockType: Step Participant List:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", @"Page used for viewing a person's profile. If set a view profile button will show for each participant.", 2, @"", "D9ECF014-7936-4D87-809D-56303AB15C7D" );
            // Attrib for BlockType: Step Participant List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 1, @"", "2D657072-4288-4AC4-98AE-79833C84AB11" );
            // Attrib for BlockType: Step Program List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 2, @"", "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6" );
            // Attrib for BlockType: Step Type List:Bulk Entry
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Bulk Entry", "BulkEntryPage", "", @"Linked page that allows for bulk entry of steps for a step type.", 3, @"", "A5791226-ABCC-4BBA-BDE9-CE605B8AC2DD" );
            // Attrib for BlockType: Step Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 2, @"", "09478006-E619-4210-9A5B-8442576259B5" );
            // Attrib for BlockType: Step Program Detail:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "Chart Style", "", @"", 1, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "6910B0DD-3F05-42F9-9183-928E076A82F3" );
            // Attrib for BlockType: Step Type Detail:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", @"", 1, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "7B950595-6CDF-4A6C-A418-974849F3FC2D" );
            // Attrib for BlockType: Step Program List:Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Categories", "Categories", "", @"If block should only display Step Programs from specific categories, select the categories here.", 1, @"", "CF2E1C24-EEE8-4375-9282-257A3B72A996" );
            // Attrib for BlockType: Step Type Detail:Data View Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Data View Categories", "DataViewCategories", "", @"The categories from which the Audience and Autocomplete data view options can be selected. If empty, all data views will be available.", 7, @"", "81BAADD8-F855-495C-BC07-FF97CE7AB8DF" );
            // Attrib for BlockType: Step Program Detail:Default Chart Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "", @"", 2, @"Current||Year||", "6DF66E4A-2F7E-4A7C-9A90-B12EC711D4A2" );
            // Attrib for BlockType: Step Type Detail:Default Chart Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Chart Date Range", "SlidingDateRange", "", @"", 2, @"Current||Year||", "ACBEFEFC-9605-4645-90A9-0E50506D19C8" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "StepProgram", "", @"The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.", 1, @"", "625E9A05-AF25-4886-9961-4F00263EBC82" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "Programs", "", @"Display Step Types from a specified program. If none selected, the block will display the program from the current context.", 1, @"", "3728C201-1D50-456C-8CF2-29A380558F67" );
            // Attrib Value for Block:Step Program List, Attribute:Detail Page Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6AD9C580-387D-413F-9791-EB0DF4D382FC", "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6", @"6e46bc35-1fcb-4619-84f0-bb6926d2ddd5,0b796f9d-1294-40e7-b264-d460d62b4f2f" );
            // Attrib Value for Block:Step Type List, Attribute:Detail Page Page: Program, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B7DFAB79-858E-4D44-BD74-38B273BA1EBB", "09478006-E619-4210-9A5B-8442576259B5", @"8e78f9dc-657d-41bf-be0f-56916b6bf92f,74df0b98-b980-4ef7-b879-7a028535c3fa" );
            // Attrib Value for Block:Step Participant List, Attribute:Person Profile Page Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "D9ECF014-7936-4D87-809D-56303AB15C7D", @"cb9aba3b-6962-4a42-bda1-ea71b7309232,181a8246-0f80-44be-a448-dadf680e6f73" );
            // Attrib Value for Block:Step Participant List, Attribute:Detail Page Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "2D657072-4288-4AC4-98AE-79833C84AB11", @"2109228c-d828-4b58-9310-8d93d10b846e,c72f337f-4320-4ced-b5ff-20a443268123" );
            // Attrib Value for Block:Step Participant List, Attribute:Show Note Column Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "FDEAB18C-637C-4B7F-A742-437AAB53C0C4", @"False" );
            // Attrib Value for Block:Step Participant List, Attribute:core.CustomGridEnableStickyHeaders Page: Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9F149FB6-95BA-4B4F-B98B-20A29892D03B", "9120E7A5-658B-4EA5-8517-A072ED5B0C20", @"False" );
            // Attrib Value for Block:Steps, Attribute:Steps Per Row Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "FBAB162B-556B-44DA-B830-D629529C0542", @"6" );
            // Attrib Value for Block:Steps, Attribute:Steps Per Row Mobile Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75", @"2" );
            // Attrib Value for Block:Steps, Attribute:Step Entry Page Page: Steps, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367", "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500", @"7a04966a-8e4e-49ea-a03c-7dd4b52a7b28,6ba3b394-c827-4548-94ae-ca9ad585cf3a" );
            // Attrib Value for Block:Step Entry, Attribute:Success Page Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "74E22668-FE00-4238-AC40-6A2DACD48F56", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB", @"8e78f9dc-657d-41bf-be0f-56916b6bf92f,74df0b98-b980-4ef7-b879-7a028535c3fa" );
            // Attrib Value for Block:Step Entry, Attribute:Success Page Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "826E3498-AEC5-45DF-A454-F3AD19573714", "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB", @"cb9aba3b-6962-4a42-bda1-ea71b7309232,181a8246-0f80-44be-a448-dadf680e6f73" );
            RockMigrationHelper.UpdateFieldType( "Registry Entry", "", "Rock", "Rock.Field.Types.RegistryEntryFieldType", "D98E1D88-2240-4248-B93B-0512BD3BB61A" );
            RockMigrationHelper.UpdateFieldType( "Step Program", "", "Rock", "Rock.Field.Types.StepProgramFieldType", "33875369-7D2B-4CD7-BB89-ABC29906CCAE" );
            // Add/Update PageContext for Page:Steps, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232", "Rock.Model.Person", "PersonId", "A14075A7-8A09-424D-AC71-D81535194EB7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Step Type List:Detail Page
            RockMigrationHelper.DeleteAttribute( "09478006-E619-4210-9A5B-8442576259B5" );
            // Attrib for BlockType: Step Type List:Bulk Entry
            RockMigrationHelper.DeleteAttribute( "A5791226-ABCC-4BBA-BDE9-CE605B8AC2DD" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.DeleteAttribute( "3728C201-1D50-456C-8CF2-29A380558F67" );
            // Attrib for BlockType: Step Type Detail:Chart Style
            RockMigrationHelper.DeleteAttribute( "7B950595-6CDF-4A6C-A418-974849F3FC2D" );
            // Attrib for BlockType: Step Type Detail:Show Chart
            RockMigrationHelper.DeleteAttribute( "793E9E60-B1DA-4D76-A7B5-8C933E87B574" );
            // Attrib for BlockType: Step Type Detail:Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "ACBEFEFC-9605-4645-90A9-0E50506D19C8" );
            // Attrib for BlockType: Step Type Detail:Data View Categories
            RockMigrationHelper.DeleteAttribute( "81BAADD8-F855-495C-BC07-FF97CE7AB8DF" );
            // Attrib for BlockType: Step Program List:Detail Page
            RockMigrationHelper.DeleteAttribute( "7CDD9D31-BD0F-4600-9BC7-5C4B530750B6" );
            // Attrib for BlockType: Step Program List:Categories
            RockMigrationHelper.DeleteAttribute( "CF2E1C24-EEE8-4375-9282-257A3B72A996" );
            // Attrib for BlockType: Step Program Detail:Default Chart Date Range
            RockMigrationHelper.DeleteAttribute( "6DF66E4A-2F7E-4A7C-9A90-B12EC711D4A2" );
            // Attrib for BlockType: Step Program Detail:Chart Style
            RockMigrationHelper.DeleteAttribute( "6910B0DD-3F05-42F9-9183-928E076A82F3" );
            // Attrib for BlockType: Step Program Detail:Show Chart
            RockMigrationHelper.DeleteAttribute( "ECF644EF-C74F-4182-82EB-56BFC9C63630" );
            // Attrib for BlockType: Step Participant List:Show Note Column
            RockMigrationHelper.DeleteAttribute( "FDEAB18C-637C-4B7F-A742-437AAB53C0C4" );
            // Attrib for BlockType: Step Participant List:Detail Page
            RockMigrationHelper.DeleteAttribute( "2D657072-4288-4AC4-98AE-79833C84AB11" );
            // Attrib for BlockType: Step Participant List:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "D9ECF014-7936-4D87-809D-56303AB15C7D" );
            // Attrib for BlockType: Step Entry:Success Page
            RockMigrationHelper.DeleteAttribute( "1BC93D94-6AB0-438B-B2EB-F3A3681DABEB" );
            // Attrib for BlockType: Step Entry:Step Type Id
            RockMigrationHelper.DeleteAttribute( "BB5B26FD-1056-4C2E-85F2-A87F3FA45D68" );
            // Attrib for BlockType: Steps:Step Entry Page
            RockMigrationHelper.DeleteAttribute( "B6AF94FF-6D7F-4FFC-8DA7-A78519EF7500" );
            // Attrib for BlockType: Steps:Steps Per Row Mobile
            RockMigrationHelper.DeleteAttribute( "9E4F6CB9-0228-4D37-BDED-A33FD96EBC75" );
            // Attrib for BlockType: Steps:Steps Per Row
            RockMigrationHelper.DeleteAttribute( "FBAB162B-556B-44DA-B830-D629529C0542" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.DeleteAttribute( "625E9A05-AF25-4886-9961-4F00263EBC82" );
            // Remove Block: Step Entry, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "826E3498-AEC5-45DF-A454-F3AD19573714" );
            // Remove Block: Step Entry, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "74E22668-FE00-4238-AC40-6A2DACD48F56" );
            // Remove Block: Steps, from Page: Steps, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "46E5C15A-44A5-4FB3-8CE8-572FB0D85367" );
            // Remove Block: Step Participant List, from Page: Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9F149FB6-95BA-4B4F-B98B-20A29892D03B" );
            // Remove Block: Step Type Detail, from Page: Type, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B572C7D7-3989-4BEA-AC63-3447B5CF7ED8" );
            // Remove Block: Step Type List, from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B7DFAB79-858E-4D44-BD74-38B273BA1EBB" );
            // Remove Block: Step Program Detail, from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "84AF01F1-6904-4B89-9CE5-FDE4B1C5EA93" );
            // Remove Block: Step Program List, from Page: Steps, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6AD9C580-387D-413F-9791-EB0DF4D382FC" );
            RockMigrationHelper.DeleteBlockType( "3EFB4302-9AB4-420F-A818-48B1B06AD109" ); // Step Type List
            RockMigrationHelper.DeleteBlockType( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD" ); // Step Type Detail
            RockMigrationHelper.DeleteBlockType( "429A817E-1379-4BCC-AEFE-01D9C75273E5" ); // Step Program List
            RockMigrationHelper.DeleteBlockType( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" ); // Step Program Detail
            RockMigrationHelper.DeleteBlockType( "2E4A1578-145E-4052-9B56-1739F7366827" ); // Step Participant List
            RockMigrationHelper.DeleteBlockType( "8D78BC55-6E67-40AB-B453-994D69503838" ); // Step Entry
            RockMigrationHelper.DeleteBlockType( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4" ); // Steps
            RockMigrationHelper.DeletePage( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28" ); //  Page: Step, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "2109228C-D828-4B58-9310-8D93D10B846E" ); //  Page: Step, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "CB9ABA3B-6962-4A42-BDA1-EA71B7309232" ); //  Page: Steps, Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.DeletePage( "8E78F9DC-657D-41BF-BE0F-56916B6BF92F" ); //  Page: Type, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6E46BC35-1FCB-4619-84F0-BB6926D2DDD5" ); //  Page: Program, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F5E8A369-4856-42E5-B187-276DFCEB1F3F" ); //  Page: Steps, Layout: Full Width, Site: Rock RMS
            // Delete PageContext for Page:Steps, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "A14075A7-8A09-424D-AC71-D81535194EB7" );
        }
    }
}
