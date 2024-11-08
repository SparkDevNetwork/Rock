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
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddShortTermServingProjects : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ShortTermServingProjects_AddDefinedTypesAndValues();
            ShortTermServingProjects_AddGroups();
            ShortTermServingProjects_AddSystemCommunications();
            ShortTermServingProjects_AddAdminBlockTypes();
            ShortTermServingProjects_AddAdminPagesAndBlocks();
            ShortTermServingProjects_AddPublicBlockTypes();
            ShortTermServingProjects_AddPublicPagesAndBlocks();
            ShortTermServingProjects_UpdateExistingBlockValues();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ShortTermServingProjects_RevertChangesToExistingBlockValues();
            ShortTermServingProjects_DeletePublicPagesAndBlocks();
            ShortTermServingProjects_DeletePublicBlockTypes();
            ShortTermServingProjects_DeleteAdminPagesAndBlocks();
            ShortTermServingProjects_DeleteAdminBlockTypes();
            ShortTermServingProjects_DeleteSystemCommunications();
            ShortTermServingProjects_DeleteGroups();
            ShortTermServingProjects_DeleteDefinedTypesAndValues();
        }

        /// <summary>
        /// JPH: Add defined types and values needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddDefinedTypesAndValues()
        {
            RockMigrationHelper.AddDefinedType( "Group", "Project Type", "List of different types (In-Person, Project Due, etc.) of projects.", SystemGuid.DefinedType.PROJECT_TYPE, string.Empty );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PROJECT_TYPE, "In-Person", "The project happens on the configured date/time.", SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PROJECT_TYPE, "Project Due", "The project is due on the configured date/time.", SystemGuid.DefinedValue.PROJECT_TYPE_PROJECT_DUE );
        }

        /// <summary>
        /// JPH: Add group related records needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddGroups()
        {
            RockMigrationHelper.AddGroupType( "Sign-Up Group", "Used to track individuals who have signed up for events such as short term serving projects.", "Group", "Member", false, true, true, "fa fa-clipboard-check", 0, null, 3, null, SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );

            Sql( $"UPDATE [GroupType] SET [AllowedScheduleTypes] = 6 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );
            Sql( $"UPDATE [GroupType] SET [EnableGroupHistory] = 1 WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP}'" );

            RockMigrationHelper.UpdateGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, "Leader", "Indicates the person is a leader in the group.", 0, null, null, SystemGuid.GroupRole.GROUPROLE_SIGNUP_GROUP_LEADER, true, true, false );
            Sql( $"UPDATE [GroupTypeRole] SET [CanEdit] = 1, [CanManageMembers] = 1 WHERE [Guid] = '{SystemGuid.GroupRole.GROUPROLE_SIGNUP_GROUP_LEADER}';" );
            RockMigrationHelper.UpdateGroupTypeRole( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, "Member", "Indicates the person is a member in the group.", 1, null, null, SystemGuid.GroupRole.GROUPROLE_SIGNUP_GROUP_MEMBER, true, false, true );

            RockMigrationHelper.AddGroupTypeAssociation( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );

            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, SystemGuid.FieldType.DEFINED_VALUE, "Project Type", "The specified project type.", 0, string.Empty, SystemGuid.Attribute.GROUPTYPE_SIGNUP_GROUP_PROJECT_TYPE, true );

            RockMigrationHelper.AddDefinedTypeAttributeQualifier( SystemGuid.Attribute.GROUPTYPE_SIGNUP_GROUP_PROJECT_TYPE, SystemGuid.DefinedType.PROJECT_TYPE, "D49200BC-9E54-4906-9B20-53FD8973A43D" );

            RockMigrationHelper.UpdateGroup( null, SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP, "Sign-Up Groups", "Parent group for all sign-up groups.", null, 0, SystemGuid.Group.GROUP_SIGNUP_GROUPS );
        }

        /// <summary>
        /// JPH: Add system communications needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddSystemCommunications()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.SYSTEM_COMMUNICATION, "Sign-Up Group Confirmation", "fa fa-clipboard-check", string.Empty, SystemGuid.Category.SYSTEM_COMMUNICATION_SIGNUP_GROUP_CONFIRMATION );

            RockMigrationHelper.UpdateSystemCommunication(
                "Sign-Up Group Confirmation", // category
                "Sign-Up Group Reminder", // title
                string.Empty, // from
                string.Empty, // fromName
                string.Empty, // to
                string.Empty, // cc
                string.Empty, // bcc
                "Sign-Up Group Reminder for {{ OccurrenceTitle }} on {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}", // subject
                @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Sign-Up Group Reminder</h1>
<p>Hi {{  Person.NickName  }}!</p>
<p>This is a reminder that you have signed up for {{ OccurrenceTitle }} on {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}.</p>
<p>Thanks!</p>
<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>
{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.SIGNUP_GROUP_REMINDER, // guid
                true, // isActive
                "This is a reminder from {{ 'Global' | Attribute:'OrganizationName' }} that you have signed up for {{ OccurrenceTitle }} on {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}." // smsMessage
            );
        }

        /// <summary>
        /// JPH: Add admin block types needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddAdminBlockTypes()
        {
            RockMigrationHelper.UpdateBlockType( "Sign-Up Overview", "Displays an overview of sign-up projects with upcoming and recently-occurred opportunities.", "~/Blocks/Engagement/SignUp/SignUpOverview.ascx", "Engagement > Sign-Up", "B539F3B5-01D3-4325-B32A-85AFE2A9D18B" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Project Detail Page", "ProjectDetailPage", "Project Detail Page", "Page used for viewing details about the scheduled opportunities for a given project group. Clicking a row in the grid will take you to this page.", 0, "", "E306CB42-10FE-428C-A8B9-224BB7B30C6A" );
            Sql( "UPDATE [Attribute] SET [IsRequired] = 1 WHERE [Guid] = 'E306CB42-10FE-428C-A8B9-224BB7B30C6A';" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Sign-Up Opportunity Attendee List Page", "SignUpOpportunityAttendeeListPage", "Sign-Up Opportunity Attendee List Page", "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.", 1, "", "B86F9026-5B63-414C-A069-B5C86B8FEFC2" );

            RockMigrationHelper.UpdateBlockType( "Sign-Up Detail", "Displays details about the scheduled opportunities for a given project group.", "~/Blocks/Engagement/SignUp/SignUpDetail.ascx", "Engagement > Sign-Up", "69F5C6BD-7A22-42FE-8285-7C8E586E746A" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "69F5C6BD-7A22-42FE-8285-7C8E586E746A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Sign-Up Opportunity Attendee List Page", "SignUpOpportunityAttendeeListPage", "Sign-Up Opportunity Attendee List Page", "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.", 0, "", "525A1D90-CF46-4710-ADC3-86552EBB1E9C" );

            RockMigrationHelper.UpdateBlockType( "Sign-Up Opportunity Attendee List", "Lists all the group members for the selected group, location and schedule.", "~/Blocks/Engagement/SignUp/SignUpOpportunityAttendeeList.ascx", "Engagement > Sign-Up", "EE652767-5070-4EAB-8BB7-BB254DD01B46" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE652767-5070-4EAB-8BB7-BB254DD01B46", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", "Page used for viewing an attendee's group member detail for this Sign-Up project. Clicking a row in the grid will take you to this page.", 0, "", "E0908F51-8B7E-4D94-8972-7DDCDB3D37A6" );
            Sql( "UPDATE [Attribute] SET [IsRequired] = 1 WHERE [Guid] = 'E0908F51-8B7E-4D94-8972-7DDCDB3D37A6';" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE652767-5070-4EAB-8BB7-BB254DD01B46", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", "Page used for viewing a person's profile. If set, a view profile button will show for each group member.", 1, "", "E1FB0EC5-F0C8-4BBE-BEB1-B2C5D7B1A1C0" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Auto-Select First Group", "DisableAutoSelectFirstGroup", "Disable Auto-Select First Group", "Whether to disable the default behavior of auto-selecting the first group (ordered by name) in the tree view.", 10, "False", "AD145399-2D61-40B4-802A-400766574692" );
        }

        /// <summary>
        /// JPH: Add admin pages and blocks needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddAdminPagesAndBlocks()
        {
            // [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddPage( true, "48242949-944A-4651-B6CC-60194EDE08A0", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Sign-Up", "", "1941542C-21F2-4341-BDE1-996AA1E0C0A2", "", "2A0C135A-8421-4125-A484-83C8B4FB3D34" );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "1941542C-21F2-4341-BDE1-996AA1E0C0A2", "people/signup", "75332FE3-DB1B-4B83-9287-5EDDD09A1A4E" );
            RockMigrationHelper.AddPageRoute( "1941542C-21F2-4341-BDE1-996AA1E0C0A2", "people/sign-up", "A9EEE819-13CA-425C-9118-65B421BC9FEB" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlock( true, "1941542C-21F2-4341-BDE1-996AA1E0C0A2".AsGuidOrNull(), null, null, "B539F3B5-01D3-4325-B32A-85AFE2A9D18B".AsGuidOrNull(), "Sign-Up Overview", "Main", "", "", 0, "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4" );
            // [Attribute Value]: Project Detail Page for [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4", "E306CB42-10FE-428C-A8B9-224BB7B30C6A", "34212f8e-5f14-4d92-8b19-46748eba2727,d6aefd09-630e-40d7-aa56-77cc904c6595" );
            // [Attribute Value]: Sign-Up Opportunity Attendee List Page for [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4", "B86F9026-5B63-414C-A069-B5C86B8FEFC2", "aaf11844-ec6c-498b-a9d8-387390206570,db9c7e0d-5ec7-4cc2-ba9e-dd398d4b9714" );
            // [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlock( true, "1941542C-21F2-4341-BDE1-996AA1E0C0A2".AsGuidOrNull(), null, null, "2D26A2C4-62DC-4680-8219-A52EB2BC0F65".AsGuidOrNull(), "Sign-Up Groups", "Sidebar1", "", "", 0, "B9D4522A-38D7-4F5B-B9CD-E5497B258471" );
            // [Attribute Value]: Detail Page for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2", "34212f8e-5f14-4d92-8b19-46748eba2727,d6aefd09-630e-40d7-aa56-77cc904c6595" );
            // [Attribute Value]: Disable Auto-Select First Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "AD145399-2D61-40B4-802A-400766574692", "True" );
            // [Attribute Value]: Display Inactive Campuses for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "22D5915F-D449-4E03-A8AD-0C473A3D4864", "True" );
            // [Attribute Value]: Initial Active Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "2AD968BA-6721-4B69-A4FE-B57D8FB0ECFB", "1" );
            // [Attribute Value]: Initial Count Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "36D18581-3874-4C5A-A01B-793A458F9F91", "0" );
            // [Attribute Value]: Limit to Security Role Groups for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "1688837B-73CF-46C3-8880-74C46605807C", "False" );
            // [Attribute Value]: Root Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "0E1768CD-87CC-4361-8BCD-01981FBFE24B", "d649638a-ef91-42d8-9b38-32172d614a5f" );
            // [Attribute Value]: Show Settings Panel for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "4633ED5A-7A2C-4A78-B092-6733FED8CFA6", "True" );
            // [Attribute Value]: Treeview Title for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.AddBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "D1583306-2504-48D2-98EE-3DE55C2806C7", "Sign-Up Groups" );

            // [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddPage( true, "1941542C-21F2-4341-BDE1-996AA1E0C0A2", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Sign-Up Detail", "", "34212F8E-5F14-4D92-8B19-46748EBA2727", "", null );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "34212F8E-5F14-4D92-8B19-46748EBA2727", "people/signup/{GroupId}", "D6AEFD09-630E-40D7-AA56-77CC904C6595" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Detail for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlock( true, "34212F8E-5F14-4D92-8B19-46748EBA2727".AsGuidOrNull(), null, null, "69F5C6BD-7A22-42FE-8285-7C8E586E746A".AsGuidOrNull(), "Sign-Up Detail", "Main", "", "", 0, "735C2380-5E10-4EDF-91ED-4EDF9BD5C507" );
            // [Attribute Value]: Sign-Up Opportunity Attendee List Page for [Block]: Sign-Up Detail for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "735C2380-5E10-4EDF-91ED-4EDF9BD5C507", "525A1D90-CF46-4710-ADC3-86552EBB1E9C", "aaf11844-ec6c-498b-a9d8-387390206570,db9c7e0d-5ec7-4cc2-ba9e-dd398d4b9714" );
            // [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlock( true, "34212F8E-5F14-4D92-8B19-46748EBA2727".AsGuidOrNull(), null, null, "2D26A2C4-62DC-4680-8219-A52EB2BC0F65".AsGuidOrNull(), "Sign-Up Groups", "Sidebar1", "", "", 0, "D493C133-08EE-4E78-A85D-FF8E4FF80158" );
            // [Attribute Value]: Detail Page for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2", "34212f8e-5f14-4d92-8b19-46748eba2727,d6aefd09-630e-40d7-aa56-77cc904c6595" );
            // [Attribute Value]: Disable Auto-Select First Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "AD145399-2D61-40B4-802A-400766574692", "False" );
            // [Attribute Value]: Display Inactive Campuses for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "22D5915F-D449-4E03-A8AD-0C473A3D4864", "True" );
            // [Attribute Value]: Initial Active Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "2AD968BA-6721-4B69-A4FE-B57D8FB0ECFB", "1" );
            // [Attribute Value]: Initial Count Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "36D18581-3874-4C5A-A01B-793A458F9F91", "0" );
            // [Attribute Value]: Limit to Security Role Groups for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "1688837B-73CF-46C3-8880-74C46605807C", "False" );
            // [Attribute Value]: Root Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "0E1768CD-87CC-4361-8BCD-01981FBFE24B", "d649638a-ef91-42d8-9b38-32172d614a5f" );
            // [Attribute Value]: Show Settings Panel for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "4633ED5A-7A2C-4A78-B092-6733FED8CFA6", "True" );
            // [Attribute Value]: Treeview Title for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "D1583306-2504-48D2-98EE-3DE55C2806C7", "Sign-Up Groups" );

            // [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.AddPage( true, "34212F8E-5F14-4D92-8B19-46748EBA2727", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Sign-Up Opportunity Attendee List", "", "AAF11844-EC6C-498B-A9D8-387390206570", "", null );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "AAF11844-EC6C-498B-A9D8-387390206570", "people/signup/{GroupId}/location/{LocationId}/schedule/{ScheduleId}", "DB9C7E0D-5EC7-4CC2-BA9E-DD398D4B9714" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.AddBlock( true, "AAF11844-EC6C-498B-A9D8-387390206570".AsGuidOrNull(), null, null, "EE652767-5070-4EAB-8BB7-BB254DD01B46".AsGuidOrNull(), "Sign-Up Opportunity Attendee List", "Main", "", "", 0, "54FC3FA7-2D25-4694-8DD4-647222582CEB" );
            // [Attribute Value]: Group Member Detail Page for [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.AddBlockAttributeValue( "54FC3FA7-2D25-4694-8DD4-647222582CEB", "E0908F51-8B7E-4D94-8972-7DDCDB3D37A6", "05b79031-183f-4a64-a689-56b5c8e7519f,40566dcd-ac73-4c61-95b3-8f9b2e06528c" );
            // [Attribute Value]: Person Profile Page for [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.AddBlockAttributeValue( "54FC3FA7-2D25-4694-8DD4-647222582CEB", "E1FB0EC5-F0C8-4BBE-BEB1-B2C5D7B1A1C0", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );

            // [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddPage( true, "AAF11844-EC6C-498B-A9D8-387390206570", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail", "", "05B79031-183F-4A64-A689-56B5C8E7519F", "", null );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "05B79031-183F-4A64-A689-56B5C8E7519F", "people/signup/{GroupId}/location/{LocationId}/schedule/{ScheduleId}/member/{GroupMemberId}", "40566DCD-AC73-4C61-95B3-8F9B2E06528C" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlock( true, "05B79031-183F-4A64-A689-56B5C8E7519F".AsGuidOrNull(), null, null, "AAE2E5C3-9279-4AB0-9682-F4D19519D678".AsGuidOrNull(), "Group Member Detail", "Main", "", "", 0, "C4D268FC-17B8-4E55-B3A2-7C55F79015BD" );
            // [Attribute Value]: Allow Selecting 'From' for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "65FCFD8F-0BD9-4285-AC2B-6CCB6654EC20", "True" );
            // [Attribute Value]: Append Organization Email Header/Footer for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "A0513BD2-3A68-40A4-94F0-063AEF476048", "True" );
            // [Attribute Value]: Are Requirements Publicly Hidden for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "924FFC5A-FF18-4EC1-ADE6-E5E9BCD3EBA4", "False" );
            // [Attribute Value]: Are Requirements Refreshed When Block Is Loaded for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "78A53B17-B4BA-4345-B984-6172E03F9B0E", "False" );
            // [Attribute Value]: Enable Communications for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "9C78478B-A1D9-4F62-BB36-EB9F32AA3035", "True" );
            // [Attribute Value]: Enable SMS for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "D657F24D-565F-4F19-B6D8-CB0D9A3F3121", "True" );
            // [Attribute Value]: Is Requirement Summary Hidden for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "562D04DB-744C-48CC-8738-BA094F6FEA26", "False" );
            // [Attribute Value]: Registration Page for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "2EDA5282-EA3E-446F-9CD6-5B3F323FC245", "aaf11844-ec6c-498b-a9d8-387390206570,db9c7e0d-5ec7-4cc2-ba9e-dd398d4b9714" );
            // [Attribute Value]: Show "Move To Another Group" Button for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "260A458D-BC35-4A36-B966-172870AFB24B", "False" );
            // [Attribute Value]: Workflow Entry Page for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.AddBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "75C4FE0F-58E1-4BE2-896B-9ADA0A0D4D4F", "0550d2aa-a705-4400-81ff-ab124fdf83d7" );
        }

        /// <summary>
        /// JPH: Add public block types needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddPublicBlockTypes()
        {
            #region Sign-Up Finder

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.SignUp.SignUpFinder", "Sign-Up Finder", "Rock.Blocks.Engagement.SignUp.SignUpFinder, Rock.Blocks, Version=1.15.0.10, Culture=neutral, PublicKeyToken=null", false, false, "BF09747C-786D-4979-BADF-2D0157F4CB21" );
            RockMigrationHelper.UpdateMobileBlockType( "Sign-Up Finder", "Block used for finding a sign-up group/project.", "Rock.Blocks.Engagement.SignUp.SignUpFinder", "Engagement > Sign-Up", "74A20402-00DF-4A87-98D1-B5A8920F1D32" );

            // Layout / Initial Page Load
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Overcapacity Projects", "HideOvercapacityProjects", "Hide Overcapacity Projects", "Determines if projects that are full should be shown.", 0, "False", "4D983F00-58DE-4524-8584-3DE82551A0E8" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Load Results on Initial Page Load", "LoadResultsOnInitialPageLoad", "Load Results on Initial Page Load", "When enabled the project finder will load with all configured projects (no filters enabled).", 0, "True", "8B41DDE9-5FE8-4BFA-83FB-75869B64F6C3" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Project Filters As", "DisplayProjectFiltersAs", "Display Project Filters As", "Determines if the project filters should be show as checkboxes or multi-select dropdowns.", 0, "Checkboxes", "F4640D8E-0EAC-4DEF-AD7A-3C07E3DD8FBC" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Columns", "FilterColumns", "Filter Columns", "The number of columns the filters should be displayed as.", 0, "1", "CC3757ED-D18D-48C5-AEB1-D6CF4543B8AA" );

            // Project Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Project Types", "ProjectTypes", "Project Types", "Select the sign-up project group types that should be considered for the search.", 0, "499B1367-06B3-4538-9D56-56D53F55DCB1", "747F9EEE-1D1E-4E2F-A37B-37BC376D94BF" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Project Type Filter Label", "ProjectTypeFilterLabel", "Project Type Filter Label", "The label to use for the project type filter.", 0, "Project Type", "D2C2CC22-3B23-4E19-B7DF-E605B599EAF0" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Display Attribute Filters", "DisplayAttributeFilters", "Display Attribute Filters", "The group attributes that should be available for an individual to filter the results by.", 0, "", "82D4761F-3F36-43EE-95D6-54B5417DBF16" );

            // Campus Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Campus Filter", "DisplayCampusFilter", "Display Campus Filter", "Determines if the campus filter should be shown. If there is only one active campus to display then this filter will not be shown, even if enabled.", 0, "False", "C3589A12-795C-4B98-AB6B-6901886FC728" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "Enable Campus Context", "If the page has a campus context, its value will be used as a filter.", 0, "False", "E77B1785-C82C-4A73-8D37-A7351C1FB3F2" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", "The types of campuses to include in the campus list.", 0, "", "FFE4F58E-6FE0-4E6E-8CD5-F60294F57189" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", "The statuses of the campuses to include in the campus list.", 0, "", "EB976D8F-9382-4B6E-A435-25B040FDD717" );

            // Schedule Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Named Schedule Filter", "DisplayNamedScheduleFilter", "Display Named Schedule Filter", "When enabled a list of named schedules will be show as a filter.", 0, "False", "A1A1CA52-21A3-4CFE-A5F3-EAFC5EC27E46" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Named Schedule Filter Label", "NamedScheduleFilterLabel", "Named Schedule Filter Label", "The label to use for the named schedule filter.", 0, "Schedules", "A739AF3F-913D-4E82-BDEC-D19A71DCA185" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Root Schedule Category", "RootScheduleCategory", "Root Schedule Category", "When displaying the named schedule filter this will serve to filter which named schedules to show. Only direct descendants of this root schedule category will be displayed.", 0, "", "09471FDD-88B2-4155-9655-9851B1B3C7AE" );

            // Location Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Location Sort", "DisplayLocationSort", "Display Location Sort", "Determines if the location sort field should be shown.", 0, "True", "21936ABB-7C05-4752-9D91-E8214B6AEFAD" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Sort Label", "LocationSortLabel", "Location Sort Label", "The label to use for the location sort filter.", 0, "Location (City, State or Zip Code)", "EF208061-FB54-4A85-8672-BC785C8DB867" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Location Range Filter", "DisplayLocationRangeFilter", "Display Location Range Filter", "When enabled a filter will be shown to limit results to a specified number of miles from the location selected or their mailing address if logged in. If the Location Sort entry is not enabled to be shown and the individual is not logged in then this filter will not be shown, even if enabled, as we will not be able to honor the filter.", 0, "True", "EED92C2B-370B-4EF5-98D3-30F4174053A6" );

            // Additional Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Date Range", "DisplayDateRange", "Display Date Range", "When enabled, individuals would be able to filter the results by projects occurring inside the provided date range.", 0, "True", "B33CF69F-8A0F-4EA1-9625-D6AD8174584E" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Slots Available Filter", "DisplaySlotsAvailableFilter", "Display Slots Available Filter", @"When enabled allows the individual to find projects with ""at least"" or ""no more than"" the provided spots available.", 0, "True", "6E05D7CD-3D6F-48D0-863D-8D44D4716121" );

            // Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "Results Lava Template", "The Lava template to use to show the results of the search.", 0, @"{% assign projectCount = Projects | Size %}
{% if projectCount > 0 %}
    <div class=""row d-flex flex-wrap"">
        {% for project in Projects %}
            <div class=""col-md-4 col-sm-6 col-xs-12 mb-4"">
                <div class=""card h-100"">
                    <div class=""card-body"">
                        <h3 class=""card-title mt-0"">{{ project.Name }}</h3>
                        {% if project.ScheduleName != empty %}
                            <p class=""card-subtitle text-muted mb-3"">{{ project.ScheduleName }}</p>
                        {% endif %}
                        <p class=""mb-2"">{{ project.FriendlySchedule }}</p>
                        <div class=""d-flex justify-content-between mb-3"">
                            {% if project.AvailableSpots != null %}
                                <span class=""badge badge-info"">Available Spots: {{ project.AvailableSpots }}</span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                            {% if project.DistanceInMiles != null %}
                                <span class=""badge"">{{ project.DistanceInMiles | Format:'0.0' }} miles<span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                        </div>
                        {% if project.MapCenter != empty %}
                            <div class=""mb-3"">
                                {[ googlestaticmap center:'{{ project.MapCenter }}' zoom:'15' ]}
                                {[ endgooglestaticmap ]}
                            </div>
                        {% endif %}
                        {% if project.Description != empty %}
                            <p class=""card-text"">
                                {{ project.Description }}
                            </p>
                        {% endif %}
                    </div>
                    <div class=""card-footer bg-white border-0"">
                        <a href=""{{ project.ProjectDetailPageUrl }}"" class=""btn btn-link btn-xs pl-0 text-muted"">Details</a>
                        <a href=""{{ project.RegisterPageUrl }}"" class=""btn btn-warning btn-xs pull-right"">Register</a>
                    </div>
                </div>
            </div>
        {% endfor %}
    </div>
{% else %}
    <div>
        No projects found.
    </div>
{% endif %}", "5B123108-1C30-4F91-BCA1-1A91561EA07E" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Header Lava Template", "ResultsHeaderLavaTemplate", "Results Header Lava Template", "The Lava Template to use to show the results header.", 0, @"<h3>Results</h3>
<p>Below is a listing of the projects that match your search results.</p>
<hr class=""mb-5"" />", "DF0ADA6D-FF88-4569-957A-EF1509333BF5" );

            // Linked Pages
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Project Detail Page", "ProjectDetailPage", "Project Detail Page", "The page reference to pass to the Lava template for the details of the project.", 0, "", "E6F7BF12-0502-4EC5-872C-3241139D065F" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", "The page reference to pass to the Lava template for the registration page.", 0, "", "4F9996C6-AA58-46AC-B322-109C019BBEC4" );

            #endregion

            #region Sign-Up Register

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.SignUp.SignUpRegister", "Sign-Up Register", "Rock.Blocks.Engagement.SignUp.SignUpRegister, Rock.Blocks, Version=1.15.0.10, Culture=neutral, PublicKeyToken=null", false, false, "ED7A31F2-8D4C-469A-B2D8-7E28B8717FB8" );
            RockMigrationHelper.UpdateMobileBlockType( "Sign-Up Register", "Block used to register for a sign-up group/project occurrence date time.", "Rock.Blocks.Engagement.SignUp.SignUpRegister", "Engagement > Sign-Up", "161587D9-7B74-4D61-BF8E-3CDB38F16A12" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "Mode", "Determines which registration mode the block is in.", 0, "1", "CA187D3D-C168-4E5A-B7B6-01E5B30820CB" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Children", "IncludeChildren", "Include Children", "Determines if children should be displayed as options when in Family and Group modes.", 1, "False", "551351B4-F5F2-448E-A58B-D38882A8D41E" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", "Workflow to launch when the sign-up is complete.", 2, "", "794B86B3-174A-4518-BCDC-1944469B8C67" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Registrant Confirmation System Communication", "RegistrantConfirmationSystemCommunication", "Registrant Confirmation System Communication", "Confirmation email to be sent to each registrant (in Family mode, only send to adults and the child if they were the registrar).", 3, "", "AD847694-C431-4163-AEAF-6A3A2AB4B608" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Email", "RequireEmail", "Require Email", "When enabled, requires that a value be entered for email when registering in Anonymous mode.", 4, "False", "3AD6B4F9-6EFB-4CCA-8A3A-B7D4CD13E2F5" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Mobile Phone", "RequireMobilePhone", "Require Mobile Phone", "When enabled, requires that a value be entered for mobile phone when registering in Anonymous mode.", 5, "False", "89ABA9D9-6141-42FE-8B09-A3ED481816FA" );

            #endregion

            #region Sign-Up Attendance Detail

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.SignUp.SignUpAttendanceDetail", "Sign-Up Attendance Detail", "Rock.Blocks.Engagement.SignUp.SignUpAttendanceDetail, Rock.Blocks, Version=1.15.0.10, Culture=neutral, PublicKeyToken=null", false, false, "747587A0-87E9-437D-A4ED-75431CED55B3" );
            RockMigrationHelper.UpdateMobileBlockType( "Sign-Up Attendance Detail", "Lists the group members for a specific sign-up group/project occurrence date time and allows selecting if they attended or not.", "Rock.Blocks.Engagement.SignUp.SignUpAttendanceDetail", "Engagement > Sign-Up", "96D160D9-5668-46EF-9941-702BD3A577DB" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "96D160D9-5668-46EF-9941-702BD3A577DB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", "The Lava template to show at the top of the page.", 0, @"<h3>{{ Group.Name }}</h3>
<div>
    Please enter attendance for the project below.
    <br />Date: {{ AttendanceOccurrenceDate | Date:'dddd, MMM d' }}
    {% if WasScheduleParamProvided %}
        <br />Schedule: {{ ScheduleName }}
    {% endif %}
    {% if WasLocationParamProvided %}
        <br />Location: {{ LocationName }}
    {% endif %}
</div>", "0B3B0549-2353-4B06-A8F6-9C52135AB235" );

            #endregion
        }

        /// <summary>
        /// JPH: Add public pages and blocks needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_AddPublicPagesAndBlocks()
        {
            // [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddPage( true, "7625A63E-6650-4886-B605-53C2234FA5E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Sign-Up Finder", "", "2DC1906D-C9D5-411D-B961-A2295C9450A4", "", "3B31B9A2-DE35-4407-8E7D-3633F93906CD" );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "2DC1906D-C9D5-411D-B961-A2295C9450A4", "signup", "37B68603-6D07-4C37-A89A-253DB72DBBE3" );
            RockMigrationHelper.AddPageRoute( "2DC1906D-C9D5-411D-B961-A2295C9450A4", "sign-up", "6D5797CC-7179-41DC-BF49-8BE5DEB6B40D" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlock( true, "2DC1906D-C9D5-411D-B961-A2295C9450A4".AsGuidOrNull(), null, null, "74A20402-00DF-4A87-98D1-B5A8920F1D32".AsGuidOrNull(), "Sign-Up Finder", "Main", "", "", 0, "B9ADC017-782B-4100-AA07-DD1D703EE971" );
            // [Attribute Value]: Project Detail Page for [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "B9ADC017-782B-4100-AA07-DD1D703EE971", "E6F7BF12-0502-4EC5-872C-3241139D065F", "7f22b3b0-64f8-47db-a6c0-5a1ce5f68bef,7e53fb15-b4c1-4339-afab-48b403ede875" );
            // [Attribute Value]: Registration Page for [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "B9ADC017-782B-4100-AA07-DD1D703EE971", "4F9996C6-AA58-46AC-B322-109C019BBEC4", "bbb8a41d-e65f-4aff-8987-bff3458a46c1,e6685354-09c3-479f-8b6a-c6bd0a18a675" );
            // [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlock( true, "2DC1906D-C9D5-411D-B961-A2295C9450A4".AsGuidOrNull(), null, null, "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuidOrNull(), "Sub Nav", "Sidebar1", "", "", 0, "C3E499E7-4B4B-4DD8-961A-E17384B2B13C" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "False" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "False" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "False" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", "1" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7625a63e-6650-4886-b605-53c2234fa5e1" );
            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.AddBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageSubNav.lava' %}" );

            // [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddPage( true, "2DC1906D-C9D5-411D-B961-A2295C9450A4", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Sign-Up Detail", "", "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF", "", null );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF", "signup/detail/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}", "7E53FB15-B4C1-4339-AFAB-48B403EDE875" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlock( true, "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF".AsGuidOrNull(), null, null, "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuidOrNull(), "Sub Nav", "Sidebar1", "", "", 0, "8803EA4E-ADA5-4163-B1C7-78600E046F40" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "False" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "False" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "False" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "6C952052-BC79-41BA-8B88-AB8EA3E99648", "1" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7625a63e-6650-4886-b605-53c2234fa5e1" );
            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageSubNav.lava' %}" );

            // [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddPage( true, "2DC1906D-C9D5-411D-B961-A2295C9450A4", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Sign-Up Register", "", "BBB8A41D-E65F-4AFF-8987-BFF3458A46C1", "", "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF" );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "BBB8A41D-E65F-4AFF-8987-BFF3458A46C1", "signup/register/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}", "E6685354-09C3-479F-8B6A-C6BD0A18A675" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Register for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlock( true, "BBB8A41D-E65F-4AFF-8987-BFF3458A46C1".AsGuidOrNull(), null, null, "161587D9-7B74-4D61-BF8E-3CDB38F16A12".AsGuidOrNull(), "Sign-Up Register", "Main", "", "", 0, "9D0AF2E9-7BE2-4320-A81C-CF22D0E94BD4" );
            // [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlock( true, "BBB8A41D-E65F-4AFF-8987-BFF3458A46C1".AsGuidOrNull(), null, null, "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuidOrNull(), "Sub Nav", "Sidebar1", "", "", 0, "282E8B4A-3FEA-4487-AF20-907B9027AD75" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", "False" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", "False" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "False" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "6C952052-BC79-41BA-8B88-AB8EA3E99648", "1" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "41F1C42E-2395-4063-BD4F-031DF8D5B231", "7625a63e-6650-4886-b605-53c2234fa5e1" );
            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageSubNav.lava' %}" );

            // [Page]: Group Attendance > Sign-Up Attendance Detail
            RockMigrationHelper.AddPage( true, "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Sign-Up Attendance Detail", "", "73FC6F39-6194-483A-BF0D-7FDD1DD91C91", "", "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7" );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "73FC6F39-6194-483A-BF0D-7FDD1DD91C91", "signup/attendance/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}", "33DE9AF2-456C-413D-8559-A58DEA78D62A" );
#pragma warning restore CS0618 // Type or member is obsolete
            // [Block]: Sign-Up Attendance Detail for [Page]: Group Attendance > Sign-Up Attendance Detail
            RockMigrationHelper.AddBlock( true, "73FC6F39-6194-483A-BF0D-7FDD1DD91C91".AsGuidOrNull(), null, null, "96D160D9-5668-46EF-9941-702BD3A577DB".AsGuidOrNull(), "Sign-Up Attendance Detail", "Main", "", "", 0, "5ED5DD3E-7280-4617-8BB1-0A6CF2DFED6D" );
        }

        /// <summary>
        /// JPH: Update existing block values for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_UpdateExistingBlockValues()
        {
            // Update the Group Viewer page's Group Tree View to exclude groups of type "Sign-Up Group", since we're adding new blocks to manage groups of this type.
            RockMigrationHelper.AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "D8EEB91B-745E-4D63-911B-728D8F1B0B6E", "499b1367-06b3-4538-9d56-56d53f55dcb1", true );
        }

        /// <summary>
        /// JPH: Revert changes made to existing block values for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_RevertChangesToExistingBlockValues()
        {
            // No consistent way to revert the changes we've made to the Group Viewer page's Group Tree View's "Group Types Exclude" attribute; we'll just leave an invalid value in this setting's comma-separated Guid list, which will cause no harm.
        }

        /// <summary>
        /// JPH: Delete public pages and blocks added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeletePublicPagesAndBlocks()
        {
            // [Block]: Sign-Up Attendance Detail for [Page]: Group Attendance > Sign-Up Attendance Detail
            RockMigrationHelper.DeleteBlock( "5ED5DD3E-7280-4617-8BB1-0A6CF2DFED6D" );
            // [Page]: Group Attendance > Sign-Up Attendance Detail
            RockMigrationHelper.DeletePageRoute( "33DE9AF2-456C-413D-8559-A58DEA78D62A" );
            RockMigrationHelper.DeletePage( "73FC6F39-6194-483A-BF0D-7FDD1DD91C91" );

            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "1322186A-862A-4CF1-B349-28ECB67229BA" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "282E8B4A-3FEA-4487-AF20-907B9027AD75", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            // [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlock( "282E8B4A-3FEA-4487-AF20-907B9027AD75" );
            // [Block]: Sign-Up Register for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlock( "9D0AF2E9-7BE2-4320-A81C-CF22D0E94BD4" );
            // [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeletePageRoute( "E6685354-09C3-479F-8B6A-C6BD0A18A675" );
            RockMigrationHelper.DeletePage( "BBB8A41D-E65F-4AFF-8987-BFF3458A46C1" );

            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "1322186A-862A-4CF1-B349-28ECB67229BA" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "8803EA4E-ADA5-4163-B1C7-78600E046F40", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            // [Block]: Sub Nav (Page Menu) for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlock( "8803EA4E-ADA5-4163-B1C7-78600E046F40" );
            // [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeletePageRoute( "7E53FB15-B4C1-4339-AFAB-48B403EDE875" );
            RockMigrationHelper.DeletePage( "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF" );

            // [Attribute Value]: Template for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "1322186A-862A-4CF1-B349-28ECB67229BA" );
            // [Attribute Value]: Root Page for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "41F1C42E-2395-4063-BD4F-031DF8D5B231" );
            // [Attribute Value]: Number of Levels for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "6C952052-BC79-41BA-8B88-AB8EA3E99648" );
            // [Attribute Value]: Is Secondary Block for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            // [Attribute Value]: Include Current QueryString for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69" );
            // [Attribute Value]: Include Current Parameters for [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183" );
            // [Block]: Sub Nav (Page Menu) for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlock( "C3E499E7-4B4B-4DD8-961A-E17384B2B13C" );
            // [Attribute Value]: Registration Page for [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "B9ADC017-782B-4100-AA07-DD1D703EE971", "4F9996C6-AA58-46AC-B322-109C019BBEC4" );
            // [Attribute Value]: Project Detail Page for [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlockAttributeValue( "B9ADC017-782B-4100-AA07-DD1D703EE971", "E6F7BF12-0502-4EC5-872C-3241139D065F" );
            // [Block]: Sign-Up Finder for [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeleteBlock( "B9ADC017-782B-4100-AA07-DD1D703EE971" );
            // [Page]: Connect > Sign-Up Finder
            RockMigrationHelper.DeletePageRoute( "6D5797CC-7179-41DC-BF49-8BE5DEB6B40D" );
            RockMigrationHelper.DeletePageRoute( "37B68603-6D07-4C37-A89A-253DB72DBBE3" );
            RockMigrationHelper.DeletePage( "2DC1906D-C9D5-411D-B961-A2295C9450A4" );
        }

        /// <summary>
        /// JPH: Delete public block types added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeletePublicBlockTypes()
        {
            RockMigrationHelper.DeleteBlockType( "74A20402-00DF-4A87-98D1-B5A8920F1D32" );
            RockMigrationHelper.DeleteEntityType( "BF09747C-786D-4979-BADF-2D0157F4CB21" );

            RockMigrationHelper.DeleteBlockType( "161587D9-7B74-4D61-BF8E-3CDB38F16A12" );
            RockMigrationHelper.DeleteEntityType( "ED7A31F2-8D4C-469A-B2D8-7E28B8717FB8" );

            RockMigrationHelper.DeleteBlockAttribute( "0B3B0549-2353-4B06-A8F6-9C52135AB235" );
            RockMigrationHelper.DeleteBlockType( "96D160D9-5668-46EF-9941-702BD3A577DB" );
            RockMigrationHelper.DeleteEntityType( "747587A0-87E9-437D-A4ED-75431CED55B3" );
        }

        /// <summary>
        /// JPH: Delete admin pages and blocks added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteAdminPagesAndBlocks()
        {
            // [Attribute Value]: Workflow Entry Page for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "75C4FE0F-58E1-4BE2-896B-9ADA0A0D4D4F" );
            // [Attribute Value]: Show "Move To Another Group" Button for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "260A458D-BC35-4A36-B966-172870AFB24B" );
            // [Attribute Value]: Registration Page for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "2EDA5282-EA3E-446F-9CD6-5B3F323FC245" );
            // [Attribute Value]: Is Requirement Summary Hidden for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "562D04DB-744C-48CC-8738-BA094F6FEA26" );
            // [Attribute Value]: Enable SMS for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "D657F24D-565F-4F19-B6D8-CB0D9A3F3121" );
            // [Attribute Value]: Enable Communications for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "9C78478B-A1D9-4F62-BB36-EB9F32AA3035" );
            // [Attribute Value]: Are Requirements Refreshed When Block Is Loaded for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "78A53B17-B4BA-4345-B984-6172E03F9B0E" );
            // [Attribute Value]: Are Requirements Publicly Hidden for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "924FFC5A-FF18-4EC1-ADE6-E5E9BCD3EBA4" );
            // [Attribute Value]: Append Organization Email Header/Footer for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "A0513BD2-3A68-40A4-94F0-063AEF476048" );
            // [Attribute Value]: Allow Selecting 'From' for [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD", "65FCFD8F-0BD9-4285-AC2B-6CCB6654EC20" );
            // [Block]: Group Member Detail for [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeleteBlock( "C4D268FC-17B8-4E55-B3A2-7C55F79015BD" );
            // [Page]: Sign-Up Opportunity Attendee List > Group Member Detail
            RockMigrationHelper.DeletePageRoute( "40566DCD-AC73-4C61-95B3-8F9B2E06528C" );
            RockMigrationHelper.DeletePage( "05B79031-183F-4A64-A689-56B5C8E7519F" );

            // [Attribute Value]: Person Profile Page for [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.DeleteBlockAttributeValue( "54FC3FA7-2D25-4694-8DD4-647222582CEB", "E1FB0EC5-F0C8-4BBE-BEB1-B2C5D7B1A1C0" );
            // [Attribute Value]: Group Member Detail Page for [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.DeleteBlockAttributeValue( "54FC3FA7-2D25-4694-8DD4-647222582CEB", "E0908F51-8B7E-4D94-8972-7DDCDB3D37A6" );
            // [Block]: Sign-Up Opportunity Attendee List for [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.DeleteBlock( "54FC3FA7-2D25-4694-8DD4-647222582CEB" );
            // [Page]: Sign-Up Detail > Sign-Up Opportunity Attendee List
            RockMigrationHelper.DeletePageRoute( "DB9C7E0D-5EC7-4CC2-BA9E-DD398D4B9714" );
            RockMigrationHelper.DeletePage( "AAF11844-EC6C-498B-A9D8-387390206570" );

            // [Attribute Value]: Treeview Title for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "D1583306-2504-48D2-98EE-3DE55C2806C7" );
            // [Attribute Value]: Show Settings Panel for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "4633ED5A-7A2C-4A78-B092-6733FED8CFA6" );
            // [Attribute Value]: Root Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "0E1768CD-87CC-4361-8BCD-01981FBFE24B" );
            // [Attribute Value]: Limit to Security Role Groups for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "1688837B-73CF-46C3-8880-74C46605807C" );
            // [Attribute Value]: Initial Count Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "36D18581-3874-4C5A-A01B-793A458F9F91" );
            // [Attribute Value]: Initial Active Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "2AD968BA-6721-4B69-A4FE-B57D8FB0ECFB" );
            // [Attribute Value]: Display Inactive Campuses for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "22D5915F-D449-4E03-A8AD-0C473A3D4864" );
            // [Attribute Value]: Disable Auto-Select First Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "AD145399-2D61-40B4-802A-400766574692" );
            // [Attribute Value]: Detail Page for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "D493C133-08EE-4E78-A85D-FF8E4FF80158", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2" );
            // [Block]: Sign-Up Groups (Group Tree View) for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlock( "D493C133-08EE-4E78-A85D-FF8E4FF80158" );
            // [Attribute Value]: Sign-Up Opportunity Attendee List Page for [Block]: Sign-Up Detail for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "735C2380-5E10-4EDF-91ED-4EDF9BD5C507", "525A1D90-CF46-4710-ADC3-86552EBB1E9C" );
            // [Block]: Sign-Up Detail for [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeleteBlock( "735C2380-5E10-4EDF-91ED-4EDF9BD5C507" );
            // [Page]: Sign-Up > Sign-Up Detail
            RockMigrationHelper.DeletePageRoute( "D6AEFD09-630E-40D7-AA56-77CC904C6595" );
            RockMigrationHelper.DeletePage( "34212F8E-5F14-4D92-8B19-46748EBA2727" );

            // [Attribute Value]: Treeview Title for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "D1583306-2504-48D2-98EE-3DE55C2806C7" );
            // [Attribute Value]: Show Settings Panel for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "4633ED5A-7A2C-4A78-B092-6733FED8CFA6" );
            // [Attribute Value]: Root Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "0E1768CD-87CC-4361-8BCD-01981FBFE24B" );
            // [Attribute Value]: Limit to Security Role Groups for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "1688837B-73CF-46C3-8880-74C46605807C" );
            // [Attribute Value]: Initial Count Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "36D18581-3874-4C5A-A01B-793A458F9F91" );
            // [Attribute Value]: Initial Active Setting for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "2AD968BA-6721-4B69-A4FE-B57D8FB0ECFB" );
            // [Attribute Value]: Display Inactive Campuses for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "22D5915F-D449-4E03-A8AD-0C473A3D4864" );
            // [Attribute Value]: Disable Auto-Select First Group for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "AD145399-2D61-40B4-802A-400766574692" );
            // [Attribute Value]: Detail Page for [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "B9D4522A-38D7-4F5B-B9CD-E5497B258471", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2" );
            // [Block]: Sign-Up Groups (Group Tree View) for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlock( "B9D4522A-38D7-4F5B-B9CD-E5497B258471" );
            // [Attribute Value]: Sign-Up Opportunity Attendee List Page for [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4", "B86F9026-5B63-414C-A069-B5C86B8FEFC2" );
            // [Attribute Value]: Project Detail Page for [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlockAttributeValue( "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4", "E306CB42-10FE-428C-A8B9-224BB7B30C6A" );
            // [Block]: Sign-Up Overview for [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeleteBlock( "4ECCC106-5374-4E33-A8AA-3ADE977FB1A4" );
            // [Page]: Engagement > Sign-Up
            RockMigrationHelper.DeletePageRoute( "A9EEE819-13CA-425C-9118-65B421BC9FEB" );
            RockMigrationHelper.DeletePageRoute( "75332FE3-DB1B-4B83-9287-5EDDD09A1A4E" );
            RockMigrationHelper.DeletePage( "1941542C-21F2-4341-BDE1-996AA1E0C0A2" );
        }

        /// <summary>
        /// JPH: Delete admin block types added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteAdminBlockTypes()
        {
            RockMigrationHelper.DeleteBlockAttribute( "E306CB42-10FE-428C-A8B9-224BB7B30C6A" );
            RockMigrationHelper.DeleteBlockAttribute( "B86F9026-5B63-414C-A069-B5C86B8FEFC2" );
            RockMigrationHelper.DeleteBlockType( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B" );

            RockMigrationHelper.DeleteBlockAttribute( "525A1D90-CF46-4710-ADC3-86552EBB1E9C" );
            RockMigrationHelper.DeleteBlockType( "69F5C6BD-7A22-42FE-8285-7C8E586E746A" );

            RockMigrationHelper.DeleteBlockAttribute( "E0908F51-8B7E-4D94-8972-7DDCDB3D37A6" );
            RockMigrationHelper.DeleteBlockAttribute( "E1FB0EC5-F0C8-4BBE-BEB1-B2C5D7B1A1C0" );
            RockMigrationHelper.DeleteBlockType( "EE652767-5070-4EAB-8BB7-BB254DD01B46" );

            RockMigrationHelper.DeleteBlockAttribute( "AD145399-2D61-40B4-802A-400766574692" );
        }

        /// <summary>
        /// JPH: Delete system communications added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteSystemCommunications()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SIGNUP_GROUP_REMINDER );

            RockMigrationHelper.DeleteCategory( SystemGuid.Category.SYSTEM_COMMUNICATION_SIGNUP_GROUP_CONFIRMATION );
        }

        /// <summary>
        /// JPH: Delete group related records added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteGroups()
        {
            RockMigrationHelper.DeleteGroup( SystemGuid.Group.GROUP_SIGNUP_GROUPS );
            RockMigrationHelper.DeleteAttributeQualifier( "D49200BC-9E54-4906-9B20-53FD8973A43D" );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.GROUPTYPE_SIGNUP_GROUP_PROJECT_TYPE );
            RockMigrationHelper.DeleteGroupType( SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );
        }

        /// <summary>
        /// JPH: Delete defined types and values added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteDefinedTypesAndValues()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.PROJECT_TYPE_PROJECT_DUE );

            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.PROJECT_TYPE );
        }
    }
}
