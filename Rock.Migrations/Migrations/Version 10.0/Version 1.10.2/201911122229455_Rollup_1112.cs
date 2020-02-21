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
    public partial class Rollup_1112 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Connection Request Detail:Lava Badge Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Badge Bar", "LavaBadgeBar", "Lava Badge Bar", @"The HTML Content intended to be used as a kind of custom badge bar for the connection request. <span class='tip tip-lava'></span>", 7, @"", "120E931F-0245-4183-A95D-49094D85FD6F" );
            // Attrib for BlockType: Connection Request Detail:Lava Heading Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Heading Template", "LavaHeadingTemplate", "Lava Heading Template", @"The HTML Content to render above the person’s name. <span class='tip tip-lava'></span>", 6, @"", "C73A4952-F2A5-4326-939C-4F2F693ADB68" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Connection Request Detail:Lava Heading Template
            RockMigrationHelper.DeleteAttribute( "C73A4952-F2A5-4326-939C-4F2F693ADB68" );
            // Attrib for BlockType: Connection Request Detail:Lava Badge Bar
            RockMigrationHelper.DeleteAttribute( "120E931F-0245-4183-A95D-49094D85FD6F" );
        }
    }
}
