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
        private Label _lblTitle;
        private Image _imgThumbnail;
        private HiddenField _hfBinaryFileId;
        private HiddenField _hfBinaryFileTypeGuid;
        private FileUpload _fileUpload;
        private HtmlAnchor _aRemove;

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
                _fileUpload.ClientID,
                BinaryFileId,
                BinaryFileTypeGuid,
                _hfBinaryFileId.ClientID,
                _imgThumbnail.ClientID,
                _aRemove.ClientID );
            ScriptManager.RegisterStartupScript( _fileUpload, _fileUpload.GetType(), "KendoImageScript_" + this.ID, script, true );
        }

        /// <summary>
        /// Renders a lblTitle and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderControlGroupDiv = ( !string.IsNullOrWhiteSpace( Label ) );

            if ( renderControlGroupDiv )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _lblTitle.AddCssClass( "control-lblTitle" );
                _lblTitle.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }
            
            writer.AddAttribute( "class", "rock-imgThumbnail" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( BinaryFileId != null )
            {
                _imgThumbnail.Style["display"] = "inline";
                _imgThumbnail.ImageUrl = "~/GetImage.ashx?id=" + BinaryFileId.ToString() + "&width=50&height=50";
                _aRemove.Style[HtmlTextWriterStyle.Display] = "inline";
            }
            else
            {
                _imgThumbnail.Style["display"] = "none";
                _imgThumbnail.ImageUrl = string.Empty;
                _aRemove.Style[HtmlTextWriterStyle.Display] = "none";
            }

            _imgThumbnail.RenderControl( writer );

            _hfBinaryFileId.RenderControl( writer );
            _hfBinaryFileTypeGuid.RenderControl( writer );
            _aRemove.RenderControl( writer );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _fileUpload.Attributes["name"] = string.Format( "{0}[]", base.ID );
            _fileUpload.RenderControl( writer );
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
            _lblTitle = new Label();
            Controls.Add( _lblTitle );
            
            _imgThumbnail = new Image();
            _imgThumbnail.ID = "img";
            Controls.Add( _imgThumbnail );

            _lblTitle.AssociatedControlID = _imgThumbnail.ID;

            _hfBinaryFileId = new HiddenField();
            _hfBinaryFileId.ID = "hfBinaryFileId";
            Controls.Add( _hfBinaryFileId );

            _hfBinaryFileTypeGuid = new HiddenField();
            _hfBinaryFileTypeGuid.ID = "hfBinaryFileTypeGuid";
            Controls.Add( _hfBinaryFileTypeGuid );

            _aRemove = new HtmlAnchor();
            _aRemove.ID = "rmv";
            _aRemove.InnerText = "Remove";
            _aRemove.Attributes["class"] = "remove-imgThumbnail";
            Controls.Add( _aRemove );

            _fileUpload = new FileUpload();
            _fileUpload.ID = "fu";
            Controls.Add( _fileUpload );
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