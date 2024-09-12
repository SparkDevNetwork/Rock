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
    [MigrationNumber( 999, "1.16.6" )]
    public class AddObsidianPageParameterFilterBlockType : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddObsidianPageParameterFilterBlockTypeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddObsidianPageParameterFilterBlockTypeDown();
        }

        /// <summary>
        /// JPH: Add the obsidian page parameter filter block type and attributes up.
        /// </summary>
        private void AddObsidianPageParameterFilterBlockTypeUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.PageParameterFilter", "Page Parameter Filter", "Rock.Blocks.Reporting.PageParameterFilter, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "59F94307-B2B0-4383-9C2C-88A4E154C461" );

            // Add/Update Obsidian Block Type
            //   Name:Page Parameter Filter
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Parameter Filter", "Filter block that passes the filter values as query string parameters.", "Rock.Blocks.Reporting.PageParameterFilter", "Reporting", "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Text", "BlockTitleText", "Block Title Text", @"The text to display as the block title.", 0, @"", "8B424C65-1C4D-4A47-AFF6-64C99F777E6A" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCSSClass", "Block Title Icon CSS Class", @"The CSS class name to use for the block title icon.", 0, @"fa fa-filter", "7E543C23-6FB8-4538-9249-261C35DE1491" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Block Title", "ShowBlockTitle", "Show Block Title", @"Determines if the block title should be displayed.", 0, @"True", "54DCAD13-576B-475D-A6D2-5ABA57639212" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filter Button Text", "FilterButtonText", "Filter Button Text", @"The text to display on the filter button.", 0, @"Filter", "EFA46D88-B9A8-422C-B4A5-ADDA5CF13301" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Button Size", "FilterButtonSize", "Filter Button Size", @"The size of the filter button.", 0, @"3", "F6FFA298-9434-4BA8-9863-051E4DD52994" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter Button", "ShowFilterButton", "Show Filter Button", @"Determines if the filter button should be displayed.", 0, @"True", "B85336F2-6F40-4C7B-980D-1F957C4CE167" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Reset Filters Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reset Filters Button", "ShowResetFiltersButton", "Show Reset Filters Button", @"Determines if the reset filters button should be displayed.", 0, @"True", "82327B97-8A4F-4743-93CF-2E32FF5A8097" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filters Per Row
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filters Per Row", "FiltersPerRow", "Filters Per Row", @"The number of filters to display per row. Maximum is 12.", 0, @"2", "FF78F581-A1E8-41B4-96A0-70DCDC996A82" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "Redirect Page", @"If set, will redirect to the selected page when applying filters.", 0, @"", "0C59A9E4-017C-4675-88E0-AC8428DA2501" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Selection Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Selection Action", "DoesSelectionCausePostback", "Apply Filters Upon Selection", @"Describes the action to take when a non-textbox filter is selected by the individual. If ""Apply Filters"", all filters are applied instantly without the need to click the filter button. If ""Update Filters"", any filters whose available values rely on the selected values of other filters will be updated, but the user must click the filter button to apply them. If ""Do Nothing"", no updates happen, and the user must click the button to apply filters.", 0, @"0", "80239FA8-6643-4529-B7C0-F36E1B2304E4" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Enable Legacy Reload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Legacy Reload", "EnableLegacyReload", "Enable Legacy Reload", @"If enabled, a full page reload will be triggered to apply the filter selections (helpful when using this block to drive the behavior of legacy blocks on the page). If disabled, the filter selections will be communicated directly to any Obsidian blocks listening for these filters, so they can respond accordingly.", 0, @"False", "FA6FFA32-FC5D-44C0-B54C-FB35E4209156" );
        }

        /// <summary>
        /// JPH: Add the obsidian page parameter filter block type and attributes down.
        /// </summary>
        private void AddObsidianPageParameterFilterBlockTypeDown()
        {
            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Enable Legacy Reload
            RockMigrationHelper.DeleteAttribute( "FA6FFA32-FC5D-44C0-B54C-FB35E4209156" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Selection Action
            RockMigrationHelper.DeleteAttribute( "80239FA8-6643-4529-B7C0-F36E1B2304E4" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Redirect Page
            RockMigrationHelper.DeleteAttribute( "0C59A9E4-017C-4675-88E0-AC8428DA2501" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filters Per Row
            RockMigrationHelper.DeleteAttribute( "FF78F581-A1E8-41B4-96A0-70DCDC996A82" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Reset Filters Button
            RockMigrationHelper.DeleteAttribute( "82327B97-8A4F-4743-93CF-2E32FF5A8097" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.DeleteAttribute( "B85336F2-6F40-4C7B-980D-1F957C4CE167" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Size
            RockMigrationHelper.DeleteAttribute( "F6FFA298-9434-4BA8-9863-051E4DD52994" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Text
            RockMigrationHelper.DeleteAttribute( "EFA46D88-B9A8-422C-B4A5-ADDA5CF13301" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Block Title
            RockMigrationHelper.DeleteAttribute( "54DCAD13-576B-475D-A6D2-5ABA57639212" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "7E543C23-6FB8-4538-9249-261C35DE1491" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Text
            RockMigrationHelper.DeleteAttribute( "8B424C65-1C4D-4A47-AFF6-64C99F777E6A" );

            // Delete BlockType
            //   Name: Page Parameter Filter
            //   Category: Reporting
            //   Path: -
            //   EntityType: Page Parameter Filter
            RockMigrationHelper.DeleteBlockType( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.DeleteEntityType( "59F94307-B2B0-4383-9C2C-88A4E154C461" );
        }
    }
}
