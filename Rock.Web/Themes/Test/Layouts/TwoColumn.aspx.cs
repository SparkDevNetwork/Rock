using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms;

namespace Rock.Themes.Test.Layouts
{
    public partial class TwoColumn : CmsPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
        }

        protected override void DefineZones()
        {
            Zones.Add( "Header", Heading );
            Zones.Add( "FirstColumn", FirstColumn );
            Zones.Add( "SecondColumn", SecondColumn );
            Zones.Add( "Footer", Footer );
        }
    }
}