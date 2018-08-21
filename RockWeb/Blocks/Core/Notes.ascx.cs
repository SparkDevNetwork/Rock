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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Context aware block for adding notes to an entity.
    /// </summary>
    [DisplayName( "Notes" )]
    [Category( "Core" )]
    [Description( "Context aware block for adding notes to an entity." )]

    [ContextAware]
    [TextField( "Heading", "The text to display as the heading.  If left blank, the Note Type name will be used.", false, "", "", 1 )]
    [TextField( "Heading Icon CSS Class", "The css class name to use for the heading icon. ", false, "fa fa-sticky-note-o", "", 2, "HeadingIcon" )]
    [TextField( "Note Term", "The term to use for note (i.e. 'Note', 'Comment').", false, "Note", "", 3 )]
    [CustomDropdownListField( "Display Type", "The format to use for displaying notes.", "Full,Light", true, "Full", "", 4 )]
    [BooleanField( "Use Person Icon", "", false, "", 5 )]
    [BooleanField( "Show Alert Checkbox", "", true, "", 6 )]
    [BooleanField( "Show Private Checkbox", "", true, "", 7 )]
    [BooleanField( "Show Security Button", "", true, "", 8 )]
    [BooleanField( "Allow Anonymous", "", false, "", 9 )]
    [BooleanField( "Add Always Visible", "Should the add entry screen always be visible (vs. having to click Add button to display the entry screen).", false, "", 10 )]
    [CustomDropdownListField( "Display Order", "Descending will render with entry field at top and most recent note at top.  Ascending will render with entry field at bottom and most recent note at the end.  Ascending will also disable the more option", "Ascending,Descending", true, "Descending", "", 11 )]
    [BooleanField( "Allow Backdated Notes", "", false, "", 12 )]
    [NoteTypeField( "Note Types", "Optional list of note types to limit display to", true, "", "", "", false, "", "", 12 )]
    [BooleanField( "Display Note Type Heading", "Should each note's Note Type be displayed as a heading above each note?", false, "", 13 )]
    [BooleanField( "Expand Replies", "Should replies to automatically expanded?", false, "", 14 )]
    [CodeEditorField( "Note View Lava Template", "The Lava Template to use when rendering the readonly view of all the notes.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, @"{% include '~~/Assets/Lava/NoteViewList.lava' %}", order: 15 )]
    //[BooleanField("Delete Me too")]
    public partial class Notes : RockBlock, ISecondaryBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Notes_BlockUpdated;

            ShowNotes();
        }

        /// <summary>
        /// Renders the notes.
        /// </summary>
        private void ShowNotes()
        {
            IEntity contextEntity;
            if ( ContextTypesRequired.Count == 1 )
            {
                contextEntity = this.ContextEntity( ContextTypesRequired.First().Name );
            }
            else
            {
                contextEntity = this.ContextEntity();
            }

            if ( contextEntity != null )
            {
                upNotes.Visible = true;

                string noteTypeName = GetAttributeValue( "NoteType" );

                using ( var rockContext = new RockContext() )
                {
                    var noteTypes = NoteTypeCache.GetByEntity(contextEntity.TypeId, string.Empty, string.Empty, true);

                    // If block is configured to only allow certain note types, limit notes to those types.
                    var configuredNoteTypes = GetAttributeValue( "NoteTypes" ).SplitDelimitedValues().AsGuidList();
                    if ( configuredNoteTypes.Any() )
                    {
                        noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
                    }

                    NoteOptions noteOptions = new NoteOptions( notesTimeline )
                    {
                        EntityId = contextEntity.Id,
                        NoteTypes = noteTypes.ToArray(),
                        NoteLabel = GetAttributeValue( "NoteTerm" ),
                        DisplayType = GetAttributeValue( "DisplayType" ) == "Light" ? NoteDisplayType.Light : NoteDisplayType.Full,
                        ShowAlertCheckBox = GetAttributeValue( "ShowAlertCheckbox" ).AsBoolean(),
                        ShowPrivateCheckBox = GetAttributeValue( "ShowPrivateCheckbox" ).AsBoolean(),
                        ShowSecurityButton = GetAttributeValue( "ShowSecurityButton" ).AsBoolean(),
                        AddAlwaysVisible = GetAttributeValue( "AddAlwaysVisible" ).AsBoolean(),
                        ShowCreateDateInput = GetAttributeValue( "AllowBackdatedNotes" ).AsBoolean(),
                        NoteViewLavaTemplate = GetAttributeValue( "NoteViewLavaTemplate" ),
                        DisplayNoteTypeHeading = GetAttributeValue( "DisplayNoteTypeHeading" ).AsBoolean(),
                        UsePersonIcon = GetAttributeValue( "UsePersonIcon" ).AsBoolean(),
                        ExpandReplies = GetAttributeValue( "ExpandReplies" ).AsBoolean()
                    };

                    notesTimeline.NoteOptions = noteOptions;
                    notesTimeline.Title = GetAttributeValue( "Heading" );
                    notesTimeline.TitleIconCssClass = GetAttributeValue( "HeadingIcon" );
                    notesTimeline.AllowAnonymousEntry = GetAttributeValue( "Allow Anonymous" ).AsBoolean();
                    notesTimeline.SortDirection = GetAttributeValue( "DisplayOrder" ) == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
                }
            }
            else
            {
                upNotes.Visible = false;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Notes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Notes_BlockUpdated( object sender, EventArgs e )
        {
            ShowNotes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            notesTimeline.Visible = visible;
        }

        #endregion
    }
}