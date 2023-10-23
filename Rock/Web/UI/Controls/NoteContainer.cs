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
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Note Container control
    /// </summary>
    [DefaultProperty( "NoteViewLavaTemplate" )]
    [ToolboxData( "<{0}:NoteContainer runat=server></{0}:NoteContainer>" )]
    [ParseChildren( true, "NoteViewLavaTemplate" )]
    public class NoteContainer : CompositeControl, INamingContainer
    {
        #region Fields

        private NoteEditor _noteEditor;
        private LinkButton _lbShowMore;
        private HiddenFieldWithClass _hfCurrentNoteId;
        private HiddenFieldWithClass _hfExpandedNoteIds;
        private ModalAlert _mdDeleteWarning;
        private LinkButton _lbDeleteNote;
        private LinkButton _lbAddNoteHidden;
        private LinkButton _lbEditNoteHidden;
        private LinkButton _lbReplyToNoteHidden;

        #endregion

        #region Note Options

        /// <summary>
        /// Gets or sets the NoteOptions for note type list.
        /// </summary>
        /// <value>
        /// The note type list.
        /// </value>
        public void SetNoteTypes( List<NoteTypeCache> noteTypeList )
        {
            NoteOptions.SetNoteTypes( noteTypeList );
        }

        /// <summary>
        /// Gets or sets the note view lava template.
        /// </summary>
        /// <value>
        /// The note view lava template.
        /// </value>
        [PersistenceMode( PersistenceMode.InnerDefaultProperty )]
        public string NoteViewLavaTemplate
        {
            get
            {
                return NoteOptions?.NoteViewLavaTemplate;
            }

            set
            {
                NoteOptions.NoteViewLavaTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions for entity identifier.
        /// </summary>
        public int? EntityId
        {
            get
            {
                return NoteOptions?.EntityId;
            }

            set
            {
                NoteOptions.EntityId = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions for indicating whether [add always visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add always visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AddAlwaysVisible
        {
            get
            {
                return this.NoteOptions.AddAlwaysVisible;
            }

            set
            {
                this.NoteOptions.AddAlwaysVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions for indicating whether [display note type heading].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display note type heading]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayNoteTypeHeading
        {
            get
            {
                return this.NoteOptions.DisplayNoteTypeHeading;
            }

            set
            {
                this.NoteOptions.DisplayNoteTypeHeading = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions for display type.
        /// </summary>
        /// <value>
        /// The display type.
        /// </value>
        public NoteDisplayType DisplayType
        {
            get
            {
                return this.NoteOptions.DisplayType;
            }

            set
            {
                this.NoteOptions.DisplayType = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions indicating whether [show alert CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show alert CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAlertCheckBox
        {
            get
            {
                return this.NoteOptions.ShowAlertCheckBox;
            }

            set
            {
                this.NoteOptions.ShowAlertCheckBox = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions indicating whether [show create date input].
        /// Set this to true to allow the create date time to be editable.
        /// For example, when Back-Dating Notes is allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show create date input]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCreateDateInput
        {
            get
            {
                return this.NoteOptions.ShowCreateDateInput;
            }

            set
            {
                this.NoteOptions.ShowCreateDateInput = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions indicating whether [show private CheckBox].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show private CheckBox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrivateCheckBox
        {
            get
            {
                return this.NoteOptions.ShowPrivateCheckBox;
            }

            set
            {
                this.NoteOptions.ShowPrivateCheckBox = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions indicating whether [show security button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show security button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSecurityButton
        {
            get
            {
                return this.NoteOptions.ShowSecurityButton;
            }

            set
            {
                this.NoteOptions.ShowSecurityButton = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions note label.
        /// </summary>
        /// <value>
        /// The note label.
        /// </value>
        public string NoteLabel
        {
            get
            {
                return this.NoteOptions.NoteLabel;
            }

            set
            {
                this.NoteOptions.NoteLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets the NoteOptions value indicating whether [use person icon].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use person icon]; otherwise, <c>false</c>.
        /// </value>
        public bool UsePersonIcon
        {
            get
            {
                return this.NoteOptions.UsePersonIcon;
            }

            set
            {
                this.NoteOptions.UsePersonIcon = value;
            }
        }

        #endregion Note Options

        #region Properties

        /// <summary>
        /// Returns the ViewState StateBag of the NoteContainer
        /// Used Internally to manage state of NoteOptions object
        /// </summary>
        /// <value>
        /// The state of the container view.
        /// </value>
        internal StateBag ContainerViewState
        {
            get
            {
                return this.ViewState;
            }
        }

        private NoteOptions _noteOptions = null;

        /// <summary>
        /// Gets or sets all the note options as a NoteOptions object.
        /// </summary>
        /// <value>
        /// The note options.
        /// </value>
        public NoteOptions NoteOptions
        {
            get
            {
                _noteOptions = _noteOptions ?? new NoteOptions( this );
                return _noteOptions;
            }

            set
            {
                _noteOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display heading of the note container
        /// </summary>
        public bool ShowHeading
        {
            get { return ViewState["ShowHeading"] as bool? ?? true; }
            set { ViewState["ShowHeading"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Class to use for the title icon of the note container
        /// </summary>
        public string TitleIconCssClass
        {
            get { return ViewState["TitleIconCssClass"] as string; }
            set { ViewState["TitleIconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the title to display on the note container
        /// </summary>
        public string Title
        {
            get { return ViewState["Title"] as string; }
            set { ViewState["Title"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether adds are allowed
        /// </summary>
        public bool AddAllowed
        {
            get { return ViewState["AddAllowed"] as bool? ?? true; }
            set { ViewState["AddAllowed"] = value; }
        }

        /// <summary>
        /// Gets or sets the css for the add anchor tag
        /// </summary>
        public string AddAnchorCSSClass
        {
            get { return ViewState["AddAnchorCSSClass"] as string ?? "btn btn-xs btn-action btn-square"; }
            set { ViewState["AddAnchorCSSClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the add icon CSS class.
        /// </summary>
        public string AddIconCSSClass
        {
            get { return ViewState["AddIconCSSClass"] as string ?? "fa fa-plus"; }
            set { ViewState["AddIconCSSClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the add text.
        /// </summary>
        public string AddText
        {
            get { return ViewState["AddText"] as string ?? string.Empty; }
            set { ViewState["AddText"] = value; }
        }

        /// <summary>
        /// Gets or sets the sort direction.  Descending will render with entry field at top and most
        /// recent note at top.  Ascending will render with entry field at bottom and most recent note
        /// at the end.  Ascending will also disable the more option
        /// </summary>
        public ListSortDirection SortDirection
        {
            get
            {
                return this.ViewState["SortDirection"] as ListSortDirection? ?? ListSortDirection.Descending;
            }

            set
            {
                this.ViewState["SortDirection"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the allow anonymous.
        /// </summary>
        public bool AllowAnonymousEntry
        {
            get { return ViewState["AllowAnonymous"] as bool? ?? false; }
            set { ViewState["AllowAnonymous"] = value; }
        }

        /// <summary>
        /// Gets or sets the default source type value identifier.
        /// </summary>
        public int? DefaultNoteTypeId
        {
            get
            {
                EnsureChildControls();
                return _noteEditor.NoteTypeId;
            }

            set
            {
                EnsureChildControls();
                _noteEditor.NoteTypeId = value;
            }
        }

        /// <summary>
        /// Gets or sets the current display count. Only applies if notes are in descending order.
        /// If notes are displayed in ascending order, all notes will always be displayed
        /// </summary>
        public int DisplayCount
        {
            get { return ViewState["DisplayCount"] as int? ?? 10; }
            set { ViewState["DisplayCount"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show more option].
        /// </summary>
        private bool ShowMoreOption
        {
            get { return ViewState["ShowMoreOption"] as bool? ?? true; }
            set { ViewState["ShowMoreOption"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int NoteCount
        {
            get { return ViewState["NoteCount"] as int? ?? 0; }
            private set { ViewState["NoteCount"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();

            if ( this.NoteOptions != null )
            {
                _noteEditor.EntityId = this.NoteOptions.EntityId;
            }
            else
            {
                Debug.Assert( this.NoteOptions != null, "this.NoteOptions is null" );
            }

            // If Both the title and Title Icon are not provided and the add button outside of the header is available then don't show the heading.
            if ( Title.IsNullOrWhiteSpace() && TitleIconCssClass.IsNullOrWhiteSpace() && AddAlwaysVisible == true )
            {
                ShowHeading = false;
            }

            if ( this.Page.IsPostBack )
            {
                // RouteCustomAction handles WatchNote or UnwatchNote actions.
                // but Add, Edit, Delete and ReplyTo are handled with regular postback events
                RouteCustomAction();
            }
        }

        /// <summary>
        /// Routes any ApproveNote, DenyApproveNote, WatchNote or UnwatchNote action
        /// </summary>
        private void RouteCustomAction()
        {
            if ( this.Page.Request.Form["__EVENTARGUMENT"] == null )
            {
                return;
            }

            string[] eventArgs = this.Page.Request.Form["__EVENTARGUMENT"].Split( '^' );

            if ( eventArgs.Length != 2 )
            {
                return;
            }

            string action = eventArgs[0];
            string parameters = eventArgs[1];
            int? noteId;

            switch ( action )
            {
                case "WatchNote":
                    noteId = parameters.AsIntegerOrNull();
                    WatchNote( noteId, true );
                    break;

                case "UnwatchNote":
                    noteId = parameters.AsIntegerOrNull();
                    WatchNote( noteId, false );
                    break;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _noteEditor = new NoteEditor();
            _noteEditor.ID = this.ID + "_noteEditor";
            _noteEditor.SetNoteOptions( this.NoteOptions );
            _noteEditor.CssClass = "note-new";

            _noteEditor.CreatedByPersonAlias = ( this.Page as RockPage )?.CurrentPersonAlias;
            _noteEditor.SaveButtonClick += note_SaveButtonClick;

            Controls.Add( _noteEditor );

            // Create a hidden field that javascript will populate with the selected note
            _hfCurrentNoteId = new HiddenFieldWithClass();
            _hfCurrentNoteId.ID = this.ID + "_hfCurrentNoteId";
            _hfCurrentNoteId.CssClass = "js-currentnoteid";
            Controls.Add( _hfCurrentNoteId );

            _hfExpandedNoteIds = new HiddenFieldWithClass();
            _hfExpandedNoteIds.ID = this.ID + "_hfExpandedNoteIds";
            _hfExpandedNoteIds.CssClass = "js-expandednoteids";
            Controls.Add( _hfExpandedNoteIds );

            //
            // Create a hidden AddNote,ReplyTo, EditNote and DeleteNote linkbuttons that will hookup to lava'd and rendered buttons
            //

            _lbAddNoteHidden = new LinkButton();
            _lbAddNoteHidden.ID = this.ID + "_lbAddNoteHidden";
            _lbAddNoteHidden.CssClass = "js-add-postback";
            _lbAddNoteHidden.Click += _lbAddNoteHidden_Click;
            _lbAddNoteHidden.Style[HtmlTextWriterStyle.Display] = "none";
            _lbAddNoteHidden.CausesValidation = false;
            Controls.Add( _lbAddNoteHidden );

            _lbEditNoteHidden = new LinkButton();
            _lbEditNoteHidden.ID = this.ID + "_lbEditNoteHidden";
            _lbEditNoteHidden.CssClass = "js-edit-postback";
            _lbEditNoteHidden.Click += _lbEditNoteHidden_Click;
            _lbEditNoteHidden.Style[HtmlTextWriterStyle.Display] = "none";
            _lbEditNoteHidden.CausesValidation = false;
            Controls.Add( _lbEditNoteHidden );

            _lbReplyToNoteHidden = new LinkButton();
            _lbReplyToNoteHidden.ID = this.ID + "_lbReplyToNoteHidden";
            _lbReplyToNoteHidden.CssClass = "js-reply-to-postback";
            _lbReplyToNoteHidden.Click += _lbReplyToNoteHidden_Click;
            _lbReplyToNoteHidden.Style[HtmlTextWriterStyle.Display] = "none";
            _lbReplyToNoteHidden.CausesValidation = false;
            Controls.Add( _lbReplyToNoteHidden );


            _lbDeleteNote = new LinkButton();
            _lbDeleteNote.ID = this.ID + "_lbDeleteNote";
            _lbDeleteNote.CssClass = "js-delete-postback";
            _lbDeleteNote.Click += _lbDeleteNote_Click;
            _lbDeleteNote.CausesValidation = false;
            _lbDeleteNote.Style[HtmlTextWriterStyle.Display] = "none";
            Controls.Add( _lbDeleteNote );

            _mdDeleteWarning = new ModalAlert();
            _mdDeleteWarning.ID = this.ID + "_mdDeleteWarning";
            Controls.Add( _mdDeleteWarning );

            _lbShowMore = new LinkButton();
            _lbShowMore.ID = "lbShowMore";
            _lbShowMore.Click += _lbShowMore_Click;
            _lbShowMore.AddCssClass( "load-more btn btn-xs btn-action" );
            _lbShowMore.CausesValidation = false;
            Controls.Add( _lbShowMore );

            var iDownPre = new HtmlGenericControl( "i" );
            iDownPre.Attributes.Add( "class", "fa fa-angle-down" );
            _lbShowMore.Controls.Add( iDownPre );

            var spanDown = new HtmlGenericControl( "span" );
            spanDown.InnerHtml = " Load More ";
            _lbShowMore.Controls.Add( spanDown );

            var iDownPost = new HtmlGenericControl( "i" );
            iDownPost.Attributes.Add( "class", "fa fa-angle-down" );
            _lbShowMore.Controls.Add( iDownPost );
        }

        /// <summary>
        /// Handles the Click event of the _lbReplyToNoteHidden control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbReplyToNoteHidden_Click( object sender, EventArgs e )
        {
            ReplyToNote( _hfCurrentNoteId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the _lbEditNoteHidden control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbEditNoteHidden_Click( object sender, EventArgs e )
        {
            EditNote( _hfCurrentNoteId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the _lbAddNoteHidden control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbAddNoteHidden_Click( object sender, EventArgs e )
        {
            AddNote();
        }

        /// <summary>
        /// Handles the Click event of the _lbDeleteNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbDeleteNote_Click( object sender, EventArgs e )
        {
            DeleteNote( _hfCurrentNoteId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        private void DeleteNote( int? noteId )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var currentPerson = rockPage.CurrentPerson;

                var rockContext = new RockContext();
                var service = new NoteService( rockContext );
                Note note = null;

                if ( noteId.HasValue )
                {
                    note = service.Get( noteId.Value );
                    if ( note != null && note.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        string errorMessage;
                        if ( service.CanDeleteChildNotes( note, currentPerson, out errorMessage ) && service.CanDelete( note, out errorMessage ) )
                        {
                            service.Delete( note, true );
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            _mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds the note.
        /// </summary>
        private void AddNote()
        {
            var note = new Note();
            note.CreatedByPersonAlias = this.RockBlock()?.CurrentPersonAlias;

            _noteEditor.IsEditing = true;
            _noteEditor.SetNote( note );
        }

        /// <summary>
        /// Edits the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        private void EditNote( int? noteId )
        {
            if ( !noteId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var noteService = new NoteService( rockContext );
            var note = noteService.Get( noteId.Value );

            _noteEditor.IsEditing = true;
            _noteEditor.SetNote( note );
        }

        /// <summary>
        /// Replies to note.
        /// </summary>
        /// <param name="parentNoteId">The parent note identifier.</param>
        private void ReplyToNote( int? parentNoteId )
        {
            var note = new Note();
            note.ParentNoteId = parentNoteId;

            _noteEditor.IsEditing = true;
            _noteEditor.SetNote( note );
        }

        /// <summary>
        /// Set the note as Watched or Unwatched by the current person
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <param name="watching">if set to <c>true</c> [watching].</param>
        private void WatchNote( int? noteId, bool watching )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var currentPerson = rockPage.CurrentPerson;

                var rockContext = new RockContext();
                var noteService = new NoteService( rockContext );
                var noteWatchService = new NoteWatchService( rockContext );

                if ( noteId.HasValue )
                {
                    var note = noteService.Get( noteId.Value );
                    if ( note != null && note.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        var noteWatch = noteWatchService.Queryable().Where( a => a.NoteId == noteId.Value && a.WatcherPersonAlias.PersonId == currentPerson.Id ).FirstOrDefault();
                        if ( noteWatch == null )
                        {
                            noteWatch = new NoteWatch { NoteId = noteId.Value, WatcherPersonAliasId = rockPage.CurrentPersonAliasId };
                            noteWatchService.Add( noteWatch );
                        }

                        noteWatch.IsWatching = watching;
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                var currentPerson = ( this.Page as RockPage )?.CurrentPerson;
                var editableNoteTypes = NoteOptions.GetEditableNoteTypes( currentPerson );
                bool canAdd = AddAllowed &&
                    editableNoteTypes.Any() &&
                    ( AllowAnonymousEntry || currentPerson != null );

                string cssClass = "panel panel-note js-notecontainer" +
                    ( this.NoteOptions.DisplayType == NoteDisplayType.Light ? " panel-note-light" : string.Empty ) + ( NoteOptions.AddAlwaysVisible ? " panel-noteadd-visible" : string.Empty );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, cssClass );
                writer.AddAttribute( "data-sortdirection", this.SortDirection.ConvertToString( false ) );
                writer.RenderBeginTag( "section" );

                // Heading
                if ( ShowHeading )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-title" );
                    writer.RenderBeginTag( HtmlTextWriterTag.H3 );

                    if ( !string.IsNullOrWhiteSpace( TitleIconCssClass ) )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, TitleIconCssClass );
                        writer.RenderBeginTag( HtmlTextWriterTag.I );
                        writer.RenderEndTag();      // I
                    }

                    if ( !string.IsNullOrWhiteSpace( Title ) )
                    {
                        writer.Write( " " );
                        writer.Write( Title );
                    }

                    writer.RenderEndTag();

                    if ( !NoteOptions.AddAlwaysVisible && canAdd && SortDirection == ListSortDirection.Descending )
                    {
                        RenderAddButton( writer );
                    }

                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _noteEditor.ShowEditMode = NoteOptions.AddAlwaysVisible;

                if ( canAdd && SortDirection == ListSortDirection.Descending )
                {
                    if ( !ShowHeading && !NoteOptions.AddAlwaysVisible )
                    {
                        RenderAddButton( writer );
                    }

                    _noteEditor.RenderControl( writer );
                }

                _hfCurrentNoteId.RenderControl( writer );
                _hfExpandedNoteIds.RenderControl( writer );
                _lbDeleteNote.RenderControl( writer );
                _lbEditNoteHidden.RenderControl( writer );
                _lbReplyToNoteHidden.RenderControl( writer );

                _mdDeleteWarning.RenderControl( writer );
                using ( var rockContext = new RockContext() )
                {
                    List<Note> viewableNoteList = GetViewableNoteList( rockContext, currentPerson );

                    this.ShowMoreOption = ( SortDirection == ListSortDirection.Descending ) && ( viewableNoteList.Count > this.DisplayCount );
                    if ( this.ShowMoreOption )
                    {
                        viewableNoteList = viewableNoteList.Take( this.DisplayCount ).ToList();
                    }

                    var rockBlock = this.RockBlock();
                    var noteMergeFields = LavaHelper.GetCommonMergeFields( rockBlock?.RockPage, currentPerson, new CommonMergeFieldsOptions() );
                    noteMergeFields.Add( "NoteOptions", this.NoteOptions );
                    noteMergeFields.Add( "NoteList", viewableNoteList );
                    List<int> expandedNoteIdList = _hfExpandedNoteIds.Value.SplitDelimitedValues().AsIntegerList();
                    noteMergeFields.Add( "ExpandedNoteIds", expandedNoteIdList );

                    var noteTreeHtml = this.NoteOptions.NoteViewLavaTemplate.ResolveMergeFields( noteMergeFields ).ResolveClientIds( this.ParentUpdatePanel()?.ClientID );
                    writer.Write( noteTreeHtml );
                }

                if ( canAdd && SortDirection == ListSortDirection.Ascending )
                {
                    if ( !NoteOptions.AddAlwaysVisible )
                    {
                        RenderAddButton( writer );
                    }

                    _noteEditor.RenderControl( writer );
                }
                else
                {
                    if ( ShowMoreOption )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "load-more-container" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        _lbShowMore.RenderControl( writer );
                        writer.RenderEndTag();
                    }
                }

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SaveButtonClick event of the note control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NoteEventArgs"/> instance containing the event data.</param>
        protected void note_SaveButtonClick( object sender, NoteEventArgs e )
        {
            EnsureChildControls();
            _noteEditor.Text = string.Empty;
            _noteEditor.IsAlert = false;
            _noteEditor.IsPrivate = false;
            _noteEditor.NoteId = null;

            if ( NotesUpdated != null )
            {
                NotesUpdated( this, e );
            }
        }

        /// <summary>
        /// Handles the Updated event of the note control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void note_Updated( object sender, NoteEventArgs e )
        {
            if ( NotesUpdated != null )
            {
                NotesUpdated( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the _lbShowMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbShowMore_Click( object sender, EventArgs e )
        {
            DisplayCount += 10;
            if ( NotesUpdated != null )
            {
                NotesUpdated( this, new NoteEventArgs( null ) );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the List of root notes that the currentPerson is authorized to view for this EntityId and NoteTypes
        /// </summary>
        private List<Note> GetViewableNoteList( RockContext rockContext, Person currentPerson )
        {
            var configuredNoteTypes = this.NoteOptions?.NoteTypes.ToList();
            var entityId = this.NoteOptions?.EntityId;

            ShowMoreOption = false;
            if ( configuredNoteTypes != null && configuredNoteTypes.Any() && entityId.HasValue )
            {
                var configuredNoteTypeIds = configuredNoteTypes.Select( t => t.Id ).ToList();

                // only show Viewable Note Types for this Entity and only show the Root Notes (the NoteControl will take care of child notes)
                var qry = new NoteService( rockContext ).Queryable()
                    .Include( a => a.ChildNotes )
                    .Include( a => a.CreatedByPersonAlias.Person )
                    .Where( n =>
                        configuredNoteTypeIds.Contains( n.NoteTypeId )
                        && n.EntityId == entityId.Value
                        && n.ParentNoteId == null );

                if ( SortDirection == ListSortDirection.Descending )
                {
                    qry = qry.OrderByDescending( n => n.IsAlert == true )
                        .ThenByDescending( n => n.CreatedDateTime );
                }
                else
                {
                    qry = qry.OrderByDescending( n => n.IsAlert == true )
                        .ThenBy( n => n.CreatedDateTime );
                }

                var noteList = qry.ToList();

                /*
                 * 3-DEC-2021 DMV
                 *
                 * Moved the viewable note types here because granting
                 * an individual rights to view an specific note gets lost
                 * if the viewable types are in the query above.
                 *
                 */
                // only get notes they have auth to VIEW
                var viewableNoteList = noteList.Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) ).ToList();

                NoteCount = viewableNoteList.Count();

                return viewableNoteList;
            }

            return new List<Note>();
        }

        /// <summary>
        /// Renders the add button.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderAddButton( HtmlTextWriter writer )
        {
            // NOTE that _lbAddNoteHidden is rendered as display:none, but noteEditor.js will use it to figure the correct postback js for adding a note
            _lbAddNoteHidden.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "add-note js-addnote " + AddAnchorCSSClass );
            writer.RenderBeginTag( HtmlTextWriterTag.A );

            if ( !string.IsNullOrWhiteSpace( AddIconCSSClass ) )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-plus" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
            }

            writer.Write( AddText );

            writer.RenderEndTag();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when notes are updated.
        /// </summary>
        public event EventHandler<NoteEventArgs> NotesUpdated;

        #endregion
    }
}