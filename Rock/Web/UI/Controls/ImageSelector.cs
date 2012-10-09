//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls
{   
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:ImageSelector runat=server></{0}:ImageSelector>" )]
    public class ImageSelector : CompositeControl
    {
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
        public string ImageId
        {
            get
            {
                EnsureChildControls();
                return hiddenField.Value;
            }
            set
            {
                EnsureChildControls();
                hiddenField.Value = value;
            }
        }

        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            Rock.Web.UI.RockPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );
            Rock.Web.UI.RockPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.upload.min.js" );
            Rock.Web.UI.RockPage.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.common.min.css" );
            Rock.Web.UI.RockPage.AddCSSLink( this.Page, "~/CSS/Kendo/kendo.rock.min.css" );

            string script = string.Format( @"
    $(document).ready(function() {{

        $('#{0}').kendoUpload({{
            multiple: false,
            showFileList: false,
            async: {{
                saveUrl: rock.baseUrl + 'ImageUploader.ashx'
            }},

            success: function(e) {{

                if (e.operation == 'upload' && e.response != '0') {{
                    $('#{1}').val(e.response);
                    $('#{2}').attr('src',rock.baseUrl + 'Image.ashx?id=' + e.response + '&width=50&height=50');
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

    }});
        ", 
                fileUpload.ClientID, 
                hiddenField.ClientID, 
                image.ClientID,
                htmlAnchor.ClientID);
         
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "image-selector-kendo-" + this.ID.ToString(), script, true );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void  RenderControl(HtmlTextWriter writer)
        {
            writer.AddAttribute( "class", "rock-image" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !string.IsNullOrEmpty( ImageId ) && ImageId != "0" )
            {
                image.Style["display"] = "inline";
                image.ImageUrl = "~/image.ashx?" + ImageId + "&width=50&height=50";
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
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            image = new Image();
            image.ID = "img";
            Controls.Add( image );

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
    }
}