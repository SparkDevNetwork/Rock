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
using Apollos.OneSignal.RestAPIv3.Client;
using Apollos.OneSignal.RestAPIv3.Client.Resources;
using Apollos.OneSignal.RestAPIv3.Client.Resources.Notifications;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending push notifications using OneSignal
    /// </summary>
    [Description( "Sends a communication through OneSignal API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "OneSignal" )]
    [TextField( "AppId", "The App Id for your OneSignal account", true, "", "", 1 )]
    [TextField( "RestAPIKey", "The Rest API key for your OneSignal account", true, "", "", 2)]
    class OneSignal : TransportComponent
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

            var pushMessage = rockMessage as RockPushMessage;
            if ( pushMessage != null )
            {
                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                var recipients = rockMessage.GetRecipientData();

                if ( pushMessage.SendSeperatelyToEachRecipient )
                {
                    foreach ( var recipient in recipients )
                    {
                        try
                        {
                            foreach ( var mergeField in mergeFields )
                            {
                                recipient.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
                            }
                            PushMessage( new List<string> { recipient.To }, pushMessage, recipient.MergeFields );
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
                        PushMessage( recipients.Select( r => r.To ).ToList(), pushMessage, mergeFields );
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

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
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
                        string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                        var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                        var personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                        var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                        var communicationCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), communicationRockContext ).Id;

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
                                        var mergeObjects = recipient.CommunicationMergeValues( mergeFields );
                                        var message = ResolveText( communication.PushMessage, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        var title = ResolveText( communication.PushTitle, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        // var sound = ResolveText( communication.PushSound, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        string appId = GetAttributeValue( "AppId" );
                                        string restApiKey = GetAttributeValue( "RestAPIKey" );
                                        OneSignalClient client = new OneSignalClient( restApiKey );

                                        var options = new NotificationCreateOptions
                                        {
                                            AppId = new Guid( appId ),
                                            IncludeExternalUserIds = new List<string> { recipient.PersonAliasId.ToString() }
                                        };

                                        options.Headings.Add( LanguageCodes.English, title );
                                        options.Contents.Add( LanguageCodes.English, message );
                                        NotificationCreateResult response = client.Notifications.Create( options );

                                    // *****
                                    // Right now OneSignal handles whether or not a notification gets sent.
                                    // We aren't storing whether or not someone is allowed to get push notifications in Rock.
                                    // Also, it doesn't look like the response returned from OneSignal includes any information that
                                    // would tell us whether or not the delivery of the notification was successful. We will need that
                                    // information if we want to set the delivery status correctly below.
                                    // *****

                                    //int personAlias = recipient.PersonAliasId;

                                    //ResponseContent response = Utility.AsyncHelpers.RunSync( () => sender.SendAsync( notification ) );

                                    bool failed = !string.IsNullOrWhiteSpace(response.Error);

                                    //bool failed = response.MessageResponse.Failure == devices.Count;
                                    var status = failed ? CommunicationRecipientStatus.Failed : CommunicationRecipientStatus.Delivered;
                                    //var status = CommunicationRecipientStatus.Delivered;

                                    //if ( failed )
                                    //{
                                    //    recipient.StatusNote = "Firebase failed to notify devices";
                                    //}
                                    //else
                                    //{
                                    //    recipient.SendDateTime = RockDateTime.Now;
                                    //}
                                    recipient.SendDateTime = RockDateTime.Now;

                                    recipient.Status = status;
                                    recipient.TransportEntityTypeName = this.GetType().FullName;
                                    //recipient.UniqueMessageId = response.MessageResponse.MulticastId;
                                    recipient.UniqueMessageId = response.Id;

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
                                            ValueName = "Push Notification",
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
                                catch ( Exception ex )
                                    {
                                        recipient.Status = CommunicationRecipientStatus.Failed;
                                        recipient.StatusNote = "OneSignal Exception: " + ex.Message;
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

        /// <summary>
        /// Pushes the message.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="to">To.</param>
        /// <param name="emailMessage">The email message.</param>
        /// <param name="mergeFields">The merge fields.</param>
        //private void PushMessage( Sender sender, List<string> to, RockPushMessage pushMessage, Dictionary<string, object> mergeFields )
        private void PushMessage( List<string> to, RockPushMessage pushMessage, Dictionary<string, object> mergeFields )
        {
            string title = ResolveText( pushMessage.Title, pushMessage.CurrentPerson, pushMessage.EnabledLavaCommands, mergeFields, pushMessage.AppRoot, pushMessage.ThemeRoot );
            //    string sound = ResolveText( emailMessage.Sound, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string message = ResolveText( pushMessage.Message, pushMessage.CurrentPerson, pushMessage.EnabledLavaCommands, mergeFields, pushMessage.AppRoot, pushMessage.ThemeRoot );
            string appId = GetAttributeValue( "AppId" );
            string restApiKey = GetAttributeValue( "RestAPIKey" );
            OneSignalClient client = new OneSignalClient( restApiKey );

            var options = new NotificationCreateOptions
            {
                AppId = new Guid( appId ),
                IncludeExternalUserIds = to
            };

            options.Headings.Add(LanguageCodes.English, title );
            options.Contents.Add(LanguageCodes.English, message );
            client.Notifications.Create(options );

        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication )
        {
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Id ?? 0;
            Send( communication, mediumEntityId, null );
        }

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        public override void Send( Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot )
        {
            var message = new RockPushMessage();
            message.Title = mediumData.GetValueOrNull( "Title" ) ?? string.Empty;
            message.Message = mediumData.GetValueOrNull( "Message" ) ?? string.Empty;
            message.Sound = mediumData.GetValueOrNull( "Sound" ) ?? string.Empty;
            message.SetRecipients( recipients );
            message.SendSeperatelyToEachRecipient = false;
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot )
        {
            throw new NotImplementedException();
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
        /// <exception cref="NotImplementedException"></exception>
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null )
        {
            throw new NotImplementedException();
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
        /// <exception cref="NotImplementedException"></exception>
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
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
        /// <exception cref="NotImplementedException"></exception>
        public override void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
        {
            throw new NotImplementedException();
        }

    }
}
