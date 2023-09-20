// <copyright>
// Copyright Pillars Inc.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Jobs
{
    [DisplayName("SQL To Workflow")]
    [Description("Job that iterates through rows of an SQL Query and launches a workflow for each row, passing in the row's column values as attributes if the column name matches up to a workflow attribute key.")]

    [CodeEditorField("SQL Query", "SQL query to run. Each row of the query will launch a workflow. Ensure column names match up to desired attribute keys in the workflow.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, false, "", "", 2, "SQLQuery")]
    [IntegerField("Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL query to complete. Leave blank to use the default (30 seconds).", false, 180, "", 4, "CommandTimeout")]
    [WorkflowTypeField("Workflow", "The workflow to be fired for each row.", required: true, key: AttributeKey.WORKFLOW)]

    [DisallowConcurrentExecution]
    public class SQLToWorkflow : IJob
    {
        private static class AttributeKey
        {
            public const string SQL_QUERY = "SQLQuery";
            public const string COMMAND_TIMEOUT = "CommandTimeout";
            public const string WORKFLOW = "Workflow";
        }

        private class WorkflowAttribute
        {
            public string key;
            public string value;

            public WorkflowAttribute(string k, string v)
            {
                key = k;
                value = v;
            }
        }

        public virtual void Execute(IJobExecutionContext context)
        {
            using (var rockContext = new RockContext())
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                int? commandTimeout = dataMap.GetString("CommandTimeout").AsIntegerOrNull();
                Guid? workflowTypeGuid = dataMap.GetString(AttributeKey.WORKFLOW).AsGuidOrNull();

                var personService = new PersonService(rockContext);

                List<Person> people = new List<Person>();

                // Get SQL Query and get list of people
                string query = dataMap.GetString("SQLQuery");
                if (query.IsNotNullOrWhiteSpace())
                {
                    DataSet dataSet = DbService.GetDataSet(query, System.Data.CommandType.Text, null, commandTimeout);

                    if (dataSet.Tables.Count == 0) //make sure a dataset was actually returned from the SQL query
                    {
                        throw new Exception("SQL Query did not return a result set.");
                    }

                    DataTable table = dataSet.Tables[0];
                    List<Object> rows = new List<object>();

                    //iterates through rows and creates key value pairs to pass into the workflow as attributes later
                    foreach (DataRow row in table.Rows)
                    {
                        List<WorkflowAttribute> workflowAttributes = new List<WorkflowAttribute>();
                        foreach (DataColumn col in table.Columns)
                        {
                            workflowAttributes.Add(new WorkflowAttribute(col.ColumnName, row[col.ColumnName].ToString()));
                        }

                        LaunchWorkflow(workflowTypeGuid.Value, workflowAttributes);
                    }
                }
            }
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        private void LaunchWorkflow(Guid workflowTypeGuid, List<WorkflowAttribute> workflowAttributes)
        {
            // only launch workflow if at least one attribute is going to be passed in
            if (workflowAttributes.Count > 0)
            {
                using (var rockContext = new RockContext())
                {
                    var workflowType = WorkflowTypeCache.Get(workflowTypeGuid);
                    if (workflowType != null && (workflowType.IsActive ?? true))
                    {
                        var workflowService = new WorkflowService(rockContext);
                        var workflow = Rock.Model.Workflow.Activate(workflowType, string.Empty, rockContext);


                        //for each column in the SQL row, attempt to set that as an attribute on the workflow.
                        foreach(WorkflowAttribute attribute in workflowAttributes)
                        {
                            if(attribute.value != null) //ensures that the SQL query returned an actual value in this cell.
                            {
                                workflow.SetAttributeValue(attribute.key, attribute.value);
                            }
                        }

                        List<string> workflowErrorMessages = new List<string>();
                        new Rock.Model.WorkflowService(rockContext).Process(workflow, out workflowErrorMessages);
                    }
                }
            }
        }
    }
}
