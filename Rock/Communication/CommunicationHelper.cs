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
using Rock.Logging;
using Rock.Model;

namespace Rock.Communication
{
    internal class CommunicationHelper
    {
        /// <summary>
        /// Sends the message to a person via the specified medium type using the system communication.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="mediumType">Type of the medium.</param>
        /// <param name="systemCommunication">The system communication.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <returns></returns>
        internal static SendMessageResult SendMessage( Person person, int mediumType, SystemCommunication systemCommunication, Dictionary<string, object> mergeObjects )
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
            createMessageResult.Message = message;
            return createMessageResult;
        }

        private static CreateMessageResult CreateEmailMessage( Person person, Dictionary<string, object> mergeObjects, SystemCommunication systemCommunication )
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
            createMessageResult.Message = message;
            return createMessageResult;
        }
    }
}
