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
    DECLARE @BibleStrictSpouse BIT = 1;
    DECLARE @BibleStrictSpouseValue NVARCHAR(MAX) = (
        SELECT TOP (1) [DefaultValue]
        FROM [Attribute]
        WHERE [Key] = 'core_BibleStrictSpouse'
            AND [EntityTypeId] IS NULL
            AND [EntityTypeQualifierColumn] = 'SystemSetting'
            AND ([EntityTypeQualifierValue] IS NULL OR [EntityTypeQualifierValue] = '')
    );

    /* Match AsBoolean(true): whitespace defaults to true, and only unpadded true tokens are true. */
    DECLARE @WhitespaceCharacters NVARCHAR(25) =
        NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(32) + NCHAR(133) + NCHAR(160) + NCHAR(5760)
        + NCHAR(8192) + NCHAR(8193) + NCHAR(8194) + NCHAR(8195) + NCHAR(8196) + NCHAR(8197) + NCHAR(8198)
        + NCHAR(8199) + NCHAR(8200) + NCHAR(8201) + NCHAR(8202) + NCHAR(8232) + NCHAR(8233) + NCHAR(8239) + NCHAR(8287) + NCHAR(12288);

    IF @BibleStrictSpouseValue COLLATE Latin1_General_100_BIN2 LIKE N'%[^' + @WhitespaceCharacters + N']%'
    BEGIN
        SET @BibleStrictSpouse = CASE
            WHEN LOWER(@BibleStrictSpouseValue) COLLATE Latin1_General_100_BIN2 IN (N'true', N'yes', N't', N'y', N'1')
                AND DATALENGTH(@BibleStrictSpouseValue) = DATALENGTH(RTRIM(@BibleStrictSpouseValue)) THEN 1
            ELSE 0
        END;
    END;
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

            AND( @BibleStrictSpouse = 0 OR P.[Gender] != S.[Gender] OR P.[Gender] = 0 OR S.[Gender] = 0 )-- Genders cannot match if both are known

        ORDER BY
            isnull(pgm.GroupOrder, 9999),
            ABS( DATEDIFF( DAY, ISNULL( P.[BirthDate], '1/1/0001' ), ISNULL( S.[BirthDate], '1/1/0001' ) ) )-- If multiple results, choose nearest in age
            , S.[Id]-- Sort by Id so that the same result is always returned
    )

END