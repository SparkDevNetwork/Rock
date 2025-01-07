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

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Core.MyNotes;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Allows the currently logged in individual to view notes created by them with the ability to manage associations and link unassociated notes.
    /// </summary>

    [DisplayName( "My Notes" )]
    [Category( "Mobile > Core" )]
    [Description( "View notes created by you with the ability to manage associations and link unassociated notes." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Note Item Template",
        Description = "The item template to use when rendering the notes.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_NOTES,
        DefaultValue = "421F2759-B6B6-4C47-AA42-320B6DB9F0A7",
        IsRequired = true,
        Key = AttributeKey.NoteItemTemplate,
        Order = 0 )]

    [BooleanField( "Enable Swipe for Options",
        Description = "When enabled, swipe actions will be available for each note.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.EnableSwipeForOptions,
        Order = 1 )]

    [NoteTypeField( "Person Note Types",
        Description = "The note types to allow selecting from when linking to a person.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 2,
        EntityTypeName = "Rock.Model.Person",
        Key = AttributeKey.PersonNoteTypes )]

    [NoteTypeField( "Reminder Note Type",
        Description = "The note type to link when creating a reminder.",
        AllowMultiple = false,
        IsRequired = false,
        Order = 3,
        EntityTypeName = "Rock.Model.Reminder",
        Key = AttributeKey.ReminderNoteType )]

    [NoteTypeField( "Connection Note Type",
        Description = "The note type to link when creating a connection.",
        AllowMultiple = false,
        IsRequired = false,
        Order = 4,
        EntityTypeName = "Rock.Model.ConnectionRequest",
        Key = AttributeKey.ConnectionNoteType )]

    [LinkedPage( "Person Profile Detail Page",
        Description = "The page to link to view a person profile when a note is associated to a person.",
        IsRequired = false,
        Key = AttributeKey.PersonProfilePage,
        Order = 5 )]

    [LinkedPage( "Reminder Detail Page",
        Description = "The page to link to view a reminder when a note is associated to a reminder.",
        IsRequired = false,
        Key = AttributeKey.ReminderDetailPage,
        Order = 6 )]

    [LinkedPage( "Add Connection Page",
        Description = "The page to link to add a connection that will be associated with the note.",
        IsRequired = false,
        Key = AttributeKey.AddConnectionPage,
        Order = 7 )]

    [LinkedPage( "Connection Detail Page",
        Description = "The page to link to view a connection.",
        IsRequired = false,
        Key = AttributeKey.ConnectionDetailPage,
        Order = 8 )]

    [BooleanField( "Group Notes by Date",
        Description = "When enabled, notes will be grouped by date.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.GroupNotesByDate,
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_MY_NOTES_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CORE_MY_NOTES )]
    public class MyNotes : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The attribute keys for the block.
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The note item template key.
            /// </summary>
            public const string NoteItemTemplate = "NoteItemTemplate";

            /// <summary>
            /// Enable swipe for options key.
            /// </summary>
            public const string EnableSwipeForOptions = "EnableSwipeForOptions";

            /// <summary>
            /// The note types key.
            /// </summary>
            public const string PersonNoteTypes = "PersonNoteTypes";

            /// <summary>
            /// The reminder note type key.
            /// </summary>
            public const string ReminderNoteType = "ReminderNoteType";

            /// <summary>
            /// The connection note type key.
            /// </summary>
            public const string ConnectionNoteType = "ConnectionNoteType";

            /// <summary>
            /// The person profile page key.
            /// </summary>
            public const string PersonProfilePage = "PersonProfilePage";

            /// <summary>
            /// The reminder detail page key.
            /// </summary>
            public const string ReminderDetailPage = "ReminderDetailPage";

            /// <summary>
            /// The add connection page key.
            /// </summary>
            public const string AddConnectionPage = "AddConnectionPage";

            /// <summary>
            /// The view connection page key.
            /// </summary>
            public const string ConnectionDetailPage = "ConnectionDetailPage";

            /// <summary>
            /// Whether or not to group notes by date.
            /// </summary>
            public const string GroupNotesByDate = "GroupNotesByDate";
        }

        #endregion

        #region Properties

        /// <summary>
        /// The template to use for each item.
        /// </summary>
        /// <value>
        /// The XAML template to parse on the shell.
        /// </value>
        protected string NoteItemTemplate => Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.NoteItemTemplate ) );

        /// <summary>
        /// Whether or not to enable swipe for options.
        /// </summary>
        protected bool EnableSwipeForOptions => GetAttributeValue( AttributeKey.EnableSwipeForOptions ).AsBoolean();

        /// <summary>
        /// Gets the note type unique identifiers selected in the block configuration.
        /// </summary>
        protected ICollection<Guid> PersonNoteTypes => GetAttributeValue( AttributeKey.PersonNoteTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the note type to use for adding a reminder.
        /// </summary>
        protected Guid? ReminderNoteTypeGuid => GetAttributeValue( AttributeKey.ReminderNoteType ).AsGuidOrNull();

        /// <summary>
        /// Gets the note type to use for adding a connection.
        /// </summary>
        protected Guid? ConnectionNoteTypeGuid => GetAttributeValue( AttributeKey.ConnectionNoteType ).AsGuidOrNull();

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? PersonProfilePageGuid => GetAttributeValue( AttributeKey.PersonProfilePage ).AsGuidOrNull();

        /// <summary>
        /// Gets the add connection page unique identifier.
        /// </summary>
        protected Guid? AddConnectionPageGuid => GetAttributeValue( AttributeKey.AddConnectionPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the view connection page unique identifier.
        /// </summary>
        protected Guid? ViewConnectionPageGuid => GetAttributeValue( AttributeKey.ConnectionDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the reminder detail page unique identifier.
        /// </summary>
        protected Guid? ReminderDetailPageGuid => GetAttributeValue( AttributeKey.ReminderDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Whether or not to group notes by date.
        /// </summary>
        protected bool GroupNotesByDate => GetAttributeValue( AttributeKey.GroupNotesByDate ).AsBoolean();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Core.MyNotes.Configuration
            {
                GroupNotesByDate = GroupNotesByDate,
                EnableSwipeForOptions = EnableSwipeForOptions,
                AddConnectionPageGuid = AddConnectionPageGuid,
                AddReminderPageGuid = ReminderDetailPageGuid,
                ReminderNoteTypeGuid = ReminderNoteTypeGuid,
                ConnectionNoteTypeGuid = ConnectionNoteTypeGuid,
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the note types that can be used when linking a note to a person.
        /// </summary>
        /// <returns></returns>
        private List<NoteTypeCache> GetLinkToPersonNoteTypes()
        {
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;

            return NoteTypeCache.GetByEntity( personEntityTypeId, string.Empty, string.Empty, false )
                .Where( a => ( PersonNoteTypes == null || !PersonNoteTypes.Any() ) || PersonNoteTypes.Contains( a.Guid ) )
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the item templates for each note item.
        /// </summary>
        /// <returns></returns>
        private void PopulateNoteItemsInformation( List<NoteItemBag> notes )
        {
            // Load our common note entity types.
            var personEntityType = EntityTypeCache.Get<Person>();
            var personAliasEntityType = EntityTypeCache.Get<PersonAlias>();
            var reminderEntityType = EntityTypeCache.Get<Reminder>();
            var connectionRequestEntityType = EntityTypeCache.Get<ConnectionRequest>();

            // Load our services for each entity type.
            var personService = new PersonService( RockContext );
            var reminderTypeService = new ReminderTypeService( RockContext );
            var reminderService = new ReminderService( RockContext );
            var connectionService = new ConnectionRequestService( RockContext );
            var personAliasService = new PersonAliasService( RockContext );

            // Group all of the notes by their entity type.
            // This will allow us to load the additional entity information
            // for each note, depending on the entity type.
            var noteGroups = notes.GroupBy( n => n.NoteTypeEntityTypeId );
            var personList = new List<Person>();
            var personAliasList = new List<PersonAlias>();
            var reminderList = new List<Reminder>();
            var connectionRequestList = new List<ConnectionRequest>();

            foreach ( var grp in noteGroups )
            {
                // Extract the EntityId values into a simple list.
                var entityIds = grp.Select( n => n.EntityId ).ToList();

                // If note type is person
                if ( grp.Key == personEntityType.Id )
                {
                    personList = personService.Queryable()
                        .Where( p => entityIds.Contains( p.Id ) ).ToList();
                }
                // Reminder
                else if ( grp.Key == reminderEntityType.Id )
                {
                    reminderList = reminderService
                        .Queryable()
                        .Where( r => entityIds.Contains( r.Id ) ).ToList();

                    // Reminders are typically associated with a person alias.
                    foreach ( var reminder in reminderList )
                    {
                        if ( reminder.ReminderType.EntityTypeId == personAliasEntityType.Id )
                        {
                            var personAlias = personAliasService.Get( reminder.EntityId );

                            if ( personAlias != null )
                            {
                                personAliasList.Add( personAlias );
                            }
                        }
                    }
                }
                else if ( grp.Key == connectionRequestEntityType.Id )
                {
                    connectionRequestList = connectionService
                        .Queryable()
                        .Where( r => entityIds.Contains( r.Id ) ).ToList();
                }
            }

            // Loop through each note and populate the note items with the
            // additional entity type specific information, as well as the standard
            // note information (such as Template).
            foreach ( var note in notes )
            {
                var noteTypeEntityTypeId = NoteTypeCache.Get( note.NoteTypeId )?.EntityTypeId;
                if ( noteTypeEntityTypeId.HasValue )
                {
                    if ( note.EntityId.HasValue )
                    {
                        var entity = new EntityTypeService( RockContext ).GetEntity( noteTypeEntityTypeId.Value, note.EntityId.Value );
                        note.EntityName = entity?.ToString();
                        note.EntityGuid = entity?.Guid;
                    }

                    var entityType = EntityTypeCache.Get( noteTypeEntityTypeId.Value );
                    note.EntityTypeName = entityType?.FriendlyName;
                    note.NoteTypeEntityTypeGuid = entityType?.Guid;
                }

                if ( note.NoteTypeEntityTypeId == personEntityType.Id && note.EntityId.HasValue )
                {
                    var person = personList.FirstOrDefault( p => p.Id == note.EntityId.Value );

                    if ( person?.PhotoUrl.IsNotNullOrWhiteSpace() == true )
                    {
                        note.PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( person.PhotoUrl );
                    }
                }
                else if ( note.NoteTypeEntityTypeId == reminderEntityType.Id )
                {
                    var reminder = reminderList.FirstOrDefault( r => r.Id == note.EntityId.Value );

                    if ( reminder.ReminderType.EntityTypeId == personAliasEntityType.Id )
                    {
                        var personAlias = personAliasList.FirstOrDefault( p => p.Id == reminder.EntityId );
                        note.EntityName = personAlias?.Person.FullName;
                    }
                    else
                    {
                        var entity = new EntityTypeService( RockContext ).GetEntity( reminderEntityType.Id, reminder.EntityId );
                        note.EntityName = entity?.ToString();
                    }
                }
                else if ( note.NoteTypeEntityTypeId == connectionRequestEntityType.Id )
                {
                    var connectionRequest = connectionRequestList.FirstOrDefault( r => r.Id == note.EntityId.Value );

                    if ( connectionRequest != null )
                    {
                        note.EntityName = connectionRequest.PersonAlias.Person.FullName;
                    }
                }

                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "Note", new Lava.LavaDataWrapper( note ) );
                mergeFields.Add( "PersonEntityTypeId", personEntityType.Id );
                mergeFields.Add( "PersonAliasEntityTypeId", personAliasEntityType.Id );
                mergeFields.Add( "PersonAliasEntityTypeGuid", personAliasEntityType.Guid );
                mergeFields.Add( "ReminderEntityTypeId", reminderEntityType.Id );
                mergeFields.Add( "ReminderNoteTypeGuid", ReminderNoteTypeGuid );
                mergeFields.Add( "ConnectionNoteTypeGuid", ConnectionNoteTypeGuid );
                mergeFields.Add( "ConnectionEntityTypeId", connectionRequestEntityType.Id );
                mergeFields.Add( "PersonDetailPage", PersonProfilePageGuid );
                mergeFields.Add( "ReminderDetailPage", ReminderDetailPageGuid );
                mergeFields.Add( "ConnectionDetailPage", ViewConnectionPageGuid );
                mergeFields.Add( "AddConnectionPage", AddConnectionPageGuid );
                mergeFields.Add( "GroupNotesByDate", GroupNotesByDate );

                note.Template = NoteItemTemplate.ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Gets the notes created by the specified person.
        /// </summary>
        /// <param name="personGuid">The person who created the notes.</param>
        /// <param name="beforeDate">Load notes after the specified date, not inclusive.</param>
        /// <param name="index">The index of the notes to load.</param>
        /// <param name="filter">The filter options to use for the notes.</param>
        /// <param name="count">The number of notes to load.</param>
        /// <returns>A list of the notes returned from the query and a flag indicating whether or not the person has more notes.</returns>
        private (List<NoteItemBag> Notes, bool HasMore) GetNotesCreatedByPerson( Guid personGuid, DateTime? beforeDate, int? index, FilterOptionsBag filter, int count )
        {
            var noteService = new NoteService( RockContext );

            // Load all of the notes created by the person.
            var notesQry = noteService.Queryable()
                .OrderByDescending( n => n.CreatedDateTime )
                .Where( n => n.CreatedDateTime.HasValue
                    && n.CreatedByPersonAlias.Person.Guid == personGuid )
                .Where( n => ( beforeDate.HasValue && n.CreatedDateTime < beforeDate ) || !beforeDate.HasValue );

            // Show only notes that have an associated entity.
            if ( filter.ShowLinkedNotes && !filter.ShowStandaloneNotes )
            {
                notesQry = notesQry.Where( n => n.EntityId.HasValue );
            }

            // Show only notes that do not have an associated entity.
            if ( !filter.ShowLinkedNotes && filter.ShowStandaloneNotes )
            {
                notesQry = notesQry.Where( n => !n.EntityId.HasValue );
            }

            // Show only notes that are of the specified note types.
            if ( filter.LimitToNoteTypes != null && filter.LimitToNoteTypes.Any() )
            {
                var noteTypeIds = filter.LimitToNoteTypes.Select( nt => NoteTypeCache.Get( nt )?.Id ).Where( nt => nt.HasValue ).Select( nt => nt.Value );
                notesQry = notesQry.Where( n => noteTypeIds.Contains( n.NoteTypeId ) );
            }

            // Limit notes to a custom date range.
            if ( filter.UseCustomDateRange )
            {
                notesQry = notesQry.Where( n => n.CreatedDateTime >= filter.DateRangeStart && n.CreatedDateTime <= filter.DateRangeEnd );
            }

            // Limit notes to a specific number of days.
            else if ( filter.WithinDays > 0 )
            {
                var dateMinimum = DateTime.Today.AddDays( -filter.WithinDays );
                notesQry = notesQry.Where( n => n.CreatedDateTime >= dateMinimum );
            }

            // This really shouldn't be used in tandem with BeforeDate.
            if ( index.HasValue )
            {
                notesQry = notesQry.Skip( index.Value );
            }

            var notes = notesQry.OrderByDescending( n => n.CreatedDateTime )
                .Take( count )
                .Select( note => new NoteItemBag
                {
                    EntityId = note.EntityId,
                    NoteText = note.Text,
                    NoteTypeName = note.NoteType.Name,
                    NoteTypeColor = note.NoteType.Color,
                    NoteTypeGuid = note.NoteType.Guid,
                    CreatedDateTime = note.CreatedDateTime.Value,
                    NoteTypeId = note.NoteTypeId,
                    NoteTypeEntityTypeId = note.NoteType.EntityTypeId,
                    Guid = note.Guid,
                    IsPrivateNote = note.IsPrivateNote,
                    IsAlert = note.IsAlert ?? false,
                    Id = note.Id,
                } ).ToList();

            // If the person has not created any notes
            // we don't need to continue.
            if ( !notes.Any() )
            {
                return (new List<NoteItemBag>(), false);
            }

            // If there are less notes than the count requested we don't need to continue.
            if ( notes.Count < count )
            {
                return (notes, false);
            }

            // Filter out the oldest note in the list if we have a before date.
            // This is to ensure that we don't return a partial result set of notes
            // for a specific date.
            if ( beforeDate.HasValue )
            {
                // This is the last, also the "oldest" date returned from our query.
                // We want to remove all notes created on this date to prevent returning
                // some but not all notes of a day.
                var lastSeenDate = notes.Last().CreatedDateTime.Date;
                notes.RemoveAll( n => n.CreatedDateTime.Date == lastSeenDate );

                // This catches the case in which we have removed every note (we only had one date).
                // Re-run the query but double the amount of notes we load to ensure we have enough to take.
                if ( !notes.Any() )
                {
                    return GetNotesCreatedByPerson( personGuid, lastSeenDate, index, filter, count * 2 );
                }
            }

            return (notes, true);
        }

        /// <summary>
        /// Returns a list of note types that the person has left a note for.
        /// </summary>
        /// <returns></returns>
        private List<NoteTypeCache> GetNoteTypesWithNoteCreatedByPerson()
        {
            // We want to get a distinct list of all of the note type IDs that the user
            // has left a note for.
            var noteService = new NoteService( RockContext );

            var noteTypeIds = noteService.Queryable()
                .Where( n => n.CreatedByPersonAliasId != null && n.CreatedByPersonAliasId == RequestContext.CurrentPerson.PrimaryAliasId )
                .Select( n => n.NoteTypeId )
                .Distinct()
                .ToList();

            return NoteTypeCache.All()
                .Where( nt => noteTypeIds.Contains( nt.Id ) && nt.UserSelectable )
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Deletes the linked entity for the note.
        /// </summary>
        /// <param name="note">The note to delete the linked entity for.</param>
        /// <param name="result">The result from deleting the linked entity.</param>
        /// <returns>A <see cref="BlockActionResult"/> that describes if we successfully deleted the entity.</returns>
        private bool DeleteLinkedEntity( Rock.Model.Note note, out BlockActionResult result )
        {
            var reminderEntityType = EntityTypeCache.Get<Reminder>();
            var connectionRequestEntityType = EntityTypeCache.Get<ConnectionRequest>();

            // If we're trying to delete an entity on a note
            // that doesn't have one, this is an invalid request.
            if ( !note.EntityId.HasValue )
            {
                result = ActionBadRequest( "The note does not have a linked entity." );
                return false;
            }

            // We only support deleting reminders and connection requests as of today.
            if ( note.NoteType.EntityTypeId == reminderEntityType.Id )
            {
                var reminderService = new ReminderService( RockContext );
                var reminder = reminderService.Get( note.EntityId.Value );

                if ( !reminderService.CanDelete( reminder, out var errorMessage ) )
                {
                    result = ActionBadRequest( errorMessage );
                    return false;
                }

                if ( reminder != null )
                {
                    reminderService.Delete( reminder );
                }
            }
            else if ( note.NoteType.EntityTypeId == connectionRequestEntityType.Id )
            {
                var connectionService = new ConnectionRequestService( RockContext );
                var connection = connectionService.Get( note.EntityId.Value );

                if ( !connectionService.CanDelete( connection, out var errorMessage ) )
                {
                    result = ActionBadRequest( errorMessage );
                    return false;
                }

                if ( connection != null )
                {
                    connectionService.Delete( connection );
                }
            }
            else
            {
                result = ActionBadRequest( "The linked entity type is not supported to be deleted." );
                return false;
            }

            result = null;
            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial data for the block.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetInitialData( GetMyNotesRequestBag options )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }


            var notesBag = GetNotesCreatedByPerson( RequestContext.CurrentPerson.Guid, null, null, options.Filter, options.Count );
            PopulateNoteItemsInformation( notesBag.Notes );

            var viewableNoteTypes = GetNoteTypesWithNoteCreatedByPerson().Select( nt => new
            {
                Name = nt.Name,
                Guid = nt.Guid,
                UserSelectable = nt.UserSelectable,
                IsMentionEnabled = nt.IsMentionEnabled,
                EntityTypeId = nt.EntityTypeId,
            } );

            return ActionOk( new
            {
                Notes = notesBag.Notes,
                ViewableNoteTypes = viewableNoteTypes,
                LinkPersonNoteTypes = GetLinkToPersonNoteTypes().Select( nt => new ListItemBag
                {
                    Text = nt.Name,
                    Value = nt.Guid.ToString()
                } ).ToList(),
                HasMore = notesBag.HasMore
            } );
        }

        /// <summary>
        /// Gets the notes created by the current person.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetMyNotes( GetMyNotesRequestBag options )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                var notesBag = GetNotesCreatedByPerson( RequestContext.CurrentPerson.Guid, options.BeforeDate?.Date, options.Index, options.Filter, options.Count );
                PopulateNoteItemsInformation( notesBag.Notes );

                return ActionOk( new
                {
                    Notes = notesBag.Notes,
                    HasMore = notesBag.HasMore
                } );
            }
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A response that contains either an error or informs the client the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( DeleteNoteRequestBag options )
        {
            var service = new NoteService( RockContext );
            var note = service.Get( options.NoteGuid );

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
                if ( options.DeleteLinkedEntity && !DeleteLinkedEntity( note, out var result ) )
                {
                    return result;
                }

                service.Delete( note, true );
                RockContext.SaveChanges();
                return ActionOk();
            }
            else
            {
                return ActionForbidden( errorMessage );
            }
        }

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateNote( UpdateNoteRequestBag options )
        {
            var noteService = new NoteService( RockContext );
            var note = noteService.Get( options.NoteGuid );
            var noteType = NoteTypeCache.Get( options.NoteTypeGuid );

            if ( note == null || noteType == null )
            {
                return ActionNotFound();
            }

            if ( !note.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this note." );
            }

            note.Text = options.NoteText;
            note.IsAlert = options.IsAlert;
            note.IsPrivateNote = options.IsPrivate;
            note.NoteTypeId = noteType.Id;
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Will link a note to a person.
        /// </summary>
        /// <param name="options">The options for linking a note to a person.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult LinkToPerson( LinkToPersonRequestBag options )
        {
            var noteService = new NoteService( RockContext );
            var personService = new PersonService( RockContext );

            var note = noteService.Get( options.NoteGuid );
            var person = personService.Get( options.PersonGuid );
            var noteType = NoteTypeCache.Get( options.NoteTypeGuid );

            if ( note == null || person == null || noteType == null )
            {
                return ActionNotFound();
            }

            note.EntityId = person.Id;
            note.NoteTypeId = noteType.Id;
            note.IsAlert = options.IsAlert;
            note.IsPrivateNote = options.IsPrivate;
            note.IsPinned = options.PinToTop;
            RockContext.SaveChanges();

            return ActionOk();
        }


        /// <summary>
        /// Gets the note types that can be used when linking a note to a person.
        /// </summary>
        /// <param name="noteGuid">The note guid.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetSingleNote( Guid noteGuid )
        {
            var noteService = new NoteService( RockContext );
            var note = noteService.Get( noteGuid );

            if ( note == null )
            {
                return ActionNotFound();
            }

            if ( !note.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this note." );
            }

            var noteItem = new NoteItemBag
            {
                EntityId = note.EntityId,
                NoteText = note.Text,
                NoteTypeName = note.NoteType.Name,
                NoteTypeColor = note.NoteType.Color,
                NoteTypeGuid = note.NoteType.Guid,
                CreatedDateTime = note.CreatedDateTime.Value,
                NoteTypeId = note.NoteTypeId,
                NoteTypeEntityTypeId = note.NoteType.EntityTypeId,
                Guid = note.Guid,
                IsPrivateNote = note.IsPrivateNote,
                IsAlert = note.IsAlert ?? false,
                Id = note.Id,
            };

            PopulateNoteItemsInformation( new List<NoteItemBag> { noteItem } );

            return ActionOk( noteItem );
        }

        #endregion
    }
}
