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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Mobile.Communication.SmsConversation;
using Rock.ViewModels.Communication;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Communication
{
    /// <summary>
    /// Displays a single SMS conversation between Rock and individual.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "SMS Conversation" )]
    [Category( "Mobile > Communication" )]
    [Description( "Displays a single SMS conversation between Rock and individual." )]
    [IconCssClass( "fa fa-comments" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CustomDropdownListField( "Snippet Type",
        Description = "The type of snippets to make available when sending a message.",
        IsRequired = false,
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [SnippetType] ORDER BY [Name]",
        Key = AttributeKey.SnippetType,
        Order = 0 )]

    [IntegerField( "Message Count",
        Description = "The number of messages to be returned each time more messages are requested.",
        IsRequired = true,
        DefaultIntegerValue = 50,
        Key = AttributeKey.MessageCount,
        Order = 2 )]

    [IntegerField( "Database Timeout",
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "99812f83-b514-4a76-a79d-01a97369f726" )]
    [Rock.SystemGuid.BlockTypeGuid( "4ef4250e-2d22-426c-adac-571c1301d18e" )]
    public class SmsConversation : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="SmsConversationList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
            public const string MessageCount = "MessageCount";
            public const string SnippetType = "SnippetType";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the list item bags that represent the snippets which can be
        /// used by the individual.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>A collection of bags that represent the snippets.</returns>
        private List<ListItemBag> GetSnippetBags( RockContext rockContext )
        {
            var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuidOrNull();
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( !snippetTypeGuid.HasValue )
            {
                return new List<ListItemBag>();
            }

            return new SnippetService( rockContext )
                .GetAuthorizedSnippets( RequestContext.CurrentPerson,
                    s => s.SnippetType.Guid == snippetTypeGuid.Value )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .Select( s =>
                {
                    var bag = s.ToListItemBag();

                    bag.Category = s.OwnerPersonAliasId.HasValue ? "Personal" : "Shared";

                    return bag;
                } )
                .ToList();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the conversation details between a Rock phone number and the
        /// specified individual.
        /// </summary>
        /// <param name="rockPhoneNumberGuid">The rock phone number unique identifier.</param>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns>An instance of <see cref="ConversationDetailBag"/> that describes the conversation or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetConversationDetails( Guid rockPhoneNumberGuid, Guid personGuid )
        {
            var rockPhoneNumber = SystemPhoneNumberCache.Get( rockPhoneNumberGuid );
            var messageCount = GetAttributeValue( AttributeKey.MessageCount ).AsInteger();

            if ( rockPhoneNumber == null )
            {
                return ActionBadRequest( "Invalid Rock phone number specified." );
            }

            if ( !rockPhoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to view messages for this phone number." );
            }

            using ( var rockContext = new RockContext() )
            {
                // Get the person via either their Guid.
                var person = new PersonService( rockContext )
                    .GetQueryableByKey( personGuid.ToString() )
                    .Include( p => p.PhoneNumbers )
                    .Where( p => p.Guid == personGuid )
                    .FirstOrDefault();

                // Make sure we actually found a person.
                if ( person == null )
                {
                    return ActionBadRequest( "Individual was not found." );
                }

                var responses = new CommunicationResponseService( rockContext )
                    .GetCommunicationConversationForPerson( person.Id, rockPhoneNumber )
                    .Where( r => r.CreatedDateTime.HasValue )
                    .OrderByDescending( r => r.CreatedDateTime.Value )
                    .Take( messageCount )
                    .ToList();

                var messages = responses.ToMessageBags().ToList();
                var snippets = GetSnippetBags( rockContext );

                Task.Run( () =>
                {
                    using ( var taskRockContext = new RockContext() )
                    {
                        new CommunicationResponseService( taskRockContext )
                            .UpdateReadPropertyByFromPersonId( person.Id, rockPhoneNumber );
                    }

                } );

                var bag = new ConversationDetailBag
                {
                    ConversationKey = $"SMS:{rockPhoneNumber.Guid}:{person.Guid}",
                    FullName = person.FullName,
                    IsNamelessPerson = person.IsNameless(),
                    Messages = messages,
                    PersonGuid = person.Guid,
                    PhoneNumber = person.PhoneNumbers.GetFirstSmsNumber(),
                    PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( person.PhotoUrl ),
                    Snippets = snippets
                };

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Gets all messages for the specified conversation.
        /// </summary>
        /// <param name="conversationKey">The conversation key to retrieve messages for.</param>
        /// <param name="beforeDateTime">All returned messages will be before this date. If using the last message date, you should add 1 second to its value and then filter out duplicate messages.</param>
        /// <returns>A collection of message objects or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetMessages( string conversationKey, DateTimeOffset? beforeDateTime = null )
        {
            var rockPhoneNumberGuid = CommunicationService.GetRockPhoneNumberGuidForConversationKey( conversationKey );
            var personGuid = CommunicationService.GetPersonGuidForConversationKey( conversationKey );

            if ( !rockPhoneNumberGuid.HasValue || !personGuid.HasValue )
            {
                return ActionBadRequest( "Invalid conversation." );
            }

            var rockPhoneNumber = SystemPhoneNumberCache.Get( rockPhoneNumberGuid.Value );
            var messageCount = GetAttributeValue( AttributeKey.MessageCount ).AsInteger();

            if ( rockPhoneNumber == null )
            {
                return ActionBadRequest( "Invalid Rock phone number specified." );
            }

            if ( !rockPhoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to view messages for this phone number." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

                    var communicationResponseService = new CommunicationResponseService( rockContext );
                    var recipientPersonId = new PersonService( rockContext ).GetId( personGuid.Value );

                    if ( !recipientPersonId.HasValue )
                    {
                        return ActionBadRequest( "Unknown person." );
                    }

                    var responses = communicationResponseService
                        .GetCommunicationConversationForPerson( recipientPersonId.Value, rockPhoneNumber )
                        .Where( r => r.CreatedDateTime.HasValue
                            && ( !beforeDateTime.HasValue || r.CreatedDateTime.Value < beforeDateTime.Value ) )
                        .Take( messageCount )
                        .ToList();

                    var messages = responses.ToMessageBags();

                    return ActionOk( messages );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                if ( ReportingHelper.FindSqlTimeoutException( ex ) != null )
                {
                    return ActionInternalServerError( "Unable to load SMS messages in a timely manner. You can try again or adjust the timeout setting of this block." );
                }
                else
                {
                    return ActionInternalServerError( "An error occurred when loading SMS messages." );
                }
            }
        }

        /// <summary>
        /// Resolves the snippet text and returns it.
        /// </summary>
        /// <param name="snippetGuid">The unique identifier of the snippet to be resolved.</param>
        /// <param name="personGuid">The unique identifier of the person who will receive the snippet.</param>
        /// <returns>A string that represents the snippet text.</returns>
        [BlockAction]
        public BlockActionResult ResolveSnippetText( Guid snippetGuid, Guid personGuid )
        {
            var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuidOrNull();
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            if ( !snippetTypeGuid.HasValue )
            {
                return ActionNotFound( "Snippet could not be found." );
            }

            using ( var rockContext = new RockContext() )
            {
                var snippet = new SnippetService( rockContext )
                    .GetAuthorizedSnippets( RequestContext.CurrentPerson,
                        s => s.Guid == snippetGuid && s.SnippetType.Guid == snippetTypeGuid.Value )
                    .FirstOrDefault();

                if ( snippet == null )
                {
                    return ActionNotFound( "Snippet was not found." );
                }

                var person = new PersonService( rockContext ).Get( personGuid );

                if ( person == null )
                {
                    return ActionNotFound( "Person could not be found." );
                }

                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Person", person );

                var text = snippet.Content.ResolveMergeFields( mergeFields );

                return ActionOk( text );
            }
        }

        /// <summary>
        /// Sends a message from the Rock phone number to the specified person.
        /// </summary>
        /// <param name="conversationKey">The conversation key to send the message to.</param>
        /// <param name="message">The message text to be sent.</param>
        /// <param name="attachments">The list of attachment unique identifiers.</param>
        /// <returns>An instance of <see cref="ConversationMessageBag"/> that identifies the message or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult SendMessage( string conversationKey, string message, List<Guid> attachments )
        {
            var rockPhoneNumberGuid = CommunicationService.GetRockPhoneNumberGuidForConversationKey( conversationKey );
            var personGuid = CommunicationService.GetPersonGuidForConversationKey( conversationKey );

            if ( !rockPhoneNumberGuid.HasValue || !personGuid.HasValue )
            {
                return ActionBadRequest( "Invalid conversation." );
            }

            var rockPhoneNumber = SystemPhoneNumberCache.Get( rockPhoneNumberGuid.Value );

            if ( rockPhoneNumber == null )
            {
                return ActionNotFound( "Rock phone number was not found." );
            }

            if ( !rockPhoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to view messages for this phone number." );
            }

            if ( RequestContext.CurrentPerson == null )
            {
                return ActionBadRequest( "Must be logged in to send messages." );
            }

            // If no message and no attachments then fail.
            if ( message.IsNullOrWhiteSpace() && attachments?.Any() != true )
            {
                return ActionBadRequest( "Must provide either message or attachments." );
            }

            using ( var rockContext = new RockContext() )
            {
                try
                {
                    // The sender is the logged in user.
                    var fromPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId.Value;

                    var responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );
                    var toPrimaryAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personGuid.Value );

                    var attachmentFiles = new List<BinaryFile>();

                    if ( attachments != null )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        foreach ( var attachmentGuid in attachments )
                        {
                            var binaryFile = binaryFileService.Get( attachmentGuid );

                            if ( binaryFile != null )
                            {
                                attachmentFiles.Add( binaryFile );
                            }
                        }
                    }

                    // Create and enqueue the communication
                    var communication = Rock.Communication.Medium.Sms.CreateCommunicationMobile( RequestContext.CurrentPerson, toPrimaryAliasId, message, rockPhoneNumber, responseCode, attachmentFiles, rockContext );

                    if ( communication.Recipients.Count == 0 )
                    {
                        return ActionInternalServerError( "Unable to determine recipient of message." );
                    }

                    // Must use a new context in order to get an object that
                    // has valid navigation properties.
                    using ( var rockContext2 = new RockContext() )
                    {
                        var recipientId = communication.Recipients.First().Id;

                        var messageBag = new CommunicationRecipientService( rockContext2 )
                            .GetConversationMessageBag( recipientId );

                        return ActionOk( messageBag );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );

                    return ActionInternalServerError( "Unexpected error encountered when trying to send message." );
                }
            }
        }

        /// <summary>
        /// Marks the conversation as read. This will mark all responses in the
        /// conversation as read.
        /// </summary>
        /// <param name="conversationKey">The conversation key to be marked as read.</param>
        /// <returns>A 204-No Content response if the conversation was marked as read. </returns>
        [BlockAction]
        public BlockActionResult MarkConversationAsRead( string conversationKey )
        {
            var phoneNumberGuid = CommunicationService.GetRockPhoneNumberGuidForConversationKey( conversationKey );
            var personGuid = CommunicationService.GetPersonGuidForConversationKey( conversationKey );

            if ( !phoneNumberGuid.HasValue || !personGuid.HasValue )
            {
                return ActionBadRequest( "Invalid conversation." );
            }

            using ( var rockContext = new RockContext() )
            {
                var personId = new PersonService( rockContext ).GetId( personGuid.Value );
                var rockPhoneNumber = SystemPhoneNumberCache.Get( phoneNumberGuid.Value );

                if ( rockPhoneNumber == null || !personId.HasValue )
                {
                    return ActionBadRequest( "Invalid message." );
                }

                if ( !rockPhoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to view messages for this phone number." );
                }

                Task.Run( () =>
                {
                    using ( var taskRockContext = new RockContext() )
                    {
                        new CommunicationResponseService( taskRockContext )
                            .UpdateReadPropertyByFromPersonId( personId.Value, rockPhoneNumber );
                    }

                } );

                return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
            }
        }

        #endregion
    }
}
