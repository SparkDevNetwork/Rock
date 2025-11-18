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
    public partial class AddServingConnectionRecordSource : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Serving Connection",
                description: "Describes a record that was created from a serving connection.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SERVING_CONNECTION,
                isSystem: true
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", // Connection Opportunity Signup (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Serving Connection'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 6,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SERVING_CONNECTION,
                guid: "A8C9E2C2-3D4F-45CD-9B43-A0FFA750915A"
            );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "A8C9E2C2-3D4F-45CD-9B43-A0FFA750915A" ); // Connection Opportunity Signup (Obsidian)

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SERVING_CONNECTION );
        }
    }
}
