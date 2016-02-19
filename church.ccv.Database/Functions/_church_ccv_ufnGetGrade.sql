/*
<doc>
       <summary>
             This function returns a person's grade based on their graduation year.
       </summary>

       <returns>
              * Grade
       </returns>
       <param name="GraduationYear" datatype="int">The graduation year of the person</param>
       <remarks>     
       </remarks>
       <code>
              EXEC [dbo].[_church_ccv_ufnGetGrade] 2023
       </code>
</doc>
*/

ALTER FUNCTION [dbo].[_church_ccv_ufnGetGrade]
(
	@GraduationYear INT
)
RETURNS INT
AS
BEGIN

	DECLARE @Grade INT
	DECLARE @GradeTransitionAttribute VARCHAR(20) = (SELECT AV.Value 
											FROM AttributeValue AV 
											WHERE AV.AttributeId = 498)

	DECLARE @GraduationDate DATE = CONVERT(DATE, @GradeTransitionAttribute + '/' + CONVERT(VARCHAR, @GraduationYear))

	DECLARE @Years INT = 0,
		@Months INT = 0,
		@Days INT = 0	
	
	SET @Days = DAY(@GraduationDate) - DAY(GETDATE())
	IF (@Days <= 0)
		SET @Months = @Months - 1
	
	SET @Months = @Months + MONTH(@GraduationDate) - MONTH(GETDATE())
	IF (@Months < 0)
		SET @Years = @Years - 1
	
	SET @Years = @Years + YEAR(@GraduationDate) - YEAR(GETDATE())
	IF (@Years > 12 or @Years < 0)
		SET @Grade = NULL
	ELSE
		SET @Grade = (12 - @Years)
	
	RETURN @Grade 

END