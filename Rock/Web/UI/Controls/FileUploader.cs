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
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A control to select a file and set any attributes
    /// </summary>
    [ToolboxData( "<{0}:FileUploader runat=server></{0}:FileUploader>" )]
    public class FileUploader : CompositeControl, IPostBackEventHandler, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region UI Controls

        private HiddenField _hfBinaryFileId;
        private HiddenField _hfBinaryFileTypeGuid;
        private HtmlAnchor _aFileName;
        private HtmlAnchor _aRemove;
        private FileUpload _fileUpload;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploader" /> class.
        /// </summary>
        public FileUploader()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        public int? BinaryFileId
        {
            get
            {
                EnsureChildControls();
                int? result = _hfBinaryFileId.ValueAsInt();
                if ( result > 0 )
                {
                    return result;
                }
                else
                {
                    // BinaryFileId of 0 means no file, so just return null instead
                    return null;
                }
            }

            set
            {
                EnsureChildControls();
                _hfBinaryFileId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the binary file type GUID.
        /// </summary>
        /// <value>
        /// The binary file type GUID.
        /// </value>
        public Guid BinaryFileTypeGuid
        {
            get
            {
                EnsureChildControls();
                var result = _hfBinaryFileTypeGuid.Value.AsGuid();
                if ( result.IsEmpty() )
                {
                    result = SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
                }

                return result;
            }

            set
            {
                EnsureChildControls();
                _hfBinaryFileTypeGuid.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets the uploaded content file path.
        /// </summary>
        /// <value>
        /// The uploaded content file path.
        /// </value>
        public string UploadedContentFilePath
        {
            get
            {
                if ( IsBinaryFile || string.IsNullOrWhiteSpace( _hfBinaryFileId.Value ) )
                {
                    return null;
                }
                else
                {
                    return this.RootFolder.EnsureTrailingForwardslash() + _hfBinaryFileId.Value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is binary file].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is binary file]; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "true" ),
        Description( "Does this use the BinaryFile framework?" )
        ]
        public bool IsBinaryFile
        {
            get
            {
                return ViewState["IsBinaryFile"] as bool? ?? true;
            }

            set
            {
                ViewState["IsBinaryFile"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the root folder.
        /// </summary>
        /// <value>
        /// The root folder.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "true" ),
        Description( "The RootFolder where NonBinaryFile files will be uploaded to" )
        ]
        public string RootFolder
        {
            get
            {
                return ViewState["RootFolder"] as string;
            }

            set
            {
                ViewState["RootFolder"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the upload URL.
        /// </summary>
        /// <value>
        /// The upload URL.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "true" ),
        Description( "The Url where files will be uploaded to" )
        ]
        public string UploadUrl
        {
            get
            {
                string result = ViewState["UploadUrl"] as string;
                if ( string.IsNullOrWhiteSpace( result ) )
                {
                    result = "FileUploader.ashx";
                }

                return result;
            }

            set
            {
                ViewState["UploadUrl"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show delete button].
        /// Defaults to true
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show delete button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDeleteButton
        {
            get
            {
                return ViewState["ShowDeleteButton"] as bool? ?? true;
            }

            set
            {
                ViewState["ShowDeleteButton"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the submit function client script.
        /// </summary>
        /// <value>
        /// The submit function client script.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional javascript to run in the file uploader's submitFunction" )
        ]
        public string SubmitFunctionClientScript
        {
            get
            {
                return ViewState["SubmitFunctionClientScript"] as string;
            }

            set
            {
                ViewState["SubmitFunctionClientScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the done function client script.
        /// </summary>
        /// <value>
        /// The done function client script.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional javascript to run in the file uploader's doneFunction" )
        ]
        public string DoneFunctionClientScript
        {
            get
            {
                return ViewState["DoneFunctionClientScript"] as string;
            }

            set
            {
                ViewState["DoneFunctionClientScript"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum UploaderDisplayMode
        {
            /// <summary>
            /// As a dropzone
            /// </summary>
            DropZone,

            /// <summary>
            /// As a button
            /// </summary>
            Button
        }

        /// <summary>
        /// Gets or sets the display mode.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public FileUploader.UploaderDisplayMode DisplayMode
        {
            get
            {
                var mode = ViewState["DisplayMode"];
                return mode != null ? (FileUploader.UploaderDisplayMode)mode : FileUploader.UploaderDisplayMode.DropZone;
            }

            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            Controls.Add( _hfBinaryFileId );
            _hfBinaryFileId.ID = this.ID + "_hfBinaryFileId";
            _hfBinaryFileId.Value = "0";

            Controls.Add( _hfBinaryFileTypeGuid );
            _hfBinaryFileTypeGuid.ID = this.ID + "_hfBinaryFileTypeGuid";

            _aFileName = new HtmlAnchor();
            Controls.Add( _aFileName );
            _aFileName.ID = "fn";
            _aFileName.Target = "_blank";
            _aFileName.AddCssClass( "file-link" );

            _aRemove = new HtmlAnchor();
            Controls.Add( _aRemove );
            _aRemove.ID = "rmv";
            _aRemove.HRef = "#";
            _aRemove.InnerHtml = "<i class='fa fa-times'></i>";
            _aRemove.Attributes["class"] = "remove-file";

            _fileUpload = new FileUpload();
            Controls.Add( _fileUpload );
            _fileUpload.ID = this.ID + "_fu";

            RequiredFieldValidator.InitialValue = "0";
            RequiredFieldValidator.ControlToValidate = _hfBinaryFileId.ID;
            RequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "fileupload-group" );
            writer.AddAttribute( "id", this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( BinaryFileId != null || !string.IsNullOrWhiteSpace( this.UploadedContentFilePath ) )
            {
                if ( IsBinaryFile )
                {
                    _aFileName.HRef = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), BinaryFileId );
                    _aFileName.InnerText = new BinaryFileService( new RockContext() ).Queryable().Where( f => f.Id == BinaryFileId ).Select( f => f.FileName ).FirstOrDefault();
                }
                else
                {
                    _aFileName.HRef = string.Format( "{0}GetFile.ashx?isBinaryFile=F&rootFolder={1}&fileName={2}", ResolveUrl( "~" ), Rock.Security.Encryption.EncryptString( this.RootFolder ), BinaryFileId );
                    _aFileName.InnerText = this.UploadedContentFilePath;
                }

                _aFileName.AddCssClass( "file-exists" );
                _aRemove.Style[HtmlTextWriterStyle.Display] = "block";
            }
            else
            {
                _aFileName.HRef = string.Empty;
                _aFileName.InnerText = string.Empty;
                _aFileName.RemoveCssClass( "file-exists" );
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if ( this.Enabled )
            {
                if ( this.DisplayMode == UploaderDisplayMode.DropZone )
                {
                    writer.AddAttribute( "class", "fileupload-thumbnail" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _hfBinaryFileId.RenderControl( writer );
                    _hfBinaryFileTypeGuid.RenderControl( writer );
                    _aFileName.RenderControl( writer );


                    writer.AddAttribute( "class", "fileupload-remove" );
                    if ( !ShowDeleteButton )
                    {
                        writer.AddStyleAttribute( HtmlTextWriterStyle.Visibility, "hidden" );
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _aRemove.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.RenderEndTag();
                }

                writer.Write( @"
                    <div class='js-upload-progress upload-progress' style='display:none;'>
                        <i class='fa fa-refresh fa-3x fa-spin'></i>                    
                    </div>" );

                if (this.DisplayMode == UploaderDisplayMode.Button)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "fileupload-button");
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "fileupload-dropzone");
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                if ( this.DisplayMode == UploaderDisplayMode.Button )
                {
                    writer.Write( "Upload File" );
                }
                else
                {
                    writer.Write( "Upload" );
                }

                writer.RenderEndTag();
                _fileUpload.Attributes["name"] = string.Format( "{0}[]", this.ID );
                _fileUpload.RenderControl( writer );
                writer.RenderEndTag();
            }

            RegisterStartupScript();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            int? maxUploadBytes = null;
            try
            {
                HttpRuntimeSection section = ConfigurationManager.GetSection( "system.web/httpRuntime" ) as HttpRuntimeSection;
                if ( section != null )
                {
                    // MaxRequestLength is in KB
                    maxUploadBytes = section.MaxRequestLength * 1024;
                }
            }
            catch
            {
                // intentionally ignore and don't tell the fileUploader the limit
            }

            var postBackScript = this.FileUploaded != null ? this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "FileUploaded" ), true ) : "";
            postBackScript = postBackScript.Replace( '\'', '"' );
            var script = string.Format(
@"
Rock.controls.fileUploader.initialize({{
    controlId: '{0}',
    fileId: '{1}',
    fileTypeGuid: '{2}',
    hfFileId: '{3}',
    aFileName: '{4}',
    aRemove: '{5}',
    postbackScript: '{6}',
    fileType: 'file',
    isBinaryFile: '{7}',
    rootFolder: '{8}',
    uploadUrl: '{9}',
    submitFunction: function (e, data) {{
        {10}
    }},
    doneFunction: function (e, data) {{
        {11}
    }},
    maxUploadBytes: {12}
}});",
                _fileUpload.ClientID,
                this.BinaryFileId,
                this.BinaryFileTypeGuid,
                _hfBinaryFileId.ClientID,
                _aFileName.ClientID,
                _aRemove.ClientID,
                postBackScript,
                this.IsBinaryFile ? "T" : "F",
                Rock.Security.Encryption.EncryptString( this.RootFolder ),
                this.UploadUrl,
                this.SubmitFunctionClientScript,
                this.DoneFunctionClientScript,
                maxUploadBytes.HasValue ? maxUploadBytes.Value.ToString() : "null" );
            ScriptManager.RegisterStartupScript( _fileUpload, _fileUpload.GetType(), "FileUploaderScript_" + this.ClientID, script, true );
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Web server control is enabled.
        /// </summary>
        /// <returns>true if control is enabled; otherwise, false. The default is true.</returns>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }

            set
            {
                base.Enabled = value;
                EnsureChildControls();
                _fileUpload.Visible = value;
                _aRemove.Visible = value;
            }
        }

        /// <summary>
        /// Occurs when a file is uploaded.
        /// </summary>
        public event EventHandler FileUploaded;

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "FileUploaded" && FileUploaded != null )
            {
                FileUploaded( this, new EventArgs() );
            }
        }
    }
}