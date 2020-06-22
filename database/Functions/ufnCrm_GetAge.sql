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

ALTER FUNCTION [dbo].[ufnCrm_GetAge](@BirthDate date) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Age INT
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = GETDATE()

	-- Year 0001 is a special year, which denotes no year selected therefore we shouldn't calculate the age.
	IF @BirthDate IS NOT NULL AND DATEPART( year, @BirthDate ) > 1
	BEGIN

		SET @Age = DATEPART( year, @CurrentDate ) - DATEPART( year, @BirthDate )
		IF @BirthDate > DATEADD( year, 0 - @Age, @CurrentDate )
		BEGIN
			SET @Age = @Age - 1
		END

	END
		
	RETURN @Age

END