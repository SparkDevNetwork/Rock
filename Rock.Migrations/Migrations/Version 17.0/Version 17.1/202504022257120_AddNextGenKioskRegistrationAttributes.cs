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
    public partial class AddNextGenKioskRegistrationAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [Attribute]
SET [Name] = 'Registration Mode (Legacy Check-in)',
    [Description] = 'Kiosk serves the role of enrolling new families. This enables Add Family and Edit Family features during the check-in process. Only used by legacy check-in.',
    [Order] = 1045
WHERE [Guid] = '05f72e44-9d94-4c97-9458-13b038edeae3'
" );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device",
                SystemGuid.FieldType.BOOLEAN,
                "DeviceTypeValueId",
                "41", // kiosk
                "Allow Adding Families",
                "Used by next-gen check-in to determine if the kiosk allows new families to be added.",
                1043,
                "False",
                "8C1C9D50-A9CF-423C-990E-A60F4CF98265",
                "core_device_KioskAllowAddingFamilies",
                false );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device",
                SystemGuid.FieldType.BOOLEAN,
                "DeviceTypeValueId",
                "41", // kiosk
                "Allow Editing Families",
                "Used by next-gen check-in to determine if the kiosk allows existing families to be edited.",
                1044,
                "False",
                "DBE46EB5-0A84-4B58-A387-822AB747B7D0",
                "core_device_KioskAllowEditingFamilies",
                false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "DBE46EB5-0A84-4B58-A387-822AB747B7D0" );
            RockMigrationHelper.DeleteAttribute( "8C1C9D50-A9CF-423C-990E-A60F4CF98265" );

            Sql( @"
UPDATE [Attribute]
SET [Name] = 'Registration Mode',
    [Description] = 'Kiosk serves the role of enrolling new families. This enables Add Family and Edit Family features during the check-in process.',
    [Order] = 1042
WHERE [Guid] = '05f72e44-9d94-4c97-9458-13b038edeae3'
" );
        }
    }
}
