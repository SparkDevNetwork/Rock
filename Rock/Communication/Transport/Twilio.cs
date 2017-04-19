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
using System.Threading.Tasks;

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
                communication.HasPendingRecipients(rockContext) &&
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
                    TwilioClient.Init( accountSid, authToken );

                    var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                    var communicationEntityTypeId = EntityTypeCache.Read( "Rock.Model.Communication" ).Id;
                    var communicationCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), rockContext ).Id;

                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                    // get message template
                    string message = communication.GetMediumDataValue( "Message" );

                    // convert any special microsoft word characters to normal chars so they don't look funny (for example "Hey â€œdouble-quotesâ€ from â€˜single quoteâ€™")
                    message = message.ReplaceWordChars();

                    // determine callback address
                    var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                    string callbackUrl = globalAttributes.GetValue( "PublicApplicationRoot" ) + "Webhooks/Twilio.ashx";


                    bool recipientFound = true;
                    while ( recipientFound )
                    {
                        var loopContext = new RockContext();
                        var historyService = new HistoryService( loopContext );
                        var recipient = Rock.Model.Communication.GetNextPending( communication.Id, loopContext );
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
                                    var mergeObjects = recipient.CommunicationMergeValues( mergeFields );
                                    
                                    var resolvedMessage = message.ResolveMergeFields( mergeObjects, communication.EnabledLavaCommands );
 
                                    string twilioNumber = phoneNumber.Number;
                                    if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
                                    {
                                        twilioNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
                                    }

                                    var response = MessageResource.Create(
                                        from: new TwilioTypes.PhoneNumber(fromPhone),
                                        to: new TwilioTypes.PhoneNumber(twilioNumber),
                                        body: resolvedMessage,
                                        statusCallback: new System.Uri(callbackUrl)
                                    );

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

                            loopContext.SaveChanges();
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
                        TwilioClient.Init( accountSid, authToken );

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
                            var response = MessageResource.Create(
                                from: new TwilioTypes.PhoneNumber(fromPhone),
                                to: new TwilioTypes.PhoneNumber(recipient),
                                body: message
                            );
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
                    TwilioClient.Init( accountSid, authToken );

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
                        var response = MessageResource.Create(
                            from: new TwilioTypes.PhoneNumber(fromPhone),
                            to: new TwilioTypes.PhoneNumber(recipient),
                            body: message
                        );
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
        public override void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
        {
            throw new NotImplementedException();
        }

    }
}
