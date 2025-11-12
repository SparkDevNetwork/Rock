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
using System.Data.Entity;
using System.Linq;
using System.Web;

using Rock.Attribute;
using Rock.Communication.Medium;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SmsActionService
    {
        #region Fields

        /// <summary>
        /// The list of keywords indicating an individual wants to opt out of receiving messages.
        /// </summary>
        private static readonly Lazy<HashSet<string>> _optOutKeywords = new Lazy<HashSet<string>>( () =>
        {
            return new HashSet<string>
            {
                "STOP", "STOPALL", "UNSUBSCRIBE", "CANCEL", "END", "QUIT", "REVOKE", "OPTOUT"
            };
        } );

        /// <summary>
        /// The list of keywords indicating an individual wants to opt into receiving messages.
        /// </summary>
        private static readonly Lazy<HashSet<string>> _optInKeywords = new Lazy<HashSet<string>>( () =>
        {
            return new HashSet<string>
            {
                "START", "YES", "UNSTOP"
            };
        } );

        /// <summary>
        /// The lazy system sender <see cref="Person"/>.
        /// </summary>
        private static readonly Lazy<Person> _systemSenderPerson = new Lazy<Person>( () =>
        {
            using ( var rockContext = new RockContext() )
            {
                var systemSenderGuid = Rock.SystemGuid.Person.SYSTEM_SENDER.AsGuid();

                return new PersonService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( p => p.Aliases )
                    .FirstOrDefault( p => p.Guid.Equals( systemSenderGuid ) );
            }
        } );

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the list of keywords indicating an individual wants to opt out of receiving messages.
        /// </summary>
        private static HashSet<string> OptOutKeywords => _optOutKeywords.Value;

        /// <summary>
        /// Gets the list of keywords indicating an individual wants to opt into receiving messages.
        /// </summary>
        private static HashSet<string> OptInKeywords => _optInKeywords.Value;

        /// <summary>
        /// Gets the system sender <see cref="Person"/>.
        /// </summary>
        private static Person SystemSenderPerson => _systemSenderPerson.Value;

        /// <summary>
        /// Gets the organization name to be used in response messages.
        /// </summary>
        private static string OrganizationName
        {
            get
            {
                var orgName = GlobalAttributesCache.Value( Rock.SystemKey.GlobalAttributeKey.ORGANIZATION_ABBREVATION );
                if ( orgName.IsNullOrWhiteSpace() )
                {
                    orgName = GlobalAttributesCache.Value( Rock.SystemKey.GlobalAttributeKey.ORGANIZATION_NAME );
                }

                return orgName;
            }
        }

        /// <summary>
        /// Gets the organization phone number to be used in response messages.
        /// </summary>
        private static string OrganizationPhone => GlobalAttributesCache.Value( Rock.SystemKey.GlobalAttributeKey.ORGANIZATION_PHONE );

        /// <summary>
        /// Gets the organization email to be used in response messages.
        /// </summary>
        private static string OrganizationEmail => GlobalAttributesCache.Value( Rock.SystemKey.GlobalAttributeKey.ORGANIZATION_EMAIL );

        #endregion Properties

        #region Message Processing

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The incoming message that should be processed.</param>
        /// <returns>
        /// A list of outcomes that might contain a response to send back to the incoming message sender.
        /// </returns>
        public static List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message )
        {
            return ProcessIncomingMessage( message, null );
        }

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The incoming message that should be processed.</param>
        /// <param name="smsPipelineId">
        /// The <see cref="SmsPipeline"/> identifier. If <c>null</c>, the active pipeline with the lowest ID will be used.
        /// </param>
        /// <returns>
        /// A list of outcomes that might contain a response to send back to the incoming message sender.
        /// </returns>
        public static List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message, int? smsPipelineId )
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
                    var errorMessage = "No default SMS Pipeline could be found.";
                    return new List<SmsActionOutcome> {
                        LogExceptionAndCreateErrorOutcome( errorMessage )
                    };
                }

                smsPipelineId = minSmsPipelineId;
            }

            return ProcessIncomingMessage( message, smsPipelineId.Value );
        }

        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The incoming message that should be processed.</param>
        /// <param name="smsPipelineId">The <see cref="SmsPipeline"/> identifier.</param>
        /// <returns>
        /// A list of outcomes that might contain a response to send back to the incoming message sender.
        /// </returns>
        public static List<SmsActionOutcome> ProcessIncomingMessage( SmsMessage message, int smsPipelineId )
        {
            // Opt-in/opt-out tracking should be processed before anything else, to ensure we respect the sender's
            // preferences and also to ensure we remain compliant with messaging regulations.
            TryUpdateOptInOutTrackingForSender( message );

            SmsPipeline smsPipeline;

            using ( var rockContext = new RockContext() )
            {
                smsPipeline = new SmsPipelineService( rockContext ).GetNoTracking( smsPipelineId );
            }

            var outcomes = new List<SmsActionOutcome>();
            var errorMessage = string.Empty;

            if ( smsPipeline == null )
            {
                errorMessage = $"SMS Pipeline with ID {smsPipelineId} could not be found.";
                outcomes.Add( LogExceptionAndCreateErrorOutcome( errorMessage ) );
            }
            else if ( !smsPipeline.IsActive )
            {
                errorMessage = $"SMS Pipeline with ID {smsPipelineId} is inactive.";
                outcomes.Add( LogExceptionAndCreateErrorOutcome( errorMessage ) );
            }
            else
            {
                var smsActions = SmsActionCache.All()
                    .Where( a => a.IsActive )
                    .Where( a => a.SmsPipelineId == smsPipelineId )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id );

                foreach ( var smsAction in smsActions )
                {
                    if ( smsAction.SmsActionComponent == null )
                    {
                        LogIfError( $"The '{smsAction.Name}' SMS Action could not be found." );
                        continue;
                    }

                    var outcome = new SmsActionOutcome
                    {
                        ActionName = smsAction.Name
                    };

                    outcomes.Add( outcome );

                    try
                    {
                        // Check if the action wants to process this message.
                        outcome.ShouldProcess = smsAction.SmsActionComponent.ShouldProcessMessage( smsAction, message, out errorMessage );
                        outcome.ErrorMessage = errorMessage;
                        LogIfError( errorMessage );

                        if ( !outcome.ShouldProcess )
                        {
                            continue;
                        }

                        // Process the message and use either the response returned by the action
                        // or the previous response we already had.
                        outcome.Response = smsAction.SmsActionComponent.ProcessMessage( smsAction, message, out errorMessage );
                        outcome.ErrorMessage = errorMessage;
                        LogIfError( errorMessage );

                        // Log an interaction if this action completed successfully.
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

                    // If the action is set to not continue after processing then stop.
                    if ( outcome.ShouldProcess && !smsAction.ContinueAfterProcessing )
                    {
                        break;
                    }
                }
            }

            // If the sender is opting into or out of messaging, ensure this response is the last outcome added, so it
            // will be the response sent back.
            var optInOutOutcome = TryGetOptInOutOutcome( message );
            if ( optInOutOutcome != null )
            {
                if ( !message.WasOptInOutTrackingProcessed )
                {
                    errorMessage = $"System Phone Number {message.ToNumber} is misconfigured: unable to send opt-in/opt-out auto-replies unless Rock also manages opt-out tracking.";
                    outcomes.Add( LogExceptionAndCreateErrorOutcome( errorMessage ) );
                }
                else
                {
                    outcomes.Add( optInOutOutcome );
                }
            }

            return outcomes;
        }

        /// <summary>
        /// Writes an <see cref="Interaction"/> for the specified SMS action, message and pipeline.
        /// </summary>
        /// <param name="action">The SMS action.</param>
        /// <param name="message">The incoming message.</param>
        /// <param name="pipeline">The pipeline through which this message is flowing.</param>
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
        /// Logs an exception and creates the error outcome.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>An <see cref="SmsActionOutcome"/> containing the error message.</returns>
        private static SmsActionOutcome LogExceptionAndCreateErrorOutcome( string errorMessage )
        {
            LogIfError( errorMessage );
            return new SmsActionOutcome
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// From the list of outcomes, get the last outcome with a non-null message and return that message.
        /// </summary>
        /// <param name="outcomes">The list of outcomes to search for a message.</param>
        /// <returns>
        /// The last non-null message or <c>null</c> if one couldn't be found.
        /// </returns>
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
        /// Creates a <see cref="Communication"/> record to represent an automated response and adds it to the Rock
        /// message bus for sending.
        /// </summary>
        /// <param name="smsMessage">
        /// The <see cref="SmsMessage"/> that contains the message body and any attachments to send.
        /// </param>
        /// <param name="toPersonAliasId">
        /// The <see cref="PersonAlias"/> identifier that represents the recipient of the automated response.
        /// </param>
        /// <param name="fromSystemPhoneNumber">
        /// The string representation of the <see cref="SystemPhoneNumber"/> from which the response should be sent.
        /// </param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The identifier of the queued <see cref="Communication"/> or <c>null</c> if unable to send.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public static int? CreateAndEnqueueResponseCommunication( SmsMessage smsMessage, int toPersonAliasId, string fromSystemPhoneNumber, RockContext rockContext )
        {
            if ( smsMessage?.SaveAsResponse != true
                || toPersonAliasId <= 0
                || fromSystemPhoneNumber.IsNullOrWhiteSpace()
                || rockContext == null )
            {
                return null;
            }

            var systemPhoneNumber = SystemPhoneNumberCache.GetByNumber( fromSystemPhoneNumber );
            if ( systemPhoneNumber == null )
            {
                return null;
            }

            if ( SystemSenderPerson == null )
            {
                return null;
            }

            var responseCode = Sms.GenerateResponseCode( rockContext );

            var responseCommunication = Sms.CreateCommunicationMobile(
                SystemSenderPerson,
                toPersonAliasId,
                smsMessage.Message,
                systemPhoneNumber,
                responseCode,
                smsMessage.Attachments,
                rockContext
            );

            return responseCommunication?.Id;
        }

        /// <summary>
        /// If the exception is not null, log it.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        static private void LogIfError( Exception exception )
        {
            if ( exception == null )
            {
                return;
            }

            var context = HttpContext.Current;
            var wrappedException = new Exception( "An error occurred in the SMS Pipeline. See the inner exception.", exception );
            ExceptionLogService.LogException( wrappedException, context );
        }

        /// <summary>
        /// If the error message is not empty, log it.
        /// </summary>
        /// <param name="errorMessage">The error message to log.</param>
        static private void LogIfError( string errorMessage )
        {
            if ( errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            var context = HttpContext.Current;
            var exception = new Exception( $"An error occurred in the SMS Pipeline: {errorMessage}" );
            ExceptionLogService.LogException( exception, context );
        }

        #endregion Message Processing

        #region Opt-In/Opt-Out Handling

        /// <summary>
        /// Tries to update opt-in/opt-out tracking values for the <see cref="Person"/> who sent this message if not
        /// already processed and if such tracking has not been disabled at the <see cref="SystemPhoneNumber"/> level.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <remarks>
        /// This is an internal API that supports the Rock infrastructure and not
        /// subject to the same compatibility standards as public APIs. It may be
        /// changed or removed without notice in any release. You should only use
        /// it directly in your code with extreme caution and knowing that doing so
        /// can result in application failures when updating to a new Rock release.
        /// </remarks>
        [RockInternal( "18.0" )]
        public static void TryUpdateOptInOutTrackingForSender( SmsMessage message )
        {
            var toNumber = message?.ToNumber;
            var fromNumber = message?.FromNumber;

            if ( toNumber.IsNullOrWhiteSpace() || fromNumber.IsNullOrWhiteSpace() || message.WasOptInOutTrackingProcessed )
            {
                return;
            }

            if ( !message.DisableSmsOptInOutTracking.HasValue )
            {
                var systemPhoneNumber = Sms.FindRockSmsSystemPhoneNumber( toNumber );
                message.DisableSmsOptInOutTracking = systemPhoneNumber?.DisableSmsOptInOutTracking ?? false;
            }

            if ( message.DisableSmsOptInOutTracking.Value )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                List<PhoneNumber> GetMatchingPhoneNumbers()
                {
                    var cleanedFromNumber = PhoneNumber.CleanNumber( fromNumber );
                    return new PhoneNumberService( rockContext )
                        .Queryable()
                        .Where( p =>
                            p.Number == cleanedFromNumber
                            || p.Number == fromNumber
                            || p.FullNumber == cleanedFromNumber
                            || p.FullNumber == fromNumber
                        )
                        .ToList();
                }

                var shouldSaveChanges = false;

                if ( IsOptOutMessage( message.Message ) )
                {
                    foreach ( var phoneNumber in GetMatchingPhoneNumbers() )
                    {
                        phoneNumber.IsMessagingEnabled = false;
                        phoneNumber.IsMessagingOptedOut = true;
                        phoneNumber.MessagingOptedOutDateTime = RockDateTime.Now;
                        shouldSaveChanges = true;
                    }
                }
                else if ( IsOptInMessage( message.Message ) )
                {
                    foreach ( var phoneNumber in GetMatchingPhoneNumbers() )
                    {
                        phoneNumber.IsMessagingEnabled = true;
                        phoneNumber.IsMessagingOptedOut = false;
                        phoneNumber.MessagingOptedOutDateTime = null;
                        shouldSaveChanges = true;
                    }
                }

                if ( shouldSaveChanges )
                {
                    rockContext.SaveChanges();
                }
            }

            message.WasOptInOutTrackingProcessed = true;
        }

        /// <summary>
        /// Tries to get an opt-in/opt-out response outcome for this message if it contains a matching keyword and if
        /// such messaging has not been disabled at the <see cref="SystemPhoneNumber"/> level.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <returns>
        /// An opt-in/opt-out response outcome or <c>null</c> if such a response should not be sent.
        /// </returns>
        private static SmsActionOutcome TryGetOptInOutOutcome( SmsMessage message )
        {
            var toNumber = message?.ToNumber;
            var fromNumber = message?.FromNumber;

            SmsActionOutcome outcome = null;

            if ( toNumber.IsNullOrWhiteSpace() || fromNumber.IsNullOrWhiteSpace() )
            {
                return outcome;
            }

            if ( !message.SuppressSmsOptInOutAutoReplies.HasValue )
            {
                var systemPhoneNumber = Sms.FindRockSmsSystemPhoneNumber( toNumber );
                message.SuppressSmsOptInOutAutoReplies = systemPhoneNumber?.SuppressSmsOptInOutAutoReplies ?? false;
            }

            if ( message.SuppressSmsOptInOutAutoReplies.Value )
            {
                return outcome;
            }

            if ( IsOptOutMessage( message.Message ) )
            {
                var contactMethods = new[] { OrganizationPhone, OrganizationEmail }
                    .Where( m => m.IsNotNullOrWhiteSpace() )
                    .ToArray();

                var optOutContactMethods = contactMethods.Any()
                    ? $" at {contactMethods.JoinStringsWithRepeatAndFinalDelimiterWithMaxLength( ", ", " or ", null )}"
                    : string.Empty;

                var optedOutMessage = $"You are unsubscribed from {OrganizationName} messages. No more messages will be sent. Reply HELP for help or contact us{optOutContactMethods}.";

                outcome = new SmsActionOutcome
                {
                    ActionName = "SMSOptOut",
                    ShouldProcess = true,
                    Response = new SmsMessage
                    {
                        ToNumber = PhoneNumber.CleanNumber( fromNumber ),
                        FromNumber = message.ToNumber,
                        Message = optedOutMessage,
                        SaveAsResponse = true
                    }
                };
            }
            else if ( IsOptInMessage( message.Message ) )
            {
                var optedInMessage = $"You are now subscribed to {OrganizationName} messages. Message and data rates may apply. Reply STOP to unsubscribe or HELP for help.";

                outcome = new SmsActionOutcome
                {
                    ActionName = "SMSOptIn",
                    ShouldProcess = true,
                    Response = new SmsMessage
                    {
                        ToNumber = PhoneNumber.CleanNumber( fromNumber ),
                        FromNumber = message.ToNumber,
                        Message = optedInMessage,
                        SaveAsResponse = true
                    }
                };
            }

            return outcome;
        }

        /// <summary>
        /// Checks the message body to see if an opt-out keyword was sent.
        /// </summary>
        /// <param name="messageBody">The message body.</param>
        /// <returns>
        /// <c>true</c> if an opt-out keyword was sent; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsOptOutMessage( string messageBody )
        {
            foreach ( var keyword in OptOutKeywords )
            {
                if ( string.Equals( messageBody.Trim(), keyword, StringComparison.OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks the message body to see if an opt-in keyword was sent.
        /// </summary>
        /// <param name="messageBody">The message body.</param>
        /// <returns>
        /// <c>true</c> if an opt-in keyword was sent; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsOptInMessage( string messageBody )
        {
            foreach ( var keyword in OptInKeywords )
            {
                if ( string.Equals( messageBody.Trim(), keyword, StringComparison.OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }
            return false;
        }

        #endregion Opt-In/Opt-Out Handling

        #region Supporting classes

        /// <summary>
        /// A POCO to represent SMS interaction data.
        /// </summary>
        private class SmsInteractionData
        {
            /// <summary>
            /// Gets or sets the phone number to which the SMS message was sent.
            /// </summary>
            public string ToPhone { get; set; }

            /// <summary>
            /// Gets or sets the phone number from which the SMS message was sent.
            /// </summary>
            public string FromPhone { get; set; }

            /// <summary>
            /// Gets or sets the SMS message body.
            /// </summary>
            public string MessageBody { get; set; }

            /// <summary>
            /// Gets or sets the datetime the SMS message was received by Rock.
            /// </summary>
            public DateTime ReceivedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the full name of the person who sent the SMS message.
            /// </summary>
            public string FromPerson { get; set; }
        }

        #endregion Supporting classes
    }
}
