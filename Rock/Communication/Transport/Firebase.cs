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

using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Google.Apis.Auth.OAuth2;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending push notifications using Firebase.
    /// </summary>
    [Description( "Sends a communication through Firebase API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Firebase" )]
    [TextField( "Server Key (Legacy)",
        Description = "The server key for your firebase account. This has been replaced by the Service Account JSON.",
        IsRequired = false,
        Key = AttributeKeys.ServerKey,
        Order = 0 )]

    [CodeEditorField( "Service Account JSON",
        Description = "Upload the service account JSON from your Google Console to authenticate your Rock server.",
        IsRequired = true,
        Key = AttributeKeys.ServiceAccountJson,
        Order = 1 )]

    /*
     * BC - 8/7/23 
     * This class can be drastically simplified in June of 2024.
     * The package we were previously using (FCM.NET) will no longer
     * work due to the deprecation of the APIs it was targeting.
     * 
     * https://firebase.google.com/docs/cloud-messaging/migrate-v1
     * https://onesignal.com/blog/what-you-should-know-about-the-fcm-depreciation-announcement/
     * 
     * Once these APIs are removed (in June of 2024),
     * the FCM.NET package will no longer work and the legacy
     * methods can be removed.
     * 
     */

    [Rock.SystemGuid.EntityTypeGuid( "3D051BA9-1A85-433C-B4B9-9A430348BCBB" )]
    public class Firebase : TransportComponent
    {
        #region Fields

        /// <summary>
        /// Gets the current firebase application for this
        /// component.
        /// </summary>
        private FirebaseAppWrapper _currentApp;


        /// <summary>
        /// The current application lock.
        /// </summary>
        private static object _currentAppLock = new object();

        #endregion

        #region Keys

        /// <summary>
        /// The attribute keys for this component.
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The server key attribute key.
            /// </summary>
            public const string ServerKey = "ServerKey";

            /// <summary>
            /// The service account json.
            /// </summary>
            public const string ServiceAccountJson = "ServiceAccountJson";
        }

        #endregion

        #region Base Overrides

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
            if( GetAttributeValue( AttributeKeys.ServiceAccountJson ).IsNullOrWhiteSpace() )
            {
                return SendLegacy( rockMessage, mediumEntityTypeId, mediumAttributes, out errorMessages );
            }

            return SendPushNotification( rockMessage, mediumEntityTypeId, mediumAttributes, out errorMessages );
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
            if ( GetAttributeValue( AttributeKeys.ServiceAccountJson ).IsNullOrWhiteSpace() )
            {
                SendLegacy( communication, mediumEntityTypeId, mediumAttributes );
                return;
            }

            SendPushNotification( communication, mediumEntityTypeId, mediumAttributes );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the firebase application.
        /// </summary>
        /// <returns>FirebaseApp.</returns>
        private FirebaseAppWrapper GetFirebaseApp()
        {
            // Get our service account json.
            var json = GetAttributeValue( AttributeKeys.ServiceAccountJson );

            if ( json.IsNullOrWhiteSpace() )
            {
                return null;
            }

            lock ( _currentAppLock )
            {
                var jsonHash = json.XxHash();

                // If there is an existing app,
                // return it if nothing has changed.
                // If something has changed delete it.
                if ( _currentApp != null )
                {
                    if ( jsonHash == _currentApp.ServiceAccountHash )
                    {
                        return _currentApp;
                    }
                    else
                    {
                        _currentApp.App.Delete();
                        _currentApp = null;
                    }
                }

                var instanceName = $"RockFirebase-{Guid.NewGuid()}";

                FirebaseApp.Create( new AppOptions
                {
                    Credential = GoogleCredential.FromJson( json )
                }, instanceName );

                _currentApp = new FirebaseAppWrapper
                {
                    InstanceName = instanceName,
                    ServiceAccountHash = jsonHash
                };

                return _currentApp;
            }
        }

        /// <summary>
        /// Sends the push notification.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        private void SendPushNotification( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
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

                                        var notification = new FirebaseAdmin.Messaging.MulticastMessage
                                        {
                                            Tokens = devices.Distinct().ToList(),
                                            Notification = new FirebaseAdmin.Messaging.Notification
                                            {
                                                Title = title,
                                                Body = message,
                                            }
                                        };

                                        // Add the sound to the push if we have one.
                                        if( sound.IsNotNullOrWhiteSpace() )
                                        {
                                            notification.Android = new AndroidConfig
                                            {
                                                Notification =
                                                {
                                                    Sound = sound
                                                }
                                            };

                                            notification.Apns = new ApnsConfig
                                            {
                                                Aps =
                                                {
                                                    Sound = sound
                                                }
                                            };
                                        }

                                        var firebaseApp = GetFirebaseApp();

                                        var firebaseMessenger = FirebaseMessaging.GetMessaging( firebaseApp.App );

                                        BatchResponse response = Utility.AsyncHelpers.RunSync( () => firebaseMessenger.SendEachForMulticastAsync( notification ) );

                                        bool failed = response.FailureCount == devices.Count;
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
                                        recipient.UniqueMessageId = response.Responses?.FirstOrDefault()?.MessageId;

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
        /// Sends the push notification.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool SendPushNotification( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
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

                var recipients = rockMessage.GetRecipients();

                if ( pushMessage.SendSeperatelyToEachRecipient )
                {
                    foreach ( var recipient in recipients )
                    {
                        try
                        {
                            foreach ( var mergeField in mergeFields )
                            {
                                recipient.MergeFields.TryAdd( mergeField.Key, mergeField.Value );
                            }

                            PushMessage( recipient.To.SplitDelimitedValues( "," ).ToList(), pushMessage, recipient.MergeFields );
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
                        PushMessage( recipients.SelectMany( r => r.To.SplitDelimitedValues( "," ).ToList() ).ToList(), pushMessage, mergeFields );
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
        /// Pushes the message.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="emailMessage">The email message.</param>
        /// <param name="mergeFields">The merge fields.</param>
        private void PushMessage( List<string> to, RockPushMessage emailMessage, Dictionary<string, object> mergeFields )
        {
            string title = ResolveText( emailMessage.Title, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string sound = ResolveText( emailMessage.Sound, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string message = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

            var firebaseApp = GetFirebaseApp();

            var notification = new FirebaseAdmin.Messaging.MulticastMessage
            {
                Tokens = to,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message,
                }
            };

            if ( sound.IsNotNullOrWhiteSpace() )
            {
                notification.Android = new AndroidConfig
                {
                    Notification =
                    {
                        Sound = sound
                    }
                };

                notification.Apns = new ApnsConfig
                {
                    Aps =
                    {
                        Sound = sound
                    }
                };
            }
            var firebaseMessenger = FirebaseMessaging.GetMessaging( firebaseApp.App );

            Utility.AsyncHelpers.RunSync( () => firebaseMessenger.SendEachForMulticastAsync( notification ) );
        }

        #region Legacy Methods

        /// <summary>
        /// Sends a push notification using the legacy FCM.NET package.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <remarks>This shouldn't be used, you should use <see cref="SendPushNotification(RockMessage, int, Dictionary{string, string}, out List{string})" /> instead.</remarks>
        private bool SendLegacy( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var pushMessage = rockMessage as RockPushMessage;

            if ( pushMessage != null )
            {
                // Get server key
                string serverKey = GetAttributeValue( AttributeKeys.ServerKey );
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
                                recipient.MergeFields.TryAdd( mergeField.Key, mergeField.Value );
                            }

                            PushMessageLegacy( sender, recipient.To.SplitDelimitedValues( "," ).ToList(), pushMessage, recipient.MergeFields );
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
                        PushMessageLegacy( sender, recipients.SelectMany( r => r.To.SplitDelimitedValues( "," ).ToList() ).ToList(), pushMessage, mergeFields );
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
        /// Sends a push notification using the legacy FCM.NET package.Sends the legacy.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <remarks>This shouldn't be used, you should use <see cref="SendPushNotification(Rock.Model.Communication, int, Dictionary{string, string})" /> instead.</remarks>
        private void SendLegacy( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
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

                    string serverKey = GetAttributeValue( AttributeKeys.ServerKey );
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

                                        var notification = new FCM.Net.Message
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
        /// Pushes the message using the legacy FCM.NET package.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="to">To.</param>
        /// <param name="emailMessage">The email message.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <remarks>This shouldn't be used, you should use <see cref="SendPushNotification(RockMessage, int, Dictionary{string, string}, out List{string})" /> instead.</remarks>
        private void PushMessageLegacy( Sender sender, List<string> to, RockPushMessage emailMessage, Dictionary<string, object> mergeFields )
        {
            string title = ResolveText( emailMessage.Title, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string sound = ResolveText( emailMessage.Sound, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string message = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

            var notification = new FCM.Net.Message
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

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// A wrapper of the <see cref="App" />.
        /// </summary>
        private class FirebaseAppWrapper
        {
            /// <summary>
            /// Gets or sets the service account hash.
            /// </summary>
            /// <value>The service account hash.</value>
            public string ServiceAccountHash { get; set; }

            /// <summary>
            /// Gets or sets the name of the instance.
            /// </summary>
            /// <value>The name of the instance.</value>
            public string InstanceName { get; set; }

            /// <summary>
            /// Gets the application.
            /// </summary>
            /// <value>The application.</value>
            public FirebaseApp App => FirebaseApp.GetInstance( InstanceName );
        }

        #endregion
    }
}
