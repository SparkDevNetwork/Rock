/*
<doc>
	<summary>
		This procedure can be used to find people who have not
		checked-in during the last few weeks but who have checked-in
		at least X times over the past 7 weeks. 

		We use this procedure with our custom SqlToLavaEmail job in order
		to send this 'report' to various teams each Monday morning.
	</summary>

	<returns>
		* Person Id
		* FirstName
		* LastName
		* Gender
		* Campus
		* Grade
		* BestContact
		* BestPhone
		* Home
		* Cell
		* Address
		* and seven columns (Wk1 Wk2 ... Wk7) with the datetime of the check-in on that date.
	</returns>
	<param name='GroupTypeId' datatype='int'>The group type id (area) of the associated child check-in groups you want included in the report.</param>
	<param name='GenderFilter' datatype='int'>Used to filter based on gender. Use 1 for males, 2 for females, NULL to include everyone.</param>
	<param name='MinGradeFilter' datatype='int'>The minimum grade to filter. Use NULL for all grades.</param>
	<param name='MaxGradeFilter' datatype='int'>The maximum grade to filter. Use NULL for all grades.</param>
	<param name='CampusId' datatype='int'>The Campus Id filter on (use NULL to see all campuses)..</param>

	<remarks>	
		None
	</remarks>
	<code>
		EXEC [dbo].[_com_centralaz_spNotSeenLately] 50, NULL, 9,12, 1
	</code>
</doc>
*/

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[_com_centralaz_spNotSeenLately]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[_com_centralaz_spNotSeenLately]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[_com_centralaz_spNotSeenLately]
	@GroupTypeId INT
	,@GenderFilter INT = NULL
	,@MinGradeFilter INT = NULL
	,@MaxGradeFilter INT = NULL
	,@CampusId INT = NULL
AS
BEGIN
-------------------------------------------------------------------

 DECLARE @ConsecutiveMissedWeeks INT = 2
-- DECLARE @GroupTypeId INT = 50 -- 50 is students, 19 is nursery
 DECLARE @NumberAttended INT = 3
-- DECLARE @GenderFilter INT = NULL -- (1 = Male, 2 = Female, NULL is everyone)
-- DECLARE @MinGradeFilter INT = 9
-- DECLARE @MaxGradeFilter INT = 12
 --DECLARE @CampusId INT = 1 -- (1 == Mesa, Gilbert, Queen Creek,Glendale, Ahwatukee)

---------------------------------------------
-- These are the Ids for our Rock system:
DECLARE @ActiveRecordStatus INT = 3
DECLARE @BestContactAttribId INT = 1900;
DECLARE @BestPhoneContactValueId INT = 1276;
DECLARE @HomePhoneValueId INT = 12;
DECLARE @CellPhoneValueId INT = 13;
---------------------------------------------

-- Build table variable to hold all related groups
DECLARE @RelatedGroups TABLE (GroupId INT) 
INSERT INTO @RelatedGroups SELECT [Id] FROM [Group] WHERE GroupTypeId = @GroupTypeId

DECLARE @AttendedStartDate DateTime
DECLARE @AttendedEndDate DateTime
DECLARE @MissedStartDate DateTime
DECLARE @MissedEndDate DateTime

-- This week's Monday
SET @AttendedEndDate = DATEADD(wk, DATEDIFF(wk,0,getdate()), 0);
-- The start of the weekend 8 weeks ago
SET @AttendedStartDate = DATEADD( d, -2, DATEADD( wk, -6, @AttendedEndDate));

-- This weekend
SET @MissedEndDate = @AttendedEndDate;
-- Two weeks ago
SET @MissedStartDate = DATEADD( wk, -@ConsecutiveMissedWeeks, @MissedEndDate );

DECLARE @Wk1StartDate DateTime
DECLARE @Wk2StartDate DateTime
DECLARE @Wk3StartDate DateTime
DECLARE @Wk4StartDate DateTime
DECLARE @Wk5StartDate DateTime
DECLARE @Wk6StartDate DateTime
DECLARE @Wk7StartDate DateTime
DECLARE @Wk8StartDate DateTime

-- Set up the Grade Transition Date
DECLARE @today DATETIME = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)
DECLARE @year INT = DATEPART(year, @today)
DECLARE @GradeTransitionDate DATETIME
SELECT @GradeTransitionDate =  CAST( [Value] + '/' + CAST(@year AS varchar) AS DATETIME)  FROM Attribute a INNER JOIN AttributeValue av on av.AttributeId = a.Id WHERE a.[Key] = 'GradeTransitionDate'

SET @Wk1StartDate = @MissedEndDate
SET @Wk2StartDate = DATEADD( wk, -1, @MissedEndDate) 
SET @Wk3StartDate = DATEADD( wk, -2, @MissedEndDate) 
SET @Wk4StartDate = DATEADD( wk, -3, @MissedEndDate) 
SET @Wk5StartDate = DATEADD( wk, -4, @MissedEndDate) 
SET @Wk6StartDate = DATEADD( wk, -5, @MissedEndDate) 
SET @Wk7StartDate = DATEADD( wk, -6, @MissedEndDate) 
SET @Wk8StartDate = DATEADD( wk, -7, @MissedEndDate) 

--SELECT @Wk1StartDate, @Wk2StartDate, @Wk3StartDate, @Wk4StartDate, @Wk5StartDate, @Wk6StartDate, @Wk7StartDate,@Wk8StartDate

SELECT p.Id, p.FirstName, p.LastName, CASE WHEN p.Gender = 1 THEN 'Male' ELSE 'Female' END as 'Gender', c.Name as 'Campus', 12 - [dbo].ufnCrm_GetGradeOffset( p.GraduationYear, @GradeTransitionDate ) as 'Grade', 
paBC.Value as 'BestContact', pnBP.NumberFormatted as 'BestPhone',
pnHP.NumberFormatted as 'Home', pnCP.NumberFormatted as 'Cell',  [dbo].[ufnCrm_GetAddress](p.Id, 'Home', 'Full') AS 'Address',

		(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk2StartDate AND @Wk1StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk1'
		 
		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk3StartDate AND @Wk2StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk2'

		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk4StartDate AND @Wk3StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk3'

		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk5StartDate AND @Wk4StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk4'

		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk6StartDate AND @Wk5StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk5'

		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk7StartDate AND @Wk6StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk6'

		 ,(	SELECT top 1 ( SELECT CONVERT(VARCHAR(20), att.StartDateTime, 100) + ' ' + g.Name)
			FROM   Attendance AS att 
			INNER JOIN [Group] g ON att.GroupId = g.Id
			WHERE  att.PersonAliasId  IN (SELECT Id FROM PersonAlias alias WHERE alias.PersonId = p.id ) AND ( att.GroupId IN ( SELECT [GroupId] From @RelatedGroups ) ) 
			AND ( att.StartDateTime between @Wk8StartDate AND @Wk7StartDate ) AND att.DidAttend = 1
		 ) AS 'Wk7'

FROM   Person AS p WITH(NOLOCK) 
       LEFT OUTER JOIN AttributeValue paBC WITH(NOLOCK) ON p.Id = paBC.EntityId AND paBC.AttributeId = @BestContactAttribId
       LEFT OUTER JOIN PhoneNumber AS pnBP WITH(NOLOCK) ON p.Id = pnBP.PersonId AND pnBP.NumberTypeValueId = @BestPhoneContactValueId AND pnBP.IsUnlisted = 0
       LEFT OUTER JOIN PhoneNumber AS pnCP WITH(NOLOCK) ON p.Id = pnCP.PersonId AND pnCP.NumberTypeValueId = @CellPhoneValueId AND pnCP.IsUnlisted = 0
       LEFT OUTER JOIN PhoneNumber AS pnHP WITH(NOLOCK) ON p.Id = pnHP.PersonId AND pnHP.NumberTypeValueId = @HomePhoneValueId AND pnHP.IsUnlisted = 0
       INNER JOIN [GroupMember] gm WITH(NOLOCK) ON gm.PersonId = p.Id
	   INNER JOIN [Group] g WITH(NOLOCK) ON g.Id = gm.GroupId AND g.GroupTypeId = 10 -- 10 is family group type
	   INNER JOIN [Campus] c WITH(NOLOCK) ON c.Id = g.CampusId
WHERE  
	-- Matches the Gender Filter
		( @GenderFilter IS NULL OR p.Gender = @GenderFilter )
	AND
	-- Matches the Min / Max Grade Filter
		( @MinGradeFilter IS NULL OR @MinGradeFilter <= 12 - [dbo].ufnCrm_GetGradeOffset( p.GraduationYear, @GradeTransitionDate ) )
		AND ( @MaxGradeFilter IS NULL OR @MaxGradeFilter >= 12 - [dbo].ufnCrm_GetGradeOffset( p.GraduationYear, @GradeTransitionDate ) )
	-- Matches the Campus Filter
	AND
		( @CampusId IS NULL OR g.CampusId = @CampusId )
	AND
		-- Have Attended at least X times between @AttendedStartDate and @AttendedEndDate
		p.Id IN (
						-- Select all the person IDs that have attended X times between the dates...
						SELECT p.Id
                        FROM   (
								SELECT DISTINCT pAlias.PersonId, COUNT(att.Id) AS number_attended, g.Id, g.Name
								FROM   Attendance AS att WITH(NOLOCK) 
								INNER JOIN [Group] AS g ON g.Id = att.GroupId
								INNER JOIN [PersonAlias] AS pAlias ON pAlias.Id = att.PersonAliasId
								WHERE  ( g.Id IN ( SELECT [GroupId] From @RelatedGroups ) )
										   AND ( att.StartDateTime between @AttendedStartDate and @AttendedEndDate )
										   AND att.DidAttend = 1
								GROUP  BY pAlias.PersonId,
										  g.Id,
										  g.Name
								) AS v
						   INNER JOIN Person AS p  ON v.PersonId = p.Id
                        WHERE  ( v.number_attended >= @NumberAttended )
                               AND p.RecordStatusValueId = @ActiveRecordStatus
                 )
	AND
		-- Have NOT Attended at least X times between @MissedStartDate and @MissedEndDate	
		p.Id NOT IN (
						-- all the people who HAVE attended at least X times between @MissedStartDate and @MissedEndDate
						SELECT p.Id
                        FROM   (
								SELECT DISTINCT pAlias.PersonId, COUNT(att.Id) AS number_attended, g.Id, g.Name
								FROM   Attendance AS att WITH(NOLOCK) 
								INNER JOIN [Group] AS g ON g.Id = att.GroupId
								INNER JOIN [PersonAlias] AS pAlias ON pAlias.Id = att.PersonAliasId
								WHERE  ( g.Id IN ( SELECT [GroupId] From @RelatedGroups ) )
										   AND ( att.StartDateTime between @MissedStartDate and @MissedEndDate )
										   AND att.DidAttend = 1
								GROUP  BY pAlias.PersonId,
										  g.Id,
										  g.Name
								) AS v
						   INNER JOIN Person AS p  ON v.PersonId = p.Id
                        WHERE  ( v.number_attended >= 1 )
                               AND p.RecordStatusValueId = @ActiveRecordStatus
                 )

ORDER  BY p.LastName ASC
-------------------------------------------------------------------
END
GO
