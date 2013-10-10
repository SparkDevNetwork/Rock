//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Reflection;
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
                writer.AddAttribute( "class", "radio-inline" );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );

                writer.AddAttribute( HtmlTextWriterAttribute.Id, rb.ClientID );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "radio" );
                
                string uniqueGroupName = rb.GetType().GetProperty( "UniqueGroupName", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( rb, null ) as string;
                writer.AddAttribute( HtmlTextWriterAttribute.Name, uniqueGroupName );

                writer.AddAttribute( HtmlTextWriterAttribute.Value, rb.ClientID );

                if ( !string.IsNullOrWhiteSpace( rb.CssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, rb.CssClass );
                }
                
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

                if ( rb.AutoPostBack )
                {
                    PostBackOptions postBackOption = new PostBackOptions( rb, string.Empty );
                    if ( rb.CausesValidation && this.Page.GetValidators( rb.ValidationGroup ).Count > 0 )
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = rb.ValidationGroup;
                    }
                    if ( this.Page.Form != null )
                    {
                        postBackOption.AutoPostBack = true;
                    }
                    writer.AddAttribute( HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference( postBackOption, true ) );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();

                writer.Write( rb.Text );

                writer.RenderEndTag();      // Label

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( uniqueGroupName, rb.ID );
                }
            }
        }
    }
}