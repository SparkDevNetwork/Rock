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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ImageEditor runat=server></{0}:ImageEditor>" )]
    public class ImageEditor : CompositeControl, IRockControl
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
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
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

        private Image _imgPhoto;
        private HiddenField _hfBinaryFileId;
        private HiddenField _hfBinaryFileTypeGuid;
        private FileUpload _fileUpload;
        private HtmlAnchor _aRemove;
        private LinkButton _lbShowModal;
        
        private ModalDialog _mdImageDialog;
        private Panel _pnlCropContainer;
        private Image _imgCropSource;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEditor"/> class.
        /// </summary>
        public ImageEditor()
            : base()
        {
            HelpBlock = new HelpBlock();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [display required indicator].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Data" ),
        DefaultValue( "" ),
        Description( "BinaryFile Id" )
        ]
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
                Guid guid;
                return Guid.TryParse( _hfBinaryFileTypeGuid.Value, out guid ) ? guid : new Guid( SystemGuid.BinaryFiletype.DEFAULT );
            }

            set
            {
                EnsureChildControls();
                _hfBinaryFileTypeGuid.Value = value.ToString();
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
        /// Gets or sets the done function client script.
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

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            _imgPhoto = new Image();
            _imgPhoto.ID = this.ID + "_imgPhoto";
            Controls.Add( _imgPhoto );

            _hfBinaryFileId.ID = this.ID + "_hfBinaryFileId";
            Controls.Add( _hfBinaryFileId );

            _hfBinaryFileTypeGuid.ID = this.ID + "_hfBinaryFileTypeGuid";
            Controls.Add( _hfBinaryFileTypeGuid );

            _aRemove = new HtmlAnchor();
            _aRemove.ID = "rmv";
            _aRemove.InnerHtml = "<i class='fa fa-times'></i>";
            Controls.Add( _aRemove );

            _lbShowModal = new LinkButton();
            _lbShowModal.ID = this.ID + "_lbShowModal";
            _lbShowModal.Text = "<i class='fa fa-pencil'></i>";
            _lbShowModal.Click += _lbShowModal_Click;
            Controls.Add( _lbShowModal );

            _fileUpload = new FileUpload();
            _fileUpload.ID = this.ID + "_fu";
            Controls.Add( _fileUpload );

            _mdImageDialog = new ModalDialog();
            _mdImageDialog.ID = this.ID + "_mdImageDialog";
            _mdImageDialog.Title = "Image";
            _mdImageDialog.SaveClick += _mdImageDialog_SaveClick;

            _pnlCropContainer = new Panel();
            _pnlCropContainer.CssClass = "crop-container clearfix";
            _imgCropSource = new Image();
            _imgCropSource.ID = this.ID + "_imgCropSource";
            _imgCropSource.CssClass = "image-editor-crop-source";
            _pnlCropContainer.Controls.Add( _imgCropSource );

            _mdImageDialog.Content.Controls.Add( _pnlCropContainer );
            

            Controls.Add( _mdImageDialog );
        }

        /// <summary>
        /// Handles the SaveClick event of the _mdImageDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _mdImageDialog_SaveClick( object sender, EventArgs e )
        {
            _mdImageDialog.Hide(); 
        }

        /// <summary>
        /// Handles the Click event of the _lbShowModal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbShowModal_Click( object sender, EventArgs e )
        {
            _imgCropSource.ImageUrl = "~/GetImage.ashx?id=" + BinaryFileId;
            _mdImageDialog.Show();
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
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "image-editor-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "image-editor-photo" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( BinaryFileId != null )
            {
                _imgPhoto.ImageUrl = "~/GetImage.ashx?id=" + BinaryFileId.ToString() + "&width=150";
                _aRemove.Style[HtmlTextWriterStyle.Display] = "inline";
            }
            else
            {
                _imgPhoto.ImageUrl = "/Assets/Images/no-picture.svg";
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            _imgPhoto.RenderControl( writer );
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "options" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div);
            _lbShowModal.RenderControl( writer );
            writer.WriteLine();
            _aRemove.RenderControl( writer );
            writer.WriteLine();
            writer.RenderEndTag();
            writer.WriteLine();
            
            _hfBinaryFileId.RenderControl( writer );
            writer.WriteLine();
            _hfBinaryFileTypeGuid.RenderControl( writer );
            writer.WriteLine();

            writer.AddAttribute( "class", "image-editor-fileinput" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _fileUpload.Attributes["name"] = string.Format( "{0}[]", this.ID );
            _fileUpload.RenderControl( writer );
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderEndTag(); //image-editor-photo
            writer.WriteLine();

            writer.Write( @"
                <div class='js-upload-progress pull-left' style='display:none'>
                    <i class='fa fa-spinner fa-3x fa-spin'></i>                    
                </div>" );

            writer.RenderEndTag(); //image-editor-group

            _mdImageDialog.RenderControl( writer );

            RegisterStartupScript();
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            var script = string.Format(
@"
Rock.controls.imageUploader.initialize({{
    controlId: '{0}',
    fileId: '{1}',
    fileTypeGuid: '{2}',
    hfFileId: '{3}',
    imgThumbnail: '{4}',
    aRemove: '{5}',
    fileType: 'image',
    isBinaryFile: '{6}',
    rootFolder: '{7}',
    submitFunction: function (e, data) {{
        {8}
    }},
    doneFunction: function (e, data) {{
        {9}
    }}
}});

$('#{10}').Jcrop({{
    aspectRatio:1
}},function(){{
	jcrop_api = this;
	jcrop_api.setSelect([ 0,0,300,300 ]);
}});


",
                _fileUpload.ClientID,
                this.BinaryFileId,
                this.BinaryFileTypeGuid,
                _hfBinaryFileId.ClientID,
                _imgPhoto.ClientID,
                _aRemove.ClientID,
                this.IsBinaryFile ? "T" : "F",
                Rock.Security.Encryption.EncryptString( this.RootFolder ),
                this.SubmitFunctionClientScript,
                this.DoneFunctionClientScript,
                this._imgCropSource.ClientID);
            ScriptManager.RegisterStartupScript( _fileUpload, _fileUpload.GetType(), "ImageUploaderScript_" + this.ID, script, true );
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
                _fileUpload.Visible = value;
                _aRemove.Visible = value;
            }
        }
    }
}