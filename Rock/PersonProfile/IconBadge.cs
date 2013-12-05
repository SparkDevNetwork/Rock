//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.PersonProfile
{
    /// <summary>
    /// Base class for person profile icon badges
    /// </summary>
    public abstract class IconBadge : BadgeComponent 
    {
        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="person">The person.</param>
        public abstract string GetToolTipText(Person person);

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        public abstract string GetIconPath( Person person );

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void Render( System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person != null )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "badge" );
                writer.AddAttribute( HtmlTextWriterAttribute.Title, GetToolTipText( Person ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Src, GetIconPath( Person ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Img );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }

}