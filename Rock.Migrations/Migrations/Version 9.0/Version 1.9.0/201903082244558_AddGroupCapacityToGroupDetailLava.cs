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
    public partial class AddGroupCapacityToGroupDetailLava : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Group Detail Lava:Enable Group Capacity Edit
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "", "Enables changing Group Capacity when editing a group. Note: The group type must have a 'Group Capacity Rule'.", 11, @"False", "D6F86AAC-D6E5-4167-BBCC-75DF88112027" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Detail Lava:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute( "D6F86AAC-D6E5-4167-BBCC-75DF88112027" );
        }
    }
}
