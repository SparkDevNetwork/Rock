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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Edits a Rock Note.
    /// </summary>
    [ToolboxData( "<{0}:NoteEditor runat=server></{0}:NoteEditor>" )]
    public class NoteEditor : CompositeControl, IDoNotBlockValidate
    {
        #region Fields

        private DropDownList _ddlNoteType;
        private HiddenFieldWithClass _hfHasUnselectableNoteType;
        private ValidationSummary _vsEditNote;
        private RockTextBox _tbNote;
        private CheckBox _cbAlert;
        private CheckBox _cbPrivate;
        private LinkButton _lbSaveNote;

        // NOTE: Intentionally using a HtmlAnchor for security instead of SecurityButton since the URL will need to be set in Javascript
        private HtmlAnchor _aSecurity;

        private DateTimePicker _dtCreateDate;
        private HiddenFieldWithClass _hfParentNoteId;
        private HiddenFieldWithClass _hfNoteId;
        private ModalAlert _mdEditWarning;

        private AttributeValuesContainer _avcNoteAttributes;
        private HiddenFieldWithClass _hfPostBackControlId;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note options.
        /// </summary>
        /// <value>
        /// The note options.
        /// </value>
        public NoteOptions NoteOptions { get; private set; }

        /// <summary>
        /// Sets the note options.
        /// </summary>
        /// <param name="noteOptions">The note options.</param>
        public void SetNoteOptions( NoteOptions noteOptions )
        {
            this.NoteOptions = noteOptions;
            this.BindNoteTypes();
            this.NoteOptions.NoteTypesChange += NoteOptions_NoteTypesChange;
        }

        /// <summary>
        /// Sets the properties of the control based on the specified note
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public void SetNote( Note note )
        {
            EnsureChildControls();
            this.NoteId = note.Id;

            if ( note.NoteTypeId == 0 && this.NoteTypeId.HasValue )
            {
                // if this is a new note, use the default NoteType
                note.NoteTypeId = this.NoteTypeId.Value;
            }
            else
            {
                this.NoteTypeId = note.NoteTypeId;
            }

            this.EntityId = note.EntityId;
            this.CreatedByPersonAlias = note.CreatedByPersonAlias;
            this.Text = note.Text;
            this.IsAlert = note.IsAlert.HasValue && note.IsAlert.Value;
            this.IsPrivate = note.IsPrivateNote;
            this.ParentNoteId = note.ParentNoteId;
            this.CreatedDateTime = note.CreatedDateTime;
            note.LoadAttributes();

            this._hasAttributes = note.Attributes.Any();

            var rockPage = ( this.Page as RockPage ) ?? System.Web.HttpContext.Current.Handler as RockPage;

            this._avcNoteAttributes.AddEditControls( note, Authorization.EDIT, rockPage?.CurrentPerson );
        }

        /// <summary>
        /// Gets or sets a value indicating whether this note has attributes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has attributes; otherwise, <c>false</c>.
        /// </value>
        private bool _hasAttributes
        {
            get => ViewState["_hasAttributes"] as bool? ?? false;
            set => ViewState["_hasAttributes"] = value;
        }

        private string _noteTerm
        {
            get => ViewState["_noteTerm"] as string ?? "Note";
            set => ViewState["_noteTerm"] = value;
        }

        /// <summary>
        /// Sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [Obsolete( "Use SetNote instead" )]
        [RockObsolete( "1.12" )]
        public Rock.Model.Note Note
        {
            set
            {
                SetNote( value );
            }
        }

        /// <summary>
        /// Gets or sets the note type identifier.
        /// </summary>
        /// <value>
        /// The note type identifier.
        /// </value>
        public int? NoteTypeId
        {
            get
            {
                int? noteTypeId = ViewState["NoteTypeId"] as int?;

                if ( !noteTypeId.HasValue || noteTypeId == 0 )
                {
                    var rockPage = ( this.Page as RockPage ) ?? System.Web.HttpContext.Current.Handler as RockPage;
                    var editableNoteTypes = this.NoteOptions.GetEditableNoteTypes( rockPage?.CurrentPerson );
                    if ( editableNoteTypes.Any() )
                    {
                        noteTypeId = editableNoteTypes.First().Id;
                    }
                }

                return noteTypeId ?? 0;
            }

            set
            {
                ViewState["NoteTypeId"] = value;

                EnsureChildControls();
                if ( value.HasValue )
                {
                    _ddlNoteType.SetValue( value.ToString() );
                }
                else
                {
                    _ddlNoteType.SelectedIndex = -1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the note id.
        /// </summary>
        /// <value>
        /// The note id.
        /// </value>
        public int? NoteId
        {
            get
            {
                return _hfNoteId.Value.AsIntegerOrNull();
            }

            set
            {
                _hfNoteId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId
        {
            get { return ViewState["EntityId"] as int?; }
            set { ViewState["EntityId"] = value; }
        }

        /// <summary>
        /// Gets or sets the created by photo identifier.
        /// </summary>
        /// <value>
        /// The created by photo identifier.
        /// </value>
        public int? CreatedByPhotoId
        {
            get { return ViewState["CreatedByPhotoId"] as int?; }
            set { ViewState["CreatedByPhotoId"] = value; }
        }

        /// <summary>
        /// Gets or sets the created by person identifier.
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
        public int? CreatedByPersonId
        {
            get { return ViewState["CreatedByPersonId"] as int?; }
            set { ViewState["CreatedByPersonId"] = value; }
        }

        /// <summary>
        /// Gets or sets the created by gender.
        /// </summary>
        /// <value>
        /// The created by gender.
        /// </value>
        public Gender CreatedByGender
        {
            get
            {
                object gender = this.ViewState["Gender"];
                return gender != null ? ( Gender ) gender : Gender.Male;
            }

            set
            {
                ViewState["Gender"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the label for the note entry box, and the html attributes data-note-term and data-placeholder.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label
        {
            get
            {
                return ViewState["Label"] as string ?? "Note";
            }

            set
            {
                // This is going to be used seperately in editor attributes as "data-note-term"
                _noteTerm = value;

                ViewState["Label"] = value;
                if ( value != null )
                {
                    _tbNote.Placeholder = string.Format( "Add a {0}...", value.ToLower() );
                }
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get
            {
                return _tbNote.Text;
            }

            set
            {
                _tbNote.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the created date time
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime
        {
            get
            {
                EnsureChildControls();
                return _dtCreateDate.SelectedDateTime;
            }

            set
            {
                EnsureChildControls();
                _dtCreateDate.SelectedDateTime = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is alert.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is alert; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlert
        {
            get
            {
                return _cbAlert.Checked;
            }

            set
            {
                _cbAlert.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is private.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate
        {
            get
            {
                return _cbPrivate.Checked;
            }

            set
            {
                _cbPrivate.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent note identifier.
        /// </summary>
        /// <value>
        /// The parent note identifier.
        /// </value>
        public int? ParentNoteId
        {
            get
            {
                return _hfParentNoteId.Value.AsIntegerOrNull();
            }

            set
            {
                _hfParentNoteId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit
        {
            get
            {
                return ViewState["CanEdit"] as bool? ?? false;
            }

            set
            {
                ViewState["CanEdit"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is editing; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditing
        {
            get
            {
                return ViewState["IsEditing"] as bool? ?? false;
            }

            set
            {
                ViewState["IsEditing"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether start this note in Edit mode instead of View mode
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show edit]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowEditMode
        {
            get
            {
                return ViewState["ShowEditMode"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowEditMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the created by person alias.
        /// </summary>
        /// <value>
        /// The created by person alias.
        /// </value>
        public PersonAlias CreatedByPersonAlias
        {
            get
            {
                return _createdByPersonAlias;
            }

            set
            {
                _createdByPersonAlias = value;
                this.CreatedByPhotoId = _createdByPersonAlias?.Person?.PhotoId;
                this.CreatedByGender = _createdByPersonAlias?.Person?.Gender ?? Gender.Male;
                this.CreatedByPersonId = _createdByPersonAlias?.Person?.Id;
            }
        }

        private PersonAlias _createdByPersonAlias = null;

        #endregion

        #region Events

        /// <summary>
        /// Handles the NoteTypesChange event of the NoteOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NoteOptions_NoteTypesChange( object sender, EventArgs e )
        {
            BindNoteTypes();
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _tbNote = new RockTextBox();
            _hfNoteId = new HiddenFieldWithClass();
            _tbNote.Placeholder = "Write a " + this._noteTerm.ToLower().Trim() + "...";
            _ddlNoteType = new DropDownList();
            _hfHasUnselectableNoteType = new HiddenFieldWithClass();
            _cbAlert = new CheckBox();
            _cbPrivate = new CheckBox();
            _lbSaveNote = new LinkButton();
            _aSecurity = new HtmlAnchor();
            _dtCreateDate = new DateTimePicker();
            _hfParentNoteId = new HiddenFieldWithClass();
            _mdEditWarning = new ModalAlert();
            _hfPostBackControlId = new HiddenFieldWithClass();
            _avcNoteAttributes = new AttributeValuesContainer();

            _hfNoteId.ID = this.ID + "_hfNoteId";
            _hfNoteId.CssClass = "js-noteid";
            Controls.Add( _hfNoteId );

            _hfParentNoteId.ID = this.ID + "_hfParentNoteId";
            _hfParentNoteId.CssClass = "js-parentnoteid";
            Controls.Add( _hfParentNoteId );
            _vsEditNote = new ValidationSummary();
            _vsEditNote.ID = this.ID + "_vsEditNote";

            _vsEditNote.CssClass = "alert alert-validation";
            _vsEditNote.HeaderText = "Please correct the following:";
            Controls.Add( _vsEditNote );

            _tbNote.ID = this.ID + "_tbNewNote";
            _tbNote.TextMode = TextBoxMode.MultiLine;
            _tbNote.Rows = 3;
            _tbNote.CssClass = "note-text js-notetext";
            _tbNote.ValidateRequestMode = ValidateRequestMode.Disabled;
            _tbNote.Required = true;
            _tbNote.RequiredFieldValidator.ErrorMessage = "Note is required.";
            Controls.Add( _tbNote );

            _hfPostBackControlId = new HiddenFieldWithClass();
            _hfPostBackControlId.ID = this.ID + "_hfEditNotePostBackScript";
            _hfPostBackControlId.CssClass = "js-postback-control-id";
            Controls.Add( _hfPostBackControlId );

            _avcNoteAttributes.ID = this.ID + "_avcNoteAttributes";
            Controls.Add( _avcNoteAttributes );

            _ddlNoteType.ID = this.ID + "_ddlNoteType";
            _ddlNoteType.CssClass = "form-control input-sm input-width-lg noteentry-notetype js-notenotetype";
            _ddlNoteType.DataValueField = "Id";
            _ddlNoteType.DataTextField = "Name";
            _ddlNoteType.AutoPostBack = true;
            _ddlNoteType.SelectedIndexChanged += _ddlNoteType_SelectedIndexChanged;
            Controls.Add( _ddlNoteType );

            _hfHasUnselectableNoteType.ID = this.ID + "_hfHasUnselectableNoteType";
            _hfHasUnselectableNoteType.CssClass = "hidden js-has-unselectable-notetype";
            Controls.Add( _hfHasUnselectableNoteType );

            _cbAlert.ID = this.ID + "_cbAlert";
            _cbAlert.Text = "Alert";
            _cbAlert.CssClass = "js-notealert";
            Controls.Add( _cbAlert );

            _cbPrivate.ID = this.ID + "_cbPrivate";
            _cbPrivate.Text = "Private";
            _cbPrivate.CssClass = "js-noteprivate";
            Controls.Add( _cbPrivate );

            _mdEditWarning.ID = this.ID + "_mdEditWarning";
            Controls.Add( _mdEditWarning );

            _lbSaveNote.ID = this.ID + "_lbSaveNote";
            _lbSaveNote.Attributes["class"] = "btn btn-primary btn-xs";
            _lbSaveNote.CausesValidation = true;
            _lbSaveNote.Click += lbSaveNote_Click;

            Controls.Add( _lbSaveNote );

            _aSecurity.ID = "_aSecurity";
            _aSecurity.Attributes["class"] = "btn btn-security btn-xs btn-square security js-notesecurity";
            _aSecurity.Attributes["data-entitytype-id"] = EntityTypeCache.Get( typeof( Rock.Model.Note ) ).Id.ToString();
            _aSecurity.InnerHtml = "<i class='fa fa-lock'></i>";
            Controls.Add( _aSecurity );

            _dtCreateDate.ID = this.ID + "_tbCreateDate";
            _dtCreateDate.Label = "Note Created Date";
            _dtCreateDate.AddCssClass( "js-notecreateddate" );
            Controls.Add( _dtCreateDate );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlNoteType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _ddlNoteType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var noteTypeId = _ddlNoteType.SelectedValue.AsIntegerOrNull();
            if ( !noteTypeId.HasValue )
            {
                return;
            }

            // get current edit values from Attribute Controls (prior to changing the NoteType)
            // then store them tempNoteForExistingAttributeValueEdits
            var tempNoteForExistingAttributeValueEdits = new Note();
            tempNoteForExistingAttributeValueEdits.LoadAttributes();
            _avcNoteAttributes.GetEditValues( tempNoteForExistingAttributeValueEdits );

            // create new temp note with the new NoteTypeId
            // any NoteTypeId specific attributes will be loaded in this new temp note
            var tempNoteForNewAttributes = new Note();
            tempNoteForNewAttributes.NoteTypeId = noteTypeId.Value;
            tempNoteForNewAttributes.LoadAttributes();

            // copy the edit values from the tempNoteForExistingAttributeValueEdits into
            // temp note that has the selected noteTypeId (which might have different attributes)
            foreach ( var attribute in tempNoteForExistingAttributeValueEdits.Attributes )
            {
                tempNoteForNewAttributes.SetAttributeValue( attribute.Key, tempNoteForExistingAttributeValueEdits.GetAttributeValue( attribute.Key ) );
            }

            var rockPage = ( this.Page as RockPage ) ?? System.Web.HttpContext.Current.Handler as RockPage;

            _avcNoteAttributes.AddEditControls( tempNoteForNewAttributes, Authorization.EDIT, rockPage?.CurrentPerson );
            _hasAttributes = tempNoteForNewAttributes.Attributes.Any();

            this.IsEditing = true;
        }

        /// <summary>
        /// Binds the note types.
        /// </summary>
        private void BindNoteTypes()
        {
            EnsureChildControls();
            var rockPage = ( this.Page as RockPage ) ?? System.Web.HttpContext.Current.Handler as RockPage;
            var editableNoteTypes = this.NoteOptions.GetEditableNoteTypes( rockPage?.CurrentPerson );
            _ddlNoteType.DataSource = editableNoteTypes;
            _ddlNoteType.DataBind();
            _ddlNoteType.Visible = editableNoteTypes.Count() > 1;
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( IsEditing )
            {
                ScriptManager.GetCurrent( this.Page ).SetFocus( this._tbNote );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( !this.Visible )
            {
                return;
            }

            var script =
$@"Rock.controls.noteEditor.initialize({{
    id: '{this.ClientID}',
    currentNoteId: {this.NoteId ?? 0},
    parentNoteId: {this.ParentNoteId ?? 0},
    isEditing: {this.IsEditing.ToJavaScriptValue()},
}});";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "noteEditor-script" + this.ClientID, script, true );

            // Add Note Validation Group here since the ClientID is now resolved
            AddNoteValidationGroup();

            StringBuilder noteCss = new StringBuilder();

            noteCss.Append( "note-editor js-note-editor meta" );
            if ( _hasAttributes )
            {
                noteCss.Append( " note-editor-attributes" );
                _tbNote.Label = this._noteTerm;
                _tbNote.FormGroupCssClass = "note-editor-text";
            }
            else
            {
                noteCss.Append( " note-editor-standard" );
                _tbNote.Label = string.Empty;
            }

            if ( !string.IsNullOrEmpty( this.CssClass ) )
            {
                noteCss.Append( " " + this.CssClass );
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, noteCss.ToString() );
            writer.AddAttribute( "data-placeholder", this.Label );
            writer.AddAttribute( "data-note-term", this._noteTerm );
            if ( this.Style[HtmlTextWriterStyle.Display] != null )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, this.Style[HtmlTextWriterStyle.Display] );
            }

            if ( !ShowEditMode )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            if ( this.NoteId.HasValue )
            {
                writer.AddAttribute( "rel", this.NoteId.Value.ToStringSafe() );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Edit Mode HTML...
            if ( NoteOptions.DisplayType == NoteDisplayType.Full && NoteOptions.UsePersonIcon )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "meta-figure" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "avatar avatar-lg" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                var getPersonPhotoImageTagArgs = new GetPersonPhotoImageTagArgs()
                {
                    PhotoId = CreatedByPhotoId,
                    Gender = CreatedByGender,
                    MaxHeight = 50,
                    MaxWidth = 50
                };

                writer.Write( Rock.Model.Person.GetPersonPhotoImageTag( this.CreatedByPersonId, getPersonPhotoImageTagArgs ) );

                writer.RenderEndTag(); // avatar div
                writer.RenderEndTag(); // meta-figure div
            }

            if ( IsEditing )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "meta-body js-focus-within focus-within" );
            }
            else
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "meta-body js-focus-within" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "noteentry-control" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _vsEditNote.RenderControl( writer );
            _tbNote.RenderControl( writer );

            _avcNoteAttributes.RenderControl( writer );

            _hfNoteId.RenderControl( writer );
            _hfParentNoteId.RenderControl( writer );

            _hfPostBackControlId.Value = this.ParentUpdatePanel()?.ClientID;
            _hfPostBackControlId.RenderControl( writer );

            writer.RenderEndTag();

            // Options
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "settings clearfix" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // The optional create date text box if ShowCreateDateInput is enabled.
            // This allows back-dating of existing notes
            if ( NoteOptions.ShowCreateDateInput)
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "createDate" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dtCreateDate.RenderControl( writer );
                writer.RenderEndTag();  // createDate div
            }

            _hfHasUnselectableNoteType.RenderControl( writer );
            _ddlNoteType.RenderControl( writer );

            if ( NoteOptions.DisplayType == NoteDisplayType.Full )
            {
                if ( NoteOptions.ShowAlertCheckBox )
                {
                    _cbAlert.RenderControl( writer );
                }

                if ( NoteOptions.ShowPrivateCheckBox )
                {
                    _cbPrivate.RenderControl( writer );
                }
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "commands" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _mdEditWarning.RenderControl( writer );

            // Don't show the security button when adding because the ID is not set.
            if ( NoteOptions.DisplayType == NoteDisplayType.Full && NoteOptions.ShowSecurityButton && this.NoteId.IsNotNullOrZero() )
            {
                _aSecurity.Attributes["data-title"] = this.Label;
                _aSecurity.Attributes["data-entity-id"] = this.NoteId.ToString();
                _aSecurity.RenderControl( writer );
            }

            if ( !NoteOptions.AddAlwaysVisible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note-cancel js-editnote-cancel btn btn-link btn-xs" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( "Cancel" );
                writer.RenderEndTag();
            }

            _lbSaveNote.Text = "Save " + Label;
            _lbSaveNote.CommandName = "SaveNote";
            _lbSaveNote.CommandArgument = this.NoteId.ToString();
            _lbSaveNote.RenderControl( writer );

            writer.RenderEndTag();  // commands div

            writer.RenderEndTag();  // settings div

            writer.RenderEndTag();  // panel body

            writer.RenderEndTag(); // ????
        }

        /// <summary>
        /// Adds the note validation group.
        /// ValidationGroups for Notes needs to a group for each
        /// Note, this is called from RenderControl
        /// So that ClientID is fully Qualified
        /// </summary>
        private void AddNoteValidationGroup()
        {
            string validationGroup = $"vgNoteEdit_{this.ClientID}";
            _vsEditNote.ValidationGroup = validationGroup;
            _tbNote.ValidationGroup = validationGroup;
            _lbSaveNote.ValidationGroup = validationGroup;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSaveNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveNote_Click( object sender, EventArgs e )
        {
            if ( _ddlNoteType.Visible )
            {
                NoteTypeId = _ddlNoteType.SelectedValueAsInt() ?? 0;
            }

            var rockPage = this.Page as RockPage;

            if ( rockPage == null || !NoteTypeId.HasValue )
            {
                return;
            }

            var currentPerson = rockPage.CurrentPerson;

            var rockContext = new RockContext();
            var service = new NoteService( rockContext );
            Note note = null;

            if ( NoteId.HasValue )
            {
                note = service.Get( NoteId.Value );
            }

            bool isNew = false;
            if ( note == null )
            {
                note = new Note();
                note.IsSystem = false;
                note.EntityId = EntityId;
                note.ParentNoteId = _hfParentNoteId.Value.AsIntegerOrNull();
                service.Add( note );
                isNew = true;
            }
            else
            {
                if ( !note.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    // if somehow a person is trying to edit a note that they aren't authorized to edit, don't update the note
                    _mdEditWarning.Show( "Not authorized to edit note", ModalAlertType.Warning );
                    return;
                }
            }

            if ( isNew || ( note.CreatedByPersonId.HasValue && currentPerson.Id == note.CreatedByPersonId ) )
            {
                note.IsPrivateNote = IsPrivate;
            }

            if ( _hfHasUnselectableNoteType.Value.AsBoolean() && note.NoteTypeId > 0 && note.Id > 0 )
            {
                // a note type with an UnSelectable NoteType was edited, so just keep the NoteType that it had before
            }
            else
            {
                note.NoteTypeId = NoteTypeId.Value;
            }

            string personalNoteCaption = "You - Personal Note";
            if ( string.IsNullOrWhiteSpace( note.Caption ) )
            {
                note.Caption = IsPrivate ? personalNoteCaption : string.Empty;
            }
            else
            {
                // if the note still has the personalNoteCaption, but was changed to have IsPrivateNote to false, change the caption to empty string
                if ( note.Caption == personalNoteCaption && !IsPrivate )
                {
                    note.Caption = string.Empty;
                }
            }

            note.Text = Text;
            note.IsAlert = IsAlert;
            _avcNoteAttributes.GetEditValues( note );

            if ( NoteOptions.ShowCreateDateInput )
            {
                note.CreatedDateTime = _dtCreateDate.SelectedDateTime;
            }

            note.EditedByPersonAliasId = currentPerson?.PrimaryAliasId;
            note.EditedDateTime = RockDateTime.Now;
            note.NoteUrl = this.RockBlock()?.CurrentPageReference?.BuildUrl();
#pragma warning disable CS0618 // Type or member is obsolete
            // Set this so anything doing direct SQL queries will still find
            // the right set of notes.
            note.ApprovalStatus = NoteApprovalStatus.Approved;
#pragma warning restore CS0618 // Type or member is obsolete

            rockContext.SaveChanges();
            Rock.Attribute.Helper.SaveAttributeValues( note );
            this.IsEditing = false;

            if ( SaveButtonClick != null )
            {
                SaveButtonClick( this, new NoteEventArgs( note.Id ) );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when [save button click].
        /// </summary>
        public event EventHandler<NoteEventArgs> SaveButtonClick;

        #endregion
    }

    /// <summary>
    /// Note Event Argument includes id of note updated
    /// </summary>
    public class NoteEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the note identifier.
        /// </summary>
        /// <value>
        /// The note identifier.
        /// </value>
        public int? NoteId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteEventArgs"/> class.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        public NoteEventArgs( int? noteId )
        {
            NoteId = noteId;
        }
    }

    #region Enums

    /// <summary>
    ///
    /// </summary>
    public enum NoteDisplayType
    {
        /// <summary>
        /// The full
        /// </summary>
        Full,

        /// <summary>
        /// The light
        /// </summary>
        Light
    }

    #endregion
}
