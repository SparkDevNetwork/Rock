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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.NotificationMessageList;
using Rock.ViewModels.Core;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays notification messages for the current individual.
    /// </summary>
    [DisplayName( "Notification Messages" )]
    [Category( "Core" )]
    [Description( "Displays notification messages for the current individual." )]
    [IconCssClass( "fa fa-bell" )]
    [SupportedSiteTypes( SiteType.Web, SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( "5f6bb4e3-94b2-41fa-94d5-af49a97b21cb" )]
    [Rock.SystemGuid.BlockTypeGuid( "2e4292b9-cd68-41e9-86bd-356accd54f36" )]
    public class NotificationMessageList : RockBlockType
    {
        #region Properties

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5, 0, 15 );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return null;
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                return new NotificationMessageListInitializationBox
                {
                    Messages = GetNotificationMessages( rockContext ).Select( m => GetMessageBag( m ) ).ToList(),
                    ComponentTypes = GetComponentTypes()
                };
            }
        }

        /// <summary>
        /// Gets the notification messages for the current person.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>A list of <see cref="NotificationMessage"/> objects.</returns>
        private List<NotificationMessage> GetNotificationMessages( RockContext rockContext )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return new List<NotificationMessage>();
            }

            return new NotificationMessageService( rockContext )
                .GetActiveMessagesForPerson( RequestContext.CurrentPerson.Guid, PageCache.Layout.Site )
                .OrderByDescending( m => m.MessageDateTime )
                .ToList();
        }

        /// <summary>
        /// Gets the known component types. This is used to present a list of
        /// message types to the individual to use when filtering the messages.
        /// </summary>
        /// <returns>A collection of <see cref="ListItemBag"/> objects.</returns>
        private List<ListItemBag> GetComponentTypes()
        {
            return Rock.Core.NotificationMessageTypeContainer.Instance
                .Dictionary
                .Values
                .Select( v => v.Value.EntityType )
                .Where( et => et != null )
                .Select( et => new ListItemBag
                {
                    Value = et.IdKey,
                    Text = et.FriendlyName
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the message bag that will reprsent the given notification message.
        /// </summary>
        /// <param name="message">The message to be sent to the client.</param>
        /// <returns>A new instance of <see cref="NotificationMessageBag"/> that represents the orginal notification message.</returns>
        private NotificationMessageBag GetMessageBag( NotificationMessage message )
        {
            var bag = new NotificationMessageBag
            {
                IdKey = message.IdKey,
                Title = message.Title,
                Description = message.Description,
                DateTime = message.MessageDateTime.ToRockDateTimeOffset(),
                Count = message.Count,
                IsRead = message.IsRead
            };

            var notificationType = NotificationMessageTypeCache.Get( message.NotificationMessageTypeId );
            var component = notificationType?.Component;
            var metadata = component?.GetMetadata( message );

            bag.ComponentIdKey = component.EntityType?.IdKey;
            bag.PhotoUrl = ResolveRockUrlIncludeRoot( metadata?.PhotoUrl );
            bag.IconCssClass = metadata?.IconCssClass;
            bag.Color = metadata?.Color;

            return bag;
        }

        /// <summary>
        /// Resolves the rock URL and includes the original scheme and domain
        /// from the request.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="url">The URL to ben resolved.</param>
        /// <returns>A new string resolved to the proper domain.</returns>
        private string ResolveRockUrlIncludeRoot( string url )
        {
            var virtualPath = RequestContext.ResolveRockUrl( url );

            if ( !virtualPath.StartsWith( "/" ) )
            {
                return virtualPath;
            }

            if ( RequestContext.RootUrlPath.IsNotNullOrWhiteSpace() )
            {
                return $"{RequestContext.RootUrlPath}{virtualPath}";
            }

            return GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) + virtualPath.RemoveLeadingForwardslash();
        }

        /// <summary>
        /// Takes a set of keys and returns a set of integer identifiers and
        /// unique identifiers that represent the keys.
        /// </summary>
        /// <param name="keys">The keys in either Id, Guid or IdKey format.</param>
        /// <param name="allowIntegerIdentifier"><c>true</c> if integer identifiers should be allowed in the keys.</param>
        /// <returns>A set of integer identifiers and a set of unique identifiers.</returns>
        private (List<int> Ids, List<Guid> Guids) GetIdsOrGuidsForKeys( IEnumerable<string> keys, bool allowIntegerIdentifier )
        {
            var ids = new List<int>();
            var guids = new List<Guid>();

            foreach ( var key in keys )
            {
                int? id = allowIntegerIdentifier ? key.AsIntegerOrNull() : null;

                if ( !id.HasValue )
                {
                    var guid = key.AsGuidOrNull();

                    if ( guid.HasValue )
                    {
                        guids.Add( guid.Value );
                        continue;
                    }

                    id = Rock.Utility.IdHasher.Instance.GetId( key );
                }

                if ( id.HasValue )
                {
                    ids.Add( id.Value );
                }
            }

            return (ids, guids);
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial data for the mobile version of the block.
        /// </summary>
        /// <returns>An instance of <see cref="NotificationMessageListInitializationBox"/>.</returns>
        [BlockAction]
        public BlockActionResult GetInitialData()
        {
            return ActionOk( GetObsidianBlockInitialization() );
        }

        /// <summary>
        /// Gets the messages that match the requested criteria.
        /// </summary>
        /// <returns>An instance of <see cref="GetMessagesResponseBag"/> that describes the matching messages.</returns>
        [BlockAction]
        public BlockActionResult GetMessages( GetMessagesRequestBag request )
        {
            using ( var rockContext = new RockContext() )
            {
                return ActionOk( new GetMessagesResponseBag
                {
                    Messages = GetNotificationMessages( rockContext ).Select( m => GetMessageBag( m ) ).ToList()
                } );
            }
        }

        /// <summary>
        /// Marks the message as either read or unread.
        /// </summary>
        /// <param name="request">The request that describes the message to be marked.</param>
        /// <returns>An instance of <see cref="MarkMessageAsReadResponseBag"/> that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult MarkMessageAsRead( MarkMessageAsReadRequestBag request )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden( "Must be logged in to perform this action." );
            }

            using ( var rockContext = new RockContext() )
            {
                var messageService = new NotificationMessageService( rockContext );
                var message = messageService
                    .GetQueryableByKey( request.IdKey, !PageCache.Layout.Site.DisablePredictableIds )
                    .Where( nm => nm.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .FirstOrDefault();

                if ( message == null )
                {
                    return ActionNotFound( "That notification message could not be found." );
                }

                var messageType = NotificationMessageTypeCache.Get( message.NotificationMessageTypeId );

                // Check if we are marking the message as read or unread.
                if ( request.IsRead == true )
                {
                    // Marking it as read, see if we need to actually delete it instead.
                    if ( messageType.IsDeletedOnRead )
                    {
                        messageService.Delete( message );
                    }
                    else
                    {
                        message.IsRead = true;
                    }

                    rockContext.SaveChanges();
                }
                else
                {
                    message.IsRead = false;

                    rockContext.SaveChanges();
                }

                var response = new MarkMessageAsReadResponseBag
                {
                    IsDeleted = request.IsRead && messageType.IsDeletedOnRead
                };

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Marks the messages as either read or unread.
        /// </summary>
        /// <param name="request">The request that describes the messages to be marked.</param>
        /// <returns>An instance of <see cref="MarkMessagesAsReadResponseBag"/> that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult MarkMessagesAsRead( MarkMessagesAsReadRequestBag request )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden( "Must be logged in to perform this action." );
            }

            using ( var rockContext = new RockContext() )
            {
                var (ids, guids) = GetIdsOrGuidsForKeys( request.IdKeys, !PageCache.Layout.Site.DisablePredictableIds );
                var messageService = new NotificationMessageService( rockContext );
                var messages = messageService
                    .Queryable()
                    .Where( m => ids.Contains( m.Id ) || guids.Contains( m.Guid ) )
                    .Where( nm => nm.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .ToList();

                if ( messages.Count == 0 )
                {
                    return ActionNotFound( "That notification messages could not be found." );
                }

                var isDeleted = new Dictionary<string, bool>();

                foreach ( var message in messages )
                {
                    var messageType = NotificationMessageTypeCache.Get( message.NotificationMessageTypeId );

                    // Check if we are marking the message as read or unread.
                    if ( request.IsRead == true )
                    {
                        // Marking it as read, see if we need to actually delete it instead.
                        if ( messageType.IsDeletedOnRead )
                        {
                            messageService.Delete( message );
                        }
                        else
                        {
                            message.IsRead = true;
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        message.IsRead = false;

                        rockContext.SaveChanges();
                    }

                    // Store the flag indicating if this message was deleted by
                    // trying to use the same format the key came in as.
                    if ( !PageCache.Layout.Site.DisablePredictableIds && request.IdKeys.Contains( message.Id.ToString() ) )
                    {
                        isDeleted.Add( message.Id.ToString(), request.IsRead && messageType.IsDeletedOnRead );
                    }
                    else if ( request.IdKeys.Contains( message.Guid.ToString() ) )
                    {
                        isDeleted.Add( message.Guid.ToString(), request.IsRead && messageType.IsDeletedOnRead );
                    }
                    else
                    {
                        isDeleted.Add( message.IdKey, request.IsRead && messageType.IsDeletedOnRead );
                    }
                }

                var response = new MarkMessagesAsReadResponseBag
                {
                    IsDeleted = isDeleted
                };

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Gets the action to be performed for a notification message.
        /// </summary>
        /// <param name="request">The request that describes which message the action is being requested for.</param>
        /// <returns>An instance of <see cref="GetMessagesResponseBag"/> that describes the action to be performed.</returns>
        [BlockAction]
        public BlockActionResult GetMessageAction( GetMessageActionRequestBag request )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden( "Must be logged in to perform this action." );
            }

            using ( var rockContext = new RockContext() )
            {
                var messageService = new NotificationMessageService( rockContext );
                var message = messageService
                    .GetQueryableByKey( request.IdKey, !PageCache.Layout.Site.DisablePredictableIds )
                    .Where( nm => nm.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .FirstOrDefault();

                if ( message == null )
                {
                    return ActionNotFound( "That notification message could not be found." );
                }

                var messageType = NotificationMessageTypeCache.Get( message.NotificationMessageTypeId );
                var action = messageType.Component?.GetActionForNotificationMessage( message, PageCache.Layout.Site, RequestContext );

                // Check if we are marking the message as read.
                if ( request.IsRead == true )
                {
                    if ( messageType.IsDeletedOnRead )
                    {
                        messageService.Delete( message );
                    }
                    else
                    {
                        message.IsRead = true;
                    }

                    rockContext.SaveChanges();
                }

                if ( action == null )
                {
                    return ActionOk( new GetMessageActionResponseBag
                    {
                        Action = new NotificationMessageActionBag
                        {
                            Type = NotificationMessageActionType.Invalid
                        }
                    } );
                }

                var response = new GetMessageActionResponseBag
                {
                    Action = action,
                    IsDeleted = request.IsRead && messageType.IsDeletedOnRead
                };

                return ActionOk( response );
            }
        }

        #endregion
    }
}
