using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms;

namespace Rock.Themes.Rock.Layouts
{
    public partial class RockInternal : CmsPage
    {
        protected override void OnInit( EventArgs e )
        {
            // register scripts
            AddScriptLink( Page, "~/Scripts/jquery-1.5.min.js" );
            AddScriptLink( Page, "~/Scripts/jquery-ui-1.8.9.custom.min.js" );
            AddScriptLink( Page, "~/Themes/Default/Scripts/jquery-placeholder-plugin.js" );

            base.OnInit( e );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
            imgCC.Src = this.ThemePath + "/Assets/Images/cc-license.png";
        }

        protected override void DefineZones()
        {
            AddZone( "Header", phHeader );
            AddZone( "Menu", Menu );
            AddZone( "ContentLeft", ContentLeft );
            AddZone( "Content", Content );
            AddZone( "ContentRight", ContentRight );
            AddZone( "UpperBand", UpperBand );
            AddZone( "LowerBand", LowerBand );
            AddZone( "LowerContentLeft", LowerContentLeft );
            AddZone( "LowerContent", LowerContent );
            AddZone( "LowerContentRight", LowerContentRight );
            AddZone( "Footer", Footer );
        }
    }
}