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
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using FCM.Net;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending push notifications using Firebase
    /// </summary>
    [Description( "Sends a communication through Firebase API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Firebase" )]
    [TextField( "SenderId", "Your Firebase account sender Id (find at https://console.firebase.google.com/project/<YOUR_APP>/settings/cloudmessaging)", true, "", "", 0 )]
    [TextField( "ServerKey", "The server key for your firebase account", true, "", "", 1 )]
    class Firebase : TransportComponent
    {

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override async void Send( Rock.Model.Communication communication )
        {
            var rockContext = new RockContext();

            // Requery the Communication
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.HasPendingRecipients(rockContext) &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                string deviceRegistrationId = string.Empty;

                string senderId = GetAttributeValue( "SenderId" );
                string serverKey = GetAttributeValue( "ServerKey" );
                var sender = new Sender(serverKey);

                var historyService = new HistoryService( rockContext );
                var recipientService = new CommunicationRecipientService( rockContext );

                var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                var communicationEntityTypeId = EntityTypeCache.Read( "Rock.Model.Communication" ).Id;
                var communicationCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid(), rockContext ).Id;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                // get message template
                string message = communication.GetMediumDataValue( "Message" );

                // get message title
                string title = communication.GetMediumDataValue("Title");

                // get sound preference
                string sound = communication.GetMediumDataValue("Sound");

                // convert any special microsoft word characters to normal chars so they don't look funny (for example "Hey â€œdouble-quotesâ€ from â€˜single quoteâ€™")
                message = message.ReplaceWordChars();

                // determine callback address
                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                string callbackUrl = globalAttributes.GetValue( "PublicApplicationRoot" ) + "Webhooks/Twilio.ashx";


                bool recipientFound = true;
                while ( recipientFound )
                {
                    rockContext = new RockContext();
                    var recipient = Rock.Model.Communication.GetNextPending( communication.Id, rockContext );
                    if ( recipient != null )
                    {
                        try
                        {
                            var service = new PersonalDeviceService( rockContext );
                            int personAlias = recipient.PersonAliasId;

                            List<string> devices = service.Queryable()
                                .Where(p => p.PersonAliasId == personAlias && p.NotificationsEnabled)
                                .Select(p => p.DeviceRegistrationId)
                                .ToList();

                            if ( devices != null )
                            {
                                // Create merge field dictionary
                                var mergeObjects = recipient.CommunicationMergeValues( mergeFields );
                                
                                var resolvedMessage = message.ResolveMergeFields( mergeObjects, communication.EnabledLavaCommands );
                                var notification = new Message
                                {
                                    RegistrationIds = devices,
                                    Notification = new FCM.Net.Notification
                                    {
                                        Title = title,
                                        Body = resolvedMessage,
                                        Sound = sound,
                                    }
                                };
                                
                                var response = await sender.SendAsync(notification);

                                var status = (response.MessageResponse.Failure > 0) ? CommunicationRecipientStatus.Failed : CommunicationRecipientStatus.Delivered;

                                recipient.Status = status;
                                recipient.TransportEntityTypeName = this.GetType().FullName;
                                recipient.UniqueMessageId = response.MessageResponse.MulticastId;

                                try
                                {
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
                                catch (Exception ex)
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

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        recipientFound = false;
                    }
                }
            }
        }

        public override void Send(Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot)
        {
            throw new NotImplementedException();
        }

        public override void Send(SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot)
        {
            throw new NotImplementedException();
        }

        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null)
        {
            throw new NotImplementedException();
        }

        public override void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }

        public override void Send(List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null)
        {
            throw new NotImplementedException();
        }
    }
}
