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
    public partial class PersonPagesNotesBlockUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block Attribute Value
            //   Block: TimeLine
            //   BlockType: Notes
            //   Category: Core
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Add Always Visible
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue("0B2B550C-B0C9-420E-9CF3-BEC8979108F2","8E0BDD15-6B92-4BB0-9138-E9382B60F3A9",@"True");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
