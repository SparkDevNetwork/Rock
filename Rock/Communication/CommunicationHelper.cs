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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Communication
{
    /// <summary>
    /// This class is used to help consolidate the sending of communications.
    /// </summary>
    public class CommunicationHelper
    {
        /// <summary>
        /// Sends the message to a person via the specified medium type using the system communication.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="mediumType">Type of the medium.</param>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        public static SendMessageResult SendMessage( Person person, int mediumType, SystemCommunication systemCommunication, Dictionary<string, object> mergeObjects )
        {
            var results = new SendMessageResult();
            CreateMessageResult createMessageResults;
            switch ( mediumType )
            {
                case ( int ) CommunicationType.SMS:
                    createMessageResults = CreateSmsMessage( person, mergeObjects, systemCommunication );
                    break;
                default:
                    createMessageResults = CreateEmailMessage( person, mergeObjects, systemCommunication );
                    break;
            }

            if ( createMessageResults.Message == null )
            {
                results.Warnings.AddRange( createMessageResults.Warnings );
                return results;
            }

            if ( createMessageResults.Message.Send( out var errors ) )
            {
                results.MessagesSent = 1;
            }
            else
            {
                results.Errors.AddRange( errors );
            }

            return results;
        }

        /// <summary>
        /// Sends the message to a group using the system communication. Each person in the group receives the message in their preferred medium.
        /// A "Recipient" merge object will be added to the merge objects.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        public static SendMessageResult SendMessage( int groupId, SystemCommunication systemCommunication, Dictionary<string, object> mergeObjects )
        {
            var results = new SendMessageResult();

            using ( var rockContext = new RockContext() )
            {
                // The group members are the message recipients
                var groupMemberViews = new GroupMemberService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( m =>
                        m.GroupId == groupId &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.Person != null
                     )
                    .Select( m => new
                    {
                        Person = m.Person,
                        GroupCommunicationPreference = m.CommunicationPreference
                    } )
                    .Distinct()
                    .ToList();

                // Send the communication to each recipient in the group
                foreach ( var groupMemberView in groupMemberViews )
                {
                    var recipient = groupMemberView.Person;
                    mergeObjects["Recipient"] = recipient;

                    var mediumType = Rock.Model.Communication.DetermineMediumEntityTypeId(
                        ( int ) CommunicationType.Email,
                        ( int ) CommunicationType.SMS,
                        ( int ) CommunicationType.PushNotification,
                        groupMemberView.GroupCommunicationPreference,
                        groupMemberView.Person.CommunicationPreference );

                    var result = SendMessage( recipient, mediumType, systemCommunication, mergeObjects );

                    // Add this result to the set of results to return
                    results.Warnings.AddRange( result.Warnings );
                    results.Errors.AddRange( result.Errors );
                    results.MessagesSent += result.MessagesSent;
                }
            }

            return results;
        }

        private static CreateMessageResult CreateSmsMessage( Person person, Dictionary<string, object> mergeObjects, SystemCommunication systemCommunication )
        {
            var isSmsEnabled = MediumContainer.HasActiveSmsTransport() && !string.IsNullOrWhiteSpace( systemCommunication.SMSMessage );
            var createMessageResult = new CreateMessageResult();
            var smsNumber = person.PhoneNumbers.GetFirstSmsNumber();
            var recipients = new List<RockMessageRecipient>();

            if ( string.IsNullOrWhiteSpace( smsNumber ) || !isSmsEnabled )
            {
                var smsWarningMessage = $"No SMS number could be found for {person.FullName}.";
                if ( !isSmsEnabled )
                {
                    smsWarningMessage = $"SMS is not enabled. {person.FullName} did not receive a notification.";
                }

                RockLogger.Log.Warning( RockLogDomains.Jobs, smsWarningMessage );
                createMessageResult.Warnings.Add( smsWarningMessage );
                return createMessageResult;
            }

            recipients.Add( new RockSMSMessageRecipient( person, smsNumber, mergeObjects ) );

            var message = new RockSMSMessage( systemCommunication );
            message.SetRecipients( recipients );
            message.AdditionalMergeFields = mergeObjects;
            createMessageResult.Message = message;
            return createMessageResult;
        }

        internal static CreateMessageResult CreateEmailMessage( Person person, Dictionary<string, object> mergeObjects, SystemCommunication systemCommunication )
        {
            var createMessageResult = new CreateMessageResult();

            if ( string.IsNullOrWhiteSpace( person.Email ) )
            {
                var warning = $"{person.FullName} does not have an email address entered.";
                createMessageResult.Warnings.Add( warning );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                return createMessageResult;
            }

            if ( !person.IsEmailActive )
            {
                var warning = $"{person.FullName.ToPossessive()} email address is inactive.";
                createMessageResult.Warnings.Add( warning );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                return createMessageResult;
            }

            if ( person.EmailPreference == EmailPreference.DoNotEmail )
            {
                var warning = $"{person.FullName} is marked as do not email.";
                createMessageResult.Warnings.Add( warning );
                RockLogger.Log.Warning( RockLogDomains.Jobs, warning );
                return createMessageResult;
            }

            var recipients = new List<RockMessageRecipient>
            {
                new RockEmailMessageRecipient( person, mergeObjects )
            };

            var message = new RockEmailMessage( systemCommunication );
            message.SetRecipients( recipients );
            message.AdditionalMergeFields = mergeObjects;
            createMessageResult.Message = message;
            return createMessageResult;
        }
    }
}
