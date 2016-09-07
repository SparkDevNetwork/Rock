// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class WorkflowNumber : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            AddColumn("dbo.WorkflowType", "WorkflowIdPrefix", c => c.String(maxLength: 100));
            AddColumn("dbo.Workflow", "WorkflowIdNumber", c => c.Int(nullable: false));
            
            Sql( @"
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

    CREATE FUNCTION [dbo].[ufnWorkflow_GetWorkflowId]( @WorkflowTypeId int, @WorkflowIdNumber int ) 

    RETURNS nvarchar(100) AS

    BEGIN
	
	    DECLARE @Prefix nvarchar(100) = ( SELECT TOP 1 [WorkflowIdPrefix] FROM [WorkflowType] WHERE [Id] = @WorkflowTypeId )
	    RETURN COALESCE( @Prefix + RIGHT( '00000' + CAST( @WorkflowIdNumber AS varchar(5) ), 5 ), '' )

    END
" );

            Sql( @"
    ALTER TABLE [Workflow] ADD [WorkflowId] as ( [dbo].[ufnWorkflow_GetWorkflowId] ( [WorkflowTypeId], [WorkflowIdNumber] ) )
" );

            Sql( @"
    UPDATE [WorkflowType] SET [WorkflowIdPrefix] = 'IT' WHERE [Guid] = '51FE9641-FB8F-41BF-B09E-235900C3E53E'
    UPDATE [WorkflowType] SET [WorkflowIdPrefix] = 'BC' WHERE [Guid] = '16D12EF7-C546-4039-9036-B73D118EDC90'

    UPDATE W SET [WorkflowIdNumber] = R.[RowNumber]
    FROM [Workflow] W 
    INNER JOIN ( 
	    SELECT [Id], ROW_NUMBER() OVER ( PARTITION BY [WorkflowTypeId] ORDER BY [Id] ) AS [RowNumber]
	    FROM [Workflow] 
    ) AS R ON R.[Id] = W.[Id]
" );

}
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Workflow", "WorkflowId");
            DropColumn("dbo.Workflow", "WorkflowIdNumber");
            DropColumn("dbo.WorkflowType", "WorkflowIdPrefix");
        }
    }
}
