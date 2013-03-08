//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;

using Rock;

namespace Rock.Web.UI.Adapters
{
    /// <summary>
    /// Control adapter for checkbox list
    /// </summary>
    public class CheckBoxListAdapter : WebControlAdapter
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
            CheckBoxList cbl = Control as CheckBoxList;
            if ( cbl != null )
            {
                int i = 0;
                foreach ( ListItem li in cbl.Items )
                {
                    writer.WriteLine();

                    writer.AddAttribute( "class", "checkbox" + ( cbl.RepeatDirection == RepeatDirection.Horizontal ? " inline" : string.Empty ) );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );

                    string itemId = string.Format( "{0}_{1}", cbl.ClientID, i );
                    writer.AddAttribute( "id", itemId );
                    writer.AddAttribute( "type", "checkbox" );
                    writer.AddAttribute( "name", string.Format( "{0}${1}", cbl.UniqueID, i++ ) );
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

                    writer.RenderBeginTag( HtmlTextWriterTag.Input );
                    writer.RenderEndTag();

                    writer.Write( li.Text );

                    writer.RenderEndTag();

                    if ( Page != null && Page.ClientScript != null )
                    {
                        Page.ClientScript.RegisterForEventValidation( cbl.UniqueID, li.Value );
                    }
                }

                if ( Page != null && Page.ClientScript != null )
                {
                    Page.ClientScript.RegisterForEventValidation( cbl.UniqueID );
                }
            }
        }
    }
}