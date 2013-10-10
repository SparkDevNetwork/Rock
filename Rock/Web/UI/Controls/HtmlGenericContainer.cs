//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// An HtmlGenericContainer that implements the INamingContainer interface
    /// </summary>
    public class HtmlGenericContainer : HtmlGenericControl, INamingContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericContainer"/> class.
        /// </summary>
        public HtmlGenericContainer()
            : base( "div" )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericContainer" /> class.
        /// </summary>
        /// <param name="tag">The name of the element for which this instance of the class is created.</param>
        /// <param name="cssClass">The CSS class.</param>
        public HtmlGenericContainer( string tag, string cssClass = null )
            : base( tag )
        {
            if ( cssClass != null )
            {
                CssClass = cssClass;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                return this.Attributes["class"];
            }

            set
            {
                this.Attributes["class"] = value;
            }
        }
    }
}