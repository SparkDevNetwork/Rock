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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 35, "1.6.9" )]
    public class TransactionListSummary : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "", "Should the account summary be displayed at the bottom of the list?", 6, @"False", "4C92974B-FB99-4E89-B215-A457646D77E1" );
            RockMigrationHelper.AddBlockAttributeValue( "B447AB11-3A19-4527-921A-2266A6B4E181", "4C92974B-FB99-4E89-B215-A457646D77E1", "True" );
            RockMigrationHelper.AddBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "4C92974B-FB99-4E89-B215-A457646D77E1", "True" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
