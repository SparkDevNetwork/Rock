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
    public partial class CodeGenerated_20220412 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "D733A2AE-C9CF-4ADD-8B75-03D637E71686");

            // Attribute for BlockType
            //   BlockType: HTML Content
            //   Category: CMS
            //   Attribute: Validate Markup
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Validate Markup", "ValidateMarkup", "Validate Markup", @"If enabled the HTML markup will be validated to ensure there are no mis-matched tags.", 11, @"True", "6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Entry Page", "EntryPage", "Entry Page", @"Page used to launch a new workflow of the selected type.", 4, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "CB28293D-2013-4745-8972-6694E150C59E" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "2659FEEE-3E1D-4117-8624-6BDC250C61B1" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "3CD53834-9E48-4E36-904C-35ECAEAB4568" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "9D537427-2FF3-483C-A40F-2A03EA2B3B53" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "B19F2A14-9A30-4080-9264-40206F5A3E35" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "45C7FD53-F5A0-42A1-B4FF-1DD4C8F05A02" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "28347AEE-6AFB-4E02-8A66-53D674C74AA0" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "D66073BD-EE49-4B9B-9E3F-4993B3CB479B" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D733A2AE-C9CF-4ADD-8B75-03D637E71686", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "2A72D681-9A14-4A2A-B35E-729357009338" );

            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66B2B513-1C71-4E6B-B4BE-C4EF90E1899C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "Gender", "Gender", @"Determines if the Gender field should be hidden, optional or required.", 10, @"2", "D9AAF055-24B9-4BF5-A2A3-2405993D9010" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Profile Details
            //   Category: Mobile > Cms
            //   Attribute: Gender
            RockMigrationHelper.DeleteAttribute("D9AAF055-24B9-4BF5-A2A3-2405993D9010");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("2A72D681-9A14-4A2A-B35E-729357009338");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("D66073BD-EE49-4B9B-9E3F-4993B3CB479B");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("28347AEE-6AFB-4E02-8A66-53D674C74AA0");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("45C7FD53-F5A0-42A1-B4FF-1DD4C8F05A02");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("B19F2A14-9A30-4080-9264-40206F5A3E35");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("9D537427-2FF3-483C-A40F-2A03EA2B3B53");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("3CD53834-9E48-4E36-904C-35ECAEAB4568");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("2659FEEE-3E1D-4117-8624-6BDC250C61B1");

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Entry Page
            RockMigrationHelper.DeleteAttribute("CB28293D-2013-4745-8972-6694E150C59E");

            // Attribute for BlockType
            //   BlockType: HTML Content
            //   Category: CMS
            //   Attribute: Validate Markup
            RockMigrationHelper.DeleteAttribute("6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("D733A2AE-C9CF-4ADD-8B75-03D637E71686");
        }
    }
}
