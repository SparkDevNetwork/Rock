/*
<doc>
	<summary>
 		This function returns the age given a birthdate.
	</summary>

	<returns>
		The age based on birthdate
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAge]( '2000-01-01')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAge](@BirthDate datetime) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Age INT

	IF @BirthDate IS NOT NULL
	BEGIN

		SET @Age = DATEPART( year, GETDATE() ) - DATEPART( year, @BirthDate )
		IF @BirthDate > DATEADD( year, 0 - @Age, GETDATE() )
		BEGIN
			SET @Age = @Age - 1
		END

	END
		
	RETURN @Age

END