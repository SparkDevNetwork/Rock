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
using Rock.Communication;
using Rock.Core.NotificationMessageTypes;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Displays entity notes to the user and allows adding new notes.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Notes" )]
    [Category( "Mobile > Core" )]
    [Description( "Displays entity notes to the user and allows adding new notes." )]
    [IconCssClass( "fa fa-sticky-note" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [EntityTypeField( "Entity Type",
        Description = "The type of entity",
        IsRequired = false,
        Key = AttributeKey.ContextEntityType,
        Order = 0 )]

    [NoteTypeField( "Note Types",
        Description = "Optional list of note types to limit display to",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.NoteTypes )]

    [FileField( Rock.SystemGuid.BinaryFiletype.DEFAULT,
        "Default Note Image",
        Description = "This image is displayed next to the note if the author has no profile image.",
        IsRequired = false,
        Key = AttributeKey.DefaultNoteImage,
        Order = 2,
        FieldTypeClass = "Rock.Field.Types.ImageFieldType" )]

    [BooleanField( "Use Template",
        Description = "If enabled, notes will be displayed using the 'Notes Template', allowing you full customization of the layout.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.NoteDisplayMode,
        Order = 3 )]

    [BooleanField( "Enable Group Notification",
        Description = "If a Group is available through page context, this will send a communication to every person in a group (using the Member 'CommunicationPreference', and the 'GroupNotificationCommunicationTemplate'), when a Note is added.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.EnableGroupNotification,
        Order = 4 )]

    [CommunicationTemplateField( "Group Notification Communication Template",
        Description = "The template to use to send the communication. Note will be passed as an additional merge field.",
        IsRequired = false,
        Key = AttributeKey.GroupNotificationCommunicationTemplate,
        Order = 5 )]

    [BooleanField( "Use Template",
        Description = "If enabled, notes will be displayed using the 'Notes Template', allowing you full customization of the layout.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.NoteDisplayMode,
        Order = 6 )]

    [BlockTemplateField( "Notes Template",
        Description = "The template to use when rendering the notes. Provided with a 'Notes' merge field, among some others (see documentation).",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_NOTES,
        DefaultValue = "C9134085-D433-444D-9803-8E5CE1B053DE",
        IsRequired = true,
        Key = AttributeKey.NotesTemplate,
        Order = 7 )]

    [LinkedPage(
        "Note List Page",
        Description = "Page to link to when user taps on the 'See All' button (in template mode). Should link to a page containing a fullscreen note block.",
        IsRequired = false,
        Key = AttributeKey.ListPage,
        Order = 8 )]

    [BooleanField( "Show Is Alert Toggle",
        Description = "If enabled, a person will have the option of toggling whether their note is 'Alert' or not.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowIsAlert,
        Order = 9 )]

    [BooleanField( "Show Is Private Toggle",
        Description = "If enabled, a person will have the option of toggling whether their note is a 'Private' note or not.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowIsPrivate,
        Order = 9 )]

    [BooleanField( "Use Template",
        Description = "If enabled, notes will be displayed using the 'Notes Template', allowing you full customization of the layout.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.NoteDisplayMode,
        Order = 10 )]

    [IntegerField( "Page Load Size",
        Description = "Determines the amount of notes to show in the initial page load. In template mode, this is the amount of notes your 'Notes' merge field will be limited to.",
        IsRequired = true,
        DefaultIntegerValue = 6,
        Key = AttributeKey.PageLoadSize,
        Order = 11 )]

    #endregion

    [ContextAware]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_NOTES_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "5B337D89-A298-4620-A0BE-078A41BC054B" )]
    public class Notes : RockBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileContent block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The context entity type key
            /// </summary>
            public const string ContextEntityType = "ContextEntityType";

            /// <summary>
            /// The note types key
            /// </summary>
            public const string NoteTypes = "NoteTypes";

            /// <summary>
            /// This image is displayed next to the note if the author has no profile image.
            /// </summary>
            public const string DefaultNoteImage = "DefaultNoteImage";

            /// <summary>
            /// The mode in which we should display these notes.
            /// </summary>
            public const string NoteDisplayMode = "NoteDisplayMode";

            /// <summary>
            /// The mode in which we should display these notes.
            /// </summary>
            public const string NotesTemplate = "NoteTemplate";

            /// <summary>
            /// The mode in which we should display these notes.
            /// </summary>
            public const string PageLoadSize = "PageLoadSize";

            /// <summary>
            /// The mode in which we should display these notes.
            /// </summary>
            public const string ListPage = "DetailPage";

            /// <summary>
            /// The enable group notification attribute key.
            /// </summary>
            public const string EnableGroupNotification = "EnableGroupNotification";

            /// <summary>
            /// The group notification communication template attribute key.
            /// </summary>
            public const string GroupNotificationCommunicationTemplate = "GroupNotificationCommunicationTemplate";

            /// <summary>
            /// The show is alert attribute key. 
            /// </summary>
            public const string ShowIsAlert = "ShowIsAlert";

            /// <summary>
            /// The show is private attribute key.
            /// </summary>
            public const string ShowIsPrivate = "ShowIsPrivate";
        }

        /// <summary>
        /// Gets the type of the context entity.
        /// </summary>
        /// <value>
        /// The type of the context entity.
        /// </value>
        protected string ContextEntityType => GetAttributeValue( AttributeKey.ContextEntityType );

        /// <summary>
        /// Gets the note type unique identifiers selected in the block configuration.
        /// </summary>
        /// <value>
        /// The note type unique identifiers selected in the block configuration.
        /// </value>
        protected ICollection<Guid> NoteTypes => GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the default note image to display next to the note if the author has no profile image.
        /// </summary>
        /// <value>
        /// The default note image to display next to the note if the author has no profile image.
        /// </value>
        protected Guid? DefaultNoteImage => GetAttributeValue( AttributeKey.DefaultNoteImage ).AsGuidOrNull();

        /// <summary>
        /// Gets whether or not to use the template for the notes block.
        /// </summary>
        /// <value>
        /// The value that indicates whether or not we should use the template.
        /// </value>
        protected bool UseTemplate => GetAttributeValue( AttributeKey.NoteDisplayMode ).AsBoolean();

        /// <summary>
        /// The template to use if <see cref="UseTemplate" /> is enabled.
        /// </summary>
        /// <value>
        /// The XAML template to parse on the shell.
        /// </value>
        protected string NotesTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.NotesTemplate ) );

        /// <summary>
        /// When in template mode, this is the amount of notes retrieved, when in List mode, this
        /// is used to indicate the amount of notes you start with. When in list mode, this value has
        /// a minimum of 12 (no matter what is set) on the shell.
        /// </summary>
        protected int PageLoadSize => GetAttributeValue( AttributeKey.PageLoadSize ).AsInteger();

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? ListPageGuid => GetAttributeValue( AttributeKey.ListPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether or not to enable group notification when a note is added.
        /// </summary>
        /// <value><c>true</c> if enable group notification; otherwise, <c>false</c>.</value>
        protected bool EnableGroupNotification => GetAttributeValue( AttributeKey.EnableGroupNotification ).AsBoolean();

        /// <summary>
        /// Gets the group notification communication template.
        /// </summary>
        /// <value>The group notification communication template.</value>
        protected Guid? GroupNotificationCommunicationTemplate => GetAttributeValue( AttributeKey.GroupNotificationCommunicationTemplate ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether to show the is alert toggle.
        /// </summary>
        /// <value><c>true</c> if [show is alert]; otherwise, <c>false</c>.</value>
        protected bool ShowIsAlert => GetAttributeValue( AttributeKey.ShowIsAlert ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the is private toggle.
        /// </summary>
        /// <value><c>true</c> if [show is private]; otherwise, <c>false</c>.</value>
        protected bool ShowIsPrivate => GetAttributeValue( AttributeKey.ShowIsPrivate ).AsBoolean();


        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 2 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            string defaultNoteImageUrl = null;

            if ( DefaultNoteImage.HasValue )
            {
                // Can't use defaultNoteImageFile.Url because it will build the path
                // relative to the current request, which won't always work with mobile
                // applications. So force it to use the PublicApplicationRoot.
                using ( var rockContext = new RockContext() )
                {
                    var defaultNoteImageFile = new BinaryFileService( rockContext ).Get( DefaultNoteImage.Value );

                    if ( defaultNoteImageFile != null )
                    {
                        var url = defaultNoteImageFile.Path;
                        url = url.StartsWith( "~" ) ? System.Web.VirtualPathUtility.ToAbsolute( url ) : url;
                        if ( !url.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                        {
                            var uri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
                            if ( uri != null )
                            {
                                url = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + url;
                            }

                            defaultNoteImageUrl = url;
                        }
                    }
                }
            }

            return new
            {
                DefaultNoteImageUrl = defaultNoteImageUrl,
                UseTemplate = UseTemplate,
                PageLoadSize = PageLoadSize,
                ShowIsAlert = ShowIsAlert,
                ShowIsPrivate = ShowIsPrivate
            };
        }

        #endregion

        /// <summary>
        /// Gets the viewable note types.
        /// </summary>
        /// <returns></returns>
        private ICollection<NoteTypeCache> GetViewableNoteTypes()
        {
            var contextEntityType = EntityTypeCache.Get( ContextEntityType );

            if ( contextEntityType == null )
            {
                return new NoteTypeCache[0];
            }

            return NoteTypeCache.GetByEntity( contextEntityType.Id, string.Empty, string.Empty, true )
                .Where( a => !NoteTypes.Any() || NoteTypes.Contains( a.Guid ) )
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the editable note types.
        /// </summary>
        /// <returns></returns>
        private ICollection<NoteTypeCache> GetEditableNoteTypes()
        {
            var contextEntityType = EntityTypeCache.Get( ContextEntityType );

            if ( contextEntityType == null )
            {
                return new NoteTypeCache[0];
            }

            return NoteTypeCache.GetByEntity( contextEntityType.Id, string.Empty, string.Empty, true )
                .Where( a => !NoteTypes.Any() || NoteTypes.Contains( a.Guid ) )
                .Where( a => a.UserSelectable )
                .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the note object that will be sent to the shell.
        /// </summary>
        /// <param name="note">The database note.</param>
        /// <returns>The note object that the shell understands.</returns>
        private object GetNoteObject( Note note )
        {
            var canEdit = note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var canReply = note.NoteType.AllowsReplies;

            // If the note type specifies the max reply depth then calculate and check.
            if ( canReply && note.NoteType.MaxReplyDepth.HasValue )
            {
                int replyDepth = 0;
                for ( var noteParent = note; noteParent != null; noteParent = noteParent.ParentNote )
                {
                    replyDepth += 1;
                }

                canReply = replyDepth < note.NoteType.MaxReplyDepth.Value;
            }

            string photoUrl = "";
            if( note.CreatedByPersonAlias?.Person?.PhotoUrl != null )
            {
                photoUrl = MobileHelper.BuildPublicApplicationRootUrl( note.CreatedByPersonAlias.Person.PhotoUrl );
            }

            return new
            {
                note.Guid,
                NoteTypeGuid = note.NoteType.Guid,
                note.Text,
                PhotoUrl = photoUrl,
                Name = note.CreatedByPersonName,
                Date = note.CreatedDateTime.HasValue ? ( DateTimeOffset? ) new DateTimeOffset( note.CreatedDateTime.Value ) : null,
                ReplyCount = note.ChildNotes.Count( b => b.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ),
                note.IsAlert,
                IsPrivate = note.IsPrivateNote,
                CanEdit = canEdit,
                CanDelete = canEdit,
                CanReply = canReply,
            };
        }

        /// <summary>
        /// Gets the viewable notes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns>List&lt;Note&gt;.</returns>
        private List<Note> GetViewableNotes( RockContext rockContext, Guid? parentNoteGuid, int startIndex, int count )
        {
            var noteService = new NoteService( rockContext );
            var viewableNoteTypeIds = GetViewableNoteTypes().Select( t => t.Id ).ToList();

            var entityType = EntityTypeCache.Get( ContextEntityType );
            var entity = entityType != null ? RequestContext.GetContextEntity( entityType.GetEntityType() ) : null;
            if ( entity == null )
            {
                // Indicate to caller "not found" error.
                return null;
            }

            var notesQuery = noteService.Queryable()
                .AsNoTracking()
                .Include( a => a.CreatedByPersonAlias.Person )
                .Include( a => a.ParentNote )
                .Include( a => a.ChildNotes )
                .Where( a => viewableNoteTypeIds.Contains( a.NoteTypeId ) )
                .Where( a => a.EntityId == entity.Id );

            if ( parentNoteGuid.HasValue )
            {
                notesQuery = notesQuery.Where( a => a.ParentNote.Guid == parentNoteGuid.Value );
            }
            else
            {
                notesQuery = notesQuery.Where( a => !a.ParentNoteId.HasValue );
            }

            return notesQuery
                .OrderByDescending( a => a.IsAlert == true )
                .ThenByDescending( a => a.CreatedDateTime )
                .ToList()
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Skip( startIndex )
                .Take( count )
                .ToList();
        }

        /// <summary>
        /// Gets the notes for the entity.
        /// </summary>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <returns>The list of notes found.</returns>
        private List<object> GetEntityNotes( Guid? parentNoteGuid, int startIndex, int count )
        {
            using ( var rockContext = new RockContext() )
            {
                var viewableNotes = GetViewableNotes( rockContext, parentNoteGuid, startIndex, count );

                if ( viewableNotes == null )
                {
                    return null;
                }

                var noteData = viewableNotes
                    .Select( a => GetNoteObject( a ) )
                    .ToList();

                return noteData;
            }
        }

        /// <summary>
        /// Sends the note added communication to group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="noteText">The note text.</param>
        private void SendNoteAddedCommunicationToGroup( Group group, string noteText )
        {
            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var communicationTemplateService = new CommunicationTemplateService( rockContext );

                // Create a new communication.
                var communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Approved;
                communication.ReviewedDateTime = RockDateTime.Now;
                communication.ReviewerPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
                communication.SenderPersonAliasId = RequestContext.CurrentPerson.PrimaryAliasId;
                communication.CommunicationType = CommunicationType.RecipientPreference;
                communication.IsBulkCommunication = true;

                // Setting the communication template that was provided in the block configuration.
                var communicationTemplate = communicationTemplateService.Get( GroupNotificationCommunicationTemplate.Value );
                communication.CommunicationTemplateId = communicationTemplate.Id;

                // Copy all communication details from the Template to CommunicationData.
                CommunicationDetails.Copy( communicationTemplate, communication );

                communicationService.Add( communication );
                rockContext.SaveChanges();

                // The group members are the message recipients
                var communicationRecipientBags = new GroupMemberService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( m =>
                        m.GroupId == group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.Person != null
                     )
                    .Select( m => new
                    {
                        m.Person,
                        GroupCommunicationPreference = m.CommunicationPreference,
                        PersonCommunicationPreference = m.Person.CommunicationPreference
                    } )
                    .Distinct()
                    .ToList();

                // Let's go through and create our actual Communication Recipients from that list.
                foreach ( var recipientBag in communicationRecipientBags )
                {
                    var emailMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;
                    var smsMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;
                    var pushMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ).Id;

                    var mediumTypeId = Rock.Model.Communication.DetermineMediumEntityTypeId(
                        emailMediumEntityTypeId,
                        smsMediumEntityTypeId,
                        pushMediumEntityTypeId,
                        recipientBag.GroupCommunicationPreference,
                        recipientBag.PersonCommunicationPreference );

                    var recipient = new CommunicationRecipient
                    {
                        PersonAliasId = recipientBag.Person.PrimaryAliasId,
                        MediumEntityTypeId = mediumTypeId,
                        AdditionalMergeValues = new Dictionary<string, object>
                        {
                            ["Note"] = noteText
                        }
                    };

                    // Check for duplicate recipients before adding.
                    if ( !communication.Recipients.Any( r => r.PersonAliasId == recipientBag.Person.PrimaryAliasId ) )
                    {
                        communication.Recipients.Add( recipient );
                    }
                }

                rockContext.SaveChanges();

                // Send off the communication.
                Task.Run( () =>
                {
                    var transactionMsg = new ProcessSendCommunication.Message()
                    {
                        CommunicationId = communication.Id
                    };
                    transactionMsg.Send();
                } );
            }

        }

        #region Action Methods

        /// <summary>
        /// Gets the initial data that will tell the note block how to function.
        /// </summary>
        /// <param name="count">The number of initial notes to load.</param>
        /// <returns>A response that contains the initial shell block data.</returns>
        [BlockAction]
        public BlockActionResult GetInitialData( int count )
        {
            var notes = GetEntityNotes( null, 0, count );

            if ( notes == null )
            {
                return ActionNotFound();
            }

            var editableNoteTypes = GetEditableNoteTypes()
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    a.UserSelectable,
                    IsMentionEnabled = a.FormatType != Enums.Core.NoteFormatType.Unstructured && a.IsMentionEnabled
                } );

            return ActionOk( new
            {
                EditableNoteTypes = editableNoteTypes,
                Notes = notes
            } );
        }

        /// <summary>
        /// Gets a subset of the notes to be displayed.
        /// </summary>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of notes to return.</param>
        /// <returns>A response that contains the requested notes or an error.</returns>
        [BlockAction]
        public BlockActionResult GetNotes( Guid? parentNoteGuid, int startIndex, int count )
        {
            var notes = GetEntityNotes( parentNoteGuid, startIndex, count );

            if ( notes == null )
            {
                return ActionNotFound();
            }

            return ActionOk( notes );
        }

        /// <summary>
        /// Gets a singular note.
        /// </summary>
        /// <param name="noteGuid">The note unique identifier.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetNote( Guid noteGuid )
        {
            using ( var rockContext = new RockContext() )
            {

                if ( noteGuid == null )
                {
                    return ActionBadRequest();
                }

                var note = new NoteService( rockContext ).Get( noteGuid );

                if ( note == null )
                {
                    return ActionNotFound();
                }

                return ActionOk( new Rock.Common.Mobile.Blocks.Core.Notes.Note
                {
                    Guid = note.Guid,
                    Text = note.Text,
                    NoteTypeName = note.NoteType.Name,
                    NoteTypeGuid = note.NoteType.Guid,
                    IsAlert = note.IsAlert ?? false,
                    IsPrivate = note.IsPrivateNote,
                    Date = note.CreatedDateTime.HasValue ? ( DateTimeOffset? ) new DateTimeOffset( note.CreatedDateTime.Value ) : null
                } );
            }
        }

        /// <summary>
        /// Saves the note.
        /// </summary>
        /// <param name="noteGuid">The note unique identifier.</param>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <param name="noteTypeGuid">The note type unique identifier.</param>
        /// <param name="text">The text of the note.</param>
        /// <param name="isAlert">if set to <c>true</c> the note is an alert.</param>
        /// <param name="isPrivate">if set to <c>true</c> the note is private.</param>
        /// <returns>A response that contains the saved note data or an error.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( Guid? noteGuid, Guid? parentNoteGuid, Guid noteTypeGuid, string text, bool isAlert, bool isPrivate )
        {
            var noteType = NoteTypeCache.Get( noteTypeGuid );

            if ( noteType == null )
            {
                return ActionBadRequest( "Invalid note type." );
            }

            var entityType = EntityTypeCache.Get( ContextEntityType );
            var entity = entityType != null ? RequestContext.GetContextEntity( entityType.GetEntityType() ) : null;
            if ( entity == null )
            {
                return ActionBadRequest( "Unknown note type." );
            }

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                Note note;

                var parentNote = parentNoteGuid.HasValue ? noteService.Get( parentNoteGuid.Value ) : null;

                // If a note guid was not supplied, we're creating a new one.
                var newNote = !noteGuid.HasValue;
                if ( newNote )
                {
                    if ( !noteType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to add note." );
                    }

                    note = rockContext.Set<Note>().Create();
                    note.IsSystem = false;
                    note.EntityId = entity.Id;
                    note.ParentNoteId = parentNote?.Id;

                    noteService.Add( note );
                }
                // Otherwise, retrieve the existing note for modification.
                else
                {
                    note = noteService.Get( noteGuid.Value );

                    if ( note == null )
                    {
                        return ActionNotFound();
                    }

                    if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to edit note." );
                    }
                }

                // If the note is new or is owned by the current person then
                // update the private flag.
                if ( note.Id == 0 || ( note.CreatedByPersonId.HasValue && RequestContext.CurrentPerson?.Id == note.CreatedByPersonId ) )
                {
                    note.IsPrivateNote = isPrivate;
                }

                // It's up to the client to handle logic for non-user selectable
                // note types.
                note.NoteTypeId = noteType.Id;

                string personalNoteCaption = "You - Personal Note";
                if ( string.IsNullOrWhiteSpace( note.Caption ) )
                {
                    note.Caption = note.IsPrivateNote ? personalNoteCaption : string.Empty;
                }
                else
                {
                    // if the note still has the personalNoteCaption, but was changed to have IsPrivateNote to false, change the caption to empty string
                    if ( note.Caption == personalNoteCaption && !note.IsPrivateNote )
                    {
                        note.Caption = string.Empty;
                    }
                }

                var mentionedPersonIds = noteType.FormatType != NoteFormatType.Unstructured && noteType.IsMentionEnabled
                    ? noteService.GetNewPersonIdsMentionedInContent( text, note.Text )
                    : new List<int>();

                note.Text = text;
                note.IsAlert = isAlert;

                note.EditedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                note.EditedDateTime = RockDateTime.Now;

#pragma warning disable CS0618 // Type or member is obsolete
                // Set this so anything doing direct SQL queries will still find
                // the right set of notes.
                note.ApprovalStatus = NoteApprovalStatus.Approved;
#pragma warning restore CS0618 // Type or member is obsolete

                rockContext.SaveChanges();

                // If we created a new note (and the feature is enabled), we want to send a communication
                // to the Group provided through context, if there is one. 
                if ( newNote && EnableGroupNotification && GroupNotificationCommunicationTemplate.HasValue )
                {
                    // If there is a Group context, send the communication. Even in the cases where the note entity type is Group.
                    if ( RequestContext.ContextEntities.TryGetValue( typeof( Group ), out var contextGroupEntity ) )
                    {
                        Task.Run( () =>
                        {
                            SendNoteAddedCommunicationToGroup( contextGroupEntity.Value as Group, text );
                        } );
                    }
                }

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

                return ActionOk( GetNoteObject( note ) );
            }
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="noteGuid">The note unique identifier.</param>
        /// <returns>A response that contains either an error or informs the client the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( Guid noteGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new NoteService( rockContext );
                var note = service.Get( noteGuid );

                if ( note == null )
                {
                    return ActionNotFound();
                }

                if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    // Rock.Constants strings include HTML so don't use.
                    return ActionForbidden( "You are not authorized to delete this note." );
                }

                if ( service.CanDeleteChildNotes( note, RequestContext.CurrentPerson, out var errorMessage ) && service.CanDelete( note, out errorMessage ) )
                {
                    service.Delete( note, true );
                    rockContext.SaveChanges();

                    return ActionOk();
                }
                else
                {
                    return ActionForbidden( errorMessage );
                }
            }
        }

        /// <summary>
        /// Gets the notes template.
        /// </summary>
        /// <param name="parentNoteGuid">The parent note unique identifier.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetNotesTemplate( Guid? parentNoteGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var notes = GetEntityNotes( parentNoteGuid, 0, PageLoadSize );

                if ( notes == null )
                {
                    return ActionNotFound();
                }

                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "Notes", notes );
                mergeFields.AddOrReplace( "ListPage", ListPageGuid );
                var content = NotesTemplate.ResolveMergeFields( mergeFields );

                var editableNoteTypes = GetEditableNoteTypes()
                    .Select( a => new
                    {
                        a.Guid,
                        a.Name,
                        a.UserSelectable,
                        IsMentionEnabled = a.FormatType != Enums.Core.NoteFormatType.Unstructured && a.IsMentionEnabled
                    } );

                return ActionOk( new
                {
                    Content = content,
                    EditableNoteTypes = editableNoteTypes,
                } );
            }
        }

        #endregion
    }
}
