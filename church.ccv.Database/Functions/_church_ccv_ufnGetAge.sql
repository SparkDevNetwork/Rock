/*
<doc>
       <summary>
             This function returns a person's age based on their birthdate.
       </summary>

       <returns>
              * Age
       </returns>
       <param name="Birthdate" datatype="int">The birthdate of the person</param>
       <remarks>     
       </remarks>
       <code>
              EXEC [dbo].[_church_ccv_ufnGetAge] '1/1/2008'
       </code>
</doc>
*/

ALTER FUNCTION [dbo].[_church_ccv_ufnGetAge]
(
	-- Add the parameters for the function here
	@Birthdate DATETIME
)
RETURNS INT
AS
BEGIN
	
	DECLARE @Result INT

	SET @Result = CONVERT(int,ROUND(DATEDIFF(hour, @Birthdate, GETDATE())/8766.0,0))
	
	RETURN @Result

END