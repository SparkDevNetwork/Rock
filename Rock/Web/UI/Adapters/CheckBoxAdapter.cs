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
    /// Control adapter for checkbox
    /// </summary>
    public class CheckBoxAdapter : WebControlAdapter
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
            CheckBox cb = Control as CheckBox;
            if ( cb != null )
            {
                writer.WriteLine();
                //writer.AddAttribute( "class", "checkbox inline" );
                //writer.RenderBeginTag( HtmlTextWriterTag.Label );

                writer.AddAttribute( "id", cb.ClientID );
                writer.AddAttribute( "type", "checkbox" );
                writer.AddAttribute( "name", cb.UniqueID );
                if ( cb.Checked )
                {
                    writer.AddAttribute( "checked", "checked" );
                }

                if ( !string.IsNullOrWhiteSpace( cb.CssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, cb.CssClass );
                }

                foreach ( var inputAttributeKey in cb.InputAttributes.Keys )
                {
                    var key = inputAttributeKey as string;
                    writer.AddAttribute( key, cb.InputAttributes[key] );
                }

                if (cb.AutoPostBack)
                {
                    PostBackOptions postBackOption = new PostBackOptions(cb, string.Empty);
                    if (cb.CausesValidation && this.Page.GetValidators(cb.ValidationGroup).Count > 0)
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = cb.ValidationGroup;
                    }
                    if (this.Page.Form != null)
                    {
                        postBackOption.AutoPostBack = true;
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference(postBackOption, true));
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                writer.Write( cb.Text );

                //writer.RenderEndTag();

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( cb.UniqueID );
                }
            }
        }
    }
}