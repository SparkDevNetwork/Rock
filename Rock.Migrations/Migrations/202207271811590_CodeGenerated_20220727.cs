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
    public partial class CodeGenerated_20220727 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: CSV Import
           //   Category: CSV Import
           //   Path: ~/Blocks/CSVImport/CSVImport.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("CSV Import","Block to import data into Rock using the CSV files.","~/Blocks/CSVImport/CSVImport.ascx","CSV Import","EDA8F90D-1201-4AFF-9E6D-A8F6D6F618D9");

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.MediaAccountDetail
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Cms.MediaAccountDetail", "Media Account Detail", "Rock.Blocks.Cms.MediaAccountDetail, Rock.Blocks, Version=1.14.0.14, Culture=neutral, PublicKeyToken=null", false, false, "704FA615-60EB-4FD2-99ED-6B5AE0879145");

            // Add/Update Obsidian Block Type
            //   Name:Media Account Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.MediaAccountDetail
            RockMigrationHelper.UpdateMobileBlockType("Media Account Detail", "Displays the details of a particular media account.", "Rock.Blocks.Cms.MediaAccountDetail", "CMS", "A63F0145-D323-4B6E-AD21-BCDA1F1D8D5D");

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "53FFD639-09E0-42B7-AACF-29F4FD2B4C86".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"C7082627-D0EF-41B0-9359-2D51E456A970"); 

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Require Location for Additional Sign-ups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Location for Additional Sign-ups", "RequireLocationForAdditionalSignups", "Require Location for Additional Sign-ups", @"When enabled, a location will be required when signing up for additional times.", 16, @"False", "CB0C53C0-324A-4287-891B-9CAC2D69D615" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Content Library
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Content Library", "ContentLibrary", "Content Library", @"The content library to use when searching.", 0, @"", "7E1AB58F-4E85-42B9-910E-23F60E6B46DD" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Filters Panel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filters Panel", "ShowFiltersPanel", "Show Filters Panel", @"Determines if the filters panel should be visible.", 0, @"True", "3B64AF08-B7C5-4E2B-98C3-F343CC34497D" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Full-Text Search
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Full-Text Search", "ShowFullTextSearch", "Show Full-Text Search", @"Determines if the full-text search box is visible.", 0, @"True", "CE355482-4A1E-4CF5-A04F-3A321A479EBF" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Sort
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Sort", "ShowSort", "Show Sort", @"Determines if the custom sorting options are displayed.", 0, @"True", "4E8455C4-5808-4215-9FAA-F21AA518AC05" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Number Of Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number Of Results", "NumberOfResults", "Number Of Results", @"The number of results to include.", 0, @"10", "638A6178-16A1-4DC6-8F81-31998233DDFD" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Search On Load
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Search On Load", "SearchOnLoad", "Search On Load", @"Determines if search results will be displayed when the block loads.", 0, @"False", "236DF5AB-F247-42CB-BAB9-936A6FBDDCC7" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Group Results By Source
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Group Results By Source", "GroupResultsBySource", "Group Results By Source", @"This will group the results by library source.", 0, @"False", "98A39355-701B-43B4-B9A5-974AEC2100E0" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Enabled Sort Orders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Enabled Sort Orders", "EnabledSortOrders", "Enabled Sort Orders", @"The sort order options to be made available to the individual.", 0, @"newest,oldest,relevance,trending", "3AF153A8-1A22-4E38-AAE5-D1E4E6754863" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Trending Term
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Trending Term", "TrendingTerm", "Trending Term", @"The term to use for the trending sort option.", 0, @"Trending", "2001A5D8-838F-4478-B762-E747C8CE295D" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filters", "Filters", "Filters", @"The configured filter settings for this block instance.", 0, @"", "84327788-D602-418F-AF4F-54EEE9D65BC6" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Results Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Results Template", "ResultsTemplate", "Results Template", @"The lava template to use to render the results container. It must contain an element with the class 'result-items'.", 0, @"<div>
    <h2><i class=""{{ SourceEntity.IconCssClass""></i> {{ SourceName }}</h2>
    <div class=""result-items""></div>
    <div class=""actions"">
       <a href=""#"" class=""btn btn-default show-more"">Show More</a>
    </div>
</div>", "085A7213-9C0B-41FD-909B-D74804ABA12E" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Item Template", "ItemTemplate", "Item Template", @"The lava template to use to render a single result.", 0, @"<div class=""result-item"">
    <div>{{ results.Title }}</div>
</div>", "CE511E4F-E2DE-4C62-877E-DCE1323F1FC9" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Pre-Search Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pre-Search Template", "PreSearchTemplate", "Pre-Search Template", @"The lava template to use to render the content displayed before a search happens. This will not be used if Search on Load is enabled.", 0, @"", "5AF8D107-2BB1-4E71-B1F0-E0641ABFE80B" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Boost Matching Segments
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Boost Matching Segments", "BoostMatchingSegments", "Boost Matching Segments", @"Determines if matching personalization segments should receive an additional boost.", 0, @"True", "1E5A3E8D-B2CA-4C92-9947-D38CDAA08AB6" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Boost Matching Request Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Boost Matching Request Filters", "BoostMatchingRequestFilters", "Boost Matching Request Filters", @"Determines if matching personalization request filters should receive an additional boost.", 0, @"True", "FB8E0624-A56A-4005-A7FC-6396DE3E6346" );

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Request Filter Boost Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Request Filter Boost Amount", "SegmentBoostAmount", "Request Filter Boost Amount", @"The amount of boost to apply to matches on personalization request filters.", 0, @"", "F4472936-D735-4296-ABD1-8192C01FA3C9" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FBE0419-5404-4866-85A1-135542D33725", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "A4E9E4D8-F698-451E-A21D-01006533E6B1" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("C7082627-D0EF-41B0-9359-2D51E456A970","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("A4E9E4D8-F698-451E-A21D-01006533E6B1");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Request Filter Boost Amount
            RockMigrationHelper.DeleteAttribute("F4472936-D735-4296-ABD1-8192C01FA3C9");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Boost Matching Request Filters
            RockMigrationHelper.DeleteAttribute("FB8E0624-A56A-4005-A7FC-6396DE3E6346");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Boost Matching Segments
            RockMigrationHelper.DeleteAttribute("1E5A3E8D-B2CA-4C92-9947-D38CDAA08AB6");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Pre-Search Template
            RockMigrationHelper.DeleteAttribute("5AF8D107-2BB1-4E71-B1F0-E0641ABFE80B");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Item Template
            RockMigrationHelper.DeleteAttribute("CE511E4F-E2DE-4C62-877E-DCE1323F1FC9");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Results Template
            RockMigrationHelper.DeleteAttribute("085A7213-9C0B-41FD-909B-D74804ABA12E");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Filters
            RockMigrationHelper.DeleteAttribute("84327788-D602-418F-AF4F-54EEE9D65BC6");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Trending Term
            RockMigrationHelper.DeleteAttribute("2001A5D8-838F-4478-B762-E747C8CE295D");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Enabled Sort Orders
            RockMigrationHelper.DeleteAttribute("3AF153A8-1A22-4E38-AAE5-D1E4E6754863");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Group Results By Source
            RockMigrationHelper.DeleteAttribute("98A39355-701B-43B4-B9A5-974AEC2100E0");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Search On Load
            RockMigrationHelper.DeleteAttribute("236DF5AB-F247-42CB-BAB9-936A6FBDDCC7");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Number Of Results
            RockMigrationHelper.DeleteAttribute("638A6178-16A1-4DC6-8F81-31998233DDFD");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Sort
            RockMigrationHelper.DeleteAttribute("4E8455C4-5808-4215-9FAA-F21AA518AC05");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Full-Text Search
            RockMigrationHelper.DeleteAttribute("CE355482-4A1E-4CF5-A04F-3A321A479EBF");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Show Filters Panel
            RockMigrationHelper.DeleteAttribute("3B64AF08-B7C5-4E2B-98C3-F343CC34497D");

            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Content Library
            RockMigrationHelper.DeleteAttribute("7E1AB58F-4E85-42B9-910E-23F60E6B46DD");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Require Location for Additional Sign-ups
            RockMigrationHelper.DeleteAttribute("CB0C53C0-324A-4287-891B-9CAC2D69D615");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C7082627-D0EF-41B0-9359-2D51E456A970");

            // Delete BlockType 
            //   Name: CSV Import
            //   Category: CSV Import
            //   Path: ~/Blocks/CSVImport/CSVImport.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("EDA8F90D-1201-4AFF-9E6D-A8F6D6F618D9");

            // Delete BlockType 
            //   Name: Media Account Detail
            //   Category: CMS
            //   Path: -
            //   EntityType: Media Account Detail
            RockMigrationHelper.DeleteBlockType("A63F0145-D323-4B6E-AD21-BCDA1F1D8D5D");
        }
    }
}
