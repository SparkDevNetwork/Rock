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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Attribute;
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
    [IntegerField( "Character Limit", "Set this to show a character limit countdown for SMS communications. Set to 0 to disable", false, 160 )]
    public class Sms : MediumComponent
    {
        const int TOKEN_REUSE_DURATION = 30; // number of days between token reuse

        /// <summary>
        /// Gets the type of the communication.
        /// </summary>
        /// <value>
        /// The type of the communication.
        /// </value>
        public override CommunicationType CommunicationType { get { return CommunicationType.SMS; } }

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            var smsControl = new Web.UI.Controls.Communication.Sms();
            smsControl.CharacterLimit = this.GetAttributeValue( "CharacterLimit" ).AsIntegerOrNull() ?? 160;
            return smsControl;
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

            using ( var rockContext = new RockContext() )
            {
                Person toPerson = null;

                var mobilePhoneNumberValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                // get from person
                var fromPerson = new PersonService( rockContext ).Queryable()
                    .Where( p => p.PhoneNumbers.Any( n => ( n.CountryCode + n.Number ) == fromPhone.Replace( "+", "" ) && n.NumberTypeValueId == mobilePhoneNumberValueId ) )
                    .OrderBy( p => p.Id ).FirstOrDefault(); // order by person id to get the oldest person to help with duplicate records of the response recipient

                // get recipient from defined value
                var fromPhoneDv = FindFromPhoneDefinedValue( toPhone );
                if ( fromPhoneDv != null )
                {
                    var toPersonAliasGuid = fromPhoneDv.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
                    if ( toPersonAliasGuid.HasValue )
                    {
                        toPerson = new PersonAliasService( rockContext )
                            .Queryable().Where( p => p.Guid.Equals( toPersonAliasGuid.Value ) )
                            .Select( p => p.Person )
                            .FirstOrDefault();
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

                            var recipient = new CommunicationRecipientService( rockContext ).Queryable( "Communication" )
                                                .Where( r => r.ResponseCode == responseCode )
                                                .OrderByDescending( r => r.CreatedDateTime ).FirstOrDefault();

                            if ( recipient != null && recipient.Communication.SenderPersonAliasId.HasValue )
                            {
                                CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, recipient.Communication.SenderPersonAliasId.Value, message.Replace( responseCode, "" ), fromPhoneDv, "", rockContext );
                            }
                            else // send a warning message back to the medium recipient
                            {
                                string warningMessage = string.Format( "A conversation could not be found with the response token {0}.", responseCode );
                                CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, fromPerson.PrimaryAliasId.Value, warningMessage, fromPhoneDv, "", rockContext );
                            }
                        }
                    }
                    else // response from someone other than the medium recipient
                    {
                        string messageId = GenerateResponseCode( rockContext );
                        message = string.Format( "-{0}-\n{1}\n( {2} )", fromPerson.FullName, message, messageId );
                        CreateCommunication( fromPerson.PrimaryAliasId.Value, fromPerson.FullName, toPerson.PrimaryAliasId.Value, message, fromPhoneDv, messageId, rockContext );
                    }
                }
                else
                {
                    var globalAttributes = GlobalAttributesCache.Read();
                    string organizationName = globalAttributes.GetValue( "OrganizationName" );

                    errorMessage = string.Format( "Could not deliver message. This phone number is not registered in the {0} database.", organizationName );
                }
            }
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="responseCode">The reponseCode to use for tracking the conversation.</param>
        /// <param name="rockContext">A context to use for database calls.</param>
        private void CreateCommunication( int fromPersonAliasId, string fromPersonName, int toPersonAliasId, string message, DefinedValueCache fromPhone, string responseCode, Rock.Data.RockContext rockContext )
        {
            // add communication for reply
            var communication = new Rock.Model.Communication();
            communication.Name = string.Format( "From: {0}", fromPersonName );
            communication.CommunicationType = CommunicationType.SMS;
            communication.SenderPersonAliasId = fromPersonAliasId;
            communication.IsBulkCommunication = false;
            communication.Status = CommunicationStatus.Approved;
            communication.SMSMessage = message;
            communication.SMSFromDefinedValueId = fromPhone.Id;

            var recipient = new Rock.Model.CommunicationRecipient();
            recipient.Status = CommunicationRecipientStatus.Pending;
            recipient.PersonAliasId = toPersonAliasId;
            recipient.ResponseCode = responseCode;
            recipient.MediumEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Medium.Sms" ).Id;
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

        /// <summary>
        /// Finds from phone defined value.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public static DefinedValueCache FindFromPhoneDefinedValue( string phoneNumber )
        {
            var definedType = DefinedTypeCache.Read( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );
            if ( definedType != null )
            {
                if ( definedType.DefinedValues != null && definedType.DefinedValues.Any() )
                {
                    return definedType.DefinedValues.Where( v => v.Value.RemoveSpaces() == phoneNumber.RemoveSpaces() ).OrderBy( v => v.Order ).FirstOrDefault();
                }
            }

            return null;
        }


        #region Obsolete 

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [Obsolete( "The GetCommunication now creates the HTML Preview directly" )]
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        [Obsolete( "The CommunicationDetail block now creates the details" )]
        public override string GetMessageDetails( Model.Communication communication )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "All mediums now support bulk communications" )]
        public override bool SupportsBulkCommunication
        {
            get
            {
                return true;
            }
        }

        #endregion

    }
}
