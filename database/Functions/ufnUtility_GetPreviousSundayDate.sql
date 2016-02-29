-- create function for attendance duration
                    /*
                    <doc>
	                    <summary>
 		                    This function returns the date of the previous Sunday.
	                    </summary>

	                    <returns>
		                    Datetime of the previous Sunday.
	                    </returns>
	                    <remarks>
		
	                    </remarks>
	                    <code>
		                    SELECT [dbo].[ufnUtility_GetPreviousSundayDate]()
	                    </code>
                    </doc>
                    */

                    ALTER FUNCTION [dbo].[ufnUtility_GetPreviousSundayDate]() 

                    RETURNS date AS

                    BEGIN

	                    RETURN DATEADD("day", -7, dbo.ufnUtility_GetSundayDate(getdate()))
                    END