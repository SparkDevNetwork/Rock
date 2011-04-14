using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms;

namespace Rock.Themes.Default.Layouts
{
    public partial class ThreeColumn : CmsPage
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lUserName.Text = this.UserName;
        }

        protected override void DefineZones()
        {
            Zones.Add( "Header", Heading );
            Zones.Add( "FirstColumn", FirstColumn );
            Zones.Add( "SecondColumn", SecondColumn );
            Zones.Add( "ThirdColumn", ThirdColumn );
            Zones.Add( "Footer", Footer );
        }
    }
}