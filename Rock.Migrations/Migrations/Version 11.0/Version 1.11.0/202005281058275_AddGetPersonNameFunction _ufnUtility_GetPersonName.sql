/*
<doc>
	<summary>
 		This admin helper function returns the person name (and Id) for the given person alias.
	</summary>

	<returns>
		The name and person Id of the given person alias id.
	</returns>

	<remarks>
 		NOTE: This is NOT FOR USE in Rock code. Only use for troubleshooting.
	</remarks>
	<code>
		SELECT [dbo].[ufnUtility_GetPersonName](3)
		SELECT Content, [dbo].[ufnUtility_GetPersonName(ModifiedByPersonAliasId) FROM [HtmlContent]
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnUtility_GetPersonName](
	@PersonAliasId int
	)

RETURNS nvarchar(500) AS

BEGIN

	RETURN ( SELECT FirstName + ' ' + LastName + ' (' + CONVERT( varchar, P.Id ) + ')' FROM [Person] P 
		INNER JOIN [PersonAlias] PA ON PA.PersonId = P.Id
		WHERE PA.Id = @PersonAliasId )
END