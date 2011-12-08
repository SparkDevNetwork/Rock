using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Controls
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