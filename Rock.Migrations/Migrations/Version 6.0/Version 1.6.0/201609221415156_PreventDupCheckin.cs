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
    public partial class PreventDupCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Page: Ability Select
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "C0414290-0F05-4587-9BF8-9EB862FE3143" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "E659796B-9C56-4668-B1AD-C1C9CDFAFF73" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "B6F5457A-72CC-4288-9291-5046CFFC04B6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page (Family Check-in)", "FamilyPreviousPage", "", "The page to navigate back to if none of the people and schedules have been processed.", 8, @"", "3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B" );
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "3D24A4D2-90AF-4FDD-8CE2-7D1F9B76104B", @"67bd09b0-0c6e-44e7-a8eb-0e71551f3e6b" ); // Previous Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "C0414290-0F05-4587-9BF8-9EB862FE3143", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" ); // Multi-Person First Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "E659796B-9C56-4668-B1AD-C1C9CDFAFF73", @"043bb717-5799-446f-b8da-30e575110b0c" ); // Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "B6F5457A-72CC-4288-9291-5046CFFC04B6", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Multi-Person Done Page (Family Check-in)

            // Page: Group Type Select
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "52050592-25DF-4651-8B58-4DD7581F78A3" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "D14CCB80-4155-4F64-92C7-14DDA4C53FC3" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "E1E51DE0-7492-493B-8EBF-311DFD4925F6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Select All and Skip", "SelectAll", "", "Select this option if end-user should never see screen to select group types, all group types will automatically be selected and all the groups in all types will be available.", 8, @"False", "41AFF704-87A8-4282-80D0-B7C40983B549" );
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "52050592-25DF-4651-8B58-4DD7581F78A3", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" ); // Multi-Person First Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "D14CCB80-4155-4F64-92C7-14DDA4C53FC3", @"043bb717-5799-446f-b8da-30e575110b0c" ); // Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "E1E51DE0-7492-493B-8EBF-311DFD4925F6", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "41AFF704-87A8-4282-80D0-B7C40983B549", @"False" ); // Select All and Skip

            // Page: Group Select
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "B3FA9F93-1338-4C38-A700-36AE29884C49" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "E4F7DEB6-4A3F-480C-B4E8-90F120E5804E" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "1A43B624-1FF9-44A8-BBB3-B6073A3C9688" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "B3FA9F93-1338-4C38-A700-36AE29884C49", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" ); // Multi-Person First Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7DEB6-4A3F-480C-B4E8-90F120E5804E", @"043bb717-5799-446f-b8da-30e575110b0c" ); // Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "1A43B624-1FF9-44A8-BBB3-B6073A3C9688", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Multi-Person Done Page (Family Check-in)

            // Page: Location Select
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "14F75C51-6176-4DBF-B1FC-6517E62E310F" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "CEE736CA-7F05-4480-B34B-2A4A743F556C" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "8EB048AF-3A8B-4D55-8045-861B9AE7DF4C" );
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "14F75C51-6176-4DBF-B1FC-6517E62E310F", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" ); // Multi-Person First Page (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "CEE736CA-7F05-4480-B34B-2A4A743F556C", @"043bb717-5799-446f-b8da-30e575110b0c" ); // Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "8EB048AF-3A8B-4D55-8045-861B9AE7DF4C", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Multi-Person Done Page (Family Check-in)

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterByPreviousCheckin", "5151FB64-35C6-48B3-ACCF-959BAD3A31CA", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5151FB64-35C6-48B3-ACCF-959BAD3A31CA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "759890BC-7990-4024-BD82-DF4C3623C3AC" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "5151FB64-35C6-48B3-ACCF-959BAD3A31CA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C134C40A-D372-44E2-A359-2268977B87C9" );

            RockMigrationHelper.UpdateWorkflowActionType( "5D86DC3F-D56A-49D7-B6CC-5ED2B7E59A93", "Remove Previous Checkins", 5, "5151FB64-35C6-48B3-ACCF-959BAD3A31CA", true, false, "", "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD", 1, "False", "A05C4258-BFD6-4810-9EBB-BC6EF1C74F11" );
            RockMigrationHelper.AddActionTypeAttributeValue( "A05C4258-BFD6-4810-9EBB-BC6EF1C74F11", "C134C40A-D372-44E2-A359-2268977B87C9", @"" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A05C4258-BFD6-4810-9EBB-BC6EF1C74F11", "759890BC-7990-4024-BD82-DF4C3623C3AC", @"False" ); // Unattended Check-in:Schedule Select:Filter Locations By Schedule:Active
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
