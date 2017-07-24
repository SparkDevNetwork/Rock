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
    [MigrationNumber( 32, "1.6.6" )]
    public class MigrationRollupsForV6_9 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // DT: Update American Express Value for Pushpay
            Sql( @"
    UPDATE [DefinedValue] SET [Value] = 'Amex' WHERE [Guid] = '696A54E3-352C-49FB-88A1-BCDBD81AA9EC'
" );
            // MP: CurrencyType Unknown
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Unknown", "The currency type is unknown. For example, it might have been imported from a system that doesn't indicate currency type.", "56C9AE9C-B5EB-46D5-9650-2EF86B14F856", false );

            // MP: Add Marital Statuses
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Divorced", "Used when the individual is divorced.", "3B689240-24C2-434B-A7B9-A4A6CBA7928C" );

            // JE - Typo Fix
            // Added to core: 201707241828445_PersonAliasNullableId
            // RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.BinaryFile", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "BinaryFileTypeId", "1", "Print For Each", "When a family checks in, should this label be printed once per family, person, or location. Note: this only applies if check-in is configured to use Family check-in vs Individual check-in.", 1, "1", "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "core_LabelType" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
