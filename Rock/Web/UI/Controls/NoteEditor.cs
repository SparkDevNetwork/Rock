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

        /// <summary>
        /// Sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
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

        /// <summary>
        /// Gets or sets the note id.
        /// </summary>
        /// <value>
        /// The note id.
        /// </value>
        public int NoteId
        {
            get { return ViewState["NoteId"] as int? ?? 0; }
            set { ViewState["NoteId"] = value; }
        }

        /// <summary>
        /// Gets or sets the source type value id.
        /// </summary>
        /// <value>
        /// The source type value id.
        /// </value>
        public int? SourceTypeValueId
        {
            get { return ViewState["SourceTypeValueId"] as int?; }
            set { ViewState["SourceTypeValueId"] = value; }
        }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption
        {
            get { return ViewState["Caption"] as string ?? string.Empty; }
            set { ViewState["Caption"] = value; }
        }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime
        {
            get { return ViewState["CreatedDateTime"] as DateTime?; }
            set { ViewState["CreatedDateTime"] = value; }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return _tbNote.Text; }
            set { _tbNote.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is alert.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alert; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlert
        {
            get { return _cbAlert.Checked; }
            set { _cbAlert.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is private.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate
        {
            get { return _cbPrivate.Checked; }
            set { _cbPrivate.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit
        {
            get { return ViewState["CanEdit"] as bool? ?? false; }
            set { ViewState["CanEdit"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show alert check box].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show alert check box]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAlertCheckBox
        {
            get { return ViewState["ShowAlertCheckBox"] as bool? ?? false; }
            set { ViewState["ShowAlertCheckBox"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show private check box].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show private check box]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrivateCheckBox
        {
            get { return ViewState["ShowPrivateCheckBox"] as bool? ?? false; }
            set { ViewState["ShowPrivateCheckBox"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show security button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show security button]; otherwise, <c>false</c>.
        /// </value>
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
                    return "clearfix highlight rollover-container";
                }

                if ( IsPrivate )
                {
                    return "clearfix personal rollover-container";
                }

                return "clearfix rollover-container";
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEditor"/> class.
        /// </summary>
        public NoteEditor()
        {
            _tbNote = new RockTextBox();
            _cbAlert = new CheckBox();
            _cbPrivate = new CheckBox();
            _lbSaveNote = new LinkButton();
            _lbEditNote = new LinkButton();
            _lbDeleteNote = new LinkButton();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
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

        /// <summary>
        /// Occurs when [save button click].
        /// </summary>
        public event EventHandler SaveButtonClick;

        /// <summary>
        /// Handles the Click event of the lbSaveNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveNote_Click( object sender, EventArgs e )
        {
            if ( SaveButtonClick != null )
            {
                SaveButtonClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete button click].
        /// </summary>
        public event EventHandler DeleteButtonClick;

        /// <summary>
        /// Handles the Click event of the lbDeleteNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteNote_Click( object sender, EventArgs e )
        {
            if ( DeleteButtonClick != null )
            {
                DeleteButtonClick( this, e );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "note-editor" );
            writer.AddAttribute( "rel", this.NoteId.ToStringSafe() );
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-security btn-xs security pull-right" );
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions rollover-item" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _lbDeleteNote.RenderControl(writer);

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note" );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-pencil" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();  // A

                
                writer.RenderEndTag();  // actions
            }

            writer.RenderEndTag();  // article

            writer.RenderEndTag();
        }

    }

    /// <summary>
    /// Event argument to track which note fired an event
    /// </summary>
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
        /// <param name="id">The id.</param>
        public IdEventArgs( int id )
        {
            Id = id;
        }
    }
}
