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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ImageUploader runat=server></{0}:ImageUploader>" )]
    public class ImageUploader : CompositeControl, ILabeledControl
    {
        private Label lblTitle;
        private Image imgThumbnail;
        private HiddenField hfBinaryFileId;
        private HiddenField hfBinaryFileTypeGuid;
        private FileUpload fileUpload;
        private HtmlAnchor aRemove;

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
                return Guid.TryParse( hfBinaryFileTypeGuid.Value, out guid ) ? guid : new Guid( SystemGuid.BinaryFiletype.DEFAULT );
            }

            set 
            { 
                EnsureChildControls();
                hfBinaryFileTypeGuid.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the lblTitle text.
        /// </summary>
        /// <value>
        /// The lblTitle text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the lblTitle." )
        ]
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            EnsureChildControls();

            var script = string.Format( @"
Rock.controls.fileUploader.initialize({{
    controlId: '{0}',
    fileId: '{1}',
    fileTypeGuid: '{2}',
    hfFileId: '{3}',
    imgThumbnail: '{4}',
    aRemove: '{5}',
    fileType: 'image'
}});",
                fileUpload.ClientID,
                BinaryFileId,
                BinaryFileTypeGuid,
                hfBinaryFileId.ClientID,
                imgThumbnail.ClientID,
                aRemove.ClientID );
            ScriptManager.RegisterStartupScript( fileUpload, fileUpload.GetType(), "KendoImageScript_" + this.ID, script, true );
        }

        /// <summary>
        /// Renders a lblTitle and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderControlGroupDiv = ( !string.IsNullOrWhiteSpace( LabelText ) );

            if ( renderControlGroupDiv )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                lblTitle.AddCssClass( "control-lblTitle" );
                lblTitle.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }
            
            writer.AddAttribute( "class", "rock-imgThumbnail" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( BinaryFileId > 0 )
            {
                imgThumbnail.Style["display"] = "inline";
                imgThumbnail.ImageUrl = "~/GetImage.ashx?id=" + BinaryFileId.ToString() + "&width=50&height=50";
            }
            else
            {
                imgThumbnail.Style["display"] = "none";
                imgThumbnail.ImageUrl = string.Empty;
            }

            imgThumbnail.RenderControl( writer );

            hfBinaryFileId.RenderControl( writer );
            hfBinaryFileTypeGuid.RenderControl( writer );
            aRemove.RenderControl( writer );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            fileUpload.Attributes["name"] = string.Format( "{0}[]", base.ID );
            fileUpload.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderEndTag();

            if ( renderControlGroupDiv )
            {
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            lblTitle = new Label();
            Controls.Add( lblTitle );
            
            imgThumbnail = new Image();
            imgThumbnail.ID = "img";
            Controls.Add( imgThumbnail );

            lblTitle.AssociatedControlID = imgThumbnail.ID;

            hfBinaryFileId = new HiddenField();
            hfBinaryFileId.ID = "hfBinaryFileId";
            Controls.Add( hfBinaryFileId );

            hfBinaryFileTypeGuid = new HiddenField();
            hfBinaryFileTypeGuid.ID = "hfBinaryFileTypeGuid";
            Controls.Add( hfBinaryFileTypeGuid );

            aRemove = new HtmlAnchor();
            aRemove.ID = "rmv";
            aRemove.InnerText = "Remove";
            aRemove.Attributes["class"] = "remove-imgThumbnail";
            Controls.Add( aRemove );

            fileUpload = new FileUpload();
            fileUpload.ID = "fu";
            Controls.Add( fileUpload );
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
    }
}