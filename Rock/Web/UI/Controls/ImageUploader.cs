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
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Security;
using Rock.Utility;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ImageUploader runat=server></{0}:ImageUploader>" )]
    public class ImageUploader : CompositeControl, IRockControl, IPostBackEventHandler
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

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
        private HiddenField _hfContentFileSource;
        private FileUpload _fileUpload;
        private HtmlAnchor _aRemove;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageUploader"/> class.
        /// </summary>
        public ImageUploader()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
            _hfContentFileSource = new HiddenFieldWithClass();
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the BinaryFileId when used in BinaryFile mode <see cref="IsBinaryFile"/>
        /// </summary>
        /// <value>
        /// The binary file identifier.
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

                if ( value.HasValue )
                {
                    _hfBinaryFileId.Value = value.ToString();
                }
                else
                {
                    _hfBinaryFileId.Value = "0";
                }
            }
        }

        /// <summary>
        /// Gets or sets the binary file type GUID.
        /// </summary>
        /// <value>
        /// The binary file type GUID.
        /// </value>
        [
        Bindable( true ),
        Category( "Data" ),
        DefaultValue( "" ),
        Description( "BinaryFileType Guid" )
        ]
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
        /// Gets or sets the file name when used in ContentFile mode (when <see cref="IsBinaryFile"/> is set to false).
        /// For example, "~/Content/External/MyUploadedFile.jpg".
        /// </summary>
        /// <value>
        /// The uploaded content file path.
        /// </value>
        public string NonBinaryFileSrc
        {
            get
            {
                EnsureChildControls();
                if ( IsBinaryFile )
                {
                    return null;
                }
                else
                {
                    if ( _hfContentFileSource.Value.IsNullOrWhiteSpace() )
                    {
                        return string.Empty;
                    }
                    else
                    {
                        var rootFolderPath = ( this.RootFolder ?? "~/Content" ).EnsureTrailingForwardslash();
                        if ( !_hfContentFileSource.Value.StartsWith( rootFolderPath ) && !_hfContentFileSource.Value.StartsWith("~") )
                        {
                            // if NonBinaryFileSrc only specified the filename, include the rootFolderPath
                            return rootFolderPath + _hfContentFileSource.Value;
                        }
                        else
                        {
                            return _hfContentFileSource.Value;
                        }
                    }
                }
            }

            set
            {
                _hfContentFileSource.Value = value;
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
        /// Gets or sets the submit function client script.
        /// </summary>
        /// <value>
        /// The submit function client script.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional javascript to run in the image uploader's submitFunction" )
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
        /// The optional javascript to run in the image uploader's doneFunction
        /// </summary>
        /// <value>
        /// The done function client script.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional javascript to run in the image uploader's doneFunction" )
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
        /// The optional javascript to run when the image is removed
        /// </summary>
        /// <value>
        /// The delete function client script.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional javascript to run when the image is removed" )
        ]
        public string DeleteFunctionClientScript
        {
            get
            {
                return ViewState["DeleteFunctionClientScript"] as string;
            }

            set
            {
                ViewState["DeleteFunctionClientScript"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail width.
        /// </summary>
        /// <value>
        /// The width of the thumbnail.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional width of the thumbnail" )
        ]
        public int ThumbnailWidth
        {
            get
            {
                return ViewState["ThumbnailWidth"] as int? ?? 100;
            }

            set
            {
                ViewState["ThumbnailWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail width.
        /// </summary>
        /// <value>
        /// The width of the thumbnail.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Determines if the image should be stored with the 'Temporary' flag." )
        ]
        public bool UploadAsTemporary
        {
            get
            {
                return ViewState["UploadAsTemporary"] as bool? ?? true;
            }

            set
            {
                ViewState["UploadAsTemporary"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail height.
        /// </summary>
        /// <value>
        /// The height of the thumbnail.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The optional height of the thumbnail" )
        ]
        public int ThumbnailHeight
        {
            get
            {
                return ViewState["ThumbnailHeight"] as int? ?? 100;
            }

            set
            {
                ViewState["ThumbnailHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the picture URL to use when there is no image selected
        /// </summary>
        /// <value>
        /// The no picture URL.
        /// </value>
        public string NoPictureUrl
        {
            get
            {
                string nopictureUrl = ViewState["NoPictureUrl"] as string;
                if ( string.IsNullOrWhiteSpace( nopictureUrl ) )
                {
                    return System.Web.VirtualPathUtility.ToAbsolute( "~/Assets/Images/no-picture.svg" );
                }
                else
                {
                    return nopictureUrl;
                }
            }

            set
            {
                ViewState["NoPictureUrl"] = value;
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

            _hfBinaryFileId.ID = this.ID + "_hfBinaryFileId";
            _hfBinaryFileId.Value = "0";
            Controls.Add( _hfBinaryFileId );

            _hfBinaryFileTypeGuid.ID = this.ID + "_hfBinaryFileTypeGuid";
            Controls.Add( _hfBinaryFileTypeGuid );

            _hfContentFileSource.ID = this.ID + "_hfContentFileSource";

            Controls.Add( _hfContentFileSource );

            _aRemove = new HtmlAnchor();
            _aRemove.ID = this.ID + "_rmv";
            _aRemove.InnerHtml = "<i class='fa fa-times'></i>";
            Controls.Add( _aRemove );

            _fileUpload = new FileUpload();
            _fileUpload.ID = this.ID + "_fu";
            Controls.Add( _fileUpload );

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
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "imageupload-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );


            writer.AddAttribute( "style", String.Format( "width: {0}px; height: {1}px;", this.ThumbnailWidth, this.ThumbnailHeight ) );
            writer.AddAttribute( "class", "imageupload-thumbnail" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            string thumbnailImageUrl = this.NoPictureUrl;

            if ( IsBinaryFile && BinaryFileId.HasValue )
            {
                thumbnailImageUrl = FileUrlHelper.GetImageUrl( BinaryFileId.Value );

                _aRemove.Style[HtmlTextWriterStyle.Display] = "block";

                writer.AddAttribute( HtmlTextWriterAttribute.Href, thumbnailImageUrl );
                writer.AddAttribute( HtmlTextWriterAttribute.Target, "_blank" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );

                thumbnailImageUrl += "&width=500";
            }
            else if ( NonBinaryFileSrc.IsNotNullOrWhiteSpace() )
            {
                string physicalFile = this.Page.Request.MapPath( NonBinaryFileSrc );
                if ( File.Exists( physicalFile ) )
                {
                    thumbnailImageUrl = System.Web.VirtualPathUtility.ToAbsolute( NonBinaryFileSrc );
                    _aRemove.Style[HtmlTextWriterStyle.Display] = "block";
                }
                else
                {
                    _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
                }
            }
            else
            {
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            writer.AddAttribute( "style", string.Format( "background-image: url({0});", thumbnailImageUrl ) );
            writer.AddAttribute( "class", "imageupload-thumbnail-image" );
            writer.AddAttribute( "id", this.ClientID + "-thumbnail" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            if ( BinaryFileId != null )
            {
                writer.RenderEndTag();
            }

            _hfBinaryFileId.RenderControl( writer );
            _hfBinaryFileTypeGuid.RenderControl( writer );
            _hfContentFileSource.RenderControl( writer );

            writer.AddAttribute( "class", "imageupload-remove" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _aRemove.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.Write( @"
        <div class='js-upload-progress upload-progress' style='display:none'>
            <i class='fa fa-refresh fa-3x fa-spin'></i>                    
        </div>" );

            writer.AddAttribute( "class", "imageupload-dropzone" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.Write( "Upload" );
            writer.RenderEndTag();

            _fileUpload.Attributes["name"] = string.Format( "{0}[]", this.ID );
            _fileUpload.RenderControl( writer );
            writer.RenderEndTag();

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

            var postBackScript = this.ImageUploaded != null ? this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "ImageUploaded" ), true ) : "";
            postBackScript = postBackScript.Replace( '\'', '"' );

            // If in BinaryFile mode, only do an ImageRemoved postback if there is a ImageRemoved event specified
            // If in ContentFile mode, do the postback regardless since we'll have to delete the contentfile from the system
            bool doImageRemovedPostback = this.ImageRemoved != null || this.IsBinaryFile == false;

            var postBackRemovedScript = doImageRemovedPostback ? this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "ImageRemoved" ), true ) : "";
            postBackRemovedScript = postBackRemovedScript.Replace( '\'', '"' );

            var securitySettings = new SecuritySettingsService().SecuritySettings;
            bool disablePredictableIds = securitySettings.DisablePredictableIds;

            var script =
            $@"
Rock.controls.imageUploader.initialize({{
    controlId: '{_fileUpload.ClientID}',
    fileId: '{this.BinaryFileId}',
    fileTypeGuid: '{this.BinaryFileTypeGuid}',
    hfFileId: '{_hfBinaryFileId.ClientID}',
    hfContentFileSource: '{_hfContentFileSource.ClientID}',
    imgThumbnail: '{( this.ClientID + "-thumbnail" )}',
    aRemove: '{_aRemove.ClientID}',
    postbackScript: '{postBackScript}',
    fileType: 'image',
    isBinaryFile: '{( this.IsBinaryFile ? "T" : "F" )}',
    rootFolder: '{Rock.Security.Encryption.EncryptString( this.RootFolder )}',
    noPictureUrl: '{this.NoPictureUrl}',
    disablePredictableIds: {disablePredictableIds.ToString().ToLower()},
    submitFunction: function (e, data) {{
        {this.SubmitFunctionClientScript}
    }},
    doneFunction: function (e, data) {{
        {this.DoneFunctionClientScript}
    }},
    postbackRemovedScript: '{postBackRemovedScript}',
    maxUploadBytes: {( maxUploadBytes.HasValue ? maxUploadBytes.Value.ToString() : "null" )},
    isTemporary: '{( this.UploadAsTemporary ? "T" : "F" )}',
    deleteFunction: function(e, data) {{
        {this.DeleteFunctionClientScript}
    }}
}});";

            ScriptManager.RegisterStartupScript( _fileUpload, _fileUpload.GetType(), "ImageUploaderScript_" + this.ClientID, script, true );
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
        public event EventHandler<ImageUploaderEventArgs> ImageUploaded;

        /// <summary>
        /// Occurs when a file is removed.
        /// </summary>
        public event EventHandler<ImageUploaderEventArgs> ImageRemoved;

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "ImageUploaded" && ImageUploaded != null )
            {
                if ( IsBinaryFile )
                {
                    ImageUploaded( this, new ImageUploaderEventArgs( this.BinaryFileId, ImageUploaderEventArgs.ArgumentType.ImageUploaded ) );
                }
                else
                {
                    ImageUploaded( this, new ImageUploaderEventArgs( this.NonBinaryFileSrc, ImageUploaderEventArgs.ArgumentType.ImageUploaded ) );
                }
            }

            if ( eventArgument == "ImageRemoved" )
            {
                if ( IsBinaryFile )
                {
                    ImageRemoved?.Invoke( this, new ImageUploaderEventArgs( this.BinaryFileId, ImageUploaderEventArgs.ArgumentType.ImageRemoved ) );
                }
                else
                {
                    if ( this.NonBinaryFileSrc.IsNotNullOrWhiteSpace() )
                    {
                        var physicalFileName = this.Page.Request.MapPath( NonBinaryFileSrc );
                        if ( File.Exists( physicalFileName ) )
                        {
                            File.Delete( physicalFileName );
                        }
                    }

                    ImageRemoved?.Invoke( this, new ImageUploaderEventArgs( this.NonBinaryFileSrc, ImageUploaderEventArgs.ArgumentType.ImageRemoved ) );
                }

                this.NonBinaryFileSrc = string.Empty;
                this.BinaryFileId = 0;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ImageUploaderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the binary file identifier of the Image that was Uploaded or Removed (see EventArgument)
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        public int? BinaryFileId { get; private set; }

        /// <summary>
        /// Gets the name of the file (If IsBinaryFile=False)
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string NonBinaryFileSrc { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderEventArgs" /> class.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="eventArgument">The event argument.</param>
        public ImageUploaderEventArgs( int? binaryFileId, ArgumentType eventArgument ) : base()
        {
            BinaryFileId = binaryFileId;
            EventArgument = eventArgument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderEventArgs" /> class.
        /// </summary>
        /// <param name="nonBinaryFileSrc">The non binary file source.</param>
        /// <param name="eventArgument">The event argument.</param>
        public ImageUploaderEventArgs( string nonBinaryFileSrc, ArgumentType eventArgument ) : base()
        {
            NonBinaryFileSrc = nonBinaryFileSrc;
            EventArgument = eventArgument;
        }

        /// <summary>
        /// Gets the event argument.
        /// </summary>
        /// <value>
        /// The event argument.
        /// </value>
        public ArgumentType EventArgument { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public enum ArgumentType
        {
            /// <summary>
            /// The image uploaded
            /// </summary>
            ImageUploaded,

            /// <summary>
            /// The image removed
            /// </summary>
            ImageRemoved
        }
    }
}