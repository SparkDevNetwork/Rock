//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a Rock Note
    /// </summary>
    [ToolboxData( "<{0}:NoteEditor runat=server></{0}:NoteEditor>" )]
    public class NoteEditor : CompositeControl
    {
        private RockTextBox _tbNote;
        private CheckBox _cbAlert;
        private CheckBox _cbPrivate;
        private LinkButton _lbSaveNote;
        private LinkButton _lbEditNote;
        private LinkButton _lbDeleteNote;

        public Rock.Model.Note Note
        {
            set
            {
                this.NoteId = value.Id;
                this.SourceTypeValueId = value.SourceTypeValueId;
                this.Caption = value.Caption;
                this.CreatedDateTime = value.CreationDateTime;
                this.Text = value.Text;
                this.IsAlert = value.IsAlert.HasValue && value.IsAlert.Value;
            }
        }

        public int NoteId
        {
            get { return ViewState["NoteId"] as int? ?? 0; }
            set { ViewState["NoteId"] = value; }
        }

        public int? SourceTypeValueId
        {
            get { return ViewState["SourceTypeValueId"] as int?; }
            set { ViewState["SourceTypeValueId"] = value; }
        }

        public string Caption
        {
            get { return ViewState["Caption"] as string ?? string.Empty; }
            set { ViewState["Caption"] = value; }
        }

        public DateTime? CreatedDateTime
        {
            get { return ViewState["CreatedDateTime"] as DateTime?; }
            set { ViewState["CreatedDateTime"] = value; }
        }

        public string Text
        {
            get { return _tbNote.Text; }
            set { _tbNote.Text = value; }
        }

        public bool IsAlert
        {
            get { return _cbAlert.Checked; }
            set { _cbAlert.Checked = value; }
        }

        public bool IsPrivate
        {
            get { return _cbPrivate.Checked; }
            set { _cbPrivate.Checked = value; }
        }

        public bool CanEdit
        {
            get { return ViewState["CanEdit"] as bool? ?? false; }
            set { ViewState["CanEdit"] = value; }
        }

        public bool ShowAlertCheckBox
        {
            get { return ViewState["ShowAlertCheckBox"] as bool? ?? false; }
            set { ViewState["ShowAlertCheckBox"] = value; }
        }

        public bool ShowPrivateCheckBox
        {
            get { return ViewState["ShowPrivateCheckBox"] as bool? ?? false; }
            set { ViewState["ShowPrivateCheckBox"] = value; }
        }

        public bool ShowSecurityButton
        {
            get { return ViewState["ShowSecurityButton"] as bool? ?? false; }
            set { ViewState["ShowSecurityButton"] = value; }
        }

        private string ArticleClass
        {
            get
            {
                if ( IsAlert )
                {
                    return "clearfix highlight";
                }

                if ( IsPrivate )
                {
                    return "clearfix personal";
                }

                return "clearfix";
            }
        }

        private string IconClass
        {
            get
            {
                if ( SourceTypeValueId.HasValue )
                {
                    var SourceType = DefinedValueCache.Read( SourceTypeValueId.Value );
                    if ( SourceType != null &&
                    SourceType.AttributeValues != null &&
                    SourceType.AttributeValues.ContainsKey( "IconClass" ) &&
                    SourceType.AttributeValues["IconClass"].Count == 1 )
                    {
                        return SourceType.AttributeValues["IconClass"][0].Value;
                    }
                }

                return "icon-comment";
            }

        }

        public NoteEditor()
        {
            _tbNote = new RockTextBox();
            _cbAlert = new CheckBox();
            _cbPrivate = new CheckBox();
            _lbSaveNote = new LinkButton();
            _lbEditNote = new LinkButton();
            _lbDeleteNote = new LinkButton();
        }

        protected override void OnInit( EventArgs e )
        {
            string script = @"
    $('a.edit-note').click(function (e) {
        e.preventDefault();
        $(this).parent().parent().parent().children().slideToggle('slow');
    });
    $('a.edit-note-cancel').click(function () {
        $(this).parent().parent().parent().children().slideToggle('slow');
    });
    $('a.remove-note').click(function() {
        return confirm('Are you sure you want to delete this note?');
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "edit-note", script, true );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _tbNote.ID = this.ID + "_tbNewNote";
            _tbNote.TextMode = TextBoxMode.MultiLine;
            Controls.Add(_tbNote);

            _cbAlert.ID = this.ID + "_cbAlert";
            _cbAlert.Text = "Alert";
            Controls.Add(_cbAlert);

            _cbPrivate.ID = this.ID + "_cbPrivate";
            _cbPrivate.Text = "Private";
            Controls.Add(_cbPrivate);

            _lbSaveNote.ID = this.ID + "_lbSaveNote";
            _lbSaveNote.Attributes["class"] = "btn btn-primary btn-xs";
            _lbSaveNote.Text = "Save Note";
            _lbSaveNote.Click += lbSaveNote_Click;

            Controls.Add(_lbSaveNote);

            var iEdit = new HtmlGenericControl( "i" );
            iEdit.Attributes["class"] = "icon-pencil";
            _lbEditNote.Controls.Add( iEdit );

            _lbDeleteNote.ID = this.ID + "_lbDeleteNote";
            _lbDeleteNote.Attributes["class"] = "remove-note";
            _lbDeleteNote.Click += lbDeleteNote_Click;
            Controls.Add( _lbDeleteNote );

            var iDelete = new HtmlGenericControl( "i" );
            iDelete.Attributes["class"] = "icon-remove";
            _lbDeleteNote.Controls.Add( iDelete );
        }

        public event EventHandler SaveButtonClick;
        protected void lbSaveNote_Click( object sender, EventArgs e )
        {
            if ( SaveButtonClick != null )
            {
                SaveButtonClick( this, e );
            }
        }

        public event EventHandler DeleteButtonClick;
        protected void lbDeleteNote_Click( object sender, EventArgs e )
        {
            if ( DeleteButtonClick != null )
            {
                DeleteButtonClick( this, e );
            }
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Edit Mode HTML...
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "panel panel-noteentry");
            writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // label and text
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbNote.Label = "Note";
            _tbNote.Text = Text;
            _tbNote.RenderControl( writer );
            

            // Options
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "settings clearfix" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "options pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( ShowAlertCheckBox )
            {
                _cbAlert.RenderControl( writer );
            }

            if ( ShowPrivateCheckBox )
            {
                _cbPrivate.RenderControl( writer );
            }

            writer.RenderEndTag();

            if ( ShowSecurityButton )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-xs security pull-right" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "button" );
                writer.RenderBeginTag( HtmlTextWriterTag.Button );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-lock" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.Write( " Security" );
                writer.RenderEndTag();
            }

            writer.RenderEndTag();  // settings div
            writer.RenderEndTag();  // panel body

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-footer" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _lbSaveNote.RenderControl( writer );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note-cancel btn btn-xs" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.Write( "Cancel" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderEndTag();  // note-entry div

            // View Mode HTML...
            writer.AddAttribute( HtmlTextWriterAttribute.Class, ArticleClass );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, IconClass );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "details" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Heading
            writer.RenderBeginTag( HtmlTextWriterTag.H5 );
            writer.Write( Caption );
            if ( CreatedDateTime.HasValue )
            {
                writer.Write( " " );
                writer.AddAttribute("class", "date");
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( CreatedDateTime.Value.ToRelativeDateString( 6 ) );
                writer.RenderEndTag();
            }
            writer.RenderEndTag();

            writer.Write( Text );

            writer.RenderEndTag();  // Details Div

            if ( CanEdit )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note" );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-pencil" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();  // A
                _lbDeleteNote.RenderControl( writer );
                writer.RenderEndTag();  // actions
            }

            writer.RenderEndTag();  // article

            writer.RenderEndTag();
        }

    }

    public class IdEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the index of the row that fired the event
        /// </summary>
        /// <value>
        /// The index of the row.
        /// </value>
        public int Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowEventArgs" /> class.
        /// </summary>
        /// <param name="row">The row.</param>
        public IdEventArgs( int id )
        {
            Id = id;
        }
    }
}
