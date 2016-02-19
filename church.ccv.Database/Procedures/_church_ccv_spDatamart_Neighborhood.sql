/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spDatamart_Neighborhood
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spDatamart_Neighborhood]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[_church_ccv_spDatamart_Neighborhood]
AS
BEGIN
    DECLARE @NeighborhoodGroupTypeGuid UNIQUEIDENTIFIER = 'C3A3EB51-53CA-4EC1-B9B4-BB62E0C61445'
    DECLARE @NeighborhoodPastorRoleId INT = 45
    DECLARE @NeighborhoodLeaderRoleId INT = 46

    SET NOCOUNT ON;

    TRUNCATE TABLE _church_ccv_Datamart_Neighborhood

    INSERT INTO _church_ccv_Datamart_Neighborhood (
        [NeighborhoodId]
        ,[NeighborhoodName]
        ,[Guid]
        ,[GroupCount]
        ,[NeighborhoodLeaderName]
        ,[NeighborhoodLeaderId]
        ,[NeighborhoodPastorName]
        ,[NeighborhoodPastorId]
        ,[HouseholdCount]
        ,[AdultCount]
        ,[AdultsInGroups]
        ,[AdultsBaptized]
        ,[AdultsTakenStartingPoint]
        ,[AdultsServing]
        ,[AdultMemberCount]
        ,[AdultMembersInGroups]
        ,[AdultAttendeeCount]
        ,[AdultAttendeesInGroups]
        ,[AdultVisitors]
        ,[AdultParticipants]
        ,[ChildrenCount]
        )
    SELECT g.[Id] AS [NeighborhoodId]
        ,g.[Name] AS [NeighborhoodName]
        ,g.[Guid]
        ,(
            SELECT COUNT(*)
            FROM [Group] gc
            WHERE gc.[ParentGroupId] = g.[Id]
            ) AS [GroupCount]
        ,(
            SELECT TOP 1 p.[NickName] + ' ' + p.[LastName]
            FROM [Person] p
            INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
            INNER JOIN [Group] lg ON lg.[Id] = gm.[GroupId]
            WHERE lg.[Id] = g.[Id]
                AND gm.[GroupRoleId] = @NeighborhoodLeaderRoleId
            ) AS [NeighborhoodLeaderName]
        ,(
            SELECT TOP 1 p.[Id]
            FROM [Person] p
            INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
            INNER JOIN [Group] lg ON lg.[Id] = gm.[GroupId]
            WHERE lg.[Id] = g.[Id]
                AND gm.[GroupRoleId] = @NeighborhoodLeaderRoleId
            ) AS [NeighborhoodLeaderId]
        ,(
            SELECT TOP 1 p.[NickName] + ' ' + p.[LastName]
            FROM [Person] p
            INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
            INNER JOIN [Group] lg ON lg.[Id] = gm.[GroupId]
            WHERE lg.[Id] = g.[Id]
                AND gm.[GroupRoleId] = @NeighborhoodPastorRoleId
            ) AS [NeighborhoodPastorName]
        ,(
            SELECT TOP 1 p.[Id]
            FROM [Person] p
            INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
            INNER JOIN [Group] lg ON lg.[Id] = gm.[GroupId]
            WHERE lg.[Id] = g.[Id]
                AND gm.[GroupRoleId] = @NeighborhoodPastorRoleId
            ) AS [NeighborhoodPastorId]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Family] df
                WHERE df.[NeighborhoodId] = g.[Id]
                ), 0) AS [HouseholdCount]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                ), 0) AS [AdultCount]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [InNeighborhoodGroup] = 1
                ), 0) AS [AdultsInGroups]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [IsBaptized] = 1
                ), 0) AS [AdultsBaptized]
 ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [TakenStartingPoint] = 1
                ), 0) AS [AdultsTakenStartingPoint]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [IsServing] = 1
                ), 0) AS [AdultsServing]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Member'
                ), 0) AS [AdultMemberCount]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Member'
                    AND [InNeighborhoodGroup] = 1
                ), 0) AS [AdultMembersInGroups]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Attendee'
                ), 0) AS [AdultAttendeeCount]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Attendee'
                    AND [InNeighborhoodGroup] = 1
                ), 0) AS [AdultAttendeesInGroups]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Visitor'
                ), 0) AS [AdultVisitors]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Adult'
                    AND [ConnectionStatus] = 'Participant'
                ), 0) AS [AdultParticipants]
        ,COALESCE((
                SELECT COUNT(*)
                FROM [_church_ccv_Datamart_Person] dp
                WHERE dp.[NeighborhoodId] = g.[Id]
                    AND [FamilyRole] = 'Child'
                ), 0) AS [ChildrenCount]
    FROM [Group] g
    INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.[Id]
    WHERE gt.[Guid] = @NeighborhoodGroupTypeGuid
        AND g.[Id] <> 1201281 -- top-level parent group of neigborhood areas
		AND g.[Id] NOT IN (1201583,1201584) -- ZZ Out of range, ZZ Unknown
END