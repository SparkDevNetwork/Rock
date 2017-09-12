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
using System.Linq;
using System.Net.Mail;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using Twilio;
using TwilioTypes = Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending SMS messages using Twilio
    /// </summary>
    [Description( "Sends a communication through Twilio API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Twilio" )]
    [TextField( "SID", "Your Twilio Account SID (find at https://www.twilio.com/user/account)", true, "", "", 0 )]
    [TextField( "Token", "Your Twilio Account Token", true, "", "", 1 )]
    public class Twilio : TransportComponent
    {
        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var smsMessage = rockMessage as RockSMSMessage;
            if ( smsMessage != null )
            {
                // Validate From Number
                if ( smsMessage.FromNumber == null )
                {
                    errorMessages.Add( "A From Number was not provided." );
                    return false;
                }

                string accountSid = GetAttributeValue( "SID" );
                string authToken = GetAttributeValue( "Token" );
                TwilioClient.Init( accountSid, authToken );

                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                foreach ( var recipientData in rockMessage.GetRecipientData() )
                {
                    try
                    {
                        foreach ( var mergeField in mergeFields )
                        {
                            recipientData.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
                        }

                        string message = ResolveText( smsMessage.Message, smsMessage.CurrentPerson, smsMessage.EnabledLavaCommands, recipientData.MergeFields, smsMessage.AppRoot, smsMessage.ThemeRoot );

                        var response = MessageResource.Create(
                            from: new TwilioTypes.PhoneNumber( smsMessage.FromNumber.Value ),
                            to: new TwilioTypes.PhoneNumber( recipientData.To ),
                            body: message
                        );

                        if ( response.ErrorMessage.IsNotNullOrWhitespace() )
                        {
                            errorMessages.Add( response.ErrorMessage );
                        }
                    }
                    catch ( Exception ex )
                    {
                        errorMessages.Add( ex.Message );
                        ExceptionLogService.LogException( ex );
                    }
                }
            }

            return !errorMessages.Any();
        }


        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        public override void Send( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var communicationRockContext = new RockContext() )
            {
                // Requery the Communication
                communication = new CommunicationService( communicationRockContext ).Get( communication.Id );

                bool hasPendingRecipients;
                if ( communication != null &&
                    communication.Status == Model.CommunicationStatus.Approved &&
                    ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
                {
                    var qryRecipients = new CommunicationRecipientService( communicationRockContext ).Queryable();
                    hasPendingRecipients = qryRecipients
                        .Where( r =>
                            r.CommunicationId == communication.Id &&
                            r.Status == Model.CommunicationRecipientStatus.Pending &&
                            r.MediumEntityTypeId.HasValue &&
                            r.MediumEntityTypeId.Value == mediumEntityTypeId )
                        .Any();
                }
                else
                {
                    hasPendingRecipients = false;
                }

                if ( hasPendingRecipients )
                {
                    var currentPerson = communication.CreatedByPersonAlias.Person;
                    var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                    string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                    string fromPhone = communication.SMSFromDefinedValue?.Value;
                    if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                    {
                        string accountSid = GetAttributeValue( "SID" );
                        string authToken = GetAttributeValue( "Token" );
                        TwilioClient.Init( accountSid, authToken );

                        var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                        var communicationEntityTypeId = EntityTypeCache.Read( "Rock.Model.Communication" ).Id;
                        var communicationCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), communicationRockContext ).Id;

                        string callbackUrl = globalAttributes.GetValue( "PublicApplicationRoot" ) + "Webhooks/Twilio.ashx";

                        bool recipientFound = true;
                        while ( recipientFound )
                        {
                            // make a new rockContext per recipient
                            var recipientRockContext = new RockContext();
                            var recipient = Model.Communication.GetNextPending( communication.Id, mediumEntityTypeId, recipientRockContext );
                            if ( recipient != null )
                            {
                                if ( ValidRecipient( recipient, communication.IsBulkCommunication ) )
                                {
                                    try
                                    {
                                        var phoneNumber = recipient.PersonAlias.Person.PhoneNumbers
                                            .Where( p => p.IsMessagingEnabled )
                                            .FirstOrDefault();

                                        if ( phoneNumber != null )
                                        {
                                            // Create merge field dictionary
                                            var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                                            string message = ResolveText( communication.SMSMessage, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                                            string twilioNumber = phoneNumber.Number;
                                            if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
                                            {
                                                twilioNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
                                            }

                                            var response = MessageResource.Create(
                                                from: new TwilioTypes.PhoneNumber( fromPhone ),
                                                to: new TwilioTypes.PhoneNumber( twilioNumber ),
                                                body: message,
                                                statusCallback: new System.Uri( callbackUrl )
                                            );

                                            recipient.Status = CommunicationRecipientStatus.Delivered;
                                            recipient.TransportEntityTypeName = this.GetType().FullName;
                                            recipient.UniqueMessageId = response.Sid;

                                            try
                                            {
                                                var historyService = new HistoryService( recipientRockContext );
                                                historyService.Add( new History
                                                {
                                                    CreatedByPersonAliasId = communication.SenderPersonAliasId,
                                                    EntityTypeId = personEntityTypeId,
                                                    CategoryId = communicationCategoryId,
                                                    EntityId = recipient.PersonAlias.PersonId,
                                                    Summary = "Sent SMS message.",
                                                    Caption = message.Truncate( 200 ),
                                                    RelatedEntityTypeId = communicationEntityTypeId,
                                                    RelatedEntityId = communication.Id
                                                } );
                                            }
                                            catch ( Exception ex )
                                            {
                                                ExceptionLogService.LogException( ex, null );
                                            }

                                        }
                                        else
                                        {
                                            recipient.Status = CommunicationRecipientStatus.Failed;
                                            recipient.StatusNote = "No Phone Number with Messaging Enabled";
                                        }
                                    }
                                    catch ( Exception ex )
                                    {
                                        recipient.Status = CommunicationRecipientStatus.Failed;
                                        recipient.StatusNote = "Twilio Exception: " + ex.Message;
                                    }
                                }

                                recipientRockContext.SaveChanges();
                            }
                            else
                            {
                                recipientFound = false;
                            }
                        }
                    }
                }
            }
        }

        #region Obsolete

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( Communication communication, Dictionary<string, string> mediumAttributes ) instead" )]
        public override void Send( Model.Communication communication )
        {
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( communication, mediumEntityId, null );
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot)
        {
            var message = new RockSMSMessage();
            message.FromNumber = DefinedValueCache.Read( ( mediumData.GetValueOrNull( "FromValue" ) ?? string.Empty ).AsInteger() );
            message.SetRecipients( recipients );
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null )
        {
            var message = new RockSMSMessage();
            message.FromNumber = DefinedValueCache.Read( from.AsInteger() );
            if ( message.FromNumber == null )
            {
                message.FromNumber = DefinedTypeCache.Read( SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() )
                    .DefinedValues
                    .Where( v => v.Value == from )
                    .FirstOrDefault();
            }
            message.SetRecipients( recipients );
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Read( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">Attachments.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">The attachments.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
