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

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication to a custom API endpoint for testing.
    /// </summary>
    [Description( "Sends a communication to a custom API endpoint for testing." )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "SMS Test" )]
    [Rock.SystemGuid.EntityTypeGuid( "c50fb8f9-6ada-4c4b-88ee-ed7bc93b1819" )]
    public class SmsTest : TransportComponent, IAsyncTransport
    {
        /// <inheritdoc/>
        public int MaxParallelization => 10;

        /// <inheritdoc/>
        public override bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var sendMessageResult = AsyncHelper.RunSync( () => SendAsync( rockMessage, mediumEntityTypeId, mediumAttributes ) );

            errorMessages.AddRange( sendMessageResult.Errors );

            return !errorMessages.Any();
        }

        /// <inheritdoc/>
        public override void Send( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            AsyncHelper.RunSync( () => SendAsync( communication, mediumEntityTypeId, mediumAttributes ) );
        }

        /// <inheritdoc/>
        public async Task SendAsync( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var fromPhone = string.Empty;
            var unprocessedRecipientCount = 0;
            var mergeFields = new Dictionary<string, object>();
            Person currentPerson = null;
            var attachments = new List<BinaryFile>();
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
                    attachments = new BinaryFileService( rockContext ).GetByIds( smsAttachmentsBinaryFileIdList ).ToList();
                }
            }

            var globalAttributes = GlobalAttributesCache.Get();
            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );

            var sendingTask = new List<Task>( unprocessedRecipientCount );
            var maxParallelization = MaxParallelization;

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

                    sendingTask.Add( ThrottleHelper.ThrottledExecute( () => SendToCommunicationRecipient( communication, fromPhone, mergeFields, currentPerson, attachments, personEntityTypeId, communicationCategoryId, communicationEntityTypeId, publicAppRoot, recipient ), mutex ) );
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

        /// <inheritdoc/>
        public async Task<SendMessageResult> SendAsync( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var sendMessageResult = new SendMessageResult();

            if ( rockMessage is RockSMSMessage smsMessage )
            {
                // Validate From Number
                if ( smsMessage.FromSystemPhoneNumber == null )
                {
                    sendMessageResult.Errors.Add( "A From Number was not provided." );
                    return sendMessageResult;
                }

                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                var sendingTask = new List<Task<SendMessageResult>>();

                using ( var mutex = new SemaphoreSlim( MaxParallelization ) )
                {
                    foreach ( var recipient in rockMessage.GetRecipients() )
                    {
                        var startMutexWait = System.Diagnostics.Stopwatch.StartNew();
                        await mutex.WaitAsync().ConfigureAwait( false );
                        sendingTask.Add( ThrottleHelper.ThrottledExecute( () => SendToRecipientAsync( recipient, mergeFields, smsMessage, rockMessage.Attachments, mediumEntityTypeId, mediumAttributes ), mutex ) );
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

            return sendMessageResult;
        }

        private async Task<SendMessageResult> SendToRecipientAsync( RockMessageRecipient recipient, Dictionary<string, object> mergeFields, RockSMSMessage smsMessage, List<BinaryFile> attachments, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
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
                        if ( attachments.Any() )
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
                        try
                        {
                            await SendToApiAsync( smsMessage.FromSystemPhoneNumber.Number, attachments, message, recipient.To ).ConfigureAwait( false );

                            sendMessageResult.MessagesSent += 1;
                        }
                        catch ( Exception ex )
                        {
                            sendMessageResult.Errors.Add( ex.Message );
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

        private async Task SendToCommunicationRecipient( Model.Communication communication, string fromPhone, Dictionary<string, object> mergeFields, Person currentPerson, List<BinaryFile> attachments, int personEntityTypeId, int communicationCategoryId, int communicationEntityTypeId, string publicAppRoot, CommunicationRecipient recipient )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    recipient = new CommunicationRecipientService( rockContext ).Get( recipient.Id );
                    var smsNumber = recipient.PersonAlias.Person.PhoneNumbers.GetFirstSmsNumber();
                    if ( !string.IsNullOrWhiteSpace( smsNumber ) )
                    {
                        // Create merge field dictionary
                        var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                        string message = ResolveText( communication.SMSMessage, currentPerson, recipient, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                        await SendToApiAsync( fromPhone, attachments, message, smsNumber ).ConfigureAwait( false );

                        recipient.Status = CommunicationRecipientStatus.Delivered;
                        recipient.SendDateTime = RockDateTime.Now;
                        recipient.TransportEntityTypeName = this.GetType().FullName;
                        recipient.UniqueMessageId = Guid.NewGuid().ToString();

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
                catch ( Exception ex )
                {
                    recipient.Status = CommunicationRecipientStatus.Failed;
                    recipient.StatusNote = "SMS Exception: " + ex.Message;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Sends to the API endpoint.
        /// </summary>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="attachments">The attachment media urls.</param>
        /// <param name="message">The message.</param>
        /// <param name="toNumber">The destination number.</param>
        private async Task SendToApiAsync( string fromPhone, List<BinaryFile> attachments, string message, string toNumber )
        {
            await RealTime.Topics.TestCommunicationTransportTopic.PostSmsMessage( toNumber, fromPhone, message, attachments );
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
    }
}
