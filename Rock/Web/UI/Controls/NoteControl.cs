﻿// <copyright>
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
    /// Displays a Rock Note.
    /// </summary>
    [ToolboxData( "<{0}:NoteControl runat=server></{0}:NoteControl>" )]
    public class NoteControl : CompositeControl
    {

        #region Fields

        private RockTextBox _tbNote;
        private CheckBox _cbAlert;
        private CheckBox _cbPrivate;
        private LinkButton _lbSaveNote;
        private LinkButton _lbEditNote;
        private LinkButton _lbDeleteNote;
        private SecurityButton _sbSecurity;

        #endregion

        #region Properties

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
                this.NoteTypeId = value.NoteTypeId;
                this.EntityId = value.EntityId;
                this.SourceTypeValueId = value.SourceTypeValueId;
                this.Caption = value.Caption;

                if (value.CreatedByPersonAlias != null && value.CreatedByPersonAlias.Person != null)
                {
                    this.CreatedByName = value.CreatedByPersonAlias.Person.FullName;
                    this.CreatedByPhotoId = value.CreatedByPersonAlias.Person.PhotoId;
                    this.CreatedByGender = value.CreatedByPersonAlias.Person.Gender;
                }
                else
                {
                    this.CreatedByName = string.Empty;
                    this.CreatedByPhotoId = null;
                    this.CreatedByGender = Gender.Male;
                }

                this.CreatedDateTime = value.CreatedDateTime;
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
        public int? NoteId
        {
            get { return ViewState["NoteId"] as int?; }
            set { ViewState["NoteId"] = value; }
        }

        /// <summary>
        /// Gets or sets the note type identifier.
        /// </summary>
        /// <value>
        /// The note type identifier.
        /// </value>
        public int? NoteTypeId
        {
            get { return ViewState["NoteTypeId"] as int?; }
            set { ViewState["NoteTypeId"] = value; }
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
        /// Gets or sets a value indicating whether an add view is always visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add always visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AddAlwaysVisible
        {
            get { return ViewState["AddAlwaysVisible"] as bool? ?? false; }
            set { ViewState["AddAlwaysVisible"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the created by.
        /// </summary>
        /// <value>
        /// The name of the created by.
        /// </value>
        public string CreatedByName
        {
            get { return ViewState["CreatedByName"] as string ?? string.Empty; }
            set { ViewState["CreatedByName"] = value; }
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
                return gender != null ? (Gender)gender : Gender.Male;
            }
            set { ViewState["Gender"] = value; }
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
        /// Gets or sets the label for the note entry box
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
                ViewState["Label"] = value;
                if (value != null)
                {
                    _tbNote.Placeholder = string.Format( "Write a {0}...", value.ToLower() );
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
        /// Gets or sets the display type.
        /// </summary>
        /// <value>
        /// The display type.
        /// </value>
        public virtual NoteDisplayType DisplayType
        {
            get
            {
                object displayType = this.ViewState["NoteDisplayType"];
                return displayType != null ? (NoteDisplayType)displayType : NoteDisplayType.Full;
            }

            set
            {
                this.ViewState["NoteDisplayType"] = value;
            }
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

        /// <summary>
        /// Gets or sets a value indicating whether person icon should used instead of icon representing the source.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use person icon]; otherwise, <c>false</c>.
        /// </value>
        public bool UsePersonIcon
        {
            get { return ViewState["UsePersonIcon"] as bool? ?? false; }
            set { ViewState["UsePersonIcon"] = value; }
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
                    SourceType.AttributeValues["IconClass"] != null )
                    {
                        return SourceType.AttributeValues["IconClass"].Value;
                    }
                }

                return "fa fa-comment";
            }

        }

        private string NoteTypeName
        {
            get
            {
                if ( SourceTypeValueId.HasValue )
                {
                    var SourceType = DefinedValueCache.Read( SourceTypeValueId.Value );
                    if ( SourceType != null  )
                    {
                        return SourceType.Value;
                    }
                }

                return string.Empty;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteControl"/> class.
        /// </summary>
        public NoteControl()
        {
            _tbNote = new RockTextBox();
            _tbNote.Placeholder = "Write a note...";
            _cbAlert = new CheckBox();
            _cbPrivate = new CheckBox();
            _lbSaveNote = new LinkButton();
            _lbEditNote = new LinkButton();
            _lbDeleteNote = new LinkButton();
            _sbSecurity = new SecurityButton();
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
    $('a.edit-note').click(function (e) {
        e.preventDefault();
        $(this).closest('.note').children().slideToggle( 'slow' );
    });
    $('a.edit-note-cancel').click(function () {
        $(this).closest('.note').children().slideToggle( 'slow' );
    });
    $('a.remove-note').click(function() {
        return Rock.dialogs.confirmDelete( event, 'Note' );
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
            _lbSaveNote.CausesValidation = false;
            _lbSaveNote.Click += lbSaveNote_Click;

            Controls.Add(_lbSaveNote);

            var iEdit = new HtmlGenericControl( "i" );
            iEdit.Attributes["class"] = "fa fa-pencil";
            _lbEditNote.Controls.Add( iEdit );

            _lbDeleteNote.ID = this.ID + "_lbDeleteNote";
            _lbDeleteNote.Attributes["class"] = "remove-note";
            _lbDeleteNote.CausesValidation = false;
            _lbDeleteNote.Click += lbDeleteNote_Click;
            Controls.Add( _lbDeleteNote );
            var iDelete = new HtmlGenericControl( "i" );
            iDelete.Attributes["class"] = "fa fa-times";
            _lbDeleteNote.Controls.Add( iDelete );

            _sbSecurity.ID = "_sbSecurity";
            _sbSecurity.Attributes["class"] = "btn btn-security btn-xs security pull-right";
            _sbSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Note ) ).Id;

            Controls.Add( _sbSecurity );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( NoteTypeName != string.Empty )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "note note-" + NoteTypeName.ToLower().Replace( " ", "" ) );
            }
            else
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "note" );
            }
            
            if ( this.NoteId.HasValue )
            {
                writer.AddAttribute( "rel", this.NoteId.Value.ToStringSafe() );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Edit Mode HTML...
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-noteentry" );
            if ( NoteId.HasValue || !AddAlwaysVisible )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( DisplayType == NoteDisplayType.Full && UsePersonIcon )
            {
                writer.Write( Person.GetPhotoImageTag( CreatedByPhotoId, CreatedByGender, 50, 50 ) );
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "noteentry-control");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            _tbNote.RenderControl( writer );
            writer.RenderEndTag();

            if ( DisplayType == NoteDisplayType.Full )
            {
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

                if ( ShowSecurityButton && this.NoteId.HasValue )
                {
                    _sbSecurity.EntityId = this.NoteId.Value;
                    _sbSecurity.Title = this.Label;
                    _sbSecurity.RenderControl( writer );
                }

                writer.RenderEndTag();  // settings div
            }

            writer.RenderEndTag();  // panel body

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-footer" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            
            _lbSaveNote.Text = "Save " + Label;
            _lbSaveNote.RenderControl( writer );

            if ( NoteId.HasValue || !AddAlwaysVisible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note-cancel btn btn-link btn-xs" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( "Cancel" );
                writer.RenderEndTag();
            }

            writer.RenderEndTag();  // panel-footer div

            writer.RenderEndTag();  // note-entry div

            if ( NoteId.HasValue )
            {
                // View Mode HTML...
                writer.AddAttribute( HtmlTextWriterAttribute.Class, ArticleClass );
                writer.RenderBeginTag( "article" );

                if ( DisplayType == NoteDisplayType.Full )
                {
                    if ( UsePersonIcon )
                    {
                        writer.Write( Person.GetPhotoImageTag( CreatedByPhotoId, CreatedByGender, 50, 50 ) );
                    }
                    else
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, IconClass );
                        writer.RenderBeginTag( HtmlTextWriterTag.I );
                        writer.RenderEndTag();
                    }
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "details" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( DisplayType == NoteDisplayType.Full )
                {
                    // Heading
                    writer.RenderBeginTag( HtmlTextWriterTag.H5 );
                    string heading = Caption;
                    if ( string.IsNullOrWhiteSpace( Caption ) )
                    {
                        heading = CreatedByName;
                    }
                    writer.Write( heading.EncodeHtml() );
                    if ( CreatedDateTime.HasValue )
                    {
                        writer.Write( " " );
                        writer.AddAttribute( "class", "date" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Span );
                        writer.Write( CreatedDateTime.Value.ToRelativeDateString( 6 ) );
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag();

                    writer.Write( Text.EncodeHtml().ConvertCrLfToHtmlBr() );
                }
                else
                {
                    writer.Write( Text.EncodeHtml().ConvertCrLfToHtmlBr() );
                    writer.Write( " - " );
                    if ( !string.IsNullOrWhiteSpace( CreatedByName ) )
                    {
                        writer.AddAttribute( "class", "note-author" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Span );
                        writer.Write( CreatedByName );
                        writer.RenderEndTag();
                        writer.Write( " " );
                    }
                    if ( CreatedDateTime.HasValue )
                    {
                        writer.AddAttribute( "class", "note-created" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Span );
                        writer.Write( CreatedDateTime.Value.ToRelativeDateString( 6 ) );
                        writer.RenderEndTag();
                    }
                }

                writer.RenderEndTag();  // Details Div

                if ( CanEdit )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions rollover-item" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    _lbDeleteNote.RenderControl( writer );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "edit-note" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-pencil" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.RenderEndTag();  // A

                    writer.RenderEndTag();  // actions
                }

                writer.RenderEndTag();  // article
            }

            writer.RenderEndTag();
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
            var rockPage = this.Page as RockPage;
            if (rockPage != null && NoteTypeId.HasValue)
            {
                var currentPerson = rockPage.CurrentPerson;

                var rockContext = new RockContext();
                var service = new NoteService( rockContext );
                Note note = null;

                if ( NoteId.HasValue )
                {
                    note = service.Get( NoteId.Value );
                }

                bool newNote = false;

                if ( note == null )
                {
                    newNote = true;

                    note = new Note();
                    note.IsSystem = false;
                    note.NoteTypeId = NoteTypeId.Value;
                    note.EntityId = EntityId;
                    note.SourceTypeValueId = SourceTypeValueId;
                    service.Add( note );
                }

                note.Caption = IsPrivate ? "You - Personal Note" : string.Empty;
                note.Text = Text;
                note.IsAlert = IsAlert;

                rockContext.SaveChanges();

                if ( newNote )
                {
                    note.AllowPerson( Authorization.EDIT, currentPerson );
                }

                if ( IsPrivate && !note.IsPrivate( Authorization.VIEW, currentPerson ) )
                {
                    note.MakePrivate( Authorization.VIEW, currentPerson );
                }

                if ( !IsPrivate && note.IsPrivate( Authorization.VIEW, currentPerson ) )
                {
                    note.MakeUnPrivate( Authorization.VIEW, currentPerson );
                }


                if ( SaveButtonClick != null )
                {
                    SaveButtonClick( this, new NoteEventArgs( note.Id ) );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteNote_Click( object sender, EventArgs e )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var currentPerson = rockPage.CurrentPerson;

                var rockContext = new RockContext();
                var service = new NoteService( rockContext );
                Note note = null;

                if ( NoteId.HasValue )
                {
                    note = service.Get( NoteId.Value );
                    if ( note != null && note.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        service.Delete( note );
                        rockContext.SaveChanges();
                    }
                }

                if ( DeleteButtonClick != null )
                {
                    DeleteButtonClick( this, new NoteEventArgs( NoteId ) );
                }

            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when [save button click].
        /// </summary>
        public event EventHandler<NoteEventArgs> SaveButtonClick;

        /// <summary>
        /// Occurs when [delete button click].
        /// </summary>
        public event EventHandler<NoteEventArgs> DeleteButtonClick;



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
        public NoteEventArgs(int? noteId)
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
