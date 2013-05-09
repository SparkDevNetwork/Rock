//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for radio button
    /// </summary>
    public class RadioButtonAdapter : WebControlAdapter
    {
        /// <summary>
        /// Creates the beginning tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderBeginTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Creates the ending tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderEndTag( System.Web.UI.HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( System.Web.UI.HtmlTextWriter writer )
        {
            RadioButton rb = Control as RadioButton;
            if ( rb != null )
            {
                writer.WriteLine();
                writer.AddAttribute( "class", "radio inline" );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );

                writer.AddAttribute( "id", rb.ClientID );
                writer.AddAttribute( "type", "radio" );
                writer.AddAttribute( "name", rb.GroupName );
                writer.AddAttribute( "class", rb.CssClass );
                if ( rb.Checked )
                {
                    writer.AddAttribute( "checked", "checked" );
                }

                foreach ( var attributeKey in rb.Attributes.Keys )
                {
                    var key = attributeKey as string;
                    writer.AddAttribute( key, rb.Attributes[key] );
                }

                foreach ( var inputAttributeKey in rb.InputAttributes.Keys )
                {
                    var key = inputAttributeKey as string;
                    writer.AddAttribute( key, rb.InputAttributes[key] );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                writer.Write( rb.Text );

                writer.RenderEndTag();

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( rb.UniqueID );
                }
            }
        }
    }
}