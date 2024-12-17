ALTER VIEW [dbo].[GroupTypeInheritance]
AS
WITH cte AS
(
    SELECT
        [GT].[Id]
        , [GT].[InheritedGroupTypeId]
    FROM [GroupType] AS [GT]

    UNION ALL
    
    SELECT
        [cte].[Id]
        , [GT].[InheritedGroupTypeId]
    FROM [cte]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [cte].[InheritedGroupTypeId]
)
SELECT * FROM cte WHERE [InheritedGroupTypeId] IS NOT NULL