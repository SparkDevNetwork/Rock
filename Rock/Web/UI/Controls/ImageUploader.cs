//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
        private Label label;
        private Image image;
        private HiddenField hiddenField;
        private FileUpload fileUpload;
        private HtmlAnchor htmlAnchor;

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
        Description( "Image Id" )
        ]
        public int? ImageId
        {
            get
            {
                EnsureChildControls();

                int id = 0;
                if ( int.TryParse( hiddenField.Value, out id ) )
                {
                    if ( id > 0 )
                    {
                        return id;
                    }
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                hiddenField.Value = value.HasValue ? value.Value.ToString() : string.Empty;
            }
        }

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
        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }

            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();

            string script = string.Format(
@"
    $(document).ready(function() {{

        function ConfigureImageUploader{0}(sender, args) {{
            $('#{0}').kendoUpload({{
                multiple: false,
                showFileList: false,
                async: {{
                    saveUrl: '{4}ImageUploader.ashx'
                }},

                success: function(e) {{

                    if (e.operation == 'upload' && e.response != '0') {{
                        $('#{1}').val(e.response.Id);
                        $('#{2}').attr('src','');
                        $('#{2}').hide();             
                        $('#{2}').attr('src','{4}Image.ashx?id=' + e.response.Id + '&width=50&height=50');
                        $('#{2}').show('fast', function() {{ 
                            if ($('#modal-scroll-container').length) {{
                                $('#modal-scroll-container').tinyscrollbar_update('relative');
                            }}
                        }});
                        $('#{3}').show('fast');
                    }}

                }}
            }});

            $('#{3}').click( function(){{
                $(this).hide('fast');
                $('#{1}').val('0');
                $('#{2}').attr('src','')
                $('#{2}').hide('fast', function() {{ 
                    if ($('#modal-scroll-container').length) {{
                        $('#modal-scroll-container').tinyscrollbar_update('relative');
                    }}
                }});
            }});

        }}

        // configure image uploaders         
        ConfigureImageUploader{0}(null, null);
    }});
        ",
                            fileUpload.ClientID,
                            hiddenField.ClientID,
                            image.ClientID,
                            htmlAnchor.ClientID,
                            ResolveUrl( "~" ) );

            ScriptManager.RegisterStartupScript( fileUpload, fileUpload.GetType(), "KendoImageScript_" + this.ID, script, true );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderControlGroupDiv = ( !string.IsNullOrWhiteSpace( LabelText ) );

            if ( renderControlGroupDiv )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                label.AddCssClass( "control-label" );
                label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }
            
            writer.AddAttribute( "class", "rock-image" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( ImageId.HasValue )
            {
                image.Style["display"] = "inline";
                image.ImageUrl = "~/image.ashx?" + ImageId.Value.ToString() + "&width=50&height=50";
            }
            else
            {
                image.Style["display"] = "none";
                image.ImageUrl = string.Empty;
            }

            image.RenderControl( writer );
            hiddenField.RenderControl( writer );
            htmlAnchor.RenderControl( writer );

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
            label = new Label();
            Controls.Add( label );
            
            image = new Image();
            image.ID = "img";
            Controls.Add( image );

            label.AssociatedControlID = image.ID;

            hiddenField = new HiddenField();
            hiddenField.ID = "hf";
            Controls.Add( hiddenField );

            htmlAnchor = new HtmlAnchor();
            htmlAnchor.ID = "rmv";
            htmlAnchor.InnerText = "Remove";
            htmlAnchor.Attributes["class"] = "remove-image";
            Controls.Add( htmlAnchor );

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
                htmlAnchor.Visible = value;
            }
        }
    }
}