//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    [ToolboxData( "<{0}:BreadCrumbs runat=server></{0}:BreadCrumbs>" )]
    public class BreadCrumbs : Control
    {
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Page is RockPage )
            {
                var crumbs = ( (RockPage)this.Page ).BreadCrumbs;
                if ( crumbs != null )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "breadcrumb" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                    int i = 0;
                    foreach ( var crumb in crumbs )
                    {
                        if ( i < crumbs.Count - 1 )
                        {
                            writer.RenderBeginTag( HtmlTextWriterTag.Li );
                            writer.AddAttribute( HtmlTextWriterAttribute.Href, crumb.Url );
                            writer.RenderBeginTag( HtmlTextWriterTag.A );
                            writer.Write( crumb.Name );
                            writer.RenderEndTag();
                            writer.Write( " " );
                            writer.AddAttribute( HtmlTextWriterAttribute.Class, "divider" );
                            writer.RenderBeginTag( HtmlTextWriterTag.Span );
                            writer.Write( "/" );
                            writer.RenderEndTag();
                            writer.RenderEndTag();
                        }
                        else
                        {
                            writer.AddAttribute( HtmlTextWriterAttribute.Class, "active" );
                            writer.RenderBeginTag( HtmlTextWriterTag.Li );
                            writer.Write( crumb.Name );
                            writer.RenderEndTag();
                        }
                        i++;
                    }

                    writer.RenderEndTag();
                }
            }
        }
    }
}
