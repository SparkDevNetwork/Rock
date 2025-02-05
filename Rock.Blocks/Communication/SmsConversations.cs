using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Core.NotificationMessageTypes;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.SmsConversations;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Communication
{
    [DisplayName( "SMS Conversations" )]
    [Category( "Communication" )]
    [Description( "Block for having SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false." )]
    [IconCssClass( "fa fa-message" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]

    [BooleanField( "Show only personal SMS number",
        Key = AttributeKey.ShowOnlyPersonalSmsNumber,
        Description = "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        DefaultBooleanValue = false,
        Order = 2
         )]

    [BooleanField( "Hide personal SMS numbers",
        Key = AttributeKey.HidePersonalSmsNumbers,
        Description = "When enabled, only SMS Numbers that are not 'Assigned to a person' will be shown.",
        DefaultBooleanValue = false,
        Order = 3
         )]

    [BooleanField( "Enable SMS Send",
        Key = AttributeKey.EnableSmsSend,
        Description = "Allow SMS messages to be sent from the block.",
        DefaultBooleanValue = true,
        Order = 4
         )]

    [IntegerField( "Show Conversations From Months Ago",
        Key = AttributeKey.ShowConversationsFromMonthsAgo,
        Description = "Limits the conversations shown in the left pane to those of X months ago or newer. This does not affect the actual messages shown on the right.",
        DefaultIntegerValue = 6,
        Order = 5
         )]

    [IntegerField( "Max Conversations",
        Key = AttributeKey.MaxConversations,
        Description = "Limits the number of conversations shown in the left pane. This does not affect the actual messages shown on the right.",
        DefaultIntegerValue = 100,
        Order = 6
         )]

    [CodeEditorField( "Person Info Lava Template",
        Key = AttributeKey.PersonInfoLavaTemplate,
        Description = "A Lava template to display person information about the selected Communication Recipient.",
        DefaultValue = "{{ Person.FullName }}",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 300,
        IsRequired = false,
        Order = 7
         )]

    [CustomCheckboxListField("Note Types",
        Description = @"Optional list of note types to limit the note editor to. Note types must have the ""User Selectable"" property enabled and Person Entity Type selected.",
        ListSource = ListSource.SQL_SELECTABLE_PERSON_NOTE_TYPES,
        IsRequired = false,
        Order = 8,
        Key = AttributeKey.NoteTypes)]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "71944E38-A578-40B7-882F-A25CCBE9D408" )]
    [Rock.SystemGuid.BlockTypeGuid( "3B052AC5-60DB-4490-BC47-C3471A2CA515" )]
    public class SmsConversations : RockBlockType
    {
        #region Keys and Values

        private static class AttributeKey
        {
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ShowOnlyPersonalSmsNumber = "ShowOnlyPersonalSmsNumber";
            public const string HidePersonalSmsNumbers = "HidePersonalSmsNumbers";
            public const string EnableSmsSend = "EnableSmsSend";
            public const string ShowConversationsFromMonthsAgo = "ShowConversationsFromMonthsAgo";
            public const string MaxConversations = "MaxConversations";
            public const string PersonInfoLavaTemplate = "PersonInfoLavaTemplate";
            public const string NoteTypes = "NoteTypes";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        private static class PreferenceKey
        {
            public const string SelectedSystemPhoneNumber = "selected-system-phone-number";
            public const string SelectedMessageFilter = "selected-message-filter";
        }

        private static class ListSource
        {
            public const string SQL_SELECTABLE_PERSON_NOTE_TYPES = @"
            SELECT 
                nt.[Guid] AS [Value],
                nt.[Name] AS [Text]
            FROM [NoteType] nt
            INNER JOIN [EntityType] et ON et.[Id] = nt.[EntityTypeId]
            WHERE nt.[UserSelectable] = 1
            AND et.[Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'";
        }

        #endregion

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        #endregion

        #region Properties

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        protected Guid? SelectedSystemPhoneNumber => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedSystemPhoneNumber )
            .AsGuidOrNull();

        protected CommunicationMessageFilter SelectedMessageFilter => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.SelectedMessageFilter )
            .ConvertToEnum<CommunicationMessageFilter>( CommunicationMessageFilter.ShowUnreadReplies );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            SmsConversationsInitializationBox box = new SmsConversationsInitializationBox();

            var noteTypes = GetConfiguredNoteTypes();

            box.SystemPhoneNumbers = LoadPhoneNumbers();
            box.MessageFilter = SelectedMessageFilter;
            box.NoteTypes = noteTypes.Select( nt => GetNoteTypeBag( nt, RequestContext.CurrentPerson ) ).ToList();
            box.Snippets = GetSnippetBags();
            box.IsNewMessageButtonVisible = GetAttributeValue( AttributeKey.EnableSmsSend ).AsBoolean();
            box.CanEditOrAdministrate = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            if ( box.SystemPhoneNumbers.Count == 0 )
            {
                return box;
            }

            var responseListingStatusBag = LoadResponseListing( null );

            if ( responseListingStatusBag.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = responseListingStatusBag.ErrorMessage;
                return box;
            }

            box.Conversations = responseListingStatusBag.Conversations;

            return box;
        }

        /// <summary>
        /// Loads all available phone numbers based on various filtering criteria, including user authorization and specific settings.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> representing the available phone numbers.</returns>
        private List<ListItemBag> LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();
            List<ListItemBag> systemPhoneNumbers = new List<ListItemBag>();

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( spn => selectedNumberGuids.Contains( spn.Guid ) ).ToList();
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( spn => !spn.AssignedToPersonAliasId.HasValue ).ToList();
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                smsNumbers = smsNumbers.Where( spn => RequestContext.CurrentPerson.Aliases.Any( a => a.Id == spn.AssignedToPersonAliasId ) ).ToList();
            }

            // If the available SMS numbers do not contain the SelectedSystemPhoneNumber preference then reset the preference
            if ( !smsNumbers.Any( spn => spn.Guid == SelectedSystemPhoneNumber ) )
            {
                // Retrieve the Guid of the first SMS number if available, else set it to null
                var selectedSystemPhoneNumber = smsNumbers.FirstOrDefault()?.Guid.ToString();
                this.PersonPreferences.SetValue( PreferenceKey.SelectedSystemPhoneNumber, selectedSystemPhoneNumber );
                this.PersonPreferences.Save();
            }

            foreach ( var smsNumber in smsNumbers )
            {
                systemPhoneNumbers.Add( new ListItemBag
                {
                    Value = smsNumber.Guid.ToString(),
                    Text = smsNumber.Name,
                } );
            }

            return systemPhoneNumbers;
        }

        /// <summary>
        /// Gets the configured note types for this block.
        /// </summary>
        /// <returns>A list of <see cref="NoteTypeCache"/> objects that represent the configured note types.</returns>
        private List<NoteTypeCache> GetConfiguredNoteTypes()
        {
            var noteTypes = NoteTypeCache.GetByEntity( EntityTypeCache.GetId<Rock.Model.Person>(), string.Empty, string.Empty, false );

            // If block is configured to only allow certain note types, limit notes to those types.
            var configuredNoteTypes = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
            if ( configuredNoteTypes.Any() )
            {
                noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
            }

            noteTypes = noteTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            return noteTypes;
        }

        /// <summary>
        /// Gets the note type bag that will represent the given note type.
        /// </summary>
        /// <param name="noteType">The note type object to be represented.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>A new instance of <see cref="NoteTypeBag"/>.</returns>
        private static NoteTypeBag GetNoteTypeBag( NoteTypeCache noteType, Rock.Model.Person currentPerson )
        {
            var note = new Note
            {
                NoteTypeId = noteType.Id
            };

            note.LoadAttributes();

            return new NoteTypeBag
            {
                IdKey = noteType.IdKey,
                Name = noteType.Name,
                Color = noteType.Color,
                IconCssClass = noteType.IconCssClass,
                UserSelectable = noteType.UserSelectable && noteType.IsAuthorized( Authorization.EDIT, currentPerson ),
                AllowsReplies = noteType.AllowsReplies,
                MaxReplyDepth = noteType.MaxReplyDepth ?? -1,
                AllowsWatching = noteType.AllowsWatching,
                IsMentionEnabled = noteType.FormatType != NoteFormatType.Unstructured && noteType.IsMentionEnabled,
                Attributes = note.GetPublicAttributesForEdit( currentPerson, enforceSecurity: true )
            };
        }

        /// <summary>
        /// Retrieves a list of snippet bags, filtered by the SMS snippet type and authorized for the current user.
        /// </summary>
        /// <returns>A list of <see cref="SnippetBag"/> containing the snippet information and visibility status.</returns>
        private List<SnippetBag> GetSnippetBags()
        {
            var snippetTypeGuid = Rock.SystemGuid.SnippetType.SMS.AsGuid();
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            return new SnippetService( RockContext )
                .GetAuthorizedSnippets( RequestContext.CurrentPerson,
                    s => s.SnippetType.Guid == snippetTypeGuid )
                .Where( s => s.IsActive )
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Name )
                .Select( s => new SnippetBag
                {
                    Snippet = new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name
                    },
                    Categories = GetSnippetCategoryGuidHierarchy( s.Category ),
                    SnippetVisibility = s.OwnerPersonAliasId.HasValue ? "Personal" : "Shared"
                } )
                .ToList();
        }

        /// <summary>
        /// Retrieves the GUID hierarchy of a snippet's category, traversing up the parent categories.
        /// </summary>
        /// <param name="category">The category to start traversing from.</param>
        /// <returns>A list of GUIDs representing the category hierarchy.</returns>
        private List<Guid> GetSnippetCategoryGuidHierarchy( Category category )
        {
            var guids = new List<Guid>();

            // Traverse up the category hierarchy
            while ( category != null )
            {
                // If guids already contains this category Guid, we got a circular reference. So just return the path so far.
                if (guids.Contains( category.Guid ) )
                {
                    return guids;
                }
                guids.Add( category.Guid );
                category = category.ParentCategory;
            }

            // Reverse the list to ensure parent categories appear before child categories
            guids.Reverse();

            return guids;
        }

        /// <summary>
        /// Loads the Conversations along with the most recent message sent.
        /// </summary>
        /// <param name="personId">A person Id that can be passed in to only retrieve conversations with that person</param>
        /// <returns>A <see cref="ResponseListingStatusBag"></see> that contains a list of conversations along with an optional error message</returns>
        private ResponseListingStatusBag LoadResponseListing( int? personId )
        {
            ResponseListingStatusBag bag = new ResponseListingStatusBag
            {
                Conversations = new List<ConversationBag>()
            };

            if ( !SelectedSystemPhoneNumber.HasValue )
            {
                return bag;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

                    var communicationResponseService = new CommunicationResponseService( rockContext );

                    int months = GetAttributeValue( AttributeKey.ShowConversationsFromMonthsAgo ).AsInteger();

                    var startDateTime = RockDateTime.Now.AddMonths( -months );

                    var maxConversations = this.GetAttributeValue( AttributeKey.MaxConversations ).AsIntegerOrNull() ?? 1000;
                    var messageFilterOption = SelectedMessageFilter;
                    var smsSystemPhoneNumberId = SystemPhoneNumberCache.Get( SelectedSystemPhoneNumber.Value ).Id;

                    var responseListItems = communicationResponseService.GetCommunicationAndResponseRecipients( smsSystemPhoneNumberId, startDateTime, maxConversations, messageFilterOption, personId );
                    var personService = new PersonService( rockContext );

                    foreach ( var r in responseListItems )
                    {
                        var recipientPerson = r.PersonId.HasValue ? personService.Get( r.PersonId.Value ) : null;
                        var smsMessage = r.SMSMessage;

                        if ( r.SMSMessage.IsNullOrWhiteSpace() && r.HasAttachments( rockContext ) )
                        {
                            smsMessage = "Image";
                        }

                        // TODO: Remove RecipientPersonAliasId when the ReminderList Block is converted to Obsidian.
                        bag.Conversations.Add( new ConversationBag()
                        {
                            ConversationKey = r.ConversationKey,
                            RecipientPersonAliasIdKey = r.RecipientPersonAliasId.HasValue ? IdHasher.Instance.GetHash( r.RecipientPersonAliasId.Value ) : null,
                            RecipientPersonAliasGuid = recipientPerson.PrimaryAlias.Guid,
                            RecipientPersonAliasId = r.RecipientPersonAliasId ?? 0,
                            RecipientPhoneNumber = r.ContactKey,
                            IsConversationRead = r.IsRead,
                            RecipientPhotoUrl = recipientPerson != null ? Rock.Model.Person.GetPersonPhotoUrl( recipientPerson, 256, 256 ) : "/Assets/Images/person-no-photo-unknown.svg?width=256&height=256",
                            IsRecipientNamelessPerson = r.IsNamelessPerson,
                            RecipientFullName = r.FullName,
                            Messages = new List<MessageBag>
                        {
                            new MessageBag
                            {
                                MessageKey = r.MessageKey,
                                SMSMessage = smsMessage,
                                IsOutbound = r.IsOutbound,
                                OutboundSenderFullName = r.OutboundSenderFullName,
                                CreatedDateTime = r.CreatedDateTime,
                            }
                        }
                        } );
                    }
                    return bag;
                }
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, ex.Message );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                if ( sqlTimeoutException != null )
                {
                    bag.ErrorMessage = "Unable to load SMS responses in a timely manner. You can try again or adjust the timeout setting of this block.";
                }
                else
                {
                    bag.ErrorMessage = "An error occurred when loading SMS responses";
                }

                return bag;
            }
        }

        /// <summary>
        /// Retrieves the recipient description by resolving a Lava template with merge fields.
        /// </summary>
        /// <param name="recipientPerson">The person object representing the recipient.</param>
        /// <returns>The resolved HTML string representing the recipient's description.</returns>
        private string GetRecipientDescription( Person recipientPerson )
        {
            var lava = GetAttributeValue( AttributeKey.PersonInfoLavaTemplate );
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Person", recipientPerson );

            string html = lava.ResolveMergeFields( mergeFields );

            return html;
        }

        /// <summary>
        /// Sends a message to the recipient, including handling attachments and system phone number selection.
        /// </summary>
        /// <param name="recipientPersonPrimaryAliasId">The primary alias ID of the recipient person.</param>
        /// <param name="bag">The message data, including text and attachments.</param>
        /// <returns>An error message if the message could not be sent; otherwise, an empty string.</returns>
        private string SendMessage( int? recipientPersonPrimaryAliasId, SendMessageBag bag )
        {
            if ( bag.RecipientPersonAliasIdKey == string.Empty || ( bag.Message.IsNullOrWhiteSpace() && bag.AttachmentGuid == null ) )
            {
                return "Message cannot be sent without text or an image.";
            }

            // The sending phone is the selected one
            var smsSystemPhoneNumber = SelectedSystemPhoneNumber.HasValue
                ? SystemPhoneNumberCache.Get( SelectedSystemPhoneNumber.Value )
                : null;

            if ( smsSystemPhoneNumber == null )
            {
                return "A System Phone Number must be selected.";
            }

            string responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( RockContext );

            BinaryFile binaryFile = null;

            if ( bag.AttachmentGuid.HasValue )
            {
                // If this is a response using the conversation window and a photo file has been uploaded then add it
                binaryFile = new BinaryFileService( RockContext ).Get( bag.AttachmentGuid.Value );
            }

            var photos = binaryFile != null ? new List<BinaryFile> { binaryFile } : null;

            // Create and enqueue the communication
            Rock.Communication.Medium.Sms.CreateCommunicationMobile( RequestContext.CurrentPerson, recipientPersonPrimaryAliasId, bag.Message, smsSystemPhoneNumber, responseCode, photos, RockContext );

            return string.Empty;
        }

        /// <summary>
        /// Finds the recipient person from their person key.
        /// </summary>
        /// <param name="personKey">The person key for the recipient person.</param>
        /// <returns>The recipient person, or null if not found.</returns>
        private Person FindRecipientFromPersonKey( string personKey )
        {
            var personAliasService = new PersonAliasService( RockContext );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var recipientPersonAliasId = Rock.Utility.IdHasher.Instance.GetId( personKey ) ?? personAliasService.Get( personKey )?.Id ?? personAliasService.GetByAliasGuid( personKey.AsGuid() )?.Id;

            if ( !recipientPersonAliasId.HasValue )
            {
                return null;
            }

            var recipientPerson = personAliasService.GetPerson( recipientPersonAliasId.Value );

            if ( recipientPerson == null )
            {
                return null;
            }

            return recipientPerson;
        }

        /// <summary>
        /// Updates a person object with the data from the provided editor bag.
        /// </summary>
        /// <param name="person">The person object to be updated.</param>
        /// <param name="personBag">The editor bag containing the updated data.</param>
        private void UpdatePersonFromEditorBag( Person person, PersonBasicEditorBag personBag )
        {
            // Update person properties from PersonBasicEditorBag
            person.FirstName = personBag.FirstName;
            person.LastName = personBag.LastName;
            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            person.ConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT.AsGuid() ).Id;

            if ( personBag.PersonTitle != null )
            {
                person.TitleValueId = DefinedValueCache.Get( personBag.PersonTitle.Value )?.Id;
            }
            if ( personBag.PersonSuffix != null )
            {
                person.SuffixValueId = DefinedValueCache.Get( personBag.PersonSuffix.Value )?.Id;
            }
            if ( personBag.PersonMaritalStatus != null )
            {
                person.MaritalStatusValueId = DefinedValueCache.Get( personBag.PersonMaritalStatus.Value )?.Id;
            }
            if ( personBag.PersonRace != null )
            {
                person.RaceValueId = DefinedValueCache.Get( personBag.PersonRace.Value )?.Id;
            }
            if ( personBag.PersonEthnicity != null )
            {
                person.EthnicityValueId = DefinedValueCache.Get( personBag.PersonEthnicity.Value )?.Id;
            }
            if ( personBag.PersonGender.HasValue )
            {
                person.Gender = personBag.PersonGender.Value;
            }
            if ( personBag.PersonBirthDate != null )
            {
                person.SetBirthDate( new DateTime( personBag.PersonBirthDate.Year, personBag.PersonBirthDate.Month, personBag.PersonBirthDate.Day ) );
            }
            if ( personBag.PersonGradeOffset != null )
            {
                int offset = Int32.Parse( personBag.PersonGradeOffset.Value );

                if ( offset >= 0 )
                {
                    person.GradeOffset = offset;
                }
            }

            UpdatePhoneNumber( person, personBag );
        }

        /// <summary>
        /// Updates the phone number for a person based on the provided editor bag data.
        /// </summary>
        /// <param name="person">The person object to update the phone number for.</param>
        /// <param name="personBag">The editor bag containing phone number data.</param>
        private void UpdatePhoneNumber( Person person, PersonBasicEditorBag personBag )
        {
            if ( !string.IsNullOrWhiteSpace( personBag.MobilePhoneNumber ) )
            {
                var cleanNumber = PhoneNumber.CleanNumber( personBag.MobilePhoneNumber );
                var phone = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == cleanNumber );
                if ( phone == null )
                {
                    phone = new PhoneNumber
                    {
                        Number = cleanNumber,
                        CountryCode = personBag.MobilePhoneCountryCode,
                        IsMessagingEnabled = personBag.IsMessagingEnabled ?? false,
                        NumberTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id
                    };
                    person.PhoneNumbers.Add( phone );
                }
                else
                {
                    phone.CountryCode = personBag.MobilePhoneCountryCode;
                    phone.IsMessagingEnabled = personBag.IsMessagingEnabled ?? false;
                    phone.Number = cleanNumber;
                }
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Retrieves the conversation for a specific recipient and updates the conversation details.
        /// </summary>
        /// <param name="bag">The conversation bag containing recipient and other related data.</param>
        /// <returns>The updated conversation bag with messages and other information.</returns>
        [BlockAction]
        public BlockActionResult GetConversationForRecipient( ConversationBag bag )
        {
            bag.Messages = new List<MessageBag>();

            var smsSystemPhoneNumber = SelectedSystemPhoneNumber.HasValue
                ? SystemPhoneNumberCache.Get( SelectedSystemPhoneNumber.Value )
                : null;

            if ( smsSystemPhoneNumber == null )
            {
                return ActionBadRequest( "A System Phone Number must be selected." );
            }

            var personKey = bag.RecipientPersonAliasIdKey ?? bag.RecipientPersonGuid.ToString();
            var recipientPerson = FindRecipientFromPersonKey( personKey );
            if ( recipientPerson == null )
            {
                return ActionBadRequest( "Could not find the Recipient Person." );
            }

            var isRecipientPartOfMergeRequest = recipientPerson.IsNameless() && recipientPerson.IsPartOfMergeRequest();

            if ( recipientPerson.IsNameless() && !isRecipientPartOfMergeRequest )
            {
                bag.IsLinkToPersonVisible = true;
                bag.IsViewMergeRequestVisible = false;
            }
            else
            {
                bag.IsLinkToPersonVisible = false;
                bag.IsViewMergeRequestVisible = isRecipientPartOfMergeRequest;
            }

            // If we didn't retrieve the recipientPerson from the RecipientPersonAliasIdKey then we found the person
            // from a realtime message. We still various keys for selected conversation features. 
            if ( bag.RecipientPersonAliasIdKey.IsNullOrWhiteSpace() )
            {
                bag.RecipientPersonAliasIdKey = recipientPerson.PrimaryAliasId.HasValue ? IdHasher.Instance.GetHash( recipientPerson.PrimaryAliasId.Value ) : null;
                bag.RecipientPersonAliasGuid = recipientPerson.PrimaryAlias.Guid;
                // TODO: Remove RecipientPersonAliasId when the ReminderList Block is converted to Obsidian.
                bag.RecipientPersonAliasId = recipientPerson.PrimaryAliasId ?? 0;
            }

            bag.RecipientDescription = GetRecipientDescription( recipientPerson );
            bag.EntityTypeGuidForReminder = SystemGuid.EntityType.PERSON_ALIAS.AsGuidOrNull();
            bag.EntityGuidForReminder = recipientPerson.PrimaryAlias.Guid;

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
                    var communicationResponseService = new CommunicationResponseService( rockContext );
                    List<CommunicationRecipientResponse> responses = communicationResponseService.GetCommunicationConversationForPerson( recipientPerson.Id, smsSystemPhoneNumber );

                    var isConversationUnread = responses.Any( r => !r.IsRead );

                    foreach ( var response in responses )
                    {
                        List<string> attachmentUrls = new List<string>();
                        if ( response.HasAttachments( rockContext ) )
                        {
                            foreach ( var binaryFileGuid in response.GetBinaryFileGuids( rockContext ) )
                            {
                                string imageUrl = FileUrlHelper.GetImageUrl( binaryFileGuid, new GetImageUrlOptions { Width = 200 } );
                                attachmentUrls.Add( imageUrl );
                            }
                        }

                        bag.Messages.Add( new MessageBag()
                        {
                            MessageKey = response.MessageKey,
                            SMSMessage = response.SMSMessage,
                            IsOutbound = response.IsOutbound,
                            OutboundSenderFullName = response.OutboundSenderFullName,
                            CreatedDateTime = response.CreatedDateTime,
                            AttachmentUrls = attachmentUrls,
                        } );
                    }

                    if ( isConversationUnread && ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ) )
                    {
                        communicationResponseService.UpdateReadPropertyByFromPersonId( recipientPerson.Id, smsSystemPhoneNumber );
                        bag.IsConversationRead = true;
                    }

                    return ActionOk( bag );
                }
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, ex.Message );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                if ( sqlTimeoutException != null )
                {
                    return ActionBadRequest( "Unable to load SMS responses for recipient in a timely manner. You can try again or adjust the timeout setting of this block." );
                }
                else
                {
                    return ActionBadRequest( "An error occurred when loading SMS responses for recipient" );
                }
            }
        }

        /// <summary>
        /// Sends a message to a new recipient and processes any required phone number and attachment details.
        /// </summary>
        /// <param name="bag">The message data to be sent.</param>
        /// <returns>Action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult SendMessageToNewRecipient( SendMessageBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to send a message." );
            }

            var recipientPerson = FindRecipientFromPersonKey( bag.RecipientPersonAliasIdKey );
            if ( recipientPerson == null )
            {
                return ActionBadRequest( "Could not find the Recipient Person." );
            }

            var personHasSMSNumbers = recipientPerson.PhoneNumbers.Where( a => a.IsMessagingEnabled ).Any();
            if ( !personHasSMSNumbers )
            {
                return ActionBadRequest( "The selected person does not have an SMS enabled Phone number." );
            }

            var sendMessageResult = SendMessage( recipientPerson.PrimaryAliasId, bag );

            if ( sendMessageResult.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( sendMessageResult );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sends a message to an existing recipient and processes any required phone number and attachment details.
        /// </summary>
        /// <param name="bag">The message data to be sent.</param>
        /// <returns>Action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult SendMessageToExistingRecipient( SendMessageBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to send a message." );
            }

            var recipientPerson = FindRecipientFromPersonKey( bag.RecipientPersonAliasIdKey );
            if ( recipientPerson == null )
            {
                return ActionBadRequest( "Could not find the Recipient Person." );
            }

            var sendMessageResult = SendMessage( recipientPerson.PrimaryAliasId, bag );

            if ( sendMessageResult.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( sendMessageResult );
            }
            return ActionOk();
        }

        /// <summary>
        /// Toggles the read status of a conversation based on its current status.
        /// </summary>
        /// <param name="bag">The conversation data including read status.</param>
        /// <returns>Action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult ToggleConversationReadStatus( ConversationBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to change the read status of a conversation." );
            }

            var smsSystemPhoneNumber = SelectedSystemPhoneNumber.HasValue
                ? SystemPhoneNumberCache.Get( SelectedSystemPhoneNumber.Value )
                : null;

            if ( smsSystemPhoneNumber == null )
            {
                return ActionBadRequest( "A System Phone Number must be selected." );
            }

            var personKey = bag.RecipientPersonAliasIdKey ?? bag.RecipientPersonGuid.ToString();
            var recipientPerson = FindRecipientFromPersonKey( personKey );
            if ( recipientPerson == null )
            {
                return ActionBadRequest( "Could not find the Recipient Person." );
            }

            var communicationResponseService = new CommunicationResponseService( RockContext );

            if ( bag.IsConversationRead )
            {
                var result = communicationResponseService.MarkResponseAsUnread( recipientPerson.Id, smsSystemPhoneNumber );
                if ( result.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( result );
                }
            }
            else
            {
                communicationResponseService.UpdateReadPropertyByFromPersonId( recipientPerson.Id, smsSystemPhoneNumber );
            }

            return ActionOk();
        }

        /// <summary>
        /// Saves the changes made to a note for the selected person.
        /// </summary>
        /// <param name="bag">The note data, including the changes to be saved.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( SmsConversationsSaveNoteBag bag )
        {
            var selectedPerson = FindRecipientFromPersonKey( bag.SelectedPersonAliasIdKey );
            if ( selectedPerson == null )
            {
                return ActionBadRequest( "Could not find the Selected Person." );
            }

            var request = bag.NoteRequestBag;

            if ( request == null || !request.IsValidProperty( nameof( NoteEditBag.IdKey ) ) )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                Note note = new Note()
                {
                    EntityId = selectedPerson.Id,
                };
                var mentionedPersonIds = new List<int>();

                if ( !request.IsValidProperty( nameof( request.Bag.ParentNoteIdKey ) ) )
                {
                    return ActionBadRequest( "New note details must include parent note identifier." );
                }

                noteService.Add( note );

                if ( request.IsValidProperty( nameof( request.Bag.NoteTypeIdKey ) ) )
                {
                    // Find the note type from either the request or the existing note.
                    var noteTypeId = IdHasher.Instance.GetId( request.Bag.NoteTypeIdKey );
                    var noteType = noteTypeId.HasValue ? NoteTypeCache.Get( noteTypeId.Value ) : null;

                    if ( noteType == null )
                    {
                        return ActionBadRequest( "Note type is invalid." );
                    }

                    // Check if the specified note type is valid for selection
                    var isValidNoteType = GetConfiguredNoteTypes()
                        .Any( nt => nt.UserSelectable && nt.Id == noteType.Id );

                    if ( !isValidNoteType )
                    {
                        return ActionBadRequest( "Note type is invalid." );
                    }

                    note.NoteTypeId = noteType.Id;
                }

                request.IfValidProperty( nameof( request.Bag.Text ), () =>
                {
                    var noteTypeCache = NoteTypeCache.Get( note.NoteTypeId );
                    if ( noteTypeCache.FormatType != NoteFormatType.Unstructured && noteTypeCache.IsMentionEnabled )
                    {
                        mentionedPersonIds = noteService.GetNewPersonIdsMentionedInContent( request.Bag.Text, note.Text );
                    }
                    note.Text = request.Bag.Text;
                } );

                request.IfValidProperty( nameof( request.Bag.IsAlert ),
                    () => note.IsAlert = request.Bag.IsAlert );

                request.IfValidProperty( nameof( request.Bag.IsPrivate ), () =>
                {
                    note.IsPrivateNote = request.Bag.IsPrivate;

                    note.UpdateCaption();
                } );

                request.IfValidProperty( nameof( request.Bag.IsPinned ), () =>
                {
                    note.IsPinned = request.Bag.IsPinned;
                } );

                note.EditedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                note.EditedDateTime = RockDateTime.Now;
                note.NoteUrl = this.GetCurrentPageUrl();

                note.LoadAttributes( rockContext );
                note.SetPublicAttributeValues( request.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );

                if ( note.Id == 0 && !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit note." );
                }

                rockContext.SaveChanges();
                note.SaveAttributeValues( rockContext );

                // If we have any new mentioned person ids, start a background
                // task to create the notifications.
                if ( mentionedPersonIds.Any() )
                {
                    Task.Run( () =>
                    {
                        foreach ( var personId in mentionedPersonIds )
                        {
                            NoteMention.CreateNotificationMessage( note, personId, RequestContext.CurrentPerson.Id, PageCache.Id, RequestContext.GetPageParameters() );
                        }
                    } );
                }

                // Return the selected Person's IdKey for the View Profile link.
                return ActionOk( selectedPerson.IdKey );
            }
        }

        /// <summary>
        /// Links an existing person to a nameless person for merging purposes.
        /// </summary>
        /// <param name="existingPersonAliasGuid">The GUID of the existing person's alias.</param>
        /// <param name="recipientPersonAliasIdKey">The alias ID key of the nameless person.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult LinkToExistingPerson( string existingPersonAliasGuid, string recipientPersonAliasIdKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to link to an existing person." );
            }

            var personAliasService = new PersonAliasService( RockContext );
            var personService = new PersonService( RockContext );

            // Get the Person Alias and then the associated Person using the Person Alias GUID
            var personAlias = personAliasService.Get( new Guid( existingPersonAliasGuid ) );
            if ( personAlias == null )
            {
                return ActionBadRequest( "Person Alias not found" );
            }

            // Get the existing person using the associated Person's ID
            var existingPerson = personService.Get( personAlias.PersonId );
            if ( existingPerson == null )
            {
                return ActionBadRequest( "Existing person not found" );
            }

            // Get the nameless person by their ID
            var namelessPerson = FindRecipientFromPersonKey( recipientPersonAliasIdKey );
            if ( namelessPerson == null )
            {
                return ActionBadRequest( "Nameless person not found" );
            }

            // Create and save the merge request
            var mergeRequest = existingPerson.CreateMergeRequest( namelessPerson );
            var entitySetService = new EntitySetService( RockContext );
            entitySetService.Add( mergeRequest );
            RockContext.SaveChanges();

            // Redirect to merge page
            var mergePageUrl = string.Format( "/PersonMerge/{0}", mergeRequest.Id );
            return ActionOk( mergePageUrl );
        }

        /// <summary>
        /// Saves a new person by either updating an existing record or creating a new one, then initiates a merge process.
        /// </summary>
        /// <param name="personBag">The bag containing the new person's data.</param>
        /// <param name="recipientPersonAliasIdKey">The alias ID key for the nameless person to merge with.</param>
        /// <returns>An action result indicating the URL for the merge page.</returns>
        [BlockAction]
        public BlockActionResult SaveNewPerson( PersonBasicEditorBag personBag, string recipientPersonAliasIdKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to save a new person." );
            }

            var personService = new PersonService( RockContext );
            var cleanMobilePhone = PhoneNumber.CleanNumber( personBag.MobilePhoneNumber );

            // Attempt to find an existing person by phone number
            var existingPerson = personService.Queryable( "PhoneNumbers" )
                .FirstOrDefault( p => p.PhoneNumbers.Any( n => n.Number == cleanMobilePhone ) );

            var namelessPerson = FindRecipientFromPersonKey( recipientPersonAliasIdKey );
            if ( namelessPerson == null )
            {
                return ActionBadRequest( "Nameless person not found." );
            }

            Person personToUpdate = existingPerson ?? new Person();

            if ( existingPerson == null )
            {
                personService.Add( personToUpdate );
            }

            UpdatePersonFromEditorBag( personToUpdate, personBag );

            RockContext.SaveChanges();

            // Create a merge request
            var mergeRequest = namelessPerson.CreateMergeRequest( personToUpdate );
            var entitySetService = new EntitySetService( RockContext );
            entitySetService.Add( mergeRequest );
            RockContext.SaveChanges();

            var mergePageUrl = string.Format( "/PersonMerge/{0}", mergeRequest.Id );
            return ActionOk( mergePageUrl );
        }

        /// <summary>
        /// Views an existing merge request for a nameless person.
        /// </summary>
        /// <param name="recipientPersonAliasIdKey">The alias ID key of the nameless person.</param>
        /// <returns>An action result containing the URL to the merge request page.</returns>
        [BlockAction]
        public BlockActionResult ViewMergeRequest( string recipientPersonAliasIdKey )
        {
            var namelessPerson = FindRecipientFromPersonKey( recipientPersonAliasIdKey );
            if ( namelessPerson == null )
            {
                return ActionBadRequest( "Nameless person not found." );
            }

            var mergeRequest = namelessPerson.GetMergeRequest( RockContext );
            var mergePageUrl = string.Format( "/PersonMerge/{0}", mergeRequest.Id );
            return ActionOk( mergePageUrl );
        }

        /// <summary>
        /// Inserts a snippet of text for a recipient, resolving any merge fields.
        /// </summary>
        /// <param name="snippetGuid">The GUID of the snippet to insert.</param>
        /// <param name="recipientPersonAliasIdKey">The alias ID key for the recipient person.</param>
        /// <returns>An action result containing the resolved snippet text.</returns>
        [BlockAction]
        public BlockActionResult InsertSnippet( Guid snippetGuid, string recipientPersonAliasIdKey )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "You are not authorized to insert a Snippet." );
            }

            var snippetTypeGuid = Rock.SystemGuid.SnippetType.SMS.AsGuid();
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            using ( var rockContext = new RockContext() )
            {
                var snippet = new SnippetService( rockContext )
                    .GetAuthorizedSnippets( RequestContext.CurrentPerson,
                        s => s.Guid == snippetGuid && s.SnippetType.Guid == snippetTypeGuid )
                    .FirstOrDefault();

                if ( snippet == null )
                {
                    return ActionNotFound( "Snippet was not found." );
                }

                var recipientPerson = FindRecipientFromPersonKey( recipientPersonAliasIdKey );
                if ( recipientPerson == null )
                {
                    return ActionBadRequest( "Could not find the Recipient Person." );
                }

                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Person", recipientPerson );

                var text = snippet.Content.ResolveMergeFields( mergeFields );

                return ActionOk( text );
            }
        }

        /// <summary>
        /// Reloads the list of conversations and returns them.
        /// </summary>
        /// <returns>An action result containing the list of conversations or an error message if the reload fails.</returns>
        [BlockAction]
        public BlockActionResult ReloadConversations()
        {
            var responseListingStatusBag = LoadResponseListing( null );

            if ( responseListingStatusBag.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( responseListingStatusBag.ErrorMessage );
            }

            return ActionOk( responseListingStatusBag.Conversations );
        }

        #endregion
    }
}
