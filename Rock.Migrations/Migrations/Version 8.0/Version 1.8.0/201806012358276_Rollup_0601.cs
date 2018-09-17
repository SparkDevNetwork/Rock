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
    public partial class Rollup_0601 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "E9DFA80F-1895-46A9-90DE-C88F21913F33" );
            // Attrib for BlockType: Content Channel View:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 3, @"", "BDE15D65-BF32-448F-9FFC-9DD58ACC475E" );
            // Attrib for BlockType: Group List Personalized Lava:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B172C33-8672-4C98-A995-8E123FF316BD", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 9, @"", "A224FB9E-B7E6-4D48-95E6-A79067378C03" );
            // Attrib for BlockType: Internal Communication View:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "D526F4A5-19B9-410F-A663-400D93C61D3C", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "E18B1B45-1BD8-49FE-A156-D0A29A4D72BD" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group List Personalized Lava:Cache Tags
            RockMigrationHelper.DeleteAttribute( "A224FB9E-B7E6-4D48-95E6-A79067378C03" );
            // Attrib for BlockType: Content Channel View:Cache Tags
            RockMigrationHelper.DeleteAttribute( "BDE15D65-BF32-448F-9FFC-9DD58ACC475E" );
            // Attrib for BlockType: Internal Communication View:Cache Tags
            RockMigrationHelper.DeleteAttribute( "E18B1B45-1BD8-49FE-A156-D0A29A4D72BD" );
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.DeleteAttribute( "E9DFA80F-1895-46A9-90DE-C88F21913F33" );
        }
    }
}
