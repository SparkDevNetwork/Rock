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

using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core;
using Rock.Web;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

namespace Rock.Core.NotificationMessageTypes
{
    /// <summary>
    /// Displays notification messages for a person that was mentioned in a connection request.
    /// </summary>
    [Description( "Displays notification messages for a person that was mentioned in a connection request." )]
    [Export( typeof( NotificationMessageTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Note Mention" )]

    [Rock.SystemGuid.EntityTypeGuid( "EFDFB2F5-B0F2-46BC-BDE6-474EBA04E386" )]
    internal class ConnectionRequestMention : NotificationMessageTypeComponent
    {
        private static readonly Lazy<int> _componentEntityTypeId = new Lazy<int>( () => EntityTypeCache.GetId<ConnectionRequestMention>().Value );

        #region Static Methods

        /// <summary>
        /// Creates a notification message that represents this mention.
        /// </summary>
        /// <param name="request">The connection request that is the source of the mention.</param>
        /// <param name="mentionedPersonId">The identifier of the <see cref="Person"/> that was mentioned.</param>
        /// <param name="fromPersonId">The identifier of the <see cref="Person"/> that wrote the message.</param>
        /// <param name="pageId">The source page identifier that should be used to link to when interacting with the notification.</param>
        /// <param name="pageParameters">The parameters that should be sent to the page.</param>
        public static void CreateNotificationMessage( ConnectionRequest request, int mentionedPersonId, int fromPersonId, int pageId, IDictionary<string, string> pageParameters )
        {
            // We don't need to create a notification if you mention yourself.
            if ( mentionedPersonId == fromPersonId )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var messageType = GetOrCreateMessageType( rockContext );
                var connectionOpportunity = request.ConnectionOpportunity ?? new ConnectionOpportunityService( rockContext ).GetNoTracking( request.ConnectionOpportunityId );

                if ( messageType == null || connectionOpportunity == null )
                {
                    return;
                }

                var messageService = new NotificationMessageService( rockContext );
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                // Find the person records we need.
                var fromPersonFuture = personService.Queryable()
                    .Where( p => p.Id == fromPersonId )
                    .DeferredFirstOrDefault()
                    .FutureValue();

                var mentionedPersonFuture = personService.Queryable()
                    .Where( p => p.Id == mentionedPersonId )
                    .DeferredFirstOrDefault()
                    .FutureValue();

                var requestPersonFuture = personAliasService.Queryable()
                    .Where( pa => pa.Id == request.PersonAliasId )
                    .Select( pa => pa.Person )
                    .DeferredFirstOrDefault()
                    .FutureValue();

                // Execute the query that provides the future values.
                var fromPerson = fromPersonFuture.Value;
                var mentionedPerson = mentionedPersonFuture.Value;
                var requestPerson = requestPersonFuture.Value;

                // Shouldn't happen, but it's always possible data changed
                // while we were processing.
                if ( fromPerson?.PrimaryAliasId == null || mentionedPerson?.PrimaryAliasId == null )
                {
                    return;
                }

                // Generate the data to store with the notification.
                var componentData = new MessageData
                {
                    ConnectionRequestId = request.Id,
                    PersonAliasId = fromPerson.PrimaryAliasId.Value,
                    SourcePageId = pageId,
                    SourcePageParameters = new Dictionary<string, string>( pageParameters )
                };

                var message = new NotificationMessage
                {
                    Key = $"request-{request.Id}",
                    NotificationMessageTypeId = messageType.Id,
                    PersonAliasId = mentionedPerson.PrimaryAliasId.Value,
                    ComponentDataJson = componentData.ToJson()
                };

                messageService.Add( message );

                message.Title = fromPerson.FullName;
                message.Description = $"You were mentioned in a {connectionOpportunity.Name} connection request for {requestPerson.FullName}.";
                message.MessageDateTime = RockDateTime.Now;
                message.ExpireDateTime = message.MessageDateTime.AddDays( 90 );

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the message type or creates it if it doesn't exist.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>An instance of <see cref="NotificationMessageTypeCache"/> or <c>null</c> if it could not be created.</returns>
        private static NotificationMessageTypeCache GetOrCreateMessageType( RockContext rockContext )
        {
            var key = "mention";

            var messageTypeCache = NotificationMessageTypeCache.All()
                .Where( nmt => nmt.EntityTypeId == _componentEntityTypeId.Value && nmt.Key == key )
                .FirstOrDefault();

            if ( messageTypeCache != null )
            {
                return messageTypeCache;
            }

            var service = new NotificationMessageTypeService( rockContext );

            var messageType = new NotificationMessageType
            {
                EntityTypeId = _componentEntityTypeId.Value,
                Key = key,
                IsMobileApplicationSupported = true,
                IsWebSupported = true
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
            var url = messageData != null ? $"~/GetAvatar.ashx?PersonAliasId={messageData.PersonAliasId}" : "~/GetAvatar.ashx?Style=Icon";

            return new NotificationMessageMetadataBag
            {
                PhotoUrl = url,
                IconCssClass = "fa fa-at",
                Color = "#009CE3"
            };
        }

        /// <inheritdoc/>
        public override NotificationMessageActionBag GetActionForNotificationMessage( NotificationMessage message, SiteCache site, RockRequestContext context )
        {
            var messageData = message.ComponentDataJson.FromJsonOrNull<MessageData>();

            if ( messageData == null )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.Invalid
                };
            }

            var url = site.SiteType == SiteType.Web
                ? BuildWebRoute( site.Id, messageData.SourcePageId, messageData.SourcePageParameters )
                : BuildMobileRoute( site.Id, messageData.SourcePageId, messageData.SourcePageParameters );

            if ( url.IsNullOrWhiteSpace() )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.Invalid
                };
            }

            return new NotificationMessageActionBag
            {
                Type = NotificationMessageActionType.LinkToPage,
                Url = url
            };
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

        /// <summary>
        /// Builds the URL route to use for web requests.
        /// </summary>
        /// <param name="siteId">The site identifier the page should be a member of.</param>
        /// <param name="sourcePageId">The page identifier.</param>
        /// <param name="parameters">The page route parameters.</param>
        /// <returns>A string that represents the URL to redirect to or <c>null</c> if it could not be determined.</returns>
        private static string BuildWebRoute( int siteId, int sourcePageId, Dictionary<string, string> parameters )
        {
            var sourcePage = PageCache.Get( sourcePageId );
            var pageRef = sourcePage?.Layout.SiteId == siteId
                ? PageReference.GetBestMatchForParameters( sourcePageId, parameters )
                : PageReference.GetBestAlternatePageRouteForParameters( sourcePageId, siteId, parameters );

            // Not same site and no alternate page was found.
            if ( pageRef == null )
            {
                return string.Empty;
            }

            var url = pageRef.BuildUrl();

            // If the site they are on isn't for the site of the target page
            // then make it a fully qualified URL. Otherwise route matching
            // might break if the site is configured for exclusive routes.
            // But if it is already the correct site, we don't want to just
            // change the domain in the URL on them if the site handles multiple
            // domain names.
            var targetPage = PageCache.Get( pageRef.PageId );
            if ( targetPage != null && siteId != targetPage.Layout.SiteId )
            {
                url = $"{targetPage.Layout.Site.DefaultDomainUri.ToString().TrimEnd( '/' )}{url}";
            }

            return url;
        }

        /// <summary>
        /// Builds the URL route to use for web requests.
        /// </summary>
        /// <param name="siteId">The site identifier the page should be a member of.</param>
        /// <param name="sourcePageId">The page identifier.</param>
        /// <param name="parameters">The page route parameters.</param>
        /// <returns>A string that represents the URL to redirect to or <c>null</c> if it could not be determined.</returns>
        private static string BuildMobileRoute( int siteId, int sourcePageId, Dictionary<string, string> parameters )
        {
            var sourcePage = PageCache.Get( sourcePageId );
            var pageRef = sourcePage?.Layout.SiteId == siteId
                ? PageReference.GetBestMatchForParameters( sourcePageId, parameters )
                : PageReference.GetBestAlternatePageRouteForParameters( sourcePageId, siteId, parameters );

            // Not same site and no alternate page was found.
            if ( pageRef == null )
            {
                return string.Empty;
            }

            var targetPage = PageCache.Get( pageRef.PageId );

            if ( targetPage == null )
            {
                return string.Empty;
            }

            // If the page is not for this site then it is a web page we will
            // be linking out to.
            if ( targetPage.Layout.SiteId != siteId )
            {
                return $"{targetPage.Layout.Site.DefaultDomainUri.ToString().TrimEnd( '/' )}{pageRef.BuildUrl()}";
            }

            return pageRef.BuildMobileUrl();
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
            /// Gets or sets the identifier of the conneciton request this mention is related to.
            /// </summary>
            /// <value>The identifier of the conneciton request this mention is related to.</value>
            public int ConnectionRequestId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person alias that mentioned
            /// the individual.
            /// </summary>
            /// <value>The person alias identifier.</value>
            public int PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the page identifier that this mention was triggered on.
            /// </summary>
            /// <value>The page identifier that this mention was triggered on.</value>
            public int SourcePageId { get; set; }

            /// <summary>
            /// Gets or sets the source page parameters.
            /// </summary>
            /// <value>The source page parameters.</value>
            public Dictionary<string, string> SourcePageParameters { get; set; }
        }

        #endregion
    }
}
