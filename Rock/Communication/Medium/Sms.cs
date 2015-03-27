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
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Medium
{
    /// <summary>
    /// An SMS communication
    /// </summary>
    [Description( "An SMS communication" )]
    [Export( typeof( MediumComponent ) )]
    [ExportMetadata( "ComponentName", "SMS" )]
    public class Sms : MediumComponent
    {
        const int TOKEN_REUSE_DURATION = 30; // number of days between token reuse
        
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override MediumControl Control
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

                var recipient = communication.Recipients.Where( r => r.PersonAlias != null && r.PersonAlias.PersonId == person.Id ).FirstOrDefault();
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

            string message = communication.GetMediumDataValue( "Message" );
            return message.ResolveMergeFields( mergeValues );
        }

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        public override string GetMessageDetails( Model.Communication communication )
        {
            StringBuilder sb = new StringBuilder();

            AppendMediumData( communication, sb, "FromValue" );
            AppendMediumData( communication, sb, "Message" );

            return sb.ToString();
        }

        private void AppendMediumData( Model.Communication communication, StringBuilder sb, string key )
        {
            string value = communication.GetMediumDataValue( key );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                AppendMediumData( sb, key, value );
            }
        }

        private void AppendMediumData( StringBuilder sb, string key, string value )
        {
            sb.AppendFormat( "<div class='form-group'><label class='control-label'>{0}</label><p class='form-control-static'>{1}</p></div>",
                key.SplitCase(), value );
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void Send( Model.Communication communication )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            communication = communicationService.Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                // Update any recipients that should not get sent the communication
                var recipientService = new CommunicationRecipientService( rockContext );
                foreach ( var recipient in recipientService.Queryable( "PersonAlias.Person" )
                    .Where( r =>
                        r.CommunicationId == communication.Id &&
                        r.Status == CommunicationRecipientStatus.Pending )
                    .ToList() )
                {
                    var person = recipient.PersonAlias.Person;
                    if ( person.IsDeceased ?? false )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Person is deceased!";
                    }
                }

                rockContext.SaveChanges();
            }

            base.Send( communication );
        }

        /// <summary>
        /// Process inbound messages that are sent to a SMS number.
        /// </summary>
        /// <param name="toPhone">The phone number a message is sent to.</param>
        /// <param name="fromPhone">The phone number a message is sent from.</param>
        /// <param name="message">The message that was sent.</param>
        /// <param name="errorMessage">The error message.</param>
        public void ProcessResponse( string toPhone, string fromPhone, string message, out string errorMessage )
        {
            errorMessage = string.Empty;
            
            string transportPhone = string.Empty;

            Rock.Data.RockContext rockContext = new Rock.Data.RockContext();

            Person toPerson = null;

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
                    var matchValue = definedType.DefinedValues.Where( v => v.Value == toPhone ).OrderBy( v => v.Order ).FirstOrDefault();
                    if ( matchValue != null )
                    {
                        transportPhone = matchValue.Id.ToString();
                        var toPersonAliasGuid = matchValue.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
                        if ( toPersonAliasGuid.HasValue )
                        {
                            toPerson = new PersonAliasService( rockContext )
                                .Queryable().Where( p => p.Guid.Equals( toPersonAliasGuid.Value ) )
                                .Select( p => p.Person )
                                .FirstOrDefault();
                        }
                    }
                }
            }

            if ( fromPerson != null && toPerson != null && fromPerson.PrimaryAliasId.HasValue && toPerson.PrimaryAliasId.HasValue )
            {
                if ( toPerson.Id == fromPerson.Id ) // message from the medium recipient
                {
                    // look for response code in the message
                    Match match = Regex.Match( message, @"@\d{3}" );
                    if ( match.Success )
                    {
                        string responseCode = match.ToString();

                        var recipient = new CommunicationRecipientService( rockContext ).Queryable("Communication")
                                            .Where( r => r.ResponseCode == responseCode )
                                            .OrderByDescending(r => r.CreatedDateTime).FirstOrDefault();

                        if ( recipient != null && recipient.Communication.SenderPersonAliasId.HasValue )
                        {
                            CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, recipient.Communication.SenderPersonAliasId.Value, message.Replace(responseCode, ""), transportPhone, "", rockContext );
                        }
                        else // send a warning message back to the medium recipient
                        {
                            string warningMessage = string.Format( "A conversation could not be found with the response token {0}.", responseCode );
                            CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, fromPerson.PrimaryAliasId.Value, warningMessage, transportPhone, "", rockContext );
                        }
                    }
                }
                else // response from someone other than the medium recipient
                {
                    string messageId = GenerateResponseCode( rockContext );
                    message = string.Format( "-{0}-\n{1}\n( {2} )", fromPerson.FullName, message, messageId );
                    CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, toPerson.PrimaryAliasId.Value, message, transportPhone, messageId, rockContext );
                }
            }
            else
            {
                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                string organizationName = globalAttributes.GetValue( "OrganizationName" );

                errorMessage = string.Format( "Could not deliver message. This phone number is not registered in the {0} database.", organizationName);
            }
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="transportPhone">The transport phone.</param>
        /// <param name="responseCode">The reponseCode to use for tracking the conversation.</param>
        /// <param name="rockContext">A context to use for database calls.</param>
        private void CreateCommunication( int fromPersonAliasId, string fromPersonName, int toPersonAliasId, string message, string transportPhone, string responseCode, Rock.Data.RockContext rockContext )
        {

            // add communication for reply
            var communication = new Rock.Model.Communication();
            communication.IsBulkCommunication = false;
            communication.Status = CommunicationStatus.Approved;
            communication.SenderPersonAliasId = fromPersonAliasId;
            communication.Subject = string.Format( "From: {0}", fromPersonName );

            communication.SetMediumDataValue( "Message", message );
            communication.SetMediumDataValue( "FromValue", transportPhone );

            communication.MediumEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Medium.Sms" ).Id;

            var recipient = new Rock.Model.CommunicationRecipient();
            recipient.Status = CommunicationRecipientStatus.Pending;
            recipient.PersonAliasId = toPersonAliasId;
            recipient.ResponseCode = responseCode;
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
        private string GenerateResponseCode( Rock.Data.RockContext rockContext )
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
                                            .Where( c => c.ResponseCode == "@" + randomNumber.ToString() && c.CreatedDateTime > tokenStartDate )
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
