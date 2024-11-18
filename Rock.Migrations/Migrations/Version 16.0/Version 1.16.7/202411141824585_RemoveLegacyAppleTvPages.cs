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
    public partial class RemoveLegacyAppleTvPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            LegacyAppleTvPagesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            LegacyAppleTvPagesDown();
        }

        /// <summary>
        /// Removes the legacy apple tv pages.
        /// </summary>
        public void LegacyAppleTvPagesUp()
        {
            // Delete Apple TV Application Screen Detail
            RockMigrationHelper.DeletePage( "6CC8D008-8D30-416D-8A36-7D01B72A2518" );

            // Delete Apple TV Page List
            RockMigrationHelper.DeletePage( "ED4341EB-3846-48B4-96D3-444D3ABBF389" );

            // Delete Apple TV Site List
            RockMigrationHelper.DeletePage( "C8B81EBE-E98F-43EF-9E39-0491685145E2" );
        }

        /// <summary>
        /// Adds the legacy apple tv pages.
        /// </summary>
        public void LegacyAppleTvPagesDown()
        {
            // Add Page Apple TV Apps to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "b4a24ab7-9369-4055-883f-4f4892c39ae3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Apple TV Apps", "", "C8B81EBE-E98F-43EF-9E39-0491685145E2", "fa fa-tv" );
            // Add Page Application Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "C8B81EBE-E98F-43EF-9E39-0491685145E2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Application Detail", "", "ED4341EB-3846-48B4-96D3-444D3ABBF389", "" );
            // Add Page Application Screen Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "ED4341EB-3846-48B4-96D3-444D3ABBF389", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Application Screen Detail", "", "6CC8D008-8D30-416D-8A36-7D01B72A2518", "" );

            // Add/Update BlockType Apple TV Application Detail
            RockMigrationHelper.UpdateBlockType( "Apple TV Application Detail", "Allows a person to edit an Apple TV application.", "~/Blocks/Tv/AppleTvAppDetail.ascx", "TV > TV Apps", "49F3D87E-BD8D-43D4-8217-340F3DFF4562" );
            // Add/Update BlockType Apple TV Page Detail
            RockMigrationHelper.UpdateBlockType( "Apple TV Page Detail", "Allows a person to edit an Apple TV page.", "~/Blocks/Tv/AppleTvPageDetail.ascx", "TV > TV Apps", "23CA8858-6D02-48A8-92C4-CE415DAB41B6" );
            // Add/Update BlockType TV Page List
            RockMigrationHelper.UpdateBlockType( "TV Page List", "Lists pages for TV apps (Apple or other).", "~/Blocks/Tv/TvPageList.ascx", "TV > TV Apps", "7BD1B79C-BF27-42C6-8359-F80EC7FEE397" );

            // Add Block Apple TV Application Detail to Page: Application Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "ED4341EB-3846-48B4-96D3-444D3ABBF389".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "49F3D87E-BD8D-43D4-8217-340F3DFF4562".AsGuid(), "Apple TV Application Detail", "Main", @"", @"", 0, "B7A5BBF5-B03F-47F1-A459-258927185EA7" );
            // Add Block TV Page List to Page: Application Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "ED4341EB-3846-48B4-96D3-444D3ABBF389".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7BD1B79C-BF27-42C6-8359-F80EC7FEE397".AsGuid(), "TV Page List", "Main", @"", @"", 1, "57711B0B-C5B0-4F8E-B190-32410EDC2C89" );
            // Add Block Apple TV Page Detail to Page: Application Screen Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6CC8D008-8D30-416D-8A36-7D01B72A2518".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "23CA8858-6D02-48A8-92C4-CE415DAB41B6".AsGuid(), "Apple TV Page Detail", "Main", @"", @"", 0, "F110B685-07EC-49A7-90AB-E02C67AEC68A" );
            // Add Block Site List to Page: Apple TV Apps, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C8B81EBE-E98F-43EF-9E39-0491685145E2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "441D5A71-C250-4FF5-90C3-DEEAD3AC028D".AsGuid(), "Site List", "Main", @"", @"", 0, "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Application Detail,  Zone: Main,  Block: Apple TV Application Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'B7A5BBF5-B03F-47F1-A459-258927185EA7'" );
            // Update Order for Page: Application Detail,  Zone: Main,  Block: TV Page List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '57711B0B-C5B0-4F8E-B190-32410EDC2C89'" );


            // Attribute for BlockType: TV Page List:Page Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BD1B79C-BF27-42C6-8359-F80EC7FEE397", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Page Detail", "PageDetail", "Page Detail", @"", 0, @"", "3BC9944D-35E2-466D-B7CC-9507BE108084" );

            // Add Block Attribute Value            //   Block: TV Page List            //   BlockType: TV Page List            //   Block Location: Page=Application Detail, Site=Rock RMS            //   Attribute: Page Detail            //   Attribute Value: 6cc8d008-8d30-416d-8a36-7d01b72a2518
            RockMigrationHelper.AddBlockAttributeValue( "57711B0B-C5B0-4F8E-B190-32410EDC2C89", "3BC9944D-35E2-466D-B7CC-9507BE108084", @"6cc8d008-8d30-416d-8a36-7d01b72a2518" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: Detail Page            //   Attribute Value: ed4341eb-3846-48b4-96d3-444d3abbf389
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "BE7EEE0F-9B2B-4CF5-8714-29166025B3DD", @"ed4341eb-3846-48b4-96d3-444d3abbf389" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: Site Type            //   Attribute Value: 2
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "786B9AA2-EA35-4C96-BA33-7A6F9945A10E", @"2" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: Block Icon CSS Class            //   Attribute Value: fa fa-tv
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "B1188B79-3BC4-4F11-8165-CA4E5858FADA", @"fa fa-tv" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: Block Title            //   Attribute Value: Apple TV Applications
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "8D88C763-B904-48F1-9FEE-31CC4D5086FD", @"Apple TV Applications" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: Show Delete Column            //   Attribute Value: True
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "72FAC7B5-AA38-491B-83AF-7945D02F6740", @"True" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: core.EnableDefaultWorkflowLauncher            //   Attribute Value: True
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "EB67C256-C509-4327-B2E2-FD3CA37CA59C", @"True" );
            // Add Block Attribute Value            //   Block: Site List            //   BlockType: Site List            //   Block Location: Page=Apple TV Apps, Site=Rock RMS            //   Attribute: core.CustomGridEnableStickyHeaders            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "2ED8AA54-A7F5-4EB9-91BC-043CC5EC2433", "739D4708-E384-4853-A985-466E3A7B905A", @"False" );
        }
    }
}
