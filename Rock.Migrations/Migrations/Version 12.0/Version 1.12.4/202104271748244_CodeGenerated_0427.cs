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
    public partial class CodeGenerated_0427 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType: Dynamic Data:Panel Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "Panel Title", @"The title of the panel.", 0, @"", "36BC9741-C4BA-4B1E-BE1C-E64491781900" );

            // Attribute for BlockType: Dynamic Data:Panel Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Icon CSS Class", "PanelTitleCssClass", "Panel Icon CSS Class", @"The CSS Class to use in the panel title.", 0, @"", "28208FC3-BB71-4400-AF3A-862167D3E493" );

            // Attribute for BlockType: Dynamic Data:Wrap In Panel
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Wrap In Panel", "WrapInPanel", "Wrap In Panel", @"This will wrap the results grid in a panel.", 0, @"False", "132C3810-0D6B-4DD7-BE4C-287E6769A274" );

            // Attribute for BlockType: Transaction Detail:Enable Foreign Currency
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Foreign Currency", "EnableForeignCurrency", "Enable Foreign Currency", @"Shows the transaction's currency code field if enabled.", 6, @"False", "9C1BFEFF-4F35-46DB-922D-866A8065A2D3" );

            // Attribute for BlockType: Transaction List:Enable Foreign Currency
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Foreign Currency", "EnableForeignCurrency", "Enable Foreign Currency", @"Shows the transaction's currency code field if enabled.", 12, @"False", "6A9AE0A2-5AC7-47E1-9485-DAD55DBA179F" );

            // Attribute for BlockType: Family Pre Registration:Scheduled Days Ahead
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Scheduled Days Ahead", "ScheduledDaysAhead", "Scheduled Days Ahead", @"When using campus specific scheduling this setting determines how many days ahead a person can select. The default is 28 days.", 4, @"28", "17D77C56-B6C8-463E-86F1-0FED10A3FE41" );

            // Attribute for BlockType: Family Pre Registration:Campus Schedule Attribute
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Campus Schedule Attribute", "CampusScheduleAttribute", "Campus Schedule Attribute", @"Allows you select a campus attribute that contains schedules for determining which dates and times for which pre-registration is available. This requries the creation of an Entity attribute for 'Campus' using a Field Type of 'Schedules'. The schedules can then be selected in the 'Edit Campus' block.", 3, @"", "8712D8D2-76C7-43BF-BEB5-7251CCDBD907" );

            // Attribute for BlockType: En Route:Filter By
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter By", "FilterBy", "Filter By", @"", 1, @"2", "FE3C4DB6-C4BB-497E-8619-6FC474515D5C" );

            // Attribute for BlockType: En Route:Show Only Parent Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Parent Group", "ShowOnlyParentGroup", "Show Only Parent Group", @"When enabled, the parent group and path for each check-in, instead of the actual group.", 2, @"False", "5BDA12AB-4282-4A3D-B4EE-FEB4A2749EDC" );

            // Attribute for BlockType: En Route:Always Show Child Groups
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Show Child Groups", "AlwaysShowChildGroups", "Always Show Child Groups", @"When enabled, all child groups of the selected group will be included in the filter. Otherwise, a 'Include Child Groups' option will 
 be displayed to include child groups.", 3, @"False", "CB651341-DE0B-48FF-BB11-7A96E6C3B787" );
            RockMigrationHelper.UpdateFieldType("Media Element","","Rock","Rock.Field.Types.MediaElementFieldType","3DC3DCEA-0912-4148-ADF2-81880000DC79");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Panel Icon CSS Class Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("28208FC3-BB71-4400-AF3A-862167D3E493");

            // Panel Title Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("36BC9741-C4BA-4B1E-BE1C-E64491781900");

            // Wrap In Panel Attribute for BlockType: Dynamic Data
            RockMigrationHelper.DeleteAttribute("132C3810-0D6B-4DD7-BE4C-287E6769A274");

            // Enable Foreign Currency Attribute for BlockType: Transaction List
            RockMigrationHelper.DeleteAttribute("6A9AE0A2-5AC7-47E1-9485-DAD55DBA179F");

            // Enable Foreign Currency Attribute for BlockType: Transaction Detail
            RockMigrationHelper.DeleteAttribute("9C1BFEFF-4F35-46DB-922D-866A8065A2D3");

            // Always Show Child Groups Attribute for BlockType: En Route
            RockMigrationHelper.DeleteAttribute("CB651341-DE0B-48FF-BB11-7A96E6C3B787");

            // Show Only Parent Group Attribute for BlockType: En Route
            RockMigrationHelper.DeleteAttribute("5BDA12AB-4282-4A3D-B4EE-FEB4A2749EDC");

            // Filter By Attribute for BlockType: En Route
            RockMigrationHelper.DeleteAttribute("FE3C4DB6-C4BB-497E-8619-6FC474515D5C");

            // Scheduled Days Ahead Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("17D77C56-B6C8-463E-86F1-0FED10A3FE41");

            // Campus Schedule Attribute Attribute for BlockType: Family Pre Registration
            RockMigrationHelper.DeleteAttribute("8712D8D2-76C7-43BF-BEB5-7251CCDBD907");
        }
    }
}
