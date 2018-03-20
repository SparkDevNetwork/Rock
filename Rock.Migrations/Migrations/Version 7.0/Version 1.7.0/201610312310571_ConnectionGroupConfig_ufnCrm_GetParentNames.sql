IF object_id('[dbo].[ufnCrm_GetParentNames]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetParentNames]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function returns the names of any parents in any family for the selected person id
	</summary>

	<returns>
		The parent names
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetParentNames]( 3 )
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetParentNames](@PersonId int) 
RETURNS VARCHAR(1000) 

AS

BEGIN

	DECLARE @Names VARCHAR(1000) =
	(
		SUBSTRING((
			SELECT ', ' + P.[NickName] + ' ' + P.[LastName]
			FROM [GroupMember] M
			INNER JOIN [Person] P ON P.[Id] = M.[PersonId]
			WHERE M.[GroupId] IN 
			(
				SELECT [GroupId] 
				FROM [GroupMember] GM
				INNER JOIN [Group] G ON G.[Id] = GM.[GroupId] 
				WHERE [PersonId] = @PersonId 
				AND G.[GroupTypeId] = 10 
			)
			AND M.[GroupRoleId] = 3
			ORDER BY P.[LastName], P.[NickName]
			FOR XML PATH('')
		), 2, 1000000 ) 
	)

	RETURN @Names
END