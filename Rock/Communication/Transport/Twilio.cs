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
using System.Text;
using System.Text.RegularExpressions;
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

using TwilioExceptions = Twilio.Exceptions;
using TwilioTypes = Twilio.Types;

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
            catch ( TwilioExceptions.ApiException ex ) when ( ex.Code == 21610 )
            {
                // This recipient has previously opted out of receiving messages from this number (e.g. by texting "STOP").
                // They'll need to text "START" to this number before they can receive messages again.
                // https://www.twilio.com/docs/api/errors/21610

                var unsubscribedMessageSb = new StringBuilder( UnsubscribedSmsRecipientMessage );
                if ( ( smsMessage.FromSystemPhoneNumber?.Number ).IsNotNullOrWhiteSpace() )
                {
                    unsubscribedMessageSb.Append( $" To resume messaging, text START to {smsMessage.FromSystemPhoneNumber.Number}." );
                }

                sendMessageResult.Errors.Add( unsubscribedMessageSb.ToString() );
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

                        var now = RockDateTime.Now;

                        recipient.Status = CommunicationRecipientStatus.Delivered;
                        recipient.SendDateTime = now;
                        recipient.DeliveredDateTime = now;
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

                    if ( DisableSmsErrorCodes.Contains( ex.Code ) )
                    {
                        // Disable SMS for this number because the response indicates that Rock should not send messages to that number anymore.
                        var phoneNumber = recipient.PersonAlias.Person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
                        if ( phoneNumber != null )
                        {
                            phoneNumber.IsMessagingEnabled = false;
                            phoneNumber.IsMessagingOptedOut = true;
                        }

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

        /// <inheritdoc/>
        public override string ResolveText( string content, Person person, string enabledLavaCommands, Dictionary<string, object> mergeFields, string appRoot = "", string themeRoot = "" )
        {
            var resolved = base.ResolveText( content, person, enabledLavaCommands, mergeFields, appRoot, themeRoot );

            resolved = ReplaceUnicodeCharacters( resolved );

            return resolved;
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

        /// <summary>
        /// Replaces specific Unicode characters in the given text with their ASCII equivalents.
        /// </summary>
        /// <param name="text">The input string containing Unicode characters to be replaced.</param>
        /// <returns>A new string with Unicode characters replaced by their ASCII equivalents.  If the input string is null or
        /// consists only of whitespace, the original string is returned.</returns>
        private string ReplaceUnicodeCharacters( string text )
        {
            if ( text.IsNullOrWhiteSpace() )
            {
                return text;
            }

            // This is from https://www.twilio.com/docs/messaging/services/smart-encoding-char-list
            var replacements = new Dictionary<string, string>
            {
                // Quotes
                { "\u00AB", "\"" }, // Left pointing double angle quotation mark («)
                { "\u00BB", "\"" }, // Right pointing double angle quotation mark (»)
                { "\u201C", "\"" }, // Left double quotation mark (“)
                { "\u201D", "\"" }, // Right double quotation mark (”)
                { "\u02BA", "\"" }, // Modifier letter double prime (ʺ)
                { "\u02EE", "\"" }, // Modifier letter double apostrophe (ˮ)
                { "\u201F", "\"" }, // Double high reversed 9 quotation mark (‟)
                { "\u275D", "\"" }, // Heavy double turned comma quotation mark ornament (❝)
                { "\u275E", "\"" }, // Heavy double comma quotation mark ornament (❞)
                { "\u301D", "\"" }, // Reversed double prime quotation mark (〝)
                { "\u301E", "\"" }, // Double prime quotation mark (〞)
                { "\uFF02", "\"" }, // Fullwidth quotation mark (＂)
                { "\u201E", "\"" }, // Double low 9 quotation mark („)

                // Apostrophes and single quotes
                { "\u2018", "'" }, // Left single quotation mark (‘)
                { "\u2019", "'" }, // Right single quotation mark (’)
                { "\u02BB", "'" }, // Modifier letter turned comma (ʻ)
                { "\u02C8", "'" }, // Modifier letter vertical line (ˈ)
                { "\u02BC", "'" }, // Modifier letter apostrophe (ʼ)
                { "\u02BD", "'" }, // Modifier letter reversed comma (ʽ)
                { "\u02B9", "'" }, // Modifier letter prime (ʹ)
                { "\u201B", "'" }, // Single high reversed 9 quotation mark (‛)
                { "\uFF07", "'" }, // Fullwidth apostrophe (＇)
                { "\u00B4", "'" }, // Acute accent (´)
                { "\u02CA", "'" }, // Modifier letter acute accent (ˊ)
                { "\u0060", "'" }, // Grave accent (`)
                { "\u02CB", "'" }, // Modifier letter grave accent (ˋ)
                { "\u275B", "'" }, // Heavy single turned comma quotation mark ornament (❛)
                { "\u275C", "'" }, // Heavy single comma quotation mark ornament (❜)
                { "\u0313", "'" }, // Combining comma above ( ̓)
                { "\u0314", "'" }, // Combining reversed comma above ( ̔)
                { "\uFE10", "'" }, // Presentation form for vertical comma (︐)
                { "\uFE11", "'" }, // Presentation form for vertical ideographic comma (︑)
                { "\u00A0", "'" }, // No break space
                { "\u2000", "'" }, // En quad

                // Vulgar fractions
                { "\u00BC", "1/4" }, // Vulgar fraction one quarter (¼)
                { "\u00BD", "1/2" }, // Vulgar fraction one half (½)
                { "\u00BE", "3/4" }, // Vulgar fraction three quarters (¾)

                // Forward slashes
                { "\u00F7", "/" }, // Division sign (÷)
                { "\u29F8", "/" }, // Big solidus (⧸)
                { "\u0337", "/" }, // Combining short solidus overlay (̷)
                { "\u0338", "/" }, // Combining long solidus overlay (̸)
                { "\u2044", "/" }, // Fraction slash (⁄)
                { "\u2215", "/" }, // Division slash (∕)
                { "\uFF0F", "/" }, // Fullwidth solidus (／)

                // Backslashes
                { "\u29F9", "\\" }, // Big reverse solidus (⧹)
                { "\u29F5", "\\" }, // Reverse solidus operator (⧵)
                { "\u20E5", "\\" }, // Combining reverse solidus overlay (⃥)
                { "\uFE68", "\\" }, // Small reverse solidus (﹨)
                { "\uFF3C", "\\" }, // Fullwidth reverse solidus (＼)

                // Underscores and low lines
                { "\u0332", "_" }, // Combining low line (̲)
                { "\uFF3F", "_" }, // Fullwidth low line (＿)
                { "\u2017", "_" }, // Double low line (‗)

                // Vertical bars and similar
                { "\u20D2", "|" }, // Combining long vertical line overlay (⃒)
                { "\u20D3", "|" }, // Combining short vertical line overlay (⃓)
                { "\u2223", "|" }, // Divides sign (∣)
                { "\uFF5C", "|" }, // Fullwidth vertical line (｜)
                { "\u23B8", "|" }, // Left vertical box line (⎸)
                { "\u23B9", "|" }, // Right vertical box line (⎹)
                { "\u23D0", "|" }, // Vertical line extension (⏐)
                { "\u239C", "|" }, // Left parenthesis upper hook (⎜)
                { "\u239F", "|" }, // Left parenthesis lower hook (⎟)
    
                // Dashes and bullets
                { "\u2014", "-" }, // Em dash (—)
                { "\u2013", "-" }, // En dash (–)
                { "\u23BC", "-" }, // Horizontal scan line 1 (⎼)
                { "\u23BD", "-" }, // Horizontal scan line 3 (⎽)
                { "\u2015", "-" }, // Horizontal bar (―)
                { "\uFE63", "-" }, // Small hyphen minus (﹣)
                { "\uFF0D", "-" }, // Fullwidth hyphen minus (－)
                { "\u2010", "-" }, // Hyphen (‐)
                { "\u2022", "-" }, // Bullet (•)
                { "\u2043", "-" }, // Hyphen bullet (⁃)

                // At signs
                { "\uFE6B", "@" }, // Small commercial at (﹫)
                { "\uFF20", "@" }, // Fullwidth commercial at (＠)

                // Dollar signs
                { "\uFE69", "$" }, // Small dollar sign (﹩)
                { "\uFF04", "$" }, // Fullwidth dollar sign (＄)

                // Exclamation marks
                { "\u01C3", "!" }, // Latin letter retroflex click (ǃ)
                { "\uFE15", "!" }, // Presentation form for vertical exclamation mark (︕)
                { "\uFE57", "!" }, // Small exclamation mark (﹗)
                { "\uFF01", "!" }, // Fullwidth exclamation mark (！)

                // Number signs
                { "\uFE5F", "#" }, // Small number sign (﹟)
                { "\uFF03", "#" }, // Fullwidth number sign (＃)

                // Percent signs
                { "\uFE6A", "%" }, // Small percent sign (﹪)
                { "\uFF05", "%" }, // Fullwidth percent sign (％)

                // Ampersands
                { "\uFE60", "&" }, // Small ampersand (﹠)
                { "\uFF06", "&" }, // Fullwidth ampersand (＆)

                // Commas and colons to comma
                { "\u201A", "," }, // Single low 9 quotation mark (‚)
                { "\u0326", "," }, // Combining comma below ( ̦)
                { "\uFE50", "," }, // Small comma (﹐)
                { "\u3001", "," }, // Ideographic comma (、)
                { "\uFE51", "," }, // Small ideographic comma (﹑)
                { "\uFF0C", "," }, // Fullwidth comma (，)
                { "\uFF64", "," }, // Halfwidth ideographic comma (､)
                { "\u02D0", "," }, // Modifier letter triangular colon (ː)
                { "\u02F8", "," }, // Modifier letter raised colon (˸)
                { "\u2982", "," }, // Z notation type colon (⦂)
                { "\uA789", "," }, // Modifier letter colon (꞉)
                { "\uFE13", "," }, // Presentation form for vertical colon (︓)
                { "\uFF1A", "," }, // Fullwidth colon (：)

                // Left parentheses
                { "\u2768", "(" }, // Medium left parenthesis ornament (❨)
                { "\u276A", "(" }, // Medium flattened left parenthesis ornament (❪)
                { "\uFE59", "(" }, // Small left parenthesis (﹙)
                { "\uFF08", "(" }, // Fullwidth left parenthesis (（)
                { "\u27EE", "(" }, // Mathematical left flattened parenthesis (⟮)
                { "\u2985", "(" }, // Left white parenthesis (⦅)

                // Right parentheses
                { "\u2769", ")" }, // Medium right parenthesis ornament (❩)
                { "\u276B", ")" }, // Medium flattened right parenthesis ornament (❫)
                { "\uFE5A", ")" }, // Small right parenthesis (﹚)
                { "\uFF09", ")" }, // Fullwidth right parenthesis (）
                { "\u27EF", ")" }, // Mathematical right flattened parenthesis (⟯)
                { "\u2986", ")" }, // Right white parenthesis (⦆)

                // Asterisks and asterisk like marks
                { "\u204E", "*" }, // Low asterisk (⁎)
                { "\u2217", "*" }, // Asterisk operator (∗)
                { "\u229B", "*" }, // Circled asterisk operator (⊛)
                { "\u2722", "*" }, // Four teardrop spoked asterisk (✢)
                { "\u2723", "*" }, // Heavy teardrop spoked asterisk (✣)
                { "\u2724", "*" }, // Snowflake symbol (✤)
                { "\u2725", "*" }, // Heavy four balloon spoked asterisk (✥)
                { "\u2731", "*" }, // Heavy asterisk (✱)
                { "\u2732", "*" }, // Open centre asterisk (✲)
                { "\u2733", "*" }, // Eight spoked asterisk (✳)
                { "\u273A", "*" }, // Heavy teardrop spoked asterisk (✺)
                { "\u273B", "*" }, // Open centre teardrop spoked asterisk (✻)
                { "\u273C", "*" }, // Heavy open centre asterisk (✼)
                { "\u273D", "*" }, // Open centre asterisk (✽)
                { "\u2743", "*" }, // Heavy teardrop spoked snowflake (❃)
                { "\u2749", "*" }, // Balloon spoked asterisk (❉)
                { "\u274A", "*" }, // Heavy eight teardrop spoked asterisk (❊)
                { "\u274B", "*" }, // Heavy eight balloon spoked asterisk (❋)
                { "\u29C6", "*" }, // Square with diagonal crosshatch fill (⧆)
                { "\uFE61", "*" }, // Small asterisk (﹡)
                { "\uFF0A", "*" }, // Fullwidth asterisk (＊)

                // Plus signs
                { "\u02D6", "+" }, // Modifier letter plus sign (˖)
                { "\uFE62", "+" }, // Small plus sign (﹢)
                { "\uFF0B", "+" }, // Fullwidth plus sign (＋)

                // Periods and full stops
                { "\u3002", "." }, // Ideographic full stop (。)
                { "\uFE52", "." }, // Small full stop (﹒)
                { "\uFF0E", "." }, // Fullwidth full stop (．)
                { "\uFF61", "." }, // Halfwidth ideographic full stop (｡)

                // Fullwidth digits
                { "\uFF10", "0" }, // Fullwidth digit zero (０)
                { "\uFF11", "1" }, // Fullwidth digit one (１)
                { "\uFF12", "2" }, // Fullwidth digit two (２)
                { "\uFF13", "3" }, // Fullwidth digit three (３)
                { "\uFF14", "4" }, // Fullwidth digit four (４)
                { "\uFF15", "5" }, // Fullwidth digit five (５)
                { "\uFF16", "6" }, // Fullwidth digit six (６)
                { "\uFF17", "7" }, // Fullwidth digit seven (７)
                { "\uFF18", "8" }, // Fullwidth digit eight (８)
                { "\uFF19", "9" }, // Fullwidth digit nine (９)

                // Semicolons
                { "\u204F", ";" }, // Reversed semicolon (⁏)
                { "\uFE14", ";" }, // Presentation form for vertical semicolon (︔)
                { "\uFE54", ";" }, // Small semicolon (﹔)
                { "\uFF1B", ";" }, // Fullwidth semicolon (；)

                // Less than signs and similar
                { "\uFE64", "<" }, // Small less than sign (﹤)
                { "\uFF1C", "<" }, // Fullwidth less than sign (＜)
                { "\u203A", "<" }, // Single right pointing angle quotation mark (›)

                // Equals signs and similar
                { "\u0347", "=" }, // Combining equals sign below ( ̻̇ note: equals below)
                { "\uA78A", "=" }, // Latin letter saltillo like equals (꞊)
                { "\uFE66", "=" }, // Small equals sign (﹦)
                { "\uFF1D", "=" }, // Fullwidth equals sign (＝)
    
                // Greater than signs and similar
                { "\u2039", ">" }, // Single left pointing angle quotation mark (‹)
                { "\uFE65", ">" }, // Small greater than sign (﹥)
                { "\uFF1E", ">" }, // Fullwidth greater than sign (＞)

                // Question marks
                { "\uFE16", "?" }, // Presentation form for vertical question mark (︖)
                { "\uFE56", "?" }, // Small question mark (﹖)
                { "\uFF1F", "?" }, // Fullwidth question mark (？)

                // Letters A
                { "\uFF21", "A" }, // Fullwidth Latin capital letter A (Ａ)
                { "\u1D00", "A" }, // Latin letter small capital A (ᴀ)

                // Letters B
                { "\uFF22", "B" }, // Fullwidth Latin capital letter B (Ｂ)
                { "\u0299", "B" }, // Latin letter small capital B (ʙ)

                // Letters C
                { "\uFF23", "C" }, // Fullwidth Latin capital letter C (Ｃ)
                { "\u1D04", "C" }, // Latin letter small capital C (ᴄ)

                // Letters D
                { "\uFF24", "D" }, // Fullwidth Latin capital letter D (Ｄ)
                { "\u1D05", "D" }, // Latin letter small capital D (ᴅ)

                // Letters E
                { "\uFF25", "E" }, // Fullwidth Latin capital letter E (Ｅ)
                { "\u1D07", "E" }, // Latin letter small capital E (ᴇ)

                // Letters F
                { "\uFF26", "F" }, // Fullwidth Latin capital letter F (Ｆ)
                { "\uA730", "F" }, // Latin letter small capital F (ꜰ)

                // Letters G
                { "\uFF27", "G" }, // Fullwidth Latin capital letter G (Ｇ)
                { "\u0262", "G" }, // Latin letter small capital G (ɢ)

                // Letters H
                { "\uFF28", "H" }, // Fullwidth Latin capital letter H (Ｈ)
                { "\u029C", "H" }, // Latin letter small capital H (ʜ)

                // Letters I
                { "\uFF29", "I" }, // Fullwidth Latin capital letter I (Ｉ)
                { "\u026A", "I" }, // Latin letter small capital I (ɪ)

                // Letters J
                { "\uFF2A", "J" }, // Fullwidth Latin capital letter J (Ｊ)
                { "\u1D0A", "J" }, // Latin letter small capital J (ᴊ)

                // Letters K
                { "\uFF2B", "K" }, // Fullwidth Latin capital letter K (Ｋ)
                { "\u1D0B", "K" }, // Latin letter small capital K (ᴋ)

                // Letters L
                { "\uFF2C", "L" }, // Fullwidth Latin capital letter L (Ｌ)
                { "\u029F", "L" }, // Latin letter small capital L (ʟ)

                // Letters M
                { "\uFF2D", "M" }, // Fullwidth Latin capital letter M (Ｍ)
                { "\u1D0D", "M" }, // Latin letter small capital M (ᴍ)

                // Letters N
                { "\uFF2E", "N" }, // Fullwidth Latin capital letter N (Ｎ)
                { "\u0274", "N" }, // Latin letter small capital N (ɴ)

                // Letters O
                { "\uFF2F", "O" }, // Fullwidth Latin capital letter O (Ｏ)
                { "\u1D0F", "O" }, // Latin letter small capital O (ᴏ)

                // Letters P
                { "\uFF30", "P" }, // Fullwidth Latin capital letter P (Ｐ)
                { "\u1D18", "P" }, // Latin letter small capital P (ᴘ)

                // Letters Q
                { "\uFF31", "Q" }, // Fullwidth Latin capital letter Q (Ｑ)

                // Letters R
                { "\uFF32", "R" }, // Fullwidth Latin capital letter R (Ｒ)
                { "\u0280", "R" }, // Latin letter small capital R (ʀ)

                // Letters S
                { "\uFF33", "S" }, // Fullwidth Latin capital letter S (Ｓ)
                { "\uA731", "S" }, // Latin letter small capital S (ꜱ)

                // Letters T
                { "\uFF34", "T" }, // Fullwidth Latin capital letter T (Ｔ)
                { "\u1D1B", "T" }, // Latin letter small capital T (ᴛ)

                // Letters U
                { "\uFF35", "U" }, // Fullwidth Latin capital letter U (Ｕ)
                { "\u1D1C", "U" }, // Latin letter small capital U (ᴜ)

                // Letters V
                { "\uFF36", "V" }, // Fullwidth Latin capital letter V (Ｖ)
                { "\u1D20", "V" }, // Latin letter small capital V (ᴠ)

                // Letters W
                { "\uFF37", "W" }, // Fullwidth Latin capital letter W (Ｗ)
                { "\u1D21", "W" }, // Latin letter small capital W (ᴡ)

                // Letters X
                { "\uFF38", "X" }, // Fullwidth Latin capital letter X (Ｘ)

                // Letters Y
                { "\uFF39", "Y" }, // Fullwidth Latin capital letter Y (Ｙ)
                { "\u028F", "Y" }, // Latin letter small capital Y (ʏ)

                // Letters Z
                { "\uFF3A", "Z" }, // Fullwidth Latin capital letter Z (Ｚ)
                { "\u1D22", "Z" }, // Latin letter small capital Z (ᴢ)

                // Carets and modifiers
                { "\u02C6", "^" }, // Modifier letter circumflex accent (ˆ)
                { "\u0302", "^" }, // Combining circumflex accent ( ̂)
                { "\uFF3E", "^" }, // Fullwidth circumflex accent (＾)
                { "\u1DCD", "^" }, // Combining double circumflex above ( ͍̂ note: double circumflex)

                // Braces
                { "\u2774", "{" }, // Medium left curly bracket ornament (❴)
                { "\uFE5B", "{" }, // Small left curly bracket (﹛)
                { "\uFF5B", "{" }, // Fullwidth left curly bracket (｛)

                { "\u2775", "}" }, // Medium right curly bracket ornament (❵)
                { "\uFE5C", "}" }, // Small right curly bracket (﹜)
                { "\uFF5D", "}" }, // Fullwidth right curly bracket (｝)

                // Brackets
                { "\uFF3B", "[" }, // Fullwidth left square bracket (［)
                { "\uFF3D", "]" }, // Fullwidth right square bracket (］)

                // Tildes and similar
                { "\u02DC", "~" }, // Small tilde (˜)
                { "\u02F7", "~" }, // Modifier letter low tilde (˷)
                { "\u0303", "~" }, // Combining tilde ( ̃)
                { "\u0330", "~" }, // Combining tilde below ( ̰)
                { "\u0334", "~" }, // Combining tilde overlay ( ̴)
                { "\u223C", "~" }, // Tilde operator (∼)
                { "\uFF5E", "~" }, // Fullwidth tilde (～)

                // Spaces and separators to empty
                { "\u2001", string.Empty }, // Em quad
                { "\u2002", string.Empty }, // En space
                { "\u2003", string.Empty }, // Em space
                { "\u2004", string.Empty }, // Three per em space
                { "\u2005", string.Empty }, // Four per em space
                { "\u2006", string.Empty }, // Six per em space
                { "\u2007", string.Empty }, // Figure space
                { "\u2008", string.Empty }, // Punctuation space
                { "\u2009", string.Empty }, // Thin space
                { "\u200A", string.Empty }, // Hair space
                { "\u200B", string.Empty }, // Zero width space
                { "\u202F", string.Empty }, // Narrow no break space
                { "\u205F", string.Empty }, // Medium mathematical space
                { "\u3000", string.Empty }, // Ideographic space
                { "\uFEFF", string.Empty }, // Zero width no break space
                { "\u008D", string.Empty }, // C1 control: Reverse line feed
                { "\u009F", string.Empty }, // C1 control: Application program command
                { "\u0080", string.Empty }, // C1 control: Padding character
                { "\u0090", string.Empty }, // C1 control: Device control string
                { "\u009B", string.Empty }, // C1 control: Control sequence introducer
                { "\u0010", string.Empty }, // C0 control: Data link escape
                { "\u0009", string.Empty }, // C0 control: Horizontal tab
                { "\u0000", string.Empty }, // C0 control: Null
                { "\u0003", string.Empty }, // C0 control: End of text
                { "\u0004", string.Empty }, // C0 control: End of transmission
                { "\u0017", string.Empty }, // C0 control: End of transmission block
                { "\u0019", string.Empty }, // C0 control: End of medium
                { "\u0011", string.Empty }, // C0 control: Device control one
                { "\u0012", string.Empty }, // C0 control: Device control two
                { "\u0013", string.Empty }, // C0 control: Device control three
                { "\u0014", string.Empty }, // C0 control: Device control four
                { "\u2028", string.Empty }, // Line separator
                { "\u2029", string.Empty }, // Paragraph separator
                { "\u2060", string.Empty }, // Word joiner

                // Double punctuation expansions
                { "\u203C", "!!" }, // Double exclamation mark (‼)

                // Ellipsis
                { "\u2026", "..." }, // Horizontal ellipsis (…)
            };

            // Aggregate the replacements into character classes to improve performance.
            var aggregatedReplacements = AggregateReplacements( replacements );

            foreach ( var replacement in aggregatedReplacements )
            {
                text = Regex.Replace( text, replacement.Key, replacement.Value );
            }

            return text;
        }

        /// <summary>
        /// Aggregates a dictionary of character replacements into a new dictionary where keys are grouped by their
        /// replacement values, forming regex-compatible character classes.
        /// </summary>
        /// <param name="replacements">A dictionary where each key-value pair represents a character and its replacement.</param>
        /// <returns>A dictionary where each key is a regex character class representing characters to be replaced, and each
        /// value is the replacement character.</returns>
        private static Dictionary<string, string> AggregateReplacements( Dictionary<string, string> replacements )
        {
            // Group all entries by their replacement value (the target character)
            var grouped = replacements
                .GroupBy( kvp => kvp.Value )
                .ToDictionary(
                    g => $"[{string.Concat( g.Select( kvp => kvp.Key ) )}]",
                    g => g.Key == "\"" || g.Key == "'" ? g.First().Value : g.Key // ensures correct mapping type
                );

            // Now properly escape regex special characters inside the brackets
            var aggregated = new Dictionary<string, string>();
            foreach ( var kvp in replacements.GroupBy( kvp => kvp.Value ) )
            {
                var chars = string.Concat( kvp.Select( x =>
                {
                    // Escape characters that have special meaning in regex char classes
                    var c = x.Key;
                    
                    if ( c == "\\" || c == "]" || c == "^" || c == "-" )
                    {
                        c = "\\" + c;
                    }

                    return c;
                } ) );

                aggregated.Add( $"[{chars}]", kvp.Key );
            }

            return aggregated;
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
