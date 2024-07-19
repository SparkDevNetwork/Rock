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
    public partial class Rollup_20220615 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            LavaDashboardWidget();
            DataViewAndReportSearchResultPagesUp();
            AddOrUpdateStepFlowPageAndRouteUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DataViewAndReportSearchResultPagesDown();
            AddOrUpdateStepFlowPageAndRouteDown();
        }

        /// <summary>
        /// PA: Rename Liquid Dashboard Widget to Lava 
        /// </summary>
        private void LavaDashboardWidget()
        {
            RockMigrationHelper.UpdateBlockType( "Lava Dashboard Widget", "Dashboard Widget from Lava using YTD metric values", "~/Blocks/Reporting/Dashboard/LiquidDashboardWidget.ascx", "Reporting > Dashboard", Rock.SystemGuid.BlockType.REPORTING_LAVA_DASHBOARD_WIDGET );
        }

        /// <summary>
        /// KA: Migration to Add Search result pages for DataViews and Reports
        /// </summary>
        private void DataViewAndReportSearchResultPagesUp()
        {
            // Add Page - Internal Name: DataView Search Results - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.DATA_VIEWS, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "DataView Search Results", "", SystemGuid.Page.DATAVIEW_SEARCH_RESULTS );
            // Add/Update BlockType - Name: DataView Search - Category: Reporting - Path: ~/Blocks/Reporting/DataViewSearch.ascx - EntityType: -
            RockMigrationHelper.UpdateBlockType( "DataView Search", "Handles displaying dataview search results and redirects to the dataview result page (via route ~/reporting/dataViews?) when only one match was found.", "~/Blocks/Reporting/DataViewSearch.ascx", "Reporting", SystemGuid.BlockType.DATAVIEW_SEARCH_RESULTS );
            // Add Block - Block Name: DataView Search - Page Name: DataView Search Results Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.DATAVIEW_SEARCH_RESULTS.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.DATAVIEW_SEARCH_RESULTS.AsGuid(), "DataView Search", "Main", @"", @"", 0, SystemGuid.Block.DATAVIEW_SEARCH_RESULTS );
        
            // Add Page - Internal Name: Reports Search Results - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.REPORTS_REPORTING, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Reports Search Results", "", SystemGuid.Page.REPORT_SEARCH_RESULTS );
            // Add/Update BlockType - Name: Reports Search - Category: Reporting - Path: ~/Blocks/Reporting/ReportSearch.ascx - EntityType: -
            RockMigrationHelper.UpdateBlockType( "Report Search", "Handles displaying report search results and redirects to the report result page (via route ~/reporting/reports?) when only one match was found.", "~/Blocks/Reporting/ReportSearch.ascx", "Reporting", SystemGuid.BlockType.REPORT_SEARCH_RESULTS );
            // Add Block - Block Name: Reports Search - Page Name: Report Search Results Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.REPORT_SEARCH_RESULTS.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.REPORT_SEARCH_RESULTS.AsGuid(), "Report Search", "Main", @"", @"", 0, SystemGuid.Block.REPORT_SEARCH_RESULTS );
        
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841",
                SystemGuid.FieldType.PAGE_REFERENCE,
                "Search Results Page",
                "SearchResultsPage",
                "Search Results Page",
                "The page to display search results on",
                11,
                "",
                SystemGuid.Attribute.CATEGORY_TREEVIEW_SEARCH_RESULTS );

            // Set the DataViewSearchResults page as the value for the SearcResultsPage attribute value for the CategoryTreeview block on that page
            RockMigrationHelper.AddBlockAttributeValue( true, "6A9111AC-34E7-4103-A12A-9A89C2A14B57", SystemGuid.Attribute.CATEGORY_TREEVIEW_SEARCH_RESULTS, SystemGuid.Page.DATAVIEW_SEARCH_RESULTS );
            // Set the ReportSearchResults page as the value for the SearcResultsPage attribute value for the CategoryTreeview block on that page
            RockMigrationHelper.AddBlockAttributeValue( true, "0F1F8343-A187-4653-9A4A-47D67CE86D71", SystemGuid.Attribute.CATEGORY_TREEVIEW_SEARCH_RESULTS, SystemGuid.Page.REPORT_SEARCH_RESULTS );
        
        }

        /// <summary>
        /// KA: Migration to Add Search result pages for DataViews and Reports
        /// </summary>
        private void DataViewAndReportSearchResultPagesDown()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( "6A9111AC-34E7-4103-A12A-9A89C2A14B57", SystemGuid.Attribute.CATEGORY_TREEVIEW_SEARCH_RESULTS );
            RockMigrationHelper.DeleteBlockAttributeValue( "0F1F8343-A187-4653-9A4A-47D67CE86D71", SystemGuid.Attribute.CATEGORY_TREEVIEW_SEARCH_RESULTS );
        
            // Remove Block - Name: DataView Search, from Page: DataView Search Results, Site: Rock RMS - from Page: DataView Search Results, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.DATAVIEW_SEARCH_RESULTS );
            // Delete BlockType - Name: DataView Search - Category: Reporting - Path: ~/Blocks/Reporting/DataViewSearch.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.DATAVIEW_SEARCH_RESULTS );
            // Delete Page Internal Name: DataView Search Results Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.DATAVIEW_SEARCH_RESULTS );
        
            // Remove Block - Name: Report Search, from Page: Report Search Results, Site: Rock RMS - from Page: Report Search Results, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.REPORT_SEARCH_RESULTS );
            // Delete BlockType - Name: Report Search - Category: Reporting - Path: ~/Blocks/Reporting/ReportSearch.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.REPORT_SEARCH_RESULTS );
            // Delete Page Internal Name: Report Search Results Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.REPORT_SEARCH_RESULTS );
        }

        /// <summary>
        /// CR: Add StepFlow page, route, and block (dependent on v14 block)
        /// </summary>
        private void AddOrUpdateStepFlowPageAndRouteUp()
        {
            // Add Page - Internal Name: Step Flow - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.STEP_PROGRAM_DETAIL, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step Flow", "", SystemGuid.Page.STEP_FLOW, "fa-project-diagram" );
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route - Page:Step Flow - Route: steps/program/{ProgramId}/flow
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.STEP_FLOW, "steps/program/{ProgramId}/flow", SystemGuid.PageRoute.STEP_FLOW );
#pragma warning restore CS0618 // Type or member is obsolete
            // Add Block - Block Name: Step Flow - Page Name: Step Flow Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.STEP_FLOW.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "2B4E0128-BCDF-48BF-AEC9-85001169DA3E".AsGuid(), "Step Flow", "Main", @"", @"", 0, "A40684E9-10DA-4CF8-815B-EBDE53624419" );
        }

        /// <summary>
        /// CR: Add StepFlow page, route, and block (dependent on v14 block)
        /// </summary>
        private void AddOrUpdateStepFlowPageAndRouteDown()
        {
            // Remove Block - Name: Step Flow, from Page: Step Flow, Site: Rock RMS - from Page: Step Flow, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A40684E9-10DA-4CF8-815B-EBDE53624419" );
            // Delete Page Internal Name: Step Flow Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.STEP_FLOW );
        }
    }
}
