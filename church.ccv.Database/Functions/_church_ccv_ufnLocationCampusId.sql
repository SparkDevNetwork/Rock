/*
<doc>
       <summary>
             This function returns the campus id for a given location id.
       </summary>

       <returns>
              * Campus ID
       </returns>
       <param name="LocationId" datatype="int">The location id</param>
       <remarks>     
       </remarks>
       <code>
              EXEC [dbo].[_church_ccv_ufnLocationCampusId] 25
       </code>
</doc>
*/

ALTER FUNCTION [dbo].[_church_ccv_ufnLocationCampusId]
(
	@LocationId INT
)
RETURNS int
AS
BEGIN

	DECLARE @CampusId int

    ;WITH CTE AS (
        SELECT 
			[Id],
			[ParentLocationId],
			0 AS [Order]
		FROM [Location] 
		WHERE [Id] = @LocationId

        UNION ALL

        SELECT 
			PL.[Id],
			PL.[ParentLocationId],
			CTE.[Order] + 1 
		FROM [Location] PL
        INNER JOIN CTE ON CTE.[ParentLocationId] = PL.[Id]
    )

	SELECT @CampusId = ( 
		SELECT TOP 1 C.[Id]
		FROM CTE 
		INNER JOIN [Campus] C
			ON C.[LocationId] = CTE.[Id]
		ORDER BY CTE.[Order] DESC
	)

	RETURN @CampusId

END