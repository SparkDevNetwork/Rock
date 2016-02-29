/*
    <doc>
	    <summary>
 		    This function returns the person id for the person alias given.
	    </summary>

	    <returns>
		    Int of the person id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPersonIdFromPersonAlias](1)
	    </code>
    </doc>
    */

    ALTER FUNCTION [dbo].[ufnUtility_GetPersonIdFromPersonAlias](@PersonAlias int) 

    RETURNS int AS

    BEGIN

	    RETURN (SELECT [PersonId] FROM PersonAlias WHERE [Id] = @PersonAlias)
    END