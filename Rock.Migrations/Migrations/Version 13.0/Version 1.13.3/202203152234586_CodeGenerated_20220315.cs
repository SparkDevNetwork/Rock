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
    public partial class CodeGenerated_20220315 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "442D227E-5E89-4DC1-B764-BC3DF224E048");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Comment Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Field Label", "CommentFieldLabel", "Comment Field Label", @"The label to apply to the comment field.", 11, @"Comments", "A446BABC-4658-4533-8F2E-8AE4BEB7339B" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "DF0EEF8C-7400-4251-B07E-CEFDAE7EFB33" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "461CB614-0601-4E2C-952C-EE4D5BCD35A7" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "053EA026-6E5B-4B56-A7A1-309BA4A62D9F" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "7F977D29-63CC-4D29-AF74-39CA7A6E542C" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "07169CA0-33B2-44B8-BD2C-903F6D803A18" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "F62088B2-80EF-4EC7-B9DA-2C695CC408C3" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "6220C5A0-DCCC-4E9B-A46D-68872441755A" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "442D227E-5E89-4DC1-B764-BC3DF224E048", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "51795178-602E-46DF-9BE6-36A1703DA533" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Update Page Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2E0E4E3-30B1-45BD-B808-C55BCD540894", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page Title", "UpdatePageTitle", "Update Page Title", @"Updates the page title with the connection type name.", 3, @"False", "A6F10CBE-9CFC-4714-9293-666FE62325E4" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Update Page Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Update Page Title", "UpdatePageTitle", "Update Page Title", @"Updates the page title with the opportunity name.", 4, @"False", "5821942B-E5F1-4BEE-85AD-150BF9380656" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Update Page Title
            RockMigrationHelper.DeleteAttribute("5821942B-E5F1-4BEE-85AD-150BF9380656");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Update Page Title
            RockMigrationHelper.DeleteAttribute("A6F10CBE-9CFC-4714-9293-666FE62325E4");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("51795178-602E-46DF-9BE6-36A1703DA533");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("6220C5A0-DCCC-4E9B-A46D-68872441755A");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("F62088B2-80EF-4EC7-B9DA-2C695CC408C3");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("07169CA0-33B2-44B8-BD2C-903F6D803A18");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("7F977D29-63CC-4D29-AF74-39CA7A6E542C");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("053EA026-6E5B-4B56-A7A1-309BA4A62D9F");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("461CB614-0601-4E2C-952C-EE4D5BCD35A7");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("DF0EEF8C-7400-4251-B07E-CEFDAE7EFB33");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Comment Field Label
            RockMigrationHelper.DeleteAttribute("A446BABC-4658-4533-8F2E-8AE4BEB7339B");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("442D227E-5E89-4DC1-B764-BC3DF224E048");
        }
    }
}
