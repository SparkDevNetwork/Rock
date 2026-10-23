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
    /// Restricts View on system phone numbers to the administration and staff security roles.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 331, "17.8" )]
    public class AddSystemPhoneNumberViewSecurity : Migration
    {
        private const string SystemPhoneNumberEntityTypeName = "Rock.Model.SystemPhoneNumber";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSystemPhoneNumberViewAuthUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Allows View on the System Phone Number entity type for the administration and staff
        /// security roles, then denies View for all other users.
        /// </summary>
        /// <remarks>
        /// Each rule is appended after any entity type View rules that already exist,
        /// so rules an administrator has added keep their precedence.
        /// </remarks>
        private void AddSystemPhoneNumberViewAuthUp()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "C64A249D-6210-46C4-B06C-0E3C6EF494D1" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "E82E4FC1-B45C-4315-8D93-D75C91877F6A" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) Model.SpecialRole.None,
                "EB1F5BB8-96EA-4A4A-893C-65A5C2EC838D" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) Model.SpecialRole.None,
                "54087D1B-5808-4EDE-808D-3528279E2151" );

            // The helper's duplicate check compares GroupId with = and never matches a NULL group,
            // so check for an existing All Users rule here to avoid a duplicate or a Guid conflict.
            var allUsersRuleCount = SqlScalar( $@"
SELECT COUNT(*)
FROM [Auth] a
INNER JOIN [EntityType] et ON et.[Id] = a.[EntityTypeId]
WHERE et.[Name] = '{SystemPhoneNumberEntityTypeName}'
    AND a.[EntityId] = 0
    AND a.[Action] = '{Security.Authorization.VIEW}'
    AND a.[SpecialRole] = {( int ) Model.SpecialRole.AllUsers}
    AND a.[GroupId] IS NULL" ).ToStringSafe().AsInteger();

            if ( allUsersRuleCount == 0 )
            {
                RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                    int.MaxValue,
                    Security.Authorization.VIEW,
                    false,
                    null,
                    ( int ) Model.SpecialRole.AllUsers,
                    "30CC7B5E-7104-466E-A522-BD5AB9139C67" );
            }
        }
    }
}
