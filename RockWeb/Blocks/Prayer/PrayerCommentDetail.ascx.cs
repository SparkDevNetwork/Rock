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
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Comment Detail" )]
    [Category( "Prayer" )]
    [Description( "Shows a list of prayer comments and allows the noteId that is passed in (via querystring) to be editable." )]

    [ContextAware( typeof( PrayerRequest ) )]
    [TextField( "Title", "The title of the notes/comments section.", false, "Comments", "Behavior", 0 )]
    public partial class PrayerCommentDetail : RockBlock, IDetailBlock
    {
        #region Private BlockType Attributes
        private IEntity contextEntity = null;
        private NoteType noteType;
        private Note prayerComment = null;
        private bool useTheAfterPH = false;
        private static readonly string PrayerCommentKeyParameter = "noteId";
        #endregion

        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddNote.Click += lbAddNote_Click;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            string noteId = PageParameter( PrayerCommentKeyParameter );

            RegisterScripts();
            if ( !Page.IsPostBack )
            {
                lTitle.Text = GetAttributeValue( "Title" );

                if ( !string.IsNullOrWhiteSpace( noteId ) )
                {
                    // Set up to scroll to the top of the given note...
                    string script = string.Format(
                        @"$('html, body').animate({{scrollTop: $(""[rel='{0}']"").offset().top}}, {{ duration: 'slow', easing: 'swing'}});", noteId );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "scroll-to-comment-{0}", this.ClientID ), script, true );

                    prayerComment = new NoteService( new RockContext() ).Get( int.Parse( noteId ) );
                }
                else
                {
                    fieldsetEditDetails.Visible = false;
                }
            }

            contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                GetNoteType();

                if ( !Page.IsPostBack )
                {
                    // This will produce a complete list of related context entity notes with the editible one
                    // inline (in the middle of the note stream).
                    ShowNotes();
                }
            }
            else if ( !string.IsNullOrWhiteSpace( noteId ) && !Page.IsPostBack )
            {
                ShowEditDetails( prayerComment );
            }
        }

        /// <summary>
        /// Registers the scripts.
        /// </summary>
        private void RegisterScripts()
        {
            // script for handling the "+add" note button.
            string script = @"
    $('a.add-note').click(function () {
        $(this).parent().siblings('.widget-content').children('.note-entry').slideToggle(""slow"");
    });
    
    $('a.add-note-cancel').click(function () {
        $(this).parent().siblings('.note').children('textarea').val('');
        $(this).parent().parent().slideToggle(""slow"");
    });

    $('.persontimeline article').on({
        mouseenter:
            function () {
                var actionsDiv = $('.actions', this);
                if (actionsDiv.length > 0) {
                    $(actionsDiv).stop(true, true).fadeIn(""slow"");
                }
            },
        mouseleave:
            function () {
                var actionsDiv = $('.actions', this);
                if (actionsDiv.length > 0) {
                    $(actionsDiv).stop(true, true).fadeOut(""slow"");
                }
            }
    });
";
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "add-note", script, true );
        }

        /// <summary>
        /// Handles the Click event of the lbAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddNote_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new NoteService( rockContext );

            var note = new Note();
            note.IsSystem = false;
            note.IsAlert = false;
            note.NoteTypeId = noteType.Id;
            note.EntityId = contextEntity.Id;
            note.Text = tbNewNote.Text;

            if ( noteType.Sources != null )
            {
                var source = noteType.Sources.DefinedValues.FirstOrDefault();
                if ( source != null )
                {
                    note.SourceTypeValueId = source.Id;
                }
            }

            service.Add( note );
            rockContext.SaveChanges();

            ShowNotes();
        }

        /// <summary>
        /// Gets the type of the note.
        /// </summary>
        private void GetNoteType()
        {
            var rockContext = new RockContext();
            var service = new NoteTypeService( rockContext );
            noteType = service.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
        }

        /// <summary>
        /// Shows the notes.
        /// </summary>
        private void ShowNotes()
        {
            phNotesBefore.Controls.Clear();
            phNotesAfter.Controls.Clear();

            var rockContext = new RockContext();
            var service = new NoteService( rockContext );

            foreach ( var note in service.Get( noteType.Id, contextEntity.Id ) )
            {
                if ( note.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    if ( prayerComment != null && note.Id == prayerComment.Id )
                    {
                        ShowEditDetails( note );
                    }
                    else
                    {
                        AddNoteHtml( note );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the note HTML in the form:
        ///    <article class="alert alert-info">
        ///    <i class="fa fa-comment"></i>
        ///      <div class="detail"> <strong>Name/Caption</strong> <span class="muted">relative date</span><br>
        ///          <p>text</p>
        ///      </div>
        ///   </article>
        /// </summary>
        /// <param name="note">The note.</param>
        private void AddNoteHtml( Note note )
        {
            var article = new HtmlGenericControl( "article" );
            PlaceHolder phNotes = useTheAfterPH ? phNotesAfter : phNotesBefore;
            phNotes.Controls.Add( article );
            article.AddCssClass( "alert alert-info" );

            // The comment icon or possibly a picture of the person who made the comment...
            article.Controls.Add( new LiteralControl( "<i class='fa fa-comment'></i> " ) );

            var divDetail = new HtmlGenericControl( "div" );
            article.Controls.Add( divDetail );
            divDetail.AddCssClass( "detail" );

            // Add the name/caption
            string caption = note.Caption;
            if ( string.IsNullOrWhiteSpace( caption ) && note.CreatedByPersonAlias != null && note.CreatedByPersonAlias.Person != null )
            {
                caption = note.CreatedByPersonAlias.Person.FullName;
            }

            divDetail.Controls.Add( new LiteralControl( string.Format( "<strong>{0}</strong> <span class='text-muted'>{1}</span>", caption, note.CreatedDateTime.ToRelativeDateString() ) ) );

            var paragraphText = new HtmlGenericControl( "p" );
            divDetail.Controls.Add( paragraphText );
            paragraphText.Controls.Add( new LiteralControl( HttpUtility.HtmlEncode( note.Text ) ) );
        }

        /// <summary>
        /// Handles the edit Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( prayerComment );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        public void ShowDetail( int noteId )
        {
        }

        #endregion

        #region View & Edit Details

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void ShowReadonlyDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( false );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="prayerComment">The prayer comment.</param>
        private void ShowEditDetails( Note prayerComment )
        {
            SetEditMode( true );

            useTheAfterPH = true;
            fieldsetEditDetails.Visible = true;
            hfNoteId.Value = prayerComment.Id.ToString();

            dtbText.Text = prayerComment.Text;
            dtbCaption.Text = prayerComment.Caption;
            dtbText.Attributes.Add( "rel", prayerComment.Id.ToString() );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="enableEdit">if set to <c>true</c> [enable edit].</param>
        private void SetEditMode( bool enableEdit )
        {
            fieldsetEditDetails.Visible = enableEdit;
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SaveNote();
            NavigateToParentPage();
        }

        /// <summary>
        /// Saves the note.
        /// </summary>
        private void SaveNote()
        {
            int noteId = hfNoteId.Value.AsInteger();
            if ( noteId == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            NoteService noteService = new NoteService( rockContext );
            Note note = noteService.Get( noteId );
            if ( note != null )
            {
                note.Text = dtbText.Text;
                note.Caption = dtbCaption.Text;

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !note.IsValid )
                {
                    // field controls render error messages
                    return;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion
    }
}