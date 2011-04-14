using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Controls
{
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