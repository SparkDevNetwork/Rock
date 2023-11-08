/*
<doc>
    <summary>
        This function returns the most likely spouse for the person [Id] provided
    </summary>

    <returns>
        Person [Id] of the most likely spouse; otherwise returns NULL
    </returns>
    <remarks>


    </remarks>
    <code>
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](5) -- Ted Decker (married)
        SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](8) -- Ben Jones (single)
    </code>
</doc>
*/


ALTER FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId] (
    @PersonId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @BibleStrictSpouse INT = (SELECT TOP (1) [Id] FROM [Attribute] WHERE [Key]= 'core_BibleStrictSpouse')
    IF (@BibleStrictSpouse = NULL)
        SET @BibleStrictSpouse = 0
    RETURN (
        SELECT
            TOP 1 S.[Id]
        FROM
            [Group] F
            INNER JOIN [GroupType] GT ON F.[GroupTypeId] = GT.[Id]
            INNER JOIN [GroupMember] FM ON FM.[GroupId] = F.[Id]
            INNER JOIN [Person] P ON P.[Id] = FM.[PersonId]
            INNER JOIN [GroupTypeRole] R ON R.[Id] = FM.[GroupRoleId]
            INNER JOIN [GroupMember] FM2 ON FM2.[GroupID] = F.[Id]
            INNER JOIN [Person] S ON S.[Id] = FM2.[PersonId]
            INNER JOIN [GroupTypeRole] R2 ON R2.[Id] = FM2.[GroupRoleId]
            CROSS APPLY (SELECT top 1 GroupOrder FROM GroupMember where GroupId = f.Id and PersonId = @PersonId) pgm
        WHERE
            GT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
            AND P.[Id] = @PersonID
            AND R.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Person must be an Adult
            AND R2.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Potential spouse must be an Adult
            AND P.[MaritalStatusValueId] = 143 -- Person must be Married
            AND S.[MaritalStatusValueId] = 143 -- Potential spouse must be Married
            AND FM.[PersonId] != FM2.[PersonId] -- Cannot be married to yourself
            -- In the future, we may need to implement and check a GLOBAL Attribute "BibleStrict" with this logic:

            AND( @BibleStrictSpouse = 0 OR P.[Gender] != S.[Gender] OR P.[Gender] = 0 OR S.[Gender] = 0 )-- Genders cannot match if both are known

        ORDER BY
            isnull(pgm.GroupOrder, 9999),
            ABS( DATEDIFF( DAY, ISNULL( P.[BirthDate], '1/1/0001' ), ISNULL( S.[BirthDate], '1/1/0001' ) ) )-- If multiple results, choose nearest in age
            , S.[Id]-- Sort by Id so that the same result is always returned
    )

END