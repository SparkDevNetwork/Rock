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
    public partial class UpdateSecuritySettingsWithWellknownGuid : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateSecuritySettingsGuidUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Update system security settings with well-known Guid.
        /// </summary>
        private void UpdateSecuritySettingsGuidUp()
        {
            Sql( $@"
            UPDATE Attribute
                SET [Guid] = '{SystemGuid.Attribute.SYSTEM_SECURITY_SETTINGS}'
                WHERE [EntityTypeQualifierColumn] = '{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}'
                AND [Key] = '{Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS}'
                AND [Guid] != '{SystemGuid.Attribute.SYSTEM_SECURITY_SETTINGS}' " );
        }
    }
}
