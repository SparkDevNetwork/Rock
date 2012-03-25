//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field used to save and dispaly a text value
    /// </summary>
    public class Image : Field
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, bool condensed )
        {
            return string.Format( "<a href='{0}image.ashx?{1}' target='_blank'>Image</a>",
                parentControl.ResolveUrl( "~" ),
                value );
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="value">The current value</param>
        /// <param name="setValue">Should the control's value be set</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control CreateControl( string value, bool setValue )
        {
            Rock.Web.UI.Controls.ImageSelector imageSelector = new Web.UI.Controls.ImageSelector();
            imageSelector.ImageId = value;
            return imageSelector;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is Rock.Web.UI.Controls.ImageSelector )
                return ( ( Rock.Web.UI.Controls.ImageSelector )control ).ImageId;
            return null;
        }

        /// <summary>
        /// Creates a client-side function that can be called to display appropriate html and event handler to update the target element.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="id">The id.</param>
        /// <param name="id">The current value.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="targetElement">The target element.</param>
        /// <returns></returns>
        public override string ClientUpdateScript( Page page, string id, string value, string parentElement, string targetElement )
        {
            Rock.Web.UI.Page.AddScriptLink( page, "~/scripts/Kendo/kendo.core.min.js" );
            Rock.Web.UI.Page.AddScriptLink( page, "~/scripts/Kendo/kendo.upload.min.js" );
            Rock.Web.UI.Page.AddCSSLink( page, "~/CSS/Kendo/kendo.common.min.css" );
            Rock.Web.UI.Page.AddCSSLink( page, "~/CSS/Kendo/kendo.rock.min.css" );

            string uniqueId = parentElement + ( string.IsNullOrWhiteSpace( id ) ? "" : "_" + id );
            string functionName = uniqueId + "_Save_" + this.GetType().Name;

            string script = string.Format( @"

    function {0}(value){{

        var hasValue = value != '';
        var html = '';

        if (value != '' && value != '0')
            html = '<div class=""rock-image""><img id=""{1}_img"" src=""' + rock.baseUrl + 'image.ashx?' + value + '&width=50&height=50"" style=""display:inline""><a id=""{1}_rmv"" class=""remove-image"" style=""display:inline"">Remove</a><input id=""{1}_fu"" type=""file""></div>';
        else
            html = '<div class=""rock-image""><img id=""{1}_img"" src="""" style=""display:none""><a id=""{1}_rmv"" class=""remove-image"" style=""display:none"">Remove</a><input id=""{1}_fu"" type=""file""></div>';

        $('#{2}').html( html );

        $('#{1}_fu').kendoUpload({{
            multiple: false,
            showFileList: false,
            async: {{
                saveUrl: rock.baseUrl + 'ImageUploader.ashx'
            }},

            success: function(e) {{

                if (e.operation == 'upload' && e.response != '0') {{
                    $('#{3}').val(e.response);
                    $('#{1}_img').attr('src',rock.baseUrl + 'Image.ashx?id=' + e.response + '&width=50&height=50');
                    $('#{1}_img').show('fast');
                    $('#{1}_rmv').show('fast');
                }}

            }}
        }});

        $('#{1}_rmv').click( function(){{
            $(this).hide('fast');
            $('#{3}').val('0');
            $('#{1}_img').attr('src','')
            $('#{1}_img').hide('fast');
        }});
    }}

", functionName, uniqueId, parentElement, targetElement );

            page.ClientScript.RegisterStartupScript( this.GetType(), functionName, script, true );

            return functionName;
        }
    }
}