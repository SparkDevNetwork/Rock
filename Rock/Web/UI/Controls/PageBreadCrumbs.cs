//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    [ToolboxData( "<{0}:PageBreadCrumbs runat=server></{0}:PageBreadCrumbs>" )]
    public class PageBreadCrumbs : Control
    {
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Page is RockPage )
            {
                var rockPage = ( (RockPage)this.Page );
                if ( rockPage.CurrentPage.PageDisplayBreadCrumb )
                {
                    var crumbs = rockPage.BreadCrumbs;
                    if ( crumbs != null )
                    {
                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "breadcrumb" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                        foreach ( var crumb in crumbs )
                        {
                            if (!crumb.Active )
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
                        }

                        writer.RenderEndTag();
                    }
                }
            }
        }
    }
}
