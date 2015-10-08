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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class InvolvementSignup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Page: Serve
            RockMigrationHelper.UpdateBlockType( "Connection Opportunity Search Lava", "Allows users to search for an opportunity to join", "~/Blocks/Involvement/ExternalOpportunitySearch.ascx", "Involvement", "C0D58DEE-D266-4AA8-8750-414A3CC26C07" );

            RockMigrationHelper.AddBlock( "2AA719FD-5B9F-4A9A-A8BF-C135EEA02BC8", "", "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "Connection Opportunity Search Lava", "Main", "", "", 1, "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Name Filter", "DisplayNameFilter", "", "Display the name filter", 0, @"True", "4361D6EE-B95F-4DE7-88C5-F53A5494A1F6" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the connection type name.", 0, @"False", "60CB4180-F2C1-4725-BF8C-5D219B6ABE81" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Attribute Filters", "DisplayAttributeFilters", "", "Display the attribute filters", 0, @"True", "F81DE506-4E3F-4350-91F6-F7662408B509" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "The page used to view a connection opportunity.", 0, @"", "89658162-3B46-43F4-A51A-690FC49D1DD5" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Connection Type Id", "ConnectionTypeId", "", "The Id of the connection type whose opportunities are displayed.", 0, @"1", "9FFEFD20-57C0-4C51-A31F-67F492EE884F" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of opportunities.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunitySearch.lava' %}", "8A631314-EAB9-458F-9A95-292C2F15F957" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "664F3BDE-BD2F-42C5-B133-0E35A2856BE1" );

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "4361D6EE-B95F-4DE7-88C5-F53A5494A1F6", @"True" ); // Display Name Filter

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "8A631314-EAB9-458F-9A95-292C2F15F957", @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunitySearch.lava' %}" ); // Lava Template

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "60CB4180-F2C1-4725-BF8C-5D219B6ABE81", @"False" ); // Set Page Title

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "664F3BDE-BD2F-42C5-B133-0E35A2856BE1", @"False" ); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "F81DE506-4E3F-4350-91F6-F7662408B509", @"True" ); // Display Attribute Filters

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "89658162-3B46-43F4-A51A-690FC49D1DD5", @"eda56877-d431-4067-9db0-569e93bc7258" ); // Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD", "9FFEFD20-57C0-4C51-A31F-67F492EE884F", @"1" ); // Connection Type Id

            // Page: Opportunity Detail
            RockMigrationHelper.AddPage( "2AA719FD-5B9F-4A9A-A8BF-C135EEA02BC8", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Opportunity Detail", "", "EDA56877-D431-4067-9DB0-569E93BC7258", "" ); // Site:External Website

            RockMigrationHelper.UpdateBlockType( "External Connection Opportunity Detail", "Displays the details of the given opportunity for the external website.", "~/Blocks/Involvement/ExternalConnectionOpportunityDetail.ascx", "Involvement", "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC" );

            RockMigrationHelper.AddBlock( "EDA56877-D431-4067-9DB0-569E93BC7258", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "22AABB65-94B1-4B45-96E0-7DAED139AB8F" );

            RockMigrationHelper.AddBlock( "EDA56877-D431-4067-9DB0-569E93BC7258", "", "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "External Connection Opportunity Detail", "Main", "", "", 0, "7467A0E6-5348-47A5-A6A3-DFAD2F19236E" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Mobile Phone", "DisplayMobilePhone", "", "Whether to display mobile phone", 0, @"True", "0F56AAD0-A625-476F-BC72-0305AA125543" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Home Phone", "DisplayHomePhone", "", "Whether to display home phone", 0, @"True", "AD7359D0-CFF3-435F-98E2-F552A75DDD89" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 0, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "E967EC6C-7DFD-42B4-94B9-AFC093BE8093" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "BCCD15FA-DF2C-455E-A73F-BE6183FB0F74" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of opportunities.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunityResponseMessage.lava' %}", "B6DD8328-5D75-444C-AE8D-05011165668D" );

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" ); // Template

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" ); // Root Page

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue( "22AABB65-94B1-4B45-96E0-7DAED139AB8F", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "BCCD15FA-DF2C-455E-A73F-BE6183FB0F74" );
            RockMigrationHelper.DeleteAttribute( "E967EC6C-7DFD-42B4-94B9-AFC093BE8093" );
            RockMigrationHelper.DeleteAttribute( "AD7359D0-CFF3-435F-98E2-F552A75DDD89" );
            RockMigrationHelper.DeleteAttribute( "B6DD8328-5D75-444C-AE8D-05011165668D" );
            RockMigrationHelper.DeleteAttribute( "0F56AAD0-A625-476F-BC72-0305AA125543" );
            RockMigrationHelper.DeleteBlock( "7467A0E6-5348-47A5-A6A3-DFAD2F19236E" );
            RockMigrationHelper.DeleteBlock( "22AABB65-94B1-4B45-96E0-7DAED139AB8F" );
            RockMigrationHelper.DeleteBlockType( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC" );
            RockMigrationHelper.DeletePage( "EDA56877-D431-4067-9DB0-569E93BC7258" ); //  Page: Opportunity Detail

            RockMigrationHelper.DeleteAttribute( "9FFEFD20-57C0-4C51-A31F-67F492EE884F" );
            RockMigrationHelper.DeleteAttribute( "89658162-3B46-43F4-A51A-690FC49D1DD5" );
            RockMigrationHelper.DeleteAttribute( "F81DE506-4E3F-4350-91F6-F7662408B509" );
            RockMigrationHelper.DeleteAttribute( "664F3BDE-BD2F-42C5-B133-0E35A2856BE1" );
            RockMigrationHelper.DeleteAttribute( "60CB4180-F2C1-4725-BF8C-5D219B6ABE81" );
            RockMigrationHelper.DeleteAttribute( "8A631314-EAB9-458F-9A95-292C2F15F957" );
            RockMigrationHelper.DeleteAttribute( "4361D6EE-B95F-4DE7-88C5-F53A5494A1F6" );
            RockMigrationHelper.DeleteBlock( "2B60FE4A-5AD8-4F0A-9205-0744485C3DDD" );
            RockMigrationHelper.DeleteBlockType( "C0D58DEE-D266-4AA8-8750-414A3CC26C07" );//Page: Serve
        }
    }
}
