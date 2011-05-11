using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Cms;

namespace Rock.Themes.Test.Layouts
{
    public partial class OneColumn : CmsPage
    {
        protected override void OnInit( EventArgs e )
        {
            AddCSSLink( Page, "~/CSS/reset-core.css" );
            AddCSSLink( Page, "~/CSS/cms-core.css" );
            AddCSSLink( Page, "../CSS/styles.css" );
            AddCSSLink( Page, "~/CSS/overcast/jquery-ui-1.8.9.custom.css" );

            AddScriptLink( Page, "~/Scripts/jquery-1.5.min.js" );
            AddScriptLink( Page, "~/Scripts/jquery-ui-1.8.9.custom.min.js" );
            AddScriptLink( Page, "~/Themes/Default/Scripts/jquery-placeholder-plugin.js" );

            base.OnInit( e );
        }

        protected override void DefineZones()
        {
            Zones.Add( "Header", Heading );
            Zones.Add( "Content", Content );
            Zones.Add( "Footer", Footer );

        }
    }
}