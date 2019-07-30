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
    /// This control is used to allow a photo to be uploaded and then cropped
    /// by the user before it is automatically resized (by this control).
    /// </summary>
    /// <remarks>
    /// To bind an image to the control set the BinaryFileId property to an existing file id.
    ///
    /// An OnFileSaved event will be raised after the photo is uploaded and cropped
    /// allowing another control to handle this event to presumably do something
    /// immediately with the image.  The PhotoRequest's Upload block uses this feature.
    /// <example>
    /// <code>
    /// <![CDATA[
    ///     <Rock:ImageEditor ID="imgedPhoto" runat="server" ButtonText="<i class='fa fa-pencil'></i> Select Photo"
    ///             ButtonCssClass="btn btn-primary margin-t-sm" CommandArgument='<%# Eval("Id") %>'
    ///             OnFileSaved="imageEditor_FileSaved" ShowDeleteButton="false" />
    /// ]]>
    /// </code>
    /// </example>
    ///
    /// By setting the ShowDeleteButton to false, a more simplified UX occurs which is intended
    /// for normal end-users. The remove button is not shown and the image upload button is
    /// always shown. This is allows for the existing image to always be replaced when the
    /// upload button is clicked.
    /// </remarks>
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

        /// <summary>
        /// Gets or sets the edit button text.
        /// </summary>
        /// <value>
        /// The edit button text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "<i class='fa fa-pencil'></i>" ),
        Description( "The text for the edit button." )
        ]
        public string ButtonText
        {
            get { return ViewState["ButtonText"] as string ?? "<i class='fa fa-pencil'></i>"; }
            set { ViewState["ButtonText"] = value; }
        }

        /// <summary>
        /// Gets or sets the edit button tool tip.
        /// </summary>
        /// <value>
        /// The edit button tool tip.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The title attribute for the edit button." )
        ]
        public string ButtonToolTip
        {
            get { return ViewState["ButtonToolTip"] as string ?? string.Empty; }
            set { ViewState["ButtonToolTip"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS class for the edit button.
        /// </summary>
        /// <value>
        /// The CSS class name.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The CSS class to use on the edit button." )
        ]
        public string ButtonCssClass
        {
            get { return ViewState["ButtonCssClass"] as string ?? string.Empty; }
            set { ViewState["ButtonCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the delete button is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "true" ),
        Description( "Allow delete by showing the delete button?" )
        ]
        public bool ShowDeleteButton
        {
            get { return ViewState["ShowDeleteButton"] as bool? ?? true; }
            set { ViewState["ShowDeleteButton"] = value; }
        }

        #region OnLoad

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.Request.Params["__EVENTTARGET"] == _lbUploadImage.UniqueID )
            {
                // manually wire up to _lblUploadImage_Click since we want the fileUpload dialog javascript to happen first, then this
                _lbUploadImage_Click( _lbUploadImage, e );
            }
        }

        #endregion

        #region UI Controls

        private HiddenField _hfOriginalBinaryFileId;
        private HiddenField _hfBinaryFileId;
        private HiddenField _hfBinaryFileTypeGuid;
        private FileUpload _fileUpload;
        private HtmlAnchor _aRemove;
        private Label _lSaveStatus;
        private LinkButton _lbShowModal;
        private LinkButton _lbUploadImage;

        private HiddenField _hfCropBinaryFileId;
        private ModalDialog _mdImageDialog;
        private Panel _pnlCropContainer;
        private NotificationBox _nbImageWarning;
        private NotificationBox _nbErrorMessage;
        private Image _imgCropSource;
        private HiddenField _hfCropCoords;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEditor"/> class.
        /// </summary>
        public ImageEditor()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
            _hfOriginalBinaryFileId = new HiddenField();
            _hfCropBinaryFileId = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The OriginalBinaryFileId of the image displayed on in the main image (before uploading any new one occurs)
        /// </summary>
        /// <value>
        /// The binary file identifier of the original file
        /// </value>
        public int? OriginalBinaryFileId
        {
            get
            {
                EnsureChildControls();
                int? result = _hfOriginalBinaryFileId.ValueAsInt();
                if ( result > 0 )
                {
                    return result;
                }
                else
                {
                    // OriginalBinaryFileId of 0 means no file, so just return null instead
                    return null;
                }
            }
        }

        /// <summary>
        /// The BinaryFileId of the image displayed on in the main image (not necessarily the one being cropped in the Modal)
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

                // only set the OriginalBinaryFileId once...
                if ( string.IsNullOrEmpty( _hfOriginalBinaryFileId.Value ) )
                {
                    _hfOriginalBinaryFileId.Value = _hfBinaryFileId.Value;
                }
            }
        }

        /// <summary>
        /// The BinaryFileId of the BinaryFile that in the process of being cropped (not necessarily the one shown in the base image)
        /// </summary>
        /// <value>
        /// The uploaded binary file identifier.
        /// </value>
        public int? CropBinaryFileId
        {
            get
            {
                EnsureChildControls();
                int? result = _hfCropBinaryFileId.ValueAsInt();
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
                _hfCropBinaryFileId.Value = value.ToString();
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
        /// Gets or sets the maximum height of the image.
        /// </summary>
        /// <value>
        /// The maximum height of the image.
        /// </value>
        public int? MaxImageHeight
        {
            get
            {
                return ViewState["MaxImageHeight"] as int?;
            }

            set
            {
                ViewState["MaxImageHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum width of the image.
        /// </summary>
        /// <value>
        /// The maximum width of the image.
        /// </value>
        public int? MaxImageWidth
        {
            get
            {
                return ViewState["MaxImageWidth"] as int?;
            }

            set
            {
                ViewState["MaxImageWidth"] = value;
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
                    return System.Web.VirtualPathUtility.ToAbsolute( "~/Assets/Images/person-no-photo-unknown.svg" );
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

        /// <summary>
        /// Occurs when [file saved].
        /// </summary>
        public event EventHandler FileSaved;

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            //_hfBinaryFileId = new HiddenField();
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            Controls.Add( _hfBinaryFileId );
            _hfBinaryFileId.ID = this.ID + "_hfBinaryFileId";
            _hfBinaryFileId.Value = "0";

            _hfOriginalBinaryFileId.ID = this.ID + "_hfOriginalBinaryFileId";
            Controls.Add( _hfOriginalBinaryFileId );

            _hfCropBinaryFileId.ID = this.ID + "_hfCropBinaryFileId";
            Controls.Add( _hfCropBinaryFileId );

            _hfBinaryFileTypeGuid.ID = this.ID + "_hfBinaryFileTypeGuid";
            Controls.Add( _hfBinaryFileTypeGuid );

            _aRemove = new HtmlAnchor();
            _aRemove.ID = "rmv";
            _aRemove.InnerHtml = "<i class='fa fa-times'></i>";
            Controls.Add( _aRemove );

            _lbShowModal = new LinkButton();
            _lbShowModal.ID = this.ID + "_lbShowModal";
            _lbShowModal.CssClass = this.ButtonCssClass;
            _lbShowModal.Text = this.ButtonText;
            _lbShowModal.ToolTip = this.ButtonToolTip;
            _lbShowModal.Click += _lbShowModal_Click;
            _lbShowModal.CausesValidation = false;
            Controls.Add( _lbShowModal );

            // If we are not showing the delete button then
            // only the UploadImage button should be active.
            if ( !ShowDeleteButton )
            {
                _aRemove.Visible = false;
                _lbShowModal.Visible = false;
            }

            _lbUploadImage = new LinkButton();
            _lbUploadImage.ID = this.ID + "_lbUploadImage";
            _lbUploadImage.CssClass = this.ButtonCssClass;
            _lbUploadImage.Text = this.ButtonText;
            _lbUploadImage.ToolTip = this.ButtonToolTip;
            _lbUploadImage.CausesValidation = false;
            Controls.Add( _lbUploadImage );

            _lSaveStatus = new Label();
            _lSaveStatus.ID = this.ID + "_lSaveStatus";
            _lSaveStatus.CssClass = "fa fa-2x fa-check-circle-o text-success";
            _lSaveStatus.Style.Add( "vertical-align", "bottom" );
            _lSaveStatus.Visible = false;
            Controls.Add( _lSaveStatus );

            _fileUpload = new FileUpload();
            _fileUpload.ID = this.ID + "_fu";
            Controls.Add( _fileUpload );

            _mdImageDialog = new ModalDialog();
            _mdImageDialog.ValidationGroup = "vg_mdImageDialog";
            _mdImageDialog.ID = this.ID + "_mdImageDialog";
            _mdImageDialog.Title = "Image";
            _mdImageDialog.SaveButtonText = "Crop";
            _mdImageDialog.SaveClick += _mdImageDialog_SaveClick;

            _pnlCropContainer = new Panel();
            _pnlCropContainer.CssClass = "crop-container image-editor-crop-container clearfix";
            _nbImageWarning = new NotificationBox();
            _nbImageWarning.ID = this.ID + "_nbImageWarning";
            _nbImageWarning.NotificationBoxType = NotificationBoxType.Warning;
            _nbImageWarning.Text = "SVG image cropping is not supported.";

            _nbErrorMessage = new NotificationBox();
            _nbErrorMessage.ID = this.ID + "_nbErrorMessage";
            _nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
            _nbErrorMessage.Text = "File Type is not supported.";
            _nbErrorMessage.Visible = false;

            _imgCropSource = new Image();
            _imgCropSource.ID = this.ID + "_imgCropSource";
            _imgCropSource.CssClass = "image-editor-crop-source";

            _pnlCropContainer.Controls.Add( _imgCropSource );

            _mdImageDialog.Content.Controls.Add( _nbImageWarning );
            _mdImageDialog.Content.Controls.Add( _pnlCropContainer );

            _hfCropCoords = new HiddenField();
            _hfCropCoords.ID = this.ID + "_hfCropCoords";
            _pnlCropContainer.Controls.Add( _hfCropCoords );

            Controls.Add( _mdImageDialog );

            RequiredFieldValidator.InitialValue = "0";
            RequiredFieldValidator.ControlToValidate = _hfBinaryFileId.ID;
            RequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
        }

        /// <summary>
        /// Handles the SaveClick event of the _mdImageDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _mdImageDialog_SaveClick( object sender, EventArgs e )
        {
            try
            {
                var rockContext = new RockContext();
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );

                // load image from database
                var binaryFile = binaryFileService.Get( CropBinaryFileId ?? 0 );
                if ( binaryFile != null )
                {
                    var croppedBinaryFile = new BinaryFile();
                    binaryFileService.Add( croppedBinaryFile );
                    croppedBinaryFile.IsTemporary = true;
                    croppedBinaryFile.BinaryFileTypeId = binaryFile.BinaryFileTypeId;
                    croppedBinaryFile.MimeType = binaryFile.MimeType;
                    croppedBinaryFile.FileName = binaryFile.FileName;
                    croppedBinaryFile.Description = binaryFile.Description;

                    using ( var sourceStream = binaryFile.ContentStream )
                    {
                        if ( sourceStream != null )
                        {
                            croppedBinaryFile.ContentStream = CropImage( sourceStream, binaryFile.MimeType );
                        }
                    }

                    rockContext.SaveChanges();

                    this.BinaryFileId = croppedBinaryFile.Id;
                }

                // Raise the FileSaved event if one is defined by another control.
                if ( FileSaved != null )
                {
                    FileSaved( this, e );
                    _lSaveStatus.Visible = true;
                }

                _hfOriginalBinaryFileId.Value = this.BinaryFileId.ToStringSafe();
                _mdImageDialog.Hide();
            }
            catch ( ImageResizer.Plugins.Basic.SizeLimits.SizeLimitException )
            {
                // shouldn't happen because we resize it below the limit in CropImage(), but just in case
                var sizeLimits = new ImageResizer.Plugins.Basic.SizeLimits();
                _nbImageWarning.Visible = true;
                _nbImageWarning.Text = string.Format( "The image size exceeds the maximum resolution of {0}x{1}. Press cancel and try selecting a smaller image.", sizeLimits.TotalSize.Width, sizeLimits.TotalSize.Height );
                _mdImageDialog.Show();
            }
        }

        /// <summary>
        /// Crops the image.
        /// </summary>
        /// <param name="bitmapContent">Content of the bitmap.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <returns></returns>
        private Stream CropImage( Stream bitmapContent, string mimeType )
        {
            if ( mimeType == "image/svg+xml" )
            {
                return bitmapContent;
            }

            int[] photoCoords = _hfCropCoords.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => (int)a.AsDecimal() ).ToArray();
            int x = photoCoords[0];
            int y = photoCoords[1];
            int width = photoCoords[2];
            int height = photoCoords[3];
            int x2 = x + width;
            int y2 = y + height;

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( bitmapContent );

            // intentionally tell imageResizer to ignore the 3200x3200 size limit so that we can crop it first before limiting the size.
            var sizingPlugin = ImageResizer.Configuration.Config.Current.Plugins.Get<ImageResizer.Plugins.Basic.SizeLimiting>();
            var origLimit = sizingPlugin.Limits.TotalBehavior;
            sizingPlugin.Limits.TotalBehavior = ImageResizer.Plugins.Basic.SizeLimits.TotalSizeBehavior.IgnoreLimits;
            MemoryStream croppedStream = new MemoryStream();
            ImageResizer.ResizeSettings resizeCropSettings = new ImageResizer.ResizeSettings( string.Format( "crop={0},{1},{2},{3}", x, y, x2, y2 ) );
            MemoryStream imageStream = new MemoryStream();
            bitmap.Save( imageStream, bitmap.RawFormat );
            imageStream.Seek( 0, SeekOrigin.Begin );

            // set the size limit behavior back to what it was
            try
            {
                ImageResizer.ImageBuilder.Current.Build( imageStream, croppedStream, resizeCropSettings );
            }
            finally
            {
                sizingPlugin.Limits.TotalBehavior = origLimit;
            }

            // Make sure Image is no bigger than maxwidth/maxheight.  Default to whatever imageresizer's limits are set to
            int maxWidth = this.MaxImageWidth ?? sizingPlugin.Limits.TotalSize.Width;
            int maxHeight = this.MaxImageHeight ?? sizingPlugin.Limits.TotalSize.Height;
            croppedStream.Seek( 0, SeekOrigin.Begin );
            System.Drawing.Bitmap croppedBitmap = new System.Drawing.Bitmap( croppedStream );

            if ( ( croppedBitmap.Width > maxWidth ) || ( croppedBitmap.Height > maxHeight ) )
            {
                string resizeParams = string.Format( "width={0}&height={1}", maxWidth, maxHeight );
                MemoryStream croppedAndResizedStream = new MemoryStream();
                ImageResizer.ResizeSettings resizeSettings = new ImageResizer.ResizeSettings( resizeParams );
                croppedStream.Seek( 0, SeekOrigin.Begin );
                ImageResizer.ImageBuilder.Current.Build( croppedStream, croppedAndResizedStream, resizeSettings );
                return croppedAndResizedStream;
            }
            else
            {
                return croppedStream;
            }
        }

        /// <summary>
        /// Handles the Click event of the _lbUploadImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbUploadImage_Click( object sender, EventArgs e )
        {
            _lbShowModal_Click( _lbUploadImage, e );
        }

        /// <summary>
        /// Handles the Click event of the _lbShowModal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbShowModal_Click( object sender, EventArgs e )
        {
            // return if there's no image to crop or if this is a new upload before the image is replaced
            if ( !BinaryFileId.HasValue || ( sender == _lbUploadImage && OriginalBinaryFileId == BinaryFileId ) )
            {
                // no image to crop. they probably canceled the image upload dialog
                return;
            }

            CropBinaryFileId = BinaryFileId;

            if ( sender == _lbUploadImage )
            {
                // we are uploading a new file and might cancel cropping it, so set the base ImageId to null
                BinaryFileId = null;
            }

            _nbImageWarning.Visible = false;
            _nbErrorMessage.Visible = false;
            var binaryFile = new BinaryFileService( new RockContext() ).Get( CropBinaryFileId ?? 0 );
            if ( binaryFile != null )
            {
                _imgCropSource.ImageUrl = ( (RockPage)Page ).ResolveRockUrl( "~/GetImage.ashx?guid=" + binaryFile.Guid.ToString() );
                if ( binaryFile.MimeType != "image/svg+xml" )
                {
                    using ( var stream = binaryFile.ContentStream )
                    {
                        if ( stream != null )
                        {
                            try
                            {
                                var bitMap = new System.Drawing.Bitmap( stream );
                                _imgCropSource.Width = bitMap.Width;
                                _imgCropSource.Height = bitMap.Height;
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex );
                                _imgCropSource.Width = Unit.Empty;
                                _imgCropSource.Height = Unit.Empty;
                                _nbErrorMessage.Visible = true;
                                BinaryFileId = null;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    _imgCropSource.Width = Unit.Empty;
                    _imgCropSource.Height = Unit.Empty;
                    _nbImageWarning.Visible = true;
                    _nbImageWarning.Text = "SVG image cropping is not supported.";
                }
            }
            else
            {
                _imgCropSource.ImageUrl = "";
            }

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
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _nbErrorMessage.RenderControl( writer );
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "image-editor-group imageupload-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "image-editor-photo" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.Write( @"
                <div class='js-upload-progress' style='display:none'>
                    <i class='fa fa-refresh fa-3x fa-spin'></i>
                </div>" );

            string backgroundImageFormat = "<div class='image-container' id='{0}' style='background-image:url({1});background-size:cover;background-position:50%'></div>";
            string imageDivHtml = "";

            if ( BinaryFileId != null )
            {
                imageDivHtml = string.Format( backgroundImageFormat, this.ClientID + "_divPhoto", this.ResolveUrl( "~/GetImage.ashx?id=" + BinaryFileId.ToString() + "&width=150" ) );
            }
            else
            {
                imageDivHtml = string.Format( backgroundImageFormat, this.ClientID + "_divPhoto", this.NoPictureUrl );
            }

            writer.Write( imageDivHtml );
            writer.WriteLine();

            if ( string.IsNullOrEmpty( ButtonCssClass ) )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "options" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // We only show the Modal/Editor if the delete button is enabled and
            // we have a file already.
            if ( ShowDeleteButton && ( BinaryFileId ?? 0 ) > 0 )
            {
                _lbShowModal.Style[HtmlTextWriterStyle.Display] = string.Empty;
                _lbUploadImage.Style[HtmlTextWriterStyle.Display] = "none";
            }
            else
            {
                _lbShowModal.Style[HtmlTextWriterStyle.Display] = "none";
                _lbUploadImage.Style[HtmlTextWriterStyle.Display] = string.Empty;
            }

            _lbShowModal.RenderControl( writer );
            _lbUploadImage.Text = this.ButtonText;
            _lbUploadImage.CssClass = this.ButtonCssClass;
            _lbUploadImage.Attributes["onclick"] = "return false;";
            _lbUploadImage.RenderControl( writer );

            // render the circle check status for when save happens
            writer.WriteLine();
            _lSaveStatus.RenderControl( writer );

            // Don't render the _aRemove control if there is no BinaryFile to remove.
            if ( BinaryFileId != null )
            {
                writer.WriteLine();
                _aRemove.RenderControl( writer );
            }

            writer.WriteLine();
            writer.RenderEndTag();
            writer.WriteLine();

            _hfBinaryFileId.RenderControl( writer );
            writer.WriteLine();
            _hfOriginalBinaryFileId.RenderControl( writer );
            writer.WriteLine();
            _hfCropBinaryFileId.RenderControl( writer );
            writer.WriteLine();
            _hfBinaryFileTypeGuid.RenderControl( writer );
            writer.WriteLine();

            writer.AddAttribute( "class", "image-editor-fileinput" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _fileUpload.Attributes["name"] = string.Format( "{0}[]", this.ID );
            _fileUpload.RenderControl( writer );
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderEndTag(); // image-editor-photo
            writer.WriteLine();

            writer.RenderEndTag(); // image-editor-group

            _mdImageDialog.RenderControl( writer );

            RegisterStartupScript();
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

            var jsDoneFunction = string.Format( "window.location = $('#{0}').prop('href');", _lbUploadImage.ClientID );

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
    setImageUrlOnUpload: false,
    noPictureUrl: '{11}',
    doneFunction: function (e, data) {{
        // toggle the edit/upload buttons
        $('#{8}').hide();
        $('#{9}').show();

        // postback to show Modal after uploading new image
        {10}
    }},
    maxUploadBytes: {12}
}});

$('#{6}').Jcrop({{
    aspectRatio:1,
    setSelect:   [0,0, $('#{6}').width(), $('#{6}').height() ],
    boxWidth:480,
    boxHeight:480,
    onSelect: function(c) {{
        $('#{7}').val(c.x.toFixed() + ',' + c.y.toFixed() + ',' + c.w.toFixed() + ',' + c.h.toFixed() + ',');
    }}
}});

// prompt to upload image
$('#{8}').click( function (e, data) {{
    $('#{0}').click();
}});

// hide/show buttons and remove this button when remove is clicked (note: imageUploader.js also does stuff when remove is clicked)
$('#{5}').click(function () {{
    $('#{8}').show();
    $('#{9}').hide();
    $('#{5}').remove();
}});

",
                _fileUpload.ClientID, // {0}
                this.BinaryFileId, // {1}
                this.BinaryFileTypeGuid, // {2}
                _hfBinaryFileId.ClientID, // {3}
                this.ClientID + "_divPhoto", // {4}
                _aRemove.ClientID, // {5}
                _imgCropSource.ClientID, // {6}
                _hfCropCoords.ClientID, // {7}
                _lbUploadImage.ClientID, // {8}
                _lbShowModal.ClientID, // {9}
                jsDoneFunction, // {10}
                this.NoPictureUrl, // {11}
                maxUploadBytes.HasValue ? maxUploadBytes.Value.ToString() : "null"  // {12}
                );

            _lbUploadImage.Enabled = false;

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
                _fileUpload.Visible = value;
                _aRemove.Visible = value;
            }
        }
    }
}