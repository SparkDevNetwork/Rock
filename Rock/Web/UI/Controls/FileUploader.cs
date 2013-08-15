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

        private Label lblTitle;
        private HiddenField hfBinaryFileId;
        private HiddenField hfBinaryFileTypeGuid;
        private HtmlAnchor aFileName;
        private HtmlAnchor aRemove;
        private FileUpload fileUpload;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledFileUploader" /> class.
        /// </summary>
        public FileUploader()
        {
            lblTitle = new Label();
            hfBinaryFileId = new HiddenField();
            hfBinaryFileTypeGuid = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return lblTitle.Text;
            }

            set
            {
                EnsureChildControls();
                lblTitle.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        public int BinaryFileId
        {
            get
            {
                EnsureChildControls();
                return hfBinaryFileId.ValueAsInt();
            }

            set
            {
                EnsureChildControls();
                hfBinaryFileId.Value = value.ToString();
            }
        }

        public Guid BinaryFileTypeGuid
        {
            get
            {
                EnsureChildControls();
                Guid guid;
                return Guid.TryParse( hfBinaryFileTypeGuid.Value, out guid ) ? guid : Guid.Empty;
            }

            set
            {
                EnsureChildControls();
                hfBinaryFileTypeGuid.Value = value.ToString();
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

            Controls.Add( lblTitle );
            lblTitle.ID = "lblTitle";

            Controls.Add( hfBinaryFileId );
            hfBinaryFileId.ID = "hfBinaryFileId";

            Controls.Add( hfBinaryFileTypeGuid );
            hfBinaryFileTypeGuid.ID = "hfBinaryFileTypeGuid";

            aFileName = new HtmlAnchor();
            Controls.Add( aFileName );
            aFileName.ID = "fn";
            aFileName.Target = "_blank";

            aRemove = new HtmlAnchor();
            Controls.Add( aRemove );
            aRemove.ID = "rmv";
            aRemove.HRef = "#";
            aRemove.InnerText = "Remove";
            aRemove.Attributes["class"] = "remove-file";

            fileUpload = new FileUpload();
            Controls.Add( fileUpload );
            fileUpload.ID = "fu";
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderWithLabel = !string.IsNullOrEmpty( LabelText );

            if ( renderWithLabel )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                lblTitle.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( BinaryFileId != 0 )
            {
                aFileName.HRef = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), BinaryFileId );
                aFileName.InnerText = new BinaryFileService().Queryable().Where( f => f.Id == BinaryFileId ).Select( f => f.FileName ).FirstOrDefault();
                aFileName.Style[HtmlTextWriterStyle.Display] = "inline";
                aRemove.Style[HtmlTextWriterStyle.Display] = "inline";
            }
            else
            {
                aFileName.HRef = string.Empty;
                aFileName.InnerText = string.Empty;
                aFileName.Style[HtmlTextWriterStyle.Display] = "none";
                aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( string.IsNullOrWhiteSpace( hfBinaryFileTypeGuid.Value ) )
            {
                throw new Exception( "BinaryFileTypeGuid must be set." );
            }

            hfBinaryFileId.RenderControl( writer );
            hfBinaryFileTypeGuid.RenderControl( writer );
            aFileName.RenderControl( writer );
            writer.Write( " " );
            aRemove.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            fileUpload.Attributes["name"] = string.Format( "{0}[]", base.ID );
            fileUpload.RenderControl( writer );
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
                fileUpload.ClientID,
                BinaryFileId,
                BinaryFileTypeGuid,
                hfBinaryFileId.ClientID,
                aFileName.ClientID,
                aRemove.ClientID,
                this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "FileUploaded" ), true ) );
            ScriptManager.RegisterStartupScript( fileUpload, fileUpload.GetType(), "KendoImageScript_" + this.ID, script, true );
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
                fileUpload.Visible = value;
                aRemove.Visible = value;
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