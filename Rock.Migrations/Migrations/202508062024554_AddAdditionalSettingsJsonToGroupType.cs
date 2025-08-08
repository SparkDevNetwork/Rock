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
    public partial class AddAdditionalSettingsJsonToGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.GroupType", "AdditionalSettingsJson", c => c.String() );

            // Update order of the legacy Registration Mode attribute.
            Sql( @"
UPDATE [Attribute]
SET [Order] = 1046
WHERE [Guid] = '05f72e44-9d94-4c97-9458-13b038edeae3'
" );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device",
                SystemGuid.FieldType.SINGLE_SELECT,
                "DeviceTypeValueId",
                "41", // kiosk
                "Allow Adding Individuals To Existing Families",
                "Controls who can be added to an existing family during check-in, regardless of the Allow Editing Families setting.",
                1045,
                "0",
                "e6a95de0-c370-4ce7-83a7-738f6885f275",
                "core_device_KioskAllowAddingIndividualsToExistingFamilies",
                false );

            RockMigrationHelper.AddAttributeQualifier( "e6a95de0-c370-4ce7-83a7-738f6885f275",
                "values",
                "0^None,1^Adults & Children,2^Adults Only,3^Children Only",
                "d25d7906-d93e-4368-adf3-f1c45284a0bf" );

            RockMigrationHelper.AddAttributeQualifier( "e6a95de0-c370-4ce7-83a7-738f6885f275",
                "fieldtype",
                "ddl",
                "9d0f4ec9-b517-445b-b48d-be48d240db9f" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "e6a95de0-c370-4ce7-83a7-738f6885f275" );

            // Update order of the legacy Registration Mode attribute.
            Sql( @"
UPDATE [Attribute]
SET [Order] = 1045
WHERE [Guid] = '05f72e44-9d94-4c97-9458-13b038edeae3'
" );

            DropColumn( "dbo.GroupType", "AdditionalSettingsJson" );
        }
    }
}
