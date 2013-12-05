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
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile
{
    /// <summary>
    /// Base class for person profile icon badges
    /// </summary>
    public abstract class TextBadge : BadgeComponent
    {
        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public virtual HighlightLabel GetLabel( Person person )
        {
            return new HighlightLabel();
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void Render( System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person != null )
            {
                var label = GetLabel( Person );
                if ( label != null )
                {
                    label.RenderControl( writer );
                }
            }
        }
    }

}