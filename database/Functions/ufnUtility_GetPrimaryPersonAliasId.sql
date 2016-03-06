/*
    <doc>
	    <summary>
 		    This function returns the primary person alias id for the person id given.
	    </summary>

	    <returns>
		    Int of the primary person alias id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPrimaryPersonAliasId](1)
	    </code>
    </doc>
    */

    ALTER FUNCTION [dbo].[ufnUtility_GetPrimaryPersonAliasId](@PersonId int) 

    RETURNS int AS

    BEGIN

	    RETURN ( 
			SELECT TOP 1 [Id] FROM [PersonAlias]
			WHERE [PersonId] = @PersonId AND [AliasPersonId] = @PersonId
		)

    END