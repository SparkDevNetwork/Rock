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
    public partial class GroupScheduling : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            SchemaChangesUp();

            /** Section 1**/
            // Group Schedule Decline Reasons
            RockMigrationHelper.AddDefinedType( "Group", "Group Schedule Decline Reason", "List of all possible schedule decline reasons.", "70C9F9C4-20CC-43DD-888D-9243853A0E52", @"" );
            RockMigrationHelper.UpdateDefinedValue( "70C9F9C4-20CC-43DD-888D-9243853A0E52", "Family Emergency", "", "7533A32D-CC7B-4218-A1CA-030FB4F2473B", false );
            RockMigrationHelper.UpdateDefinedValue( "70C9F9C4-20CC-43DD-888D-9243853A0E52", "Have to Work", "", "8B9BF3F5-11CF-4E33-98A0-D48067A18103", false );
            RockMigrationHelper.UpdateDefinedValue( "70C9F9C4-20CC-43DD-888D-9243853A0E52", "On Vacation / Out of Town", "", "BB2F0712-5C57-40E9-83BF-68876890EC7A", false );
            RockMigrationHelper.UpdateDefinedValue( "70C9F9C4-20CC-43DD-888D-9243853A0E52", "Serving Elsewhere", "", "BBD314E2-B65A-4C23-8AE1-1ADFBD58C4B4", false );

            Sql( MigrationSQL._201905081957016_GroupScheduling_PopulateScheduleTemplates );

            PagesBlocksUp();

            AddSchedulingResponseEmailTemplateUp();

            AddSchedulingConfirmationSystemEmailUp();

            Sql( @"
-- Update existing GroupTypes to default ScheduleConfirmationEmailOffsetDays and ScheduleReminderEmailOffsetDays
UPDATE [GroupType]
SET [ScheduleConfirmationEmailOffsetDays] = 4
	,[ScheduleReminderEmailOffsetDays] = 2

-- Enable Group Scheduling for the Serving Teams group type, and set the default ScheduleConfirmationSystemEmailId
UPDATE [GroupType]
SET [IsSchedulingEnabled] = 1
	,ScheduleConfirmationSystemEmailId = (
		SELECT TOP 1 Id
		FROM SystemEmail
		WHERE [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'
		)
WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4'" );
        }

        /// <summary>
        /// Pageses the blocks up.
        /// </summary>
        private void PagesBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Schedule Templates", "", "1F50B5C5-2486-4D8F-9435-27BDF8302683", "fa fa-calendar-day" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "1F50B5C5-2486-4D8F-9435-27BDF8302683", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Schedule Template Detail", "", "B7B0864D-91F2-4B24-A7B0-FC7BEE769FA0", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Scheduler", "", "1815D8C6-7C4A-4C05-A810-CF23BA937477", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Schedule Status Board", "", "31576E5D-7B6C-46D1-89F4-A14F4F8095D1", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Schedule Confirmation", "", "EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Schedule Toolbox", "", "3CDBF848-0055-483B-9ED2-FB86AD0FE2C8", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Scheduler Analytics", "", "1E031B86-1476-4C72-9115-F94056398444", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "1815D8C6-7C4A-4C05-A810-CF23BA937477", "GroupScheduler/{GroupId}", "D0F198E2-6111-4EC1-8D1D-55AC10E28D04" );// for Page:Group Scheduler
            RockMigrationHelper.AddPageRoute( "1815D8C6-7C4A-4C05-A810-CF23BA937477", "GroupScheduler", "98CB9BAC-AE45-4EDB-BC31-352B889F908E" );// for Page:Group Scheduler
            RockMigrationHelper.AddPageRoute( "EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF", "ScheduleConfirmation", "00153288-DE3C-421A-AA7D-6555C8986D75" );// for Page:Schedule Confirmation
            RockMigrationHelper.AddPageRoute( "3CDBF848-0055-483B-9ED2-FB86AD0FE2C8", "ScheduleToolbox", "E87DF366-BCC6-4248-B7B8-3F66B58F52AC" );// for Page:Schedule Toolbox
            RockMigrationHelper.UpdateBlockType( "Group Member Schedule Template Detail", "Displays the details of a group member schedule template.", "~/Blocks/GroupScheduling/GroupMemberScheduleTemplateDetail.ascx", "Group Scheduling", "B5EB66A1-7391-49D5-B613-5ED804A31E7B" );
            RockMigrationHelper.UpdateBlockType( "Group Member Schedule Template List", "Lists group member schedule templates.", "~/Blocks/GroupScheduling/GroupMemberScheduleTemplateList.ascx", "Group Scheduling", "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7" );
            RockMigrationHelper.UpdateBlockType( "Group Scheduler", "Allows schedules for groups and locations to be managed by a scheduler.", "~/Blocks/GroupScheduling/GroupScheduler.ascx", "Group Scheduling", "37D43C21-1A4D-4B13-9555-EF0B7304EB8A" );
            RockMigrationHelper.UpdateBlockType( "Group Schedule Confirmation", "Allows a person to confirm a schedule RSVP and view pending schedules.", "~/Blocks/GroupScheduling/GroupScheduleConfirmation.ascx", "Group Scheduling", "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C" );
            RockMigrationHelper.UpdateBlockType( "Group Scheduler Analytics", "Provides some visibility into scheduling accountability. Shows check-ins, missed confirmations, declines, and decline reasons with ability to filter by group, date range, data view, and person.", "~/Blocks/GroupScheduling/GroupSchedulerAnalytics.ascx", "Group Scheduling", "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900" );
            RockMigrationHelper.UpdateBlockType( "Group Schedule Status Board", "Scheduler can see overview of current schedules by groups and dates.", "~/Blocks/GroupScheduling/GroupScheduleStatusBoard.ascx", "Group Scheduling", "1BFB72CC-A224-4A0B-B291-21733597738A" );
            RockMigrationHelper.UpdateBlockType( "Group Schedule Toolbox", "Allows management of group scheduling for a specific person (worker).", "~/Blocks/GroupScheduling/GroupScheduleToolbox.ascx", "Group Scheduling", "7F9CEA6F-DCE5-4F60-A551-924965289F1D" );
            // Add Block to Page: Internal Homepage Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "20F97A93-7949-4C2A-8A5E-C756FE8585CA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Dev Links", "Main", @"", @"", 0, "F337823D-BA5D-49F8-9BC4-1EF48C9000CE" );
            // Add Block to Page: Group Member Schedule Templates Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1F50B5C5-2486-4D8F-9435-27BDF8302683".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7".AsGuid(), "Group Member Schedule Template List", "Main", @"", @"", 0, "DFF3E9C7-1FB8-42E3-A6CB-5F28FC7DA564" );
            // Add Block to Page: Group Member Schedule Template Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B7B0864D-91F2-4B24-A7B0-FC7BEE769FA0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B5EB66A1-7391-49D5-B613-5ED804A31E7B".AsGuid(), "Group Member Schedule Template Detail", "Main", @"", @"", 0, "B251F51D-075A-4744-9788-F9AD89AA0552" );
            // Add Block to Page: Group Scheduler Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1815D8C6-7C4A-4C05-A810-CF23BA937477".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "37D43C21-1A4D-4B13-9555-EF0B7304EB8A".AsGuid(), "Group Scheduler", "Main", @"", @"", 0, "B282B285-4600-4097-9CC0-3439E2B66C34" );
            // Add Block to Page: Group Schedule Status Board Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "31576E5D-7B6C-46D1-89F4-A14F4F8095D1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "1BFB72CC-A224-4A0B-B291-21733597738A".AsGuid(), "Group Schedule Status Board", "Main", @"", @"", 0, "EEC694EB-C5A7-43E0-A8D5-77E52769252F" );
            // Add Block to Page: Schedule Confirmation Site: External Website
            RockMigrationHelper.AddBlock( true, "EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C".AsGuid(), "Group Schedule Confirmation", "Main", @"", @"", 0, "B1240109-EEAC-487D-A45C-FDF4375278A1" );
            // Add Block to Page: Schedule Toolbox Site: External Website
            RockMigrationHelper.AddBlock( true, "3CDBF848-0055-483B-9ED2-FB86AD0FE2C8".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "7F9CEA6F-DCE5-4F60-A551-924965289F1D".AsGuid(), "Group Schedule Toolbox", "Main", @"", @"", 0, "21FCFF03-2EBC-41D1-81A5-88BF456893AB" );
            // Add Block to Page: Group Scheduler Analytics Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1E031B86-1476-4C72-9115-F94056398444".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900".AsGuid(), "Group Scheduler Analytics", "Main", @"", @"", 0, "B54C01A9-1807-48A6-91B6-B34CE2F98107" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '60469A41-5180-446F-9935-0A09D81CD319'" );  // Page: Internal Homepage,  Zone: Main,  Block: Notification List
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '879BC5A7-3CE2-43FC-BEDB-B93B0054F417'" );  // Page: Internal Homepage,  Zone: Main,  Block: Internal Communication View
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F337823D-BA5D-49F8-9BC4-1EF48C9000CE'" );  // Page: Internal Homepage,  Zone: Main,  Block: Dev Links
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'" );  // Page: Internal Homepage,  Zone: Main,  Block: Install Checklist
            // Attrib for BlockType: Group Detail:Enable Group Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Tags", "EnableGroupTags", "", @"If enabled, the tags will be shown.", 17, @"True", "6C02D152-15E7-4601-9554-DB573D38554E" );
            // Attrib for BlockType: Group Detail:Group Scheduler Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Scheduler Page", "GroupSchedulerPage", "", @"The page to schedule this group.", 16, @"1815D8C6-7C4A-4C05-A810-CF23BA937477,D0F198E2-6111-4EC1-8D1D-55AC10E28D04", "62D1C332-CF70-41F8-8203-583089CF31AD" );
            // Attrib for BlockType: Group Scheduler:Number of Weeks To Show
            RockMigrationHelper.UpdateBlockTypeAttribute( "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Weeks To Show", "FutureWeeksToShow", "", @"The number of weeks out that can scheduled.", 0, @"6", "9F23627D-3223-437F-9E29-911F705145C7" );
            // Attrib for BlockType: Group Schedule Confirmation:Confirm Heading Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirm Heading Template", "ConfirmHeadingTemplate", "", @"Text to display when person confirms a schedule RSVP. <span class='tip tip-lava'></span>", 1, @"<h2 class='margin-t-none'>You’re confirmed to serve</h2><p>Thanks for letting us know.  You’re confirmed for:</p><p>{{ Group.Name }}<br>{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name }}<br></p>
<p>Thanks again!</p>
<p>{{ Group.Scheduler.FullName }}<br>{{ 'Global' | Attribute:'OrganizationName' }}</p>", "7726CD7E-3ECF-4ECE-BAB3-407493CF120C" );
            // Attrib for BlockType: Group Schedule Confirmation:Decline Heading Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Decline Heading Template", "DeclineHeadingTemplate", "", @"Text to display when person confirms a schedule RSVP. <span class='tip tip-lava'></span>", 2, @"<h2 class='margin-t-none'>Can’t make it?</h2><p>Thanks for letting us know.  We’ll try to schedule another person for:</p>
<p>{{ Group.Name }}<br>
{{ ScheduledItem.Location.Name }} {{ ScheduledItem.Schedule.Name }}<br></p>", "C1A4A35B-D5F3-497F-8951-FBF788B45DE9" );
            // Attrib for BlockType: Group Schedule Confirmation:Scheduling Response Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Scheduling Response Email", "SchedulingResponseEmail", "", @"The system email that will be used for sending responses back to the scheduler.", 8, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "6F1CF348-0CD3-44B0-A505-147992F9AF3E" );
            // Attrib for BlockType: Group Schedule Confirmation:Decline Note Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Decline Note Title", "DeclineNoteTitle", "", @"A custom title for the decline elaboration note.", 7, @"Please elaborate on why you cannot attend.", "5B14B8C9-F470-422D-8DFF-0D02070AFFAE" );
            // Attrib for BlockType: Group Schedule Confirmation:Require Decline Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Decline Note", "RequireDeclineNote", "", @"If checked, a custom note response will be required in order to save their decline status.", 6, @"False", "8E486D66-992B-4507-AF3A-8DBF22CC3403" );
            // Attrib for BlockType: Group Schedule Confirmation:Enable Decline Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Decline Note", "EnableDeclineNote", "", @"If checked, a note will be shown for the person to elaborate on why they cannot attend.", 5, @"False", "8860E132-E4DA-4844-881C-BFCEDFD3FABC" );
            // Attrib for BlockType: Group Schedule Confirmation:Scheduler Receive Confirmation Emails
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "", @"If checked, the scheduler will receive an email response for each confirmation or decline.", 3, @"False", "EE8CEEAB-8E65-40F0-B06A-3CF4E22D68C0" );
            // Attrib for BlockType: Group Schedule Confirmation:Require Decline Reasons
            RockMigrationHelper.UpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Decline Reasons", "RequireDeclineReasons", "", @"If checked, a person must choose one of the ‘Decline Reasons’ to submit their decline status.", 4, @"True", "7535ABCF-1F73-42C7-9EA6-557238E15278" );
            // Attrib for BlockType: Group Scheduler Analytics:Decline Chart Colors
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Decline Chart Colors", "DeclineChartColors", "", @"A comma-delimited list of colors that the decline reason chart will use. You will want as many colors as there are decline reasons.", 0, @"#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766", "D3CA6AA6-D7B1-450B-A93E-C3629BE79368" );
            // Attrib for BlockType: Group Scheduler Analytics:Attended
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Attended", "BarChartAttendedColor", "", @"Choose the color to show the number of schedule requests where the person attended.", 4, @"#66FF66", "948A44FE-7ED3-44DD-9151-53BE5FD6190D" );
            // Attrib for BlockType: Group Scheduler Analytics:Scheduled
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Scheduled", "BarChartScheduledColor", "", @"Choose the color to show the number of scheduled persons.", 1, @"#66B2FF", "11AE4B19-870B-4E51-87E5-E0F11626B9C9" );
            // Attrib for BlockType: Group Scheduler Analytics:No Response
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "D747E6AE-C383-4E22-8846-71518E3DD06F", "No Response", "BarChartNoResponseColor", "", @"Choose the color to show the number of schedule requests where the person did not respond.", 2, @"#FFFF66", "B7113CE2-377F-4387-923D-D9F20207AD82" );
            // Attrib for BlockType: Group Scheduler Analytics:Declines
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Declines", "BarChartDeclinesColor", "", @"Choose the color to show the number of schedule requests where the person declined.", 3, @"#FFB266", "E4840618-CB62-4A66-8D9B-68FFC2D1BBA9" );
            // Attrib for BlockType: Group Scheduler Analytics:Committed No Show
            RockMigrationHelper.UpdateBlockTypeAttribute( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Committed No Show", "BarChartCommittedNoShowColor", "", @"Choose the color to show the number of schedule requests where the person committed but did not attend.", 5, @"#FF6666", "B2F7E7DC-EB0D-4360-869D-6CA8FED671A6" );
            // Attrib for BlockType: Group Schedule Status Board:Number Of Weeks (Max 16)
            RockMigrationHelper.UpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number Of Weeks (Max 16)", "NumberOfWeeks", "", @"How many weeks into the future should be displayed.", 0, @"2", "92003B77-1D47-4394-8540-8FE78F4377D5" );
            // Attrib for BlockType: Group Schedule Status Board:Parent Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "1BFB72CC-A224-4A0B-B291-21733597738A", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Parent Group", "ParentGroup", "", @"A parent group to start from when allowing someone to pick one or more groups to view.", 0, @"", "B24C6793-86CE-46E6-AEE9-8B921EED8468" );
            // Attrib for BlockType: Group Schedule Toolbox:Number of Future Weeks To Show
            RockMigrationHelper.UpdateBlockTypeAttribute( "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Future Weeks To Show", "FutureWeeksToShow", "", @"The number of weeks into the future to allow users to signup for a schedule.", 0, @"6", "7016B03D-38C2-4CE9-8508-B04AC27C958C" );

            // Attrib for BlockType: Group Member Schedule Template List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "8869DC36-CB12-4842-BDA0-7C125A34F112" );
            // Attrib Value for Block:Group Member Schedule Template List, Attribute:Detail Page Page: Group Member Schedule Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DFF3E9C7-1FB8-42E3-A6CB-5F28FC7DA564", "8869DC36-CB12-4842-BDA0-7C125A34F112", @"b7b0864d-91f2-4b24-a7b0-fc7bee769fa0" );


            // Add Block to Page: Groups Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7F9CEA6F-DCE5-4F60-A551-924965289F1D".AsGuid(), "Group Schedule Toolbox", "SectionC1", @"", @"", 2, "47199FAE-BB88-4CDC-B9EA-5BAB72042D64" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'E18B1B2D-BF2A-43AD-BB9E-5DADBEFFB908'" );  // Page: Groups,  Zone: SectionC1,  Block: Person Group History
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '1CBE10C7-5E64-4385-BEE3-81DCA43DC47F'" );  // Page: Groups,  Zone: SectionC1,  Block: Group List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '47199FAE-BB88-4CDC-B9EA-5BAB72042D64'" );  // Page: Groups,  Zone: SectionC1,  Block: Group Schedule Toolbox


            // add ServiceJob: Send Group Schedule Confirmation and Reminder Emails
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendGroupScheduleNotifications' AND [Guid] = 'A7D45C92-18D7-42DD-83E9-CBD204C8A4C8' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Send Group Schedule Notifications'
                  ,'Sends Group Scheduling Confirmation and Reminder emails to people that haven''t been notified yet.'
                  ,'Rock.Jobs.SendGroupScheduleNotifications'
                  ,'0 0 16 1/1 * ? *'
                  ,1
                  ,'A7D45C92-18D7-42DD-83E9-CBD204C8A4C8'
                  );
            END" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Class", "Rock.Jobs.SendGroupScheduleNotifications", "Group", "Only people in or under this group will receive the schedule notifications emails.", 0, @"", "3BAA9243-1AAD-46A4-9363-BC6BD63B31B6", "RootGroup" );

            RockMigrationHelper.UpdateFieldType( "Value Filter", "", "Rock", "Rock.Field.Types.ValueFilterFieldType", "80ED0575-8FAE-4BC4-A51F-CAC211DD104F" );
        }

        /// <summary>
        /// Adds the scheduling confirmation system email up.
        /// </summary>
        private void AddSchedulingConfirmationSystemEmailUp()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Confirmation Email", "", "", "", "", "", "Scheduling Confirmation", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you'll be attending as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><h5>{{attendance.Occurrence.OccurrenceDate | Date:'dddd, MMMM, d, yyyy'}}</h5></td></tr>
    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
    {% if forloop.first  %}
    <tr><td><a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&isConfirmed=true"">Accept</a>&nbsp;<a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&isConfirmed=false"">Decline</a></td>
    </tr>
    <tr><td>&nbsp;</td></tr>
{% endif %}
{% endfor %}
</table>

<br/>

{{ 'Global' | Attribute:'EmailFooter' }}", "F8E4CE07-68F5-4169-A865-ECE915CF421C" );
        }

        /// <summary>
        /// Adds the scheduling response email template up.
        /// </summary>
        private void AddSchedulingResponseEmailTemplateUp()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Scheduling Response Email", "", "", "", "", "", "{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %}Accepted{% else %}Declined{% endif %}",
            @"{{ ""Global"" | Attribute:""EmailHeader"" }}
<h1>Scheduling Response</h1>
<p>Hi {{ Scheduler.NickName }}!</p>
<br/>
<p>{{ Person.FullName }}{%- assign rsvp = ScheduledItem.RSVP | AsBoolean -%} {% if rsvp %} has confirmed and will be at the:{%else %} is unable to attend the: {% endif %}</p>
<br/>
{{ Group.Name }}
{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name}}
<br/>
{{ ""Global"" | Attribute:""OrganizationName"" }}<br/>
<h2>{{ScheduledItem.Occurence.OccurenceDate | Date: ""dddd, MMMM, d, yyyy""}}</h2>
<p>&nbsp;</p>
{{ ""Global"" | Attribute:""EmailFooter"" }}", "D095F78D-A5CF-4EF6-A038-C7B07E250611" );
        }

        private void SchemaChangesUp()
        {
            CreateTable(
                            "dbo.GroupMemberAssignment",
                            c => new
                            {
                                Id = c.Int( nullable: false, identity: true ),
                                GroupMemberId = c.Int( nullable: false ),
                                LocationId = c.Int(),
                                ScheduleId = c.Int(),
                                CreatedDateTime = c.DateTime(),
                                ModifiedDateTime = c.DateTime(),
                                CreatedByPersonAliasId = c.Int(),
                                ModifiedByPersonAliasId = c.Int(),
                                Guid = c.Guid( nullable: false ),
                                ForeignId = c.Int(),
                                ForeignGuid = c.Guid(),
                                ForeignKey = c.String( maxLength: 100 ),
                            } )
                            .PrimaryKey( t => t.Id )
                            .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                            .ForeignKey( "dbo.GroupMember", t => t.GroupMemberId )
                            .ForeignKey( "dbo.Location", t => t.LocationId )
                            .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                            .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                            .Index( t => new { t.GroupMemberId, t.LocationId, t.ScheduleId }, unique: true, name: "IX_GroupMemberIdLocationIdScheduleId" )
                            .Index( t => t.CreatedByPersonAliasId )
                            .Index( t => t.ModifiedByPersonAliasId )
                            .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupMemberScheduleTemplate",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    GroupTypeId = c.Int(),
                    ScheduleId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.GroupType", t => t.GroupTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .Index( t => t.GroupTypeId )
                .Index( t => t.ScheduleId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupLocationScheduleConfig",
                c => new
                {
                    GroupLocationId = c.Int( nullable: false ),
                    ScheduleId = c.Int( nullable: false ),
                    MinimumCapacity = c.Int(),
                    DesiredCapacity = c.Int(),
                    MaximumCapacity = c.Int(),
                } )
                .PrimaryKey( t => new { t.GroupLocationId, t.ScheduleId } )
                .ForeignKey( "dbo.GroupLocation", t => t.GroupLocationId )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .Index( t => t.GroupLocationId )
                .Index( t => t.ScheduleId );

            CreateTable(
                "dbo.PersonScheduleExclusion",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int( nullable: false ),
                    Title = c.String( maxLength: 100 ),
                    StartDate = c.DateTime( nullable: false, storeType: "date" ),
                    EndDate = c.DateTime( nullable: false, storeType: "date" ),
                    GroupId = c.Int(),
                    ParentPersonScheduleExclusionId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Group", t => t.GroupId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonScheduleExclusion", t => t.ParentPersonScheduleExclusionId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.GroupId )
                .Index( t => t.ParentPersonScheduleExclusionId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.Group", "SchedulingMustMeetRequirements", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Group", "AttendanceRecordRequiredForCheckIn", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Group", "ScheduleCancellationPersonAliasId", c => c.Int() );
            AddColumn( "dbo.GroupType", "IsSchedulingEnabled", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleReminderSystemEmailId", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleCancellationWorkflowTypeId", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleConfirmationEmailOffsetDays", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleReminderEmailOffsetDays", c => c.Int() );
            AddColumn( "dbo.GroupType", "RequiresReasonIfDeclineSchedule", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.GroupMember", "ScheduleTemplateId", c => c.Int() );
            AddColumn( "dbo.GroupMember", "ScheduleStartDate", c => c.DateTime( storeType: "date" ) );
            AddColumn( "dbo.GroupMember", "ScheduleReminderEmailOffsetDays", c => c.Int() );
            AddColumn( "dbo.Attendance", "ScheduledToAttend", c => c.Boolean() );
            AddColumn( "dbo.Attendance", "RequestedToAttend", c => c.Boolean() );
            AddColumn( "dbo.Attendance", "ScheduleConfirmationSent", c => c.Boolean() );
            AddColumn( "dbo.Attendance", "ScheduleReminderSent", c => c.Boolean() );
            AddColumn( "dbo.Attendance", "RSVPDateTime", c => c.DateTime() );
            AddColumn( "dbo.Attendance", "DeclineReasonValueId", c => c.Int() );
            AddColumn( "dbo.Attendance", "ScheduledByPersonAliasId", c => c.Int() );
            CreateIndex( "dbo.Group", "ScheduleCancellationPersonAliasId" );
            CreateIndex( "dbo.GroupType", "ScheduleConfirmationSystemEmailId" );
            CreateIndex( "dbo.GroupType", "ScheduleReminderSystemEmailId" );
            CreateIndex( "dbo.GroupType", "ScheduleCancellationWorkflowTypeId" );
            CreateIndex( "dbo.GroupMember", "ScheduleTemplateId" );
            CreateIndex( "dbo.Attendance", "DeclineReasonValueId" );
            CreateIndex( "dbo.Attendance", "ScheduledByPersonAliasId" );
            AddForeignKey( "dbo.GroupType", "ScheduleCancellationWorkflowTypeId", "dbo.WorkflowType", "Id" );
            AddForeignKey( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", "dbo.SystemEmail", "Id" );
            AddForeignKey( "dbo.GroupType", "ScheduleReminderSystemEmailId", "dbo.SystemEmail", "Id" );
            AddForeignKey( "dbo.GroupMember", "ScheduleTemplateId", "dbo.GroupMemberScheduleTemplate", "Id" );
            AddForeignKey( "dbo.Group", "ScheduleCancellationPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Attendance", "DeclineReasonValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Attendance", "ScheduledByPersonAliasId", "dbo.PersonAlias", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SchemaChangesDown();

            // Down for AddSchedulingResponseEmailTemplate
            RockMigrationHelper.DeleteSystemEmail( "D095F78D-A5CF-4EF6-A038-C7B07E250611" );

            // Down for AddSchedulingUpdateSystemEmail
            RockMigrationHelper.DeleteSystemEmail( "F8E4CE07-68F5-4169-A865-ECE915CF421C" );

            PagesBlocksDown();
        }

        /// <summary>
        /// Pageses the blocks down.
        /// </summary>
        private void PagesBlocksDown()
        {
            // Attrib for BlockType: Group Scheduler Analytics:Committed No Show
            RockMigrationHelper.DeleteAttribute( "B2F7E7DC-EB0D-4360-869D-6CA8FED671A6" );
            // Attrib for BlockType: Group Detail:Group Scheduler Page
            RockMigrationHelper.DeleteAttribute( "62D1C332-CF70-41F8-8203-583089CF31AD" );
            // Attrib for BlockType: Group Detail:Enable Group Tags
            RockMigrationHelper.DeleteAttribute( "6C02D152-15E7-4601-9554-DB573D38554E" );
            // Attrib for BlockType: Group Scheduler:Number of Weeks To Show
            RockMigrationHelper.DeleteAttribute( "9F23627D-3223-437F-9E29-911F705145C7" );
            // Attrib for BlockType: Group Schedule Toolbox:Number of Future Weeks To Show
            RockMigrationHelper.DeleteAttribute( "7016B03D-38C2-4CE9-8508-B04AC27C958C" );
            // Attrib for BlockType: Group Schedule Status Board:Parent Group
            RockMigrationHelper.DeleteAttribute( "B24C6793-86CE-46E6-AEE9-8B921EED8468" );
            // Attrib for BlockType: Group Schedule Status Board:Number Of Weeks (Max 16)
            RockMigrationHelper.DeleteAttribute( "92003B77-1D47-4394-8540-8FE78F4377D5" );
            // Attrib for BlockType: Group Scheduler Analytics:Declines
            RockMigrationHelper.DeleteAttribute( "E4840618-CB62-4A66-8D9B-68FFC2D1BBA9" );
            // Attrib for BlockType: Group Scheduler Analytics:No Response
            RockMigrationHelper.DeleteAttribute( "B7113CE2-377F-4387-923D-D9F20207AD82" );
            // Attrib for BlockType: Group Scheduler Analytics:Scheduled
            RockMigrationHelper.DeleteAttribute( "11AE4B19-870B-4E51-87E5-E0F11626B9C9" );
            // Attrib for BlockType: Group Scheduler Analytics:Attended
            RockMigrationHelper.DeleteAttribute( "948A44FE-7ED3-44DD-9151-53BE5FD6190D" );
            // Attrib for BlockType: Group Scheduler Analytics:Decline Chart Colors
            RockMigrationHelper.DeleteAttribute( "D3CA6AA6-D7B1-450B-A93E-C3629BE79368" );
            // Attrib for BlockType: Group Schedule Confirmation:Require Decline Reasons
            RockMigrationHelper.DeleteAttribute( "7535ABCF-1F73-42C7-9EA6-557238E15278" );
            // Attrib for BlockType: Group Schedule Confirmation:Scheduler Receive Confirmation Emails
            RockMigrationHelper.DeleteAttribute( "EE8CEEAB-8E65-40F0-B06A-3CF4E22D68C0" );
            // Attrib for BlockType: Group Schedule Confirmation:Decline Heading Template
            RockMigrationHelper.DeleteAttribute( "C1A4A35B-D5F3-497F-8951-FBF788B45DE9" );
            // Attrib for BlockType: Group Schedule Confirmation:Confirm Heading Template
            RockMigrationHelper.DeleteAttribute( "7726CD7E-3ECF-4ECE-BAB3-407493CF120C" );
            // Attrib for BlockType: Group Schedule Confirmation:Scheduling Response Email
            RockMigrationHelper.DeleteAttribute( "6F1CF348-0CD3-44B0-A505-147992F9AF3E" );
            // Attrib for BlockType: Group Schedule Confirmation:Decline Note Title
            RockMigrationHelper.DeleteAttribute( "5B14B8C9-F470-422D-8DFF-0D02070AFFAE" );
            // Attrib for BlockType: Group Schedule Confirmation:Enable Decline Note
            RockMigrationHelper.DeleteAttribute( "8860E132-E4DA-4844-881C-BFCEDFD3FABC" );
            // Attrib for BlockType: Group Schedule Confirmation:Require Decline Note
            RockMigrationHelper.DeleteAttribute( "8E486D66-992B-4507-AF3A-8DBF22CC3403" );
            // Remove Block: Group Scheduler Analytics, from Page: Group Scheduler Analytics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B54C01A9-1807-48A6-91B6-B34CE2F98107" );
            // Remove Block: Group Schedule Toolbox, from Page: Schedule Toolbox, Site: External Website
            RockMigrationHelper.DeleteBlock( "21FCFF03-2EBC-41D1-81A5-88BF456893AB" );
            // Remove Block: Group Schedule Confirmation, from Page: Schedule Confirmation, Site: External Website
            RockMigrationHelper.DeleteBlock( "B1240109-EEAC-487D-A45C-FDF4375278A1" );
            // Remove Block: Group Schedule Status Board, from Page: Group Schedule Status Board, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EEC694EB-C5A7-43E0-A8D5-77E52769252F" );
            // Remove Block: Group Scheduler, from Page: Group Scheduler, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B282B285-4600-4097-9CC0-3439E2B66C34" );
            // Remove Block: Group Member Schedule Template Detail, from Page: Group Member Schedule Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B251F51D-075A-4744-9788-F9AD89AA0552" );
            // Remove Block: Group Member Schedule Template List, from Page: Group Member Schedule Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DFF3E9C7-1FB8-42E3-A6CB-5F28FC7DA564" );
            // Remove Block: Dev Links, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F337823D-BA5D-49F8-9BC4-1EF48C9000CE" );
            RockMigrationHelper.DeleteBlockType( "7F9CEA6F-DCE5-4F60-A551-924965289F1D" ); // Group Schedule Toolbox
            RockMigrationHelper.DeleteBlockType( "1BFB72CC-A224-4A0B-B291-21733597738A" ); // Group Schedule Status Board
            RockMigrationHelper.DeleteBlockType( "3037D2F8-C9B0-40BF-B2E3-8C5CBA7F4900" ); // Group Scheduler Analytics
            RockMigrationHelper.DeleteBlockType( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C" ); // Group Schedule Confirmation
            RockMigrationHelper.DeleteBlockType( "37D43C21-1A4D-4B13-9555-EF0B7304EB8A" ); // Group Scheduler
            RockMigrationHelper.DeleteBlockType( "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7" ); // Group Member Schedule Template List
            RockMigrationHelper.DeleteBlockType( "B5EB66A1-7391-49D5-B613-5ED804A31E7B" ); // Group Member Schedule Template Detail
            RockMigrationHelper.DeletePage( "1E031B86-1476-4C72-9115-F94056398444" ); //  Page: Group Scheduler Analytics, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "3CDBF848-0055-483B-9ED2-FB86AD0FE2C8" ); //  Page: Schedule Toolbox, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF" ); //  Page: Schedule Confirmation, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "31576E5D-7B6C-46D1-89F4-A14F4F8095D1" ); //  Page: Group Schedule Status Board, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1815D8C6-7C4A-4C05-A810-CF23BA937477" ); //  Page: Group Scheduler, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B7B0864D-91F2-4B24-A7B0-FC7BEE769FA0" ); //  Page: Group Member Schedule Template Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "1F50B5C5-2486-4D8F-9435-27BDF8302683" ); //  Page: Group Member Schedule Templates, Layout: Full Width, Site: Rock RMS
        }


        private void SchemaChangesDown()
        {
            DropForeignKey( "dbo.PersonScheduleExclusion", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonScheduleExclusion", "ParentPersonScheduleExclusionId", "dbo.PersonScheduleExclusion" );
            DropForeignKey( "dbo.PersonScheduleExclusion", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonScheduleExclusion", "GroupId", "dbo.Group" );
            DropForeignKey( "dbo.PersonScheduleExclusion", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Attendance", "ScheduledByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Attendance", "DeclineReasonValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupLocationScheduleConfig", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.GroupLocationScheduleConfig", "GroupLocationId", "dbo.GroupLocation" );
            DropForeignKey( "dbo.Group", "ScheduleCancellationPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMember", "ScheduleTemplateId", "dbo.GroupMemberScheduleTemplate" );
            DropForeignKey( "dbo.GroupMemberScheduleTemplate", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.GroupMemberScheduleTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMemberScheduleTemplate", "GroupTypeId", "dbo.GroupType" );
            DropForeignKey( "dbo.GroupMemberScheduleTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMemberAssignment", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.GroupMemberAssignment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMemberAssignment", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo.GroupMemberAssignment", "GroupMemberId", "dbo.GroupMember" );
            DropForeignKey( "dbo.GroupMemberAssignment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupType", "ScheduleReminderSystemEmailId", "dbo.SystemEmail" );
            DropForeignKey( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", "dbo.SystemEmail" );
            DropForeignKey( "dbo.GroupType", "ScheduleCancellationWorkflowTypeId", "dbo.WorkflowType" );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "Guid" } );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "ParentPersonScheduleExclusionId" } );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "GroupId" } );
            DropIndex( "dbo.PersonScheduleExclusion", new[] { "PersonAliasId" } );
            DropIndex( "dbo.Attendance", new[] { "ScheduledByPersonAliasId" } );
            DropIndex( "dbo.Attendance", new[] { "DeclineReasonValueId" } );
            DropIndex( "dbo.GroupLocationScheduleConfig", new[] { "ScheduleId" } );
            DropIndex( "dbo.GroupLocationScheduleConfig", new[] { "GroupLocationId" } );
            DropIndex( "dbo.GroupMemberScheduleTemplate", new[] { "Guid" } );
            DropIndex( "dbo.GroupMemberScheduleTemplate", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberScheduleTemplate", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberScheduleTemplate", new[] { "ScheduleId" } );
            DropIndex( "dbo.GroupMemberScheduleTemplate", new[] { "GroupTypeId" } );
            DropIndex( "dbo.GroupMemberAssignment", new[] { "Guid" } );
            DropIndex( "dbo.GroupMemberAssignment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberAssignment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberAssignment", "IX_GroupMemberIdLocationIdScheduleId" );
            DropIndex( "dbo.GroupMember", new[] { "ScheduleTemplateId" } );
            DropIndex( "dbo.GroupType", new[] { "ScheduleCancellationWorkflowTypeId" } );
            DropIndex( "dbo.GroupType", new[] { "ScheduleReminderSystemEmailId" } );
            DropIndex( "dbo.GroupType", new[] { "ScheduleConfirmationSystemEmailId" } );
            DropIndex( "dbo.Group", new[] { "ScheduleCancellationPersonAliasId" } );
            DropColumn( "dbo.Attendance", "ScheduledByPersonAliasId" );
            DropColumn( "dbo.Attendance", "DeclineReasonValueId" );
            DropColumn( "dbo.Attendance", "RSVPDateTime" );
            DropColumn( "dbo.Attendance", "ScheduleReminderSent" );
            DropColumn( "dbo.Attendance", "ScheduleConfirmationSent" );
            DropColumn( "dbo.Attendance", "RequestedToAttend" );
            DropColumn( "dbo.Attendance", "ScheduledToAttend" );
            DropColumn( "dbo.GroupMember", "ScheduleReminderEmailOffsetDays" );
            DropColumn( "dbo.GroupMember", "ScheduleStartDate" );
            DropColumn( "dbo.GroupMember", "ScheduleTemplateId" );
            DropColumn( "dbo.GroupType", "RequiresReasonIfDeclineSchedule" );
            DropColumn( "dbo.GroupType", "ScheduleReminderEmailOffsetDays" );
            DropColumn( "dbo.GroupType", "ScheduleConfirmationEmailOffsetDays" );
            DropColumn( "dbo.GroupType", "ScheduleCancellationWorkflowTypeId" );
            DropColumn( "dbo.GroupType", "ScheduleReminderSystemEmailId" );
            DropColumn( "dbo.GroupType", "ScheduleConfirmationSystemEmailId" );
            DropColumn( "dbo.GroupType", "IsSchedulingEnabled" );
            DropColumn( "dbo.Group", "ScheduleCancellationPersonAliasId" );
            DropColumn( "dbo.Group", "AttendanceRecordRequiredForCheckIn" );
            DropColumn( "dbo.Group", "SchedulingMustMeetRequirements" );
            DropTable( "dbo.PersonScheduleExclusion" );
            DropTable( "dbo.GroupLocationScheduleConfig" );
            DropTable( "dbo.GroupMemberScheduleTemplate" );
            DropTable( "dbo.GroupMemberAssignment" );
        }
    }
}
