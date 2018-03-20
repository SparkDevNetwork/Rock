IF object_id('[dbo].[ufnCrm_GetGradeOffset]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetGradeOffset]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function returns the grade offset given a graduation date. The offset can then be used with defined value to find grade
	</summary>

	<returns>
		The grade offset based on graduation date
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetGradeOffset]( 2018, null )
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetGradeOffset](@GraduationYear int, @TransitionDate datetime ) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Offset INT

	IF @GraduationYear IS NOT NULL
	BEGIN

		DECLARE @today DATETIME = DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)
		DECLARE @year INT = DATEPART(year, @today)

		IF @TransitionDate IS NULL
		BEGIN
			SET @TransitionDate = CAST(CAST(@year AS varchar) + '-06-01' AS DATETIME)
		END
		ELSE
		BEGIN
			SET @TransitionDate = CAST(CAST(@year AS varchar) + '-' + CAST(DATEPART(month,@TransitionDate) AS varchar) + '-' + CAST(DATEPART(day,@TransitionDate) AS varchar) AS DATETIME)
		END

		IF @today >= @TransitionDate
		BEGIN
			SET @year = @year + 1
		END

		SET @Offset = @GraduationYear - @year

	END
		
	RETURN @Offset

END