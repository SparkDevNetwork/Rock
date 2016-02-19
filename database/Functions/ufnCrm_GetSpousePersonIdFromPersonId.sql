/*
<doc>
	<summary>
 		This function returns the head of house for the giving id provided
	</summary>

	<returns>
		Person Id of the head of household. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](3) -- Ted Decker (married) 
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](7) -- Ben Jones (single)
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](@PersonId int ) 

RETURNS int AS
BEGIN
	
	RETURN (SELECT TOP 1 p.[Id] 
				FROM 
					[Person] p
					INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
					INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
					INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
					INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
				WHERE 
					gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
					AND gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- family
					AND g.[Id] IN (SELECT g2.[Id] 
									FROM [GroupMember] gm2
										INNER JOIN [GroupTypeRole] gtr2 ON gtr2.[Id] = gm2.[GroupRoleId]
										INNER JOIN [Group] g2 ON g.[Id] = gm2.[GroupId]
										INNER JOIN [GroupType] gt2 ON gt2.[Id] = g2.[GroupTypeId]
									WHERE gm2.[PersonId] = @PersonId
										AND gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
										AND gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- family
									)
		
					AND gm.[PersonId] != @PersonId
				ORDER BY p.[Gender])

END