//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
    public class FileUploader : CompositeControl, IPostBackEventHandler, ILabeledControl
    {

        #region UI Controls

        private Label _lblTitle;
        private HiddenField _hfBinaryFileId;
        private HiddenField _hfBinaryFileTypeGuid;
        private HtmlAnchor _aFileName;
        private HtmlAnchor _aRemove;
        private FileUpload _fileUpload;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledFileUploader" /> class.
        /// </summary>
        public FileUploader()
        {
            _lblTitle = new Label();
            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileTypeGuid = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Label
        {
            get
            {
                EnsureChildControls();
                return _lblTitle.Text;
            }

            set
            {
                EnsureChildControls();
                _lblTitle.Text = value;
            }
        }

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

        #region Control Methods

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            Controls.Add( _lblTitle );
            _lblTitle.ID = "lblTitle";

            Controls.Add( _hfBinaryFileId );
            _hfBinaryFileId.ID = "hfBinaryFileId";

            Controls.Add( _hfBinaryFileTypeGuid );
            _hfBinaryFileTypeGuid.ID = "hfBinaryFileTypeGuid";

            _aFileName = new HtmlAnchor();
            Controls.Add( _aFileName );
            _aFileName.ID = "fn";
            _aFileName.Target = "_blank";

            _aRemove = new HtmlAnchor();
            Controls.Add( _aRemove );
            _aRemove.ID = "rmv";
            _aRemove.HRef = "#";
            _aRemove.InnerText = "Remove";
            _aRemove.Attributes["class"] = "remove-file";

            _fileUpload = new FileUpload();
            Controls.Add( _fileUpload );
            _fileUpload.ID = "fu";
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderWithLabel = !string.IsNullOrEmpty( Label );

            if ( renderWithLabel )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _lblTitle.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( BinaryFileId != null )
            {
                _aFileName.HRef = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), BinaryFileId );
                _aFileName.InnerText = new BinaryFileService().Queryable().Where( f => f.Id == BinaryFileId ).Select( f => f.FileName ).FirstOrDefault();
                _aFileName.Style[HtmlTextWriterStyle.Display] = "inline";
                _aRemove.Style[HtmlTextWriterStyle.Display] = "inline";
            }
            else
            {
                _aFileName.HRef = string.Empty;
                _aFileName.InnerText = string.Empty;
                _aFileName.Style[HtmlTextWriterStyle.Display] = "none";
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfBinaryFileId.RenderControl( writer );
            _hfBinaryFileTypeGuid.RenderControl( writer );
            _aFileName.RenderControl( writer );
            writer.Write( " " );
            _aRemove.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _fileUpload.Attributes["name"] = string.Format( "{0}[]", base.ID );
            _fileUpload.RenderControl( writer );
            writer.RenderEndTag();

            if ( renderWithLabel )
            {
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            RegisterStartupScript();
        }

        private void RegisterStartupScript()
        {
            var postBackScript = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "FileUploaded" ), true );
            postBackScript = postBackScript.Replace( '\'', '"' );
            var script = string.Format( @"
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
            ScriptManager.RegisterStartupScript( _fileUpload, _fileUpload.GetType(), "KendoImageScript_" + this.ID, script, true );
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