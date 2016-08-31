/*
    <doc>
	    <summary>
 		    This function returns all group types that are used to denote 
		    groups that are for tracking attendance for weekly services
	    </summary>

	    <returns>
		    * GroupTypeId
		    * Guid
		    * Name
	    </returns>

	    <code>
		    SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
	    </code>
    </doc>
    */


    ALTER FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
    RETURNS TABLE AS

    RETURN ( 
	    SELECT [Id], [Guid], [Name]
	    FROM [GroupType] 
	    WHERE [AttendanceCountsAsWeekendService] = 1
    )