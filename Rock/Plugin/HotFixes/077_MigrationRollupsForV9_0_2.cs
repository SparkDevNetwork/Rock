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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 77, "1.9.0" )]
    public class MigrationRollupsForV9_0_2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // There were a couple of things from the old MoreMigrationRollupsForV7_3 hotfix migration that got missed 
            MoreMigrationRollupsForV7_3();
        }

        /// <summary>
        /// Mores the migration rollups for v7 3.
        /// </summary>
        private void MoreMigrationRollupsForV7_3()
        {
            // JE: Update Person Attribute to 'Suppress Sending Contribution Statements'
            Sql( @"  UPDATE [Attribute]
  SET [Name] = 'Suppress Sending Contribution Statements'
  WHERE [Guid] = 'B767F2CF-A4F0-45AA-A2E9-8270F31B307B'
" );
            // MP: Transaction Matching Batch Page
            // Attrib Value for Block:Transaction Matching, Attribute:Batch Detail Page Page: Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A18A0A0A-0B71-43B4-B830-44B802C272D4", "494C6487-8007-439F-BF0B-3F6020D159E8", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }
    }
}
