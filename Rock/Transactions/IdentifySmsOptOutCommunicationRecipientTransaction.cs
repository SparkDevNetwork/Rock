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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Observability;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Represents a transaction for attempting to identify the <see cref="CommunicationRecipient"/> record associated
    /// with an SMS opt-out event and updating that record with the details of the opt-out.
    /// </summary>
    /// <seealso cref="ITransaction" />
    public class IdentifySmsOptOutCommunicationRecipientTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the number that the opt-out message was sent from.
        /// </summary>
        public string FromNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifySmsOptOutCommunicationRecipientTransaction"/> class.
        /// </summary>
        /// <param name="fromNumber">The number that the opt-out message was sent from.</param>
        public IdentifySmsOptOutCommunicationRecipientTransaction( string fromNumber )
        {
            FromNumber = fromNumber;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            if ( FromNumber.IsNullOrWhiteSpace() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            using ( var activity = ObservabilityHelper.StartActivity( "SMS: Identify SMS Opt-Out Communication Recipient" ) )
            {
                activity?.AddTag( "rock.sms.from.number", FromNumber );

                // Query for all people to whom this phone number belongs.
                var cleanedFromNumber = PhoneNumber.CleanNumber( FromNumber );
                var personIdsQry = new PhoneNumberService( rockContext )
                    .Queryable()
                    .Where( p =>
                        p.Number == cleanedFromNumber
                        || p.Number == FromNumber
                        || p.FullNumber == cleanedFromNumber
                        || p.FullNumber == FromNumber
                    )
                    .Select( p => p.PersonId );

                // Query for all matching person alias IDs.
                var personAliasIdsQry = new PersonAliasService( rockContext )
                    .Queryable()
                    .Where( a => personIdsQry.Contains( a.PersonId ) )
                    .Select( a => a.Id );

                // Get the most recent SMS communication recipient record within the last 60 days (for all people who
                // might share this number) and choose that as the record to update with the unsubscribe details. This
                // is a best effort approach to try to associate the opt-out with a specific communication, but it may
                // not always be 100% correct.
                var smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
                if ( !smsMediumEntityTypeId.HasValue )
                {
                    // Should never happen.
                    return;
                }

                var now = RockDateTime.Now;
                var sixtyDaysAgo = now.AddDays( -60 ).Date;

                var communicationRecipient = new CommunicationRecipientService( rockContext )
                    .Queryable()
                    .Where( r =>
                        r.MediumEntityTypeId == smsMediumEntityTypeId.Value
                        && r.Status == CommunicationRecipientStatus.Delivered
                        && r.PersonAliasId.HasValue
                        && personAliasIdsQry.Contains( r.PersonAliasId.Value )
                        && r.SendDateTime.HasValue
                        && DbFunctions.TruncateTime( r.SendDateTime.Value ) >= sixtyDaysAgo
                    )
                    .OrderByDescending( r => r.SendDateTime )
                    .ThenByDescending( r => r.Id )
                    .FirstOrDefault();

                if ( communicationRecipient == null || communicationRecipient.UnsubscribeDateTime.HasValue )
                {
                    return;
                }

                communicationRecipient.UnsubscribeDateTime = now;
                communicationRecipient.UnsubscribeLevel = UnsubscribeLevel.All;

                rockContext.SaveChanges();
            }
        }
    }
}
