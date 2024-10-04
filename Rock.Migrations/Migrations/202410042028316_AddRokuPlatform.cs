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
using System;
using System.Data.Entity.Migrations;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class AddRokuPlatform : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            TvAppSettingPagesUp();
            AddThemeDetailPageUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            TvAppSettingPagesDown();
            AddThemeDetailPageDown();
        }

        /// <summary>
        /// Updates Rock to have a new TV Apps setting page with nested pages.
        /// </summary>
        private void TvAppSettingPagesUp()
        {
            // Add TV Application List Page
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "TV Apps", string.Empty, "452D2C48-8802-4449-9C57-DAC3876BF5DC", "fa fa-tv" );

            // Add Roku Application Detail Page
            RockMigrationHelper.AddPage( true, "452D2C48-8802-4449-9C57-DAC3876BF5DC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Roku Application Detail", string.Empty, "867EC436-7F72-4108-81B6-ADBCFFC3918A", "fa fa-tv" );

            // Add Apple TV Application Detail Page
            RockMigrationHelper.AddPage( true, "452D2C48-8802-4449-9C57-DAC3876BF5DC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Apple TV Application Detail", string.Empty, "3D874455-7FE1-407B-A817-B0F82A51CEB8", "fa fa-tv" );

            // Add Roku Page Detail Page
            RockMigrationHelper.AddPage( true, "867EC436-7F72-4108-81B6-ADBCFFC3918A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Roku Page Detail", string.Empty, "35DDAEC3-0FC7-4B3E-93B3-FADB7658A4D0", "fa fa-tv" );

            // Add Apple TV Page Detail Page
            RockMigrationHelper.AddPage( true, "3D874455-7FE1-407B-A817-B0F82A51CEB8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Apple TV Page Detail", string.Empty, "01B8E8F7-736D-4380-B81E-063F030B90D0", "fa fa-tv" );

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.TvApplicationList", "869B2D70-4AE6-40A0-8899-A3EB9EDFB3B3", false, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.TvPageList", "BFE024A8-BDF2-4F11-8266-8AE4F4EA483B", false, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.RokuApplicationDetail", "89843E83-ADDB-4140-AA54-926ADCCD5558", false, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.RokuPageDetail", "DDD1ACC4-7FC4-42C8-B66D-64346C026FD1", false, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.AppleTvAppDetail", "E66D1530-8E39-4C00-8FA4-078482E56080", false, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Tv.AppleTvPageDetail", "D8419B3C-EDA1-46FC-9810-B1D81FB37CB3", false, false );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "TV Application List", "Displays a list of TV applications.", "Rock.Blocks.Tv.TvApplicationList", "TV > TV Apps", "5DA60F71-DD30-4333-9863-1CCFCE241CDF" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "TV Page List", "Displays a list of pages.", "Rock.Blocks.Tv.TvPageList", "TV > TV Apps", "11616362-6F7F-4B98-BC2A-DFD18AB983D9" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Roku Application Detail", "Displays the details of a Roku application.", "Rock.Blocks.Tv.RokuApplicationDetail", "TV > TV Apps", "261903DF-8632-456B-8272-4E4FFF07147A" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Roku Page Detail", "Displays the details of a Roku page.", "Rock.Blocks.Tv.RokuPageDetail", "TV > TV Apps", "97C8A25D-8CB3-4662-8371-A37CC28B6F36" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Application Detail", "Displays the details of an Apple TV application.", "Rock.Blocks.Tv.AppleTvAppDetail", "TV > TV Apps", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" );
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page Detail", "Displays the details of an Apple TV page.", "Rock.Blocks.Tv.AppleTvPageDetail", "TV > TV Apps", "ADBF3377-A491-4016-9375-346496A25FB4" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "11616362-6F7F-4B98-BC2A-DFD18AB983D9", Rock.SystemGuid.FieldType.PAGE_REFERENCE, "Detail Page", "DetailPage", string.Empty, "The page that will show the page details.", 0, string.Empty, "0A23278F-3BD0-40DF-954D-193329C40EB0" );

            // Add TV Application List Block
            RockMigrationHelper.AddBlock( true, "452D2C48-8802-4449-9C57-DAC3876BF5DC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "5DA60F71-DD30-4333-9863-1CCFCE241CDF".AsGuid(), "TV Application List", "Main", @"", @"", 0, "E6E00D74-28FD-4287-800F-1B4124ADEDF2" );

            // Add Roku Application Detail Block
            RockMigrationHelper.AddBlock( true, "867EC436-7F72-4108-81B6-ADBCFFC3918A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "261903df-8632-456b-8272-4e4fff07147a".AsGuid(), "Roku Application Detail", "Main", @"", @"", 0, "1BAF70C6-CFF7-485C-8C67-8A7507D0150A" );

            // Add Roku TV Page List Block
            RockMigrationHelper.AddBlock( true, "867EC436-7F72-4108-81B6-ADBCFFC3918A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "11616362-6f7f-4b98-bc2a-dfd18ab983d9".AsGuid(), "Roku Page List", "Main", @"", @"", 1, "55A5B34B-0CC3-4ED8-BB81-BA26732179EB" );

            // Add Apple TV Application Detail Block
            RockMigrationHelper.AddBlock( true, "3D874455-7FE1-407B-A817-B0F82A51CEB8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "cdab601d-1369-44cb-a146-4e80c7d66bcd".AsGuid(), "Apple TV Application Detail", "Main", @"", @"", 0, "7DD94637-4AEA-4DC2-A4AA-A5ACE71C6218" );

            // Add Apple TV Page List Block
            RockMigrationHelper.AddBlock( true, "3D874455-7FE1-407B-A817-B0F82A51CEB8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "11616362-6f7f-4b98-bc2a-dfd18ab983d9".AsGuid(), "Apple TV Page List", "Main", @"", @"", 1, "C722EAEA-A78C-4BA2-8A0A-F1D64EF3470C" );

            // Add Roku Page Detail Block
            RockMigrationHelper.AddBlock( true, "35DDAEC3-0FC7-4B3E-93B3-FADB7658A4D0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "97C8A25D-8CB3-4662-8371-A37CC28B6F36".AsGuid(), "Roku Page Detail", "Main", @"", @"", 0, "97E0B18C-1F27-4C36-8932-3C7E12F0D207" );

            // Add Apple TV Page Detail Block
            RockMigrationHelper.AddBlock( true, "01B8E8F7-736D-4380-B81E-063F030B90D0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ADBF3377-A491-4016-9375-346496A25FB4".AsGuid(), "Apple TV Page Detail", "Main", @"", @"", 0, "418DE1CB-DE9E-42C4-B022-7DA4804A6A7E" );

            // Set default pages for Roku Page List block and Apple Tv Page List block

            // Apple Tv
            RockMigrationHelper.AddBlockAttributeValue( true, "C722EAEA-A78C-4BA2-8A0A-F1D64EF3470C", "0A23278F-3BD0-40DF-954D-193329C40EB0", "01B8E8F7-736D-4380-B81E-063F030B90D0" );

            // Roku 
            RockMigrationHelper.AddBlockAttributeValue( true, "55A5B34B-0CC3-4ED8-BB81-BA26732179EB", "0A23278F-3BD0-40DF-954D-193329C40EB0", "35DDAEC3-0FC7-4B3E-93B3-FADB7658A4D0" );
        }

        /// <summary>
        /// Downgrades the <see cref="TvAppSettingPagesUp"/>.
        /// </summary>
        private void TvAppSettingPagesDown()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( "55A5B34B-0CC3-4ED8-BB81-BA26732179EB", "0A23278F-3BD0-40DF-954D-193329C40EB0" );
            RockMigrationHelper.DeleteBlockAttributeValue( "C722EAEA-A78C-4BA2-8A0A-F1D64EF3470C", "0A23278F-3BD0-40DF-954D-193329C40EB0" );

            RockMigrationHelper.DeleteBlock( "418DE1CB-DE9E-42C4-B022-7DA4804A6A7E" );
            RockMigrationHelper.DeleteBlock( "97E0B18C-1F27-4C36-8932-3C7E12F0D207" );
            RockMigrationHelper.DeleteBlock( "C722EAEA-A78C-4BA2-8A0A-F1D64EF3470C" );
            RockMigrationHelper.DeleteBlock( "7DD94637-4AEA-4DC2-A4AA-A5ACE71C6218" );
            RockMigrationHelper.DeleteBlock( "55A5B34B-0CC3-4ED8-BB81-BA26732179EB" );
            RockMigrationHelper.DeleteBlock( "1BAF70C6-CFF7-485C-8C67-8A7507D0150A" );
            RockMigrationHelper.DeleteBlock( "E6E00D74-28FD-4287-800F-1B4124ADEDF2" );

            RockMigrationHelper.DeletePage( "01B8E8F7-736D-4380-B81E-063F030B90D0" );
            RockMigrationHelper.DeletePage( "35DDAEC3-0FC7-4B3E-93B3-FADB7658A4D0" );
            RockMigrationHelper.DeletePage( "3D874455-7FE1-407B-A817-B0F82A51CEB8" );
            RockMigrationHelper.DeletePage( "867EC436-7F72-4108-81B6-ADBCFFC3918A" );
            RockMigrationHelper.DeletePage( "452D2C48-8802-4449-9C57-DAC3876BF5DC" );
        }

        private void AddThemeDetailPageUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ThemeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ThemeDetail", "Theme Detail", "Rock.Blocks.Cms.ThemeDetail, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "D4BFE2A3-B5FA-45CB-9C53-A6BEA98ECDDA" );

            // Add/Update Obsidian Block Type
            //   Name:Theme Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ThemeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Theme Detail", "Displays the details of a particular theme.", "Rock.Blocks.Cms.ThemeDetail", "CMS", "4BD81377-E3C2-48C8-8BBE-20D2BE915446" );

            // Attribute for BlockType
            //   BlockType: Theme List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "fd99e0aa-e1cb-4049-a6f6-9c5f2a34f694", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to use for editing next-gen themes.", 0, "", "f92381df-470e-4599-b7b4-59d24e6f924f" );

            // Add Page 
            //  Internal Name: Theme Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Theme Detail", "", "d6a96f8e-56dd-4e09-abe0-d4a7a84b1dc5", "" );

            // Add Block 
            //  Block Name: Theme Detail
            //  Page Name: Theme Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "d6a96f8e-56dd-4e09-abe0-d4a7a84b1dc5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4BD81377-E3C2-48C8-8BBE-20D2BE915446".AsGuid(), "Theme Detail", "Main", @"", @"", 0, "e39b61f6-14b0-4e45-a0e1-cb9ba6d93b1f" );

            RockMigrationHelper.AddBlockAttributeValue( "197363ee-20cd-47ac-97f8-b47401f7b2c5", "f92381df-470e-4599-b7b4-59d24e6f924f", "d6a96f8e-56dd-4e09-abe0-d4a7a84b1dc5" );

            // Update old styler page route to use new route.
            RockMigrationHelper.UpdatePageRoute( "132c10e9-4bdc-8141-7bf3-1cbb7875512a", "a74eec7c-4f9e-48f5-a996-74a856981b4c", "admin/cms/themes/{EditTheme}/styler" );

            // Add theme detail page route.
            RockMigrationHelper.AddOrUpdatePageRoute( "d6a96f8e-56dd-4e09-abe0-d4a7a84b1dc5", "admin/cms/themes/{ThemeId}", "e701b696-7bb7-48a6-9f34-2973d20f420e" );
        }

        private void AddThemeDetailPageDown()
        {
            RockMigrationHelper.DeletePageRoute( "e701b696-7bb7-48a6-9f34-2973d20f420e" );
            RockMigrationHelper.UpdatePageRoute( "132c10e9-4bdc-8141-7bf3-1cbb7875512a", "a74eec7c-4f9e-48f5-a996-74a856981b4c", "admin/cms/themes/{EditTheme}" );
            RockMigrationHelper.DeleteBlock( "e39b61f6-14b0-4e45-a0e1-cb9ba6d93b1f" );
            RockMigrationHelper.DeletePage( "d6a96f8e-56dd-4e09-abe0-d4a7a84b1dc5" );
            RockMigrationHelper.DeleteAttribute( "f92381df-470e-4599-b7b4-59d24e6f924f" );
            RockMigrationHelper.DeleteBlockType( "4BD81377-E3C2-48C8-8BBE-20D2BE915446" );
        }
    }
}
