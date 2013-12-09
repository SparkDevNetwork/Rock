//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
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
            HelpBlock = new HelpBlock();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // NOTE: The Script Registration is done in RenderBaseControl because Render has to be called before calling GetPostBackEventReference
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                string eventTarget = Page.Request.Params["__EVENTTARGET"] ?? string.Empty;
                EnsureChildControls();
                if ( eventTarget == _fileUpload.UniqueID )
                {
                    RaisePostBackEvent( Page.Request.Params["__EVENTARGUMENT"] );
                }
            }
        }

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
                Guid guid;
                return Guid.TryParse( _hfBinaryFileTypeGuid.Value, out guid ) ? guid : new Guid( SystemGuid.BinaryFiletype.DEFAULT );
            }

            set
            {
                EnsureChildControls();
                _hfBinaryFileTypeGuid.Value = value.ToString();
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

            Controls.Add( _hfBinaryFileTypeGuid );
            _hfBinaryFileTypeGuid.ID = this.ID + "_hfBinaryFileTypeGuid";

            _aFileName = new HtmlAnchor();
            Controls.Add( _aFileName );
            _aFileName.ID = "fn";
            _aFileName.Target = "_blank";
            _aFileName.AddCssClass("file-link");

            _aRemove = new HtmlAnchor();
            Controls.Add( _aRemove );
            _aRemove.ID = "rmv";
            _aRemove.HRef = "#";
            _aRemove.InnerHtml = "<i class='fa fa-times'></i>";
            _aRemove.Attributes["class"] = "remove-file";

            _fileUpload = new FileUpload();
            Controls.Add( _fileUpload );
            _fileUpload.ID = this.ID + "_fu";
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
            writer.AddAttribute("class", "fileupload-group");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);


            if ( BinaryFileId != null )
            {
                _aFileName.HRef = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), BinaryFileId );
                _aFileName.InnerText = new BinaryFileService().Queryable().Where( f => f.Id == BinaryFileId ).Select( f => f.FileName ).FirstOrDefault();
                _aFileName.AddCssClass("file-exists");
                _aRemove.Style[HtmlTextWriterStyle.Display] = "inline";
            }
            else
            {
                _aFileName.HRef = string.Empty;
                _aFileName.InnerText = string.Empty;
                _aFileName.RemoveCssClass("file-exists");
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            writer.AddAttribute("class", "fileupload-thumbnail");
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfBinaryFileId.RenderControl( writer );
            _hfBinaryFileTypeGuid.RenderControl( writer );
            _aFileName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute("class", "fileupload-remove");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            _aRemove.RenderControl(writer);
            writer.RenderEndTag();


            // render drop zone
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "fileupload-dropzone");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write("drop / click to upload");
            writer.RenderEndTag();

            _fileUpload.Attributes["name"] = string.Format("{0}[]", this.ID);
            _fileUpload.RenderControl(writer);
            writer.RenderEndTag();

            RegisterStartupScript();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            var postBackScript = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this._fileUpload, "FileUploaded" ), true );
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
    fileType: 'file'
}});",
                _fileUpload.ClientID,
                BinaryFileId,
                BinaryFileTypeGuid,
                _hfBinaryFileId.ClientID,
                _aFileName.ClientID,
                _aRemove.ClientID,
                postBackScript );
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