using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// The base class to be inherited by all the PostUpdateJobs in Rock.
    /// </summary>
    public abstract class PostUpdateJob : RockJob
    {
        private static class AttributeKey
        {
            public const string SqlCommandTimeOut = "SqlCommandTimeOut";
        }

        /// <summary>
        /// Saves the attribute the post update job along with the value to the database so that it is persisted across restarts.
        /// </summary>
        /// <param name="key">The key of the attribute to be saved</param>
        /// <param name="value">The value of the attribute to be saved</param>
        internal void SetAttributeValue( string key, string value )
        {
            ServiceJob.SetAttributeValue( key, value );
            ServiceJob.SaveAttributeValues();
        }

        /// <summary>
        /// The method used by the Post Update Jobs which are required to run in batches
        /// This method helps to resume the job across restarts and continue more or less from where it last left off.
        /// Please ensure the SQL used in this method is idempotent, that is they may be run mulitple times over the same data and have the same effect everytime.
        /// This is because the job, after a restart, may resume from an record which has already been processed.
        /// </summary>
        /// <param name="sql">The Sql statement to execute on of the many batches of the job. Please ensure the parametes @StartId and @BatchSize are included in the query</param>
        /// <param name="attributeKey">The Attribute Key of the attribute to store the id from which the job needs to start or resume the execution.</param>
        /// <param name="lastId">The id of the record upto which the job needs to be executed. It is generally the id of the last record in the table.</param>
        /// <param name="defaultStartingId">The id after which the job needs to start the execution. By default it is set to 0</param>
        /// <param name="batchSize">The size of the batch on which the SQL query needs to execute in a single run. It is defaulted to 5000</param>
        internal void BulkUpdateRecords( string sql, string attributeKey, int lastId, int defaultStartingId = 0, int batchSize = 5000 )
        {
            int startingId = Math.Max( GetAttributeValue( attributeKey ).ToIntSafe(), defaultStartingId );

            // get the configured timeout, or default to 240 minutes if it is blank
            var sqlCommandTimeOut = GetAttributeValue( AttributeKey.SqlCommandTimeOut ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( sqlCommandTimeOut );

            Stopwatch stopwatch = Stopwatch.StartNew();
            for ( int currentId = startingId; currentId < lastId; currentId += batchSize )
            {
                // run the sql for the batch
                jobMigration.Sql( sql, new Dictionary<string, object>
                {
                    ["StartId"] = currentId,
                    ["BatchSize"] = batchSize
                } );

                // save last run id to the job attribute every 30 seconds.
                if ( stopwatch.ElapsedMilliseconds >= 30000 )
                {
                    SetAttributeValue( attributeKey, currentId.ToString() );
                    stopwatch.Restart();
                }
            }
        }
    }
}
