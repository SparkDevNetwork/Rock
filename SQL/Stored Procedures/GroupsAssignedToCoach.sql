CREATE OR ALTER PROCEDURE dbo._org_lakepointe_sp_GroupsAssignedToCoach @PersonAliasGuid NVARCHAR(40)
AS
BEGIN
   -- Identify current user
    DECLARE @PersonId INT;
    SELECT @PersonId = p.Id
    FROM [Person] p
    JOIN [PersonAlias] pa ON pa.PersonId = p.Id AND pa.Guid = TRY_CAST(@PersonAliasGuid AS UNIQUEIDENTIFIER);

    SELECT g.Id, g.Name
    FROM [Group] g
    JOIN [AttributeValue] coachav ON coachav.EntityId = g.Id
    JOIN [Attribute] coacha ON coacha.Id = coachav.AttributeId AND coacha.[Key] = 'AGCoach' AND coacha.EntityTypeId = 16
    JOIN [PersonAlias] coach ON coach.Guid = TRY_CAST(coachav.Value AS UNIQUEIDENTIFIER) AND coach.PersonId = @PersonId
    WHERE g.IsActive = 1 AND g.IsArchived = 0 -- Added SNS 20221116 per https://helpdesk.lakepointe.org/app/itdesk/ui/requests/63677000019976129/details
END;
GO