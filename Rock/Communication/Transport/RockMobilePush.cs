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

using FCM.Net;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending push notifications using Firebase
    /// </summary>
    [Description( "Rock Mobile Push Transport that uses Firebase as the backend, but enables advanced push capabilities in Rock." )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Rock Mobile Push" )]
    [TextField( "ServerKey", "The server key for your firebase account", true, "", "", 1 )]
    class RockMobilePush : TransportComponent, IRockMobilePush
    {
        /// <summary>
        /// The keys that can be present in the notification payload data.
        /// </summary>
        private static class PushKeys
        {
            /// <summary>
            /// The communication identifier that this push notification is for.
            /// </summary>
            public const string CommunicationId = "Rock-CommunicationId";

            /// <summary>
            /// The page unique identifier.
            /// </summary>
            public const string PageGuid = "Rock-PageGuid";

            /// <summary>
            /// The query string parameters.
            /// </summary>
            public const string QueryString = "Rock-QueryString";

            /// <summary>
            /// The recipient identifier that this push notification is for.
            /// </summary>
            public const string RecipientId = "Rock-RecipientId";
        }

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
            var pushData = communication.PushData.FromJsonOrNull<PushData>();

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
                                    var siteId = pushData?.MobileApplicationId;
                                    List<string> devices = null;

                                    if ( recipient.PersonAliasId.HasValue )
                                    {
                                        int personAliasId = recipient.PersonAliasId.Value;
                                        var service = new PersonalDeviceService( recipientRockContext );

                                        devices = service.Queryable()
                                            .Where( p => p.PersonAliasId.HasValue && p.PersonAliasId.Value == personAliasId && p.IsActive && p.NotificationsEnabled && !string.IsNullOrEmpty( p.DeviceRegistrationId ) )
                                            .Where( p => !siteId.HasValue || siteId.Value == p.SiteId )
                                            .Select( p => p.DeviceRegistrationId )
                                            .ToList();
                                    }
                                    else if ( !string.IsNullOrEmpty( recipient.PersonalDevice?.DeviceRegistrationId ) )
                                    {
                                        devices = new List<string> { recipient.PersonalDevice?.DeviceRegistrationId };
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
                                            },
                                            Data = GetPushNotificationData( communication.PushOpenAction, pushData, recipient )
                                        };

                                        ResponseContent response = Utility.AsyncHelpers.RunSync( () => sender.SendAsync( notification ) );

                                        bool failed = response.MessageResponse.Failure == devices.Count || response.MessageResponse.Success == 0;
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
                },
                Data = GetPushNotificationData( emailMessage.OpenAction, emailMessage.Data, null )
            };

            AsyncHelpers.RunSync( () => sender.SendAsync( notification ) );
        }

        /// <summary>
        /// URL encodes the dictionary as a query string.
        /// </summary>
        /// <param name="dictionary">The dictionary to be encoded.</param>
        /// <returns>A string that represents the dictionary as a query string.</returns>
        private static string UrlEncode( Dictionary<string, string> dictionary )
        {
            if ( dictionary == null )
            {
                return string.Empty;
            }

            return string.Join( "&", dictionary.Select( a => $"{a.Key.UrlEncode()}={a.Value.UrlEncode()}" ) );
        }

        /// <summary>
        /// Gets the push notification data to be sent to FCM.
        /// </summary>
        /// <param name="openAction">The open action.</param>
        /// <param name="pushData">The push data.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns>The data to be included in the <see cref="FCM.Net.Message.Data"/> property.</returns>
        private static Dictionary<string, string> GetPushNotificationData( PushOpenAction? openAction, PushData pushData, CommunicationRecipient recipient )
        {
            var notificationData = new Dictionary<string, string>();

            if ( recipient != null )
            {
                notificationData.Add( PushKeys.CommunicationId, recipient.CommunicationId.ToString() );
                notificationData.Add( PushKeys.RecipientId, recipient.Id.ToString() );
            };

            if ( !openAction.HasValue || pushData == null )
            {
                return notificationData;
            }

            if ( openAction == PushOpenAction.ShowDetails && pushData.MobileApplicationId.HasValue && recipient != null )
            {
                var site = SiteCache.Get( pushData.MobileApplicationId.Value );
                var additionalSettings = site?.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                if ( additionalSettings?.CommunicationViewPageId != null )
                {
                    var page = PageCache.Get( additionalSettings.CommunicationViewPageId.Value );

                    if ( page != null )
                    {
                        notificationData.Add( PushKeys.PageGuid, page.Guid.ToString() );
                        notificationData.Add( PushKeys.QueryString, $"CommunicationRecipientGuid={recipient.Guid}" );
                    }
                }
            }
            else if ( openAction == PushOpenAction.LinkToMobilePage && pushData.MobilePageId.HasValue )
            {
                var page = PageCache.Get( pushData.MobilePageId.Value );

                if ( page != null )
                {
                    notificationData.Add( PushKeys.PageGuid, page.Guid.ToString() );
                    notificationData.Add( PushKeys.QueryString, UrlEncode( pushData.MobilePageQueryString ) );
                }
            }

            return notificationData;
        }
    }
}
