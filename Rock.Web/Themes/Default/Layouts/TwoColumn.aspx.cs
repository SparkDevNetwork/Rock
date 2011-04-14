using System;
using Rock.Cms;

namespace Rock.Themes.Default.Layouts
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