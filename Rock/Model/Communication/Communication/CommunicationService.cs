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
using System.Linq;
using System.Threading.Tasks;

using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Communication POCO Service class
    /// </summary>
    public partial class CommunicationService
    {
        /// <summary>
        /// Creates the email communication
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="bulkCommunication">if set to <c>true</c> [bulk communication].</param>
        /// <param name="sendDateTime">The send date time.</param>
        /// <param name="recipientStatus">The recipient status.</param>
        /// <param name="senderPersonAliasId">The sender person alias identifier.</param>
        /// <returns></returns>
        public Communication CreateEmailCommunication
        (
            List<RockEmailMessageRecipient> recipients,
            string fromName,
            string fromAddress,
            string replyTo,
            string subject,
            string message,
            bool bulkCommunication,
            DateTime? sendDateTime,
            CommunicationRecipientStatus recipientStatus = CommunicationRecipientStatus.Delivered,
            int? senderPersonAliasId = null )
        {
            return CreateEmailCommunication( new CreateEmailCommunicationArgs
            {
                Recipients = recipients,
                FromName = fromName,
                FromAddress = fromAddress,
                ReplyTo = replyTo,
                Subject = subject,
                Message = message,
                BulkCommunication = bulkCommunication,
                SendDateTime = sendDateTime,
                RecipientStatus = recipientStatus,
                SenderPersonAliasId = senderPersonAliasId,
                SystemCommunicationId = null
            } );
        }

        /// <summary>
        ///
        /// </summary>
        public sealed class CreateEmailCommunicationArgs
        {
            /// <summary>
            /// Gets or sets the recipients.
            /// </summary>
            /// <value>
            /// The recipients.
            /// </value>
            public List<RockEmailMessageRecipient> Recipients { get; set; }

            /// <summary>
            /// Gets or sets from name.
            /// </summary>
            /// <value>
            /// From name.
            /// </value>
            public string FromName { get; set; }

            /// <summary>
            /// Gets or sets from address.
            /// </summary>
            /// <value>
            /// From address.
            /// </value>
            public string FromAddress { get; set; }

            /// <summary>
            /// Gets or sets the reply to.
            /// </summary>
            /// <value>
            /// The reply to.
            /// </value>
            public string ReplyTo { get; set; }

            /// <summary>
            /// Gets or sets the subject.
            /// </summary>
            /// <value>
            /// The subject.
            /// </value>
            public string Subject { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [bulk communication].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [bulk communication]; otherwise, <c>false</c>.
            /// </value>
            public bool BulkCommunication { get; set; }

            /// <summary>
            /// Gets or sets the send date time.
            /// </summary>
            /// <value>
            /// The send date time.
            /// </value>
            public DateTime? SendDateTime { get; set; }

            /// <summary>
            /// Gets or sets the recipient status.
            /// </summary>
            /// <value>
            /// The recipient status.
            /// </value>
            public CommunicationRecipientStatus RecipientStatus { get; set; } = CommunicationRecipientStatus.Delivered;

            /// <summary>
            /// Gets or sets the sender person alias identifier.
            /// </summary>
            /// <value>
            /// The sender person alias identifier.
            /// </value>
            public int? SenderPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the system communication identifier.
            /// </summary>
            /// <value>
            /// The system communication identifier.
            /// </value>
            public int? SystemCommunicationId { get; set; }
        }

        /// <summary>
        /// Creates the email communication.
        /// </summary>
        /// <param name="createEmailCommunicationArgs">The create email communication arguments.</param>
        /// <returns></returns>
        public Communication CreateEmailCommunication( CreateEmailCommunicationArgs createEmailCommunicationArgs )
        {
            var recipients = createEmailCommunicationArgs.Recipients;
            var senderPersonAliasId = createEmailCommunicationArgs.SenderPersonAliasId;
            var fromName = createEmailCommunicationArgs.FromName;
            var fromAddress = createEmailCommunicationArgs.FromAddress;
            var replyTo = createEmailCommunicationArgs.ReplyTo;
            var subject = createEmailCommunicationArgs.Subject;
            var message = createEmailCommunicationArgs.Message;
            var bulkCommunication = createEmailCommunicationArgs.BulkCommunication;
            var sendDateTime = createEmailCommunicationArgs.SendDateTime;
            var recipientStatus = createEmailCommunicationArgs.RecipientStatus;
            var systemCommunicationId = createEmailCommunicationArgs.SystemCommunicationId;

            var recipientsWithPersonIds = recipients.Where( a => a.PersonId.HasValue ).Select( a => a.PersonId ).ToList();
            var recipientEmailsUnknownPersons = recipients.Where( a => a.PersonId == null ).Select( a => a.EmailAddress );

            /*
             * 4-MAY-2022 DMV
             *
             * In tracking down alleged duplicate communications we discovered
             * that duplicates could be sent to the same person if they are in the
             * recipient list more that once with mulitple Person Alias IDs.
             * This could have occured through a person merge or other data changes
             * in Rock. This code removes those duplicates from the list before
             * sending the communication.
             *
             */

            var recipientPersonList = new PersonAliasService( ( RockContext ) Context )
                .GetPrimaryAliasQuery()
                .Where( pa => recipientsWithPersonIds.Contains( pa.PersonId ) )
                .Select( a => a.Person )
                .ToList();

            if ( !recipientPersonList.Any() && recipientEmailsUnknownPersons.Any( a => a != null ) )
            {
                // For backwards compatibility, if no PersonIds where specified, but there are recipients that are only specified by EmailAddress, take a guess at the personIds by looking for matching email addresses
                recipientPersonList = new PersonService( ( RockContext ) Context )
                .Queryable()
                .Where( p => recipientEmailsUnknownPersons.Contains( p.Email ) )
                .ToList();
            }

            if ( !recipientPersonList.Any() )
            {
                return null;
            }

            var communication = new Communication
            {
                CommunicationType = CommunicationType.Email,
                Status = CommunicationStatus.Approved,
                ReviewedDateTime = RockDateTime.Now,
                ReviewerPersonAliasId = senderPersonAliasId,
                SenderPersonAliasId = senderPersonAliasId
            };

            communication.FromName = fromName.TrimForMaxLength( communication, "FromName" );
            communication.FromEmail = fromAddress.TrimForMaxLength( communication, "FromEmail" );
            communication.ReplyToEmail = replyTo.TrimForMaxLength( communication, "ReplyToEmail" );
            communication.Subject = subject.TrimForMaxLength( communication, "Subject" );
            communication.Message = message;
            communication.IsBulkCommunication = bulkCommunication;
            communication.FutureSendDateTime = null;
            communication.SendDateTime = sendDateTime;
            communication.SystemCommunicationId = systemCommunicationId;
            Add( communication );

            // add each person as a recipient to the communication
            foreach ( var person in recipientPersonList )
            {
                var personAliasId = person.PrimaryAliasId;
                if ( !personAliasId.HasValue )
                    continue;

                var communicationRecipient = new CommunicationRecipient
                {
                    PersonAliasId = personAliasId.Value,
                    Status = recipientStatus,
                    SendDateTime = sendDateTime
                };
                communication.Recipients.Add( communicationRecipient );
            }

            return communication;

        }

        /// <summary>
        /// Creates an SMS communication with a CommunicationRecipient and adds it to the context.
        /// </summary>
        /// <param name="fromPerson">the Sender for the communication (For the communication.SenderPersonAlias). If null the name for the communication will be From: unknown person.</param>
        /// <param name="toPersonAliasId">To person alias identifier. If null the CommunicationRecipient is not created</param>
        /// <param name="message">The message.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="responseCode">The response code. If null/empty/whitespace then one is generated</param>
        /// <param name="communicationName">Name of the communication.</param>
        /// <returns></returns>
        [Obsolete( "Use the CreateSMSCommunication() method that takes a SystemPhoneNumberCache parameter." )]
        [RockObsolete( "1.15" )]
        public Communication CreateSMSCommunication( Person fromPerson, int? toPersonAliasId, string message, DefinedValueCache fromPhone, string responseCode, string communicationName )
        {
            var args = new CreateSMSCommunicationArgs
            {
                FromPerson = fromPerson,
                ToPersonAliasId = toPersonAliasId,
                CommunicationName = communicationName,
                FromPhone = fromPhone,
                Message = message,
                ResponseCode = responseCode,
                SystemCommunicationId = null
            };

            return CreateSMSCommunication( args );
        }

        /// <summary>
        /// Creates an SMS communication with a CommunicationRecipient and adds it to the context.
        /// </summary>
        /// <param name="fromPerson">the Sender for the communication (For the communication.SenderPersonAlias). If null the name for the communication will be From: unknown person.</param>
        /// <param name="toPersonAliasId">To person alias identifier. If null the CommunicationRecipient is not created</param>
        /// <param name="message">The message.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="responseCode">The response code. If null/empty/whitespace then one is generated</param>
        /// <param name="communicationName">Name of the communication.</param>
        /// <returns></returns>
        public Communication CreateSMSCommunication( Person fromPerson, int? toPersonAliasId, string message, SystemPhoneNumberCache fromPhone, string responseCode, string communicationName )
        {
            var args = new CreateSMSCommunicationArgs
            {
                FromPerson = fromPerson,
                ToPersonAliasId = toPersonAliasId,
                CommunicationName = communicationName,
                FromSystemPhoneNumber = fromPhone,
                Message = message,
                ResponseCode = responseCode,
                SystemCommunicationId = null
            };

            return CreateSMSCommunication( args );
        }

        /// <summary>
        ///
        /// </summary>
        public sealed class CreateSMSCommunicationArgs
        {
            /// <summary>
            /// Gets or sets from person.
            /// </summary>
            /// <value>
            /// From person.
            /// </value>
            public Person FromPerson { get; set; }

            /// <summary>
            /// Converts to personaliasid.
            /// </summary>
            /// <value>
            /// To person alias identifier.
            /// </value>
            public int? ToPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets from phone.
            /// </summary>
            /// <value>
            /// From phone.
            /// </value>
            [Obsolete( "Use FromSystemPhoneNumber instead." )]
            [RockObsolete( "1.15" )]
            public DefinedValueCache FromPhone
            {
                // The old SMS values are synced and use the same Guids as the new model.
                get => FromSystemPhoneNumber != null ? DefinedValueCache.Get( FromSystemPhoneNumber.Guid ) : null;
                set => FromSystemPhoneNumber = SystemPhoneNumberCache.Get( value.Guid );
            }

            /// <summary>
            /// Gets or sets the system phone number the message will be sent from.
            /// </summary>
            /// <value>The system phone number the message will be sent from.</value>
            public SystemPhoneNumberCache FromSystemPhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the response code.
            /// </summary>
            /// <value>
            /// The response code.
            /// </value>
            public string ResponseCode { get; set; }

            /// <summary>
            /// Gets or sets the name of the communication.
            /// </summary>
            /// <value>
            /// The name of the communication.
            /// </value>
            public string CommunicationName { get; set; }

            /// <summary>
            /// Gets or sets the system communication identifier.
            /// </summary>
            /// <value>
            /// The system communication identifier.
            /// </value>
            public int? SystemCommunicationId { get; set; }
        }

        /// <summary>
        /// Creates an SMS communication with a CommunicationRecipient and adds it to the context.
        /// </summary>
        /// <param name="createSMSCommunicationArgs">The create SMS communication arguments.</param>
        /// <returns></returns>
        public Communication CreateSMSCommunication( CreateSMSCommunicationArgs createSMSCommunicationArgs )
        {
            RockContext rockContext = ( RockContext ) this.Context;
            var responseCode = createSMSCommunicationArgs.ResponseCode;
            var communicationName = createSMSCommunicationArgs.CommunicationName;
            var fromPerson = createSMSCommunicationArgs.FromPerson;
            var message = createSMSCommunicationArgs.Message;
            var fromPhone = createSMSCommunicationArgs.FromSystemPhoneNumber;
            var systemCommunicationId = createSMSCommunicationArgs.SystemCommunicationId;
            var toPersonAliasId = createSMSCommunicationArgs.ToPersonAliasId;

            if ( responseCode.IsNullOrWhiteSpace() )
            {
                responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );
            }

            // add communication for reply
            var communication = new Rock.Model.Communication
            {
                Name = communicationName,
                CommunicationType = CommunicationType.SMS,
                Status = CommunicationStatus.Approved,
                ReviewedDateTime = RockDateTime.Now,
                // NOTE: if this communication was created from a mobile device, fromPerson should never be null since a Nameless Person record should have been created if a regular person record wasn't found
                ReviewerPersonAliasId = fromPerson?.PrimaryAliasId,
                SenderPersonAliasId = fromPerson?.PrimaryAliasId,
                IsBulkCommunication = false,
                SMSMessage = message,
                SmsFromSystemPhoneNumberId = fromPhone.Id,
                SystemCommunicationId = systemCommunicationId
            };

            if ( toPersonAliasId != null )
            {
                var recipient = new Rock.Model.CommunicationRecipient();
                recipient.Status = CommunicationRecipientStatus.Pending;
                recipient.PersonAliasId = toPersonAliasId.Value;
                recipient.ResponseCode = responseCode;
                recipient.MediumEntityTypeId = EntityTypeCache.Get( "Rock.Communication.Medium.Sms" ).Id;
                recipient.SentMessage = message;
                communication.Recipients.Add( recipient );
            }

            Add( communication );
            return communication;
        }

        /// <summary>
        /// Gets the queued communications.
        /// </summary>
        /// <param name="expirationDays">The expiration days.</param>
        /// <param name="delayMinutes">The delay minutes.</param>
        /// <param name="includeFuture">if set to <c>true</c> [include future].</param>
        /// <param name="includePendingApproval">if set to <c>true</c> communications that haven't been approved yet will be included.</param>
        /// <returns></returns>
        public IQueryable<Communication> GetQueued( int expirationDays, int delayMinutes, bool includeFuture, bool includePendingApproval )
        {
            var beginWindow = RockDateTime.Now.AddDays( 0 - expirationDays );
            var endWindow = RockDateTime.Now.AddMinutes( 0 - delayMinutes );
            var currentDateTime = RockDateTime.Now;

            // Conditions for communications that should be queued for Sending (indicated by includeFuture == false and includePending == false)
            // -  communications that haven't been sent yet (SendDateTime is null)
            // -  communication is approved (or includePendingApproval == false)
            // - FutureSendDateTime is not set (not scheduled), and communication was created within a reasonable window based on expiration days (for example, no older than 3 days ago)
            //   - OR - FutureSendDateTime IS set (scheduled), and the FutureSendDateTime is Now (or within the expiration window)

            // Limit to communications that haven't been sent yet
            var queuedQry = Queryable().Where( c => !c.SendDateTime.HasValue );

            var qryPendingRecipients = new CommunicationRecipientService( ( RockContext ) Context )
                .Queryable()
                .Where( a => a.Status == CommunicationRecipientStatus.Pending )
                .Select( cr => new { Id = cr.CommunicationId } );

            if ( includePendingApproval )
            {
                // Also limit to communications that are Approved or Pending Approval
                queuedQry = queuedQry.Where( c => c.Status == CommunicationStatus.Approved || c.Status == CommunicationStatus.PendingApproval );
            }
            else
            {
                // Also limit to communications that are Approved
                queuedQry = queuedQry.Where( c => c.Status == CommunicationStatus.Approved );
            }

            if ( includeFuture )
            {
                // Also limit to communications that have either been created within a reasonable timeframe (typically no older than 3 days ago) or are Scheduled to be sent at some point
                queuedQry = queuedQry.Where( c =>
                    // Use the reviewed date to get the communications to send
                    ( !c.FutureSendDateTime.HasValue && c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value >= beginWindow && c.ReviewedDateTime.Value <= endWindow )
                    // Use the created date to get the communications to send this is for communications that are created by legacy plugins that have switched to using reviewed date.
                    || ( !c.FutureSendDateTime.HasValue && !c.ReviewedDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    // Get all future communications.
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow ) );
            }
            else
            {
                // Also limit to communications that have either been created within a reasonable timeframe (typically no older than 3 days ago)
                // or are Scheduled to be sent (also within that reasonable timeframe. In other words, if it was scheduled to be sent, but stil hasn't been sent 3 days after it was scheduled, don't include it)
                queuedQry = queuedQry.Where( c =>
                    // Use the reviewed date to get the communications to send
                    ( !c.FutureSendDateTime.HasValue && c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value >= beginWindow && c.ReviewedDateTime.Value <= endWindow )
                    // Use the created date to get the communications to send this is for communications that are created by legacy plugins that have switched to using reviewed date.
                    || ( !c.FutureSendDateTime.HasValue && !c.ReviewedDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    // Get all future communication that are need to be sent.
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow && c.FutureSendDateTime.Value <= currentDateTime ) );
            }

            // just in case SendDateTime is null (pre-v8 communication), also limit to communications that either have a ListGroupId or has PendingRecipients
            var listGroupQuery = Queryable().Where( c => c.ListGroupId.HasValue ).Select( c => new { c.Id } );
            var communicationListQry = qryPendingRecipients.Union( listGroupQuery );

            var returnQry = Queryable()
                .Where( c => queuedQry.Any( c2 => c2.Id == c.Id ) )
                .Where( c => communicationListQry.Any( c2 => c2.Id == c.Id ) );
            return returnQry;
        }

        /// <summary>
        /// Send all real time notifications for an outbound SMS message on
        /// a new background Task.
        /// </summary>
        /// <param name="communicationRecipientId">The identifier of the <see cref="CommunicationRecipient"/> object that is the source of the notification.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal static void SendOutboundSmsRealTimeNotificationsInBackground( int communicationRecipientId )
        {
            Task.Run( async () =>
            {
                try
                {
                    await SendOutboundSmsRealTimeNotificationsAsync( communicationRecipientId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }

        /// <summary>
        /// Send all real time notifications for an outbound SMS message.
        /// </summary>
        /// <param name="communicationRecipientId">The identifier of the <see cref="CommunicationRecipient"/> object that is the source of the notification.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal static async Task SendOutboundSmsRealTimeNotificationsAsync( int communicationRecipientId )
        {
            using ( var rockContext = new RockContext() )
            {
                var messageBag = new CommunicationRecipientService( rockContext )
                    .GetConversationMessageBag( communicationRecipientId );

                var channelName = RealTime.Topics.ConversationParticipantTopic.GetChannelForConversationKey( messageBag.ConversationKey );

                await RealTime.RealTimeHelper.GetTopicContext<RealTime.Topics.IConversationParticipant>()
                    .Clients
                    .Channel( channelName )
                    .NewSmsMessage( messageBag );
            }
        }

        /// <summary>
        /// Send all real time notifications for an inbound SMS message on
        /// a new background Task.
        /// </summary>
        /// <param name="communicationResponseId">The identifier of the <see cref="CommunicationResponse"/> object that is the source of the notification.</param>
        internal static void SendInboundSmsRealTimeNotificationsInBackground( int communicationResponseId )
        {
            Task.Run( async () =>
            {
                try
                {
                    await SendInboundSmsRealTimeNotificationsAsync( communicationResponseId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }

        /// <summary>
        /// Send all real time notifications for an inbound SMS message.
        /// </summary>
        /// <param name="communicationResponseId">The identifier of the <see cref="CommunicationResponse"/> object that is the source of the notification.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal static async Task SendInboundSmsRealTimeNotificationsAsync( int communicationResponseId )
        {
            using ( var rockContext = new RockContext() )
            {
                var messageBag = new CommunicationResponseService( rockContext )
                    .GetConversationMessageBag( communicationResponseId );

                var channelName = RealTime.Topics.ConversationParticipantTopic.GetChannelForConversationKey( messageBag.ConversationKey );

                await RealTime.RealTimeHelper.GetTopicContext<RealTime.Topics.IConversationParticipant>()
                    .Clients
                    .Channel( channelName )
                    .NewSmsMessage( messageBag );
            }
        }

        /// <summary>
        /// Send all real time notifications for a conversation that has been
        /// marked as read on a new background Task.
        /// </summary>
        /// <param name="conversationKey">The key that identifies the conversation that was read.</param>
        internal static void SendConversationReadSmsRealTimeNotificationsInBackground( string conversationKey )
        {
            Task.Run( async () =>
            {
                try
                {
                    await SendConversationReadSmsRealTimeNotificationsAsync( conversationKey );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }

        /// <summary>
        /// Send all real time notifications for a conversation that has been marked as read.
        /// </summary>
        /// <param name="conversationKey">The key that identifies the conversation that was read.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal static async Task SendConversationReadSmsRealTimeNotificationsAsync( string conversationKey )
        {
            var channelName = RealTime.Topics.ConversationParticipantTopic.GetChannelForConversationKey( conversationKey );

            await RealTime.RealTimeHelper.GetTopicContext<RealTime.Topics.IConversationParticipant>()
                .Clients
                .Channel( channelName )
                .ConversationMarkedAsRead( conversationKey );
        }

        /// <summary>
        /// Sends out a push notification to all of the devices that should
        /// receive one about the response that was just submitted. This
        /// operation is performed on a new background Task.
        /// </summary>
        /// <param name="communicationResponseId">The identifier of the <see cref="CommunicationResponse"/> object that is the source of the notification.</param>
        internal static void SendInboundSmsPushPushNotificationsInBackground( int communicationResponseId )
        {
            Task.Run( async () =>
            {
                try
                {
                    await SendInboundSmsPushPushNotificationsAsync( communicationResponseId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }

        /// <summary>
        /// Sends out a push notification to all of the devices that should
        /// receive one about the response that was just submitted.
        /// </summary>
        /// <param name="communicationResponseId">The identifier of the <see cref="CommunicationResponse"/> object that is the source of the notification.</param>
        /// <returns>A task that represents this operation.</returns>
        internal static async Task SendInboundSmsPushPushNotificationsAsync( int communicationResponseId )
        {
            if ( !MediumContainer.HasActivePushTransport() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                // Find the response and pull out just the data we need.
                var response = new CommunicationResponseService( rockContext )
                    .Queryable()
                    .Where( cr => cr.Id == communicationResponseId )
                    .Select( cr => new
                    {
                        cr.RelatedSmsFromSystemPhoneNumberId,
                        cr.FromPersonAlias.Person,
                        Message = cr.Response,
                        cr.MessageKey
                    } )
                    .FirstOrDefault();

                // Find the system phone number it was sent to.
                var systemPhoneNumber = response.RelatedSmsFromSystemPhoneNumberId.HasValue
                    ? SystemPhoneNumberCache.Get( response.RelatedSmsFromSystemPhoneNumberId.Value )
                    : null;

                // Shouldn't happen, but it's possible the phone number was deleted
                // before we processed to this point or the site is misconfigured.
                if ( systemPhoneNumber == null )
                {
                    return;
                }

                // Make sure the phone number is configured for push notifications.
                if ( !systemPhoneNumber.SmsNotificationGroupId.HasValue || !systemPhoneNumber.MobileApplicationSiteId.HasValue )
                {
                    return;
                }

                // Build a sub-query to get all the person identifiers that
                // should receive a push notification.
                var personIdQry = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( gm => gm.GroupId == systemPhoneNumber.SmsNotificationGroupId.Value
                        && gm.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( gm => gm.PersonId );

                // Get all the registrations for devices belonging to one of
                // these people and that are attached to the mobile application site.
                var deviceRegistrationIds = new PersonalDeviceService( rockContext )
                    .Queryable()
                    .Where( pd => personIdQry.Contains( pd.PersonAlias.PersonId )
                        && pd.NotificationsEnabled
                        && pd.SiteId == systemPhoneNumber.MobileApplicationSiteId.Value
                        && !string.IsNullOrEmpty( pd.DeviceRegistrationId ) )
                    .Select( pd => pd.DeviceRegistrationId )
                    .Distinct()
                    .ToList();

                var pushMessage = new RockPushMessage
                {
                    Message = response.Message?.Truncate( 100 ),
                    Data = new PushData()
                };

                // If no message content then it must be an attachment.
                if ( pushMessage.Message.IsNullOrWhiteSpace() )
                {
                    pushMessage.Message = "Sent an attachment.";
                }

                // Use either the person's name or the phone number it was sent from.
                if ( !response.Person.IsNameless() )
                {
                    pushMessage.Title = response.Person.FullName;
                }
                else
                {
                    pushMessage.Title = PhoneNumber.FormattedNumber( null, response.MessageKey );
                }

                var conversationKey = GetSmsConversationKey( systemPhoneNumber.Guid, response.Person.Guid );

                pushMessage.Data.CustomData = new Dictionary<string, string>
                {
                    ["Rock-SmsConversationKey"] = conversationKey,
                    ["Rock-SmsConversationPhoneNumberGuid"] = systemPhoneNumber.Guid.ToString(),
                    ["Rock-SmsConversationPersonGuid"] = response.Person.Guid.ToString()
                };

                var mergeFields = new Dictionary<string, object>();

                pushMessage.SetRecipients( deviceRegistrationIds.Select( d => RockPushMessageRecipient.CreateAnonymous( d, mergeFields ) ).ToList() );

                await pushMessage.SendAsync();
            }
        }

        /// <summary>
        /// Gets the conversation key used for an SMS conversation between a Rock
        /// phone number and a Person.
        /// </summary>
        /// <param name="rockPhoneNumberGuid">The rock phone number unique identifier.</param>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns>A string that represents the unique conversation key.</returns>
        internal static string GetSmsConversationKey( Guid rockPhoneNumberGuid, Guid personGuid )
        {
            return $"SMS:{rockPhoneNumberGuid}:{personGuid}";
        }

        /// <summary>
        /// Gets the rock phone number unique identifier for the conversation key.
        /// </summary>
        /// <param name="conversationKey">The conversation key.</param>
        /// <returns>The unique identifier of the Rock phone number that is part of the conversation or <c>null</c> if the conversation key was not valid.</returns>
        internal static Guid? GetRockPhoneNumberGuidForConversationKey( string conversationKey )
        {
            if ( conversationKey == null )
            {
                return null;
            }

            var segments = conversationKey.Split( ':' );

            if ( segments.Length != 3 || segments[0] != "SMS" )
            {
                return null;
            }

            return segments[1].AsGuidOrNull();
        }

        /// <summary>
        /// Gets the person unique identifier for the conversation key. This is
        /// the person being sent a message from Rock, and the person who is
        /// sending messages to Rock.
        /// </summary>
        /// <param name="conversationKey">The conversation key.</param>
        /// <returns>The unique identifier of the Person outside of Rock that is part of the conversation or <c>null</c> if the conversation key was not valid.</returns>
        internal static Guid? GetPersonGuidForConversationKey( string conversationKey )
        {
            if ( conversationKey == null )
            {
                return null;
            }

            var segments = conversationKey.Split( ':' );

            if ( segments.Length != 3 || segments[0] != "SMS" )
            {
                return null;
            }

            return segments[2].AsGuidOrNull();
        }

        /// <summary>
        /// Copies the specified communication identifier.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns></returns>
        public Communication Copy( int communicationId, int? currentPersonAliasId )
        {
            var dataContext = ( RockContext ) Context;

            var service = new CommunicationService( dataContext );
            var communicationRecipientService = new CommunicationRecipientService( dataContext );
            var communication = service.Get( communicationId );
            if ( communication != null )
            {
                var newCommunication = communication.Clone( false );
                newCommunication.CreatedByPersonAlias = null;
                newCommunication.CreatedByPersonAliasId = null;
                newCommunication.CreatedDateTime = RockDateTime.Now;
                newCommunication.ModifiedByPersonAlias = null;
                newCommunication.ModifiedByPersonAliasId = null;
                newCommunication.ModifiedDateTime = RockDateTime.Now;
                newCommunication.Id = 0;
                newCommunication.Guid = Guid.Empty;
                newCommunication.SenderPersonAliasId = currentPersonAliasId;
                newCommunication.Status = CommunicationStatus.Draft;
                newCommunication.ReviewerPersonAliasId = null;
                newCommunication.ReviewedDateTime = null;
                newCommunication.ReviewerNote = string.Empty;
                newCommunication.SendDateTime = null;

                // Get the recipients from the original communication,
                // but only for recipients that are using the person's primary alias id.
                // This will avoid an issue where a copied communication will include the same person multiple times
                // if they have been merged since the original communication was created
                var primaryAliasRecipients = communicationRecipientService.Queryable()
                    .Where( a => a.CommunicationId == communication.Id )
                    .Select( a => new
                    {
                        a.PersonAlias.Person,
                        a.AdditionalMergeValuesJson,
                        a.PersonAliasId
                    } ).ToList()
                    .GroupBy( a => a.Person.PrimaryAliasId )
                    .Select( s => new
                    {
                        PersonAliasId = s.Key,
                        AdditionalMergeValuesJson = s.Where( a => a.PersonAliasId == s.Key ).Select( x => x.AdditionalMergeValuesJson ).FirstOrDefault()
                    } )
                    .Where( s => s.PersonAliasId.HasValue )
                    .ToList();

                foreach ( var primaryAliasRecipient in primaryAliasRecipients )
                {
                    newCommunication.Recipients.Add( new CommunicationRecipient()
                    {
                        PersonAliasId = primaryAliasRecipient.PersonAliasId.Value,
                        Status = CommunicationRecipientStatus.Pending,
                        StatusNote = string.Empty,
                        AdditionalMergeValuesJson = primaryAliasRecipient.AdditionalMergeValuesJson
                    } );
                }

                foreach ( var attachment in communication.Attachments.ToList() )
                {
                    var newAttachment = new CommunicationAttachment();
                    newAttachment.BinaryFileId = attachment.BinaryFileId;
                    newAttachment.CommunicationType = attachment.CommunicationType;
                    newCommunication.Attachments.Add( newAttachment );
                }

                return newCommunication;
            }

            return null;
        }
    }
}