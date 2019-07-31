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
    public partial class Rollup_0319 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            CheckInBadgeAttendance();
            UpdateContentChannelView();
            AddBlockSettingsAttendanceAndGivingAnalytics();
            UpdateContentBlackList();
            RemoveCommunicationSettings();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            AddBlockSettingsAttendanceAndGivingAnalyticsDown();
        }

        /// <summary>
        /// Codes the gen migrations up.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Account Entry:Campus Selector Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Selector Label", "CampusSelectorLabel", "", @"The label for the campus selector (only effective when ""Show Campus Selector"" is enabled).", 21, @"Campus", "0C4EDAD9-8CC2-465E-8C48-7FEDE90A0C7D" );
            // Attrib for BlockType: Public Profile Edit:Campus Selector Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Selector Label", "CampusSelectorLabel", "", @"The label for the campus selector (only effective when ""Show Campus Selector"" is enabled).", 16, @"Campus", "2C7C853D-381E-4A97-AF05-0A60FD701952" );
        }

        /// <summary>
        /// Codes the gen migrations down.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Account Entry:Campus Selector Label
            RockMigrationHelper.DeleteAttribute( "0C4EDAD9-8CC2-465E-8C48-7FEDE90A0C7D" );
            // Attrib for BlockType: Public Profile Edit:Campus Selector Label
            RockMigrationHelper.DeleteAttribute( "2C7C853D-381E-4A97-AF05-0A60FD701952" );
        }

        /// <summary>
        /// NA: From a a pull request Merge
        /// </summary>
        private void CheckInBadgeAttendance()
        {
            Sql( @"ALTER PROCEDURE[dbo].[spCheckin_BadgeAttendance]
                  @PersonId int
                  , @RoleGuid uniqueidentifier = null
                  , @ReferenceDate datetime = null
                  , @MonthCount int = 24
              AS
              BEGIN
                  DECLARE @cROLE_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
              
                  DECLARE @cROLE_CHILD uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
              
                  DECLARE @cGROUP_TYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
              
                  DECLARE @StartDay datetime
              
                  DECLARE @LastDay datetime

                  -- if role( adult / child ) is unknown determine it

    IF( @RoleGuid IS NULL )

    BEGIN
        SELECT TOP 1 @RoleGuid = gtr.[Guid]

            FROM[GroupTypeRole] gtr
               INNER JOIN[GroupMember] gm ON gm.[GroupRoleId] = gtr.[Id]

                INNER JOIN[Group] g ON g.[Id] = gm.[GroupId]

            WHERE gm.[PersonId] = @PersonId
AND g.[GroupTypeId] = (SELECT[ID] FROM [GroupType] WHERE[Guid] = @cGROUP_TYPE_FAMILY)

    END

	-- if start date null get today's date

    IF( @ReferenceDate IS NULL)

        SET @ReferenceDate = getdate()

    -- set data boundaries
    SET @LastDay = [dbo].[ufnUtility_GetLastDayOfMonth] (@ReferenceDate) -- last day is most recent day
SET @StartDay = DATEADD( M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	-- make sure last day is not in future( in case there are errant checkin data)

    IF( @LastDay > getdate())

        SET @LastDay = getdate()

    --PRINT 'Last Day: ' + CONVERT( VARCHAR, @LastDay, 101)
	--PRINT 'Start Day: ' + CONVERT( VARCHAR, @StartDay, 101)

    DECLARE @familyMemberPersonIds table( [PersonId] int);

        IF( @RoleGuid = @cROLE_ADULT )

        INSERT INTO @familyMemberPersonIds SELECT[Id] FROM[dbo].[ufnCrm_FamilyMembersOfPersonId]
        (@PersonId)
  ELSE IF( @RoleGuid = @cROLE_CHILD )

        INSERT INTO @familyMemberPersonIds SELECT @PersonId

	-- query for attendance data

    SELECT
        COUNT( b.[Attended]) AS[AttendanceCount]
		, (SELECT[dbo].[ufnUtility_GetNumberOfSundaysInMonth] (DATEPART(year, b.[SundayDate]), DATEPART( month, b.[SundayDate]), 'True' )) AS[SundaysInMonth]
		, DATEPART( month, b.[SundayDate]) AS[Month]
		, DATEPART( year, b.[SundayDate]) AS[Year]
     FROM(
         SELECT
             s.[SundayDate], NULL AS [Attended]
        FROM
             dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
 
         UNION ALL
 
             SELECT
                 DISTINCT ao.[SundayDate], 1 AS[Attended]
             FROM
 
                 [AttendanceOccurrence] ao
                 INNER JOIN[dbo].[ufnCheckin_WeeklyServiceGroups]() wg ON ao.[GroupId] = wg.[Id]
 
                 INNER JOIN [Attendance] a ON ao.[Id] = a.[OccurrenceId] AND a.[DidAttend] = 1
 
                 INNER JOIN [PersonAlias] pa ON a.[PersonAliasId] = pa.[Id] AND pa.[PersonId] IN (SELECT[PersonId] FROM @familyMemberPersonIds )

            WHERE
                ao.[OccurrenceDate] BETWEEN @StartDay AND @LastDay
    ) b
    GROUP BY DATEPART( month, b.[SundayDate]), DATEPART( year, b.[SundayDate])

    OPTION( MAXRECURSION 1000)
END");
        }

        /// <summary>
        /// SK:Updates the content channel view.
        /// </summary>
        private void UpdateContentChannelView()
        {
            Sql( @"
DECLARE @BlockTypeId int, @AttributeId int
SET @BlockTypeId = (SELECT [Id] FROM [dbo].[BlockType] WHERE [Guid] = '63659EBE-C5AF-4157-804A-55C7D565110E')
SET @AttributeId = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Key]='DetailPage' AND [EntityTypeQualifierColumn]='BlockTypeId' AND [EntityTypeQualifierValue]=@BlockTypeId)
UPDATE
	[A]
SET 
	[Value] = [B].[Guid]
FROM
	[AttributeValue] A
INNER JOIN
	[Page] B ON A.[Value] = CONVERT(nvarchar,B.[Id])
WHERE [AttributeId]=@AttributeId
" );
        }

        /// <summary>
        /// ED:Adds the block settings to attendance and giving analytics.
        /// </summary>
        private void AddBlockSettingsAttendanceAndGivingAnalytics()
        {
            // Attrib for BlockType: Attendance Analytics:Filter Column Count
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Column Count", "FilterColumnCount", "", @"The number of check boxes for each row.", 14, @"1", "244327E8-01EE-4860-9F12-4CF6144DFD61" );
            // Attrib for BlockType: Attendance Analytics:Filter Column Direction
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Column Direction", "FilterColumnDirection", "", @"Choose the direction for the checkboxes for filter selections.", 13, @"vertical", "0807FC61-26CE-41A1-9C54-8C4FA0DB6B5C" );
            // Attrib for BlockType: Giving Analytics:Filter Column Count
            RockMigrationHelper.UpdateBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Column Count", "FilterColumnCount", "", @"The number of check boxes for each row.", 4, @"1", "43B7025A-778A-4107-8243-D91A2FA74AA4" );
            // Attrib for BlockType: Giving Analytics:Filter Column Direction
            RockMigrationHelper.UpdateBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Column Direction", "FilterColumnDirection", "", @"Choose the direction for the checkboxes for filter selections.", 3, @"vertical", "11C6953E-E176-40A2-9BF7-979344BD8FD8" );
            // Attrib Value for Block:Attendance Reporting, Attribute:Filter Column Count Page: Attendance Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "244327E8-01EE-4860-9F12-4CF6144DFD61", @"3" );
            // Attrib Value for Block:Attendance Reporting, Attribute:Filter Column Direction Page: Attendance Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "0807FC61-26CE-41A1-9C54-8C4FA0DB6B5C", @"horizontal" );
            // Attrib Value for Block:Giving Analysis, Attribute:Filter Column Count Page: Giving Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "784C58EF-B1B8-4237-BF12-E04DE8271A5A", "43B7025A-778A-4107-8243-D91A2FA74AA4", @"3" );
            // Attrib Value for Block:Giving Analysis, Attribute:Filter Column Direction Page: Giving Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "784C58EF-B1B8-4237-BF12-E04DE8271A5A", "11C6953E-E176-40A2-9BF7-979344BD8FD8", @"horizontal" );
        }

        /// <summary>
        /// ED: (Down)Adds the block settings attendance and giving analytics down.
        /// </summary>
        private void AddBlockSettingsAttendanceAndGivingAnalyticsDown()
        {
            // Attrib for BlockType: Giving Analytics:Filter Column Direction
            RockMigrationHelper.DeleteAttribute( "11C6953E-E176-40A2-9BF7-979344BD8FD8" );
            // Attrib for BlockType: Giving Analytics:Filter Column Count
            RockMigrationHelper.DeleteAttribute( "43B7025A-778A-4107-8243-D91A2FA74AA4" );
            // Attrib for BlockType: Attendance Analytics:Filter Column Direction
            RockMigrationHelper.DeleteAttribute( "0807FC61-26CE-41A1-9C54-8C4FA0DB6B5C" );
            // Attrib for BlockType: Attendance Analytics:Filter Column Count
            RockMigrationHelper.DeleteAttribute( "244327E8-01EE-4860-9F12-4CF6144DFD61" );
        }

        /// <summary>
        /// ED: Delete the attribute value for the Global blacklist if the value is exactly ', config' 
        /// </summary>
        private void UpdateContentBlackList()
        {
            Sql( @"
                DELETE FROM AttributeValue 
                WHERE [Value] = ', config' 
                 AND [AttributeId] = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5' )" );
        }

        /// <summary>
        /// ED: Removes the communication settings page, block, and attributes(Rollback Prevent Send During DND)
        /// </summary>
        private void RemoveCommunicationSettings()
        {
            // Delete the block attribute value for the category attribute on the communication setting block
            RockMigrationHelper.DeleteBlockAttributeValue( "8083E072-A5F6-4BA0-8110-7D3A9E94A05F", "25E02584-4B8F-4A8F-8558-8D2EDA2C5393" );
            // Delete the block attribute category on the systems settings block type
            RockMigrationHelper.DeleteAttribute( "25E02584-4B8F-4A8F-8558-8D2EDA2C5393" );
            // Delete the System Setting attribute for Do Not Disturb Start
            RockMigrationHelper.DeleteAttribute( "4A558666-32C7-4490-B860-0F41358E14CA" );
            // Delete the System Setting attribute for Do Not Disturb End
            RockMigrationHelper.DeleteAttribute( "661802FC-E636-4CE2-B75A-4AC05595A347" );
            // Delete the System Setting attribute for Do Not Disturb Active
            RockMigrationHelper.DeleteAttribute( "1BE30413-5C90-4B78-B324-BD31AA83C002" );
            // Delete the Communication Settings Category
            RockMigrationHelper.DeleteCategory( "1059CCF2-933F-488E-8DBF-4FEC64A12409" );
            // Delete the Communication Setting Block
            RockMigrationHelper.DeleteBlock( "8083E072-A5F6-4BA0-8110-7D3A9E94A05F" );
            // Delete the system settings block type
            RockMigrationHelper.DeleteBlockType( "41A585E0-4522-40FA-8CC6-A411C70340F7" );
            // Delete the Communication Settings page
            RockMigrationHelper.DeletePage( "5B67480F-418D-4916-9C39-A26D2F8FA95C" );
        }
    }
}
