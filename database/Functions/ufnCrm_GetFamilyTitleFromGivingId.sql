/*
<doc>
	<summary>
 		This function returns the household name from a giving id.
	</summary>

	<returns>
		String of household name. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetFamilyTitleFromGivingId]('G63') -- Decker's (married) Returns 'Ted & Cindy Decker'
		SELECT [dbo].[ufnCrm_GetFamilyTitleFromGivingId]('G64') -- Jones' (single) Returns 'Ben Jones'
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetFamilyTitleFromGivingId](@GivingId varchar(31) ) 

RETURNS nvarchar(250) AS
BEGIN
	DECLARE @UnitType char(1)
	DECLARE @UnitId int
	DECLARE @Result varchar(250)

	SET @UnitType = LEFT(@GivingId, 1)
	SET @UnitId = CAST(SUBSTRING(@GivingId, 2, LEN(@GivingId)) AS INT)

	IF @UnitType = 'P'
		SET @Result = (SELECT TOP 1 [NickName] + ' ' + [LastName] FROM [Person] WHERE [GivingId] = @GivingId)
	ELSE
		SET @Result = (SELECT * FROM dbo.ufnCrm_GetFamilyTitle(null, @UnitId, default, 1))

	RETURN @Result
END