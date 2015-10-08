// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class BadgeFixes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // Update Attendance Month Badge
            //

            Sql( @"/*
<doc>
	<summary>
 		This function returns the attendance data needed for the Attendance Badge. If no family role (adult/child)
		is given it is looked up.  If the individual is an adult it will return family attendance if it's a child
		it will return the individual's attendance. If a person is in two families once as a child once as an
		adult it will pick the first role it finds.
	</summary>

	<returns>
		* AttendanceCount
		* SundaysInMonth
		* Month
		* Year
	</returns>
	<param name=""PersonId"" datatype=""int"">Person the badge is for</param>
	<param name=""Role Guid"" datatype=""uniqueidentifier"">The role of the person in the family (optional)</param>
	<param name=""Reference Date"" datatype=""datetime"">A date in the last month for the badge (optional, default is today)</param>
	<param name=""Number of Months"" datatype=""int"">Number of months to display (optional, default is 24)</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_BadgeAttendance] 2 -- Ted Decker (adult)
		EXEC [dbo].[spCheckin_BadgeAttendance] 4 -- Noah Decker (child)
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_BadgeAttendance]
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

	-- if role (adult/child) is unknown determine it
	IF (@RoleGuid IS NULL)
	BEGIN
		SELECT TOP 1 @RoleGuid =  gtr.[Guid] 
			FROM [GroupTypeRole] gtr
				INNER JOIN [GroupMember] gm ON gm.[GroupRoleId] = gtr.[Id]
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
			WHERE gm.[PersonId] = @PersonId 
				AND g.[GroupTypeId] = (SELECT [ID] FROM [GroupType] WHERE [Guid] = @cGROUP_TYPE_FAMILY)
	END

	-- if start date null get today's date
	IF @ReferenceDate is null
		SET @ReferenceDate = getdate()

	-- set data boundaries
	SET @LastDay = dbo.ufnUtility_GetLastDayOfMonth(@ReferenceDate) -- last day is most recent day
	SET @StartDay = DATEADD(M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	-- make sure last day is not in future (in case there are errant checkin data)
	IF (@LastDay > getdate())
	BEGIN
		SET @LastDay = getdate()
	END

	--PRINT 'Last Day: ' + CONVERT(VARCHAR, @LastDay, 101) 
	--PRINT 'Start Day: ' + CONVERT(VARCHAR, @StartDay, 101) 

    declare @familyMemberPersonIds table (personId int); 
    declare @groupIds table (groupId int);

    insert into @familyMemberPersonIds SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId);
    insert into @groupIds SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]();

	-- query for attendance data
	IF (@RoleGuid = @cROLE_ADULT)
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] IN (select PersonId from @familyMemberPersonIds) 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END
	ELSE
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END

	
END" );

            //
            // Update 16 Week Badge
            //

            Sql( @" /*
    <doc>
	    <summary>
 		    This function returns the number of weekends a member of a family has attended a weekend service
		    in the last X weeks.
	    </summary>

	    <returns>
		    * Number of weeks
	    </returns>
	    <param name=""""PersonId"""" datatype=""""int"""">The person id to use</param>
	    <param name=""""WeekDuration"""" datatype=""""int"""">The number of weeks to use as the duration (default 16)</param>
	    <remarks>	
	    </remarks>
	    <code>
		    EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	    </code>
    </doc>
    */

    ALTER PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	    @PersonId int
	    ,@WeekDuration int = 16
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 

        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

        SELECT 
	        COUNT(DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) )
        FROM
	        [Attendance] a
	        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE 
	        [GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	        AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, ((@WeekDuration -1) * -1), @LastSunday) AND DATEADD(DAY, 1, @LastSunday)

    END" );

            //
            // Fix Security on the Data Integrity Page
            //

            Sql( @"  UPDATE [Auth]
	                    SET [Order] = 2
	                    WHERE [Guid] = '4DCB8D77-959A-4EC1-A709-AB684F3FA519'" );

            RockMigrationHelper.AddSecurityAuthForPage( "84FD84DF-F58B-4B9D-A407-96276C40AB7E", 1, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "6C2430D3-957D-F6B8-4AAE-8FBC99BA926B" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Revert security on data integrity page
            //

            Sql( @"  UPDATE [Auth]
	                    SET [Order] = 1
	                    WHERE [Guid] = '4DCB8D77-959A-4EC1-A709-AB684F3FA519'" );

            Sql( @"  DELETE FROM [Auth]
	                    WHERE [Guid] = '6C2430D3-957D-F6B8-4AAE-8FBC99BA926B'" );

        }
    }
}
