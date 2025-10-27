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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250805 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MigrateTransactionBlocksFor17_3Up();
            DeleteObsoleteStarkDynamicAttributesBlock();
            UpdateRestActionDetailBlockToLegacy();
            JPH_RevertHiddenApplicationGroupTypeAddition_20250715_Up();
            UpdateAttendanceOccurrenceIndexUp();
            AddUpdateNamelessSchedulesJob();
            FixBlockTypeDetailBreadcrumbs();
            AddDeleteSelfServiceKioskSiteAndBlocksJob();
            FixIfScheduleConfirmationEmailUpdateRanDuringPreAlpha();
            ReorganizeCommunicationSaturationReportPages_Up();
            UpdateFileAssetManagerBlockAttributes();
            JPH_UpdateFallbackChatNotificationSystemCommunication_20250721_Up();
            UpdateIconTransitionTable();
            JPH_UpdatePeerNetworkLavaTemplates_20250722_Up();
            UpdateLMSLearningCourseDetailLavaForCompletionsUp();
            UpdateRockTheme();
            ChopBlocksForV18Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MigrateTransactionBlocksFor17_3Down();
            JPH_UpdatePeerNetworkLavaTemplates_20250722_Down();
            UpdateLMSLearningCourseDetailLavaForCompletionsDown();
        }

        #region NA: MigrateTransactionBlocksFor17_3 (Plugin Migration #259)

        private void MigrateTransactionBlocksFor17_3Up()
        {
            Sql( @"
UPDATE [BlockType] SET [Name] = 'Scheduled Transaction Edit (Legacy)' WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE';
UPDATE [BlockType] SET [Name] = 'Scheduled Transaction Edit' WHERE [Guid] = 'F1ADF375-7442-4B30-BAC3-C387EA9B6C18';
UPDATE [BlockType] SET [Path] = '~/Blocks/Finance/TransactionEntryLegacy.ascx' WHERE [Path] = '~/Blocks/Finance/TransactionEntry.ascx';
-- Now rename the Transaction Entry (V2) block to just 'Transaction Entry'
UPDATE [BlockType] SET [Name] = 'Transaction Entry' WHERE [Guid] = '6316D801-40C0-4EED-A2AD-55C13870664D'
" );
        }

        private void MigrateTransactionBlocksFor17_3Down()
        {
            Sql( @"
UPDATE [BlockType] SET [Name] = 'Scheduled Transaction Edit' WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE';
UPDATE [BlockType] SET [Name] = 'Scheduled Transaction Edit (V2)' WHERE [Guid] = 'F1ADF375-7442-4B30-BAC3-C387EA9B6C18';
UPDATE [BlockType] SET [Path] = '~/Blocks/Finance/TransactionEntry.ascx' WHERE [Path] = '~/Blocks/Finance/TransactionEntryLegacy.ascx';
UPDATE [BlockType] SET [Name] = 'Transaction Entry (V2)' WHERE [Guid] = '6316D801-40C0-4EED-A2AD-55C13870664D'
" );
        }

        #endregion

        #region KH: MigrationRollupsForV17_3_1 (Plugin Migration #260)

        #region JH: Delete Obsolete StarkDynamicAttributes Block

        private void DeleteObsoleteStarkDynamicAttributesBlock()
        {
            Sql( @"
DECLARE @StarkDynamicAttributesBlockTypeGuid UNIQUEIDENTIFIER = '7c34a0fa-ed0d-4b8b-b458-6ec970711726';
DECLARE @BlockTypeId INT;

-- Look up the BlockType.Id if it exists
SELECT @BlockTypeId = [Id]
FROM [BlockType]
WHERE [Guid] = @StarkDynamicAttributesBlockTypeGuid ;

-- If it exists, delete dependent Blocks and then the BlockType
IF @BlockTypeId IS NOT NULL
BEGIN
    DELETE FROM [Block]
    WHERE [BlockTypeId] = @BlockTypeId;

    DELETE FROM [BlockType]
    WHERE [Id] = @BlockTypeId;
END;
" );
        }

        #endregion

        #region NA: Update REST Action Detail block to Legacy

        private void UpdateRestActionDetailBlockToLegacy()
        {
            Sql( @"
  UPDATE BlockType SET [Name] = 'REST Action Detail (Legacy)'
  WHERE [Guid] = '5BB83A28-CED2-4B40-9FDA-9C3D21FD6A83'
" );
        }

        #endregion

        #region JPH: Revert addition of "Hidden Application Group" Group Type and associated Role

        /// <summary>
        /// JPH: Revert the addition of the "Hidden Application Group" group type and associated role.
        /// </summary>
        /// <remarks>
        /// The "Chat Ban List" group was the only group seeded with these references, and they'll be shifted to the
        /// preexisting "Application Group" group type and associated role below.
        /// </remarks>
        private void JPH_RevertHiddenApplicationGroupTypeAddition_20250715_Up()
        {
            /*
                7/15/2025 - JPH

                When adding Rock's Chat feature, we originally added a "Hidden Application Group" group type and
                associated "Hidden Application Group Member" group type role. Unfortunately, a loose migration updated
                all existing "Application Group Member" group member records to be assigned to the new "Hidden .." role,
                which caused the photo verification process to break.

                While addressing this issue, it was decided that the "Hidden .." group type and role are not needed, and
                the "Chat Ban List" group can instead simply use the preexisting "Application Group" group type and
                associated role. This migration serves to update targeted group member records back to the "Application
                Group Member" role and also delete the no-longer-needed "Hidden .." group type and role.

                Reason: Remove "Hidden Application Group Type" and associated role that are not needed.
                https://github.com/SparkDevNetwork/Rock/issues/6369
                https://app.asana.com/1/20866866924293/project/1208321217019996/task/1210746747838998
            */

            Sql( @"
-- Revert the previous change that set all 'Application Group Member' records to 'Hidden Application Group Member'.
DECLARE @ApplicationGroupMemberRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '09B14358-FA17-4D65-A8E9-03FA7312CD62');
DECLARE @HiddenApplicationGroupMemberRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2008B263-CD41-45F0-8033-26D949FC0DA7');

UPDATE [GroupMember]
SET [GroupRoleId] = @ApplicationGroupMemberRoleId
WHERE [GroupRoleId] = @HiddenApplicationGroupMemberRoleId;

-- Update any groups and group members currently referencing the 'Hidden Application Group' group type to instead
-- reference the 'Application Group' group type. There aren't likely to be any.
DECLARE @ApplicationGroupGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49');
DECLARE @HiddenApplicationGroupGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '2C6F2847-B404-4595-AB35-CE42F2303868');

UPDATE [Group]
SET [GroupTypeId] = @ApplicationGroupGroupTypeId
WHERE [GroupTypeId] = @HiddenApplicationGroupGroupTypeId;

UPDATE [GroupMember]
SET [GroupTypeId] = @ApplicationGroupGroupTypeId
WHERE [GroupTypeId] = @HiddenApplicationGroupGroupTypeId;" );

            try
            {
                // Delete the "Hidden Application Group" group type and associated role.
                RockMigrationHelper.DeleteGroupTypeRole( "2008B263-CD41-45F0-8033-26D949FC0DA7" );
                RockMigrationHelper.DeleteGroupType( "2C6F2847-B404-4595-AB35-CE42F2303868" );
            }
            catch
            {
                // Fail silently.
            }
        }

        #endregion

        #region KH: Update Attendance Occurrence Index

        private void UpdateAttendanceOccurrenceIndexUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.3 - Update AttendanceOccurrence Index",
                description: "This job will remove a redundant index from the AttendanceOccurrence table.",
                jobType: "Rock.Jobs.PostV173UpdateAttendanceOccurrenceIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_173_UPDATE_ATTENDANCEOCCURRENCE_INDEX );
        }

        #endregion

        #region KH: Add Update Nameless Schedules Job

        private void AddUpdateNamelessSchedulesJob()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.3 - Update Nameless Schedules Job",
                description: "This job will update Nameless Schedules to store a Friendly Name in their Description Column.",
                jobType: "Rock.Jobs.PostV173UpdateNamelessSchedules",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_173_UPDATE_NAMELESS_SCHEDULES );
        }

        #endregion

        #endregion

        #region NA: After Chop (or during same release), Uncheck "Show Name in Breadcrumb" page setting

        private void FixBlockTypeDetailBreadcrumbs()
        {
            // For the Block Type Detail page, don't show the page name in the breadcrumb.
            // Instead, the block will add the block type's name in the breadcrumb.
            Sql( @"UPDATE [Page] SET [BreadCrumbDisplayName] = '0' WHERE [Guid] = 'C694AD7C-46DD-47FE-B2AC-1CF158FA6504'" );
        }

        #endregion

        #region NA: Register "Rock Update Helper v18.0 - Delete Self-Service Kiosk Site and Blocks"

        private void AddDeleteSelfServiceKioskSiteAndBlocksJob()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Delete Self-Service Kiosk Site and Blocks",
                description: "This job will delete the deprecated Self-Service Kiosk (Preview) website, the corresponding 'Kiosk' blocks, and KioskStark theme from the database and file system. See https://www.rockrms.com/tech-bulletin/removal-of-obsoleted-kiosk-blocks for details.",
                jobType: "Rock.Jobs.PostV18DeleteSelfServiceKioskSiteAndBlocks",
                cronExpression: "0 0 8 1 1/1 ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS
            );
        }

        #endregion

        #region NA: Fix anyone who ran Plugin Hotfix migration 255 before it was fixed in v17.2.4

        /// <summary>
        /// This data migration will fix the Schedule Confirmation (System Communication) template if it
        /// ran during the original pre-alpha (18.0.9) when it was broken in Rollup_20250715
        /// <see cref="T:Rock.Migrations.Rollup_20250715.UpdateGroupTypeScheduleConfirmationEmailUp"/>
        /// when it had the Lava code "if forloop.first" seen here:
        /// See https://github.com/SparkDevNetwork/Rock/commit/db69d6d1b29d986579a9449dacba35874d71139f#diff-f81bf98ba30168baa35509fdd2bd46f005a933cde0a249fb681a4e4e4b613769R971-R1025
        ///
        /// This was later fixed in the 17.2.4 release (<see cref="MigrationRollupsForV17_2_1.UpdateGroupTypeScheduleConfirmationEmailUp"/>),
        /// but if it already ran during the pre-alpha, it would still be broken until this runs.
        /// See https://github.com/SparkDevNetwork/Rock/commit/0e59bca4cb55c872a319dbed341842d5e131f015#diff-7ec70c3aa43329db7ba46d9b21e2b2ba8277bf2ed5a87e533b61dd791572aa8e
        /// </summary>
        private void FixIfScheduleConfirmationEmailUpdateRanDuringPreAlpha()
        {
            Sql( @"
DECLARE @SchedulingConfirmationEmailOneButtonGuid UNIQUEIDENTIFIER = 'BA1716E0-6B31-4E93-ABA1-42B3C81FDBDC';

DECLARE @Body NVARCHAR(MAX) = N'{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{ Attendance.PersonAlias.Person.NickName }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you''ll attend as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName }}
<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}

<table>

{% assign acceptText = ''Accept'' %}
{% assign declineText = ''Decline'' %}
{% if Attendances.size > 1 %}
    {% assign acceptText = ''Accept All'' %}
    {% assign declineText = ''Decline All'' %}
{% endif %}
{% capture attendanceIdList %}{% for attendance in Attendances %}{{ attendance.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '''' %}

{% assign scheduleConfirmationLogic = ''AutoAccept'' %}
{% for attendance in Attendances %}
{% if attendance.RSVP != ''Yes'' and attendance.Occurrence.Group.GroupType.ScheduleConfirmationLogic == ''Ask'' and attendance.Occurrence.Group.ScheduleConfirmationLogic == null %}
    {% assign scheduleConfirmationLogic = ''Ask'' %}
{% elseif attendance.RSVP != ''Yes'' and attendance.Occurrence.Group.ScheduleConfirmationLogic == ''Ask'' %} 
  {% assign scheduleConfirmationLogic = ''Ask'' %}
{% endif %}

{% assign currentDate = attendance.Occurrence.OccurrenceDate | Date:''dddd, MMMM d, yyyy'' %}
  {% if lastDate != currentDate %}
    {% if lastDate != '''' %}
    <tr><td><hr /></td></tr>
    {% endif %}
    <tr><td><h5>{{ currentDate }}</h5></td></tr>
    {% assign lastDate = currentDate %}
  {% else %}
    <tr><td>&nbsp;</td></tr>
  {% endif %}

    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Occurrence.Location.Name }}&nbsp;{{ attendance.Occurrence.Schedule.Name }}</td></tr>

  {% assign AttendancePerson = Attendance.PersonAlias.Person %}

{% endfor %}

    <tr><td><hr /></td></tr>
    <tr><td>
        <!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceIds={{attendanceIdList | UrlEncode}}&Person={{AttendancePerson | PersonActionIdentifier:''ScheduleConfirm''}}"" style=""height:38px;v-text-anchor:middle;width:275px;"" arcsize=""5%"" strokecolor=""#009ce3"" fillcolor=""#33cfe3"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">Required: Confirm or Decline</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#009ce3;border:1px solid ##33cfe3;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:275px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceIds={{attendanceIdList | UrlEncode}}&Person={{AttendancePerson | PersonActionIdentifier:''ScheduleConfirm''}}"">Required: Confirm or Decline</a>&nbsp;
    </td>
    </tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td><a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleToolbox"">View Schedule Toolbox</a></td></tr>
    <tr><td>&nbsp;</td></tr>
</table>

<br/>

{{ ''Global'' | Attribute:''EmailFooter'' }}';

-- Update the Scheduling Confirmation Email (One Button) template but only if it still has the broken Lava code.
IF EXISTS (
    SELECT 1
    FROM [SystemCommunication]
    WHERE [Guid] = @SchedulingConfirmationEmailOneButtonGuid AND CHARINDEX('if forloop.first', [Body]) > 0
)
BEGIN
	UPDATE [SystemCommunication] SET [Body] = @Body WHERE [Guid] = @SchedulingConfirmationEmailOneButtonGuid;
END;
            " );
        }

        #endregion

        #region NA: Reorganize Communication Saturation Report Pages

        /// <summary>
        /// Migrates and reorganizes pages related to communication reports.
        /// </summary>
        public void ReorganizeCommunicationSaturationReportPages_Up()
        {
            var newCommunicationPageLegacyGuid = "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857";
            var reportPagePageMenuBlockGuid = "5212839D-4F00-47E2-AAE6-40305890DFEE";

            // Step 1, create a new page called "Communication Reports" under the "Communications" page.
            RockMigrationHelper.AddPage( true, "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Reports", "", Rock.SystemGuid.Page.COMMUNICATION_REPORTS );

            // Step 1.5, add a PageAsList block to the new "Communication Reports" page.
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.COMMUNICATION_REPORTS.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), Rock.SystemGuid.BlockType.PAGE_MENU.AsGuid(), "Page Menu", "Main", @"", @"", 0, reportPagePageMenuBlockGuid );
            RockMigrationHelper.AddBlockAttributeValue( true, reportPagePageMenuBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Step 2, move the "Communication Saturation Report" page under the new "Communication Reports" page (Order 3).
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.COMMUNICATION_SATURATION, Rock.SystemGuid.Page.COMMUNICATION_REPORTS );

            // Step 2.5, Set page icon to ti ti-mailbox
            RockMigrationHelper.UpdatePageIcon( Rock.SystemGuid.Page.COMMUNICATION_SATURATION, "ti ti-mailbox" );

            // Step 3, rename the "Communication Saturation Report" page to "Communication Saturation".
            RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.COMMUNICATION_SATURATION, "Communication Saturation" );

            // Step 4, Move the "Communication History" page under the new "Communication Reports" page (Order 1)
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.COMMUNICATION_HISTORY, Rock.SystemGuid.Page.COMMUNICATION_REPORTS );

            // Step 4.5, Set page icon to ti ti-messages
            RockMigrationHelper.UpdatePageIcon( Rock.SystemGuid.Page.COMMUNICATION_HISTORY, "ti ti-messages" );

            // Step 5, Move the "Email Analytics"  page under the new "Communication Reports" page (Order 2).
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.EMAIL_ANALYTICS, Rock.SystemGuid.Page.COMMUNICATION_REPORTS );

            // Step 5.5, Set page icon to ti ti-chart-bar
            RockMigrationHelper.UpdatePageIcon( Rock.SystemGuid.Page.EMAIL_ANALYTICS, "ti ti-chart-bar" );

            // Step 6, Rename the "New Communication" page to "New Communication (Legacy)".
            RockMigrationHelper.RenamePage( newCommunicationPageLegacyGuid, "New Communication (Legacy)" );

            // Step 7, Rename the "New Communication (Preview)" page to "New Communication".
            RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN, "New Communication" );

            // Step 8, Re-order the pages
            Sql( $@"
                DECLARE @orderOldBlock int
                DECLARE @orderNewBlock int
                SELECT @orderOldBlock = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.NEW_COMMUNICATION}'
                SELECT @orderNewBlock = [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}'
                -- Swap the two block's orders:
                UPDATE [Page] SET [Order] = @orderOldBlock WHERE [Guid] = '{Rock.SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}'
                UPDATE [Page] SET [Order] = @orderNewBlock WHERE [Guid] = '{Rock.SystemGuid.Page.NEW_COMMUNICATION}'
            " );

        }

        #endregion

        #region JZ: Update File Asset Manager Block Attributes - Height Mode

        private void UpdateFileAssetManagerBlockAttributes()
        {
            const string fileAssetManagerBlockTypeGuid = "535500A7-967F-4DA3-8FCA-CB844203CB3D";
            const string heightModeAttributeGuid = "67ECB409-F5C5-4487-A60B-FD572B99D95B";

            // First we'll add the new HeightMode attribute for the FileAssetManager BlockType...

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( fileAssetManagerBlockTypeGuid, SystemGuid.FieldType.SINGLE_SELECT, "Height Mode", "HeightMode", "Height Mode", @"Static lets you set a CSS height below to determine the height of the block. Flexible will grow with the content. Full Worksurface is designed to fill up a full worksurface page layout.", 2, @"static", heightModeAttributeGuid );

            // Now we migrate the older block setting values to the new block setting...

            Sql( @"
DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE Guid = '535500A7-967F-4DA3-8FCA-CB844203CB3D') -- File Asset Manager

-- The new attribute
DECLARE @HeightModeAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'HeightMode' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

-- The old attribute
DECLARE @IsStaticHeightAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'IsStaticHeight' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @BlockId INT
DECLARE @IsStaticHeight VARCHAR(50)
DECLARE @TheValue VARCHAR(50)

-- Find all block instances of the File Asset Manager block type
DECLARE block_cursor CURSOR FOR
SELECT [Id] FROM [Block] WHERE BlockTypeId = @BlockTypeId
OPEN block_cursor
FETCH NEXT FROM block_cursor INTO @BlockId
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @IsStaticHeight = [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @IsStaticHeightAttributeId
    SET @TheValue = CASE
        WHEN @IsStaticHeight = 'True' THEN 'static'
        WHEN @IsStaticHeight = 'False' THEN 'flexible'
        ELSE 'static'
    END
    IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @HeightModeAttributeId)  
    BEGIN  
        UPDATE [AttributeValue]   
        SET [Value] = @TheValue, [IsPersistedValueDirty] = 1 
        WHERE [EntityId] = @BlockId AND [AttributeId] = @HeightModeAttributeId;  
    END  
    ELSE  
    BEGIN  
        INSERT INTO [AttributeValue] (
            [IsSystem],
            [AttributeId],
            [EntityId],
            [Value],
            [Guid],
            [IsPersistedValueDirty])
        VALUES(
            1,
            @HeightModeAttributeId,
            @BlockId,
            @TheValue,
            NEWID(),
            1) 
    END
    FETCH NEXT FROM block_cursor INTO @BlockId
END
CLOSE block_cursor
DEALLOCATE block_cursor
" );
        }

        #endregion

        #region JPH: Update Fallback Chat Notification System Communication

        /// <summary>
        /// JPH: Updates the Fallback Chat Notification System Communication.
        /// </summary>
        private void JPH_UpdateFallbackChatNotificationSystemCommunication_20250721_Up()
        {
            RockMigrationHelper.UpdateSystemCommunication(
                category: "System",
                title: "Fallback Chat Notification",
                from: "",
                fromName: "",
                to: "",
                cc: "",
                bcc: "",
                subject: "Secured Chat Message from {{ SenderPerson.FullName }} at {{ 'Global' | Attribute:'OrganizationName'}}",
                body: @"{% capture chatUrl %}
  {{ 'Global' | Attribute:'PublicApplicationRoot' }}chat?SelectedChannelId={{ Channel.IdKey }}
{% endcapture %}

{{ 'Global' | Attribute:'EmailHeader' }}

<h1>New Chat Message</h1>

<p>Hi {{ Person.NickName }},</p>

<p>You have a secured chat message from {{ SenderPerson.FullName }}:</p>

<p><i>{{ Message }}</i></p>

<p>Join the conversation: <a href=""{{ chatUrl }}"">{{ chatUrl }}</a></p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                guid: "007C9AA2-057C-412F-A1F1-04805300D523",
                smsMessage: @"You have a secured chat message from {{ SenderPerson.FullName }} at {{ 'Global' | Attribute:'OrganizationName'}}.

Join the conversation: {{ 'Global' | Attribute:'PublicApplicationRoot' }}chat?SelectedChannelId={{ Channel.IdKey }}"
            );
        }

        #endregion

        #region DH/JE: Update Icon Transition Table

        private void UpdateIconTransitionTable()
        {
            Sql( @"
INSERT INTO [__IconTransition] ([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-area-chart','fa fa-area-chart','ti-chart-area','ti ti-chart-area'),
('fa-arrow-circle-o-down','fa fa-arrow-circle-o-down','ti-circle-arrow-down','ti ti-circle-arrow-down'),
('fa-arrow-circle-o-left','fa fa-arrow-circle-o-left','ti-circle-arrow-left','ti ti-circle-arrow-left'),
('fa-arrow-circle-o-right','fa fa-arrow-circle-o-right','ti-circle-arrow-right','ti ti-circle-arrow-right'),
('fa-arrow-circle-o-up','fa fa-arrow-circle-o-up','ti-circle-arrow-up','ti ti-circle-arrow-up'),
('fa-bank','fa fa-bank','ti-building-bank','ti ti-building-bank'),
('fa-bar-chart','fa fa-bar-chart','ti-chart-bar','ti ti-chart-bar'),
('fa-bell-o','fa fa-bell-o','ti-bell','ti ti-bell'),
('fa-bell-slash-o','fa fa-bell-slash-o','ti-bell-off','ti ti-bell-off'),
('fa-building-o','fa fa-building-o','ti-building','ti ti-building'),
('fa-calendar-check-o','fa fa-calendar-check-o','ti-calendar-check','ti ti-calendar-check'),
('fa-calendar-minus-o','fa fa-calendar-minus-o','ti-calendar-minus','ti ti-calendar-minus'),
('fa-calendar-o','fa fa-calendar-o','ti-calendar','ti ti-calendar'),
('fa-calendar-plus-o','fa fa-calendar-plus-o','ti-calendar-plus','ti ti-calendar-plus'),
('fa-calendar-times-o','fa fa-calendar-times-o','ti-calendar-x','ti ti-calendar-x'),
('fa-cc','fa fa-cc','ti-badge-cc','ti ti-badge-cc'),
('fa-check-circle-o','fa fa-check-circle-o','ti-circle-check','ti ti-circle-check'),
('fa-check-square-o','fa fa-check-square-o','ti-square-check','ti ti-square-check'),
('fa-circle-o','fa fa-circle-o','ti-circle','ti ti-circle'),
('fa-clock-o','fa fa-clock-o','ti-clock','ti ti-clock'),
('fa-cloud-download','fa fa-cloud-download','ti-cloud-down','ti ti-cloud-down'),
('fa-comment-o','fa fa-comment-o','ti-message','ti ti-message'),
('fa-comments-o','fa fa-comments-o','ti-messages','ti ti-messages'),
('fa-envelope-o','fa fa-envelope-o','ti-mail','ti ti-mail'),
('fa-exchange','fa fa-exchange','ti-switch-3','ti ti-switch-3'),
('fa-external-link','fa fa-external-link','ti-external-link','ti ti-external-link'),
('fa-file-code-o','fa fa-file-code-o','ti-file-code','ti ti-file-code'),
('fa-file-o','fa fa-file-o','ti-file','ti ti-file'),
('fa-file-pdf-o','fa fa-file-pdf-o','ti-file-description','ti ti-file-description'),
('fa-file-text','fa fa-file-text','ti-file-type-txt','ti ti-file-type-txt'),
('fa-file-text-o','fa fa-file-text-o','ti-file-type-txt','ti ti-file-type-txt'),
('fa-files','fa fa-files','ti-files','ti ti-files'),
('fa-files-o','fa fa-files-o','ti-files','ti ti-files'),
('fa-flag-o','fa fa-flag-o','ti-flag','ti ti-flag'),
('fa-flip-horizontal','fa fa-flip-horizontal','ti-flip-horizontal','ti ti-flip-horizontal'),
('fa-floppy-o','fa fa-floppy-o','ti-device-floppy','ti ti-device-floppy'),
('fa-gear','fa fa-gear','ti-settings','ti ti-settings'),
('fa-gears','fa fa-gears','ti-settings-cog','ti ti-settings-cog'),
('fa-group','fa fa-group','ti-users','ti ti-users'),
('fa-li','fa fa-li','ti-list','ti ti-list'),
('fa-line-chart','fa fa-line-chart','ti-chart-line','ti ti-chart-line'),
('fa-paper-plane-o','fa fa-paper-plane-o','ti-send','ti ti-send'),
('fa-pencil','fa fa-pencil','ti-pencil','ti ti-pencil'),
('fa-picture-o','fa fa-picture-o','ti-photo-scan','ti ti-photo-scan'),
('fa-pie-chart','fa fa-pie-chart','ti-chart-pie','ti ti-chart-pie'),
('fa-play-circle-o','fa fa-play-circle-o','ti-player-play','ti ti-player-play'),
('fa-plus-square-o','fa fa-plus-square-o','ti-square-plus','ti ti-square-plus'),
('fa-rating-off','fa fa-rating-off','ti-star','ti ti-star'),
('fa-rating-on','fa fa-rating-on','ti-star-filled','ti ti-star-filled'),
('fa-rating-selected','fa fa-rating-selected','ti-star-filled','ti ti-star-filled'),
('fa-rating-unselected','fa fa-rating-unselected','ti-star','ti ti-star'),
('fa-refresh','fa fa-refresh','ti-refresh','ti ti-refresh'),
('fa-repeat','fa fa-repeat','ti-repeat','ti ti-repeat'),
('fa-shield','fa fa-shield','ti-shield-half','ti ti-shield-half'),
('fa-sliders','fa fa-sliders','ti-adjustments','ti ti-adjustments'),
('fa-smile-o','fa fa-smile-o','ti-mood-smile','ti ti-mood-smile'),
('fa-square-o','fa fa-square-o','ti-square','ti ti-square'),
('fa-star-o','fa fa-star-o','ti-star','ti ti-star'),
('fa-tachometer','fa fa-tachometer','ti-brand-speedtest','ti ti-brand-speedtest'),
('fa-money','fa fa-money','ti-cash-banknote','ti ti-cash-banknote')

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-user-screen', [TablerFull] = 'ti ti-user-screen'
WHERE [FontAwesomeClass] = 'fa-child'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-settings', [TablerFull] = 'ti ti-settings'
WHERE [FontAwesomeFull] = 'fa fa-cog'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-settings-cog', [TablerFull] = 'ti ti-settings-cog'
WHERE [FontAwesomeFull] = 'fa fa-cogs'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-heart-handshake', [TablerFull] = 'ti ti-heart-handshake'
WHERE [FontAwesomeFull] = 'fa fa-handshake'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-map-pin', [TablerFull] = 'ti ti-map-pin'
WHERE [FontAwesomeFull] = 'fa fa-map-marker'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-package-export', [TablerFull] = 'ti ti-package-export'
WHERE [FontAwesomeFull] = 'fa fa-people-carry'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-phone', [TablerFull] = 'ti ti-phone'
WHERE [FontAwesomeFull] = 'fa fa-phone-square'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-player-play', [TablerFull] = 'ti ti-player-play'
WHERE [FontAwesomeFull] = 'fa fa-play-circle'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-pray', [TablerFull] = 'ti ti-pray'
WHERE [FontAwesomeFull] = 'fa fa-praying-hands'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-arrow-forward-up', [TablerFull] = 'ti ti-arrow-forward-up'
WHERE [FontAwesomeFull] = 'fa fa-redo'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-share', [TablerFull] = 'ti ti-share'
WHERE [FontAwesomeFull] = 'fa fa-share-alt-square'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-share', [TablerFull] = 'ti ti-share'
WHERE [FontAwesomeFull] = 'fa fa-share-square'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-share', [TablerFull] = 'ti ti-share'
WHERE [FontAwesomeFull] = 'fa fa-share'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-briefcase-2', [TablerFull] = 'ti ti-briefcase-2'
WHERE [FontAwesomeFull] = 'fa fa-suitcase'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-ti-refresh', [TablerFull] = 'ti ti-ti-refresh'
WHERE [FontAwesomeFull] = 'fa fa-sync'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-device-ipad', [TablerFull] = 'ti ti-device-ipad'
WHERE [FontAwesomeFull] = 'fa fa-tablet-alt'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-ruler-measure', [TablerFull] = 'ti ti-ruler-measure'
WHERE [FontAwesomeFull] = 'fa fa-text-width'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-grid-dots', [TablerFull] = 'ti ti-grid-dots'
WHERE [FontAwesomeFull] = 'fa fa-th'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-arrow-back-up', [TablerFull] = 'ti ti-arrow-back-up'
WHERE [FontAwesomeFull] = 'fa fa-undo'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-lock-open', [TablerFull] = 'ti ti-lock-open'
WHERE [FontAwesomeFull] = 'fa fa-unlock'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-share', [TablerFull] = 'ti ti-share'
WHERE [FontAwesomeFull] = 'fa fa-share'

UPDATE [__IconTransition]
SET [TablerClass] = 'ti-calendar-clock', [TablerFull] = 'ti ti-calendar-clock'
WHERE [FontAwesomeFull] = 'fa fa-user-clock'
" );
        }

        #endregion

        #region JPH: Update Peer Network Lava Templates to use FromIdHash Lava Filter

        /// <summary>
        /// JPH: Update Peer Network Lava templates 20250722 - Up.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250722_Up()
        {
            #region Update Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}
{% assign displayCount = 20 %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">

            {% for peer in results limit:displayCount %}
                <div class=""row"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ peer.TargetPersonId }}"">
                            {{ peer.TargetName }}
                        </a>
                    </div>
                    <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                    <div class=""col-xs-2"">
                        {% if peer.PointDifference > 0 %}
                            <i class=""fa fa-arrow-up text-success""></i>
                        {% elseif peer.PointDifference < 0 %}
                            <i class=""fa fa-arrow-down text-danger""></i>
                        {% else %}
                            <i class=""fa fa-minus text-muted""></i>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}

            {% assign resultCount = results | Size %}
            {% if resultCount > displayCount %}
                {% assign moreCount = resultCount | Minus:displayCount %}
                <div class=""row mt-2"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ personId }}/peer-graph""><small>(and {{ moreCount | Format:'#,##0' }} more)</small></a>
                    </div>
                </div>
            {% endif %}

    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Update Person Profile > Peer Network Block

            #region Update Peer Network > Peer Map Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'D2D0FF94-1816-4B43-A49D-104CC42A5DC3');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption];

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Update Peer Network > Peer Map Block

            #region Update Peer Network > Peer List Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '46775056-3ADF-43CD-809A-88EE3378C039');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Full Peer Network</span>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Update Peer Network > Peer List Block
        }

        /// <summary>
        /// JPH: Update Peer Network Lava templates 20250722 - Down.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250722_Down()
        {
            #region Revert Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}
{% assign displayCount = 20 %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">

            {% for peer in results limit:displayCount %}
                <div class=""row"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ peer.TargetPersonId }}"">
                            {{ peer.TargetName }}
                        </a>
                    </div>
                    <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                    <div class=""col-xs-2"">
                        {% if peer.PointDifference > 0 %}
                            <i class=""fa fa-arrow-up text-success""></i>
                        {% elseif peer.PointDifference < 0 %}
                            <i class=""fa fa-arrow-down text-danger""></i>
                        {% else %}
                            <i class=""fa fa-minus text-muted""></i>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}

            {% assign resultCount = results | Size %}
            {% if resultCount > displayCount %}
                {% assign moreCount = resultCount | Minus:displayCount %}
                <div class=""row mt-2"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ personId }}/peer-graph""><small>(and {{ moreCount | Format:'#,##0' }} more)</small></a>
                    </div>
                </div>
            {% endif %}

    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Revert Person Profile > Peer Network Block

            #region Revert Peer Network > Peer Map Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'D2D0FF94-1816-4B43-A49D-104CC42A5DC3');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption];

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Revert Peer Network > Peer Map Block

            #region Revert Peer Network > Peer List Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '46775056-3ADF-43CD-809A-88EE3378C039');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Full Peer Network</span>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Revert Peer Network > Peer List Block
        }

        #endregion

        #region KH: Update LMS Learning Course Detail Lava for Completions

        private void UpdateLMSLearningCourseDetailLavaForCompletionsUp()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles

<style>

    @media (max-width: 991px) {
        .course-side-panel {
            padding-left: 0;
        }
        .card {
            margin-bottom: 24px;
        }
    }
    
    @media (max-width: 767px) {
        h1 {
            font-size: 28px;
        }
        .card {
            margin-bottom: 24px;
        }
    }

</style>

<div class=""d-flex flex-column gap-4"">
    
    <div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ CourseInfo.ImageFileGuid}}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ CourseInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ CourseInfo.Summary }} </p>
        </div>
    </div>

    <div>

        <div class=""row"">

            <div class=""col-xs-12 col-sm-12 col-md-8""> //- LEFT CONTAINER

                <div class=""card rounded-lg""> //- COURSE DESCRIPTION

                    <div class=""card-body"">
                        <div class=""card-title"">
                            <h4 class=""m-0"">Course Description</h4>
                        </div>
                        <div class=""card-text"">
                            {% if CourseInfo.CourseCode != empty %}
                            <div class=""text-gray-600 d-flex gap-1"">
                                <p class=""text-bold mb-0"">Course Code: </p>
                                <p class=""mb-0"">{{CourseInfo.CourseCode}}</p>
                            </div>
                            {% endif %}

                            {% if CourseInfo.Credits != empty and CourseInfo.Credits != 0 %}
                            <div class=""d-flex text-gray-600 gap-1 pb-3"">
                                <p class=""text-bold mb-0"">Credits: </p>
                                <p class=""mb-0"">{{CourseInfo.Credits}}</p>
                            </div>
                            {% endif %}

                            <div class=""pt-3 border-top border-gray-200"">
                                {% if CourseInfo.DescriptionAsHtml == empty %}
                                    <span>No course description provided.</span>
                                {% else %}
                                    <span>{{CourseInfo.DescriptionAsHtml}}</span>
                                {% endif %}
                            </div>
                        </div>
                    </div>

                </div>

            </div>

            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER
                <div class=""card rounded-lg mb-4"">
                    <div class=""card-body"">
                        <div class=""card-title d-flex align-items-center"">
							<h4 class=""m-0""><span><i class=""fa fa-clipboard-list mr-2""></i></span>Requirements
							    {% if CourseInfo.UnmetPrerequisites != empty %}
                                    <i class=""fa fa-exclamation-circle text-danger""></i>
                                    </h4>
                                {% else %}
                                    <i class=""fa fa-check-circle text-success""></i>
                                    </h4>
                                {% endif %}
							</h4>
						</div>
						<div class=""card-text text-muted"">
							{% if CourseInfo.CourseRequirements != empty %}
                                {% assign requirementsText = CourseInfo.CourseRequirements |
                                Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                {% if CourseInfo.UnmetPrerequisites != empty %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                    <p class=""text-danger mb-0 mt-3"">You do not meet the course requirements.</p>
                                {% else %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                {% endif %}
                            {% else %}
                            <p class=""mb-0"">None</p>
                            {% endif %}
						</div>
                    </div>
                </div>
                
                {% assign today = 'Now' | Date:'yyyy-MM-dd' | AsDateTime %}
                {% assign hasClasses = false %}
                {% for semesterInfo in CourseInfo.Semesters %}
                    
                    {% for classInfo in semesterInfo.AvailableClasses %}
                        {% if semesterInfo.EnrollmentCloseDate == null or semesterInfo.EnrollmentCloseDate >= today %}
                            {% assign hasClasses = true %}
						{% endif %}
						//- CURRENTLY ENROLLED
						{% assign isActiveSemester = semesterInfo.StartDate == null 
						    or today >= semesterInfo.StartDate %}
						{% if classInfo.IsEnrolled 
						and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" 
						and isActiveSemester %} 
							
							<div class=""card rounded-lg mb-4"">
								<div class=""card-body"">
									<div class=""card-title d-flex align-items-center"">
										<i class=""fa fa-user-check mr-2""></i>
										<h4 class=""m-0"">Currently Enrolled</h4>
									</div>
									<div class=""card-text text-muted mb-3"">
										<p>You are currently enrolled in this course.</p>
										<p class=""text-gray-800""><i class=""fa fa-arrow-right mr-2""></i>{{classInfo.Name}}</p>
									</div>
									<div>
										<a class=""btn btn-info"" href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
									</div>
								</div>
							</div>
                        {% elseif classInfo.StudentParticipant and classInfo.StudentParticipant.LearningCompletionStatus != ""Incomplete"" %}
							
                            //- HISTORICAL ACCESS
                            {% if CourseInfo.AllowHistoricalAccess == true %}
                            
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}.</div>

                                        {% if CourseInfo.IsCompletionOnly == false %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}

                                        <div class=""mt-2"">
                                            <a href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
                                        </div>
                                    </div>
                                    
                                </div>
							 
							//- NO HISTORICAL ACCESS
							{% else %}
                                
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}</div>

                                        {% if CourseInfo.IsCompletionOnly == false %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}
                                    </div>
                                    
                                </div>
                                
                            {% endif %}
                        {% endif %}
                    {% endfor %}
                {% endfor %}
                                
                {% if hasClasses == false %} //- NO UPCOMING SEMESTERS OR CLASSES
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""mt-0"">No Available Upcoming Classes</h4>
                            <p class=""text-gray-600"">Please check back again later.</p>
                        </div>
                    </div>
                {% else %}
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""card-title mt-0""><i class=""fa fa-chalkboard-teacher mr-2""></i>Classes</h4>
                            
                            //- SCOPING TO CLASS DETAILS
                            
                            {% for semesterInfo in CourseInfo.Semesters %}

                                {% assign semesterStartDate = semesterInfo.StartDate | Date: 'sd' %}
                                {% assign semesterEndDate = semesterInfo.EndDate | Date: 'sd' %}
                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div class=""py-1"">
                                        <h5 class=""mt-0 mb-0"">{{semesterInfo.Name}}</h5>
                                        {% if semesterStartDate and semesterEndDate %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}-{{semesterEndDate}}</p>
                                        {% elseif semesterEndDate != empty %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}</p>
                                        {% else %}
                                            <p class=""text-gray-600"">Date Pending</p>
                                        {% endif %}
                                    </div>
                                {% endif %}
                                        
                                {% for classInfo in semesterInfo.AvailableClasses %}
                                    {% assign facilitatorsText = classInfo.Facilitators | Select:'Name' | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
                                   
                                    <div class=""card rounded-lg bg-gray-100 mb-4"">
                                        <div class=""card-body pb-0"">
                                            <div class=""d-grid grid-flow-row gap-0 mb-3""> //- BEGIN CLASS DETAILS
                                                
                                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                    <p class=""text-bold"">{{classInfo.Name}}
                                                    {% if classInfo.IsEnrolled and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" %}
                                                        <span class=""text-normal align-top badge bg-info"">Enrolled</span>
                                                    {% elseif classInfo.IsEnrolled %}
														<span class=""text-normal align-top badge bg-success"">Recently Completed</span>
                                                    {% endif %}
                                                    </p>
                                                {% else %}
                                                <h4 class=""mt-0"">Class Details</h4>
                                                {% endif %}
                                                
                                                <div class=""d-flex flex-column"">
                                                    {% if facilitatorsText %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Facilitators: 
                                                            <p>{{facilitatorsText}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.Campus %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Campus: 
                                                            <p>{{classInfo.Campus}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Location and classInfo.Location != '' %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Location: 
                                                            <p>{{classInfo.Location}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Schedule %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Schedule: 
                                                            <p>{{classInfo.Schedule}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.IsEnrolled  == false %} //- NEVER ENROLLED?
                                                        {% if semesterInfo.IsEnrolled == true and classInfo.IsEnrolled == false %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                                <p class=""text-danger"">You've already enrolled in this course this semester.</p>
                                                            {% else %}
                                                                <p class=""text-danger"">You've already enrolled in a version of this course.</p>
                                                            {% endif %}
                                                        {% elseif classInfo.CanEnroll == false %}
                                                            {% if classInfo.EnrollmentErrorKey == 'class_full' %}
                                                                <p class=""text-danger"">Class is full.</p>
                                                            {% endif %}
                                                        
                                                        {% else %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode != ""AcademicCalendar"" or semesterInfo.IsEnrolled  == false %}
                                                                <div>
                                                                    <a class=""btn btn-default"" href=""{{ classInfo.EnrollmentLink }}"">Enroll</a>
                                                                </div>
                                                            {% endif %}
                                                        {% endif %}
                                                    {% endif %}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                {% endfor %}
       
                            {% endfor %}
                        
                        </div>
                    </div>        
                    
                {% endif %}

            </div>
        </div>
    </div>
</div>
", "DA6C3170-5264-427D-AC22-8D50D2F6D2F6" );
        }

        private void UpdateLMSLearningCourseDetailLavaForCompletionsDown()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "CourseDetailTemplate", "Lava Template", @"The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles

<style>

    @media (max-width: 991px) {
        .course-side-panel {
            padding-left: 0;
        }
        .card {
            margin-bottom: 24px;
        }
    }
    
    @media (max-width: 767px) {
        h1 {
            font-size: 28px;
        }
        .card {
            margin-bottom: 24px;
        }
    }

</style>

<div class=""d-flex flex-column gap-4"">
    
    <div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ CourseInfo.ImageFileGuid}}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ CourseInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ CourseInfo.Summary }} </p>
        </div>
    </div>

    <div>

        <div class=""row"">

            <div class=""col-xs-12 col-sm-12 col-md-8""> //- LEFT CONTAINER

                <div class=""card rounded-lg""> //- COURSE DESCRIPTION

                    <div class=""card-body"">
                        <div class=""card-title"">
                            <h4 class=""m-0"">Course Description</h4>
                        </div>
                        <div class=""card-text"">
                            {% if CourseInfo.CourseCode != empty %}
                            <div class=""text-gray-600 d-flex gap-1"">
                                <p class=""text-bold mb-0"">Course Code: </p>
                                <p class=""mb-0"">{{CourseInfo.CourseCode}}</p>
                            </div>
                            {% endif %}

                            {% if CourseInfo.Credits != empty and CourseInfo.Credits != 0 %}
                            <div class=""d-flex text-gray-600 gap-1 pb-3"">
                                <p class=""text-bold mb-0"">Credits: </p>
                                <p class=""mb-0"">{{CourseInfo.Credits}}</p>
                            </div>
                            {% endif %}

                            <div class=""pt-3 border-top border-gray-200"">
                                {% if CourseInfo.DescriptionAsHtml == empty %}
                                    <span>No course description provided.</span>
                                {% else %}
                                    <span>{{CourseInfo.DescriptionAsHtml}}</span>
                                {% endif %}
                            </div>
                        </div>
                    </div>

                </div>

            </div>

            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER
                <div class=""card rounded-lg mb-4"">
                    <div class=""card-body"">
                        <div class=""card-title d-flex align-items-center"">
							<h4 class=""m-0""><span><i class=""fa fa-clipboard-list mr-2""></i></span>Requirements
							    {% if CourseInfo.UnmetPrerequisites != empty %}
                                    <i class=""fa fa-exclamation-circle text-danger""></i>
                                    </h4>
                                {% else %}
                                    <i class=""fa fa-check-circle text-success""></i>
                                    </h4>
                                {% endif %}
							</h4>
						</div>
						<div class=""card-text text-muted"">
							{% if CourseInfo.CourseRequirements != empty %}
                                {% assign requirementsText = CourseInfo.CourseRequirements |
                                Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                {% if CourseInfo.UnmetPrerequisites != empty %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                    <p class=""text-danger mb-0 mt-3"">You do not meet the course requirements.</p>
                                {% else %}
                                    </p>
                                    <p class=""mb-0"">{{requirementsText}}</p>
                                {% endif %}
                            {% else %}
                            <p class=""mb-0"">None</p>
                            {% endif %}
						</div>
                    </div>
                </div>
                
                {% assign today = 'Now' | Date:'yyyy-MM-dd' | AsDateTime %}
                {% assign hasClasses = false %}
                {% for semesterInfo in CourseInfo.Semesters %}
                    
                    {% for classInfo in semesterInfo.AvailableClasses %}
                        {% if semesterInfo.EnrollmentCloseDate == null or semesterInfo.EnrollmentCloseDate >= today %}
                            {% assign hasClasses = true %}
						{% endif %}
						//- CURRENTLY ENROLLED
						{% assign isActiveSemester = semesterInfo.StartDate == null 
						    or today >= semesterInfo.StartDate %}
						{% if classInfo.IsEnrolled 
						and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" 
						and isActiveSemester %} 
							
							<div class=""card rounded-lg mb-4"">
								<div class=""card-body"">
									<div class=""card-title d-flex align-items-center"">
										<i class=""fa fa-user-check mr-2""></i>
										<h4 class=""m-0"">Currently Enrolled</h4>
									</div>
									<div class=""card-text text-muted mb-3"">
										<p>You are currently enrolled in this course.</p>
										<p class=""text-gray-800""><i class=""fa fa-arrow-right mr-2""></i>{{classInfo.Name}}</p>
									</div>
									<div>
										<a class=""btn btn-info"" href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
									</div>
								</div>
							</div>
                        {% elseif classInfo.StudentParticipant and classInfo.StudentParticipant.LearningCompletionStatus != ""Incomplete"" %}
							
                            //- HISTORICAL ACCESS
                            {% if CourseInfo.AllowHistoricalAccess == true %}
                            
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}.</div>

                                        {% if CourseInfo.IsCompletionOnly == false %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}

                                        <div class=""mt-2"">
                                            <a href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
                                        </div>
                                    </div>
                                    
                                </div>
							 
							//- NO HISTORICAL ACCESS
							{% else %}
                                
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}</div>

                                        {% if classInfo.StudentParticipant.LearningGradingSystemScale.Name != empty %}
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                        {% endif %}
                                    </div>
                                    
                                </div>
                                
                            {% endif %}
                        {% endif %}
                    {% endfor %}
                {% endfor %}
                                
                {% if hasClasses == false %} //- NO UPCOMING SEMESTERS OR CLASSES
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""mt-0"">No Available Upcoming Classes</h4>
                            <p class=""text-gray-600"">Please check back again later.</p>
                        </div>
                    </div>
                {% else %}
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""card-title mt-0""><i class=""fa fa-chalkboard-teacher mr-2""></i>Classes</h4>
                            
                            //- SCOPING TO CLASS DETAILS
                            
                            {% for semesterInfo in CourseInfo.Semesters %}

                                {% assign semesterStartDate = semesterInfo.StartDate | Date: 'sd' %}
                                {% assign semesterEndDate = semesterInfo.EndDate | Date: 'sd' %}
                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div class=""py-1"">
                                        <h5 class=""mt-0 mb-0"">{{semesterInfo.Name}}</h5>
                                        {% if semesterStartDate and semesterEndDate %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}-{{semesterEndDate}}</p>
                                        {% elseif semesterEndDate != empty %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}</p>
                                        {% else %}
                                            <p class=""text-gray-600"">Date Pending</p>
                                        {% endif %}
                                    </div>
                                {% endif %}
                                        
                                {% for classInfo in semesterInfo.AvailableClasses %}
                                    {% assign facilitatorsText = classInfo.Facilitators | Select:'Name' | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
                                   
                                    <div class=""card rounded-lg bg-gray-100 mb-4"">
                                        <div class=""card-body pb-0"">
                                            <div class=""d-grid grid-flow-row gap-0 mb-3""> //- BEGIN CLASS DETAILS
                                                
                                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                    <p class=""text-bold"">{{classInfo.Name}}
                                                    {% if classInfo.IsEnrolled and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" %}
                                                        <span class=""text-normal align-top badge bg-info"">Enrolled</span>
                                                    {% elseif classInfo.IsEnrolled %}
														<span class=""text-normal align-top badge bg-success"">Recently Completed</span>
                                                    {% endif %}
                                                    </p>
                                                {% else %}
                                                <h4 class=""mt-0"">Class Details</h4>
                                                {% endif %}
                                                
                                                <div class=""d-flex flex-column"">
                                                    {% if facilitatorsText %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Facilitators: 
                                                            <p>{{facilitatorsText}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.Campus %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Campus: 
                                                            <p>{{classInfo.Campus}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Location and classInfo.Location != '' %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Location: 
                                                            <p>{{classInfo.Location}}</p>
                                                        </div>
                                                    {% endif %}
                                                    {% if classInfo.Schedule %}
                                                        <div class=""text-gray-600"">
                                                            <p class=""text-bold mb-0"">Schedule: 
                                                            <p>{{classInfo.Schedule}}</p>
                                                        </div>
                                                    {% endif %}
                                                    
                                                    {% if classInfo.IsEnrolled  == false %} //- NEVER ENROLLED?
                                                        {% if semesterInfo.IsEnrolled == true and classInfo.IsEnrolled == false %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                                <p class=""text-danger"">You've already enrolled in this course this semester.</p>
                                                            {% else %}
                                                                <p class=""text-danger"">You've already enrolled in a version of this course.</p>
                                                            {% endif %}
                                                        {% elseif classInfo.CanEnroll == false %}
                                                            {% if classInfo.EnrollmentErrorKey == 'class_full' %}
                                                                <p class=""text-danger"">Class is full.</p>
                                                            {% endif %}
                                                        
                                                        {% else %}
                                                            {% if CourseInfo.ProgramInfo.ConfigurationMode != ""AcademicCalendar"" or semesterInfo.IsEnrolled  == false %}
                                                                <div>
                                                                    <a class=""btn btn-default"" href=""{{ classInfo.EnrollmentLink }}"">Enroll</a>
                                                                </div>
                                                            {% endif %}
                                                        {% endif %}
                                                    {% endif %}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                {% endfor %}
       
                            {% endfor %}
                        
                        </div>
                    </div>        
                    
                {% endif %}

            </div>
        </div>
    </div>
</div>
", "DA6C3170-5264-427D-AC22-8D50D2F6D2F6" );
        }

        #endregion

        #region JE: Rock Theme Change

        private void UpdateRockTheme()
        {
            Sql( @"
UPDATE [Site]
SET [Theme] = 'RockNextGen'
WHERE [Guid] = 'c2d29296-6a87-47a9-a753-ee4e9159c4c4'
" );
        }

        #endregion

        #region KH: Register block attributes for chop job in v18 (18.0.10)

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.BlockTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.BlockTypeDetail", "Block Type Detail", "Rock.Blocks.Cms.BlockTypeDetail, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "81B9BFD5-621D-4E82-84F6-38CAE1810332" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.BlockTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.BlockTypeList", "Block Type List", "Rock.Blocks.Cms.BlockTypeList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8FCEE05F-6757-4B16-8718-63CD80FF07D6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.CampusList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.CampusList", "Campus List", "Rock.Blocks.Core.CampusList, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "A21A13C9-9429-4BA1-85B2-D2FA4E3D5081" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.NamelessPersonList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.NamelessPersonList", "Nameless Person List", "Rock.Blocks.Crm.NamelessPersonList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "911EA779-AC00-4A93-B706-B6A642C727CB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BenevolenceRequestList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BenevolenceRequestList", "Benevolence Request List", "Rock.Blocks.Finance.BenevolenceRequestList, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "D1245F63-A9BA-4289-BD82-44A489F9DA9A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPersonBankAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPersonBankAccountList", "Financial Person Bank Account List", "Rock.Blocks.Finance.FinancialPersonBankAccountList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "30150FA5-A4E9-4767-A320-C9092B8FFD61" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.SavedAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.SavedAccountList", "Saved Account List", "Rock.Blocks.Finance.SavedAccountList, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "AD9C4AAC-54BB-498D-9BD3-47D8F21B9549" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduleToolbox
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox", "Group Schedule Toolbox", "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "FDADA51C-C7E6-4ECA-A984-646B42FBFC40" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.WebFarm.WebFarmNodeDetail", "Web Farm Node Detail", "Rock.Blocks.WebFarm.WebFarmNodeDetail, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "8471BF7F-6D0D-411B-899F-CD853F496BB9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeLogList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.WebFarm.WebFarmNodeLogList", "Web Farm Node Log List", "Rock.Blocks.WebFarm.WebFarmNodeLogList, Rock.Blocks, Version=1.16.7.2, Culture=neutral, PublicKeyToken=null", false, false, "57E8356D-6E59-4F5B-8DB9-A274B7A0EFD8" );

            // Add/Update Obsidian Block Type
            //   Name:Bank Account List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPersonBankAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Bank Account List", "Lists bank accounts for a person.", "Rock.Blocks.Finance.FinancialPersonBankAccountList", "Finance", "E1DCE349-2F5B-46ED-9F3D-8812AF857F69" );

            // Add/Update Obsidian Block Type
            //   Name:Benevolence Request List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BenevolenceRequestList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Benevolence Request List", "Block used to list Benevolence Requests.", "Rock.Blocks.Finance.BenevolenceRequestList", "Finance", "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1" );

            // Add/Update Obsidian Block Type
            //   Name:Block Type Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.BlockTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Block Type Detail", "Displays the details of a particular block type.", "Rock.Blocks.Cms.BlockTypeDetail", "CMS", "6C329001-9C04-4090-BED0-12E3F6B88FB6" );

            // Add/Update Obsidian Block Type
            //   Name:Block Type List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.BlockTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Block Type List", "Displays a list of block types.", "Rock.Blocks.Cms.BlockTypeList", "CMS", "1C3D7F3D-E8C7-4F27-871C-7EC20483B416" );

            // Add/Update Obsidian Block Type
            //   Name:Campus List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.CampusList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Campus List", "Displays a list of campuses.", "Rock.Blocks.Core.CampusList", "Core", "52DF00E5-BC19-43F2-8533-A386DB53C74F" );

            // Add/Update Obsidian Block Type
            //   Name:Group Schedule Toolbox
            //   Category:Group Scheduling
            //   EntityType:Rock.Blocks.Group.Scheduling.GroupScheduleToolbox
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Schedule Toolbox", "Allows management of group scheduling for a specific person (worker).", "Rock.Blocks.Group.Scheduling.GroupScheduleToolbox", "Group Scheduling", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" );

            // Add/Update Obsidian Block Type
            //   Name:Nameless Person List
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.NamelessPersonList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Nameless Person List", "List unmatched phone numbers with an option to link to a person that has the same phone number.", "Rock.Blocks.Crm.NamelessPersonList", "CRM", "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C" );

            // Add/Update Obsidian Block Type
            //   Name:Saved Account List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.SavedAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Saved Account List", "List of a person's saved accounts that can be used to delete an account.", "Rock.Blocks.Finance.SavedAccountList", "Finance", "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702" );

            // Add/Update Obsidian Block Type
            //   Name:Web Farm Node Detail
            //   Category:WebFarm
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Web Farm Node Detail", "Displays the details of a particular web farm node.", "Rock.Blocks.WebFarm.WebFarmNodeDetail", "WebFarm", "6BBA1FC0-AC56-4E58-9E99-EB20DA7AA415" );

            // Add/Update Obsidian Block Type
            //   Name:Web Farm Node Log List
            //   Category:WebFarm
            //   EntityType:Rock.Blocks.WebFarm.WebFarmNodeLogList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Web Farm Node Log List", "Displays a list of web farm node logs.", "Rock.Blocks.WebFarm.WebFarmNodeLogList", "WebFarm", "6C824483-6624-460B-9DD8-E127B25CA65D" );

            // Attribute for BlockType
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1DCE349-2F5B-46ED-9F3D-8812AF857F69", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "449AB58C-668B-439F-A38C-1872E0962F1B" );

            // Attribute for BlockType
            //   BlockType: Bank Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1DCE349-2F5B-46ED-9F3D-8812AF857F69", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F7272148-6D24-44C8-8748-2DBB4B8A49B6" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Case Worker Role
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Case Worker Role", "CaseWorkerRole", "Case Worker Role", @"The security role to draw case workers from", 0, @"02FA0881-3552-42B8-A519-D021139B800F", "FF6578B9-EB29-4A8D-AB86-30C250D70D2A" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create benevolence type.", 2, @"C6BE9CF1-FFE9-4DC1-8472-865FD93B89A8", "A0743A61-F13A-400D-A1C7-933BC966DAC2" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F29430D2-547B-4854-B030-E9701C04A1FC" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B2D4CCF1-D81C-448F-A8D5-5F8CF70EDB8A" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "BenevolenceRequestDetail", "Detail Page", @"The page that will show the benevolence request details.", 0, @"", "676009F4-E109-4B46-AF26-907D903254A5" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsAttributeKey", "Hide Columns on Grid", @"The grid columns that should be hidden.", 3, @"", "712381CF-08E8-4841-94F6-4E66815C3F10" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request List
            //   Category: Finance
            //   Attribute: Include Benevolence Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8ADB5C0D-9A4F-4396-AB0F-DEB552C094E1", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Include Benevolence Types", "FilterBenevolenceTypesAttributeKey", "Include Benevolence Types", @"The benevolence types to display in the list.<br/><i>If none are selected, all types will be included.<i>", 4, @"", "DEA0086A-8B29-4A0E-9B67-72A40ACA0CD4" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "38E12A82-1D83-4777-A80D-3E6E3B778973" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6987D90D-CB37-4873-8BBC-5C1186B53353" );

            // Attribute for BlockType
            //   BlockType: Block Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1C3D7F3D-E8C7-4F27-871C-7EC20483B416", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the block type details.", 0, @"", "1E67AA59-6189-425F-8C07-D930CB5A479B" );

            // Attribute for BlockType
            //   BlockType: Campus List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4A927AA1-F071-4FB9-8D1F-17310BB4FCDD" );

            // Attribute for BlockType
            //   BlockType: Campus List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7AC8E1D6-8728-4866-978A-E13D52946851" );

            // Attribute for BlockType
            //   BlockType: Campus List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52DF00E5-BC19-43F2-8533-A386DB53C74F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the campus details.", 0, @"", "BC6A44F6-218C-454C-9988-66C9E44C5124" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Action Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Action Header Lava Template", "ActionHeaderLavaTemplate", "Action Header Lava Template", @"Header content to show above the action buttons. <span class='tip tip-lava'></span>", 0, @"<h4>Actions</h4>", "E49211AE-9520-4108-9476-68E91D27FBB8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Time Sign-Up Button Text", "AdditionalTimeSignUpButtonText", "Additional Time Sign-Up Button Text", @"The text to display for the Additional Time Sign-Up button.", 1, @"Sign Up for Additional Times", "EDDD5B92-E5A2-4CCA-AE81-DF3229EF609C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Time Sign-Up Header", "SignupforAdditionalTimesHeader", "Additional Time Sign-Up Header", @"Header content to show above the Additional Time Sign-Up panel. <span class='tip tip-lava'></span>", 2, @"", "5C6CAAAA-5CA9-4DEB-80E5-92D09E07B5E8" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Schedule Exclusions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Additional Time Sign-Up Schedule Exclusions", "AdditionalTimeSignUpScheduleExclusions", "Additional Time Sign-Up Schedule Exclusions", @"Select named schedules that you would like to exclude from all groups on the Additional Time Sign-Up panel.", 6, @"", "A6BB5229-63F9-4940-B9A8-E8C1A8DBF54F" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Confirmed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmed Button Text", "ConfirmedButtonText", "Confirmed Button Text", @"The text to display for the Confirmed Button Text.", 4, @"Confirmed", "AE39BCCF-5F0C-4E2B-A0E0-B6875B9AC34D" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Current Schedule Button Text", "CurrentScheduleButtonText", "Current Schedule Button Text", @"The text to display for the Current Schedule button.", 0, @"Current Schedule", "6374FED9-0021-4C92-86C1-1AF8963723E3" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Current Schedule Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Current Schedule Header", "CurrentScheduleHeader", "Current Schedule Header", @"Header content to show above the Current Schedule panel. <span class='tip tip-lava'></span>", 1, @"", "607953EC-9A9B-4FC0-8119-7851BCF35C27" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Cutoff Time (Hours)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cutoff Time (Hours)", "AdditionalTimeSignUpCutoffTime", "Cutoff Time (Hours)", @"Set the cutoff time in hours for hiding schedules that are too close to their start time. Schedules within this cutoff window will not be displayed for sign-up. Schedules that have already started will never be displayed.", 4, @"12", "F4A7DECE-F034-4F2B-91C4-C1AA03F7E382" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "FutureWeekDateRange", "Date Range", @"The date range to allow individuals to sign up for a schedule. Please note that only current and future dates will be accepted. Schedules that have already started will never be displayed.", 3, @"Next|6|Week||", "AE77BD8D-147D-46C7-8370-9DD147CD57F4" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Decline Button Text", "DeclineButtonText", "Decline Button Text", @"The text to display for the Decline Button Text.", 5, @"Cancel Confirmation", "410A9CBF-AEDC-42F4-AA60-4C092486437E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Decline Reason Note
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Decline Reason Note", "DeclineReasonNote", "Decline Reason Note", @"Controls whether a note will be shown for the person to elaborate on why they cannot attend. A schedule's Group Type must also require a decline reason for this setting to have any effect.", 4, @"hide", "EFEA542E-6103-44BE-AFB8-17F1BA3DD3AE" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Additional Time Sign-Up
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Additional Time Sign-Up", "EnableAdditionalTimeSignUp", "Enable Additional Time Sign-Up", @"When enabled, a button will allow the individual to sign up for upcoming schedules for their group.", 0, @"True", "2F24C3AC-461A-4AF2-A733-A609E0AE9C73" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Immediate Needs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Immediate Needs", "EnableImmediateNeeds", "Enable Immediate Needs", @"When enabled, upcoming opportunities that still need individuals will be highlighted.", 7, @"False", "A24D633D-A93F-4811-B0ED-601F257E8C4D" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Schedule Unavailability
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Schedule Unavailability", "EnableScheduleUnavailability", "Enable Schedule Unavailability", @"When enabled, a button will allow the individual to specify dates or date ranges when they will be unavailable to serve.", 0, @"True", "35B5BF8B-307E-4D2B-A4AF-DBE51DBD3A26" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Enable Update Schedule Preferences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Update Schedule Preferences", "EnableUpdateSchedulePreferences", "Enable Update Schedule Preferences", @"When enabled, a button will allow the individual to set their group reminder preferences and preferred schedule.", 0, @"True", "B9D046B3-71E2-4683-A209-8AC6E587F592" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Exclude Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Exclude Group Types", "ExcludeGroupTypes", "Exclude Group Types", @"The group types to exclude from the list (only valid if including all groups).", 3, @"", "C159F464-03EE-47C7-9796-DF7C7B8DCD56" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Introduction
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Immediate Need Introduction", "ImmediateNeedIntroduction", "Immediate Need Introduction", @"The introductory text to show above the Immediate Need panel.", 9, @"This group has an immediate need for volunteers. If you're able to assist we would greatly appreciate your help.", "4F00EFD1-B49F-4D1E-B1A5-90EA7E548B43" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Immediate Need Title", "ImmediateNeedTitle", "Immediate Need Title", @"The title to use for the Immediate Need panel.", 8, @"Immediate Needs", "9419F50C-B027-49B7-A959-966B4B176065" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Immediate Need Window (Hours)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Immediate Need Window (Hours)", "ImmediateNeedWindow", "Immediate Need Window (Hours)", @"The hour range to determine which schedules are in the immediate window. This works with the cutoff setting so ensure that you reduce the cutoff setting to include schedules you will want shown in the Immediate Need panel.", 10, @"0", "5971D51D-DD6D-46D3-8B7D-1C5745AA66E1" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Include Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Include Group Types", "IncludeGroupTypes", "Include Group Types", @"The group types to display in the list. If none are selected, all group types will be included.", 2, @"", "0666A1D5-0ED4-4899-88E2-9F7A7B029C09" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Override Hide from Toolbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override Hide from Toolbox", "OverrideHideFromToolbox", "Override Hide from Toolbox", @"When enabled this setting will show all schedule enabled groups no matter what their ""Disable Schedule Toolbox Access"" setting is set to.", 1, @"False", "880373F4-BD70-44CC-9821-B8FC9E69FD71" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Require Location for Additional Time Sign-Up
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Location for Additional Time Sign-Up", "RequireLocationForAdditionalSignups", "Require Location for Additional Time Sign-Up", @"When enabled, a location will be required when signing up for additional times.", 5, @"False", "7E67EAA5-641A-4D55-9436-7841DB84CB5C" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule List Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Schedule List Format", "ScheduleListFormat", "Schedule List Format", @"The format to be used when displaying schedules for schedule preferences and additional time sign-ups.", 5, @"1", "AE327AC7-65A4-470A-94CC-9DB2DAAEB89A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Unavailability Button Text", "ScheduleUnavailabilityButtonText", "Schedule Unavailability Button Text", @"The text to display for the Schedule Unavailability button.", 1, @"Schedule Unavailability", "F985115C-E0C4-422A-89F9-1F850963D199" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Schedule Unavailability Header", "ScheduleUnavailabilityHeader", "Schedule Unavailability Header", @"Header content to show above the Schedule Unavailability panel. <span class='tip tip-lava'></span>", 2, @"", "794DC298-719A-4003-8FCE-FCAE0EE2C4B5" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"When enabled, the scheduler will receive an email for each confirmation or decline. Note that if a Group's ""Schedule Coordinator"" is defined, that person will automatically receive emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule), regardless of this setting.", 2, @"False", "BF2C31AA-A97B-43C4-886D-67654B3A979A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system communication used to send emails to the scheduler for each confirmation or decline. If a Group's ""Schedule Coordinator"" is defined, this will also be used when sending emails based on the Group/GroupType's configured notification options (e.g., accept, decline, self-schedule).", 3, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "DA96A6CD-D853-48DA-9863-30EF68B75401" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Show Campus on Tabs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Campus on Tabs", "ShowCampusOnTabs", "Show Campus on Tabs", @"Optionally shows the group's campus on the tabs.", 4, @"never", "56A31EA7-2F98-4637-8A16-178706F7EB1E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Update Schedule Preferences Button Text", "UpdateSchedulePreferencesButtonText", "Update Schedule Preferences Button Text", @"The text to display for the Update Schedule Preferences button.", 1, @"Update Schedule Preferences", "BF2736D3-A158-4071-9313-C61F4CD8D645" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6554ADE3-2FC8-482B-BA63-2C3EABC11D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Update Schedule Preferences Header", "UpdateSchedulePreferencesHeader", "Update Schedule Preferences Header", @"Header content to show above the Update Schedule Preferences panel. <span class='tip tip-lava'></span>", 2, @"", "B8727222-F857-4AED-874D-43C6932206BD" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E9DDF9B9-839B-4F2D-AA5A-51F507AB0734" );

            // Attribute for BlockType
            //   BlockType: Nameless Person List
            //   Category: CRM
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6E9672E6-EE42-4AAC-B0A9-B041C3B8368C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0CC87BD8-FD68-4B86-B65B-77FC3F7105E7" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "94FFEDC7-188F-4E09-A41D-76A974D8BA37" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A5AF7CAE-B742-4F28-BF5D-B84139CC4E5D" );

            // Attribute for BlockType
            //   BlockType: Saved Account List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E20B2FE2-2708-4E9A-B9FB-B370E8B0E702", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page used to view details of a saved account.", 0, @"", "5ED73BC4-3FA0-4552-8DC8-8B4BA1A5762C" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Detail
            //   Category: WebFarm
            //   Attribute: Node CPU Chart Hours
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BBA1FC0-AC56-4E58-9E99-EB20DA7AA415", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Node CPU Chart Hours", "CpuChartHours", "Node CPU Chart Hours", @"The amount of hours represented by the width of the Node CPU chart.", 2, @"24", "4D88CC76-B7F1-43A9-860F-97C75F730A2A" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C824483-6624-460B-9DD8-E127B25CA65D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8238E099-2EFD-44A5-BE64-78C831CBF43B" );

            // Attribute for BlockType
            //   BlockType: Web Farm Node Log List
            //   Category: WebFarm
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C824483-6624-460B-9DD8-E127B25CA65D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "60E40A77-17FC-4E29-AB01-5D87F38EA9D0" );
        }

        private void ChopBlockTypesv18_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 18.0 (18.0.10)",
                blockTypeReplacements: new Dictionary<string, string> {
                // blocks chopped in v18.0.10
{ "3131C55A-8753-435F-85F3-DF777EFBD1C8", "8adb5c0d-9a4f-4396-ab0f-deb552c094e1" }, // Benevolence Request List ( Finance )
{ "41AE0574-BE1E-4656-B45D-2CB734D1BE30", "6e9672e6-ee42-4aac-b0a9-b041c3b8368c" }, // Nameless Person List ( CRM )
{ "63ADDB5A-75D6-4E86-A031-98B3451C49A3", "6c824483-6624-460b-9dd8-e127b25ca65d" }, // Web Farm Node Log List ( WebFarm )
{ "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "1c3d7f3d-e8c7-4f27-871c-7ec20483b416" }, // Block Type List ( CMS )
{ "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" }, // Group Schedule Toolbox ( Group Scheduling )
{ "95F38562-6CEF-4798-8A4F-05EBCDFB07E0", "6bba1fc0-ac56-4e58-9e99-eb20da7aa415" }, // Web Farm Node Detail ( WebFarm )
{ "A3E648CC-0F19-455F-AF1D-B70A8205802D", "6c329001-9c04-4090-bed0-12e3f6b88fb6" }, // Block Type Detail ( CMS )
{ "C4191011-0391-43DF-9A9D-BE4987C679A4", "e1dce349-2f5b-46ed-9f3d-8812af857f69" }, // Bank Account List ( Finance )
{ "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "52df00e5-bc19-43f2-8533-a386db53c74f" }, // Campus List ( Core )
{ "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "e20b2fe2-2708-4e9a-b9fb-b370e8b0e702" }, // Saved Account List ( Finance )
                    // blocks chopped in v18.0.8
{ "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2", "770d3039-3f07-4d6f-a64e-c164acce93e1" }, // Signal Type List ( Core )
{ "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "633a75a7-7186-4cfd-ab80-6f2237f0bdd8" }, // AI Provider List ( Core )
{ "88820905-1B5A-4B82-8E56-F9A0736A0E98", "13f49f94-d9bc-434a-bb20-a6ba87bbe81f" }, // AI Provider Detail ( Core )
{ "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "120552e2-5c36-4220-9a73-fbbbd75b0964" }, // Audit List ( Core )
                    // blocks chopped in v18.0
{ "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "e44cac85-346f-41a4-884b-a6fb5fc64de1" }, // Page Short Link Click List ( CMS )
{ "4C4A46CD-1622-4642-A655-11585C5D3D31", "eddfcaff-70aa-4791-b051-6567b37518c4" }, // Achievement Type Detail ( Achievements )
{ "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "fbe75c18-7f71-4d23-a546-7a17cf944ba6" }, // Achievement Attempt Detail ( Engagement )
{ "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "b294c1b9-8368-422c-8054-9672c7f41477" }, // Achievement Attempt List ( Achievements )
{ "C26C7979-81C1-4A20-A167-35415CD7FED3", "09FD3746-48D1-4B94-AAA9-6896443AA43E" }, // Lava Shortcode List ( CMS )
{ "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "4acfbf3f-3d49-4ae3-b468-529f79da9898" }, // Achievement Type List ( Streaks )
{ "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" }, // Page Short Link List ( CMS )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "FutureWeeksToShow,SignupInstructions,EnableSignup,DeclineReasonPage" }, // Group Schedule Toolbox    
            } );
        }

        private void ChopBlocksForV18Up()
        {
            RegisterBlockAttributesForChop();
            //ChopBlockTypesv18_0();
        }

        #endregion
    }
}
