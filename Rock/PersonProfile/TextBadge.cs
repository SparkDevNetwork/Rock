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
        /// Gets the text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public abstract string GetText( Person person );

        /// <summary>
        /// Gets the type of the badge.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public virtual string GetBadgeType( Person person )
        {
            return string.Empty;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void Render( System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person != null )
            {
                string text = GetText( Person );
                string badgeType = GetBadgeType( Person );

                if ( !string.IsNullOrWhiteSpace( text ) )
                {
                    string css = "label";
                    if ( !string.IsNullOrWhiteSpace( badgeType ) )
                    {
                        css += " label-" + badgeType.ToLower();
                    }
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, css );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.Write( text );
                    writer.RenderEndTag();
                }
            }
        }
    }

}