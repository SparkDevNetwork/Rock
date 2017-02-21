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
    [MigrationNumber( 18, "1.6.1" )]
    public class RestrictGroupRegistrationGroupTypes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Additional related fix for issue #1799 
            // Allow restriction by group type
            RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", Rock.SystemGuid.FieldType.GROUP_TYPES, "Allowed Group Types", "GroupTypes", "",
                "This setting restricts which types of groups a person can be added to, however selecting a specific group via the Group setting will override this restriction.",
                0, Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP, "1B7D5073-BDEB-4E73-94F1-920E5537A40D", isRequired: true );
            // Ability to disable passing in by GroupId
            RockMigrationHelper.UpdateBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Passing Group Id", "EnablePassingGroupId", "", "If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.", 0, @"True", "92767359-EFE2-4C7D-BA7F-90D901BDDEAE" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
