/****** Object:  StoredProcedure [dbo].[_org_lakepointe_spReport_Checkin_Dashboard_v2]    Script Date: 2/29/2024 3:52:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/************************************************************************************************************************************************
??/??/???? Steve Swaringen - Created to use for the Check-In Troubleshooting Page
08/22/2023 Jeff McClure - Updated to include all of the serving hours a person might be serving in the last record set.  Column: ServingHours
09/26/2023 Jeff McClure - Updated to exclude inactive groups from all sections
03/01/2024 Steve Swaringen - Update Belongs to Groups Requiring Belongs to also check for missing group requirements
06/18/2024 shanedlp - Updated to include 2024 Summer Blast Volunteer Check-In Groups(684) and Placement Groups(674)
08/28/2024 Jon Corey - Updated "Serving Teams" section to pull all groups with requirements and to filter out archived/inactive groups and members

Test:

DECLARE @PersonId INT = 260979 --<----Change this to any personid
DECLARE @Person NVARCHAR(36) = (SELECT GUID FROM PersonAlias pa WHERE PersonId = @PersonId)
DECLARE @Campus NVARCHAR(36) = (SELECT c.guid FROM Campus c JOIN Person p ON p.PrimaryCampusId = c.id WHERE p.Id = @PersonId)
SELECT @Person AS PersonAliasGuid, @Campus AS CampusId
EXEC [dbo].[_org_lakepointe_spReport_Checkin_Dashboard_v2]
	@Person,
	@Campus

************************************************************************************************************************************************/
ALTER   PROCEDURE [dbo].[_org_lakepointe_spReport_Checkin_Dashboard_v2]
	@Person NVARCHAR(36), --Sending this in as Nvarchar because it's coming off of a query string and might be empty
	@Campus NVARCHAR(36) --Sending this in as Nvarchar because it's coming off of a query string and might be empty
AS

-- Check-in Dashboard (Troubleshooting Tool)
IF @Person IS NULL
	RETURN; --No need to continue if we don't know the person

-- Already Checked In
SELECT g.[Name] AS [GroupName], a.StartDateTime AS [CheckInTime], CASE WHEN p.Id IS NULL THEN 'OVERRIDE' ELSE p.NickName + ' ' + p.LastName END AS [CheckedInBy]
FROM dbo.[Attendance] a
JOIN dbo.[AttendanceOccurrence] ao ON ao.Id = a.OccurrenceId AND ao.OccurrenceDate = CONVERT(DATE, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Central Standard Time' AS DATETIME))
JOIN dbo.[Group] g ON g.Id = ao.GroupId
LEFT JOIN dbo.[PersonAlias] pa ON pa.Id = a.CheckedInByPersonAliasId
LEFT JOIN dbo.[Person] p ON p.Id = pa.PersonId
JOIN dbo.[PersonAlias] cpa ON cpa.Id = a.PersonAliasId AND cpa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER)
WHERE a.DidAttend = 1
	AND a.EndDateTime IS NULL
	AND g.IsActive = 1;

-- Allow Check In By
SELECT other.NickName, other.LastName, gtr.[Name], other.Id
FROM dbo.[GroupMember] gm
JOIN dbo.[PersonAlias] pa ON pa.PersonId = gm.PersonId AND pa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER)
JOIN dbo.[Group] g ON g.Id = gm.GroupId AND g.GroupTypeId = 11
JOIN dbo.[GroupMember] others ON others.GroupId = g.Id AND others.GroupRoleId = 8
JOIN dbo.[Person] other ON other.Id = others.PersonId
JOIN dbo.[GroupTypeRole] gtr ON gtr.Id = others.GroupRoleId
WHERE gm.GroupRoleId = 5
	AND g.IsActive = 1
ORDER BY other.LastName, other.NickName;

-- Can Check In
SELECT other.NickName, other.LastName, gtr.[Name], other.Id
FROM dbo.[GroupMember] gm
JOIN dbo.[PersonAlias] pa ON pa.PersonId = gm.PersonId AND pa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER)
JOIN dbo.[Group] g ON g.Id = gm.GroupId AND g.GroupTypeId = 11
JOIN dbo.[GroupMember] others ON others.GroupId = g.Id AND others.GroupRoleId = 9
JOIN dbo.[Person] other ON other.Id = others.PersonId
JOIN dbo.[GroupTypeRole] gtr ON gtr.Id = others.GroupRoleId
WHERE gm.GroupRoleId = 5
	AND g.IsActive = 1
ORDER BY other.LastName, other.NickName;

-- DOB, Grade, Inactive
SELECT p.NickName, p.LastName, p.BirthDate, p.GraduationYear,
	CASE
		WHEN p.RecordStatusValueId = 3 THEN 'Active'
		WHEN p.RecordStatusValueId = 4 THEN 'Inactive'
		WHEN p.RecordStatusValueId = 5 THEN 'Pending'
	END AS [RecordStatus]
FROM dbo.[PersonAlias] pa
JOIN dbo.[Person] p ON p.Id = pa.PersonId
WHERE pa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER);

-- Belongs to groups requiring belongs
SELECT g.[Name] AS [Group], c.[Name] AS [Campus],
	CASE
		WHEN gm.GroupMemberStatus = 0 THEN 'Inactive'
		WHEN gm.GroupMemberStatus = 1 THEN 'Active'
		WHEN gm.GroupMemberStatus = 2 THEN 'Pending'
	END AS [GroupMemberStatus]
	, CASE
	WHEN r.GroupMemberId IS NULL THEN
		'<span style="color: Green;"><i class="fal fa-check-square"></i></span>'
	ELSE
		'<a href="/GroupMember/' + CAST(r.GroupMemberId AS NVARCHAR) + '" target="_blank"><span style="color: Red;"><i class="fal fa-times-square"></i></span></a>'
	END AS [MeetsRequirements]
	,r.GroupMemberId
FROM dbo.[Group] g
JOIN dbo.[GroupType] gt ON gt.Id = g.GroupTypeId AND gt.AttendanceRule = 2
JOIN dbo.[GroupMember] gm ON gm.GroupId = g.Id AND gm.GroupMemberStatus <> 0 AND gm.IsArchived = 0
JOIN dbo.[PersonAlias] pa ON pa.PersonId = gm.PersonId AND pa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER)
LEFT JOIN dbo.[Campus] c ON c.Id = g.CampusId
LEFT JOIN (
	SELECT gmr.GroupMemberId
	FROM [GroupMemberRequirement] gmr
	WHERE gmr.RequirementMetDateTime IS NULL
	GROUP BY gmr.GroupMemberId
) r ON r.GroupMemberId = gm.Id
WHERE g.IsActive = 1;

-- PS Class Capacity
IF @Campus != ''
WITH attendance AS
(
	SELECT glall.Id AS [GroupLocationId], COUNT(*) AS [Count]
	FROM dbo.[GroupLocation] glall
	JOIN dbo.[AttendanceOccurrence] ao ON ao.GroupId = glall.GroupId AND ao.OccurrenceDate = CONVERT(DATE, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'Central Standard Time' AS DATETIME))
	JOIN dbo.[Attendance] a ON a.OccurrenceId = ao.Id AND a.DidAttend = 1 AND a.EndDateTime IS NULL
	GROUP BY glall.Id
)
SELECT g.Name, gs.StartDate, gs.EndDate, gs.SoftRoomThreshold, gs.FirmRoomThreshold, gs.[Open], ISNULL(gs.[Count],0) AS [Count], s.Id AS [Calendar]
FROM dbo.[Person] p
JOIN dbo.[PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@Person AS UNIQUEIDENTIFIER)
JOIN (
	SELECT g.Id, gl.Id AS [LocationId], gls.ScheduleId, l.SoftRoomThreshold, l.FirmRoomThreshold, l.IsActive AS [Open],
		( SELECT attendance.[Count] FROM attendance WHERE attendance.GroupLocationId = gl.Id ) AS [Count],
		CONVERT(DATE, LEFT(av.Value,19)) AS [StartDate], CONVERT(DATE, RIGHT(av.Value,27)) AS [EndDate]
	FROM dbo.[Group] g
	JOIN dbo.[GroupLocation] gl ON gl.GroupId = g.Id
	JOIN dbo.[Location] l ON l.Id = gl.LocationId
	JOIN dbo.[GroupLocationSchedule] gls ON gls.GroupLocationId = gl.Id
	JOIN dbo.[AttributeValue] av ON av.EntityId = g.Id AND av.AttributeId = 34041
	JOIN dbo.[Campus] c ON c.Id = g.CampusId AND c.Guid = TRY_CAST(@Campus AS UNIQUEIDENTIFIER)
	WHERE g.IsActive = 1
		AND g.GroupTypeId IN (416, 629)
) AS gs ON p.BirthDate >= gs.StartDate AND p.BirthDate < gs.EndDate
JOIN dbo.[Group] g ON g.Id = gs.Id
JOIN dbo.[Schedule] s ON s.Id = gs.ScheduleId AND s.IsActive = 1;

-- Serving Teams
IF @Campus != ''
Begin
SELECT
	g.[Name] as [GroupName],
	gtr.[Name] AS [GroupRole],
	CASE WHEN gmr.[RequirementMetDateTime] IS NULL THEN 0 ELSE 1 END AS [GroupRequirementMet],
	grt.[Name] as [Requirement],
	CASE WHEN gmr.[RequirementMetDateTime] IS NULL THEN grt.[NegativeLabel] ELSE grt.[PositiveLabel] END AS [GroupRequirementStatus]
FROM [GroupMember] gm
JOIN [PersonAlias] pa ON pa.[PersonId] = gm.[PersonId]
JOIN [Person] p ON p.[Id] = pa.[PersonId]
JOIN [Group] g ON g.[Id] = gm.[GroupId]
JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
JOIN [GroupMemberRequirement] gmr ON gmr.[GroupMemberId] = gm.[Id]
JOIN [GroupRequirement] gr ON gr.[Id] = gmr.[GroupRequirementId]
JOIN [GroupRequirementType] grt ON grt.[Id] = gr.[GroupRequirementTypeId]
WHERE pa.[Guid] = @Person
	AND gm.[GroupMemberStatus] != 0 AND gm.[IsArchived] = 0
	AND g.[IsActive] = 1 AND g.[IsArchived] = 0
	AND (gr.[GroupId] = g.[Id] OR gr.[GroupTypeId] = g.[GroupTypeId])
	AND (gr.[GroupRoleId] IS NULL OR gr.[GroupRoleId] = gm.[GroupRoleId])
	AND (gr.[AppliesToAgeClassification] IS NULL OR gr.[AppliesToAgeClassification] = 0 OR gr.[AppliesToAgeClassification] = p.[AgeClassification])
ORDER BY gm.[GroupId], grt.[Id]
End
GO
