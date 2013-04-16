//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [ContextAware(typeof(PrayerRequest))]
    [TextField( "Note Type", "The note type name associated with the context entity to use (If it doesn't exist it will be created. Default is 'Prayer Comment').", false, "Prayer Comment", "Behavior", 0, "NoteType" )]
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
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            string noteId = PageParameter( PrayerCommentKeyParameter );
            
            if ( !Page.IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( noteId ) )
                {
                    // Set up to scroll to the top of the given note...
                    string script = string.Format(
                        @"$(document).ready(function() {{
                            $(""html, body"").animate({{scrollTop: $(""[rel='{0}']"").offset().top}}, {{ duration: 'slow', easing: 'swing'}});
                        }});", noteId );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "scroll-to-comment-{0}", this.ClientID ), script, true );

                    prayerComment = new NoteService().Get( int.Parse( noteId ) );
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
                ShowEditDetails(prayerComment); 
            }
        }

        private void GetNoteType()
        {
            string noteTypeName = GetAttributeValue( "NoteType" );

            var service = new NoteTypeService();
            noteType = service.Get( contextEntity.TypeId, noteTypeName );

            // If a note type with the specified name does not exist for the context entity type, create one
            if ( noteType == null )
            {
                noteType = new NoteType();
                noteType.IsSystem = false;
                noteType.EntityTypeId = contextEntity.TypeId;
                noteType.EntityTypeQualifierColumn = string.Empty;
                noteType.EntityTypeQualifierValue = string.Empty;
                noteType.Name = noteTypeName;
                service.Add( noteType, CurrentPersonId );
                service.Save( noteType, CurrentPersonId );
            }
        }

        private void ShowNotes()
        {
            //tbNewNote.Text = string.Empty;
            //cbAlert.Checked = false;
            //cbPrivate.Checked = false;

            phNotesBefore.Controls.Clear();
            phNotesAfter.Controls.Clear();

            var service = new NoteService();

            foreach ( var note in service.Get( noteType.Id, contextEntity.Id ) )
            {
                if ( note.IsAuthorized( "View", CurrentPerson ) )
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
        ///    <i class="icon-comment"></i>
	    ///      <div class="detail"> <strong>Name/Caption</strong> <span class="muted">relative date</span><br>
        ///          <p>text</p>
        ///      </div>
        ///   </article>
        /// </summary>
        /// <param name="note">The note.</param>
        private void AddNoteHtml( Note note )
        {
            var article = new HtmlGenericControl( "article" );
            PlaceHolder phNotes = ( useTheAfterPH ) ? phNotesAfter : phNotesBefore;
            phNotes.Controls.Add( article );
            article.AddCssClass( "alert alert-info" );

            // The comment icon or possibly a picture of the person who made the comment...
            article.Controls.Add( new LiteralControl( "<i class='icon-comment'></i> " ) );

            var divDetail = new HtmlGenericControl( "div" );
            article.Controls.Add( divDetail );
            divDetail.AddCssClass( "detail" );

            // Add the name/caption
            divDetail.Controls.Add( new LiteralControl( string.Format("<strong>{0}</strong> <span class='muted'>{1}</span>", note.Caption, note.CreationDateTime.ToRelativeDateString() ) ) );

            var pText = new HtmlGenericControl( "p" );
            divDetail.Controls.Add( pText );
            pText.Controls.Add( new LiteralControl( HttpUtility.HtmlEncode( note.Text ) ) );
        }

        /// <summary>
        /// Handles the edit Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( prayerComment );
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( PrayerCommentKeyParameter ) )
            {
                return;
            }

            //PrayerRequest prayerRequest;

            //if ( !itemKeyValue.Equals( 0 ) )
            //{
            //    prayerRequest = new PrayerRequestService().Get( itemKeyValue );
            //}
            //else
            //{
            //    prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = true, AllowComments = true };
            //}

            //hfPrayerRequestId.Value = prayerRequest.Id.ToString();

            //// render UI based on Authorized and IsSystem
            //bool readOnly = false;

            //nbEditModeMessage.Text = string.Empty;
            //if ( !IsUserAuthorized( "Edit" ) )
            //{
            //    readOnly = true;
            //    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PrayerRequest.FriendlyTypeName );
            //}

            //if ( readOnly )
            //{
            //    //btnEdit.Visible = false;
            //    ShowReadonlyDetails( prayerRequest );
            //}
            //else
            //{
            //    if ( prayerRequest.Id > 0 )
            //    {
            //        ShowReadonlyDetails( prayerRequest );
            //    }
            //    else
            //    {
            //        ShowEditDetails( prayerRequest );
            //    }
            //}
        }
        #endregion

        #region View & Edit Details

        private void ShowReadonlyDetails( PrayerRequest prayerRequest )
        {
            SetEditMode( false );

            //litFullName.Text = prayerRequest.FullName;
            //litCategory.Text = prayerRequest.Category.Name;
            //litRequest.Text = HttpUtility.HtmlEncode( prayerRequest.Text );

            //ShowStatus( prayerRequest, this.CurrentPerson, litFlaggedMessageRO );
            //ShowPrayerCount( prayerRequest, litPrayerCountRO );

            //litStatus.Text = ( ! prayerRequest.IsApproved ?? false ) ? "<dt><span class='label label-important'>unapproved</span></dt>" : "";
            //litUrgent.Text = ( prayerRequest.IsUrgent ?? false ) ? "<dt><span class='label label-info'><i class='icon-exclamation-sign'></i> urgent</span></dt>" : "";

        }

        private void ShowEditDetails( Note prayerComment )
        {
            SetEditMode(true);

            useTheAfterPH = true;
            fieldsetEditDetails.Visible = true;
            hfNoteId.Value = prayerComment.Id.ToString();

            tbText.Text = prayerComment.Text;
            tbCaption.Text = prayerComment.Caption;
            tbText.Attributes.Add( "rel", prayerComment.Id.ToString() );
        }

        private void SetEditMode(bool enableEdit)
        {
            fieldsetEditDetails.Visible = enableEdit;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveNote();
            NavigateToParentPage();     
        }

        private void SaveNote()
        {
            int noteId = 0;
            if ( ! int.TryParse( hfNoteId.Value, out noteId ) )
            {
                return;
            }

            Note note = new Note();
            NoteService noteService = new NoteService();
            note = noteService.Get( noteId );

            note.Text = tbText.Text;
            note.Caption = tbCaption.Text;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !note.IsValid )
            {
                // field controls render error messages
                return;
            }

            noteService.Save( note, CurrentPersonId );
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();       
        }
        #endregion
    }
}