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
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericContainer"/> class.
        /// </summary>
        /// <param name="tag">The name of the element for which this instance of the class is created.</param>
        public HtmlGenericContainer( string tag )
            : base( tag )
        {
        }
    }
}