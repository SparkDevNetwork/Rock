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
    public partial class CodeGenerated_20240307 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "B3726171-604B-4A84-B4A3-4145E23372B6" );

            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Context Filter Attribute
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Filter Attribute", "ContextAttribute", "Context Filter Attribute", @"Item attribute to compare when filtering items using the block Context. If the block doesn't have a context, this setting will be ignored.", 0, @"", "F29C62E8-3746-446E-B8EC-158BAC9AD823" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Context Filter Attribute
            RockMigrationHelper.DeleteAttribute( "F29C62E8-3746-446E-B8EC-158BAC9AD823" );

            // Attribute for BlockType
            //   BlockType: Content Channel View
            //   Category: CMS
            //   Attribute: Entity Type
            RockMigrationHelper.DeleteAttribute( "B3726171-604B-4A84-B4A3-4145E23372B6" );
        }
    }
}
