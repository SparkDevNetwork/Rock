//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
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
    [ToolboxData( "<{0}:FileSelector runat=server></{0}:FileSelector>" )]
    public class FileSelector : CompositeControl, ILabeledControl, IPostBackEventHandler
    {

        #region UI Controls

        private Label lblTitle;
        private HiddenField hfBinaryFileId;
        private HtmlAnchor aFileName;
        private HtmlAnchor aRemove;
        private FileUpload fileUpload;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSelector" /> class.
        /// </summary>
        public FileSelector()
        {
            lblTitle = new Label();
            hfBinaryFileId = new HiddenField();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        public int BinaryFileId
        {
            get { return hfBinaryFileId.ValueAsInt(); }
            set { hfBinaryFileId.Value = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get { return lblTitle.Text; }
            set { lblTitle.Text = value; }
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

            aFileName = new HtmlAnchor();
            Controls.Add( aFileName );
            aFileName.ID = "fn";
            aFileName.Target = "_blank";
            lblTitle.AssociatedControlID = aFileName.ID;

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
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            lblTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( BinaryFileId != 0 )
            {
                aFileName.HRef = string.Format( "{0}file.ashx?id={1}", ResolveUrl( "~" ), BinaryFileId );
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
            hfBinaryFileId.RenderControl( writer );
            aFileName.RenderControl( writer );
            writer.Write( " " );
            aRemove.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            fileUpload.Attributes["name"] = string.Format( "{0}[]", base.ID );
            fileUpload.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderEndTag();

            RegisterStartupScript();
        }

        private void RegisterStartupScript()
        {
            string script = string.Format(
@"
        function ConfigureFileUploaders(sender, args) {{
            $('#{0}').kendoUpload({{
                multiple: false,
                showFileList: false,
                async: {{
                    saveUrl: '{4}FileUploader.ashx?' + $('#{1}').val()
                }},

                success: function(e) {{

                    if (e.operation == 'upload' && e.response ) {{
                        $('#{1}').val(e.response.Id);
                        var $fileLink = $('#{2}');
                        $fileLink.attr('href','')
                        $fileLink.hide();   
                        $fileLink.text(e.response.FileName);        
                        $fileLink.attr('href','{4}file.ashx?id=' + e.response.Id);
                        $fileLink.show('fast', function() {{ 
                            if ($('#modal-scroll-container').length) {{
                                $('#modal-scroll-container').tinyscrollbar_update('relative');
                            }}
                        }});
                        $('#{3}').show('fast');
                        {5};
                    }}

                }}
            }});

            $('#{3}').click( function(e){{
                $(this).hide('fast');
                $('#{1}').val('0');
                var $fileLink = $('#{2}');
                $fileLink.attr('href','')
                $fileLink.hide('fast', function() {{ 
                    if ($('#modal-scroll-container').length) {{
                        $('#modal-scroll-container').tinyscrollbar_update('relative');
                    }}
                }});
                return false;
            }});

        }}

        // configure file uploaders         
        ConfigureFileUploaders(null, null);
  
        ",
                            fileUpload.ClientID,        // 0
                            hfBinaryFileId.ClientID,    // 1
                            aFileName.ClientID,         // 2
                            aRemove.ClientID,           // 3
                            ResolveUrl( "~" ),          // 4
                            this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "FileUploaded" ), true ) );    // 5

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