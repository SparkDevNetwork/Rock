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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [ContextAware]
    [TextField( 1, "Note Type", "Behavior", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", false, "Notes" )]
    public partial class Notes : RockBlock
    {
        private string contextTypeName = string.Empty;
        private IEntity contextEntity = null;

        private NoteType noteType;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string scriptKey = "add-person-note";
            if ( !this.Page.ClientScript.IsClientScriptBlockRegistered( scriptKey ) )
            {
                string script = @"
Sys.Application.add_load(function () {

    $('.note-add').click(function () {
        $(this).parent().siblings('.note-entry').slideToggle(""slow"");
    });

    $('.person-notes').tinyscrollbar({ size: 150 });

});";
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), scriptKey, script, true );
            }

            btnAddNote.Click += btnAddNote_Click;
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            foreach ( KeyValuePair<string, Rock.Data.IEntity> entry in ContextEntities )
            {
                contextTypeName = entry.Key;
                contextEntity = entry.Value;
                // Should only be one.
                break;
            }

            if ( !String.IsNullOrEmpty( contextTypeName ) && contextEntity != null )
            {
                GetNoteType();
                if ( !Page.IsPostBack )
                    ShowNotes();
            }
        }

        void btnAddNote_Click( object sender, EventArgs e )
        {
            var service = new NoteService();

            var note = new Note();
            note.IsSystem = false;
            note.NoteTypeId = noteType.Id;
            note.EntityId = contextEntity.Id;
            note.Date = DateTime.Now;
            note.Caption = CurrentPerson.FullName;
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

        private void ShowNotes()
        {
            tbNewNote.Text = string.Empty;
            cbAlert.Checked = false;
            cbPrivate.Checked = false;

            phNotes.Controls.Clear();

            var service = new NoteService();

            foreach ( var note in service.Get( noteType.Id, contextEntity.Id ) )
            {
                if ( note.IsAuthorized( "View", CurrentPerson ) )
                {
                    AddNoteHtml( note );
                }
            }
        }

        private void AddNoteHtml( Note note )
        {
            var article = new HtmlGenericControl( "article" );
            phNotes.Controls.Add( article );
            article.AddCssClass( "alert" );

            if (note.IsAlert.HasValue && note.IsAlert.Value)
            {
                article.AddCssClass("alert-error");
            }
            else
            {
                article.AddCssClass("alert-info");
            }

            if ( note.IsPrivate( "View", CurrentPerson ) )
            {
                article.AddCssClass( "personal" );
            }

            var icon = new HtmlGenericControl( "i" );
            article.Controls.Add( icon );

            string iconClassName = "icon-comment";
            if ( note.SourceType != null )
            {
                try
                {
                    if ( note.SourceType.AttributeValues.ContainsKey( "IconClass" ) && note.SourceType.AttributeValues["IconClass"].Count == 1 )
                    {
                        iconClassName = note.SourceType.AttributeValues["IconClass"][0].Value;
                    }
                }
                catch
                {
                }
            }
            icon.AddCssClass( iconClassName );

            var div = new HtmlGenericControl( "div" );
            article.Controls.Add( div );
            div.AddCssClass( "detail" );

            var heading = new HtmlGenericControl( "strong" );
            div.Controls.Add( heading );
            heading.Controls.Add( new LiteralControl( string.Format( "{0} - {1}", note.Date.ToShortDateString(), note.Caption ) ) );

            div.Controls.Add( new LiteralControl( note.Text ) );
        }
    }
}