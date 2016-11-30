ALTER FUNCTION [dbo].[_church_ccv_GetSaturdayDate](@TimeStamp datetime)
	RETURNS datetime

AS 

BEGIN

	DECLARE @Saturday datetime
	DECLARE @Day int 
	
	SET @Day = DATEPART(dw, @TimeStamp)
	IF @Day = 1 SET @Saturday = DATEADD(d, -1, @TimeStamp)
	IF @Day = 2 SET @Saturday = DATEADD(d, -2, @TimeStamp)
	IF @Day = 3 SET @Saturday = DATEADD(d, -3, @TimeStamp)
	IF @Day = 4 SET @Saturday = DATEADD(d, -4, @TimeStamp)
	IF @Day = 5 SET @Saturday = DATEADD(d, -5, @TimeStamp)
	IF @Day = 6 SET @Saturday = DATEADD(d, -6, @TimeStamp)
	IF @Day = 7 SET @Saturday = @TimeStamp

	SET @Saturday = CAST(MONTH(@Saturday) AS varchar) + '/' +
		CAST(DAY(@Saturday) AS varchar) + '/' +
		CAST(YEAR(@Saturday) AS varchar)

	RETURN @Saturday

END