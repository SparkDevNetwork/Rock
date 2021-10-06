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
    public partial class Rollup_20210921 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateSampleDataUrlUp();
            PageRouteFix();
            InsertObsidianBlocks();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateSampleDataUrlDown();
        }

        /// <summary>
        /// NA: v13 Update SampleData URL
        /// </summary>
        private void UpdateSampleDataUrlUp()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_13_0.xml" );
        }

        /// <summary>
        /// NA: v13 Update SampleData URL
        /// </summary>
        private void UpdateSampleDataUrlDown()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_7_0.xml" );
        }

        /// <summary>
        /// GJ: Page Route Fix
        /// </summary>
        private void PageRouteFix()
        {
            Sql( MigrationSQL._202109212048053_Rollup_20210921_routemigration );
        }

        private void InsertObsidianBlocks()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationEntry", "Registration Entry", "Rock.Blocks.Event.RegistrationEntry, Rock.Blocks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.OBSIDIAN_EVENT_REGISTRATION_ENTRY );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Example.ControlGallery", "Control Gallery", "Rock.Blocks.Example.ControlGallery, Rock.Blocks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_CONTROL_GALLERY );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Example.FieldTypeGallery", "Field Type Gallery", "Rock.Blocks.Example.FieldTypeGallery, Rock.Blocks, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_FIELD_TYPE_GALLERY );

            RockMigrationHelper.UpdateMobileBlockType( "Registration Entry", "Block used to register for a registration instance.", "Rock.Blocks.Event.RegistrationEntry", "Obsidian > Event", Rock.SystemGuid.BlockType.OBSIDIAN_EVENT_REGISTRATION_ENTRY );
            RockMigrationHelper.UpdateMobileBlockType( "Control Gallery", "Allows the user to try out various controls.", "Rock.Blocks.Event.ControlGallery", "Obsidian > Example", Rock.SystemGuid.BlockType.OBSIDIAN_EXAMPLE_CONTROL_GALLERY );
            RockMigrationHelper.UpdateMobileBlockType( "Field Type Gallery", "Allows the user to try out various field types.", "Rock.Blocks.Event.FieldTypeGallery", "Obsidian > Example", Rock.SystemGuid.BlockType.OBSIDIAN_EXAMPLE_FIELD_TYPE_GALLERY );
        }
    }
}
