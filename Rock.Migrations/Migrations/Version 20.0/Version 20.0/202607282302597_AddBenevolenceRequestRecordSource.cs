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

    /// <summary>
    ///
    /// </summary>
    public partial class AddBenevolenceRequestRecordSource : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Benevolence Request",
                description: "Describes a record that was created from a benevolence request.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_BENEVOLENCE_REQUEST,
                isSystem: true
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "34275D0E-BC7E-4A9C-913E-623D086159A1", // Benevolence Request Detail (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Benevolence Request'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 8,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_BENEVOLENCE_REQUEST,
                guid: "35B6746E-2A0E-4646-81E0-5523B424CAF3"
            );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "35B6746E-2A0E-4646-81E0-5523B424CAF3" ); // Benevolence Request Detail (Obsidian)

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_BENEVOLENCE_REQUEST );
        }
    }
}
