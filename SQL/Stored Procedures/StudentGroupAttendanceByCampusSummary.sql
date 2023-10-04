SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
	5/31/2023 Jon Corey - Added to GitHub
	6/19/2023 Jon Corey - Updated group types and allowed group types to be chosen by the user

	Test Scripts:

	Exec dbo._org_lakepointe_spReport_StudentGroupAttendanceByCampusSummary @StartDate = '20220801', @EndDate = '20221231', @CampusId = 3, @GroupTypes = '279,516'

	DECLARE @StartDate DATE = '20220801';
	DECLARE @EndDate DATE = '20221231';
	DECLARE @CampusId INT = 3;
	DECLARE @GroupTypes NVARCHAR(100) = '279,516';
*/
CREATE OR ALTER   PROCEDURE [dbo].[_org_lakepointe_spReport_StudentGroupAttendanceByCampusSummary]
	@StartDate DATETIME,
	@EndDate DATETIME,
	@CampusId INT,
	@GroupTypes NVARCHAR(100)
AS
DECLARE @SundayDates table (SundayDate date)
INSERT INTO @SundayDates
SELECT SundayDate FROM DBO.ufnUtility_GetSundaysBetweenDates(dbo.ufnUtility_GetSundayDate(@StartDate), dbo.ufnUtility_GetSundayDate(@EndDate)) 

DECLARE @GroupWeeks TABLE
(
    Id Int,
    GroupName NVARCHAR(100),
    SundayDate DATE
)
    
INSERT INTO @GroupWeeks
(
    Id,
    GroupName,
    SundayDate
)
SELECT g.Id,
    g.Name,
    s.SundayDate
FROM [Group] g
CROSS JOIN @SundayDates s
LEFT JOIN (
	SELECT av.[EntityId] AS [GroupId], av.[Value]
	FROM AttributeValue av
	JOIN Attribute a on a.Id = av.AttributeId
	WHERE a.EntityTypeId = 16 and a.[Key] = 'UnassignedGradeRange'
) av on av.GroupId = g.Id
WHERE (EXISTS (SELECT 1 FROM STRING_SPLIT(@GroupTypes, ',') WHERE CAST(value AS INT) = g.GroupTypeId)) -- Check if g.GroupTypeId is in the list of group types in the @GroupTypes parameter
    and g.CampusId = @CampusId
    and (((SELECT Count(Id) FROM GroupMember gm WHERE GroupID = g.Id and GroupMemberStatus <> 0) > 0) or (av.[Value] != '' and av.[Value] is not null))
    and g.IsActive = 1 and g.IsArchived = 0;

SELECT 
	gw.Id,
	gw.GroupName, 
	ao.AnonymousAttendanceCount, 
	CASE IsNull(ao.Id, 0) WHEN 0 THEN Null else 
		(SELECT count(a.Id) from attendance a where a.DidAttend = 1 and a.OccurrenceId = ao.Id) END as ActualAttendance,
	ao.DidNotOccur,
	gw.SundayDate,
	ao.OccurrenceDate AS 'Met',
	ao.ModifiedDateTime AS 'Updated'
FROM @GroupWeeks gw
LEFT OUTER JOIN AttendanceOccurrence ao on gw.Id = ao.GroupId and gw.SundayDate = ao.SundayDate
ORDER BY gw.SundayDate, gw.GroupName
GO


