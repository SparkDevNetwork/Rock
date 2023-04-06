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
    public partial class Rollup_20230309 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStatementGeneratorDownloadLinkUp();
            UpdateCheckInAttendanceAnalyticsFirstDatesQueryToImprovePerformance();
            MobileGroupMemberListTemplateUp();
            MobileApplicationUsersSecurityGroupServiceJobUp();
            UpdateSignUpAttendanceDetailPageLayoutUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateStatementGeneratorDownloadLinkDown();
            MobileGroupMemberListTemplateDown();
            MobileApplicationUsersSecurityGroupServiceJobDown();
            UpdateSignUpAttendanceDetailPageLayoutDown();
        }

        /// <summary>
        /// JPH: Update the Statement Generator download link to the 1.14.2.0 version of the Statement Generator installer.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
DECLARE @DownloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9');
DECLARE @StatementGeneratorDefinedValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C');

UPDATE [AttributeValue]
SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.14.2/statementgenerator.msi'
WHERE [AttributeId] = @DownloadUrlAttributeId AND [EntityId] = @StatementGeneratorDefinedValueId;" );
        }

        /// <summary>
        /// JPH: Revert the Statement Generator download link to the 1.14.1.0 version of the Statement Generator installer.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
DECLARE @DownloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9');
DECLARE @StatementGeneratorDefinedValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C');

UPDATE [AttributeValue]
SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.14.1/statementgenerator.msi'
WHERE [AttributeId] = @DownloadUrlAttributeId AND [EntityId] = @StatementGeneratorDefinedValueId;" );
        }

        /// <summary>
        /// DL:Migration to modify the spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates query to improve performance for very large attendance datasets.
        /// </summary>
        private void UpdateCheckInAttendanceAnalyticsFirstDatesQueryToImprovePerformance()
        {
            Sql( $@"
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
	  @GroupTypeIds varchar(max)
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	DECLARE @GroupTypeTbl TABLE ( [Id] int )
	INSERT INTO @GroupTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )

	-- Get all the attendees
	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
	LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
	LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
    WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)
	AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )

	-- Get the first 5 occasions on which each person attended any of the selected group types regardless of group or campus.
	-- Multiple attendances on the same date are considered as a single occasion.
	SELECT DISTINCT
	    [PersonId]
	    , [TimeAttending]
	    , [StartDate]
	FROM (
	    SELECT 
	        [P].[Id] AS [PersonId]
	        , DENSE_RANK() OVER ( PARTITION BY [P].[Id] ORDER BY [AO].[OccurrenceDate] ) AS [TimeAttending]
	        , [AO].[OccurrenceDate] AS [StartDate]
	    FROM
	        [Attendance] [A]
	        INNER JOIN [AttendanceOccurrence] [AO] ON [AO].[Id] = [A].[OccurrenceId]
	        INNER JOIN [Group] [G] ON [G].[Id] = [AO].[GroupId]
	        INNER JOIN [PersonAlias] [PA] ON [PA].[Id] = [A].[PersonAliasId] 
	        INNER JOIN [Person] [P] ON [P].[Id] = [PA].[PersonId]
	        INNER JOIN @GroupTypeTbl [GT] ON [GT].[id] = [G].[GroupTypeId]
	    WHERE 
	        [P].[Id] IN ( SELECT [PersonId] FROM @PersonIdTbl )
	        AND [DidAttend] = 1
	) [X]
    WHERE [X].[TimeAttending] <= 5

END
" );
        }

        /// <summary>
        /// Updates the group member list template to the default value it was before this migration occurred.
        /// </summary>
        private void MobileGroupMemberListTemplateUp()
        {
            var standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Update the group member list mobile block template.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                    "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
                    Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
                    "Default",
                    @"<StackLayout StyleClass=""members-container"" 
    Spacing=""0"">
    {% for member in Members %}
        <Frame StyleClass=""member-container"" 
            Margin=""0""
            BackgroundColor=""White""
            HasShadow=""false""
            HeightRequest=""40"">
                <StackLayout Orientation=""Horizontal""
                    Spacing=""0""
                    VerticalOptions=""Center"">
                    <Rock:Image Source=""{{ member.PhotoUrl | Escape }}""
                        StyleClass=""member-person-image""
                        VerticalOptions=""Start""
                        Aspect=""AspectFit""
                        Margin=""0, 4, 14, 0""
                        BackgroundColor=""#e4e4e4"">
                        <Rock:CircleTransformation />
                    </Rock:Image>
                    
                    <StackLayout Spacing=""0"" 
                        HorizontalOptions=""FillAndExpand"">
                        <StackLayout Orientation=""Horizontal""
                        VerticalOptions=""Center"">
                            <Label StyleClass=""member-name""
                                Text=""{{ member.FullName }}""
                                LineBreakMode=""TailTruncation""
                                HorizontalOptions=""FillAndExpand"" />

                            <Grid ColumnSpacing=""4"" 
                                RowSpacing=""0""
                                ColumnDefinitions=""*, Auto""
                                VerticalOptions=""Start"">

                                <Rock:Icon IconClass=""chevron-right""
                                    VerticalTextAlignment=""Start""
                                    Grid.Column=""1"" 
                                    StyleClass=""note-read-more-icon""
                                    />
                            </Grid>
                        </StackLayout>
                            <Label StyleClass=""member-text""
                                Grid.Column=""0""
                                MaxLines=""2""
                                LineBreakMode=""TailTruncation"" 
                                Text=""{{ member.GroupRole | Escape }}"" /> 
                    </StackLayout>
                </StackLayout>
                <Frame.GestureRecognizers>
                    <TapGestureRecognizer Command=""{Binding PushPage}"" 
                     CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
                </Frame.GestureRecognizers>
            </Frame>
        <BoxView HorizontalOptions=""FillAndExpand""
            HeightRequest=""1""
            Color=""#cccccc"" />
    {% endfor %}
</StackLayout>",
                    standardIconSvg,
                    "standard-template.svg",
                    "image/svg+xml" );
        }


        /// <summary>
        /// Reverts the group member list template to the default value it was before this migration occurred.
        /// </summary>
        private void MobileGroupMemberListTemplateDown()
        {
            var standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Revert to the old group member list mobile block template.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                    "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
                    Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
                    "Default",
                    @"<StackLayout>
    {% assign groupMemberCount = Members | Size %}
    
    <Label StyleClass=""h1"" Text=""{{ Title | Escape }}"" />
    <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />

    {% if Members != empty %}
        <StackLayout Spacing=""0"" Margin=""0,20,0,0"">
            <Rock:Divider />
            
            {% for member in Members %}
				<StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""16"">
					<StackLayout.GestureRecognizers>
						<TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
					</StackLayout.GestureRecognizers>
					
					
					<Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=400' | Escape }}"" HeightRequest=""64"" WidthRequest=""64"" Aspect=""AspectFill"" BackgroundColor=""#ccc"">
						<Rock:RoundedTransformation CornerRadius=""8"" />
					</Rock:Image>
					
		
		            <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""Center"">
						<Label StyleClass=""h4"" Text=""{{ member.FullName | Escape }}"" />
						<Label StyleClass=""text, o-60"" Text=""{{ member.GroupRole | Escape }}"" />
					</StackLayout>
					<Rock:Icon IconClass=""chevron-right"" Margin=""0,0,20,0"" VerticalOptions=""Center"" />
				</StackLayout>
				<Rock:Divider />	
			{% endfor %}
        </StackLayout>
    {% endif %}
</StackLayout>",
                    standardIconSvg,
                    "standard-template.svg",
                    "image/svg+xml" );
        }

        /// <summary>
        /// Creates the Service Job responsible for creating a custom security group 
        /// for mobile application users.
        /// </summary>
        private void MobileApplicationUsersSecurityGroupServiceJobUp()
        {
            // Create the Mobile Application Users Rest group service job.
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV15DataMigrationsMobileApplicationUserRestGroup' AND [Guid] = '480E996E-6A31-40DB-AE98-BFF85CDED506' )
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
                  ,'Rock Update Helper v15.0 - Mobile Application Users Security Group'
                  ,'This job will add a new security group (if not exists) and put all mobile application rest users into it.'
                  ,'Rock.Jobs.PostV15DataMigrationsMobileApplicationUserRestGroup'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_MOBILE_APPLICATION_USERS_REST_GROUP}'
                  );
            END" );

        }

        /// <summary>
        /// Removes the service job responsible for creating a custom security group
        /// for mobile application users.
        /// </summary>
        private void MobileApplicationUsersSecurityGroupServiceJobDown()
        {
            // Delete the Mobile Application Users Rest group service job.
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_MOBILE_APPLICATION_USERS_REST_GROUP}'" );
        }

        /// <summary>
        /// JPH: Update the Sign-Up Attendance Detail Page's Layout to "Full Width".
        /// </summary>
        private void UpdateSignUpAttendanceDetailPageLayoutUp()
        {
            Sql( @"
DECLARE @LayoutId [int] = (SELECT [Id] FROM [Layout] WHERE [Guid] = '5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD');

UPDATE [Page]
SET [LayoutId] = @LayoutId
WHERE [Guid] = '73FC6F39-6194-483A-BF0D-7FDD1DD91C91';" );
        }

        /// <summary>
        /// JPH: Revert the Sign-Up Attendance Detail Page's Layout to "Left Sidebar".
        /// </summary>
        private void UpdateSignUpAttendanceDetailPageLayoutDown()
        {
            Sql( @"
DECLARE @LayoutId [int] = (SELECT [Id] FROM [Layout] WHERE [Guid] = '325B7BFD-8B80-44FD-A951-4E4763DA6C0D');

UPDATE [Page]
SET [LayoutId] = @LayoutId
WHERE [Guid] = '73FC6F39-6194-483A-BF0D-7FDD1DD91C91';" );
        }
    }
}
