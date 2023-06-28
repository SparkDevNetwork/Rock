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

using Rock.Data;
using Rock.Logging;
using Rock.Model;
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
        private Rock.Model.ServiceJob ServiceJob { get; set; }

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        internal Quartz.IScheduler Scheduler { get; private set; }

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
        /// </summary>
        /// <param name="statusMessage">The status message.</param>
        public void UpdateLastStatusMessage( string statusMessage )
        {
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

        /// <inheritdoc/>
        internal void ExecuteInternal( IJobExecutionContext context )
        {
            InitializeFromJobContext( context );
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

        internal void ExecuteAsIntegrationTest( Quartz.IJobExecutionContext context, Dictionary<string, string> testAttributeValues )
        {
            InitializeFromJobContext( context );
            if ( this.ServiceJob == null )
            {
                ServiceJob = new ServiceJob();
                ServiceJob.LoadAttributes();
            }

            foreach ( var attributeValue in testAttributeValues )
            {
                var existingValue = this.ServiceJob.AttributeValues.GetValueOrNull( attributeValue.Key ) ?? new AttributeValueCache();
                existingValue.Value = attributeValue.Value;
            }

            Execute();
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
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="start">The optional start date time for the process described by this message.</param>
        /// <param name="elapsedMs">The optional elapsed time (in milliseconds) for the process described by this message.</param>
        /// <param name="propertyValues">The property values to enrich the message template, if any.</param>
        internal void Log( RockLogLevel logLevel, string messageTemplate, DateTime? start = null, long? elapsedMs = null, params object[] propertyValues )
        {
            Log( logLevel, null, messageTemplate, start, elapsedMs, propertyValues );
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
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="start">The optional start date time for the process described by this message.</param>
        /// <param name="elapsedMs">The optional elapsed time (in milliseconds) for the process described by this message.</param>
        /// <param name="propertyValues">The property values to enrich the message template, if any.</param>
        internal void Log( RockLogLevel logLevel, Exception exception, string messageTemplate, DateTime? start = null, long? elapsedMs = null, params object[] propertyValues )
        {
            if ( messageTemplate.IsNullOrWhiteSpace() )
            {
                return;
            }

            var messageTemplateSb = new StringBuilder( "Job ID: {jobId}, Job Name: {jobName}" );

            var propValues = new List<object>
            {
                this.ServiceJobId,
                this.ServiceJobName
            };

            if ( start.HasValue )
            {
                messageTemplateSb.Append( ", Start: {start}" );
                propValues.Add( start.Value );

                if ( elapsedMs.HasValue )
                {
                    messageTemplateSb.Append( ", End: {end}, Time To Run: {elapsedMs}ms" );
                    propValues.Add( start.Value.AddMilliseconds( elapsedMs.Value ) );
                    propValues.Add( elapsedMs.Value );
                }
            }

            propertyValues = propValues
                .Concat( propertyValues ?? new object[0] )
                .ToArray();

            RockLogger.Log.WriteToLog(
                logLevel,
                exception,
                RockLogDomains.Jobs,
                $"{messageTemplateSb}, {messageTemplate}",
                propertyValues
            );
        }
    }
}
