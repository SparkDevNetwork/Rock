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
    public partial class CreateFunctionsAndSpForAttendanceBadge : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // create function for getting checkin group types
Sql(@"/*
<doc>
	<summary>
 		This function returns all group types that are used to denote 
		groups that are for tracking attendance for weekly services
	</summary>

	<returns>
		* GroupTypeId
		* Guid
		* Name
	</returns>
	<remarks>
		Uses the following constants:
			* Defined Value - Check-in Filter: 6BCED84C-69AD-4F5A-9197-5C0F9C02DD34
			* Defined Value - Check-in Template: 4A406CB0-495B-4795-B788-52BDFDE00B01
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
	</code>
</doc>
*/


CREATE FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
RETURNS TABLE AS

RETURN ( WITH
	cteServiceGroupTypes ([Id], [Guid], [Name])
	AS (

		SELECT [Id], [Guid], [Name]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] in (SELECT [Id] FROM [DefinedValue] WHERE [Guid] in ('6BCED84C-69AD-4F5A-9197-5C0F9C02DD34', '4A406CB0-495B-4795-B788-52BDFDE00B01'))

		UNION ALL

		SELECT g.[Id], g.[Guid], g.[Name]
		FROM [GroupType] g
			INNER JOIN cteServiceGroupTypes r ON g.[InheritedGroupTypeId] = r.[Id]

	)

 SELECT DISTINCT * FROM cteServiceGroupTypes )
");




// create function for getting checkin groups
Sql(@"/*
<doc>
	<summary>
 		This function returns all groups that are used to denote groups that are for tracking
		attendance for weekly services.
	</summary>

	<returns>
		* Id
		* Name
		* Description
		* CampusId
		* IsActive
		* Guid
		* Order
	</returns>
	<remarks>
		Uses the function dbo.ufnCheckin-WeeklyServiceGroupTypes() to get 
		group types that are used to track weekly services.
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroups]()
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroups]()
RETURNS TABLE AS

RETURN ( 
	SELECT 
		[Id] 
		, [Name] 
		, [Description]
		, [CampusId]
		, [IsActive]
		, [Guid]
		, [Order]
	FROM
		[Group] g
	WHERE
		[GroupTypeId] in (SELECT [Id] FROM dbo.ufnCheckin_WeeklyServiceGroupTypes())
)
");


// create function for people in families
Sql(@"/*
<doc>
	<summary>
 		This function returns all people in a family with the provided person id includes the person.
	</summary>

	<returns>
		* Id
		* NickName
		* LastName
		* Guid
		* GroupId
		* GroupGuid
		* GroupName
		* GroupRoleId
	</returns>
	<param name=""PersonId"" datatype=""int"">Person Id of family member</param>
	<remarks>
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](6) -- Ted Decker's family in sample data
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId int)
RETURNS TABLE AS

RETURN ( 
	SELECT 
		p.[Id]
		, [NickName]
		, [LastName]
		, p.[Guid]
		, g.[Id] as [GroupId]
		, g.[Guid] as [GroupGuid]
		, g.[Name] as [GroupName]
		, gm.[GroupRoleId]

	FROM
		[Person] p
		INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
		INNER JOIN [Group] g ON g.Id = gm.[GroupId]
	WHERE
		g.[GroupTypeId] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E')
		AND g.[Id] IN (SELECT [GroupId] FROM [GroupMember] WHERE [PersonId] = @PersonId)
)
");

// function get sunday date
Sql(@"/*
<doc>
	<summary>
 		This function returns the Sunday date of a given date.
	</summary>

	<returns>
		The Sunday of the date given with Sunday being the last day of the week.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetSundayDate](getdate())
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnUtility_GetSundayDate](@InputDate datetime) 

RETURNS date AS

BEGIN
	DECLARE @DayOfWeek int
	DECLARE @DaysToAdd int
	DECLARE @SundayDate datetime

	-- get day of the week in a way that will work with all SQL Server settings Monday = 1
	SET @DayOfWeek = (DATEPART(weekday, @InputDate) + @@DATEFIRST + 5) % 7 + 1

	-- calculate days to add to get to Sunday
	SET @DaysToAdd = 7 - @DayOfWeek

	SET @SundayDate = DATEADD(day, @DaysToAdd, @InputDate)

	RETURN @SundayDate
END");


// create function get sundays between dates
Sql(@"/*
<doc>
	<summary>
 		This function returns a list of Sundays between two dates
	</summary>

	<returns>
		* SundayDate - datetime
	</returns>
	<remarks>
		WARNING: Depending if you are asking for more than 100 weeks you'll need to add OPTION (MAXRECURSION 1000) to your call 
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_GetSundaysBetweenDates](DATEADD(week, -24, getdate()), getdate())
	</code>
</doc>
*/


CREATE FUNCTION [dbo].[ufnUtility_GetSundaysBetweenDates](@StartDate datetime, @EndDate datetime)
RETURNS TABLE AS

RETURN ( WITH
	
	cteAllDates AS
	(
		SELECT dbo.ufnUtility_GetSundayDate(@StartDate) AS DateOf
			UNION ALL
			SELECT DATEADD(day, 7, DateOf)
				FROM cteAllDates
				WHERE
				DATEADD(day, 7, DateOf) <= @EndDate
	)

	-- select out Sundays in a way that works across SQL Server setups
	SELECT DateOf AS [SundayDate]
		FROM cteAllDates  ) ");


// create function get first day ot month
Sql(@"


/*
<doc>
	<summary>
 		This function returns the date of the first of the month.
	</summary>

	<returns>
		Datetime of the first of the month.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetFirstDayOfMonth](getdate())
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnUtility_GetFirstDayOfMonth](@InputDate datetime) 

RETURNS datetime AS

BEGIN

	RETURN DATEADD(month, DATEDIFF(month, 0, getdate()), 0)
END");

// get last day of month
Sql(@"/*
<doc>
	<summary>
 		This function returns the date of the last day of the month.
	</summary>

	<returns>
		Datetime of the last day of the month.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetLastDayOfMonth](getdate())
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnUtility_GetLastDayOfMonth](@InputDate datetime) 

RETURNS datetime AS

BEGIN

	RETURN DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, @InputDate) + 1, 0))
END");

// create function to return the number of Sundays in a month
Sql(@"/*
<doc>
	<summary>
 		This function returns the number of Sundays in a given month.
	</summary>

	<returns>
		An integer of the number of Sundays in a given month
	</returns>
	<param name=""Year"" datatype=""int"">Year to use for the date</param>
	<param name=""Month"" datatype=""int"">Month to use for the month</param>
	<param name=""Exclude Future"" datatype=""bit"">Used to determine if future Sundays should be counted</param>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetNumberOfSundaysInMonth](3,2014, 'False')
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnUtility_GetNumberOfSundaysInMonth](@Month int, @Year int, @ExcludeFuture bit) 

RETURNS int AS

BEGIN
	DECLARE @FirstDayOfMonth datetime
	DECLARE @SundayCount int
	
	-- get date of first day
	SET @FirstDayOfMonth = CAST(CONVERT(varchar, @Year) + '-' + CONVERT(varchar, @Month) + '-01' AS datetime)

	-- fill a table with all dates in month
	;with cteAllDates AS
	(
		SELECT @FirstDayOfMonth AS DateOf
			UNION ALL
			SELECT DateOf+1
				FROM cteAllDates
				WHERE
				MONTH(DateOf+1)=MONTH(@FirstDayOfMonth)
	)

	-- select out Sundays in a way that works across SQL Server setups
	SELECT @SundayCount = COUNT(DateOf) 
		FROM cteAllDates 
		WHERE 
			(@ExcludeFuture = 0 AND ((DATEPART(weekday, DateOf) + @@DATEFIRST + 5) % 7 + 1) = 7 )
			OR 
			(@ExcludeFuture = 1 AND [DateOf] <= getdate() AND ((DATEPART(weekday, DateOf) + @@DATEFIRST + 5) % 7 + 1) = 7 )



	RETURN @SundayCount
END");

// create stoed procedure to get attendance for badge
Sql(@"
/*
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
		EXEC [dbo].[spCheckin_BadgeAttendance] 6 -- Ted Decker (adult)
		EXEC [dbo].[spCheckin_BadgeAttendance] 8 -- Noah Decker (child)
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCheckin_BadgeAttendance]
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
	SET @StartDay = DATEADD(month, (@MonthCount * -1), @LastDay) -- start day is the oldest day

	--PRINT 'Last Day: ' + CONVERT(VARCHAR, @LastDay, 101) 
	--PRINT 'Start Day: ' + CONVERT(VARCHAR, @StartDay, 101) 

	-- query for attendance data
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
					WHERE 
						[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND (
							@RoleGuid = @cROLE_ADULT AND a.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId)) -- adult
							OR
							@RoleGuid = @cROLE_CHILD AND a.[PersonId] = @PersonId -- child
						)
						AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
					) a ON [AttendedSunday] = s.[SundayDate]

	) [CheckinDates]
	GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
	OPTION (MAXRECURSION 1000)
	
	
END
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
