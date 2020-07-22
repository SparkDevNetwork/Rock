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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
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
        /// The highest value an SMS Response Code can contain. If you change this above 5 digits
        /// then you must also change the regular expression in the ProcessResponse method.
        /// </summary>
        private const int RESPONSE_CODE_MAX = 90000;

        /// <summary>
        /// Define a key to use in the cache for storing our available response code list.
        /// </summary>
        private const string RESPONSE_CODE_CACHE_KEY = "Rock:Communication:Sms:ResponseCodeCache";

        /// <summary>
        /// Used by the GenerateResponseCode method to ensure exclusive access to the cached
        /// available response code list.
        /// </summary>
        private static readonly object _responseCodesLock = new object();

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
        /// <param name="toPhone">The transport (e.g. Twilio) phone number a message is sent to.</param>
        /// <param name="fromPhone">The phone number a message is sent from. (This would be the number from somebody's mobile device) </param>
        /// <param name="message">The message that was sent.</param>
        /// <param name="errorMessage">The error message.</param>
        public void ProcessResponse( string toPhone, string fromPhone, string message, out string errorMessage )
        {
            errorMessage = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                // the person associated with the SMS Phone Defined Value
                Person toPerson = null;

                var mobilePhoneNumberValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                fromPhone = fromPhone.Replace( "+", "" );
                toPhone = toPhone.Replace( "+", "" );

                // Get the person who sent the message. This will always return a Person record since we want to get a nameless Person record if there isn't a regular person record found
                var fromPerson = new PersonService( rockContext ).GetPersonFromMobilePhoneNumber( fromPhone, true );

                // get recipient from defined value
                var rockSmsFromPhoneDv = FindRockSMSPhoneDefinedValue( toPhone );
                if ( rockSmsFromPhoneDv != null )
                {
                    // NOTE: As of 9.0 the SMS from number DefinedValue no longer has to have a person (toPersonAliasGuid) assigned to it.
                    var toPersonAliasGuid = rockSmsFromPhoneDv.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
                    if ( toPersonAliasGuid.HasValue )
                    {
                        toPerson = new PersonAliasService( rockContext )
                            .Queryable().Where( p => p.Guid.Equals( toPersonAliasGuid.Value ) )
                            .Select( p => p.Person )
                            .FirstOrDefault();
                    }
                }

                if ( rockSmsFromPhoneDv != null )
                {
                    string plainMessage = message;

                    if ( toPerson != null && fromPerson != null && toPerson.Id == fromPerson.Id )
                    {
                        // From and To person are the same person. For example, in an SMS Conversation, the SMS DefinedValue person replies to the conversation

                        // look for response code in the message
                        Match match = Regex.Match( message, @"@\d{3,5}" );
                        if ( match.Success )
                        {
                            string responseCode = match.ToString();

                            var recipient = new CommunicationRecipientService( rockContext ).Queryable( "Communication" )
                                                .Where( r => r.ResponseCode == responseCode && r.CreatedDateTime.HasValue )
                                                .OrderByDescending( r => r.CreatedDateTime ).FirstOrDefault();

                            if ( recipient != null && recipient.Communication.SenderPersonAliasId.HasValue )
                            {
                                CreateCommunication( fromPerson, fromPhone, recipient.Communication.SenderPersonAliasId.Value, message.Replace( responseCode, "" ), plainMessage, rockSmsFromPhoneDv, "", rockContext, out errorMessage );
                            }
                            else // send a warning message back to the medium recipient
                            {
                                string warningMessage = string.Format( "A conversation could not be found with the response token {0}.", responseCode );
                                CreateCommunication( fromPerson, fromPhone, fromPerson.PrimaryAliasId.Value, warningMessage, plainMessage, rockSmsFromPhoneDv, "", rockContext, out errorMessage );
                            }
                        }
                        else
                        {
                            errorMessage = "There was no response code (e.g., @321) found in the message and the from/to person were the same.";
                        }
                    }
                    else
                    {
                        // From and To person are not same person. For example, a person sent a message to the SMS PhoneNumber (DefinedValue). 
                        string messageId = GenerateResponseCode( rockContext );
                        int? toPersonPrimaryAliasId = toPerson?.PrimaryAliasId;

                        // NOTE: fromPerson will never be null since we would have created a Nameless Person record if there wasn't a regular person found
                        message = $"-{fromPerson.FullName}-\n{message}\n( {messageId} )";
                        CreateCommunication( fromPerson, fromPhone, toPersonPrimaryAliasId, message, plainMessage, rockSmsFromPhoneDv, messageId, rockContext, out errorMessage );

                    }
                }
                else
                {
                    var globalAttributes = GlobalAttributesCache.Get();
                    string organizationName = globalAttributes.GetValue( "OrganizationName" );

                    errorMessage = string.Format( "Could not deliver message. This phone number is not registered in the {0} database.", organizationName );
                }
            }
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        /// <param name="fromPerson">From person.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="plainMessage">The plain message.</param>
        /// <param name="rockSmsFromPhoneDv">From phone.</param>
        /// <param name="responseCode">The reponseCode to use for tracking the conversation.</param>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <param name="errorMessage">The error message.</param>
        private void CreateCommunication( Person fromPerson, string messageKey, int? toPersonAliasId, string message, string plainMessage, DefinedValueCache rockSmsFromPhoneDv, string responseCode, Rock.Data.RockContext rockContext, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                LaunchWorkflow( fromPerson?.PrimaryAliasId, messageKey, message, toPersonAliasId, rockSmsFromPhoneDv );

            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                // Log error and continue, don't stop because the workflow failed.
                ExceptionLogService.LogException( ex );
            }

            // See if this should go to a phone or to the DB. Default is to the phone so if for some reason we get a null here then just send it to the phone.
            var enableResponseRecipientForwarding = rockSmsFromPhoneDv.GetAttributeValue( "EnableResponseRecipientForwarding" ).AsBooleanOrNull() ?? true;

            // NOTE: FromPerson should never be null

            if ( enableResponseRecipientForwarding )
            {
                CreateCommunicationMobile( fromPerson, toPersonAliasId, message, rockSmsFromPhoneDv, responseCode, rockContext );
            }

            CreateCommunicationResponse( fromPerson, messageKey, toPersonAliasId, plainMessage, rockSmsFromPhoneDv, responseCode, rockContext );
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="message">The message.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="rockSmsFromPhoneDv">The rock SMS from phone DefinedValue.</param>
        private void LaunchWorkflow( int? fromPersonAliasId, string fromPhone, string message, int? toPersonAliasId, DefinedValueCache rockSmsFromPhoneDv )
        {
            var workflowTypeGuid = rockSmsFromPhoneDv.GetAttributeValue( "LaunchWorkflowOnResponseReceived" );
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );

            if ( workflowType == null || ( workflowType.IsActive != true ) )
            {
                return;
            }

            var personAliasService = new PersonAliasService( new RockContext() );
            var workflowAttributeValues = new Dictionary<string, string>();
            workflowAttributeValues.Add( "FromPhone", fromPhone );
            workflowAttributeValues.Add( "Message", message );
            workflowAttributeValues.Add( "SMSFromDefinedValue", rockSmsFromPhoneDv.Guid.ToString() );

            if ( fromPersonAliasId != null )
            {
                workflowAttributeValues.Add( "FromPerson", personAliasService.Get( fromPersonAliasId.Value ).Guid.ToString() ?? string.Empty );
            }

            if ( toPersonAliasId != null )
            {
                workflowAttributeValues.Add( "ToPerson", personAliasService.Get( toPersonAliasId.Value ).Guid.ToString() ?? string.Empty );
            }

            var launchWorkflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowType.Id );
            launchWorkflowTransaction.WorkflowAttributeValues = workflowAttributeValues;
            launchWorkflowTransaction.Enqueue();
        }

        /// <summary>
        /// Creates the CommunicationResponse for Rock SMS Conversations
        /// </summary>
        /// <param name="fromPerson">From person.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="rockSmsFromPhoneDv">From phone.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateCommunicationResponse( Person fromPerson, string messageKey, int? toPersonAliasId, string message, DefinedValueCache rockSmsFromPhoneDv, string responseCode, Rock.Data.RockContext rockContext )
        {
            var smsMedium = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS );

            if ( this.Transport == null )
            {
                throw new Exception( "Configuration Error. No SMS Transport Component is currently active." );
            }

            var smsTransport = this.Transport.EntityType.Id;
            int? communicationId = null;

            if ( fromPerson != null )
            {
                communicationId = GetCommunicationId( rockSmsFromPhoneDv, fromPerson.PrimaryAliasId.Value, 2 );
            }

            var communicationResponse = new CommunicationResponse
            {
                MessageKey = messageKey,
                FromPersonAliasId = fromPerson?.PrimaryAliasId,
                ToPersonAliasId = toPersonAliasId,
                IsRead = false,
                RelatedSmsFromDefinedValueId = rockSmsFromPhoneDv.Id,
                RelatedCommunicationId = communicationId,
                RelatedTransportEntityTypeId = smsTransport,
                RelatedMediumEntityTypeId = smsMedium.Id,
                Response = message
            };

            var communicationResposeService = new CommunicationResponseService( rockContext );
            communicationResposeService.Add( communicationResponse );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the latest communication ID for the SMSFromDefinedValueId to the recipient within daysPastToSearch to present
        /// </summary>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="daysPastToSearch">The days past to search.</param>
        /// <returns></returns>
        private int? GetCommunicationId( DefinedValueCache fromPhone, int fromPersonAliasId, int daysPastToSearch )
        {
            // This is the last communication
            using ( var rockContext = new RockContext() )
            {
                var recipientService = new CommunicationRecipientService( rockContext );
                var latestRecipientCommunication = recipientService
                    .Queryable()
                    .AsNoTracking()
                    .Where( r => r.PersonAliasId == fromPersonAliasId )
                    .Where( r => r.CreatedDateTime >= DbFunctions.AddDays( RockDateTime.Now, -daysPastToSearch ) )
                    .OrderByDescending( c => c.CreatedDateTime )
                    .FirstOrDefault();

                if ( latestRecipientCommunication == null )
                {
                    return null;
                }

                var communicationService = new CommunicationService( rockContext );
                var communication = communicationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( c => c.SMSFromDefinedValueId == fromPhone.Id )
                    .Where( c => c.Id == latestRecipientCommunication.Id )
                    .FirstOrDefault();

                if ( communication != null )
                {
                    return communication.Id;
                }

                return null;
            }
        }

        /// <summary>
        /// Creates the communication to the recipient's mobile device.
        /// </summary>
        /// <param name="fromPerson">From person.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void CreateCommunicationMobile( Person fromPerson, int? toPersonAliasId, string message, DefinedValueCache fromPhone, string responseCode, Rock.Data.RockContext rockContext )
        {
            // NOTE: fromPerson should never be null since a Nameless Person record should have been created if a regular person record wasn't found
            string communicationName = fromPerson != null ? string.Format( "From: {0}", fromPerson.FullName ) : "From: unknown person";

            var communicationService = new CommunicationService( rockContext );
            var communication = communicationService.CreateSMSCommunication( fromPerson, toPersonAliasId, message, fromPhone, responseCode, communicationName );
            rockContext.SaveChanges();

            // queue the sending
            var transaction = new Rock.Transactions.SendCommunicationTransaction();
            transaction.CommunicationId = communication.Id;
            transaction.PersonAlias = null;
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
        }

        /// <summary>
        /// Generate a randomized list of available response codes that can be used for SMS tracking.
        /// </summary>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <returns>A randomized <see cref="List{T}"/> of strings that are available for use.</returns>
        static private List<string> GenerateAvailableResponseCodeList( Rock.Data.RockContext rockContext )
        {
            DateTime tokenStartDate = RockDateTime.Now.Subtract( new TimeSpan( TOKEN_REUSE_DURATION, 0, 0, 0 ) );
            int[] blacklist = new int[] { 666, 911 };
            int chunkSize = 100;
            int smsEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;

            //
            // Generate a list of codes that are currently active in the database.
            //
            var activeCodes = new CommunicationRecipientService( rockContext ).Queryable()
                                    .Where( c => c.MediumEntityTypeId == smsEntityTypeId )
                                    .Where( c => System.Data.Entity.DbFunctions.Left( c.ResponseCode, 1 ) == "@")
                                    .Where( c => c.CreatedDateTime.HasValue && c.CreatedDateTime > tokenStartDate )
                                    .Select( c => c.ResponseCode )
                                    .ToList();

            //
            // Starting at code 100, try to generate a list of available codes in small chunks until
            // we have a list with at least 1 available code.
            //
            for ( int startValue = 100; startValue < RESPONSE_CODE_MAX - chunkSize; startValue += chunkSize )
            {
                var availableCodes = Enumerable.Range( startValue, chunkSize )
                    .Where( i => !blacklist.Contains( i ) )
                    .Select( i => string.Format( "@{0}", i ) )
                    .Where( c => !activeCodes.Contains( c ) )
                    .ToList();

                if ( availableCodes.Any() )
                {
                    return availableCodes.OrderBy( c => Guid.NewGuid() ).ToList();
                }
            }

            throw new Exception( "No available response codes." );
        }

        /// <summary>
        /// Creates a recipient token to help track conversations.
        /// </summary>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <returns>String token</returns>
        public static string GenerateResponseCode( Rock.Data.RockContext rockContext )
        {
            DateTime tokenStartDate = RockDateTime.Now.Subtract( new TimeSpan( TOKEN_REUSE_DURATION, 0, 0, 0 ) );
            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            lock ( _responseCodesLock )
            {
                var availableResponseCodes = RockCache.Get( RESPONSE_CODE_CACHE_KEY ) as List<string>;

                //
                // Try up to 1,000 times to find a code. This really should never go past the first
                // loop but we will give the benefit of the doubt in case a code is issued via SQL.
                //
                for ( int attempts = 0; attempts < 1000; attempts++ )
                {
                    if ( availableResponseCodes == null || !availableResponseCodes.Any() )
                    {
                        availableResponseCodes = GenerateAvailableResponseCodeList( rockContext );
                    }

                    var code = availableResponseCodes[0];
                    availableResponseCodes.RemoveAt( 0 );

                    //
                    // Verify that the code is still unused.
                    //
                    var isUsed = communicationRecipientService.Queryable()
                            .Where( c => c.ResponseCode == code )
                            .Where( c => c.CreatedDateTime.HasValue  && c.CreatedDateTime > tokenStartDate )
                            .Any();

                    if ( !isUsed )
                    {
                        RockCache.AddOrUpdate( RESPONSE_CODE_CACHE_KEY, availableResponseCodes );
                        return code;
                    }
                }
            }

            throw new Exception( "Could not find an available response code." );
        }

        /// <summary>
        /// Finds from phone defined value and ignores the plus sign.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public static DefinedValueCache FindRockSMSPhoneDefinedValue( string phoneNumber )
        {
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );
            if ( definedType != null )
            {
                if ( definedType.DefinedValues?.Any() == true )
                {
                    return definedType
                        .DefinedValues
                        .Where( v => v.Value.RemoveSpaces().Replace( "+", "" ) == phoneNumber.RemoveSpaces().Replace( "+", "" ) )
                        .OrderBy( v => v.Order )
                        .FirstOrDefault();
                }
            }

            return null;
        }

        /// <summary>
        /// Finds from phone defined value.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public static DefinedValueCache FindFromPhoneDefinedValue( string phoneNumber )
        {
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );
            if ( definedType != null )
            {
                if ( definedType.DefinedValues != null && definedType.DefinedValues.Any() )
                {
                    return definedType.DefinedValues.Where( v => v.Value.RemoveSpaces() == phoneNumber.RemoveSpaces() ).OrderBy( v => v.Order ).FirstOrDefault();
                }
            }

            return null;
        }
    }
}
