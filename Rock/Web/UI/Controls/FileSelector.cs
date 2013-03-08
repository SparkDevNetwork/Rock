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
            get { return ViewState["BinaryFileId"] as int?; }
            set { ViewState["BinaryFileId"] = value; }
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

            hfBinaryFileId = new HiddenField();
            hfBinaryFileId.Value = BinaryFileId.HasValue ? BinaryFileId.Value.ToString() : "0";
            Controls.Add( hfBinaryFileId );

            aFileName = new HtmlAnchor();
            Controls.Add( aFileName );
            aFileName.ID = "fn";
            lblTitle.AssociatedControlID = aFileName.ID;

            aRemove = new HtmlAnchor();
            Controls.Add( aRemove );
            aRemove.ID = "rmv";
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
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            lblTitle.AddCssClass( "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            lblTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            hfBinaryFileId.RenderControl( writer );
            aFileName.RenderControl( writer ); 
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
                    saveUrl: '{4}FileUploader.ashx'
                }},

                success: function(e) {{

                    if (e.operation == 'upload' && e.response && e.response.Id != 0 ) {{
                        $('#{1}').val(e.response.Id);
                        $('#{2}').attr('href','')
                        $('#{2}').hide();             
                        $('#{2}').attr('href','{4}file.ashx?id=' + e.response.Id);
                        $('#{2}').show('fast', function() {{ 
                            if ($('#modal-scroll-container').length) {{
                                $('#modal-scroll-container').tinyscrollbar_update('relative');
                            }}
                        }});
                        $('#{3}').show('fast');
                        {5};
                    }}

                }}
            }});

            $('#{3}').click( function(){{
                $(this).hide('fast');
                $('#{1}').val('');
                $('#{2}').attr('href','')
                $('#{2}').hide('fast', function() {{ 
                    if ($('#modal-scroll-container').length) {{
                        $('#modal-scroll-container').tinyscrollbar_update('relative');
                    }}
                }});
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
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "FileUploaded" )
            {
                BinaryFileId = hfBinaryFileId.ValueAsInt();
            }
        }
    }
}