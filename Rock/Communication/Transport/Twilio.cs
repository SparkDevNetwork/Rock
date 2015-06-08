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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using Twilio;

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
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication )
        {
            var rockContext = new RockContext();

            // Requery the Communication
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                string fromPhone = string.Empty;
                string fromValue = communication.GetMediumDataValue( "FromValue" );
                int fromValueId = int.MinValue;
                if ( int.TryParse( fromValue, out fromValueId ) )
                {
                    fromPhone = DefinedValueCache.Read( fromValueId, rockContext ).Value;
                }

                if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                {
                    string accountSid = GetAttributeValue( "SID" );
                    string authToken = GetAttributeValue( "Token" );
                    var twilio = new TwilioRestClient( accountSid, authToken );

                    var historyService = new HistoryService( rockContext );
                    var recipientService = new CommunicationRecipientService( rockContext );

                    var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                    var communicationEntityTypeId = EntityTypeCache.Read( "Rock.Model.Communication" ).Id;
                    var communicationCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), rockContext ).Id;

                    var globalConfigValues = GlobalAttributesCache.GetMergeFields( null );

                    bool recipientFound = true;
                    while ( recipientFound )
                    {
                        var recipient = recipientService.Get( communication.Id, CommunicationRecipientStatus.Pending ).FirstOrDefault();
                        if ( recipient != null )
                        {
                            try
                            {
                                var phoneNumber = recipient.PersonAlias.Person.PhoneNumbers
                                    .Where( p => p.IsMessagingEnabled )
                                    .FirstOrDefault();

                                if ( phoneNumber != null )
                                {
                                    // Create merge field dictionary
                                    var mergeObjects = recipient.CommunicationMergeValues( globalConfigValues );
                                    string message = communication.GetMediumDataValue( "Message" );

                                    // convert any special microsoft word characters to normal chars so they don't look funny (for example "Hey â€œdouble-quotesâ€ from â€˜single quoteâ€™")
                                    message = message.ReplaceWordChars();
                                    message = message.ResolveMergeFields( mergeObjects );
 
                                    string twilioNumber = phoneNumber.Number;
                                    if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
                                    {
                                        twilioNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
                                    }

                                    var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                                    string callbackUrl = globalAttributes.GetValue( "PublicApplicationRoot" ) + "Webhooks/Twilio.ashx";

                                    var response = twilio.SendMessage( fromPhone, twilioNumber, message, callbackUrl );

                                    recipient.Status = CommunicationRecipientStatus.Delivered;
                                    recipient.TransportEntityTypeName = this.GetType().FullName;
                                    recipient.UniqueMessageId = response.Sid;

                                    try
                                    {
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
                                    catch (Exception ex)
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

                            rockContext.SaveChanges();
                        }
                        else
                        {
                            recipientFound = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <exception cref="System.NotImplementedException"></exception>
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
        public override void Send(Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot)
        {
            try
            {
                var globalAttributes = GlobalAttributesCache.Read();

                string fromPhone = string.Empty;
                string fromValue = string.Empty;
                mediumData.TryGetValue( "FromValue", out fromValue );
                if (!string.IsNullOrWhiteSpace(fromValue))
                {
                    fromPhone = DefinedValueCache.Read( fromValue.AsInteger() ).Value;
                    if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                    {
                        string accountSid = GetAttributeValue( "SID" );
                        string authToken = GetAttributeValue( "Token" );
                        var twilio = new TwilioRestClient( accountSid, authToken );

                        string message = string.Empty;
                        mediumData.TryGetValue( "Message", out message );

                        if ( !string.IsNullOrWhiteSpace( themeRoot ) )
                        {
                            message = message.Replace( "~~/", themeRoot );
                        }

                        if ( !string.IsNullOrWhiteSpace( appRoot ) )
                        {
                            message = message.Replace( "~/", appRoot );
                            message = message.Replace( @" src=""/", @" src=""" + appRoot );
                            message = message.Replace( @" href=""/", @" href=""" + appRoot );
                        }

                        foreach (var recipient in recipients)
                        {
                            var response = twilio.SendMessage( fromPhone, recipient, message );
                        }
                    }
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
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
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null )
        {
            try
            {
                var globalAttributes = GlobalAttributesCache.Read();

                string fromPhone = from;
                if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                {
                    string accountSid = GetAttributeValue( "SID" );
                    string authToken = GetAttributeValue( "Token" );
                    var twilio = new TwilioRestClient( accountSid, authToken );

                    string message = body;
                    if ( !string.IsNullOrWhiteSpace( themeRoot ) )
                    {
                        message = message.Replace( "~~/", themeRoot );
                    }

                    if ( !string.IsNullOrWhiteSpace( appRoot ) )
                    {
                        message = message.Replace( "~/", appRoot );
                        message = message.Replace( @" src=""/", @" src=""" + appRoot );
                        message = message.Replace( @" href=""/", @" href=""" + appRoot );
                    }

                    foreach ( var recipient in recipients )
                    {
                        var response = twilio.SendMessage( fromPhone, recipient, message );
                    }
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
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
        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }
    }
}
