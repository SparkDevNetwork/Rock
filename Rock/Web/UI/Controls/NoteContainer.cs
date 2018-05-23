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
using Rock.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Note Container control
    /// </summary>
    [ToolboxData( "<{0}:NoteContainer runat=server></{0}:NoteContainer>" )]
    public class NoteContainer : CompositeControl, INamingContainer
    {
        #region Fields

        private NoteEditor _noteEditor;
        private LinkButton _lbShowMore;
        private HiddenFieldWithClass _hfCurrentNoteId;
        private ModalAlert _mdDeleteWarning;
        private ModalDialog _mdConfirmDelete;
        private Literal _lConfirmDeleteMsg;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the note options.
        /// </summary>
        /// <value>
        /// The note options.
        /// </value>
        public NoteOptions NoteOptions { get; set; } = new NoteOptions();

        /// <summary>
        /// Gets or sets the note types.
        /// </summary>
        /// <value>
        /// The note types.
        /// </value>
        [Obsolete( "Use NoteOptions.NoteTypes instead" )]
        public List<Rock.Web.Cache.NoteTypeCache> NoteTypes
        {
            get
            {
                return NoteOptions?.NoteTypes.Select( a => Rock.Web.Cache.NoteTypeCache.Read( a.Id ) ).ToList();
            }

            set
            {
                NoteOptions.NoteTypes = value?.Select( a => CacheNoteType.Get( a.Id ) ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        [Obsolete( "Use NoteOptions.EntityId instead" )]
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

            if ( this.NoteOptions != null )
            {
                _noteEditor.EntityId = this.NoteOptions.EntityId;
            }
            else
            {
                Debug.Assert( this.NoteOptions != null, "this.NoteOptions is null" );
            }

            if ( this.Page.IsPostBack )
            {
                RouteAction();
            }
        }

        /// <summary>
        /// Routes the action.
        /// </summary>
        private void RouteAction()
        {
            if ( this.Page.Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = this.Page.Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];
                    int? noteId;

                    switch ( action )
                    {
                        case "DeleteNote":
                            noteId = parameters.AsIntegerOrNull();
                            DisplayDeleteNote( noteId );
                            break;
                        case "ApproveNote":
                            noteId = parameters.AsIntegerOrNull();
                            ApproveNote( noteId, true );
                            break;
                        case "DenyApproveNote":
                            noteId = parameters.AsIntegerOrNull();
                            ApproveNote( noteId, false );
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _noteEditor = new NoteEditor( this.NoteOptions );
            _noteEditor.ID = this.ID + "_noteEditor";
            _noteEditor.CssClass = "note-new";

            _noteEditor.CreatedByPersonAlias = ( this.Page as RockPage )?.CurrentPersonAlias;
            _noteEditor.SaveButtonClick += note_SaveButtonClick;

            Controls.Add( _noteEditor );

            // Create a hidden field that javascript will populate with the selected note
            _hfCurrentNoteId = new HiddenFieldWithClass();
            _hfCurrentNoteId.ID = this.ID + "_hfCurrentNoteId";
            _hfCurrentNoteId.CssClass = "js-currentnoteid";
            Controls.Add( _hfCurrentNoteId );

            _mdConfirmDelete = new ModalDialog();
            _mdConfirmDelete.ID = this.ID + "_mdConfirmDelete";
            _mdConfirmDelete.Title = "Please Confirm";
            _mdConfirmDelete.SaveButtonText = "Yes";
            _mdConfirmDelete.SaveClick += _mdConfirmDelete_SaveClick;
            Controls.Add( _mdConfirmDelete );
            _lConfirmDeleteMsg = new Literal();
            _lConfirmDeleteMsg.ID = this.ID + "_lConfirmDeleteMsg";
            _mdConfirmDelete.Content.Controls.Add( _lConfirmDeleteMsg );

            _mdDeleteWarning = new ModalAlert();
            _mdDeleteWarning.ID = this.ID + "_mdDeleteWarning";
            Controls.Add( _mdDeleteWarning );

            _lbShowMore = new LinkButton();
            _lbShowMore.ID = "lbShowMore";
            _lbShowMore.Click += _lbShowMore_Click;
            _lbShowMore.AddCssClass( "load-more btn btn-xs btn-action" );
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
        /// Handles the SaveClick event of the _mdConfirmDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _mdConfirmDelete_SaveClick( object sender, EventArgs e )
        {
            _mdConfirmDelete.Hide();
            DeleteNote( _hfCurrentNoteId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Displays the delete note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        private void DisplayDeleteNote( int? noteId )
        {
            _lConfirmDeleteMsg.Text = "Are you sure you want to delete this note?";
            _hfCurrentNoteId.Value = noteId.ToString();
            _mdConfirmDelete.Show();
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
                        if ( service.CanDelete( note, out errorMessage ) )
                        {
                            service.Delete( note );
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
        /// Approves the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <param name="approved">if set to <c>true</c> [approved].</param>
        private void ApproveNote( int? noteId, bool approved )
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
                    if ( note != null && note.IsAuthorized( Authorization.APPROVE, currentPerson ) )
                    {
                        if ( approved )
                        {
                            note.ApprovalStatus = NoteApprovalStatus.Approved;
                        }
                        else
                        {
                            note.ApprovalStatus = NoteApprovalStatus.Denied;
                        }

                        note.ApprovedByPersonAliasId = currentPerson?.PrimaryAliasId;

                        note.ApprovedDateTime = RockDateTime.Now;
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
                    ( this.NoteOptions.DisplayType == NoteDisplayType.Light ? " panel-note-light" : string.Empty );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, cssClass );
                writer.AddAttribute( "data-sortdirection", this.SortDirection.ConvertToString( false ) );
                writer.RenderBeginTag( "section" );

                // Heading
                if ( ShowHeading )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    if ( !string.IsNullOrWhiteSpace( TitleIconCssClass ) ||
                        !string.IsNullOrWhiteSpace( Title ) )
                    {
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
                    }

                    if ( !NoteOptions.AddAlwaysVisible && canAdd && SortDirection == ListSortDirection.Descending )
                    {
                        RenderAddButton( writer );
                    }

                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( canAdd && SortDirection == ListSortDirection.Descending )
                {
                    if ( !ShowHeading && !NoteOptions.AddAlwaysVisible )
                    {
                        RenderAddButton( writer );
                    }

                    _noteEditor.RenderControl( writer );
                }

                _hfCurrentNoteId.RenderControl( writer );
                _mdConfirmDelete.RenderControl( writer );
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
                    var noteMergeFields = LavaHelper.GetCommonMergeFields( rockBlock?.RockPage, currentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                    noteMergeFields.Add( "NoteOptions", this.NoteOptions );
                    noteMergeFields.Add( "NoteList", viewableNoteList );

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
                        _lbShowMore.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
        /// Clears the rows.
        /// </summary>
        [Obsolete( "Not Needed. Notes will be cleared and rebuilt automatically" )]
        public void ClearNotes()
        {
            //
        }

        /// <summary>
        /// Rebuilds the notes.
        /// </summary>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        [Obsolete( "Not Needed. Notes will be rebuilt automatically" )]
        public void RebuildNotes( bool setSelection )
        {
            //
        }

        /// <summary>
        /// Gets the List of root notes that the currentPerson is authorized to view for this EntityId and NoteTypes
        /// </summary>
        private List<Note> GetViewableNoteList( RockContext rockContext, Person currentPerson )
        {
            var viewableNoteTypes = this.NoteOptions?.GetViewableNoteTypes( currentPerson );
            var entityId = this.NoteOptions?.EntityId;

            ShowMoreOption = false;
            if ( viewableNoteTypes != null && viewableNoteTypes.Any() && entityId.HasValue )
            {
                var viewableNoteTypeIds = viewableNoteTypes.Select( t => t.Id ).ToList();

                // only show Viewable Note Types for this Entity and only show the Root Notes (the NoteControl will take care of child notes)
                var qry = new NoteService( rockContext ).Queryable().Include( a => a.ChildNotes ).Include( a => a.CreatedByPersonAlias.Person )
                    .Where( n =>
                        viewableNoteTypeIds.Contains( n.NoteTypeId )
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

                NoteCount = noteList.Count();

                // only get notes they have auth to VIEW
                var viewableNoteList = noteList.Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) ).ToList();

                return viewableNoteList;
            }

            return null;
        }

        /// <summary>
        /// Renders the add button.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderAddButton( HtmlTextWriter writer )
        {
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