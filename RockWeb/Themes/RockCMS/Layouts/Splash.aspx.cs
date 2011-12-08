using System;

namespace RockWeb.Themes.RockCMS.Layouts
{
    public partial class Splash : Rock.Web.UI.Page
    {
        protected override void OnInit( EventArgs e )
        {
            // register scripts
            AddScriptLink( Page, "~/Scripts/jquery-1.5.min.js" );
            AddScriptLink( Page, "~/Scripts/jquery-ui-1.8.9.custom.min.js" );
            AddScriptLink( Page, "~/Themes/Default/Scripts/jquery-placeholder-plugin.js" );
          
            base.OnInit( e );
        }

        protected override void DefineZones()
        {
            AddZone( "Content", Content );
        }
    }
}