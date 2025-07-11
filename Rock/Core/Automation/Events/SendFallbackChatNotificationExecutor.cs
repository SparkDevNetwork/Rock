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
        public SendFallbackChatNotificationExecutor( int automationEventId, Guid systemCommunicationGuid, int notificationSuppressionMinutes )
        {
            _logger = RockLogger.LoggerFactory.CreateLogger<SendFallbackChatNotificationExecutor>();
            _automationEventId = automationEventId;
            _systemCommunicationGuid = systemCommunicationGuid;
            _notificationSuppressionMinutes = notificationSuppressionMinutes;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            var channel = request.Values[ChatMessage.AutomationRequestValueKey.Channel] as Group;
            var person = request.Values[ChatMessage.AutomationRequestValueKey.Person] as Person;
            var memberChatPersonKeys = request.Values[ChatMessage.AutomationRequestValueKey.MemberChatPersonKeys] as HashSet<string>;
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
                            MemberChatUserKeys = memberChatPersonKeys,
                            PersonIdToExclude = person?.Id,
                            SystemCommunicationId = systemCommunication.Id,
                            NotificationSuppressionMinutes = _notificationSuppressionMinutes
                        }
                    );

                    var structuredLog = "automation event ID {AutomationEventId}, group ID {GroupId}, chat message sender person ID {ChatMessageSenderPersonId} ({ChatMessageSenderPersonFullName})";

                    if ( fallbackNotificationPeople?.Any() != true )
                    {
                        _logger.LogDebug(
                            $"No channel members needing a fallback chat notification for {structuredLog}.",
                            _automationEventId,
                            channel?.Id,
                            person?.Id,
                            person?.FullName
                        );

                        return;
                    }

                    var eventCommunicationPreference = CommunicationType.RecipientPreference;
                    if ( !MediumContainer.HasActiveSmsTransport() || systemCommunication.SMSMessage.IsNullOrWhiteSpace() )
                    {
                        eventCommunicationPreference = CommunicationType.Email;
                    }

                    var sendMessageResult = new SendMessageResult();
                    var recipientStructuredLog = "fallback chat notification recipient person ID {FallbackRecipientPersonId} ({FallbackRecipientPersonFullName})";

                    foreach ( var fallbackNotificationPerson in fallbackNotificationPeople )
                    {
                        var recipientPerson = fallbackNotificationPerson.RecipientPerson;

                        var recipientCommunicationPreferences = new HashSet<CommunicationType>
                        {
                            eventCommunicationPreference
                        };

                        if ( fallbackNotificationPerson.GroupMemberCommunicationPreference.HasValue )
                        {
                            recipientCommunicationPreferences.Add( fallbackNotificationPerson.GroupMemberCommunicationPreference.Value );
                        }

                        recipientCommunicationPreferences.Add( recipientPerson.CommunicationPreference );

                        var mediumType = Model.Communication.DetermineMediumEntityTypeId(
                            ( int ) CommunicationType.Email,
                            ( int ) CommunicationType.SMS,
                            ( int ) CommunicationType.PushNotification,
                            recipientCommunicationPreferences.ToArray()
                        );

                        try
                        {
                            var sendResult = CommunicationHelper.SendMessage(
                                recipientPerson,
                                mediumType,
                                systemCommunication,
                                mergeFields
                            );

                            sendMessageResult.Warnings.AddRange( sendResult.Warnings );
                            sendMessageResult.Errors.AddRange( sendResult.Errors );
                            sendMessageResult.Exceptions.AddRange( sendResult.Exceptions );
                            sendMessageResult.MessagesSent += sendResult.MessagesSent;
                        }
                        catch ( Exception ex )
                        {
                            _logger.LogError(
                                ex,
                                $"Failed to send a fallback chat notification for {structuredLog}, {recipientStructuredLog}.",
                                _automationEventId,
                                channel?.Id,
                                person?.Id,
                                person?.FullName,
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
                        person?.Id,
                        person?.FullName,
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
