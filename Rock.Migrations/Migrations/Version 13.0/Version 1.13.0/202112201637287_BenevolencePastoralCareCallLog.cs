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
    /// <summary>
    ///
    /// </summary>
    public partial class BenevolencePastoralCareCallLog : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add the ShowFinancialResults to the BenevolenceType table
            AddColumn( "dbo.BenevolenceType", "ShowFinancialResults", c => c.Boolean( nullable: false, defaultValue: true ) );

            // Add Page - Internal Name: Benevolence Types - Site: Rock RMS
            RockMigrationHelper.AddPage( true, "D893CCCC-368A-42CF-B36E-69991128F016", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence Types", string.Empty, "C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", string.Empty );

            // Add Page - Internal Name: Benevolence Type Detail - Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence Type Detail", string.Empty, "DBFC432E-F0A4-457E-BA5B-572C49B899D1", string.Empty );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route - Page:Benevolence Types - Route:finance/benevolence/types
            RockMigrationHelper.AddPageRoute( "C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", "finance/benevolence/types", "E18502CE-2F60-4A7D-8DC9-9D83A6762C29" );

            // Add Page Route - Page:Benevolence Request Detail Route:finance/benevolence/{BenevolenceRequestId}
            RockMigrationHelper.AddPageRoute( "6DC7BAED-CA01-4703-B679-EC81143CDEDD", "finance/benevolence/{BenevolenceRequestId}", "16060BFA-ED0A-41B8-9CD0-56351DDD207C" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update BlockType - Name: Benevolence Type List - Category: Finance - Path: ~/Blocks/Finance/BenevolenceTypeList.ascx
            RockMigrationHelper.UpdateBlockType( "Benevolence Type List", "Block to display the benevolence types.", "~/Blocks/Finance/BenevolenceTypeList.ascx", "Finance", "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A" );

            // Add/Update BlockType - Name: Benevolence Type Detail - Category: Finance - Path: ~/Blocks/Finance/BenevolenceTypeDetail.ascx
            RockMigrationHelper.UpdateBlockType( "Benevolence Type Detail", "Block to display the benevolence type detail.", "~/Blocks/Finance/BenevolenceTypeDetail.ascx", "Finance", "C96479B6-E309-4B1A-B024-1F1276122A13" );

            // Add Block - Block Name: Benevolence Type List - Page Name: Benevolence Types - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A".AsGuid(), "Benevolence Type List", "Main", string.Empty, string.Empty, 0, "9D91C762-943A-444F-83D8-5CE38205030A" );

            // Add Block - Block Name: Benevolence Type Detail - Page Name: Benevolence Type Detail - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DBFC432E-F0A4-457E-BA5B-572C49B899D1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C96479B6-E309-4B1A-B024-1F1276122A13".AsGuid(), "Benevolence Type Detail", "Main", string.Empty, string.Empty, 0, "D5BB2522-35A8-478B-BEA3-D47697E61DF7" );

            // Add Block - Block Name: Notes - Page Name: Benevolence Request Detail - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6DC7BAED-CA01-4703-B679-EC81143CDEDD".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3".AsGuid(), "Notes", "SectionA", string.Empty, string.Empty, 0, "AAC4B265-1E94-4F27-9278-B131F7C8B717" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsAttributeKey", "Hide Columns on Grid", @"The grid columns that should be hidden.", 3, string.Empty, "2FAF20BB-2A1C-43B0-B8AE-D8AC412D5FE0" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Include Benevolence Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Include Benevolence Types", "FilterBenevolenceTypesAttributeKey", "Include Benevolence Types", @"The benevolence types to display in the list.<br/><i>If none are selected, all types will be included.<i>", 4, string.Empty, "C597B478-3207-4049-BB12-ED32C362568B" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create benevolence type.", 2, @"C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", "E50A7441-4851-4F29-8658-B5AE45F15B95" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "BenevolenceRequestDetail", "Detail Page", @"Page used to modify and create benevolence requests.", 1, @"6DC7BAED-CA01-4703-B679-EC81143CDEDD", "5D2059B6-30C2-4DF9-9C6B-94238FD34E79" );

            // Attribute for BlockType - BlockType: Benevolence Type List - Category: Finance - Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page used to view details of a benevolence type.", 0, string.Empty, "E0FF18D0-B605-4219-BE06-9C5A832AC424" );

            // Attribute for BlockType - BlockType: Benevolence Type Detail - Category: Finance - Attribute: Benevolence Type Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C96479B6-E309-4B1A-B024-1F1276122A13", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Benevolence Type Attributes", "BenevolenceTypeAttributes", "Benevolence Type Attributes", @"The attributes that should be displayed / edited for benevolence types.", 1, string.Empty, "BFBFE4CB-7426-4E24-A8CC-A2F2CBC1B995" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Case Worker Role
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9398AD06-E405-491D-B2CE-C4107A4108C6", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Case Worker Role", "CaseWorkerRole", "Case Worker Role", @"The security role to draw case workers from", 1, string.Empty, "EFB0E76C-7282-4966-9E5A-C807413A7725" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Country Code
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9398AD06-E405-491D-B2CE-C4107A4108C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Country Code", "DisplayCountryCode", "Display Country Code", @"When enabled prepends the country code to all phone numbers.", 2, @"False", "0CBBCE97-8415-4A04-A708-8D574978D55A" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Government Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9398AD06-E405-491D-B2CE-C4107A4108C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Government Id", "DisplayGovernmentId", "Display Government Id", @"Display the government identifier.", 3, @"True", "1AE729D4-B5BF-4C78-8382-7D55AE19F745" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Middle Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9398AD06-E405-491D-B2CE-C4107A4108C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Middle Name", "DisplayMiddleName", "Display Middle Name", @"Display the middle name of the person.", 4, @"False", "645E1B73-A988-4BF5-B410-5591025586FD" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Benevolence Request Statement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9398AD06-E405-491D-B2CE-C4107A4108C6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Benevolence Request Statement Page", "BenevolenceRequestStatementPage", "Benevolence Request Statement Page", @"The page which summarizes a benevolence request for printing", 5, string.Empty, "52BD8FF7-9E1F-432B-9066-0C729EF4D1EA" );

            // Add Block Attribute Value - Block: Benevolence Request List - BlockType: Benevolence Request List - Category: Finance - Block Location: Page=Benevolence, Site=Rock RMS - Attribute: core.CustomGridEnableStickyHeaders /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "8EF23024-A24D-41BD-B12E-5AB06F75D826", @"False" );

            // Add Block Attribute Value - Block: Benevolence Request List - BlockType: Benevolence Request List - Category: Finance - Block Location: Page=Benevolence, Site=Rock RMS - Attribute: Include Benevolence Types /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "C597B478-3207-4049-BB12-ED32C362568B", string.Empty );

            // Add Block Attribute Value - Block: Benevolence Request List - BlockType: Benevolence Request List - Category: Finance - Block Location: Page=Benevolence, Site=Rock RMS - Attribute: Configuration Page /*   Attribute Value: c6be9cf1-ffe9-4dc1-8472-865fd93b89a8,e18502ce-2f60-4a7d-8dc9-9d83a6762c29 */
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "E50A7441-4851-4F29-8658-B5AE45F15B95", @"c6be9cf1-ffe9-4dc1-8472-865fd93b89a8,e18502ce-2f60-4a7d-8dc9-9d83a6762c29" );

            // Add Block Attribute Value - Block: Benevolence Request List - BlockType: Benevolence Request List - Category: Finance - Block Location: Page=Benevolence, Site=Rock RMS - Attribute: Detail Page /*   Attribute Value: 6DC7BAED-CA01-4703-B679-EC81143CDEDD */
            RockMigrationHelper.AddBlockAttributeValue( "76519A99-2E29-4481-95B8-DCFF8E3225A1", "5D2059B6-30C2-4DF9-9C6B-94238FD34E79", @"6DC7BAED-CA01-4703-B679-EC81143CDEDD" );

            // Add Block Attribute Value - Block: Benevolence Type List - BlockType: Benevolence Type List - Category: Finance - Block Location: Page=Benevolence Types, Site=Rock RMS - Attribute: Detail Page /*   Attribute Value: dbfc432e-f0a4-457e-ba5b-572c49b899d1 */
            RockMigrationHelper.AddBlockAttributeValue( "9D91C762-943A-444F-83D8-5CE38205030A", "E0FF18D0-B605-4219-BE06-9C5A832AC424", @"dbfc432e-f0a4-457e-ba5b-572c49b899d1" );

            // Add Block Attribute Value - Block: Benevolence Type Detail - BlockType: Benevolence Type Detail - Category: Finance - Block Location: Page=Benevolence Type Detail, Site=Rock RMS - Attribute: Benevolence Type Attributes /*   Attribute Value: 350ca625-ed4f-4d96-a979-ee44cd682e71 */
            RockMigrationHelper.AddBlockAttributeValue( "D5BB2522-35A8-478B-BEA3-D47697E61DF7", "BFBFE4CB-7426-4E24-A8CC-A2F2CBC1B995", @"350ca625-ed4f-4d96-a979-ee44cd682e71" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Entity Type /*   Attribute Value: cf0ce5c1-9286-4310-9b50-10d040f8ebd2 */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"cf0ce5c1-9286-4310-9b50-10d040f8ebd2" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Show Private Checkbox /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"False" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Show Security Button /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Show Alert Checkbox /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "20243A98-4802-48E2-AF61-83956056AC65", @"False" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Heading /*   Attribute Value: Notes */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Heading Icon CSS Class /*   Attribute Value: fa fa-heart-o */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-heart-o" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Note Term /*   Attribute Value: Note */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Type /*   Attribute Value: Full */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Use Person Icon /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"True" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Allow Anonymous /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Add Always Visible /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Order /*   Attribute Value: Descending */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Allow Backdated Notes /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Note Types /*   Attribute Value: 93D54D23-097B-4CC2-98C4-C21FD7F29DD1 */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"93D54D23-097B-4CC2-98C4-C21FD7F29DD1" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Note Type Heading /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"True" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Note View Lava Template /*   Attribute Value: {% include '~~/Assets/Lava/NoteViewList.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );

            // Add Block Attribute Value - Block: Notes - BlockType: Notes - Category: Core - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Expand Replies /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AAC4B265-1E94-4F27-9278-B131F7C8B717", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );

            // Add Block Attribute Value - Block: Benevolence Request Detail - BlockType: Benevolence Request Detail - Category: Finance - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Case Worker Role /*   Attribute Value: 02fa0881-3552-42b8-a519-d021139b800f */
            RockMigrationHelper.AddBlockAttributeValue( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21", "EFB0E76C-7282-4966-9E5A-C807413A7725", @"02fa0881-3552-42b8-a519-d021139b800f" );

            // Add Block Attribute Value - Block: Benevolence Request Detail - BlockType: Benevolence Request Detail - Category: Finance - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Country Code /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21", "0CBBCE97-8415-4A04-A708-8D574978D55A", @"False" );

            // Add Block Attribute Value - Block: Benevolence Request Detail - BlockType: Benevolence Request Detail - Category: Finance - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Government Id /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21", "1AE729D4-B5BF-4C78-8382-7D55AE19F745", @"True" );

            // Add Block Attribute Value - Block: Benevolence Request Detail - BlockType: Benevolence Request Detail - Category: Finance - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Display Middle Name /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21", "645E1B73-A988-4BF5-B410-5591025586FD", @"False" );

            // Add Block Attribute Value - Block: Benevolence Request Detail - BlockType: Benevolence Request Detail - Category: Finance - Block Location: Page=Benevolence Request Detail, Site=Rock RMS - Attribute: Benevolence Request Statement Page /*   Attribute Value: d676a464-29a0-49f1-ba8c-752d9fe21026 */
            RockMigrationHelper.AddBlockAttributeValue( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21", "52BD8FF7-9E1F-432B-9066-0C729EF4D1EA", @"d676a464-29a0-49f1-ba8c-752d9fe21026" );

            // Add/Update PageContext for Page:Benevolence Request Detail, Entity: Rock.Model.BenevolenceRequest, Parameter: BenevolenceRequestId
            RockMigrationHelper.UpdatePageContext( "6DC7BAED-CA01-4703-B679-EC81143CDEDD", "Rock.Model.BenevolenceRequest", "BenevolenceRequestId", "90A4CD6B-47E9-4C2D-B7A2-D63703EAFAFE" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Benevolence Request Statement Page
            RockMigrationHelper.DeleteAttribute( "52BD8FF7-9E1F-432B-9066-0C729EF4D1EA" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Middle Name
            RockMigrationHelper.DeleteAttribute( "645E1B73-A988-4BF5-B410-5591025586FD" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Government Id
            RockMigrationHelper.DeleteAttribute( "1AE729D4-B5BF-4C78-8382-7D55AE19F745" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Display Country Code
            RockMigrationHelper.DeleteAttribute( "0CBBCE97-8415-4A04-A708-8D574978D55A" );

            // Attribute for BlockType - BlockType: Benevolence Request Detail - Category: Finance - Attribute: Case Worker Role
            RockMigrationHelper.DeleteAttribute( "EFB0E76C-7282-4966-9E5A-C807413A7725" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5D2059B6-30C2-4DF9-9C6B-94238FD34E79" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Configuration Page
            RockMigrationHelper.DeleteAttribute( "E50A7441-4851-4F29-8658-B5AE45F15B95" );

            // Attribute for BlockType - BlockType: Benevolence Type List - Category: Finance - Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "E0FF18D0-B605-4219-BE06-9C5A832AC424" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Include Benevolence Types
            RockMigrationHelper.DeleteAttribute( "C597B478-3207-4049-BB12-ED32C362568B" );

            // Attribute for BlockType - BlockType: Benevolence Request List - Category: Finance - Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute( "2FAF20BB-2A1C-43B0-B8AE-D8AC412D5FE0" );

            // Attribute for BlockType - BlockType: Benevolence Type Detail - Category: Finance - Attribute: Benevolence Type Attributes
            RockMigrationHelper.DeleteAttribute( "BFBFE4CB-7426-4E24-A8CC-A2F2CBC1B995" );

            // Remove Block - Name: Benevolence Request Detail, from Page: Benevolence Request Detail, Site: Rock RMS - from Page: Benevolence Request Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A8568DFB-719F-47C1-BAC6-334EA0FE6D21" );

            // Remove Block - Name: Notes, from Page: Benevolence Request Detail, Site: Rock RMS - from Page: Benevolence Request Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AAC4B265-1E94-4F27-9278-B131F7C8B717" );

            // Remove Block - Name: Benevolence Type Detail, from Page: Benevolence Type Detail, Site: Rock RMS - from Page: Benevolence Type Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D5BB2522-35A8-478B-BEA3-D47697E61DF7" );

            // Remove Block - Name: Benevolence Type List, from Page: Benevolence Types, Site: Rock RMS - from Page: Benevolence Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9D91C762-943A-444F-83D8-5CE38205030A" );

            // Delete BlockType  - Name: Benevolence Request Detail - Category: Finance - Path: ~/Blocks/Finance/BenevolenceRequestDetail.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( "9398AD06-E405-491D-B2CE-C4107A4108C6" );

            // Delete BlockType  - Name: Benevolence Type Detail - Category: Finance - Path: ~/Blocks/Finance/BenevolenceTypeDetail.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( "C96479B6-E309-4B1A-B024-1F1276122A13" );

            // Delete BlockType  - Name: Benevolence Type List - Category: Finance - Path: ~/Blocks/Finance/BenevolenceTypeList.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A" );

            // Delete BlockType  - Name: Widget List - Category: Rock Solid Church Demo > Page Debug - Path: - - EntityType: Widgets List
            RockMigrationHelper.DeleteBlockType( "9A5678B3-C197-42D2-A7D1-0114FDE4E2F9" );

            // Delete BlockType  - Name: Context Group - Category: Rock Solid Church Demo > Page Debug - Path: - - EntityType: Context Group
            RockMigrationHelper.DeleteBlockType( "FB5105A1-7954-47CC-A043-05BC5A8FD621" );

            // Delete BlockType  - Name: Context Entities - Category: Rock Solid Church Demo > Page Debug - Path: - - EntityType: Context Entities
            RockMigrationHelper.DeleteBlockType( "4F8319FF-30D9-4210-A0F8-B1E84627D17A" );

            // Delete Page  - Internal Name: Benevolence Type Detail - Site: Rock RMS - Layout: Full Width
            RockMigrationHelper.DeletePage( "DBFC432E-F0A4-457E-BA5B-572C49B899D1" );

            // Delete Page  - Internal Name: Benevolence Types - Site: Rock RMS - Layout: Full Width
            RockMigrationHelper.DeletePage( "C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8" );

            // Delete PageContext for Page:Benevolence Request Detail, Entity: Rock.Model.BenevolenceRequest, Parameter: BenevolenceRequestId
            RockMigrationHelper.DeletePageContext( "90A4CD6B-47E9-4C2D-B7A2-D63703EAFAFE" );

            // Drop the ShowFinancialResults from the BenevolenceType table
            DropColumn( "dbo.BenevolenceType", "ShowFinancialResults");
        }
    }
}
