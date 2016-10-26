ALTER FUNCTION [dbo].[ufnGroup_GetAllDescendents] (@GroupId INT)
RETURNS TABLE
AS
RETURN (
        WITH CTE AS (
                SELECT *
                FROM [Group]
                WHERE [ParentGroupId] = @GroupId
                
                UNION ALL
                
                SELECT [a].*
                FROM [Group] [a]
                INNER JOIN CTE pcte ON pcte.Id = [a].[ParentGroupId]
                )
        SELECT *
        FROM CTE
        )