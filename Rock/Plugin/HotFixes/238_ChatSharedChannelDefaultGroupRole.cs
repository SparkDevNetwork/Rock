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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 238, "1.17.0" )]
    public class ChatSharedChannelDefaultGroupRole : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            JPH_AddChatSharedChannelDefaultGroupRole_20250324_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// JPH: Add "Chat Shared Channel" default group role - up.
        /// </summary>
        private void JPH_AddChatSharedChannelDefaultGroupRole_20250324_Up()
        {
            // Update previously-seeded "Chat Shared Channel" group type.
            // We're reassigning the default group role that was wiped out in a recent migration.
            RockMigrationHelper.UpdateGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat shared channel is created within Rock.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                defaultGroupRoleGuid: "E133E458-9785-4C59-B844-35E0C3B686D9",
                allowMultipleLocations: false,
                showInGroupList: true,
                showInNavigation: true,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true
            );
        }
    }
}
