using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v12.0 - Interaction Index Update" )]
    [Description( "This job will update the indexes on the interactions table." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV12DataMigrationsAddInteractionIndexes : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
            var migrationHelper = new MigrationHelper( new JobMigration( commandTimeout ) );

            migrationHelper.DropIndexIfExists( "Interaction", "IX_InteractionComponentId_InteractionDateTime" );

            migrationHelper.CreateIndexIfNotExists( "Interaction",
                new[] { "InteractionComponentId", "InteractionDateTime" },
                new[] { "InteractionTimeToServe", "Operation", "InteractionSessionId" } );

            ServiceJobService.DeleteJob( context.GetJobId() );
        }
    }
}
