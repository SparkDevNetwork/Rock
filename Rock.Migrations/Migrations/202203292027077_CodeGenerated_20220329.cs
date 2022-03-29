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
    public partial class CodeGenerated_20220329 : Rock.Migrations.RockMigration
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
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72");

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3497603B-3BE6-4262-B7E9-EC01FC7140EB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 8, @"180", "C82CD1ED-EB0B-463F-8C72-B8C8698D4166" );

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Require Complete Birth Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Complete Birth Date", "RequireCompleteBirthDate", "Require Complete Birth Date", @"If set to true, the year portion for the birth date will be required if there are values set in the day and month parts.", 3, @"False", "7D570D95-A0A7-4723-B090-4B39FB89B207" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "2214CEFA-9B99-4F69-AD4D-52FEC76459C0" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "DC0102E4-308D-4B5D-AE71-3C07DC7C8217" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "C3E3AE36-3C80-479B-8B54-E48F96872184" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "A2005C0B-44A5-491D-9FFE-528CA2AF6BBA" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "C9FE4002-9F53-45D7-B860-34C2BF57FE61" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "C094891E-E1E4-4B16-A71E-44790FEA0AC3" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "B5071147-9B4D-401B-AA85-18B61C840D80" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "6CFE3AAA-3153-4AC8-8D29-B14517DBDB4F" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("6CFE3AAA-3153-4AC8-8D29-B14517DBDB4F");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("B5071147-9B4D-401B-AA85-18B61C840D80");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("C094891E-E1E4-4B16-A71E-44790FEA0AC3");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("C9FE4002-9F53-45D7-B860-34C2BF57FE61");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("A2005C0B-44A5-491D-9FFE-528CA2AF6BBA");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("C3E3AE36-3C80-479B-8B54-E48F96872184");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("DC0102E4-308D-4B5D-AE71-3C07DC7C8217");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("2214CEFA-9B99-4F69-AD4D-52FEC76459C0");

            // Attribute for BlockType
            //   BlockType: Edit Person
            //   Category: CRM > Person Detail
            //   Attribute: Require Complete Birth Date
            RockMigrationHelper.DeleteAttribute("7D570D95-A0A7-4723-B090-4B39FB89B207");

            // Attribute for BlockType
            //   BlockType: SMS Conversations
            //   Category: Communication
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute("C82CD1ED-EB0B-463F-8C72-B8C8698D4166");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("518A2E7F-0ACC-4605-ADDD-5E34C2BD4E72");
        }
    }
}
