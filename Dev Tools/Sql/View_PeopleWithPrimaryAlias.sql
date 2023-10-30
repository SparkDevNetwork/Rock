
/********************************************************************************************************************
 View People with their Primary Alias
*********************************************************************************************************************/

/**
 * Compared using LIKE.
 * Leave blank to show all people.
 */
DECLARE @LastName [nvarchar](50) = 'decker';

/***************************
 DON'T EDIT ANYTHING BELOW.
****************************/

---------------------------------------------------------------------------------------------------

SELECT 'Person' AS [Entity]
    , p.[Id] AS [PersonId]
    , p.[Guid] AS [PersonGuid]
    , pa.[Id] AS [PrimaryAliasId]
    , pa.[Guid] AS [PrimaryAliasGuid]
    , CONCAT(p.[NickName], ' ', p.[LastName]) AS 'Name'
    , CASE
        WHEN p.[AgeClassification] = 0 THEN 'Unknown'
        WHEN p.[AgeClassification] = 1 THEN 'Adult'
        WHEN p.[AgeClassification] = 2 THEN 'Child'
      END AS [AgeClassification]
    , CASE
        WHEN p.[Gender] = 0 THEN 'Unknown'
        WHEN p.[Gender] = 1 THEN 'Male'
        WHEN p.[Gender] = 2 THEN 'Female'
      END AS [Gender]
    , p.[BirthDate]
FROM [Person] p
LEFT OUTER JOIN [PersonAlias] pa
    ON pa.[PersonId] = p.[Id] AND pa.[AliasPersonId] = p.[Id]
WHERE (@LastName IS NULL OR @LastName = '' OR p.[LastName] LIKE '%' + @LastName + '%')
ORDER BY p.[PrimaryFamilyId]
    , p.[AgeClassification]
    , p.[Gender]
    , p.[Id];
