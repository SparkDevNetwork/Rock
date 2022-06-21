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
    public partial class CodeGenerated_20220426 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Show Campus Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Type", "ShowCampusType", "Show Campus Type", @"Display the campus type.", 16, @"True", "EE39054C-E6FA-498B-88C5-A31BBF4287A3" );

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Show Campus Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "463A454A-6370-4B4A-BCA1-415F2D9B0CB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Status", "ShowCampusStatus", "Show Campus Status", @"Display the campus status.", 17, @"True", "9AC1F3C5-AAFC-4E81-8C4B-E1B80DDB8777" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "DA3AA23E-D2A6-4141-820E-10FB1EB0FC60" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "45A4DD14-9023-47DF-9809-8603E015D175" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "63117BF8-01FB-477A-95C6-92A841F62255" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "CCF221CB-F5FC-4C83-9FCD-561036DF04FD" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "1023516E-01E0-46A5-90A8-E47085536E53" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "06C45E83-5F84-4CBB-A681-E9F41A23B566" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "F01C2E8D-C67A-44FC-A536-4AEE4C733664" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A902DA02-4261-4C7A-BFD6-8D2159CC5B5A", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "1FBA87B5-B461-439F-A122-8E4D02F00FDD" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter Button", "ShowFilterButton", "Show Filter Button", @"Shows or hides the filter buttons. This is useful when the Selection action is set to reload the page. Be sure to use this only when the page re-load will be quick.", 10, @"True", "F24169AC-2A39-476D-B519-7D2503D83298" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.DeleteAttribute("F24169AC-2A39-476D-B519-7D2503D83298");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("1FBA87B5-B461-439F-A122-8E4D02F00FDD");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("F01C2E8D-C67A-44FC-A536-4AEE4C733664");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("06C45E83-5F84-4CBB-A681-E9F41A23B566");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("1023516E-01E0-46A5-90A8-E47085536E53");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("CCF221CB-F5FC-4C83-9FCD-561036DF04FD");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("63117BF8-01FB-477A-95C6-92A841F62255");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("45A4DD14-9023-47DF-9809-8603E015D175");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("DA3AA23E-D2A6-4141-820E-10FB1EB0FC60");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Show Campus Status
            RockMigrationHelper.DeleteAttribute("9AC1F3C5-AAFC-4E81-8C4B-E1B80DDB8777");

            // Attribute for BlockType
            //   BlockType: Family Pre Registration
            //   Category: CRM
            //   Attribute: Show Campus Type
            RockMigrationHelper.DeleteAttribute("EE39054C-E6FA-498B-88C5-A31BBF4287A3");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("A902DA02-4261-4C7A-BFD6-8D2159CC5B5A");
        }
    }
}
