// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Note Editor control
    /// </summary>
    [ToolboxData( "<{0}:NoteEditor runat=server></{0}:NoteEditor>" )]
    public class NoteEditor : CompositeControl, INamingContainer
    {

        #region Fields

        private NoteControl _noteNew;
        private LinkButton _lbShowMore;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the note type identifier.
        /// </summary>
        public int? NoteTypeId
        {
            get { return ViewState["NoteTypeId"] as int?; }
            set { ViewState["NoteTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int? EntityId
        {
            get { return ViewState["EntityId"] as int?; }
            set { ViewState["EntityId"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display heading
        /// </summary>
        public bool ShowHeading
        {
            get { return ViewState["ShowHeading"] as bool? ?? true; }
            set { ViewState["ShowHeading"] = value; }
        }
        
        /// <summary>
        /// Gets or sets the CSS Class to use for the title icon.
        /// </summary>
        public string TitleIconCssClass
        {
            get { return ViewState["TitleIconCssClass"] as string; }
            set { ViewState["TitleIconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the title to display.
        /// </summary>
        public string Title
        {
            get { return ViewState["Title"] as string; }
            set { ViewState["Title"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [add always visible].
        /// </summary>
        public bool AddAlwaysVisible
        {
            get
            {
                if ( !ShowHeading )
                {
                    return true;
                }
                else
                {
                    EnsureChildControls();
                    return _noteNew.AddAlwaysVisible;
                }
            }

            set
            {
                EnsureChildControls();
                _noteNew.AddAlwaysVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets the title to display.
        /// </summary>
        public string Term
        {
            get
            {
                EnsureChildControls();
                return _noteNew.Label;
            }

            set
            {
                EnsureChildControls();
                _noteNew.Label = value;
            }
        }

        
        /// <summary>
        /// Gets or sets the display type.  Full or Light
        /// </summary>
        public NoteDisplayType DisplayType
        {
            get
            {
                EnsureChildControls();
                return _noteNew.DisplayType;
            }

            set
            {
                EnsureChildControls();
                _noteNew.DisplayType = value;
            }
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
                object sortDirection = this.ViewState["SortDirection"];
                return sortDirection != null ? (ListSortDirection)sortDirection : ListSortDirection.Descending;
            }
            set
            {
                this.ViewState["SortDirection"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Alert checkbox
        /// </summary>
        public bool ShowAlertCheckBox
        {
            get
            {
                EnsureChildControls();
                return _noteNew.ShowAlertCheckBox;
            }

            set
            {
                EnsureChildControls();
                _noteNew.ShowAlertCheckBox = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Is Private checkbox
        /// </summary>
        public bool ShowPrivateCheckBox
        {
            get
            {
                EnsureChildControls();
                return _noteNew.ShowPrivateCheckBox;
            }

            set
            {
                EnsureChildControls();
                _noteNew.ShowPrivateCheckBox = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the security button
        /// for existing notes
        /// </summary>
        public bool ShowSecurityButton
        {
            get
            {
                EnsureChildControls();
                return _noteNew.ShowSecurityButton;
            }

            set
            {
                EnsureChildControls();
                _noteNew.ShowSecurityButton = value;
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
        public int? DefaultSourceTypeValueId
        {
            get
            {
                EnsureChildControls();
                return _noteNew.SourceTypeValueId;
            }

            set
            {
                EnsureChildControls();
                _noteNew.SourceTypeValueId = value;
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the author's photo should 
        /// be displayed wiht the note instead of an icon based on the source
        /// of the note.
        /// </summary>
        public bool UsePersonIcon
        {
            get
            {
                EnsureChildControls();
                return _noteNew.UsePersonIcon;
            }

            set
            {
                EnsureChildControls();
                _noteNew.UsePersonIcon = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show more option].
        /// </summary>

        public bool ShowMoreOption
        {
            get
            {
                if ( SortDirection == ListSortDirection.Ascending )
                {
                    return false;
                }
                else
                {
                    EnsureChildControls();
                    return _lbShowMore.Visible;
                }
            }
            set
            {
                EnsureChildControls();
                _lbShowMore.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the current display count.  
        /// </summary>
        public int DisplayCount
        {
            get { return ViewState["DisplayCount"] as int? ?? 10; }
            set { ViewState["DisplayCount"] = value; }
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
            string script = @"
    $('a.add-note').click(function () {
        $(this).closest('.panel-note').find('.note-editor:first-child').children().slideToggle(""slow"");
    });
";

//            string noteId = PageParameter( "noteId" );
//            if ( !string.IsNullOrWhiteSpace( noteId ) )
//            {
//                script += string.Format( @"
//                    $('html, body').animate( {{scrollTop: $("".note-editor[rel='{0}']"").offset().top }},
//                        'slow',
//                        'swing',
//                        function() {{ 
//                            $("".note-editor[rel='{0}'] > article"").css( ""boxShadow"", ""1px 1px 8px 1px #888888"" );
//                        }}
//                    );",
//                noteId );
//            }

            ScriptManager.RegisterStartupScript( this, this.GetType(), "add-note", script, true );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            RebuildNotes( !this.Page.IsPostBack );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _noteNew = new NoteControl();
            _noteNew.ID = "noteNew";
            _noteNew.SaveButtonClick += note_SaveButtonClick;
            Controls.Add( _noteNew );

            _lbShowMore = new LinkButton();
            _lbShowMore.ID = "lbShowMore";
            _lbShowMore.Click += _lbShowMore_Click;
            Controls.Add( _lbShowMore );

            var iDownPre = new HtmlGenericControl( "i" );
            iDownPre.Attributes.Add( "class", "fa fa-angle-down" );
            _lbShowMore.Controls.Add( iDownPre );

            var spanDown = new HtmlGenericControl( "span" );
            spanDown.InnerHtml = "Load More";
            _lbShowMore.Controls.Add( spanDown );

            var iDownPost = new HtmlGenericControl( "i" );
            iDownPost.Attributes.Add( "class", "fa fa-angle-down" );
            _lbShowMore.Controls.Add( iDownPost );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool canAdd = AllowAnonymousEntry || GetCurrentPerson() != null;

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-note" );
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

                        writer.RenderEndTag();      // H3
                    }

                    if ( !AddAlwaysVisible && canAdd )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-action add-note" );
                        writer.RenderBeginTag( HtmlTextWriterTag.A );
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-plus" );
                        writer.RenderBeginTag( HtmlTextWriterTag.I );
                        writer.RenderEndTag();      // I
                        writer.RenderEndTag();      // A
                    }

                    writer.RenderEndTag();      // Div.panel-heading

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( canAdd && SortDirection == ListSortDirection.Descending )
                {
                    _noteNew.RenderControl( writer );
                }

                foreach ( Control control in Controls )
                {
                    if ( control is NoteControl && control.ID != "noteNew" )
                    {
                        var noteEditor = (NoteControl)control;
                        noteEditor.DisplayType = this.DisplayType;
                        noteEditor.ShowAlertCheckBox = this.ShowAlertCheckBox;
                        noteEditor.ShowPrivateCheckBox = this.ShowPrivateCheckBox;
                        noteEditor.ShowSecurityButton = this.ShowSecurityButton;
                        noteEditor.UsePersonIcon = this.UsePersonIcon;
                        control.RenderControl( writer );
                    }
                }

                if ( canAdd && SortDirection == ListSortDirection.Ascending )
                {
                    _noteNew.RenderControl( writer );
                }
                else
                {
                    if ( ShowMoreOption )
                    {
                        _lbShowMore.RenderControl( writer );
                    }
                }

                if ( ShowHeading )
                {
                    writer.RenderEndTag();      // Div.panel-body
                }

                writer.RenderEndTag();      // Section

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void note_SaveButtonClick( object sender, EventArgs e )
        {
            EnsureChildControls();
            _noteNew.Text = string.Empty;
            _noteNew.IsAlert = false;
            _noteNew.IsPrivate = false;
            _noteNew.NoteId = null;

            RebuildNotes( true );
        }

        /// <summary>
        /// Handles the Updated event of the note control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void note_Updated( object sender, EventArgs e )
        {
            RebuildNotes( true );
        }

        /// <summary>
        /// Handles the Click event of the _lbShowMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbShowMore_Click( object sender, EventArgs e )
        {
            DisplayCount += 10;
            RebuildNotes( true );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearNotes()
        {
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                if ( Controls[i] is NoteControl && Controls[i].ID != "noteNew" )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

        /// <summary>
        /// Rebuilds the notes.
        /// </summary>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        public void RebuildNotes( bool setSelection )
        {
            ClearNotes();

            int? currentPersonId = null;
            var currentPerson = GetCurrentPerson();
            if ( currentPerson != null )
            {
                currentPersonId = currentPerson.Id;
                _noteNew.CreatedByPhotoId = currentPerson.PhotoId;
                _noteNew.CreatedByGender = currentPerson.Gender;
                _noteNew.CreatedByName = currentPerson.FullName;
            }
            else
            {
                _noteNew.CreatedByPhotoId = null;
                _noteNew.CreatedByGender = Gender.Male;
                _noteNew.CreatedByName = string.Empty;
            }

            _noteNew.NoteTypeId = NoteTypeId;
            _noteNew.EntityId = EntityId;

            if ( NoteTypeId.HasValue && EntityId.HasValue )
            {
                ShowMoreOption = false;

                int noteCount = 0;

                var qry = new NoteService().Queryable()
                    .Where( n =>
                        n.NoteTypeId == NoteTypeId.Value &&
                        n.EntityId == EntityId.Value );

                if ( SortDirection == ListSortDirection.Descending )
                {
                    qry = qry.OrderByDescending( n => n.IsAlert )
                        .ThenByDescending( n => n.CreationDateTime );
                }
                else
                {
                    qry = qry.OrderByDescending( n => n.IsAlert )
                        .ThenBy( n => n.CreationDateTime );
                }

                foreach ( var note in qry )
                {
                    if ( noteCount >= DisplayCount )
                    {
                        ShowMoreOption = true;
                        break;
                    }

                    if ( note.IsAuthorized( "View", currentPerson ) )
                    {
                        var noteEditor = new NoteControl();
                        noteEditor.ID = string.Format( "note_{0}", note.Guid.ToString().Replace( "-", "_" ) );
                        noteEditor.Note = note;
                        noteEditor.IsPrivate = note.IsPrivate( "View", currentPerson );
                        noteEditor.CanEdit = note.IsAuthorized( "Edit", currentPerson );
                        noteEditor.SaveButtonClick += note_Updated;
                        noteEditor.DeleteButtonClick += note_Updated;
                        Controls.Add( noteEditor );

                        noteCount++;
                    }
                }
            }
        }

        private Person GetCurrentPerson()
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                return rockPage.CurrentPerson;
            }

            return null;
        }

        #endregion

    }
}