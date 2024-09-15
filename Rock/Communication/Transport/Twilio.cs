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
using System.Threading;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using TwilioTypes = Twilio.Types;
using TwilioExceptions = Twilio.Exceptions;

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
        Key = TwilioAttributeKey.Sid )]
    [TextField( "Auth Token",
        Description = "Your Twilio Account Token",
        IsRequired = true,
        Order = 1,
        Key = TwilioAttributeKey.AuthToken )]
    [IntegerField( "Long-Code Throttling",
        Description = "The amount of time (in milliseconds) to wait between sending to recipients when sending a message from a long-code number (regular phone number). When carriers detect that a message is not coming from a human, they may filter/block the message. A delay can help prevent this from happening.",
        IsRequired = false,
        DefaultIntegerValue = 500,
        Order = 2,
        Key = TwilioAttributeKey.LongCodeThrottling )]
    [BooleanField( "Enable Signature Validation",
        Description = "The Auth Token will be validated with each request to the Twilio web hooks. If enabled, the Public Application Root must be used as the Webhook URL of your configuration in Twilio otherwise your incoming messages will not validate (be accepted).  Also, if you change your AuthToken or create a secondary AuthToken in Twilio, your incoming Twilio messages would not validate until the Token has been promoted to your primary AuthToken.",
        Order = 3,
        Key = TwilioAttributeKey.EnableValidation )]
    [IntegerField( "Concurrent Send Workers",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Order = 4,
        Key = TwilioAttributeKey.MaxParallelization )]
    [Rock.SystemGuid.EntityTypeGuid( "CF9FD146-8623-4D9A-98E6-4BD710F071A4" )]
    public class Twilio : TransportComponent, IAsyncTransport, ISmsPipelineWebhook
    {
        /// <summary>
        /// Gets the sms pipeline webhook path that should be used by this transport.
        /// </summary>
        /// <value>
        /// The sms pipeline webhook path.
        /// </value>
        /// <note>
        /// This should be from the application root (https://www.rocksolidchurch.com/).
        /// </note>
        public string SmsPipelineWebhookPath => "Webhooks/TwilioSms.ashx";

        #region IAsyncTransport Implementation
        /// <summary>
        /// Gets the maximum parallelization.
        /// </summary>
        /// <value>
        /// The maximum parallelization.
        /// </value>
        public int MaxParallelization
        {
            get
            {
                return GetAttributeValue( TwilioAttributeKey.MaxParallelization ).AsIntegerOrNull() ?? 10;
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task SendAsync( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var fromPhone = string.Empty;
            var unprocessedRecipientCount = 0;
            var mergeFields = new Dictionary<string, object>();
            Person currentPerson = null;
            var attachmentMediaUrls = new List<Uri>();
            var personEntityTypeId = 0;
            var communicationCategoryId = 0;
            var communicationEntityTypeId = 0;

            using ( var rockContext = new RockContext() )
            {
                // Requery the Communication
                communication = new CommunicationService( rockContext ).Get( communication.Id );

                if ( communication != null &&
                    communication.Status == Model.CommunicationStatus.Approved &&
                    ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
                {
                    var qryRecipients = new CommunicationRecipientService( rockContext ).Queryable();
                    unprocessedRecipientCount = qryRecipients
                        .Where( r =>
                            r.CommunicationId == communication.Id &&
                            r.Status == Model.CommunicationRecipientStatus.Pending &&
                            r.MediumEntityTypeId.HasValue &&
                            r.MediumEntityTypeId.Value == mediumEntityTypeId )
                        .Count();
                }

                if ( unprocessedRecipientCount == 0 )
                {
                    return;
                }

                fromPhone = communication.SmsFromSystemPhoneNumber?.Number;
                if ( string.IsNullOrWhiteSpace( fromPhone ) )
                {
                    // just in case we got this far without a From Number, throw an exception
                    throw new Exception( "A From Number was not provided for communication: " + communication.Id.ToString() );
                }

                currentPerson = communication.CreatedByPersonAlias?.Person;
                mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                personEntityTypeId = EntityTypeCache.Get<Person>().Id;
                communicationEntityTypeId = EntityTypeCache.Get<Model.Communication>().Id;
                communicationCategoryId = CategoryCache.Get( SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), rockContext ).Id;
                var smsAttachmentsBinaryFileIdList = communication.GetAttachmentBinaryFileIds( CommunicationType.SMS );

                if ( smsAttachmentsBinaryFileIdList.Any() )
                {
                    attachmentMediaUrls = this.GetAttachmentMediaUrls( new BinaryFileService( rockContext ).GetByIds( smsAttachmentsBinaryFileIdList ) );
                }
            }

            int? throttlingWaitTimeMS = null;
            if ( this.IsLongCodePhoneNumber( fromPhone ) )
            {
                throttlingWaitTimeMS = GetAttributeValue( TwilioAttributeKey.LongCodeThrottling ).AsIntegerOrNull();
            }

            var globalAttributes = GlobalAttributesCache.Get();
            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
            var callbackUrl = publicAppRoot + "Webhooks/Twilio.ashx";

            var accountSid = GetAttributeValue( TwilioAttributeKey.Sid );
            var authToken = GetAttributeValue( TwilioAttributeKey.AuthToken );

            TwilioClient.Init( accountSid, authToken );

            if ( throttlingWaitTimeMS.HasValue )
            {
                // If throttlingWaitTime has a value we need to send all text synchronously so that the throttle is respected.
                var recipientFound = true;
                while ( recipientFound )
                {
                    // make a new rockContext per recipient
                    var recipient = GetNextPending( communication.Id, mediumEntityTypeId, communication.IsBulkCommunication );

                    // This means we are done, break the loop
                    if ( recipient == null )
                    {
                        recipientFound = false;
                        continue;
                    }

                    await SendToCommunicationRecipient( communication, fromPhone, mergeFields, currentPerson, attachmentMediaUrls, personEntityTypeId, communicationCategoryId, communicationEntityTypeId, publicAppRoot, callbackUrl, recipient ).ConfigureAwait( false );

                    await Task.Delay( throttlingWaitTimeMS.Value ).ConfigureAwait( false );
                }
            }
            else
            {
                var sendingTask = new List<Task>( unprocessedRecipientCount );
                var asyncTransport = this as IAsyncTransport;
                var maxParallelization = asyncTransport?.MaxParallelization ?? 10;

                using ( var mutex = new SemaphoreSlim( maxParallelization ) )
                {
                    var recipientFound = true;
                    while ( recipientFound )
                    {
                        // make a new rockContext per recipient
                        var recipient = GetNextPending( communication.Id, mediumEntityTypeId, communication.IsBulkCommunication );

                        // This means we are done, break the loop
                        if ( recipient == null )
                        {
                            recipientFound = false;
                            continue;
                        }

                        await mutex.WaitAsync().ConfigureAwait( false );

                        sendingTask.Add( ThrottleHelper.ThrottledExecute( () => SendToCommunicationRecipient( communication, fromPhone, mergeFields, currentPerson, attachmentMediaUrls, personEntityTypeId, communicationCategoryId, communicationEntityTypeId, publicAppRoot, callbackUrl, recipient ), mutex ) );
                    }

                    /*
                     * Now that we have fired off all of the task, we need to wait for them to complete.
                     * Once all of the task have been completed we can continue.
                     */
                    while ( sendingTask.Count > 0 )
                    {
                        var completedTask = await Task.WhenAny( sendingTask ).ConfigureAwait( false );
                        sendingTask.Remove( completedTask );
                    }
                }
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<SendMessageResult> SendAsync( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var sendMessageResult = new SendMessageResult();

            var smsMessage = rockMessage as RockSMSMessage;
            if ( smsMessage != null )
            {
                // Validate From Number
                if ( smsMessage.FromSystemPhoneNumber == null )
                {
                    sendMessageResult.Errors.Add( "A From Number was not provided." );
                    return sendMessageResult;
                }

                string accountSid = GetAttributeValue( TwilioAttributeKey.Sid );
                string authToken = GetAttributeValue( TwilioAttributeKey.AuthToken );
                TwilioClient.Init( accountSid, authToken );

                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                int? throttlingWaitTimeMS = null;
                if ( this.IsLongCodePhoneNumber( smsMessage.FromSystemPhoneNumber.Number ) )
                {
                    throttlingWaitTimeMS = this.GetAttributeValue( TwilioAttributeKey.LongCodeThrottling ).AsIntegerOrNull();
                }

                List<Uri> attachmentMediaUrls = GetAttachmentMediaUrls( rockMessage.Attachments.AsQueryable() );

                if ( throttlingWaitTimeMS.HasValue )
                {
                    foreach ( var recipient in rockMessage.GetRecipients() )
                    {
                        var result = await SendToRecipientAsync( recipient, mergeFields, smsMessage, attachmentMediaUrls, mediumEntityTypeId, mediumAttributes ).ConfigureAwait( false );

                        sendMessageResult.Errors.AddRange( result.Errors );
                        sendMessageResult.Errors.AddRange( result.Warnings );
                        sendMessageResult.MessagesSent += result.MessagesSent;

                        await Task.Delay( throttlingWaitTimeMS.Value ).ConfigureAwait( false );
                    }
                }
                else
                {
                    var sendingTask = new List<Task<SendMessageResult>>();

                    using ( var mutex = new SemaphoreSlim( MaxParallelization ) )
                    {
                        foreach ( var recipient in rockMessage.GetRecipients() )
                        {
                            var startMutexWait = System.Diagnostics.Stopwatch.StartNew();
                            await mutex.WaitAsync().ConfigureAwait( false );
                            sendingTask.Add( ThrottleHelper.ThrottledExecute( () => SendToRecipientAsync( recipient, mergeFields, smsMessage, attachmentMediaUrls, mediumEntityTypeId, mediumAttributes ), mutex ) );
                        }

                        /*
                         * Now that we have fired off all of the task, we need to wait for them to complete, get their results,
                         * and then process that result. Once all of the task have been completed we can continue.
                         */
                        while ( sendingTask.Count > 0 )
                        {
                            var completedTask = await Task.WhenAny( sendingTask ).ConfigureAwait( false );
                            sendingTask.Remove( completedTask );

                            var result = await completedTask.ConfigureAwait( false );
                            sendMessageResult.Errors.AddRange( result.Errors );
                            sendMessageResult.Errors.AddRange( result.Warnings );
                            sendMessageResult.MessagesSent += result.MessagesSent;
                        }
                    }
                }
            }

            return sendMessageResult;
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
        private async Task<MessageResource> SendToTwilioAsync( string fromPhone, string callbackUrl, List<Uri> attachmentMediaUrls, string message, string twilioNumber )
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

                    response = await SendTwilioMessageAsync( fromPhone, callbackUrl, shouldAddAttachments ? attachmentMediaUrls : null, twilioNumber, messageChunk ).ConfigureAwait( false );
                }
            }
            else
            {
                response = await SendTwilioMessageAsync( fromPhone, callbackUrl, attachmentMediaUrls, twilioNumber, message ).ConfigureAwait( false );
            }

            return response;
        }

        private async static Task<MessageResource> SendTwilioMessageAsync( string fromPhone, string callbackUrl, List<Uri> attachmentMediaUrls, string twilioNumber, string messageText )
        {
            MessageResource response = null;
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

            response = await MessageResource.CreateAsync( createMessageOptions ).ConfigureAwait( false );
            return response;
        }

        private async Task<SendMessageResult> SendToRecipientAsync( RockMessageRecipient recipient, Dictionary<string, object> mergeFields, RockSMSMessage smsMessage, List<Uri> attachmentMediaUrls, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var sendMessageResult = new SendMessageResult();
            try
            {
                foreach ( var mergeField in mergeFields )
                {
                    recipient.MergeFields.TryAdd( mergeField.Key, mergeField.Value );
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
                    if ( smsMessage.CreateCommunicationRecord && recipientPerson != null )
                    {
                        var communicationService = new CommunicationService( rockContext );

                        var createSMSCommunicationArgs = new CommunicationService.CreateSMSCommunicationArgs
                        {
                            FromPerson = smsMessage.CurrentPerson,
                            ToPersonAliasId = recipientPerson?.PrimaryAliasId,
                            Message = message,
                            FromSystemPhoneNumber = smsMessage.FromSystemPhoneNumber,
                            CommunicationName = smsMessage.CommunicationName,
                            ResponseCode = string.Empty,
                            SystemCommunicationId = smsMessage.SystemCommunicationId
                        };

                        Rock.Model.Communication communication = communicationService.CreateSMSCommunication( createSMSCommunicationArgs );

                        if ( smsMessage?.CurrentPerson != null )
                        {
                            communication.CreatedByPersonAliasId = smsMessage.CurrentPerson.PrimaryAliasId;
                            communication.ModifiedByPersonAliasId = smsMessage.CurrentPerson.PrimaryAliasId;
                        }

                        // Since we just created a new communication record, we need to move any attachments from the rockMessage
                        // to the communication's attachments since the Send method below will be handling the delivery.
                        if ( attachmentMediaUrls.Any() )
                        {
                            foreach ( var attachment in smsMessage.Attachments.AsQueryable() )
                            {
                                communication.AddAttachment( new CommunicationAttachment { BinaryFileId = attachment.Id }, CommunicationType.SMS );
                            }
                        }

                        rockContext.SaveChanges();
                        await SendAsync( communication, mediumEntityTypeId, mediumAttributes ).ConfigureAwait( false );

                        communication.SendDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                        sendMessageResult.MessagesSent += 1;
                    }
                    else
                    {
                        MessageResource response = await SendToTwilioAsync( smsMessage.FromSystemPhoneNumber.Number, null, attachmentMediaUrls, message, recipient.To ).ConfigureAwait( false );

                        if ( response.ErrorMessage.IsNotNullOrWhiteSpace() )
                        {
                            sendMessageResult.Errors.Add( response.ErrorMessage );
                        }
                        else
                        {
                            sendMessageResult.MessagesSent += 1;
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
                sendMessageResult.Errors.Add( ex.Message );
                ExceptionLogService.LogException( ex );
            }

            return sendMessageResult;
        }

        private async Task SendToCommunicationRecipient( Model.Communication communication, string fromPhone, Dictionary<string, object> mergeFields, Person currentPerson, List<Uri> attachmentMediaUrls, int personEntityTypeId, int communicationCategoryId, int communicationEntityTypeId, string publicAppRoot, string callbackUrl, CommunicationRecipient recipient )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    recipient = new CommunicationRecipientService( rockContext ).Get( recipient.Id );
                    var twilioNumber = recipient.PersonAlias.Person.PhoneNumbers.GetFirstSmsNumber();
                    if ( !string.IsNullOrWhiteSpace( twilioNumber ) )
                    {
                        // Create merge field dictionary
                        var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                        string message = ResolveText( communication.SMSMessage, currentPerson, recipient, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                        var response = await SendToTwilioAsync( fromPhone, callbackUrl, attachmentMediaUrls, message, twilioNumber ).ConfigureAwait( false );

                        recipient.Status = CommunicationRecipientStatus.Delivered;
                        recipient.SendDateTime = RockDateTime.Now;
                        recipient.TransportEntityTypeName = this.GetType().FullName;
                        recipient.UniqueMessageId = response.Sid;

                        try
                        {
                            var historyService = new HistoryService( rockContext );
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
                catch ( TwilioExceptions.ApiException ex )
                {
                    recipient.Status = CommunicationRecipientStatus.Failed;
                    recipient.StatusNote = "Twilio Exception: " + ex.Message;

                    if( DisableSmsErrorCodes.Contains( ex.Code ) )
                    {
                        // Disable SMS for this number because the response indicates that Rock should not send messages to that number anymore.
                        var phoneNumber = recipient.PersonAlias.Person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
                        phoneNumber.IsMessagingEnabled = false;

                        // Add this to the Person Activity history
                        var historyChanges = new History.HistoryChangeList();
                        historyChanges.AddCustom( string.Empty, History.HistoryChangeType.Property.ToString(), $"SMS Disabled for {phoneNumber.NumberTypeValue} {phoneNumber.NumberFormatted}. The error received from Twilio is <em>\"{ex.Message}\"</em> <a href='{ex.MoreInfo}' target='_blank' rel='noopener noreferrer'>More info here</a>" );
                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), recipient.PersonAlias.Person.Id, historyChanges );
                    }
                }
                catch ( Exception ex )
                {
                    recipient.Status = CommunicationRecipientStatus.Failed;
                    recipient.StatusNote = "Twilio Exception: " + ex.Message;
                }

                rockContext.SaveChanges();
            }
        }
        #endregion

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

            var sendMessageResult = AsyncHelper.RunSync( () => SendAsync( rockMessage, mediumEntityTypeId, mediumAttributes ) );

            errorMessages.AddRange( sendMessageResult.Errors );

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
            AsyncHelper.RunSync( () => SendAsync( communication, mediumEntityTypeId, mediumAttributes ) );
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
                        return new Uri( FileUrlHelper.GetImageUrl( b.Id, new GetImageUrlOptions { PublicAppRoot = publicAppRoot } ) );
                    }
                    else
                    {
                        return new Uri( FileUrlHelper.GetFileUrl( b.Id, new GetFileUrlOptions { PublicAppRoot = publicAppRoot } ) );
                    }
                } ).ToList();
            }

            return attachmentMediaUrls;
        }

        private Rock.Model.CommunicationRecipient GetNextPending( int communicationId, int mediumEntityId, bool isBulkCommunication )
        {
            using ( var rockContext = new RockContext() )
            {
                var recipient = Model.Communication.GetNextPending( communicationId, mediumEntityId, rockContext );
                if ( ValidRecipient( recipient, isBulkCommunication ) )
                {
                    return recipient;
                }
                else
                {
                    rockContext.SaveChanges();
                    return GetNextPending( communicationId, mediumEntityId, isBulkCommunication );
                }
            }
        }

        #endregion

        #region 

        /// <summary>
        /// If the Twilio API returns any of these exceptions then SMS should be disabled (PhoneNumberIsMessagingEnabled) for the phone number along with any other actions taken.
        /// https://www.twilio.com/docs/api/errors/
        /// </summary>
        private static readonly List<int> DisableSmsErrorCodes = new List<int>
        {
            21211,
            21214,
            21612,
            21614,
            21610,
            30006,
            60205,
            63033
        };

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
