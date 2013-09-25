//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [ContextAware]
    [TextField( "Note Type", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", false, "Notes" )]
    [BooleanField( "Show Alert Checkbox", "", true)]
    [BooleanField( "Show Private Checkbox", "", true )]
    [BooleanField( "Show Security Button", "", true )]
    public partial class Notes : RockBlock
    {
        private IEntity contextEntity = null;
        private NoteType noteType;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            btnAddNote.Click += btnAddNote_Click;
            rptNotes.ItemDataBound += rptNotes_ItemDataBound;

            cbAlert.Visible = GetAttributeValue( "ShowAlertCheckbox" ).AsBoolean();
            cbPrivate.Visible = GetAttributeValue( "ShowPrivateCheckbox" ).AsBoolean();
            btnSecurity.Visible = GetAttributeValue( "ShowSecurityButton" ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            contextEntity = this.ContextEntity();

            if ( contextEntity != null )
            {
                GetNoteType();
                if ( !Page.IsPostBack )
                    ShowNotes();
            }

            string script = @"
    $('a.add-note').click(function () {
        $(this).parent().siblings('.panel-body').children('.note-entry').slideToggle(""slow"");
    });
    
    $('a.add-note-cancel').click(function () {
        $(this).parent().siblings('.note').children('textarea').val('');
        $(this).parent().parent().slideToggle(""slow"");
    });

    $('.panel-notes article').live({
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
        /// Handles the Click event of the btnAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void btnAddNote_Click( object sender, EventArgs e )
        {
            var service = new NoteService();

            var note = new Note();
            note.IsSystem = false;
            note.NoteTypeId = noteType.Id;
            note.EntityId = contextEntity.Id;
            note.CreationDateTime = DateTime.Now;
            note.Caption = cbPrivate.Checked ? "You - Personal Note" : CurrentPerson.FullName;
            note.IsAlert = cbAlert.Checked;
            note.Text = tbNewNote.Text;

            if ( noteType.Sources != null )
            {
                var source = noteType.Sources.DefinedValues.FirstOrDefault();
                if ( source != null )
                {
                    note.SourceTypeValueId = source.Id;
                }
            }

            service.Add( note, CurrentPersonId );
            service.Save( note, CurrentPersonId );

            if ( cbPrivate.Checked )
            {
                note.MakePrivate( "View", CurrentPerson, CurrentPersonId );
            }

            ShowNotes();
        }

        void rptNotes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var note = e.Item.DataItem as Rock.Model.Note;
                var control = e.Item.FindControl( "noteEditor" );
                if ( note != null && control != null )
                {
                    var noteEditor = control as NoteEditor;
                    noteEditor.IsPrivate = note.IsPrivate( "View", CurrentPerson );
                    noteEditor.CanEdit = note.IsAuthorized( "Edit", CurrentPerson );
                    noteEditor.ShowAlertCheckBox = cbAlert.Visible;
                    noteEditor.ShowPrivateCheckBox = cbPrivate.Visible;
                    noteEditor.ShowSecurityButton = btnSecurity.Visible;
                }
            }
        }

        /// <summary>
        /// Handles the SaveButtonClick event of the Note control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void noteEditor_SaveButtonClick( object sender, EventArgs e )
        {
            var noteEditor = sender as NoteEditor;
            if ( noteEditor != null )
            {
                var service = new NoteService();

                var note = service.Get( noteEditor.NoteId );
                if ( note != null && note.IsAuthorized( "Edit", CurrentPerson ) )
                {
                    note.Caption = noteEditor.IsPrivate ? "You - Personal Note" : CurrentPerson.FullName;
                    note.IsAlert = noteEditor.IsAlert;
                    note.Text = noteEditor.Text;

                    service.Save( note, CurrentPersonId );

                    if ( noteEditor.IsPrivate && !note.IsPrivate("View", CurrentPerson) )
                    {
                        note.MakePrivate( "View", CurrentPerson, CurrentPersonId );
                    }
                }

                ShowNotes();
            }
        }

        /// <summary>
        /// Handles the DeleteButtonClick event of the Note control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void noteEditor_DeleteButtonClick( object sender, EventArgs e )
        {
            var noteEditor = sender as NoteEditor;
            if ( noteEditor != null )
            {
                var service = new NoteService();

                var note = service.Get( noteEditor.NoteId );
                if ( note != null && note.IsAuthorized( "Edit", CurrentPerson ) )
                {
                    service.Delete( note, CurrentPersonId );
                    service.Save( note, CurrentPersonId );
                }

                ShowNotes();

            }
        }

        /// <summary>
        /// Handles the Click event of the lbShowMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbShowMore_Click( object sender, EventArgs e )
        {
            int displayCount = 10;
            if ( !int.TryParse( hfDisplayCount.Value, out displayCount ) )
            {
                displayCount = 10;
            }

            ShowNotes();
        }

        /// <summary>
        /// Gets the type of the note.
        /// </summary>
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

            lTitle.Text = noteType.Name;
        }

        /// <summary>
        /// Shows the notes.
        /// </summary>
        private void ShowNotes()
        {
            tbNewNote.Text = string.Empty;
            cbAlert.Checked = false;
            cbPrivate.Checked = false;

            int displayCount = 10;
            if (!int.TryParse(hfDisplayCount.Value, out displayCount))
            {
                displayCount = 10;
            }
            lbShowMore.Visible = false;

            var notes = new List<Note>();
            foreach ( var note in new NoteService().Get( noteType.Id, contextEntity.Id ) )
            {
                if ( note.IsAuthorized( "View", CurrentPerson ) )
                {
                    if ( notes.Count < displayCount )
                    {
                        notes.Add( note );
                    }
                    else
                    {
                        lbShowMore.Visible = true;
                        break;
                    }
                }
            }

            rptNotes.DataSource = notes;
            rptNotes.DataBind();
        }

        /// <summary>
        /// Gets the note class.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        protected string GetNoteClass( object dataItem )
        {
            var note = dataItem as Note;
            if ( note != null )
            {
                if ( note.IsAlert.HasValue && note.IsAlert.Value )
                {
                    return " highlight";
                }

                if ( note.IsPrivate( "View", CurrentPerson ) )
                {
                    return " personal";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the icon class.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        protected string GetIconClass( object dataItem )
        {
            var note = dataItem as Note;
            if ( note != null )
            {
                if ( note.SourceType != null )
                {
                    try
                    {
                        if ( note.SourceType.AttributeValues.ContainsKey( "IconClass" ) && 
                            note.SourceType.AttributeValues["IconClass"].Count == 1 )
                        {
                            return note.SourceType.AttributeValues["IconClass"][0].Value;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return "icon-comment";
        }
    }
}