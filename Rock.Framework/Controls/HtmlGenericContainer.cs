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
        public HtmlGenericContainer()
        {
        }

        public HtmlGenericContainer( string tag )
            : base( tag )
        {
        }
    }
}