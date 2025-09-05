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
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Communication;
using Rock.Communication.Chat;
using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Sync;
using Rock.Core.Automation.Triggers;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Handles execution for the <see cref="SendFallbackChatNotification"/> event component.
    /// </summary>
    class SendFallbackChatNotificationExecutor : AutomationEventExecutor
    {
        #region Fields

        /// <summary>
        /// The logger instance that will handle logging diagnostic messages.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The identifier of the automation event that will be executed.
        /// </summary>
        private readonly int _automationEventId;

        /// <summary>
        /// The unique identifier of the system communication that will be sent.
        /// </summary>
        private readonly Guid _systemCommunicationGuid;

        /// <summary>
        /// The number of minutes the system will suppress notifications if the recipient has already received a
        /// recent notification and has not yet read the chat message that triggered it.
        /// </summary>
        private readonly int _notificationSuppressionMinutes;

        /// <summary>
        /// A Chat member will be excluded from fallback notifications if they have accessed Rock using a personal device
        /// within this number of days. Note that the same device must also currently have Rock notifications enabled.
        /// </summary>
        private readonly int _deviceSeenWithinDays;

        private static volatile bool HasLoggedMissingSystemCommunicationException = false;
        private static readonly object _missingSystemCommunicationExceptionLock = new object();

        private const string CacheKeyPrefix = "Rock.SendFallbackChatNotificationExecutor.SystemCommunication.Guid-";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendFallbackChatNotificationExecutor"/> class.
        /// </summary>
        /// <param name="automationEventId">The identifier of the automation event that will be executed.</param>
        /// <param name="systemCommunicationGuid"> The unique identifier of the system communication that will be sent.</param>
        /// <param name="notificationSuppressionMinutes">
        /// The number of minutes the system will suppress notifications if the recipient has already received a recent
        /// notification and has not yet read the chat message that triggered it.
        /// </param>
        /// <param name="deviceSeenWithinDays">
        /// A Chat member will be excluded from fallback notifications if they have accessed Rock using a personal device
        /// within this number of days. Note that the same device must also currently have Rock notifications enabled.
        /// </param>
        public SendFallbackChatNotificationExecutor(
            int automationEventId,
            Guid systemCommunicationGuid,
            int notificationSuppressionMinutes,
            int deviceSeenWithinDays
        )
        {
            _logger = RockLogger.LoggerFactory.CreateLogger<SendFallbackChatNotificationExecutor>();
            _automationEventId = automationEventId;
            _systemCommunicationGuid = systemCommunicationGuid;
            _notificationSuppressionMinutes = notificationSuppressionMinutes;
            _deviceSeenWithinDays = deviceSeenWithinDays;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            var channel = request.Values[ChatMessage.AutomationRequestValueKey.Channel] as Group;
            var senderPerson = request.Values[ChatMessage.AutomationRequestValueKey.SenderPerson] as Person;
            var sentRockDateTime = request.Values[ChatMessage.AutomationRequestValueKey.SentRockDateTime] as DateTime?;
            var eventChannelMembers = request.Values[ChatMessage.AutomationRequestValueKey.EventChannelMembers] as List<RockChatMessageEventChannelMember>;
            var mergeFields = new Dictionary<string, object>();

            foreach ( var value in request.Values )
            {
                mergeFields[value.Key] = value.Value;
            }

            Task.Run( () =>
            {
                using ( var rockContext = new RockContext() )
                using ( var chatHelper = new ChatHelper( rockContext ) )
                {
                    var systemCommunication = GetSystemCommunicationFromCacheOrDb( rockContext );
                    if ( systemCommunication == null )
                    {
                        // An exception will have already been logged below.
                        return;
                    }

                    var fallbackNotificationPeople = chatHelper.GetPeopleNeedingFallbackChatNotifications(
                        new FallbackChatNotificationsConfig
                        {
                            GroupId = channel?.Id,
                            EventChannelMembers = eventChannelMembers,
                            EventRockDateTime = sentRockDateTime ?? RockDateTime.Now,
                            PersonIdToExclude = senderPerson?.Id,
                            SystemCommunicationId = systemCommunication.Id,
                            NotificationSuppressionMinutes = _notificationSuppressionMinutes,
                            DeviceSeenWithinDays = _deviceSeenWithinDays
                        }
                    );

                    var structuredLog = "automation event ID {AutomationEventId}, group ID {GroupId}, chat message sender person ID {ChatMessageSenderPersonId} ({ChatMessageSenderPersonFullName})";

                    if ( fallbackNotificationPeople?.Any() != true )
                    {
                        _logger.LogDebug(
                            $"No channel members needing a fallback chat notification for {structuredLog}.",
                            _automationEventId,
                            channel?.Id,
                            senderPerson?.Id,
                            senderPerson?.FullName
                        );

                        return;
                    }

                    var eventCommunicationPreference = CommunicationType.RecipientPreference;
                    if ( !MediumContainer.HasActiveSmsTransport()
                        || !systemCommunication.SmsFromSystemPhoneNumberId.HasValue
                        || systemCommunication.SMSMessage.IsNullOrWhiteSpace() )
                    {
                        eventCommunicationPreference = CommunicationType.Email;
                    }

                    var sendMessageResult = new SendMessageResult();
                    var recipientStructuredLog = "fallback chat notification recipient person ID {FallbackRecipientPersonId} ({FallbackRecipientPersonFullName})";

                    foreach ( var fallbackNotificationPerson in fallbackNotificationPeople )
                    {
                        var recipientPerson = fallbackNotificationPerson.RecipientPerson;

                        // The recipient merge field MUST be added as "Person" in order for a [CommunicationRecipient]
                        // record to be created for SMS messages.
                        mergeFields["Person"] = recipientPerson;

                        var recipientCommunicationPreferences = new HashSet<CommunicationType>
                        {
                            eventCommunicationPreference
                        };

                        if ( fallbackNotificationPerson.GroupMemberCommunicationPreference.HasValue )
                        {
                            recipientCommunicationPreferences.Add( fallbackNotificationPerson.GroupMemberCommunicationPreference.Value );
                        }

                        recipientCommunicationPreferences.Add( recipientPerson.CommunicationPreference );

                        var emailMediumType = ( int ) CommunicationType.Email;
                        var smsMediumType = ( int ) CommunicationType.SMS;
                        var pushMediumType = ( int ) CommunicationType.PushNotification;

                        var mediumType = Model.Communication.DetermineMediumEntityTypeId(
                            emailMediumType,
                            smsMediumType,
                            pushMediumType,
                            recipientCommunicationPreferences.ToArray()
                        );

                        try
                        {
                            CreateMessageResult createMessageResult = null;

                            // A local function to help with the aggregation of per-message warnings and errors into the
                            // outer send message[s] result.
                            void AggregateWarningsAndErrors()
                            {
                                if ( createMessageResult == null )
                                {
                                    return;
                                }

                                sendMessageResult.Warnings.AddRange( createMessageResult.Warnings );
                                sendMessageResult.Errors.AddRange( createMessageResult.Errors );
                            }

                            if ( mediumType == emailMediumType )
                            {
                                createMessageResult = CommunicationHelper.CreateEmailMessage(
                                    recipientPerson,
                                    mergeFields,
                                    systemCommunication,
                                    _logger
                                );

                                if ( createMessageResult == null || !( createMessageResult.Message is RockEmailMessage emailMessage ) )
                                {
                                    AggregateWarningsAndErrors();
                                    continue;
                                }

                                // We need to ensure this email communication record is created as quickly as possible
                                // so we don't accidentally send someone multiple, rapid fallback notifications in the
                                // case of a busy chat channel. This is why we're not using the `CommunicationHelper
                                // .SendMessage()` method here.
                                emailMessage.CreateCommunicationRecordImmediately = true;

                                if ( !emailMessage.Send( out var errorMessages ) )
                                {
                                    sendMessageResult.Errors.AddRange( errorMessages );
                                    continue;
                                }

                                sendMessageResult.MessagesSent++;
                            }
                            else if ( mediumType == smsMediumType )
                            {
                                createMessageResult = CommunicationHelper.CreateSmsMessage(
                                    recipientPerson,
                                    mergeFields,
                                    systemCommunication,
                                    _logger
                                );

                                if ( createMessageResult == null || !( createMessageResult.Message is RockSMSMessage smsMessage ) )
                                {
                                    AggregateWarningsAndErrors();
                                    continue;
                                }

                                // SMS communication records are created immediately by default, so we don't have to do
                                // anything special for this to happen. However, if we need to get a handle on this
                                // communication's ID within this method body in the future, we'll have to take a more
                                // manual sending approach. Here's an example:
                                // https://github.com/SparkDevNetwork/Rock/blob/92e7472ae66543baf43db3c834067a6a198b29e9/Rock/Jobs/SendLearningNotifications.cs#L740-L783
                                // For now, the standard sending approach will suffice.
                                if ( !smsMessage.Send( out var errorMessages ) )
                                {
                                    sendMessageResult.Errors.AddRange( errorMessages );
                                    continue;
                                }

                                sendMessageResult.MessagesSent++;
                            }
                        }
                        catch ( Exception ex )
                        {
                            _logger.LogError(
                                ex,
                                $"Failed to send a fallback chat notification for {structuredLog}, {recipientStructuredLog}.",
                                _automationEventId,
                                channel?.Id,
                                senderPerson?.Id,
                                senderPerson?.FullName,
                                recipientPerson.Id,
                                recipientPerson.FullName
                            );
                        }
                    }

                    structuredLog += ". {@SendMessageResult}";

                    _logger.LogDebug(
                        $"Sent fallback chat notification for {structuredLog}.",
                        _automationEventId,
                        channel?.Id,
                        senderPerson?.Id,
                        senderPerson?.FullName,
                        sendMessageResult
                    );
                }
            } );
        }

        /// <summary>
        /// Gets the <see cref="SystemCommunication"/> from Rock's cache or the database.
        /// </summary>
        /// <param name="rockContext">The rock context to use if the communication isn't already cached.</param>
        /// <returns>The <see cref="SystemCommunication"/> to be sent.</returns>
        /// <remarks>
        /// If not already cached, the communication will be added to Rock's cache before being returned from this method.
        /// </remarks>
        private SystemCommunication GetSystemCommunicationFromCacheOrDb( RockContext rockContext )
        {
            var cacheKey = $"{CacheKeyPrefix}{_systemCommunicationGuid}";

            var systemCommunication = RockCache.Get( cacheKey ) as SystemCommunication;
            if ( systemCommunication != null )
            {
                return systemCommunication;
            }

            systemCommunication = new SystemCommunicationService( rockContext ).GetNoTracking( _systemCommunicationGuid );
            if ( systemCommunication == null )
            {
                // Only log an exception the first time this is encountered, so we don't pollute the exception log.
                if ( !HasLoggedMissingSystemCommunicationException )
                {
                    lock ( _missingSystemCommunicationExceptionLock )
                    {
                        if ( !HasLoggedMissingSystemCommunicationException )
                        {
                            ExceptionLogService.LogException( $"'{nameof( SendFallbackChatNotification ).SplitCase()}' Automation Event with ID {_automationEventId} is referencing a System Communication that cannot be found (System Communication Guid: '{_systemCommunicationGuid}')." );

                            HasLoggedMissingSystemCommunicationException = true;
                        }
                    }
                }
            }

            if ( systemCommunication != null )
            {
                RockCache.AddOrUpdate( cacheKey, null, systemCommunication, RockDateTime.Now.AddSeconds( 300 ) );
            }

            return systemCommunication;
        }

        #endregion
    }
}
