//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Renders the description of a page
    /// </summary>
    public class PageDescription : Control
    {
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Page is RockPage )
            {
                var rockPage = (RockPage)this.Page;;

                if ( rockPage.DisplayDescription && !string.IsNullOrWhiteSpace( rockPage.Description ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "pageoverview-description" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( rockPage.Description );
                    writer.RenderEndTag();
                }
            }
        }
    }
}