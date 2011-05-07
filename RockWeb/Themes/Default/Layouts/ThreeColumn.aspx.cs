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
            
            // register scripts
            AddScriptLink( Page, "~/Scripts/jquery-1.5.min.js" );
            AddScriptLink( Page, "~/Scripts/jquery-ui-1.8.9.custom.min.js" );
            AddScriptLink( Page, "~/Themes/Default/Scripts/jquery-placeholder-plugin.js" );

            // register css
            AddCSSLink( Page, "~/css/reset-core.css" );
            AddCSSLink( Page, "~/css/cms-core.css" );
            AddCSSLink( Page, "~/Themes/Default/CSS/styles.css" );
            AddCSSLink( Page, "~/css/reset-core.css" );
            AddCSSLink( Page, "~/css/overcast/jquery-ui-1.8.9.custom.css" );

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