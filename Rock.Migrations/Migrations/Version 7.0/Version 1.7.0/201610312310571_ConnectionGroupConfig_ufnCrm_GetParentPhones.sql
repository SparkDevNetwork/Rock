IF object_id('[dbo].[ufnCrm_GetParentPhones]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetParentPhones]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function returns the Phones of any parents in any family for the selected person id
	</summary>

	<returns>
		The parent Phone numbers
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetParentPhones]( 3, 'Mobile' )
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetParentPhones](@PersonId int, @PhoneType varchar(20)) 
RETURNS VARCHAR(1000) 

AS

BEGIN

	DECLARE @Phones VARCHAR(1000) =
	(
		SUBSTRING((
			SELECT ', ' + P.[NumberFormatted] 
			FROM [GroupMember] M
			INNER JOIN [PhoneNumber] P 
				ON P.[PersonId] = M.[PersonId]
			INNER JOIN [DefinedValue] T
				ON T.[Id] = P.[NumberTypeValueId] 
				AND T.[Value] = @PhoneType
			WHERE M.[GroupId] IN 
			(
				SELECT [GroupId] 
				FROM [GroupMember] GM
				INNER JOIN [Group] G ON G.[Id] = GM.[GroupId] 
				WHERE [PersonId] = @PersonId 
				AND G.[GroupTypeId] = 10 
			)
			AND M.[GroupRoleId] = 3
			AND P.[Number] IS NOT NULL
			AND P.[Number] <> ''
			FOR XML PATH('')
		), 2, 1000000 ) 
	)

	RETURN @Phones
END