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
    /// Makes changes necessary to support sign-up member and member opportunity attributes.
    /// Also corrects some breadcrumb issues introduced by the initial sign-ups feature implementation.
    /// </summary>
    public partial class AddSignUpAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGroupIdToGroupMemberAssignment_Up();
            HidePageNamesInBreadcrumbs_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddGroupIdToGroupMemberAssignment_Down();
            HidePageNamesInBreadcrumbs_Down();
        }

        /// <summary>
        /// Add Group ID to GroupMemberAssignment up.
        /// </summary>
        private void AddGroupIdToGroupMemberAssignment_Up()
        {
            AddColumn( "dbo.GroupMemberAssignment", "GroupId", c => c.Int( nullable: true ) );
            Sql( @"
-- Set all GroupMemberAssignment GroupIds to their current respective value.
UPDATE gma
SET gma.[GroupId] = g.[Id]
FROM [GroupMemberAssignment] gma
INNER JOIN [GroupMember] gm ON gm.[Id] = gma.[GroupMemberId]
INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
WHERE gma.[GroupId] IS NULL;" );

            AlterColumn( "dbo.GroupMemberAssignment", "GroupId", c => c.Int( nullable: false ) );
            AddForeignKey( "dbo.GroupMemberAssignment", "GroupId", "dbo.Group", "Id", cascadeDelete: true );
        }

        /// <summary>
        /// Add Group ID to GroupMemberAssignment down.
        /// </summary>
        private void AddGroupIdToGroupMemberAssignment_Down()
        {
            DropForeignKey( "dbo.GroupMemberAssignment", "GroupId", "dbo.Group" );
            DropColumn( "dbo.GroupMemberAssignment", "GroupId" );
        }

        /// <summary>
        /// Hide page names in breadcrumbs up.
        /// </summary>
        private void HidePageNamesInBreadcrumbs_Up()
        {
            Sql( @"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Guid] IN (
    '34212F8E-5F14-4D92-8B19-46748EBA2727'
    , 'AAF11844-EC6C-498B-A9D8-387390206570'
    , '05B79031-183F-4A64-A689-56B5C8E7519F'
);" );
        }

        /// <summary>
        /// Hide page names in breadcrumbs up.
        /// </summary>
        private void HidePageNamesInBreadcrumbs_Down()
        {
            Sql( @"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 1
WHERE [Guid] IN (
    '34212F8E-5F14-4D92-8B19-46748EBA2727'
    , 'AAF11844-EC6C-498B-A9D8-387390206570'
    , '05B79031-183F-4A64-A689-56B5C8E7519F'
);" );
        }
    }
}
