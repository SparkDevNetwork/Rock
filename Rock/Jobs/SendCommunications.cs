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
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to process communications
    /// </summary>
    [DisplayName( "Send Communications" )]
    [Description( "Job to send any future communications or communications not sent immediately by Rock." )]

    #region Job Attributes

    [IntegerField(
        "Delay Period",
        Key = AttributeKey.DelayPeriod,
        Description = "The number of minutes to wait before sending any new communication (If the communication block's 'Send When Approved' option is turned on, then a delay should be used here to prevent a send overlap).",
        IsRequired = false,
        DefaultIntegerValue = 30,
        Category = "",
        Order = 0 )]

    [IntegerField(
        "Expiration Period",
        Key = AttributeKey.ExpirationPeriod,
        Description = "The number of days after a communication was created or scheduled to be sent when it should no longer be sent.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Category = "",
        Order = 1 )]

    [IntegerField(
        "Parallel Communications",
        Key = AttributeKey.ParallelCommunications,
        Description = "The number of communications that can be sent at the same time.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Category = "",
        Order = 2 )]

    #endregion

    [RockLoggingCategory]
    public class SendCommunications : RockJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for job Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string DelayPeriod = "DelayPeriod";
            public const string ExpirationPeriod = "ExpirationPeriod";
            public const string ParallelCommunications = "ParallelCommunications";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendCommunications()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="RockJob.Execute()" />
        public override void Execute()
        {
            int expirationDays = this.GetAttributeValue( "ExpirationPeriod" ).AsInteger();
            int delayMinutes = this.GetAttributeValue( "DelayPeriod" ).AsInteger();
            int maxParallelization = this.GetAttributeValue( "ParallelCommunications" ).AsInteger();

            List<Model.Communication> sendCommunications = null;
            var startDateTime = RockDateTime.Now;
            var stopWatch = Stopwatch.StartNew();
            using ( var rockContext = new RockContext() )
            {
                sendCommunications = new CommunicationService( rockContext )
                    .GetQueued( expirationDays, delayMinutes, false, false )
                    .AsNoTracking()
                    .ToList()
                    .OrderBy( c => c.Id )
                    .ToList();
            }

            Log( LogLevel.Information, $"Retrieved {sendCommunications.Count} queued communications.", startDateTime, stopWatch.ElapsedMilliseconds );

            if ( sendCommunications == null )
            {
                this.Result = "No communications to send";
            }

            var exceptionMsgs = new List<string>();
            int communicationsSent = 0;

            startDateTime = RockDateTime.Now;
            stopWatch = Stopwatch.StartNew();
            var sendCommunicationTasks = new List<Task<SendCommunicationAsyncResult>>();

            using ( var mutex = new SemaphoreSlim( maxParallelization ) )
            {
                for ( var i = 0; i < sendCommunications.Count(); i++ )
                {
                    mutex.Wait();
                    var comm = sendCommunications[i];

                    sendCommunicationTasks.Add( Task.Run<SendCommunicationAsyncResult>( async () => await SendCommunicationAsync( comm, mutex ).ConfigureAwait( false ) ) );
                }

                /*
                 * Now that we have fired off all of the task, we need to wait for them to complete, get their results,
                 * and then process that result. Once all of the task have been completed we can continue.
                 */
                while ( sendCommunicationTasks.Count > 0 )
                {
                    // Wait for a task to complete using WhenAny and then return the completed task. Since we are not running in an asynchronous method we need to use RunSync.
                    var completedTask = AsyncHelper.RunSync( () => Task.WhenAny<SendCommunicationAsyncResult>( sendCommunicationTasks.ToArray() ) );

                    // Get and process the result of the completed task.
                    var communicationResult = completedTask.Result;
                    if ( communicationResult.Exception != null )
                    {
                        var agException = communicationResult.Exception as AggregateException;
                        if ( agException == null )
                        {
                            exceptionMsgs.Add( $"Exception occurred sending communication ID:{communicationResult.Communication.Id}:{Environment.NewLine}    {communicationResult.Exception.Messages().AsDelimited( Environment.NewLine + "   " )}" );
                        }
                        else
                        {
                            var allExceptions = agException.Flatten();
                            foreach ( var ex in allExceptions.InnerExceptions )
                            {
                                exceptionMsgs.Add( $"Exception occurred sending communication ID:{communicationResult.Communication.Id}:{Environment.NewLine}    {ex.Messages().AsDelimited( Environment.NewLine + "   " )}" );
                            }
                        }

                        ExceptionLogService.LogException( communicationResult.Exception, System.Web.HttpContext.Current );
                    }
                    else
                    {
                        communicationsSent++;
                    }

                    sendCommunicationTasks.Remove( completedTask );
                }
            }

            Log( LogLevel.Information, $"Sent {communicationsSent} communications.", startDateTime, stopWatch.ElapsedMilliseconds );

            if ( communicationsSent > 0 )
            {
                this.Result = string.Format( "Sent {0} {1}", communicationsSent, "communication".PluralizeIf( communicationsSent > 1 ) );
            }
            else
            {
                this.Result = "No communications to send";
            }

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred sending communications..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }

            // check for communications that have not been sent but are past the expire date. Mark them as failed and set a warning.
            var expireDateTimeEndWindow = RockDateTime.Now.AddDays( 0 - expirationDays );

            // limit the query to only look a week prior to the window to avoid performance issue (it could be slow to query at ALL the communication recipient before the expire date, as there could several years worth )
            var expireDateTimeBeginWindow = expireDateTimeEndWindow.AddDays( -7 );

            startDateTime = RockDateTime.Now;
            stopWatch = Stopwatch.StartNew();
            using ( var rockContext = new RockContext() )
            {
                var qryExpiredRecipients = new CommunicationRecipientService( rockContext ).Queryable()
                    .Where( cr =>
                        cr.Communication.Status == CommunicationStatus.Approved &&
                        cr.Status == CommunicationRecipientStatus.Pending &&
                        (
                            ( !cr.Communication.FutureSendDateTime.HasValue && cr.Communication.ReviewedDateTime.HasValue && cr.Communication.ReviewedDateTime < expireDateTimeEndWindow && cr.Communication.ReviewedDateTime > expireDateTimeBeginWindow )
                            || ( cr.Communication.FutureSendDateTime.HasValue && cr.Communication.FutureSendDateTime < expireDateTimeEndWindow && cr.Communication.FutureSendDateTime > expireDateTimeBeginWindow )
                        ) );

                rockContext.BulkUpdate( qryExpiredRecipients, c => new CommunicationRecipient { Status = CommunicationRecipientStatus.Failed, StatusNote = "Communication was not sent before the expire window (possibly due to delayed approval)." } );
            }

            Log( LogLevel.Information, @"Updated expired communication recipients' status to ""Failed"".", startDateTime, stopWatch.ElapsedMilliseconds );

            var statusMessage = SendEmailMetricsReminders();

            if ( statusMessage.IsNotNullOrWhiteSpace() )
            {
                this.Result += Environment.NewLine + statusMessage;
            }
        }

        /// <summary>
        /// Sends the communication asynchronous.
        /// </summary>
        /// <param name="comm">The comm.</param>
        /// <param name="mutex">The mutex.</param>
        /// <returns></returns>
        private async Task<SendCommunicationAsyncResult> SendCommunicationAsync( Model.Communication comm, SemaphoreSlim mutex )
        {
            var communicationResult = new SendCommunicationAsyncResult
            {
                Communication = comm
            };

            var startDateTime = RockDateTime.Now;
            var communicationStopWatch = Stopwatch.StartNew();
            Log( LogLevel.Debug, $"Starting to send {comm.Name}.", startDateTime );
            try
            {
                await Model.Communication.SendAsync( comm ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                communicationResult.Exception = ex;
            }

            Log( LogLevel.Information, $"{comm.Name} sent.", startDateTime, communicationStopWatch.ElapsedMilliseconds );
            mutex.Release();
            return communicationResult;
        }

        /// <summary>
        /// Sends the pending email metrics reminders.
        /// </summary>
        /// <returns></returns>
        private string SendEmailMetricsReminders()
        {
            var startDateTime = RockDateTime.Now;
            var stopWatch = Stopwatch.StartNew();

            using ( var rockContext = new RockContext() )
            {
                // Get the communications for which to send email metrics reminders.
                var emailMetricsReminders = new CommunicationService( rockContext )
                    .Queryable()
                    .Include( c => c.SenderPersonAlias.Person )

                    // Filter communications that have been sent (unsent communications will not have metrics).
                    .Where( c => c.SendDateTime.HasValue && c.SendDateTime.Value <= RockDateTime.Now )

                    // Filter communications that have a sender (this person will receive the email metrics reminders, so it cannot be null).
                    .Where( c => c.SenderPersonAliasId.HasValue )

                    // Filter communications that qualify for a metrics reminder.
                    .Where( c =>
                        c.EmailMetricsReminderOffsetDays.HasValue
                        && !c.EmailMetricsReminderSentDateTime.HasValue
                        && DbFunctions.AddDays( c.SendDateTime.Value, c.EmailMetricsReminderOffsetDays.Value ) <= RockDateTime.Now )

                    .Select( c => new SendEmailMetricsReminderData
                    {
                        Communication = c,
                        RecipientsCount = c.Recipients.Count,
                        EmailMetricsReminderRecipient = c.SenderPersonAlias.Person
                    } )
                    .ToList();

                if ( !emailMetricsReminders.Any() )
                {
                    var statusMessage = "No email metrics reminders to send";
                    Log( LogLevel.Information, statusMessage, startDateTime, stopWatch.ElapsedMilliseconds );
                    return statusMessage;
                }

                var communicationPage = PageCache.Get( SystemGuid.Page.NEW_COMMUNICATION.AsGuid(), rockContext );
                var communicationPageRoute = communicationPage?.GetBestMatchingRoute( new Dictionary<string, string> { ["CommunicationId"] = "0" } );

                if ( communicationPage == null || communicationPageRoute == null )
                {
                    var statusMessage = $@"Unable to send {emailMetricsReminders.Count} email metrics {( emailMetricsReminders.Count == 1 ? "reminder" : "reminders" )} because the internal communication page could not be found";
                    Log( LogLevel.Error, statusMessage, startDateTime, stopWatch.ElapsedMilliseconds );
                    return statusMessage;
                }

                var errorMessages = new List<string>();
                var pendingChanges = 0;
                var successCount = 0;

                foreach ( var emailMetricsReminder in emailMetricsReminders )
                {
                    if ( SendEmailMetricsReminder( emailMetricsReminder, communicationPage.Id, communicationPageRoute.Id, out var reminderErrorMessages ) )
                    {
                        successCount++;
                        emailMetricsReminder.Communication.EmailMetricsReminderSentDateTime = RockDateTime.Now;
                        pendingChanges++;

                        if ( pendingChanges >= 1000 )
                        {
                            rockContext.SaveChanges();
                            pendingChanges = 0;
                        }
                    }
                    else
                    {
                        string errorMessage;
                        if ( reminderErrorMessages?.Any() == true )
                        {
                            errorMessage = $"{( reminderErrorMessages.Count > 1 ? "Errors" : "Error" )} sending email metrics reminder for communication #{emailMetricsReminder.Communication.Id}:{Environment.NewLine}    {reminderErrorMessages.AsDelimited( Environment.NewLine + "    " )}";
                        }
                        else
                        {
                            errorMessage = $"Unknown error sending email metrics reminder for communication #:{emailMetricsReminder.Communication.Id}";
                        }

                        if ( !errorMessages.Contains( errorMessage ) )
                        {
                            errorMessages.Add( errorMessage );
                        }

                        Log( LogLevel.Error, errorMessage, startDateTime, stopWatch.ElapsedMilliseconds );
                    }
                }

                if ( pendingChanges > 0 )
                {
                    rockContext.SaveChanges();
                    pendingChanges = 0;
                }

                var statusMessages = new List<string>();

                if ( successCount > 0 )
                {
                    // Add the success message first...
                    statusMessages.Add( $"Sent {successCount:N0} email metrics {( successCount == 1 ? "reminder" : "reminders" )}" );
                }

                // ... then add the error messages.
                statusMessages.AddRange( errorMessages );

                return statusMessages.AsDelimited( Environment.NewLine );
            }
        }

        /// <summary>
        /// Sends an email metrics reminder.
        /// </summary>
        private bool SendEmailMetricsReminder( SendEmailMetricsReminderData data, int communicationPageId, int communicationPageRouteId, out List<string> errorMessages )
        {
            var internalApplicationRoot = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            var communicationPage = new Rock.Web.PageReference( communicationPageId, communicationPageRouteId )
            {
                Parameters = new Dictionary<string, string>
                {
                    ["CommunicationId"] = data.Communication.Id.ToString()
                }
            };
            var metricsUrl = internalApplicationRoot.EnsureTrailingForwardslash() + communicationPage.BuildUrl().RemoveLeadingForwardslash();
                
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Person", data.EmailMetricsReminderRecipient );
            mergeFields.AddOrReplace( "MetricsUrl", metricsUrl );
            mergeFields.AddOrReplace( "Communication", data.Communication );
            mergeFields.AddOrReplace( "RecipientsCount", data.RecipientsCount );

            var message = new Rock.Communication.RockEmailMessage( SystemGuid.SystemCommunication.COMMUNICATION_EMAIL_METRICS_REMINDER.AsGuid() );
            message.SetRecipients( new List<Rock.Communication.RockEmailMessageRecipient>
            {
                new Rock.Communication.RockEmailMessageRecipient( data.EmailMetricsReminderRecipient, mergeFields )
            } );
            message.CreateCommunicationRecord = false;

            return message.Send( out errorMessages );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// The result of the Send communication request.
        /// </summary>
        private class SendCommunicationAsyncResult
        {
            /// <summary>
            /// Gets or sets the exception.
            /// </summary>
            /// <value>
            /// The exception.
            /// </value>
            public Exception Exception { get; set; }

            /// <summary>
            /// Gets or sets the communication.
            /// </summary>
            /// <value>
            /// The communication.
            /// </value>
            public Model.Communication Communication { get; set; }
        }

        /// <summary>
        /// The data for sending an email metrics reminder.
        /// </summary>
        private class SendEmailMetricsReminderData
        {
            /// <summary>
            /// Gets or sets the communication.
            /// </summary>
            public Model.Communication Communication { get; set; }

            /// <summary>
            /// Gets or sets the communication recipients count.
            /// </summary>
            public int RecipientsCount { get; set; }

            /// <summary>
            /// Gets or sets the person who will receive the email metrics reminder.
            /// </summary>
            public Person EmailMetricsReminderRecipient { get; set; }
        }

        #endregion
    }
}