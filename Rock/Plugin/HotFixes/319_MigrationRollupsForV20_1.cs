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
    /// Rollup of migrations for v20.1: adds the kiosk "Skip Screen Behavior" Device attribute
    /// and renames the "My Contact" block type to "My Contacts".
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 319, "20.1" )]
    public class MigrationRollupsForV20_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DH_AddSkipScreenBehaviorKioskAttribute_Up();
            PS_RenameMyContactsBlockType_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PS_RenameMyContactsBlockType_Down();
        }

        #region DH_AddSkipScreenBehaviorKioskAttribute

        /// <summary>
        /// DH: Adds the new attribute to the Device entity to control the behavior of the skip screen in kiosks.
        /// </summary>
        public void DH_AddSkipScreenBehaviorKioskAttribute_Up()
        {
            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device",
                SystemGuid.FieldType.SINGLE_SELECT,
                "DeviceTypeValueId",
                "41", // kiosk
                "Skip Screen Behavior",
                "Determines when the 'skip' screen is shown when an attendee has no valid check-in option. 'Never Show' quietly skips the attendee (labels for other family members may print before you realize this person had no option). 'Show When Needed' skips only when the attendee could never check in, but shows the screen when a room exists yet is unavailable (e.g. full). 'Always Show' always displays the screen so the operator can confirm the skip.",
                1047,
                "0", // Show When Needed
                "ABD069C5-472E-4A5D-93F9-73724D4EA99F",
                "core_device_KioskSkipScreenBehavior",
                false );

            RockMigrationHelper.AddAttributeQualifier( "ABD069C5-472E-4A5D-93F9-73724D4EA99F",
                "values",
                "1^Never Show,0^Show When Needed,2^Always Show",
                "ED17061D-B042-47AD-9D38-8EC45412A90F" );

            RockMigrationHelper.AddAttributeQualifier( "ABD069C5-472E-4A5D-93F9-73724D4EA99F",
                "fieldtype",
                "ddl",
                "72667DC5-C8FB-4BC7-862D-94EBD5F60669" );
        }

        #endregion DH_AddSkipScreenBehaviorKioskAttribute

        #region PS_RenameMyContactsBlockType

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        /// <remarks>
        /// A plain UPDATE is used intentionally. RockMigrationHelper.RenameBlockType()
        /// keys off [Path] and leads with a DELETE, which does not apply to entity-based
        /// block types. Rock's start-up registration only rewrites [Name] for block types
        /// it has not seen before, so an already-installed database needs this.
        /// </remarks>
        public void PS_RenameMyContactsBlockType_Up()
        {
            Sql( $@"
                UPDATE [BlockType]
                SET [Name] = 'My Contacts'
                WHERE [Guid] = '{Rock.SystemGuid.BlockType.MOBILE_OUTREACH_MY_CONTACTS}'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public void PS_RenameMyContactsBlockType_Down()
        {
            Sql( $@"
                UPDATE [BlockType]
                SET [Name] = 'My Contact'
                WHERE [Guid] = '{Rock.SystemGuid.BlockType.MOBILE_OUTREACH_MY_CONTACTS}'" );
        }

        #endregion PS_RenameMyContactsBlockType
    }
}
