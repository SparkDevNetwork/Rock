/*
<doc>
       <summary>
             This function returns the location path for a given location id.
       </summary>

       <returns>
              * Grade
       </returns>
       <param name="LocationId" datatype="int">The location id</param>
       <remarks>     
       </remarks>
       <code>
              EXEC [dbo].[_church_ccv_ufnLocationPath] 25
       </code>
</doc>
*/

ALTER FUNCTION [dbo].[_church_ccv_ufnLocationPath]
(
	@LocationId INT
)
RETURNS varchar(2000)
AS
BEGIN

	DECLARE @Path varchar(2000)

    ;WITH CTE AS (
        SELECT 
			[Id],
			[ParentLocationId],
			[Name],
			0 AS [Order]
		FROM [Location] 
		WHERE [Id] = @LocationId

        UNION ALL

        SELECT 
			PL.[Id],
			PL.[ParentLocationId],
			PL.[Name],
			CTE.[Order] + 1 
		FROM [Location] PL
        INNER JOIN CTE ON CTE.[ParentLocationId] = PL.[Id]
    )

	SELECT @Path = [Name] + COALESCE( ' > ' + @Path, '' ) 
	FROM CTE
	ORDER BY [Order]

	RETURN @Path

END