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

using Microsoft.Extensions.Logging;

using Quartz;

using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Observability;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Base class that all scheduled jobs in Rock should inherit from.
    /// </summary>
    [DisallowConcurrentExecution]
#pragma warning disable CS0612, CS0618 // Type or member is obsolete
    public abstract class RockJob : Quartz.IJob
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Gets the job identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetJobId()
        {
            return ServiceJobId;
        }

        /// <summary>
        /// Gets the service job identifier.
        /// </summary>
        /// <value>The service job identifier.</value>
        public int ServiceJobId { get; private set; }

        /// <summary>
        /// Gets the name of the service job.
        /// </summary>
        /// <value>The name of the service job.</value>
        public string ServiceJobName => ServiceJob?.Name;

        /// <summary>
        /// Gets the service job.
        /// </summary>
        /// <value>The service job.</value>
        protected Rock.Model.ServiceJob ServiceJob { get; set; }

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        internal Quartz.IScheduler Scheduler { get; private set; }

        /// <summary>
        /// <para>
        /// Gets or sets the logger used to capture output messages.
        /// If set to <c>null</c> then the default logger will be used.
        /// </para>
        /// <para>
        /// The log message will be prefixed with:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], 
        /// </code>
        /// </para>
        /// </summary>
        protected internal ILogger Logger
        {
            get => _logger;
            internal set
            {
                if ( value != null )
                {
                    _logger = new RockJobLogger( value, ServiceJobId, ServiceJobName );
                }
                else
                {
                    _logger = new RockJobLogger( RockLogger.LoggerFactory.CreateLogger( GetType().FullName ), ServiceJobId, ServiceJobName );
                }
            }
        }
        private RockJobLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJob"/> class.
        /// </summary>
        public RockJob()
        {
            // Initialize the logger with a default instance.
            Logger = null;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Initializes from job context.
        /// </summary>
        /// <param name="context">The context.</param>
        internal void InitializeFromJobContext( IJobExecutionContext context )
        {
            var serviceJobId = context.GetJobIdFromQuartz();
            var rockContext = new Rock.Data.RockContext();
            this.ServiceJobId = serviceJobId;
            ServiceJob = new ServiceJobService( rockContext ).Get( serviceJobId );
            ServiceJob.LoadAttributes();
            Scheduler = context.Scheduler;

            _logger.JobId = ServiceJobId;
            _logger.JobName = ServiceJobName;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.String.</returns>
        public string GetAttributeValue( string key )
        {
            return ServiceJob?.GetAttributeValue( key );
        }

        /// <summary>
        /// Updates the last status message.
        /// NOTE: This method has a read and a write database operation and also writes to the Rock Logger with DEBUG level logging.
        /// </summary>
        /// <param name="statusMessage">The status message.</param>
        public void UpdateLastStatusMessage( string statusMessage )
        {
            Logger.LogDebug( statusMessage );

            Result = statusMessage;
            using ( var rockContext = new RockContext() )
            {
                var serviceJob = new ServiceJobService( rockContext ).Get( this.ServiceJobId );
                if ( serviceJob == null )
                {
                    return;
                }

                serviceJob.LastStatusMessage = statusMessage;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        public string Result { get; set; }

        /// <summary>
        /// Execute the Job using the specified context configuration.
        /// </summary>
        /// <param name="context"></param>
        internal void ExecuteInternal( IJobExecutionContext context )
        {
            using ( var activity = ObservabilityHelper.StartActivity( $"JOB: {GetType().FullName.Replace( "Rock.Jobs.", "" )} - {context.JobDetail.Key?.Group}" ) )
            {
                InitializeFromJobContext( context );

                activity?.AddTag( "rock-otel-type", "rock-job" );
                activity?.AddTag( "rock-job-id", ServiceJob.Id );
                activity?.AddTag( "rock-job-type", GetType().FullName.Replace( "Rock.Jobs.", "" ) );
                activity?.AddTag( "rock-job-description", ServiceJob.Description );

                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    Execute();
                    activity?.AddTag( "rock-job-result", "Success" );
                }
                catch
                {
                    activity?.AddTag( "rock-job-result", "Failed" );
                    // the exception needs to be thrown so that the Scheduler catches it and passes it to the <see cref="Rock.Jobs.RockJobListener.JobWasExecuted" /> for logging to the front end
                    throw;
                }
                finally
                {
                    activity?.AddTag( "rock-job-duration", sw.Elapsed.TotalSeconds );
                    activity?.AddTag( "rock-job-message", Result );
                }
            }
        }

        /// <summary>
        /// Execute the Job using the specified configuration settings.
        /// </summary>
        /// <param name="testAttributeValues"></param>
        internal void ExecuteInternal( Dictionary<string, string> testAttributeValues )
        {
            // If this job instance is not associated with a stored Job definition, create a new definition.
            if ( this.ServiceJob == null )
            {
                ServiceJob = new ServiceJob();
                ServiceJob.LoadAttributes();
            }

            foreach ( var attributeValue in testAttributeValues )
            {
                var existingValue = this.ServiceJob.AttributeValues.GetValueOrNull( attributeValue.Key );
                if ( existingValue == null )
                {
                    existingValue = new AttributeValueCache();
                    this.ServiceJob.AttributeValues.Add( attributeValue.Key, existingValue );
                }
                existingValue.Value = attributeValue.Value;
            }

            Execute();
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
#pragma warning disable CS0612, CS0618 // Type or member is obsolete
        void Quartz.IJob.Execute( Quartz.IJobExecutionContext context )
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
        {
            ExecuteInternal( context );
        }

        /// <summary>
        /// Writes a message to the log at the specified level.
        /// <para>
        /// The log message will be prefixed with:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], 
        /// </code>
        /// If <paramref name="start"/> is provided:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], Start: [<paramref name="start"/>], 
        /// </code>
        /// If <paramref name="start"/> and <paramref name="elapsedMs"/> are provided:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], Start: [<paramref name="start"/>], End: [<paramref name="start"/> + <paramref name="elapsedMs"/>], Time to Run: [<paramref name="elapsedMs"/>]ms, 
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="start">The optional start date time for the process described by this message.</param>
        /// <param name="elapsedMs">The optional elapsed time (in milliseconds) for the process described by this message.</param>
        internal void Log( LogLevel logLevel, string message, DateTime? start = null, long? elapsedMs = null )
        {
            Log( logLevel, null, message, start, elapsedMs );
        }

        /// <summary>
        /// Writes a message to the log at the specified level.
        /// <para>
        /// The log message will be prefixed with:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], 
        /// </code>
        /// If <paramref name="start"/> is provided:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], Start: [<paramref name="start"/>], 
        /// </code>
        /// If <paramref name="start"/> and <paramref name="elapsedMs"/> are provided:
        /// <code>
        /// Job ID: [ServiceJob.Id], Job Name: [ServiceJob.Name], Start: [<paramref name="start"/>], End: [<paramref name="start"/> + <paramref name="elapsedMs"/>], Time to Run: [<paramref name="elapsedMs"/>]ms, 
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="start">The optional start date time for the process described by this message.</param>
        /// <param name="elapsedMs">The optional elapsed time (in milliseconds) for the process described by this message.</param>
        internal void Log( LogLevel logLevel, Exception exception, string message, DateTime? start = null, long? elapsedMs = null )
        {
            if ( message.IsNullOrWhiteSpace() )
            {
                return;
            }

            var prefix = new StringBuilder();

            if ( start.HasValue )
            {
                prefix.Append( $"Start: {start}" );

                if ( elapsedMs.HasValue )
                {
                    prefix.Append( $", End: {start.Value.AddMilliseconds( elapsedMs.Value )}, Time To Run: {elapsedMs}ms" );
                }
            }

            var logger = Logger ?? RockLogger.LoggerFactory.CreateLogger( GetType().FullName );

            logger.Log(
               logLevel,
               exception,
               prefix.Length > 0 ? $"{prefix}, {message}" : message
           );
        }
    }
}
