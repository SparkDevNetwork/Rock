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
    public partial class CodeGenerated_20211130 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "AdultAddress", "Address", @"How should Address be displayed for adults?", 8, @"Optional", "7939DFE1-5950-4462-8306-E2F71DB6F9CD" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Component
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Search Component", "SearchComponent", "Search Component", @"The search component to use when performing searches.", 0, @"", "BA787B11-3CAD-40CD-B7DA-B63258CD50AA" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Show Search Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Search Label", "ShowSearchLabel", "Show Search Label", @"Determines if the input label for the search box should be displayed.", 1, @"True", "06851EC6-6C4B-4D67-AC9A-C58CAA945F5B" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Label Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Label Text", "SearchLabelText", "Search Label Text", @"The label for the search field.", 2, @"Search", "06832BBF-4058-4002-8843-37F692CDDEE4" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Placeholder Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Placeholder Text", "SearchPlaceholderText", "Search Placeholder Text", @"The text to show as the placeholder text in the search box.", 3, @"", "E1EE6EB9-86AB-4970-B316-0973277C52DD" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Result Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Result Item Template", "ResultItemTemplate", "Result Item Template", @"Lava template for rendering each result item. The Lava merge field will be 'Item'.", 4, @"50FABA2A-B23C-46CD-A634-2F54BC1AE8C3", "ACA705D3-F96F-4EAC-8EA6-582F1F479C33" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Results Separator Content
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Separator Content", "ResultsSeparatorContent", "Results Separator Content", @"Content to display between the search input and the results. This content will show with the display of the results.", 5, @"<StackLayout StyleClass=""search-result-header"">
    <Rock:Divider />
</StackLayout>", "6C6E6D25-0FB4-4486-B042-69EE17F568BB" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Detail Navigation Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Detail Navigation Action", "DetailNavigationAction", "Detail Navigation Action", @"The navigation action to perform when an item is tapped. The Guid of the item will be passed as the entity name and Guid, such as PersonGuid=value.", 6, @"{""Type"": 0}", "AA6384F3-0B7F-478A-A8EF-22C683627DE9" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Max Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "Max Results", @"The maximum number of results to show before displaying a 'Show More' option.", 7, @"25", "42EB2DFC-1BAE-4AE3-BCD8-EC262109BA8A" );

            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Delete Navigation Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Delete Navigation Action", "DeleteNavigationAction", "Delete Navigation Action", @"The action to perform after the group member is deleted from the group.", 7, @"{""Type"": 1, ""PopCount"": 1}", "002FFC80-FA1E-43C2-98E3-E61D4AF9127A" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Member Edit
            //   Category: Mobile > Groups
            //   Attribute: Delete Navigation Action
            RockMigrationHelper.DeleteAttribute("002FFC80-FA1E-43C2-98E3-E61D4AF9127A");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Max Results
            RockMigrationHelper.DeleteAttribute("42EB2DFC-1BAE-4AE3-BCD8-EC262109BA8A");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Detail Navigation Action
            RockMigrationHelper.DeleteAttribute("AA6384F3-0B7F-478A-A8EF-22C683627DE9");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Results Separator Content
            RockMigrationHelper.DeleteAttribute("6C6E6D25-0FB4-4486-B042-69EE17F568BB");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Result Item Template
            RockMigrationHelper.DeleteAttribute("ACA705D3-F96F-4EAC-8EA6-582F1F479C33");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Placeholder Text
            RockMigrationHelper.DeleteAttribute("E1EE6EB9-86AB-4970-B316-0973277C52DD");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Label Text
            RockMigrationHelper.DeleteAttribute("06832BBF-4058-4002-8843-37F692CDDEE4");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Show Search Label
            RockMigrationHelper.DeleteAttribute("06851EC6-6C4B-4D67-AC9A-C58CAA945F5B");

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Search Component
            RockMigrationHelper.DeleteAttribute("BA787B11-3CAD-40CD-B7DA-B63258CD50AA");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Address
            RockMigrationHelper.DeleteAttribute("7939DFE1-5950-4462-8306-E2F71DB6F9CD");

            // Delete BlockType 
            //   Name: Widget List
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Widgets List
            RockMigrationHelper.DeleteBlockType("CFA0B72E-2816-4852-A5BD-DF50834B4FCA");

            // Delete BlockType 
            //   Name: Context Group
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Group
            RockMigrationHelper.DeleteBlockType("DACC3F98-3BBA-4525-8CE8-D3C9ADFB69D2");

            // Delete BlockType 
            //   Name: Context Entities
            //   Category: Rock Solid Church Demo > Page Debug
            //   Path: -
            //   EntityType: Context Entities
            RockMigrationHelper.DeleteBlockType("D2F7FAC3-D8C3-44CD-B8C9-4CFE1003260C");
        }
    }
}
