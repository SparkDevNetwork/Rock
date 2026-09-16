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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Seeds the hidden Chat People marker group. Same shape as the Chat Ban
    /// List: Application Group type, system, not a security role, group history
    /// off (the type does not enable it). Chat is forced off so the group is
    /// never treated as a channel.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 323, "21.0" )]
    public class AddChatPeopleGroup : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP,
                name: "Chat People",
                description: "Marker for people who have opened chat or otherwise received a durable stamp on the chat platform. Membership is sticky and is not a security role.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            Sql( $@"
UPDATE [Group]
SET [IsChatEnabledOverride] = 0
WHERE [Guid] = '{Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE}';
" );
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
