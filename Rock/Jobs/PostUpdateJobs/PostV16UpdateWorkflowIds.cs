using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16 to update WorkflowId columns after switch from computed Column.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.0 - Update WorkflowId column." )]
    [Description( "This job updates all WorkflowId values on the Workflow table using the format specified in 'ufnWorkflow_GetWorkflowId'." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV16UpdateWorkflowIds : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
            public const string StartAtId = "StartAtId";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var lastId = new WorkflowService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .Select( i => ( int? ) i.Id )
                .Max();

            if ( lastId.HasValue )
            {

                string sqlFormat = @"UPDATE Workflow
        SET WorkflowId = COALESCE( WFT.[WorkflowIdPrefix] + RIGHT( '00000' + CAST( WF.[WorkflowIdNumber] AS varchar(5) ), 5 ), '' )
		FROM WorkflowType WFT
		LEFT JOIN Workflow WF ON WFT.Id = WF.WorkflowTypeId
		WHERE (WFT.Id > @StartId
		AND WFT.Id <= @StartId + @BatchSize)";

                BulkUpdateRecords( sqlFormat, AttributeKey.StartAtId, lastId.Value );
            }

            DeleteJob();
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
