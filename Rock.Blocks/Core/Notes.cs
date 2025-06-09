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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.Note;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.Notes;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Context aware block for adding notes to an entity.
    /// </summary>
    [DisplayName( "Notes" )]
    [Category( "Core" )]
    [Description( "Context aware block for adding notes to an entity." )]
    [IconCssClass( "fa fa-note" )]
    [SupportedSiteTypes( SiteType.Web )]
    [Rock.Web.UI.ContextAware]

    #region Block Attributes

    [TextField( "Heading",
        Description = "The text to display as the heading.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Heading )]

    [TextField( "Heading Icon CSS Class",
        Description = "The css class name to use for the heading icon. ",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.HeadingIcon )]

    [TextField( "Note Term",
        Description = "The term to use for note (i.e. 'Note', 'Comment').",
        IsRequired = false,
        DefaultValue = "Note",
        Order = 3,
        Key = AttributeKey.NoteTerm )]

    [CustomDropdownListField( "Display Type",
        Description = "The format to use for displaying notes.",
        ListSource = "Full,Light",
        IsRequired = true,
        DefaultValue = "Full",
        Order = 4,
        Key = AttributeKey.DisplayType )]

    [BooleanField( "Use Person Icon",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.UsePersonIcon )]

    [BooleanField( "Show Alert Checkbox",
        DefaultValue = "true",
        Order = 6,
        Key = AttributeKey.ShowAlertCheckbox )]

    [BooleanField( "Show Private Checkbox",
        DefaultValue = "true",
        Order = 7,
        Key = AttributeKey.ShowPrivateCheckbox )]

    [BooleanField( "Show Security Button",
        DefaultValue = "true",
        Order = 8,
        Key = AttributeKey.ShowSecurityButton )]

    [BooleanField( "Allow Anonymous",
        DefaultValue = "false",
        Order = 9,
        Key = AttributeKey.AllowAnonymous )]

    [BooleanField( "Add Always Visible",
        Description = "Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).",
        DefaultValue = "false",
        Order = 10,
        Key = AttributeKey.AddAlwaysVisible )]

    [CustomDropdownListField( "Display Order",
        Description = "Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option",
        ListSource = "Ascending,Descending",
        IsRequired = true,
        DefaultValue = "Descending",
        Order = 11,
        Key = AttributeKey.DisplayOrder )]

    [BooleanField( "Allow Backdated Notes",
        DefaultValue = "false",
        Order = 12,
        Key = AttributeKey.AllowBackdatedNotes )]

    [NoteTypeField( "Note Types",
        Description = "Optional list of note types to limit display to",
        AllowMultiple = true,
        IsRequired = false,
        Order = 12,
        Key = AttributeKey.NoteTypes )]

    [BooleanField( "Display Note Type Heading",
        Description = "Should each note's Note Type be displayed as a heading above each note?",
        DefaultValue = "false",
        Order = 13,
        Key = AttributeKey.DisplayNoteTypeHeading )]

    [BooleanField( "Expand Replies",
        Description = "Should replies be automatically expanded?",
        DefaultValue = "false",
        Order = 14,
        Key = AttributeKey.ExpandReplies )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "33566B2B-D74F-4148-B962-1897D418C6DF" )]
    [Rock.SystemGuid.BlockTypeGuid( "D87B84DC-7AD9-42A2-B18D-88B7E71DADA8" )]
    public class Notes : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Heading = "Heading";
            public const string HeadingIcon = "HeadingIcon";
            public const string NoteTerm = "NoteTerm";
            public const string DisplayType = "DisplayType";
            public const string UsePersonIcon = "UsePersonIcon";
            public const string ShowAlertCheckbox = "ShowAlertCheckbox";
            public const string ShowPrivateCheckbox = "ShowPrivateCheckbox";
            public const string ShowSecurityButton = "ShowSecurityButton";
            public const string AllowAnonymous = "AllowAnonymous";
            public const string AddAlwaysVisible = "AddAlwaysVisible";
            public const string DisplayOrder = "DisplayOrder";
            public const string AllowBackdatedNotes = "AllowBackdatedNotes";
            public const string NoteTypes = "NoteTypes";
            public const string DisplayNoteTypeHeading = "DisplayNoteTypeHeading";
            public const string ExpandReplies = "ExpandReplies";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var contextEntity = GetContextEntity();

            if ( contextEntity == null )
            {
                return new
                {
                };
            }

            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson )
            {
                AllowBackdatedNotes = GetAttributeValue( AttributeKey.AllowBackdatedNotes ).AsBoolean(),
                AllowedNoteTypes = GetConfiguredNoteTypes( contextEntity.TypeId )
            };
            var noteTypes = GetConfiguredNoteTypes( contextEntity.TypeId );
            var noteTypeIds = noteTypes.Select( nt => nt.Id ).ToList();

            var noteCollection = noteClientService.GetViewableNotes( contextEntity );
            var notes = noteClientService.OrderNotes( noteCollection, GetAttributeValue( AttributeKey.DisplayOrder ) == "Descending" ).ToList();
            var watchedNoteIds = noteClientService.GetWatchedNoteIds( notes );

            notes.LoadAttributes( RockContext );

            return new NotesInitializationBox
            {
                ErrorMessage = ( string ) null,
                EntityIdKey = contextEntity.IdKey,
                EntityTypeIdKey = EntityTypeCache.Get( contextEntity.TypeId ).IdKey,
                Notes = notes.Select( n => noteClientService.GetNoteBag( n, watchedNoteIds ) ).ToList(),
                NoteTypes = noteTypes.Select( nt => noteClientService.GetNoteTypeBag( nt ) ).ToList(),
                Title = GetAttributeValue( AttributeKey.Heading ),
                TitleIconCssClass = GetAttributeValue( AttributeKey.HeadingIcon ),
                ShowAdd = GetAttributeValue( AttributeKey.AllowAnonymous ).AsBoolean() || RequestContext.CurrentPerson != null,
                IsDescending = GetAttributeValue( AttributeKey.DisplayOrder ) == "Descending",
                NoteLabel = GetAttributeValue( AttributeKey.NoteTerm ),
                IsLightMode = GetAttributeValue( AttributeKey.DisplayType ) == "Light",
                ShowAlertCheckBox = GetAttributeValue( AttributeKey.ShowAlertCheckbox ).AsBoolean(),
                ShowPrivateCheckBox = GetAttributeValue( AttributeKey.ShowPrivateCheckbox ).AsBoolean(),
                ShowSecurityButton = GetAttributeValue( AttributeKey.ShowSecurityButton ).AsBoolean(),
                AddAlwaysVisible = GetAttributeValue( AttributeKey.AddAlwaysVisible ).AsBoolean(),
                ShowCreateDateInput = GetAttributeValue( AttributeKey.AllowBackdatedNotes ).AsBoolean(),
                DisplayNoteTypeHeading = GetAttributeValue( AttributeKey.DisplayNoteTypeHeading ).AsBoolean(),
                UsePersonIcon = GetAttributeValue( AttributeKey.UsePersonIcon ).AsBoolean(),
                ExpandReplies = GetAttributeValue( AttributeKey.ExpandReplies ).AsBoolean(),
                PersonAvatarUrl = RequestContext.CurrentPerson?.PhotoUrl
            };
        }

        /// <summary>
        /// Gets the configured note types for this block. This considers the
        /// context entity type and then the block settings.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier for determining possible note types.</param>
        /// <returns>A list of <see cref="NoteTypeCache"/> objects that represent the configured note types.</returns>
        private List<NoteTypeCache> GetConfiguredNoteTypes( int entityTypeId )
        {
            var noteTypes = NoteTypeCache.GetByEntity( entityTypeId, string.Empty, string.Empty, true )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            // If block is configured to only allow certain note types, limit notes to those types.
            var configuredNoteTypes = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
            if ( configuredNoteTypes.Any() )
            {
                noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
            }

            return noteTypes;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Begins the edit process of a note by returning the <see cref="NoteEditBag"/>
        /// that contains any information required to edit the requested note.
        /// </summary>
        /// <param name="request">The request that describes which note to edit.</param>
        /// <returns>An instance of <see cref="NoteEditBag"/> that provides the editable details of the note.</returns>
        [BlockAction]
        public BlockActionResult EditNote( EditNoteRequestBag request )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );

            if ( request == null || request.IdKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            var note = new NoteService( RockContext ).Get( request.IdKey, false );

            if ( note == null )
            {
                return ActionNotFound( "Note not found." );
            }

            var noteBag = noteClientService.EditNote( note, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Saves the changes made to a note.
        /// </summary>
        /// <param name="request">The request that describes the note and the changes that were made.</param>
        /// <returns>A new <see cref="NoteBag"/> instance that describes the note for display purposes.</returns>
        [BlockAction]
        public BlockActionResult SaveNote( SaveNoteRequestBag request )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var contextEntity = GetContextEntity();

            if ( contextEntity == null )
            {
                return ActionBadRequest( "No context is available." );
            }

            if ( request == null || !request.IsValidProperty( nameof( NoteEditBag.IdKey ) ) )
            {
                return ActionBadRequest( "Request details are not valid." );
            }

            var noteBag = noteClientService.SaveNote( request, contextEntity, PageCache.Id, this.GetCurrentPageUrl(), RequestContext, out var errorMessage );

            if ( noteBag == null )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( noteBag );
        }

        /// <summary>
        /// Deletes the requested note from the database.
        /// </summary>
        /// <param name="request">The request that describes which note to delete.</param>
        /// <returns>An empty 200-OK response if the note was deleted.</returns>
        [BlockAction]
        public BlockActionResult DeleteNote( DeleteNoteRequestBag request )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var note = new NoteService( RockContext ).Get( request.IdKey, false );

            if ( !noteClientService.DeleteNote( note, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sets the watched state of a specific note.
        /// </summary>
        /// <param name="request">The request that describes which note and if it is to be watched or unwatched.</param>
        /// <returns>An empty 200-OK response if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult WatchNote( WatchNoteRequestBag request )
        {
            var noteClientService = new NoteClientService( RockContext, RequestContext.CurrentPerson );
            var note = new NoteService( RockContext ).Get( request.IdKey, false );

            if ( !noteClientService.WatchNote( note, request.IsWatching, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        #endregion
    }
}
