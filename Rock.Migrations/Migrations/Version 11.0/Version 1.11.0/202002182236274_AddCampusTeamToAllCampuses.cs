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
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddCampusTeamToAllCampuses : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add 'Campus Team' GroupType
            RockMigrationHelper.AddGroupType( "Campus Team", "Used to track groups that serve a given Campus.", "Group", "Member", false, false, false, null, 0, null, 0, null, SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM, true );

            // Add default Roles to 'Campus Team' GroupType
            RockMigrationHelper.AddGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM, "Pastor", "Pastor of a Campus", 0, 1, null, SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_PASTOR, true, true, false );
            RockMigrationHelper.AddGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM, "Administrator", "Administrator of a Campus", 1, null, null, SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_ADMINISTRATOR, true, false, true );

            // Add 'Group Member Detail' Page to 'Campus Detail' Page
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CAMPUS_DETAIL, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Group Member Detail", "", SystemGuid.Page.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL, "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL, "Campus/{CampusId}/GroupMember/{GroupMemberId}", SystemGuid.PageRoute.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL ); // for Page:Group Member Detail

            // Add Block to Page: Campus Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CAMPUS_DETAIL.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.GROUPS_GROUP_MEMBER_LIST.AsGuid(), "Group Member List", "Main", @"", @"", 1, SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( $@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '{SystemGuid.Block.CAMPUS_DETAIL_CAMPUS_DETAIL}'" );  // Page: Campus Detail,  Zone: Main,  Block: Campus Detail
            Sql( $@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '{SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST}'" );  // Page: Campus Detail,  Zone: Main,  Block: Group Member List

            // Attrib Value for Block:Group Member List, Attribute:Show Note Column Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "5F54C068-1418-44FA-B215-FBF70072F6A5", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Date Added Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "F281090E-A05D-4F81-AD80-A3599FB8E2CD", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Campus Filter Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "65B9EA6C-D904-4105-8B51-CCA784DDAAFA", @"True" );
            // Attrib Value for Block:Group Member List, Attribute:Show First/Last Attendance Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"eb135ae0-5bac-458b-ad5b-47460c2bfd31,9660b9fb-c90f-4afe-9d58-c0ec271c1377" );

            // Add Block to Page: Group Member Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.GROUPS_GROUP_MEMBER_DETAIL.AsGuid(), "Group Member Detail", "Main", @"", @"", 0, SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_DETAIL );

            // Seed all existing Campuses with a new 'TeamGroup' Group association
            Sql( RockMigrationSQL._202002182236274_AddCampusTeamToAllCampuses_Up );

            // Add 'PersonGetCampusTeamMember' Action to the EntityType table
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersonGetCampusTeamMember", SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER, false, true );

            // Add 'PersonGetCampusTeamMember' Action's Attributes
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Person", "Person", "Workflow attribute that contains the person to get the Campus team member for.", 0, @"", SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_PERSON );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Campus", "Campus", "Workflow attribute that contains the Campus to get the Campus team member for. If both Person and Campus are provided, Campus takes precedence over the Person's Campus. If Campus is not provided, the Person's primary Campus will be assigned to this attribute.", 1, @"", SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Campus Role", "CampusRole", "Workflow attribute that contains the Role of the Campus team member to get. If multiple team members are in this role for a given Campus, the first match will be selected.", 2, @"", SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_ROLE );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Campus Team Member", "CampusTeamMember", "Workflow attribute to assign the Campus team member to.", 3, @"", SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_TEAM_MEMBER );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove 'PersonGetCampusTeamMember' Action's Attributes
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_PERSON );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_ROLE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.WORKFLOW_ACTION_PERSON_GET_CAMPUS_TEAM_MEMBER_CAMPUS_TEAM_MEMBER );

            // Remove 'PersonGetCampusTeamMember' Action from the EntityType table
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.PERSON_GET_CAMPUS_TEAM_MEMBER );

            // Delete any Campus > Group assiations that were seeded as a part of the Up() method
            Sql( RockMigrationSQL._202002182236274_AddCampusTeamToAllCampuses_Down );

            // Remove Block: Group Member Detail, from Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_DETAIL );

            // Attrib Value for Block:Group Member List, Attribute:Show Note Column Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "5F54C068-1418-44FA-B215-FBF70072F6A5" );
            // Attrib Value for Block:Group Member List, Attribute:Show Date Added Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "F281090E-A05D-4F81-AD80-A3599FB8E2CD" );
            // Attrib Value for Block:Group Member List, Attribute:Show Campus Filter Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "65B9EA6C-D904-4105-8B51-CCA784DDAAFA" );
            // Attrib Value for Block:Group Member List, Attribute:Show First/Last Attendance Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664" );
            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST, "E4CCB79C-479F-4BEE-8156-969B2CE05973" );

            // Remove Block: Group Member List, from Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.CAMPUS_DETAIL_GROUP_MEMBER_LIST );

            // Remove 'Group Member Detail' Page from 'Campus Detail' Page
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL );
            RockMigrationHelper.DeletePage( SystemGuid.Page.GROUP_MEMBER_DETAIL_CAMPUS_DETAIL ); //  Page: Group Member Detail, Layout: Full Width, Site: Rock RMS

            // Remove default Roles from 'Campus Team' GroupType
            RockMigrationHelper.DeleteGroupTypeRole( SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_PASTOR );
            RockMigrationHelper.DeleteGroupTypeRole( SystemGuid.GroupRole.GROUPROLE_CAMPUS_TEAM_ADMINISTRATOR );

            // Remove 'Campus Team' GroupType
            RockMigrationHelper.DeleteGroupType( SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM );
        }
    }
}
