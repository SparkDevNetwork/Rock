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
using System.Web;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SmsActionService
    {
        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The message received by the communications component.</param>
        /// <returns>If not null, identifies the response that should be sent back to the sender.</returns>
        static public List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message )
        {
            return ProcessIncomingMessage( message, null );
        }

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="smsPipelineId">
        /// The SMS pipeline identifier. When null the active pipeline with the lowest SmsPipelineId will be used.
        /// </param>
        /// <returns></returns>
        static public List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message, int? smsPipelineId )
        {
            if ( smsPipelineId == null )
            {
                var minSmsPipelineId = new SmsPipelineService( new RockContext() )
                                        .Queryable()
                                        .Where( p => p.IsActive )
                                        .Select( p => ( int? ) p.Id )
                                        .Min( p => p );

                if ( minSmsPipelineId == null )
                {
                    var errorMessage = string.Format( "No default SMS Pipeline could be found." );
                    return CreateErrorOutcomesWithLogging( errorMessage );
                }

                smsPipelineId = minSmsPipelineId;
            }

            return ProcessIncomingMessage( message, smsPipelineId.Value );
        }

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The message received by the communications component.</param>
        /// <param name="smsPipelineId">The SMS pipeline identifier.</param>
        /// <returns>
        /// If not null, identifies the response that should be sent back to the sender.
        /// </returns>
        static public List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message, int smsPipelineId )
        {
            var errorMessage = string.Empty;
            var outcomes = new List<SmsActionOutcome>();
            var smsPipelineService = new SmsPipelineService( new RockContext() );
            var smsPipeline = smsPipelineService.Get( smsPipelineId );

            if ( smsPipeline == null )
            {
                errorMessage = string.Format( "The SMS Pipeline for SMS Pipeline Id {0} was null.", smsPipelineId );
                return CreateErrorOutcomesWithLogging( errorMessage );
            }

            if ( !smsPipeline.IsActive )
            {
                errorMessage = string.Format( "The SMS Pipeline for SMS Pipeline Id {0} was inactive.", smsPipelineId );
                return CreateErrorOutcomesWithLogging( errorMessage );
            }

            var smsActions = SmsActionCache.All()
                .Where( a => a.IsActive )
                .Where( a => a.SmsPipelineId == smsPipelineId )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id );

            foreach ( var smsAction in smsActions )
            {
                if ( smsAction.SmsActionComponent == null )
                {
                    LogIfError( string.Format( "The SmsActionComponent for {0} was null", smsAction.Name ) );
                    continue;
                }

                var outcome = new SmsActionOutcome
                {
                    ActionName = smsAction.Name
                };
                outcomes.Add( outcome );

                try
                {
                    //
                    // Check if the action wants to process this message.
                    //
                    outcome.ShouldProcess = smsAction.SmsActionComponent.ShouldProcessMessage( smsAction, message, out errorMessage );
                    outcome.ErrorMessage = errorMessage;
                    LogIfError( errorMessage );

                    if ( !outcome.ShouldProcess )
                    {
                        continue;
                    }

                    //
                    // Process the message and use either the response returned by the action
                    // or the previous response we already had.
                    //
                    outcome.Response = smsAction.SmsActionComponent.ProcessMessage( smsAction, message, out errorMessage );
                    outcome.ErrorMessage = errorMessage;
                    LogIfError( errorMessage );

                    if ( outcome.Response != null )
                    {
                        LogIfError( errorMessage );
                    }

                    //
                    // Log an interaction if this action completed successfully.
                    //
                    if ( smsAction.IsInteractionLoggedAfterProcessing && errorMessage.IsNullOrWhiteSpace() )
                    {
                        WriteInteraction( smsAction, message, smsPipeline );
                        outcome.IsInteractionLogged = true;
                    }
                }
                catch ( Exception exception )
                {
                    outcome.Exception = exception;
                    LogIfError( exception );
                }

                //
                // If the action is set to not continue after processing then stop.
                //
                if ( outcome.ShouldProcess && !smsAction.ContinueAfterProcessing )
                {
                    break;
                }
            }

            return outcomes;
        }

        /// <summary>
        /// Adds an Interaction for the specified SmsAction.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="pipeline"></param>
        private static void WriteInteraction( SmsActionCache action, SmsMessage message, SmsPipeline pipeline )
        {
            if ( action == null || message == null || pipeline == null )
            {
                return;
            }

            // Get the Interaction Channel for SMS Pipelines.
            var interactionChannelId = InteractionChannelCache.GetId( SystemGuid.InteractionChannel.SMS_PIPELINE.AsGuid() );
            if ( interactionChannelId == null )
            {
                ExceptionLogService.LogException( $"Interaction Write for SMS Action failed. The SMS Pipeline Channel is not configured.\n[ChannelGuid={Rock.SystemGuid.InteractionChannel.SMS_PIPELINE}, Pipeline={pipeline.Name}, Action={action.Name}]" );
                return;
            }

            // Create the interaction data.
            var now = RockDateTime.Now;

            var interactionData = new SmsInteractionData
            {
                ToPhone = message.ToNumber,
                FromPhone = message.FromNumber,
                MessageBody = message.Message,
                ReceivedDateTime = now,
                FromPerson = message.FromPerson?.FullName
            };

            // Create a transaction to add the Interaction.
            var summary = $"{pipeline.Name} ({pipeline.Id}) - {action.Name}";

            var info = new InteractionTransactionInfo
            {
                InteractionDateTime = now,
                InteractionChannelId = interactionChannelId.GetValueOrDefault(),

                ComponentEntityId = pipeline.Id,
                ComponentName = pipeline.Name,

                InteractionSummary = summary,
                InteractionOperation = action.Name,
                InteractionData = interactionData.ToJson(),

                PersonAliasId = message.FromPerson?.PrimaryAliasId
            };

            var interactionTransaction = new InteractionTransaction( info );
            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Creates the error outcomes with logging.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private static List<SmsActionOutcome> CreateErrorOutcomesWithLogging( string errorMessage )
        {
            LogIfError( errorMessage );
            return new List<SmsActionOutcome>
            {
                new SmsActionOutcome
                {
                    ErrorMessage = errorMessage
                }
            };
        }

        /// <summary>
        /// From the list of outcomes, get the last outcome with a non-null message and return that message
        /// </summary>
        /// <param name="outcomes"></param>
        /// <returns></returns>
        public static SmsMessage GetResponseFromOutcomes( List<SmsActionOutcome> outcomes )
        {
            if ( outcomes == null )
            {
                return null;
            }

            var lastOutcomeWithResponse = outcomes.LastOrDefault( o => o.Response != null );
            return lastOutcomeWithResponse == null ? null : lastOutcomeWithResponse.Response;
        }

        /// <summary>
        /// If the exception is not null, log it
        /// </summary>
        /// <param name="exception"></param>
        static private void LogIfError( Exception exception )
        {
            if ( exception == null )
            {
                return;
            }

            var context = HttpContext.Current;
            var wrappedException = new Exception( "An exception occurred in the SmsAction pipeline. See the inner exception.", exception );
            ExceptionLogService.LogException( wrappedException, context );
        }

        /// <summary>
        /// If the errorMessage is not empty, log it
        /// </summary>
        /// <param name="errorMessage"></param>
        static private void LogIfError( string errorMessage )
        {
            if ( string.IsNullOrEmpty( errorMessage ) )
            {
                return;
            }

            var context = HttpContext.Current;
            var exception = new Exception( string.Format( "An error occurred in the SmsAction pipeline: {0}", errorMessage ) );
            ExceptionLogService.LogException( exception, context );
        }

        #region Support classes

        private class SmsInteractionData
        {
            public string ToPhone;
            public string FromPhone;
            public string MessageBody;
            public DateTime ReceivedDateTime;
            public string FromPerson;
        }

        #endregion
    }
}
