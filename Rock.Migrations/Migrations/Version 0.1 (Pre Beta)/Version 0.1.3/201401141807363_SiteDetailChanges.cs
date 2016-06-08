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
    public partial class SiteDetailChanges : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Set the Site page to Show Child Pages
            Sql( @"
                UPDATE [Page]
                SET [MenuDisplayChildPages] = 1
                WHERE [Id] = 2
            " );

            // Add a PageLiquid block to the current SiteDetail page in between the SiteDetail and Layout blocks
            AddBlock( "A2991117-0B85-4209-9008-254929C6E00F", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", @"", @"", 1, "34EC5861-84EF-4D1A-8C89-D207B3004FDC" );
            Sql( @"
                UPDATE [Block]
                SET [Order] = 2
                WHERE [Guid] = '937BC322-633D-43BF-89C6-54970CB67D52'
            " );

            // Set the attributes on that PageLiquid block: Include Current Parameters = true, Include Current QueryString = true, Root Page = Sites, Template = "{% include 'PageListAsTabs' %}"
            AddBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "True" );   // Current Parameters
            AddBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "True" );   // Current QueryString
            AddBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7596D389-4EAB-4535-8BEE-229737F46F44" );   // Root Page
            AddBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include 'PageListAsTabs' %}" );   // Template

            // Set the Title of the Site Detail page = "Layouts"
            Sql( @"
                UPDATE [Page]
                SET [Title] = 'Layouts'
                WHERE [Id] = 116
            " );

            // Create a second page called Site Detail (setting the Title to "Pages")
            AddPage( "7596D389-4EAB-4535-8BEE-229737F46F44", "195BCD57-1C10-4969-886F-7324B6287B75", "Site Detail", "A detail page for another site information", "1C763885-291F-44B7-A5E3-539584E07085" );
            Sql( @"
                UPDATE [Page]
                SET [Title] = 'Pages'
                WHERE Guid = '1C763885-291F-44B7-A5E3-539584E07085'
            " );

            // Create new Page List and Page Detail block types
            AddBlockType( "Page List", "A list of pages tied to the layout of a site", "~/Blocks/Cms/PageList.ascx", "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70" );

            // On this page put a SiteDetail block, a Page Liquid block (with the same settings as above), and the new PageList block.
            AddBlock( "1C763885-291F-44B7-A5E3-539584E07085", "", "2AC06C36-869F-45F7-8C14-802781C5F70E", "Site Detail", "Main", @"", @"", 0, "79CA5915-B78F-4230-B08E-6BFDB4CEF60A" );
            AddBlock( "1C763885-291F-44B7-A5E3-539584E07085", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", @"", @"", 1, "6E8216F2-6A48-4EF8-ABA3-736C2468610D" );
            AddBlock( "1C763885-291F-44B7-A5E3-539584E07085", "", "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70", "Page List", "Main", @"", @"", 2, "07E492AD-5F5D-4F3E-8103-CCE7D31CA25A" );

            // Set the attributes on that PageLiquid block: Include Current Parameters = true, Include Current QueryString = true, Root Page = Sites, Template = "{% include 'PageListAsTabs' %}"
            AddBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "True" );   // Current Parameters
            AddBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "True" );   // Current QueryString
            AddBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7596D389-4EAB-4535-8BEE-229737F46F44" );   // Root Page
            AddBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include 'PageListAsTabs' %}" );   // Template
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Once all that stuff in the Up() is done, take it all away here...in reverse order.
            DeleteBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "1322186A-862A-4CF1-B349-28ECB67229BA" );   // Template
            DeleteBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );   // Root Page
            DeleteBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );   // Current QueryString
            DeleteBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );   // Current Parameters
            DeleteBlock( "07E492AD-5F5D-4F3E-8103-CCE7D31CA25A" );
            DeleteBlock( "6E8216F2-6A48-4EF8-ABA3-736C2468610D" );
            DeleteBlock( "79CA5915-B78F-4230-B08E-6BFDB4CEF60A" );
            DeleteBlockType( "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70" );
            Sql( @"
                UPDATE [Page]
                SET [Title] = 'Site Detail'
                WHERE Guid = '1C763885-291F-44B7-A5E3-539584E07085'
            " );
            DeletePage( "1C763885-291F-44B7-A5E3-539584E07085" );
            Sql( @"
                UPDATE [Page]
                SET [Title] = 'Site Detail'
                WHERE [Id] = 116
            " );
            DeleteBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "1322186A-862A-4CF1-B349-28ECB67229BA" );   // Template
            DeleteBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );   // Root Page
            DeleteBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );   // Current QueryString
            DeleteBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );   // Current Parameters
            Sql( @"
                UPDATE [Block]
                SET [Order] = 1
                WHERE [Guid] = '937BC322-633D-43BF-89C6-54970CB67D52'
            " );
            DeleteBlock( "34EC5861-84EF-4D1A-8C89-D207B3004FDC" );
            Sql( @"
                UPDATE [Page]
                SET [MenuDisplayChildPages] = 0
                WHERE [Id] = 2
            " );
        }
    }
}
