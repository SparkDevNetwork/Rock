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
using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add additional check-in settings.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 28, "1.6.6" )]
    public class BarcodeCheckin : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue( "", "Scanned Id", "Search for family based on a barcode, proximity card, etc.", "7668CE15-E372-47EE-8FF8-6FEE09F7C858", true );
            RockMigrationHelper.UpdateDefinedValue( "", "Family Id", "Search for family based on a Family Id", "111385BB-DAEB-4CE3-A945-0B50DC15EE02", true );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Group", Rock.SystemGuid.FieldType.VALUE_LIST, "GroupTypeId", "10", "Check-in Identifiers", "One or more identifiers such as a barcode, or proximity card value that can be used during check-in.", 0, "", "8F528431-A438-4488-8DC3-CA42E66C1B37", "CheckinId" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
