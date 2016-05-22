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
    public partial class TagsByLetterPage : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Tags", "", "2654EBE9-F585-4E64-93F3-102357F89660", "fa fa-tags"); // Site:Rock Internal
            AddPage("2654EBE9-F585-4E64-93F3-102357F89660", "195BCD57-1C10-4969-886F-7324B6287B75", "Tag Details", "", "D258BF5B-B585-4C5B-BDCD-99F7519D45E2", "fa fa-tag"); // Site:Rock Internal

            AddBlockType("Tags By Letter", "Lists tags grouped by the first letter of the name with counts for people to select.", "~/Blocks/Core/TagsByLetter.ascx", "784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B");

            // Add Block to Page: Tags, Site: Rock Internal
            AddBlock("2654EBE9-F585-4E64-93F3-102357F89660", "", "784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B", "Tag Display", "Main", 0, "75F3922D-2ECD-44B0-B1F3-03CEA35AAD6B");

            // Add Block to Page: Tag Details, Site: Rock Internal
            AddBlock("D258BF5B-B585-4C5B-BDCD-99F7519D45E2", "", "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "Individuals In Tag", "Main", 1, "9F491A97-50D2-44FA-AB3E-E36989793570");

            // Add Block to Page: Tag Details, Site: Rock Internal
            AddBlock("D258BF5B-B585-4C5B-BDCD-99F7519D45E2", "", "B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF", "Tag Detail", "Main", 0, "92361A75-BDEA-4F95-A6F2-FA36EF1AD957");


            // Attrib for BlockType: Tags By Letter:Detail Page
            AddBlockTypeAttribute("784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "3CE6EC7B-3660-4E4E-BB25-0CC5B8EEC64B");

            // Attrib Value for Block:Tag Display, Attribute:Detail Page Page: Tags, Site: Rock Internal
            AddBlockAttributeValue("75F3922D-2ECD-44B0-B1F3-03CEA35AAD6B", "3CE6EC7B-3660-4E4E-BB25-0CC5B8EEC64B", @"d258bf5b-b585-4c5b-bdcd-99f7519d45e2");
      
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Tags By Letter:Detail Page
            DeleteAttribute("3CE6EC7B-3660-4E4E-BB25-0CC5B8EEC64B");

            // Remove Block: Tag Detail, from Page: Tag Details, Site: Rock Internal
            DeleteBlock("92361A75-BDEA-4F95-A6F2-FA36EF1AD957");

            // Remove Block: Individuals In Tag, from Page: Tag Details, Site: Rock Internal
            DeleteBlock("9F491A97-50D2-44FA-AB3E-E36989793570");

            // Remove Block: Tag Display, from Page: Tags, Site: Rock Internal
            DeleteBlock("75F3922D-2ECD-44B0-B1F3-03CEA35AAD6B");

            DeleteBlockType("784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B"); // Tags By Letter

            DeletePage("D258BF5B-B585-4C5B-BDCD-99F7519D45E2"); // Page: Tag DetailsLayout: Full Width Panel, Site: Rock Internal
            DeletePage("2654EBE9-F585-4E64-93F3-102357F89660"); // Page: TagsLayout: Full Width, Site: Rock Internal
        }
    }
}
