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
    public partial class GroupSchedulingUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MigrateSystemEmailsUp();
            MigratePagesBlocksUp();
        }

        /// <summary>
        /// Migrates the pages blocks up.
        /// </summary>
        private void MigratePagesBlocksUp()
        {
            // Add Block to Page: My Account Site: External Website
            RockMigrationHelper.AddBlock( true, "C0854F84-2E8B-479C-A3FB-6B47BE89B795".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Group Schedule Toolbox Links", "Sidebar1", @"", @"", 2, "95061C6F-08D5-4B19-942D-690AD17F7D1D" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '87068AAB-16A7-42CC-8A31-5A957D6C4DD5'" );  // Page: My Account,  Zone: Sidebar1,  Block: Actions
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8'" );  // Page: My Account,  Zone: Sidebar1,  Block: Group List Personalized Lava
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '95061C6F-08D5-4B19-942D-690AD17F7D1D'" );  // Page: My Account,  Zone: Sidebar1,  Block: Group Schedule Toolbox Links
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '37D4A991-9F9A-47CE-9084-04466F166B6A'" );  // Page: My Account,  Zone: Sidebar1,  Block: Assessment List
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = 'E5596525-B176-4753-A337-25F1F9B83FCE'" );  // Page: My Account,  Zone: Sidebar1,  Block: Recent Registrations

            // Add/Update HtmlContent for Block: Group Schedule Toolbox Links
            RockMigrationHelper.UpdateHtmlContentBlock( "95061C6F-08D5-4B19-942D-690AD17F7D1D", @"<a href='/ScheduleToolbox' class='btn btn-default btn-block btn-xs margin-b-md'>
    <i class='fa fa-calendar'></i> 
    Schedule Toolbox
</a>", "3B13EE46-5E31-461F-B7EF-3211B0B4D4EA" );

            // Attrib for BlockType: Group Schedule Toolbox:Sign Up Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute( "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Sign Up Instructions", "SignupInstructions", "", @"Instructions here will show up on Sign Up tab. <span class='tip tip-lava'></span>", 2, @"<div class=""alert alert-info"">
    {%- if IsSchedulesAvailable -%}
        {%- if CurrentPerson.Id == Person.Id -%}
            Sign up to attend a group and location on the given date.
        {%- else -%}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {%- endif -%}
     {%- else -%}
        No sign-ups available.
     {%- endif -%}
</div>", "1AED7473-DCF1-42DA-B3ED-A8CF14587BA8" );

            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Scheduling", "", "896ED8DA-46A5-440B-92A0-76459869D921", "" );

            // Add Block to Page: Group Scheduling Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "896ED8DA-46A5-440B-92A0-76459869D921".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Group Scheduling Page Menu", "Main", @"", @"", 0, "3A597A40-FC16-4168-8857-3F23CC6833E8" );

            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Include Current Parameters Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Template Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );
            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Root Page Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"896ed8da-46a5-440b-92a0-76459869d921" );
            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Number of Levels Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );
            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Include Current QueryString Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:Group Scheduling Page Menu, Attribute:Is Secondary Block Page: Group Scheduling, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A597A40-FC16-4168-8857-3F23CC6833E8", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Scheduler", "", "1815D8C6-7C4A-4C05-A810-CF23BA937477", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Schedule Status Board", "", "31576E5D-7B6C-46D1-89F4-A14F4F8095D1", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Scheduler Analytics", "", "1E031B86-1476-4C72-9115-F94056398444", "" ); // Site:Rock RMS


            // Move the Group Scheduling related menu items as pages under the Group Scheduling menu page
            Sql( @"DECLARE @GroupSchedulingMenuPageId INT = (
		SELECT TOP 1 Id
		FROM [Page]
		WHERE [Guid] = '896ED8DA-46A5-440B-92A0-76459869D921'
		)

UPDATE [Page]
SET [ParentPageId] = @GroupSchedulingMenuPageId
WHERE [Guid] IN (
		'1815D8C6-7C4A-4C05-A810-CF23BA937477'
		,'31576E5D-7B6C-46D1-89F4-A14F4F8095D1'
		,'1E031B86-1476-4C72-9115-F94056398444'
		)" );

            Sql( @"
UPDATE [Page]
SET [IconCssClass] = 'fa fa-calendar'
WHERE [Guid] = '31576E5D-7B6C-46D1-89F4-A14F4F8095D1'

UPDATE [Page]
SET [IconCssClass] = 'fa fa-calendar-alt'
WHERE [Guid] = '1815D8C6-7C4A-4C05-A810-CF23BA937477'

UPDATE [Page]
SET [IconCssClass] = 'fa fa-line-chart'
WHERE [Guid] = '1E031B86-1476-4C72-9115-F94056398444'
" );
        }

        /// <summary>
        /// Migrates new system emails up.
        /// Code Generated from Dev Tools\Sql\CodeGen_SystemEmail.sql
        /// </summary>
        private void MigrateSystemEmailsUp()
        {
            // Scheduling Reminder Email
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Reminder Email", "", "", "", "", "", "Scheduling Reminder for {{Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Reminder</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>This is just a reminder that you are scheduled for the following on {{Attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}} </p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><strong>{{attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}</strong></td></tr>
    <tr><td><strong>{{ attendance.Occurrence.Group.Name }}</strong></td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "8A20FE79-B73C-447A-82B1-416F9B50C038" );

            // set default scheduling system emails for all existing group types
            Sql( @"
UPDATE [GroupType]
SET [ScheduleConfirmationSystemEmailId] = (
		SELECT TOP 1 Id
		FROM [SystemEmail]
		WHERE [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'
		)
	,[ScheduleReminderSystemEmailId] = (
		SELECT TOP 1 Id
		FROM [SystemEmail]
		WHERE [Guid] = '8A20FE79-B73C-447A-82B1-416F9B50C038'
		)
" );

        }


        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Scheduling Reminder Email
            RockMigrationHelper.DeleteSystemEmail( "8A20FE79-B73C-447A-82B1-416F9B50C038" );
        }
    }
}
