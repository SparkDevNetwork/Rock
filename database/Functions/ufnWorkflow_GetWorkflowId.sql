/*
    <doc>
	    <summary>
 		    This function returns a workflow id by combining the workflow type prefix and the workflow id number.
	    </summary>

	    <returns>
		    WorkflowId.
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnWorkflow_GetWorkflowId]( 1, 12 )
	    </code>
    </doc>
    */

    ALTER FUNCTION [dbo].[ufnWorkflow_GetWorkflowId]( @WorkflowTypeId int, @WorkflowIdNumber int ) 

    RETURNS nvarchar(100) AS

    BEGIN
	
	    DECLARE @Prefix nvarchar(100) = ( SELECT TOP 1 [WorkflowIdPrefix] FROM [WorkflowType] WHERE [Id] = @WorkflowTypeId )
	    RETURN COALESCE( @Prefix + RIGHT( '00000' + CAST( @WorkflowIdNumber AS varchar(5) ), 5 ), '' )

    END