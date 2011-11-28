using System;
using Rock.Cms;

namespace Rock.Themes.Default.Layouts
{
    public partial class TwoColumn : CmsPage
    {
        protected override void OnInit( EventArgs e )
        {
            AddScriptLink( Page, "~/Scripts/jquery-1.5.min.js" );
            AddScriptLink( Page, "~/Scripts/jquery-ui-1.8.9.custom.min.js" );
            AddScriptLink( Page, "~/Themes/Default/Scripts/jquery-placeholder-plugin.js" );
            base.OnInit( e );

            lUserName.Text = this.UserName;
        }

        protected override void DefineZones()
        {
            AddZone( "Header", Heading );
            AddZone( "FirstColumn", FirstColumn );
            AddZone( "SecondColumn", SecondColumn );
            AddZone( "Footer", Footer );
        }
    }
}