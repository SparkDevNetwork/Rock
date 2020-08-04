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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 130, "1.11.0" )]
    public class ServerCheckinPrinterCutter : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Device", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "DeviceTypeValueId", "43", "Has Cutter", "Used for indicating whether or not a printer has an attached auto-cutter. Checking this will cause Rock to perform automatic label cutting at the end of the label set (for server side label printing).", 1041, "False", "AF534D3F-CC48-4838-8FDB-9D88D61B307D", "core_device_HasCutter", false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
