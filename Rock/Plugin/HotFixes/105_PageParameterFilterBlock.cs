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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 105, "1.10.0" )]
    public class PageParameterFilterBlock : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
             * This migration is taken from the EF migration Rollup_0303 in the develop branch. This was done because the PageParameterFilter block was cherry picked from develop.
             */

            //// Add block type Page Parameter Filter
            //RockMigrationHelper.UpdateBlockType("Page Parameter Filter","Filter block that passes the filter values as query string parameters.","~/Blocks/Reporting/PageParameterFilter.ascx","Reporting","6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7");

            //// Attrib for BlockType: Page Parameter Filter:Show Block Title
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Block Title", "ShowBlockTitle", "Show Block Title", @"Determines if the Block Title should be displayed", 1, @"True", "D0F41DA4-F5D3-4D43-99C5-B80954F3755B" );
            //// Attrib for BlockType: Page Parameter Filter:Block Title Text
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Text", "BlockTitleText", "Block Title Text", @"The text to display as the block title.", 2, @"BlockTitle", "03C296C8-B892-4731-AD2D-24B8A1614181" );
            //// Attrib for BlockType: Page Parameter Filter:Block Title Icon CSS Class
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCSSClass", "Block Title Icon CSS Class", @"The css class name to use for the block title icon.", 3, @"fa fa-filter", "6677A0F0-254A-4431-B336-D2266C8FE62F" );
            //// Attrib for BlockType: Page Parameter Filter:Filters Per Row
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filters Per Row", "FiltersPerRow", "Filters Per Row", @"The number of filters to have per row.  Maximum is 12.", 4, @"2", "F625D9F9-E549-4CD3-8E1D-273907AB7E70" );
            //// Attrib for BlockType: Page Parameter Filter:Show Reset Filters Button
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reset Filters Button", "ShowResetFiltersButton", "Show Reset Filters Button", @"Determines if the Reset Filters button should be displayed", 5, @"True", "C9C347F0-76FA-4610-85B0-5385F57AE725" );
            //// Attrib for BlockType: Page Parameter Filter:Filter Button Text
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filter Button Text", "FilterButtonText", "Filter Button Text", @"Sets the button text for the filter button.", 6, @"Filter", "731BB3C6-CDD4-4D69-9FA7-4592380E57CA" );
            //// Attrib for BlockType: Page Parameter Filter:Filter Button Size
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Button Size", "FilterButtonSize", "Filter Button Size", @"", 7, @"3", "B62222F0-2EF5-476F-82E6-AAE96001130A" );
            //// Attrib for BlockType: Page Parameter Filter:Redirect Page
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "Redirect Page", @"If set, the filter button will redirect to the selected page.", 8, @"", "54A62107-9A51-4F02-BD5D-639FC2684CC4" );
            //// Attrib for BlockType: Page Parameter Filter:Does Selection Cause Postback
            //RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Does Selection Cause Postback", "DoesSelectionCausePostback", "Does Selection Cause Postback", @"If set, selecting a filter will force a PostBack, recalculating the available selections. Useful for SQL values.", 9, @"False", "467B5EFC-A379-48DB-A2DA-EF390D4A4481" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
