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

        public int? NoteTypeId
        {
            get { return ViewState["NoteTypeId"] as int?; }
            set { ViewState["NoteTypeId"] = value; }
        }

        public int? EntityId
        {
            get { return ViewState["EntityId"] as int?; }
            set { ViewState["EntityId"] = value; }
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

        public bool ShowMoreOption
        {
            get
            {
                EnsureChildControls();
                return _lbShowMore.Visible;
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

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            BuildNotes( !this.Page.IsPostBack );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _noteNew = new NoteControl();
            _noteNew.ID = "noteNew";
            _noteNew.NoteTypeId = NoteTypeId;
            _noteNew.EntityId = EntityId;
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-note" );
                writer.RenderBeginTag( "section" );

                // Heading
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

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-action add-note" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-plus" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();      // I
                writer.RenderEndTag();      // A

                writer.RenderEndTag();      // Div.panel-heading

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _noteNew.RenderControl( writer );

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

                if ( ShowMoreOption )
                {
                    _lbShowMore.RenderControl( writer );
                }

                writer.RenderEndTag();      // Div.panel-body

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

            BuildNotes( true );
        }

        protected void note_Updated( object sender, EventArgs e )
        {
            BuildNotes( true );
        }

        /// <summary>
        /// Handles the Click event of the _lbShowMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbShowMore_Click( object sender, EventArgs e )
        {
            DisplayCount += 10;
            BuildNotes( true );
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

        private void BuildNotes( bool setSelection )
        {
            ClearNotes();

            var rockPage = this.Page as RockPage;
            if (rockPage != null && NoteTypeId.HasValue)
            {
                int? currentPersonId = null;
                var currentPerson = rockPage.CurrentPerson;
                if (currentPerson != null)
                {
                    currentPersonId = currentPerson.Id;
                }

                if ( NoteTypeId.HasValue && EntityId.HasValue )
                {
                    ShowMoreOption = false;

                    int noteCount = 0;
                    foreach ( var note in new NoteService().Get( NoteTypeId.Value, EntityId.Value ) )
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
        }

        #endregion

    }
}