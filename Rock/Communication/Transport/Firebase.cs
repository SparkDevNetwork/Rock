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
using FCM.Net;
using System.Threading.Tasks;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending push notifications using Firebase
    /// </summary>
    [Description( "Sends a communication through Firebase API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Firebase" )]
    [TextField( "ServerKey", "The server key for your firebase account", true, "", "", 1 )]
    class Firebase : TransportComponent
    {

        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Send( RockMessage rockMessage, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var emailMessage = rockMessage as RockPushMessage;
            if ( emailMessage != null )
            {
                string serverKey = GetAttributeValue( "ServerKey" );
                var sender = new Sender( serverKey );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );

                var recipients = rockMessage.GetRecipientData();

                if ( emailMessage.SendSeperatelyToEachRecipient )
                {
                    foreach ( var recipient in recipients )
                    {
                        try
                        {

                            foreach ( var mergeField in mergeFields )
                            {
                                recipient.MergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                            }

                            PushMessage( sender, new List<string> { recipient.To }, emailMessage, recipient.MergeFields );
                        }
                        catch ( Exception ex )
                        {
                            errorMessages.Add( ex.Message );
                            ExceptionLogService.LogException( ex, null );
                        }
                    }
                }
                else
                {
                    try
                    {
                        PushMessage( sender, recipients.Select( r => r.To ).ToList(), emailMessage, mergeFields );
                    }
                    catch ( Exception ex )
                    {
                        errorMessages.Add( ex.Message );
                        ExceptionLogService.LogException( ex, null );
                    }
                }
            }

            return !errorMessages.Any();

        }

        private void PushMessage( Sender sender, List<string> to, RockPushMessage emailMessage, Dictionary<string, object> mergeFields )
        {
            string title = emailMessage.Title.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            title = title.ReplaceWordChars();

            string sound = emailMessage.Sound.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            sound = sound.ReplaceWordChars();

            string message = emailMessage.Message.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            message = message.ReplaceWordChars();

            if ( emailMessage.ThemeRoot.IsNotNullOrWhitespace() )
            {
                message = message.Replace( "~~/", emailMessage.ThemeRoot );
            }

            if ( emailMessage.AppRoot.IsNotNullOrWhitespace() )
            {
                message = message.Replace( "~/", emailMessage.AppRoot );
                message = message.Replace( @" src=""/", @" src=""" + emailMessage.AppRoot );
                message = message.Replace( @" src='/", @" src='" + emailMessage.AppRoot );
                message = message.Replace( @" href=""/", @" href=""" + emailMessage.AppRoot );
                message = message.Replace( @" href='/", @" href='" + emailMessage.AppRoot );
            }

            //TODO: Originally this would do one send                  
            var notification = new Message
            {
                RegistrationIds = to,
                Notification = new FCM.Net.Notification
                {
                    Title = title,
                    Body = message,
                    Sound = sound,
                }
             };
    
            Utility.AsyncHelpers.RunSync(() => sender.SendAsync(notification ) );
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication )
        {
            int? mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Id;

            using ( var communicationRockContext = new RockContext() )
            {
                // Requery the Communication
                communication = new CommunicationService( communicationRockContext )
                    .Queryable( "CreatedByPersonAlias.Person" )
                    .FirstOrDefault( c => c.Id == communication.Id );

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
                            r.MediumEntityTypeId.Value == mediumEntityId )
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
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                    string serverKey = GetAttributeValue( "ServerKey" );
                    var sender = new Sender( serverKey );

                    // get message template
                    string message = communication.PushMessage;

                    // get message title
                    string title = communication.PushTitle;

                    // get sound preference
                    string sound = communication.PushSound;

                    var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                    var communicationEntityTypeId = EntityTypeCache.Read( "Rock.Model.Communication" ).Id;
                    var communicationCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), communicationRockContext ).Id;

                    bool recipientFound = true;
                    while ( recipientFound )
                    {
                        var recipientRockContext = new RockContext();
                        var recipient = Rock.Model.Communication.GetNextPending( communication.Id, recipientRockContext );

                        if ( recipient != null )
                        {
                            try
                            {
                                int personAlias = recipient.PersonAliasId;

                                var service = new PersonalDeviceService( recipientRockContext );
                                List<string> devices = service.Queryable()
                                    .Where( p => p.PersonAliasId == personAlias && p.NotificationsEnabled )
                                    .Select( p => p.DeviceRegistrationId )
                                    .ToList();

                                if ( devices != null )
                                {
                                    // Create merge field dictionary
                                    var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                                    var resolvedMessage = message.ResolveMergeFields( mergeObjects, communication.EnabledLavaCommands );
                                    resolvedMessage = resolvedMessage.ReplaceWordChars();

                                    var notification = new Message
                                    {
                                        RegistrationIds = devices.Distinct().ToList(),
                                        Notification = new FCM.Net.Notification
                                        {
                                            Title = title,
                                            Body = resolvedMessage,
                                            Sound = sound,
                                        }
                                    };

                                    ResponseContent response = Utility.AsyncHelpers.RunSync( () => sender.SendAsync( notification ) );

                                    bool failed = response.MessageResponse.Failure == devices.Count;
                                    var status = failed ? CommunicationRecipientStatus.Failed : CommunicationRecipientStatus.Delivered;

                                    if ( failed )
                                    {
                                        recipient.StatusNote = "Firebase failed to notify devices";
                                    }

                                    recipient.Status = status;
                                    recipient.TransportEntityTypeName = this.GetType().FullName;
                                    recipient.UniqueMessageId = response.MessageResponse.MulticastId;

                                    try
                                    {
                                        var historyService = new HistoryService( recipientRockContext );
                                        historyService.Add( new History
                                        {
                                            CreatedByPersonAliasId = communication.SenderPersonAliasId,
                                            EntityTypeId = personEntityTypeId,
                                            CategoryId = communicationCategoryId,
                                            EntityId = recipient.PersonAlias.PersonId,
                                            Summary = "Sent push notification.",
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
                                    recipient.StatusNote = "No Personal Devices with Messaging Enabled";
                                }
                            }
                            catch ( Exception ex )
                            {
                                recipient.Status = CommunicationRecipientStatus.Failed;
                                recipient.StatusNote = "Firebase Exception: " + ex.Message;
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

        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot)
        {
            var message = new RockPushMessage();
            message.Title = mediumData.GetValueOrNull( "Title" ) ?? string.Empty;
            message.Message = mediumData.GetValueOrNull( "Message" ) ?? string.Empty;
            message.Sound = mediumData.GetValueOrNull( "Sound" ) ?? string.Empty;
            message.Recipients.AddRange( recipients );
            message.SendSeperatelyToEachRecipient = false;
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;

            var errorMessages = new List<string>();
            Send( message, out errorMessages );
        }

        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot)
        {
            throw new NotImplementedException();
        }

        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null)
        {
            throw new NotImplementedException();
        }

        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }

        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public override void Send(List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }

    }
}
