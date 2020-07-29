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

    [TextField( "Heading",
        Description = "The text to display as the heading.  If left blank, the Note Type name will be used.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Heading )]

    [TextField( "Heading Icon CSS Class",
        Description = "The css class name to use for the heading icon. ",
        IsRequired = false,
        DefaultValue = "fa fa-sticky-note-o",
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
        Description = "Should replies to automatically expanded?",
        DefaultValue = "false",
        Order = 14,
        Key = AttributeKey.ExpandReplies )]

    [CodeEditorField( "Note View Lava Template",
        Description = "The Lava Template to use when rendering the readonly view of all the notes.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = @"{% include '~~/Assets/Lava/NoteViewList.lava' %}",
        Order = 15,
        Key = AttributeKey.NoteViewLavaTemplate )]

    public partial class Notes : RockBlock, ISecondaryBlock
    {
        public static class AttributeKey
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
            public const string NoteViewLavaTemplate = "NoteViewLavaTemplate";
        }

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

                using ( var rockContext = new RockContext() )
                {
                    var noteTypes = NoteTypeCache.GetByEntity( contextEntity.TypeId, string.Empty, string.Empty, true );

                    // If block is configured to only allow certain note types, limit notes to those types.
                    var configuredNoteTypes = GetAttributeValue( AttributeKey.NoteTypes ).SplitDelimitedValues().AsGuidList();
                    if ( configuredNoteTypes.Any() )
                    {
                        noteTypes = noteTypes.Where( n => configuredNoteTypes.Contains( n.Guid ) ).ToList();
                    }

                    NoteOptions noteOptions = new NoteOptions( notesTimeline )
                    {
                        EntityId = contextEntity.Id,
                        NoteTypes = noteTypes.ToArray(),
                        NoteLabel = GetAttributeValue( AttributeKey.NoteTerm ),
                        DisplayType = GetAttributeValue( AttributeKey.DisplayType ) == "Light" ? NoteDisplayType.Light : NoteDisplayType.Full,
                        ShowAlertCheckBox = GetAttributeValue( AttributeKey.ShowAlertCheckbox ).AsBoolean(),
                        ShowPrivateCheckBox = GetAttributeValue( AttributeKey.ShowPrivateCheckbox ).AsBoolean(),
                        ShowSecurityButton = GetAttributeValue( AttributeKey.ShowSecurityButton ).AsBoolean(),
                        AddAlwaysVisible = GetAttributeValue( AttributeKey.AddAlwaysVisible ).AsBoolean(),
                        ShowCreateDateInput = GetAttributeValue( AttributeKey.AllowBackdatedNotes ).AsBoolean(),
                        NoteViewLavaTemplate = GetAttributeValue( AttributeKey.NoteViewLavaTemplate ),
                        DisplayNoteTypeHeading = GetAttributeValue( AttributeKey.DisplayNoteTypeHeading ).AsBoolean(),
                        UsePersonIcon = GetAttributeValue( AttributeKey.UsePersonIcon ).AsBoolean(),
                        ExpandReplies = GetAttributeValue( AttributeKey.ExpandReplies ).AsBoolean()
                    };

                    notesTimeline.NoteOptions = noteOptions;
                    notesTimeline.Title = GetAttributeValue( AttributeKey.Heading );
                    notesTimeline.TitleIconCssClass = GetAttributeValue( AttributeKey.HeadingIcon );
                    notesTimeline.AllowAnonymousEntry = GetAttributeValue( AttributeKey.AllowAnonymous ).AsBoolean();
                    notesTimeline.SortDirection = GetAttributeValue( AttributeKey.DisplayOrder ) == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
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