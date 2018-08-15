using System.Collections.Generic;
using System.Data;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

using Quartz;
using Rock;

namespace org.newpointe.SqlToWorkflow
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [CodeEditorField( "SQL Query", "SQL query to run", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, true, "SELECT TOP 3 FirstName AS Person FROM [Person] ORDER BY Id", "General", 0, "SQLQuery" )]
    [WorkflowTypeField( "Workflow", "The Workflow to launch", false, true, "", "General", 1, "WGuid" )]
    [DisallowConcurrentExecution]
    public class SQLToWorkflows : IJob
    {

        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );
            WorkflowType workflowType = new WorkflowTypeService( rockContext ).Get( dataMap.GetString( "WGuid" ).AsGuid() );

            DataSet data = DbService.GetDataSet( dataMap.GetString( "SQLQuery" ), CommandType.Text, new Dictionary<string, object>() );

            foreach ( DataTable tbl in data.Tables )
            {
                foreach ( DataRow row in tbl.Rows )
                {
                    Workflow workflow = Workflow.Activate( workflowType, "New " + workflowType.WorkTerm );

                    foreach ( DataColumn col in tbl.Columns )
                    {
                        workflow.SetAttributeValue( col.ColumnName, row[col].ToString() );
                    }

                    if ( workflowService.Process( workflow, out List<string> errorMessages ) )
                    {
                        // If the workflow type is persisted, save the workflow
                        if ( workflow.IsPersisted || workflowType.IsPersisted )
                        {
                            if ( workflow.Id == 0 )
                            {
                                workflowService.Add( workflow );
                            }

                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                workflow.SaveAttributeValues( rockContext );
                                foreach ( WorkflowActivity activity in workflow.Activities )
                                {
                                    activity.SaveAttributeValues( rockContext );
                                }
                            } );

                        }
                    }
                }
            }
        }
    }
}
