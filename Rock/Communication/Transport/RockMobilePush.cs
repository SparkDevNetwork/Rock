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
using System.Threading.Tasks;

using FCM.Net;

using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Google.Apis.Auth.OAuth2;

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
    [TextField( "Server Key (Legacy)",
        Description = "The server key for your firebase account. This was replaced by the Service Account JSON.",
        IsRequired = false,
        Key = AttributeKeys.ServerKey,
        Order = 0 )]

    [CodeEditorField( "Service Account JSON",
        Description = "Upload the service account JSON from your Google Console to authenticate your Rock server.",
        IsRequired = true,
        Key = AttributeKeys.ServiceAccountJson,
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "AB86881F-4A9C-4138-A0B4-252B4E00C145" )]

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

    public class RockMobilePush : TransportComponent, IRockMobilePush
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
        /// The attribute keys for this component.
        /// </summary>
        private static class AttributeKeys
        {
            /// <summary>
            /// The server key attribute key.
            /// </summary>
            public const string ServerKey = "ServerKey";

            /// <summary>
            /// The service account json attribute key.
            /// </summary>
            public const string ServiceAccountJson = "ServiceAccountJson";
        }

        #endregion

        #region Base Method Overrides

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
            if ( GetAttributeValue( AttributeKeys.ServiceAccountJson ).IsNullOrWhiteSpace() )
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
        /// Sends the push notification.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
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

                            var to = recipient.To.SplitDelimitedValues( "," ).Where( s => s.IsNotNullOrWhiteSpace() ).ToList();

                            PushMessage( to, pushMessage, recipient.MergeFields );
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
                        PushMessage( recipients.SelectMany( r => r.To.SplitDelimitedValues( "," ).Where( s => s.IsNotNullOrWhiteSpace() ).ToList() ).ToList(), pushMessage, mergeFields );
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
        /// Sends the push notification.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        private void SendPushNotification( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
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

                                        var notification = new FirebaseAdmin.Messaging.MulticastMessage
                                        {
                                            Tokens = devices.Distinct().ToList(),
                                            Notification = new FirebaseAdmin.Messaging.Notification
                                            {
                                                Title = title,
                                                Body = message,
                                            },
                                            Data = GetPushNotificationData( communication.PushOpenAction, pushData, recipient )
                                        };

                                        if ( sound.IsNotNullOrWhiteSpace() )
                                        {
                                            notification.Android = new AndroidConfig
                                            {
                                                Notification = new AndroidNotification
                                                {
                                                    Sound = sound
                                                }
                                            };

                                            notification.Apns = new ApnsConfig
                                            {
                                                Aps = new Aps
                                                {
                                                    Sound = sound
                                                }
                                            };
                                        }

                                        var firebaseApp = GetFirebaseApp();
                                        var firebaseMessenger = FirebaseMessaging.GetMessaging( firebaseApp.App );

                                        var response = Utility.AsyncHelpers.RunSync( () => ( firebaseMessenger.SendEachForMulticastAsync( notification ) ) );

                                        if ( response.FailureCount > 0 )
                                        {
                                            DeactivateNotRegisteredDevices( notification.Tokens.ToList(), response );
                                        }

                                        bool failed = response.FailureCount == devices.Count || response.SuccessCount == 0;
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

            var data = GetPushNotificationData( emailMessage.OpenAction, emailMessage.Data, null );
            var notification = new FirebaseAdmin.Messaging.Notification();

            if ( title.IsNotNullOrWhiteSpace() || message.IsNotNullOrWhiteSpace() )
            {
                notification.Title = title;
                notification.Body = message;
            }
            else
            {
                data.AddOrReplace( "silent", "true" );
            }

            // Android config
            var androidConfig = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ClickAction = "Rock.Mobile.Main",
                    Sound = sound ?? string.Empty,
                }
            };

            // iOS config
            var apnsConfig = new ApnsConfig
            {
                Aps = new Aps
                {
                    Badge = emailMessage.Data?.ApplicationBadgeCount
                }
            };

            var msg = new FirebaseAdmin.Messaging.MulticastMessage
            {
                Tokens = to,
                Notification = notification,
                Data = data,
                Android = androidConfig,
                Apns = apnsConfig
            };

            var firebaseApp = GetFirebaseApp();
            var firebaseMessenger = FirebaseMessaging.GetMessaging( firebaseApp.App );

            AsyncHelpers.RunSync( async () =>
            {
                var response = await firebaseMessenger.SendEachForMulticastAsync( msg );

                if ( response.FailureCount > 0 )
                {
                    DeactivateNotRegisteredDevices( msg.Tokens.ToList(), response );
                }
            } );
        }

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

                var instanceName = $"RockMobilePush-{Guid.NewGuid()}";

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

            if ( pushData.CustomData != null )
            {
                foreach ( var kvp in pushData.CustomData )
                {
                    notificationData.TryAdd( kvp.Key, kvp.Value );
                }
            }

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

        /// <summary>
        /// Scans the message response data for any errors that indicate the device
        /// is no longer registered to receive push notifications. This happens when
        /// the application has been uninstalled.
        /// </summary>
        /// <param name="recipients">The ordered list of recipient device registration identifiers that were sent the message.</param>
        /// <param name="response">The response object that contains any errors.</param>
        private static void DeactivateNotRegisteredDevices( List<string> recipients, BatchResponse response )
        {
            var notRegisteredDeviceIds = new List<string>();

            // Results are in the same order as the recipients. Loop through
            // the results looking for any NotRegistered errors and add the
            // corresponding Firebase device identifier to a list.
            for ( int i = 0; i < response.Responses.Count; i++ )
            {
                var currentResponse = response.Responses[i];
                if ( !currentResponse.IsSuccess && currentResponse.Exception?.ErrorCode == ErrorCode.NotFound )
                {
                    if ( i < recipients.Count )
                    {
                        notRegisteredDeviceIds.Add( recipients[i] );
                    }
                }
            }

            // If we found any devices that are no longer registered then start
            // a background task to find them all and update them in the database
            // to be inactive.
            if ( notRegisteredDeviceIds.Any() )
            {
                Task.Run( () =>
                {
                    var rockContext = new RockContext();
                    var personalDeviceService = new PersonalDeviceService( rockContext );
                    int contextCount = 0;

                    while ( notRegisteredDeviceIds.Any() )
                    {
                        // Work with 25 devices at a time so we don't overload the
                        // sql IN operator.
                        var registrationIds = notRegisteredDeviceIds.Take( 25 ).ToList();
                        notRegisteredDeviceIds = notRegisteredDeviceIds.Skip( 25 ).ToList();

                        var devices = personalDeviceService.Queryable()
                            .Where( d => d.IsActive && registrationIds.Contains( d.DeviceRegistrationId ) )
                            .ToList();

                        if ( devices.Any() )
                        {
                            devices.ForEach( d => d.IsActive = false );
                        }

                        contextCount += devices.Count;

                        // Create a new database context after 250 so that the change
                        // tracker doesn't get too slow.
                        if ( contextCount >= 250 )
                        {
                            rockContext.SaveChanges();
                            rockContext.Dispose();

                            rockContext = new RockContext();
                            personalDeviceService = new PersonalDeviceService( rockContext );
                        }
                    }

                    rockContext.SaveChanges();
                    rockContext.Dispose();
                } );
            }
        }

        #region Legacy Implementations

        //
        // With the Firebase FCM legacy endpoints being deprecated,
        // we had to upgrade from the 'FCM.NET' package and 'Server Key'
        // attribute. These methods still exist for compatibility reasons.
        // https://firebase.google.com/docs/cloud-messaging/migrate-v1
        //

        /// <summary>
        /// Scans the message response data for any errors that indicate the device
        /// is no longer registered to receive push notifications. This happens when
        /// the application has been uninstalled.
        /// </summary>
        /// <param name="recipients">The ordered list of recipient device registration identifiers that were sent the message.</param>
        /// <param name="response">The response object that contains any errors.</param>
        private static void DeactivateNotRegisteredDevicesLegacy( List<string> recipients, MessageResponse response )
        {
            var notRegisteredDeviceIds = new List<string>();

            // Results are in the same order as the recipients. Loop through
            // the results looking for any NotRegistered errors and add the
            // corresponding Firebase device identifier to a list.
            for ( int i = 0; i < response.Results.Count; i++ )
            {
                if ( response.Results[i].Error == "NotRegistered" )
                {
                    if ( i < recipients.Count )
                    {
                        notRegisteredDeviceIds.Add( recipients[i] );
                    }
                }
            }

            // If we found any devices that are no longer registered then start
            // a background task to find them all and update them in the database
            // to be inactive.
            if ( notRegisteredDeviceIds.Any() )
            {
                Task.Run( () =>
                {
                    var rockContext = new RockContext();
                    var personalDeviceService = new PersonalDeviceService( rockContext );
                    int contextCount = 0;

                    while ( notRegisteredDeviceIds.Any() )
                    {
                        // Work with 25 devices at a time so we don't overload the
                        // sql IN operator.
                        var registrationIds = notRegisteredDeviceIds.Take( 25 ).ToList();
                        notRegisteredDeviceIds = notRegisteredDeviceIds.Skip( 25 ).ToList();

                        var devices = personalDeviceService.Queryable()
                            .Where( d => d.IsActive && registrationIds.Contains( d.DeviceRegistrationId ) )
                            .ToList();

                        if ( devices.Any() )
                        {
                            devices.ForEach( d => d.IsActive = false );
                        }

                        contextCount += devices.Count;

                        // Create a new database context after 250 so that the change
                        // tracker doesn't get too slow.
                        if ( contextCount >= 250 )
                        {
                            rockContext.SaveChanges();
                            rockContext.Dispose();

                            rockContext = new RockContext();
                            personalDeviceService = new PersonalDeviceService( rockContext );
                        }
                    }

                    rockContext.SaveChanges();
                    rockContext.Dispose();
                } );
            }
        }

        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private bool SendLegacy( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
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
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void SendLegacy( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
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

                                        var notification = new FCM.Net.Message
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

                                        if ( response.MessageResponse.Failure > 0 )
                                        {
                                            DeactivateNotRegisteredDevicesLegacy( notification.RegistrationIds, response.MessageResponse );
                                        }

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
        /// Pushes the message legacy.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="to">To.</param>
        /// <param name="emailMessage">The email message.</param>
        /// <param name="mergeFields">The merge fields.</param>
        private void PushMessageLegacy( Sender sender, List<string> to, RockPushMessage emailMessage, Dictionary<string, object> mergeFields )
        {
            string title = ResolveText( emailMessage.Title, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string sound = ResolveText( emailMessage.Sound, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            string message = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, mergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

            var data = GetPushNotificationData( emailMessage.OpenAction, emailMessage.Data, null );
            var notification = new FCM.Net.Notification();

            if ( title.IsNotNullOrWhiteSpace() || message.IsNotNullOrWhiteSpace() )
            {
                notification.ClickAction = "Rock.Mobile.Main";
                notification.Title = title;
                notification.Body = message;
                notification.Sound = sound;
            }
            else
            {
                data.AddOrReplace( "silent", "true" );
            }

            if ( emailMessage.Data.ApplicationBadgeCount.HasValue )
            {
                notification.Badge = emailMessage.Data.ApplicationBadgeCount.ToString();
            }

            var msg = new FCM.Net.Message
            {
                RegistrationIds = to,
                Notification = notification,
                Data = data
            };

            AsyncHelpers.RunSync( ( Func<Task> ) ( async () =>
            {
                var response = await sender.SendAsync( msg );

                if ( response.MessageResponse.Failure > 0 )
                {
                    DeactivateNotRegisteredDevicesLegacy( msg.RegistrationIds, response.MessageResponse );
                }
            } ) );
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
