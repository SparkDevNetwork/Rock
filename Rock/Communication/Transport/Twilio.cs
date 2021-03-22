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
using Rock.SystemKey;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending SMS messages using Twilio
    /// </summary>
    [Description( "Sends a communication through Twilio API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Twilio" )]
    [TextField( "SID",
        Description = "Your Twilio Account SID (find at https://www.twilio.com/user/account)",
        IsRequired = true,
        Order = 0,
        Key = TwilioAttributeKey.Sid)]
    [TextField( "Auth Token",
        Description = "Your Twilio Account Token",
        IsRequired = true,
        Order = 1,
        Key = TwilioAttributeKey.AuthToken)]
    [IntegerField( "Long-Code Throttling",
        Description = "The amount of time (in milliseconds) to wait between sending to recipients when sending a message from a long-code number (regular phone number). When carriers detect that a message is not coming from a human, they may filter/block the message. A delay can help prevent this from happening.",
        IsRequired = false,
        DefaultIntegerValue = 500,
        Order = 2,
        Key = TwilioAttributeKey.LongCodeThrottling)]
    [BooleanField( "Enable Signature Validation",
        Description = "The Auth Token will be validated with each request to the Twilio web hooks. If enabled, the Public Application Root must be used as the Webhook URL of your configuration in Twilio otherwise your incoming messages will not validate (be accepted).  Also, if you change your AuthToken or create a secondary AuthToken in Twilio, your incoming Twilio messages would not validate until the Token has been promoted to your primary AuthToken.",
        Order = 3,
        Key = TwilioAttributeKey.EnableValidation)]
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

                string accountSid = GetAttributeValue( TwilioAttributeKey.Sid);
                string authToken = GetAttributeValue( TwilioAttributeKey.AuthToken );
                TwilioClient.Init( accountSid, authToken );

                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                int? throttlingWaitTimeMS = null;
                if ( this.IsLongCodePhoneNumber( smsMessage.FromNumber.Value ) )
                {
                    throttlingWaitTimeMS = this.GetAttributeValue( TwilioAttributeKey.LongCodeThrottling ).AsIntegerOrNull();
                }

                List<Uri> attachmentMediaUrls = GetAttachmentMediaUrls( rockMessage.Attachments.AsQueryable() );

                foreach ( var recipient in rockMessage.GetRecipients() )
                {
                    try
                    {
                        foreach ( var mergeField in mergeFields )
                        {
                            recipient.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
                        }

                        CommunicationRecipient communicationRecipient = null;

                        using ( var rockContext = new RockContext() )
                        {
                            CommunicationRecipientService communicationRecipientService = new CommunicationRecipientService( rockContext );
                            int? recipientId = recipient.CommunicationRecipientId;
                            if ( recipientId != null )
                            {
                                communicationRecipient = communicationRecipientService.Get( recipientId.Value );
                            }

                            string message = ResolveText( smsMessage.Message, smsMessage.CurrentPerson, communicationRecipient, smsMessage.EnabledLavaCommands, recipient.MergeFields, smsMessage.AppRoot, smsMessage.ThemeRoot );
                            Person recipientPerson = ( Person ) recipient.MergeFields.GetValueOrNull( "Person" );

                            // Create the communication record and send using that if we have a person since a communication record requires a valid person. Otherwise just send without creating a communication record.
                            if ( rockMessage.CreateCommunicationRecord && recipientPerson != null )
                            {
                                var communicationService = new CommunicationService( rockContext );

                                Rock.Model.Communication communication = communicationService.CreateSMSCommunication( smsMessage.CurrentPerson, recipientPerson?.PrimaryAliasId, message, smsMessage.FromNumber, string.Empty, smsMessage.communicationName );

                                // Since we just created a new communication record, we need to move any attachments from the rockMessage
                                // to the communication's attachments since the Send method below will be handling the delivery.
                                if ( attachmentMediaUrls.Any() )
                                {
                                    foreach ( var attachment in rockMessage.Attachments.AsQueryable() )
                                    {
                                        communication.AddAttachment( new CommunicationAttachment { BinaryFileId = attachment.Id }, CommunicationType.SMS );
                                    }
                                }

                                rockContext.SaveChanges();
                                Send( communication, mediumEntityTypeId, mediumAttributes );
                                continue;
                            }
                            else
                            {
                                MessageResource response = SendToTwilio( smsMessage.FromNumber.Value, null, attachmentMediaUrls, message, recipient.To );

                                if ( response.ErrorMessage.IsNotNullOrWhiteSpace() )
                                {
                                    errorMessages.Add( response.ErrorMessage );
                                }

                                if ( communicationRecipient != null )
                                {
                                    rockContext.SaveChanges();
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        errorMessages.Add( ex.Message );
                        ExceptionLogService.LogException( ex );
                    }

                    if ( throttlingWaitTimeMS.HasValue )
                    {
                        System.Threading.Tasks.Task.Delay( throttlingWaitTimeMS.Value ).Wait();
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
                    var currentPerson = communication.CreatedByPersonAlias?.Person;
                    var globalAttributes = GlobalAttributesCache.Get();
                    string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                    string fromPhone = communication.SMSFromDefinedValue?.Value;
                    if ( string.IsNullOrWhiteSpace( fromPhone ) )
                    {
                        // just in case we got this far without a From Number, throw an exception
                        throw new Exception( "A From Number was not provided for communication: " + communication.Id.ToString() );
                    }

                    if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                    {

                        int? throttlingWaitTimeMS = null;
                        if ( this.IsLongCodePhoneNumber( fromPhone ) )
                        {
                            throttlingWaitTimeMS = this.GetAttributeValue( TwilioAttributeKey.LongCodeThrottling ).AsIntegerOrNull();
                        }

                        string accountSid = GetAttributeValue( TwilioAttributeKey.Sid );
                        string authToken = GetAttributeValue( TwilioAttributeKey.AuthToken );
                        TwilioClient.Init( accountSid, authToken );

                        var personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                        var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                        var communicationCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), communicationRockContext ).Id;

                        string callbackUrl = publicAppRoot + "Webhooks/Twilio.ashx";

                        var smsAttachmentsBinaryFileIdList = communication.GetAttachmentBinaryFileIds( CommunicationType.SMS );
                        List<Uri> attachmentMediaUrls = new List<Uri>();
                        if ( smsAttachmentsBinaryFileIdList.Any() )
                        {
                            attachmentMediaUrls = this.GetAttachmentMediaUrls( new BinaryFileService( communicationRockContext ).GetByIds( smsAttachmentsBinaryFileIdList ) );
                        }

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
                                        var twilioNumber = recipient.PersonAlias.Person.PhoneNumbers.GetFirstSmsNumber();
                                        if ( !string.IsNullOrWhiteSpace( twilioNumber ) )
                                        {
                                            // Create merge field dictionary
                                            var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                                            string message = ResolveText( communication.SMSMessage, currentPerson, recipient, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                                            MessageResource response = SendToTwilio( fromPhone, callbackUrl, attachmentMediaUrls, message, twilioNumber );

                                            recipient.Status = CommunicationRecipientStatus.Delivered;
                                            recipient.SendDateTime = RockDateTime.Now;
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
                                                    Verb = History.HistoryVerb.Sent.ConvertToString().ToUpper(),
                                                    ChangeType = History.HistoryChangeType.Record.ToString(),
                                                    ValueName = "SMS message",
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

                                if ( throttlingWaitTimeMS.HasValue )
                                {
                                    System.Threading.Tasks.Task.Delay( throttlingWaitTimeMS.Value ).Wait();
                                }
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


        #region private shared methods

        /// <summary>
        /// Gets the attachment media urls.
        /// </summary>
        /// <param name="attachments">The attachments.</param>
        /// <returns></returns>
        private List<Uri> GetAttachmentMediaUrls( IQueryable<BinaryFile> attachments )
        {
            var binaryFilesInfo = attachments.Select( a => new
            {
                a.Id,
                a.MimeType
            } ).ToList();

            List<Uri> attachmentMediaUrls = new List<Uri>();
            if ( binaryFilesInfo.Any() )
            {
                string publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
                attachmentMediaUrls = binaryFilesInfo.Select( b =>
                {
                    if ( b.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return new Uri( $"{publicAppRoot}GetImage.ashx?id={b.Id}" );
                    }
                    else
                    {
                        return new Uri( $"{publicAppRoot}GetFile.ashx?id={b.Id}" );
                    }
                } ).ToList();
            }

            return attachmentMediaUrls;
        }

        /// <summary>
        /// Sends to twilio.
        /// </summary>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="callbackUrl">The callback URL.</param>
        /// <param name="attachmentMediaUrls">The attachment media urls.</param>
        /// <param name="message">The message.</param>
        /// <param name="twilioNumber">The twilio number.</param>
        /// <returns></returns>
        private MessageResource SendToTwilio( string fromPhone, string callbackUrl, List<Uri> attachmentMediaUrls, string message, string twilioNumber )
        {
            MessageResource response = null;

            // twilio has a max message size of 1600 (one thousand six hundred) characters
            // hopefully it isn't going to be that big, but just in case, break it into chunks if it is longer than that
            if ( message.Length > 1600 )
            {
                var messageChunks = message.SplitIntoChunks( 1600 );

                foreach ( var messageChunk in messageChunks )
                {
                    var shouldAddAttachments = messageChunk == messageChunks.Last();

                    response = SendTwilioMessage( fromPhone, callbackUrl, shouldAddAttachments ? attachmentMediaUrls : null, twilioNumber, messageChunk );
                }
            }
            else
            {
                response = SendTwilioMessage( fromPhone, callbackUrl, attachmentMediaUrls, twilioNumber, message );
            }

            return response;
        }

        private static MessageResource SendTwilioMessage( string fromPhone, string callbackUrl, List<Uri> attachmentMediaUrls, string twilioNumber, string messageText )
        {
            MessageResource response;
            CreateMessageOptions createMessageOptions = new CreateMessageOptions( new TwilioTypes.PhoneNumber( twilioNumber ) )
            {
                From = new TwilioTypes.PhoneNumber( fromPhone ),
                Body = messageText
            };

            if ( callbackUrl.IsNotNullOrWhiteSpace() )
            {
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment
                    && !callbackUrl.Contains( ".ngrok.io" ) )
                {
                    createMessageOptions.StatusCallback = null;
                }
                else
                {
                    createMessageOptions.StatusCallback = new Uri( callbackUrl );
                }
            }

            if ( attachmentMediaUrls != null && attachmentMediaUrls.Any() )
            {
                createMessageOptions.MediaUrl = attachmentMediaUrls;
            }

            response = MessageResource.Create( createMessageOptions );
            return response;
        }

        #endregion

        #region 

        /// <summary>
        /// Determines whether the phone number is a regular 10 digit (or longer) phone number
        /// </summary>
        /// <param name="fromNumber">From number.</param>
        /// <returns>
        ///   <c>true</c> if [is long code phone number] [the specified from number]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLongCodePhoneNumber( string fromNumber )
        {
            // if the number of digits in the phone number 10 or more, assume is it a LongCode ( if it is less than 10, assume it is a short-code)
            return fromNumber.AsNumeric().Length >= 10;
        }

        /// <summary>
        /// The MIME types for SMS attachments that Rock and Twilio fully support (also see AcceptedMimeTypes )_
        /// Twilio's supported MimeTypes are from https://www.twilio.com/docs/api/messaging/accepted-mime-types
        /// </summary>
        public static readonly List<string> SupportedMimeTypes = new List<string>
        {
            "image/jpeg",
            "image/gif",
            "image/png",
        };

        /// <summary>
        /// The MIME types for SMS attachments that Rock and Twilio support/accept
        /// Twilio's accepted MimeTypes are from https://www.twilio.com/docs/api/messaging/accepted-mime-types
        /// Rock supports the following subset of those
        /// </summary>
        public static readonly List<string> AcceptedMimeTypes = new List<string>
        {
            // These are fully supported by Twilio and will be formatted for delivery on destination devices
            "image/jpeg",
            "image/gif",
            "image/png",

            // These are accepted, but will not be modified for device compatibility
            "audio/mp4",
            "audio/mpeg",
            "video/mp4",
            "video/quicktime",
            "video/H264",
            "image/bmp",
            "text/vcard",
            "text/x-vcard", // sometimes, vcard is reported as x-vcard when uploaded thru IIS
            "text/csv",
            "text/rtf",
            "text/richtext",
            "text/calendar"
        };


        /// <summary>
        /// The media size limit in bytes (5MB)
        /// </summary>
        public const int MediaSizeLimitBytes = 5 * 1024 * 1024;

        #endregion
    }
}
