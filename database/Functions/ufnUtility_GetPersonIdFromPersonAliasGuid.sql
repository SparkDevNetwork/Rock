/*
    <doc>
	    <summary>
 		    This function returns the person id for the person alias guid given.
	    </summary>

	    <returns>
		    Int of the person id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]('58DF3F13-B96E-4682-A14F-D411187CEBBA')
	    </code>
    </doc>
    */

    ALTER FUNCTION [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid](@PersonAliasGuid uniqueidentifier) 

    RETURNS int AS

    BEGIN
	    RETURN (SELECT TOP 1 [PersonId] FROM PersonAlias WHERE [Guid] = @PersonAliasGuid)
    END