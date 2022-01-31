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
    public partial class CodeGenerated_20211102 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Type
            //   Name:Control Gallery
            //   Category:Obsidian > Example
            //   EntityType:Rock.Blocks.Example.ControlGallery
            RockMigrationHelper.UpdateMobileBlockType("Control Gallery", "Allows the user to try out various controls.", "Rock.Blocks.Example.ControlGallery", "Obsidian > Example", "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0");

            // Add/Update Obsidian Block Type
            //   Name:Field Type Gallery
            //   Category:Obsidian > Example
            //   EntityType:Rock.Blocks.Example.FieldTypeGallery
            RockMigrationHelper.UpdateMobileBlockType("Field Type Gallery", "Allows the user to try out various field types.", "Rock.Blocks.Example.FieldTypeGallery", "Obsidian > Example", "50B7B326-8212-44E6-8CF6-515B1FF75A19");

            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Database Timeout
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeoutSeconds", "Database Timeout", @"The number of seconds to wait before reporting a database timeout.", 15, @"180", "F1DE6A0D-A1BE-494F-BC0E-1CAE42AED897" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Attendance Analytics
            //   Category: Check-in
            //   Attribute: Database Timeout
            RockMigrationHelper.DeleteAttribute("F1DE6A0D-A1BE-494F-BC0E-1CAE42AED897");

            // Delete BlockType 
            //   Name: Field Type Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Field Type Gallery
            RockMigrationHelper.DeleteBlockType("50B7B326-8212-44E6-8CF6-515B1FF75A19");

            // Delete BlockType 
            //   Name: Control Gallery
            //   Category: Obsidian > Example
            //   Path: -
            //   EntityType: Control Gallery
            RockMigrationHelper.DeleteBlockType("6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0");
        }
    }
}
