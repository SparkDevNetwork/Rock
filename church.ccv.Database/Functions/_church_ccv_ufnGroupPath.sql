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

ALTER FUNCTION [dbo].[_church_ccv_ufnGroupPath]
(
	@GroupId INT
)
RETURNS varchar(2000)
AS
BEGIN

	DECLARE @Path varchar(2000)

    ;WITH CTE AS (
        SELECT 
			[Id],
			[ParentGroupId],
			[Name],
			0 AS [Order]
		FROM [Group] 
		WHERE [Id] = @GroupId

        UNION ALL

        SELECT 
			PG.[Id],
			PG.[ParentGroupId],
			PG.[Name],
			CTE.[Order] + 1 
		FROM [Group] PG
        INNER JOIN CTE ON CTE.[ParentGroupId] = PG.[Id]
    )

	SELECT @Path = [Name] + COALESCE( ' > ' + @Path, '' ) 
	FROM CTE
	ORDER BY [Order]

	RETURN @Path

END