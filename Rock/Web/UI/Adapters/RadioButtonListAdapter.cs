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
    /// Control adapter for radio button list
    /// </summary>
    public class RadioButtonListAdapter : WebControlAdapter
    {
        /// <summary>
        /// Creates the beginning tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderBeginTag( System.Web.UI.HtmlTextWriter writer )
        {
            // Preserve any classes that a developer put on the control (such as a "well") by wrapping it in a <div>.
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null && !string.IsNullOrEmpty( cbl.CssClass ) )
            {
                writer.AddAttribute( "class", cbl.CssClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }
        }

        /// <summary>
        /// Creates the ending tag for the Web control in the markup that is transmitted to the target browser.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderEndTag( System.Web.UI.HtmlTextWriter writer )
        {
            // Close the <div> tag we may have started in the BeginTag above.
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null && !string.IsNullOrEmpty( cbl.CssClass ) )
            {
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Generates the target-specific inner markup for the Web control to which the control adapter is attached.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> containing methods to render the target-specific output.</param>
        protected override void RenderContents( System.Web.UI.HtmlTextWriter writer )
        {
            RadioButtonList rbl = Control as RadioButtonList;
            if ( rbl != null )
            {
                PostBackOptions postBackOption = null;

                if ( rbl.AutoPostBack )
                {
                    postBackOption = new PostBackOptions( rbl, string.Empty );
                    if ( rbl.CausesValidation && this.Page.GetValidators( rbl.ValidationGroup ).Count > 0 )
                    {
                        postBackOption.PerformValidation = true;
                        postBackOption.ValidationGroup = rbl.ValidationGroup;
                    }
                    if ( this.Page.Form != null )
                    {
                        postBackOption.AutoPostBack = true;
                    }
                }

                WebControl control = new WebControl(HtmlTextWriterTag.Span);
                control.ID = rbl.ClientID;
                control.CopyBaseAttributes(rbl);
                control.RenderBeginTag(writer);

                int i = 0;
                foreach ( ListItem li in rbl.Items )
                {
                    writer.WriteLine();

                    if ( rbl.RepeatDirection == RepeatDirection.Vertical )
                    {
                        writer.AddAttribute( "class", "radio" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    }
                    else
                    {
                        writer.AddAttribute( "class", "radio-inline" );
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Label );

                    string itemId = string.Format( "{0}_{1}", rbl.ClientID, i++ );
                    writer.AddAttribute( "id", itemId );
                    writer.AddAttribute( "type", "radio" );
                    writer.AddAttribute( "name", rbl.UniqueID );
                    writer.AddAttribute( "value", li.Value );
                    if ( li.Selected )
                    {
                        writer.AddAttribute( "checked", "checked" );
                    }

                    foreach ( var attributeKey in li.Attributes.Keys )
                    {
                        var key = attributeKey as string;
                        writer.AddAttribute( key, li.Attributes[key] );
                    }

                    if ( postBackOption != null )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference( postBackOption, true ) );
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();

                    writer.Write( li.Text );

                    writer.RenderEndTag();      // Label

                    if ( rbl.RepeatDirection == RepeatDirection.Vertical )
                    {
                        writer.RenderEndTag();  // Div
                    }

                    if ( rbl.Page != null )
                    {
                        rbl.Page.ClientScript.RegisterForEventValidation( rbl.UniqueID, li.Value );
                    }
                }

                control.RenderEndTag( writer );

                if ( rbl.Page != null )
                {
                    rbl.Page.ClientScript.RegisterForEventValidation( rbl.UniqueID );
                }
            }
        }
    }
}