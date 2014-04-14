using System;
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An SMS communication
    /// </summary>
    [Description( "An SMS communication" )]
    [Export( typeof( ChannelComponent ) )]
    [ExportMetadata( "ComponentName", "SMS" )]
    public class Sms : ChannelComponent
    {
        const int TOKEN_REUSE_DURATION = 30; // number of days between token reuse
        
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override ChannelControl Control
        {
            get { return new Rock.Web.UI.Controls.Communication.Sms(); }
        }

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            var rockContext = new RockContext();

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            var mergeValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

            if ( person != null )
            {
                mergeValues.Add( "Person", person );

                var recipient = communication.Recipients.Where( r => r.PersonId == person.Id ).FirstOrDefault();
                if ( recipient != null )
                {
                    // Add any additional merge fields created through a report
                    foreach ( var mergeField in recipient.AdditionalMergeValues )
                    {
                        if ( !mergeValues.ContainsKey( mergeField.Key ) )
                        {
                            mergeValues.Add( mergeField.Key, mergeField.Value );
                        }
                    }
                }
            }

            string message = communication.GetChannelDataValue( "Message" );
            return message.ResolveMergeFields( mergeValues );
        }

        /// <summary>
        /// Process inbound messages that are sent to a SMS number.
        /// </summary>
        /// <param name="toPhone">The phone number a message is sent to.</param>
        /// <param name="fromPhone">The phone number a message is sent from.</param>
        /// <param name="message">The message that was sent.</param>
        /// <returns></returns>
        public void ProcessResponse( string toPhone, string fromPhone, string message )
        {
            int toPersonId = -1;
            string transportPhone = string.Empty;

            Rock.Data.RockContext rockContext = new Rock.Data.RockContext();

            // get from person
            var fromPerson = new PersonService( rockContext ).Queryable()
                                .Where( p => p.PhoneNumbers.Any( n => (n.CountryCode + n.Number) == fromPhone.Replace( "+", "" ) ) )
                                .OrderBy( p => p.Id ).FirstOrDefault(); // order by person id to get the oldest person to help with duplicate records of the response recipient

            // get recipient from defined value
            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );
            if ( definedType != null )
            {
                if ( definedType.DefinedValues != null && definedType.DefinedValues.Any() )
                {
                    var matchValue = definedType.DefinedValues.Where( v => v.Name == toPhone ).OrderBy( v => v.Order ).FirstOrDefault();
                    if ( matchValue != null )
                    {
                        var toPersonGuid = matchValue.GetAttributeValue( "ResponseRecipient" );
                        transportPhone = matchValue.Id.ToString();

                        if ( toPersonGuid != null )
                        {
                            var toPerson = new PersonAliasService( rockContext ).Get( new Guid( toPersonGuid ) );
                            toPersonId = toPerson.PersonId;
                        }
                    }
                }
            }

            if ( fromPerson != null && toPersonId != -1 )
            {
                if ( toPersonId == fromPerson.Id ) // message from the channel recipient
                {
                    // look for response code in the message
                    Match match = Regex.Match( message, @"@\d{3}" );
                    if ( match.Success )
                    {
                        string responseCode = match.ToString();

                        var recipient = new CommunicationRecipientService( rockContext ).Queryable("Communication")
                                            .Where( r => r.UniqueMessageId == responseCode )
                                            .OrderByDescending(r => r.CreatedDateTime).FirstOrDefault();

                        if ( recipient != null )
                        {
                            CreateCommunication( fromPerson.Id, fromPerson.FullName, recipient.Communication.SenderPersonId.Value, message.Replace(responseCode, ""), transportPhone, "", rockContext );
                        }
                        else // send a warning message back to the channel recipient
                        {
                            string warningMessage = string.Format( "A conversation could not be found with the response token {0}.", responseCode );
                            CreateCommunication( fromPerson.Id, fromPerson.FullName, fromPerson.Id, warningMessage, transportPhone, "", rockContext );
                        }
                    }
                }
                else // response from someone other than the channel recipient
                {
                    string messageId = GenerateMessageToken( rockContext );
                    message = string.Format( "-{0}-\n{1}\n( {2} )", fromPerson.FullName, message, messageId );
                    CreateCommunication( fromPerson.Id, fromPerson.FullName, toPersonId, message, transportPhone, messageId, rockContext );
                }
                
                
            }
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        /// <param name="fromPersonId">Person ID of the sender.</param>
        /// <param name="fromName">The name of the sener.</param>
        /// <param name="toPersonId">The Person ID of the recipient.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="responseCode">The reponseCode to use for tracking the conversation.</param>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <returns></returns>
        private void CreateCommunication( int fromPersonId, string fromPersonName, int toPersonId, string message, string transportPhone, string responseCode, Rock.Data.RockContext rockContext )
        {

            // add communication for reply
            var communication = new Rock.Model.Communication();
            communication.SetChannelDataValue( "BulkEmail", "false" );
            communication.Status = CommunicationStatus.Approved;
            communication.SenderPersonId = fromPersonId;
            communication.Subject = string.Format( "From: {0}", fromPersonName );

            communication.SetChannelDataValue( "Message", message );
            communication.SetChannelDataValue( "FromValue", transportPhone );

            communication.ChannelEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Channel.Sms" ).Id;

            var recipient = new Rock.Model.CommunicationRecipient();
            recipient.Status = CommunicationRecipientStatus.Pending;
            recipient.PersonId = toPersonId;
            recipient.UniqueMessageId = responseCode;
            communication.Recipients.Add( recipient );

            var communicationService = new Rock.Model.CommunicationService( rockContext );
            communicationService.Add( communication );
            rockContext.SaveChanges();

            // queue the sending
            var transaction = new Rock.Transactions.SendCommunicationTransaction();
            transaction.CommunicationId = communication.Id;
            transaction.PersonAlias = null;
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
        }

        /// <summary>
        /// Creates a recipient token to help track conversations.
        /// </summary>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <returns>String token</returns>
        private string GenerateMessageToken( Rock.Data.RockContext rockContext )
        {
            bool isUnique = false;
            int randomNumber = -1;
            DateTime tokenStartDate = RockDateTime.Now.Subtract( new TimeSpan( TOKEN_REUSE_DURATION, 0, 0, 0 ) );

            Random rnd = new Random();

            while ( isUnique == false )
            {
                randomNumber = rnd.Next( 100, 1000 );

                if ( randomNumber != 666 ) // just because
                {

                    // check if token has been used recently
                    var communication = new CommunicationRecipientService( rockContext ).Queryable()
                                            .Where( c => c.UniqueMessageId == "@" + randomNumber.ToString() && c.CreatedDateTime > tokenStartDate )
                                            .FirstOrDefault();
                    if ( communication == null )
                    {
                        isUnique = true;
                    }
                }
            }

            return "@" + randomNumber.ToString();
        }

    }
}
