/*
<doc>
	<summary>
 		This function returns the emails of any parents in any family for the selected person id
	</summary>

	<returns>
		The parent emails
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetParentEmails]( 3 )
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetParentEmails](@PersonId int) 
RETURNS VARCHAR(1000) 

AS

BEGIN

	DECLARE @Emails VARCHAR(1000) =
	(
		SUBSTRING((
			SELECT ', ' + P.[Email] 
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
			AND P.[Email] IS NOT NULL
			AND P.[Email] <> ''
			ORDER BY P.[LastName], P.[NickName]
			FOR XML PATH('')
		), 2, 1000000 ) 
	)

	RETURN @Emails
END