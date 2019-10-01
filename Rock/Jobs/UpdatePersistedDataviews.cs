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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "SQL Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ", false, 5 * 60, "General", 1, TIMEOUT_KEY )]
    public class UpdatePersistedDataviews : IJob
    {
        private const string TIMEOUT_KEY = "SqlCommandTimeout";

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersistedDataviews()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int sqlCommandTimeout = dataMap.GetString( TIMEOUT_KEY ).AsIntegerOrNull() ?? 300;
            StringBuilder results = new StringBuilder();
            int updatedDataViewCount = 0;
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            using ( var rockContext = new RockContext() )
            {
                var currentDateTime = RockDateTime.Now;

                // get a list of all the data views that need to be refreshed
                var expiredPersistedDataViews = new DataViewService( rockContext ).Queryable()
                    .Where( a => a.PersistedScheduleIntervalMinutes.HasValue )
                    .Where( a =>
                        ( a.PersistedLastRefreshDateTime == null )
                        || ( System.Data.Entity.SqlServer.SqlFunctions.DateAdd( "mi", a.PersistedScheduleIntervalMinutes.Value, a.PersistedLastRefreshDateTime.Value ) < currentDateTime )
                        );

                var expiredPersistedDataViewsList = expiredPersistedDataViews.ToList();
                foreach ( var dataView in expiredPersistedDataViewsList )
                {
                    var name = dataView.Name;
                    try
                    {
                        context.UpdateLastStatusMessage( $"Updating {dataView.Name}" );
                        dataView.PersistResult( sqlCommandTimeout );
                        dataView.PersistedLastRefreshDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                        updatedDataViewCount++;
                    }
                    catch ( Exception ex )
                    {
                        // Capture and log the exception because we're not going to fail this job
                        // unless all the data views fail.
                        var errorMessage = $"An error occurred while trying to update persisted data view '{name}' so it was skipped. Error: {ex.Message}";
                        errors.Add( errorMessage );
                        var ex2 = new Exception( errorMessage, ex );
                        exceptions.Add( ex2 );
                        ExceptionLogService.LogException( ex2, null );
                        continue;
                    }
                }
            }

            // Format the result message
            results.AppendLine( $"Updated {updatedDataViewCount} {"dataview".PluralizeIf( updatedDataViewCount != 1 )}" );
            context.Result = results.ToString();

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( "Errors: " );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                // We're not going to throw an aggregate exception unless there were no successes.
                // Otherwise the status message does not show any of the success messages in
                // the last status message.
                if ( updatedDataViewCount == 0 )
                {
                    throw new AggregateException( exceptions.ToArray() );
                }
            }
        }
    }
}
