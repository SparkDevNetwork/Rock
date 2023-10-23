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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

namespace Rock.Core.NotificationMessageTypes
{
    /// <summary>
    /// Displays notification messages about unread SMS conversations.
    /// </summary>
    [Description( "Displays notification messages about unread SMS conversations." )]
    [Export( typeof( NotificationMessageTypeComponent ) )]
    [ExportMetadata( "ComponentName", "SMS Conversation" )]

    [Rock.SystemGuid.EntityTypeGuid( "A6BC9CB4-9AA8-4D9C-90C8-0131A9ED3A3F" )]
    internal class SmsConversation : NotificationMessageTypeComponent
    {
        private static readonly Lazy<int> _componentEntityTypeId = new Lazy<int>( () => EntityTypeCache.GetId<SmsConversation>().Value );

        #region Static Methods

        /// <summary>
        /// Updates the notification messages for the specified conversation.
        /// </summary>
        /// <param name="phoneNumber">The phone number that represents Rock's side of the conversation.</param>
        /// <param name="fromPersonId">The identifier that represents the Person on the other side of the conversation.</param>
        public static void UpdateNotificationMessages( SystemPhoneNumberCache phoneNumber, int fromPersonId )
        {
            if ( !phoneNumber.SmsNotificationGroupId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var messageType = GetOrCreateMessageType( phoneNumber, rockContext );

                if ( messageType == null )
                {
                    return;
                }

                var messageService = new NotificationMessageService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var personService = new PersonService( rockContext );
                var responseService = new CommunicationResponseService( rockContext );
                var key = $"person-{fromPersonId}";

                var toPersons = groupMemberService.Queryable()
                    .Where( gm => gm.GroupId == phoneNumber.SmsNotificationGroupId.Value
                        && gm.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( gm => new
                    {
                        gm.PersonId,
                        PrimaryPersonAliasId = gm.Person.Aliases.Where( a => a.AliasPersonId == gm.PersonId ).Select( a => ( int? ) a.Id ).FirstOrDefault()
                    } )
                    .Where( a => a.PrimaryPersonAliasId.HasValue )
                    .Distinct()
                    .ToList();
                var toPersonIds = toPersons.Select( a => a.PersonId ).ToList();

                // Find any existing notification messages for these people.
                var messagesValue = messageService.Queryable()
                    .Include( nm => nm.PersonAlias )
                    .Where( nm => nm.NotificationMessageTypeId == messageType.Id
                        && nm.Key == key
                        && toPersonIds.Contains( nm.PersonAlias.PersonId ) )
                    .Future();

                // Find the person that sent the message.
                var personValue = personService.Queryable()
                    .Include( p => p.Aliases )
                    .Where( p => p.Id == fromPersonId )
                    .DeferredFirstOrDefault()
                    .FutureValue();

                // Find the number of unread messages from that person.
                var responseCountValue = responseService.Queryable()
                    .Where( cr => cr.RelatedSmsFromSystemPhoneNumberId == phoneNumber.Id
                        && cr.FromPersonAlias.PersonId == fromPersonId
                        && !cr.IsRead )
                    .DeferredCount()
                    .FutureValue();

                // Materialize all 3 deferred queries.
                var person = personValue.Value;
                var messages = messagesValue.ToList();
                var responseCount = responseCountValue.Value;

                foreach ( var toPerson in toPersons )
                {
                    var message = messages
                        .Where( nm => nm.PersonAlias.PersonId == toPerson.PersonId )
                        .OrderByDescending( nm => nm.Id )
                        .FirstOrDefault();

                    // If the count is zero or the Person no longer exists, then
                    // we can delete the message if it exists.
                    if ( responseCount == 0 || person == null || !person.PrimaryAliasId.HasValue )
                    {
                        if ( message != null )
                        {
                            messageService.Delete( message );
                            messages.Remove( message );
                        }

                        continue;
                    }

                    // Count is greater than zero so we need to either create
                    // or update the existing message.
                    if ( message == null )
                    {
                        var componentData = new MessageData
                        {
                            PhoneNumberId = phoneNumber.Id,
                            PhoneNumberGuid = phoneNumber.Guid,
                            PersonId = person.Id,
                            PersonGuid = person.Guid
                        };

                        message = new NotificationMessage
                        {
                            NotificationMessageTypeId = messageType.Id,
                            Key = key,
                            PersonAliasId = toPerson.PrimaryPersonAliasId.Value,
                            ComponentDataJson = componentData.ToJson()
                        };

                        messageService.Add( message );
                    }

                    message.Title = person.FullName;
                    message.Description = $"You have {responseCount} unread messages.";
                    message.IsRead = false;
                    message.MessageDateTime = RockDateTime.Now;
                    message.ExpireDateTime = message.MessageDateTime.AddDays( 90 );
                    message.Count = responseCount;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the message type or creates it if it doesn't exist.
        /// </summary>
        /// <param name="phoneNumber">The phone number the message type is related to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>An instance of <see cref="NotificationMessageTypeCache"/> or <c>null</c> if it could not be created.</returns>
        private static NotificationMessageTypeCache GetOrCreateMessageType( SystemPhoneNumberCache phoneNumber, RockContext rockContext )
        {
            var key = $"number-{phoneNumber.Id}";

            var messageTypeCache = NotificationMessageTypeCache.All()
                .Where( nmt => nmt.EntityTypeId == _componentEntityTypeId.Value && nmt.Key == key )
                .FirstOrDefault();

            if ( messageTypeCache != null )
            {
                return messageTypeCache;
            }

            if ( !phoneNumber.MobileApplicationSiteId.HasValue )
            {
                return null;
            }

            var service = new NotificationMessageTypeService( rockContext );

            var messageType = new NotificationMessageType
            {
                EntityTypeId = _componentEntityTypeId.Value,
                Key = key,
                IsDeletedOnRead = true,
                IsMobileApplicationSupported = true,
                RelatedMobileApplicationSiteId = phoneNumber.MobileApplicationSiteId
            };

            service.Add( messageType );

            rockContext.SaveChanges();

            return NotificationMessageTypeCache.Get( messageType.Id );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override NotificationMessageMetadataBag GetMetadata( NotificationMessage message )
        {
            var messageData = message.ComponentDataJson.FromJsonOrNull<MessageData>();
            var url = messageData != null ? $"~/GetAvatar.ashx?PersonId={messageData.PersonId}" : "~/GetAvatar.ashx?Style=Icon";

            return new NotificationMessageMetadataBag
            {
                PhotoUrl = url,
                IconCssClass = "fa fa-comment-o",
                Color = "#16C98D"
            };
        }

        /// <inheritdoc/>
        public override NotificationMessageActionBag GetActionForNotificationMessage( NotificationMessage message, SiteCache site, RockRequestContext context )
        {
            var siteTerm = site.SiteType == SiteType.Web ? "web site" : "application";
            int? conversationPageId = null;
            var messageData = message.ComponentDataJson.FromJsonOrNull<MessageData>();

            if ( site.SiteType == SiteType.Mobile )
            {
                var siteOptions = site.AdditionalSettings.FromJsonOrNull<Rock.Mobile.AdditionalSiteSettings>();

                conversationPageId = siteOptions?.SmsConversationPageId;
            }

            var conversationPage = conversationPageId.HasValue ? PageCache.Get( conversationPageId.Value ) : null;

            if ( conversationPage == null )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.ShowMessage,
                    Message = $"This {siteTerm} has not been configured for SMS conversations."
                };
            }

            if ( messageData == null )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.Invalid
                };
            }

            if ( site.SiteType == SiteType.Web )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.LinkToPage,
                    Url = $"{context.RootUrlPath}/page/{conversationPage.Id}?PhoneNumberId={messageData.PhoneNumberId}&PersonId={messageData.PersonId}"
                };
            }
            else
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.LinkToPage,
                    Url = $"{conversationPage.Guid}?PhoneNumberGuid={messageData.PhoneNumberGuid}&PersonGuid={messageData.PersonGuid}"
                };
            }
        }

        /// <inheritdoc/>
        public override int DeleteObsoleteNotificationMessageTypes( int commandTimeout )
        {
            return 0;
        }

        /// <inheritdoc/>
        public override int DeleteObsoleteNotificationMessages( int commandTimeout )
        {
            return 0;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Provides the data stored on individual notification messages used
        /// by this component.
        /// </summary>
        private class MessageData
        {
            /// <summary>
            /// Gets or sets the Rock system phone number identifier.
            /// </summary>
            /// <value>The Rock system phone number identifier.</value>
            public int PhoneNumberId { get; set; }

            /// <summary>
            /// Gets or sets the Rock system phone number unique identifier.
            /// </summary>
            /// <value>The Rock system phone number unique identifier.</value>
            public Guid PhoneNumberGuid { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person that sent the messages.
            /// </summary>
            /// <value>The person identifier.</value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of the person that sent the messages.
            /// </summary>
            /// <value>The person unique identifier.</value>
            public Guid PersonGuid { get; set; }
        }

        #endregion
    }
}
