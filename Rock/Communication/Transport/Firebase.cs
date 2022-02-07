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

using FCM.Net;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
                // Get server key
                string serverKey = GetAttributeValue( "ServerKey" );
                var sender = new Sender( serverKey );

                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                var recipients = rockMessage.GetRecipients();

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

                            PushMessage( sender, recipient.To.SplitDelimitedValues( "," ).ToList(), pushMessage, recipient.MergeFields );
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
                        PushMessage( sender, recipients.SelectMany( r => r.To.SplitDelimitedValues( "," ).ToList() ).ToList(), pushMessage, mergeFields );
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
                    string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

                    string serverKey = GetAttributeValue( "ServerKey" );
                    var sender = new Sender( serverKey );

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
                                    List<string> devices = null;

                                    if ( recipient.PersonAliasId.HasValue )
                                    {
                                        int personAlias = recipient.PersonAliasId.Value;
                                        var service = new PersonalDeviceService( recipientRockContext );

                                        devices = service.Queryable()
                                            .Where( p => p.PersonAliasId.HasValue && p.PersonAliasId.Value == personAlias && p.IsActive && p.NotificationsEnabled && !string.IsNullOrEmpty( p.DeviceRegistrationId ) )
                                            .Select( p => p.DeviceRegistrationId )
                                            .ToList();
                                    }

                                    if ( devices != null && devices.Any() )
                                    {
                                        // Create merge field dictionary
                                        var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                                        var message = ResolveText( communication.PushMessage, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        var title = ResolveText( communication.PushTitle, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        var sound = ResolveText( communication.PushSound, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                                        var notification = new Message
                                        {
                                            RegistrationIds = devices.Distinct().ToList(),
                                            Notification = new FCM.Net.Notification
                                            {
                                                Title = title,
                                                Body = message,
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
                                        else
                                        {
                                            recipient.SendDateTime = RockDateTime.Now;
                                        }

                                        recipient.Status = status;
                                        recipient.TransportEntityTypeName = this.GetType().FullName;
                                        recipient.UniqueMessageId = response.MessageResponse.MulticastId;

                                        if ( recipient.PersonAlias != null )
                                        {
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
        private void PushMessage( Sender sender, List<string> to, RockPushMessage emailMessage, Dictionary<string, object> mergeFields )
        {
            string title = ResolveText( emailMessage.Title, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string sound = ResolveText( emailMessage.Sound, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string message = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

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

            Utility.AsyncHelpers.RunSync( () => sender.SendAsync( notification ) );
        }
    }
}
