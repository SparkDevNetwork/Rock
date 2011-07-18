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
            AddScriptLink( Page, "~/Scripts/colorbox/jquery.colorbox.js" );

            AddCSSLink( this, "~/Themes/Rock/CSS/colorbox.css" );
          
            base.OnInit( e );

        }

        protected override void DefineZones()
        {
            Zones.Add( "Header", Header );
            Zones.Add( "Menu", Menu );
            Zones.Add( "ContentLeft", ContentLeft );
            Zones.Add( "Content", Content );
            Zones.Add( "ContentRight", ContentRight );
            Zones.Add( "UpperBand", UpperBand );
            Zones.Add( "LowerBand", LowerBand );
            Zones.Add( "LowerContentLeft", LowerContentLeft );
            Zones.Add( "LowerContent", LowerContent );
            Zones.Add( "LowerContentRight", LowerContentRight );
            Zones.Add( "Footer", Footer );
        }
    }
}