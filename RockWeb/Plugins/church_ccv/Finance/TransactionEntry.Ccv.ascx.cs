using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Security;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication;

namespace RockWeb.Plugins.church_ccv.Finance
{
    [DisplayName( "Transaction Entry (CCV)" )]
    [Category( "CCV > Finance" )]
    [Description( "Custom CCV block that provides an optimized experience for giving" )]

    public partial class TransactionEntryCcv : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/main.css" ) );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/vendor.css" ) );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Plugins/church_ccv/Finance/scripts/vendor/modernizr.js" ), true );
        }
    }
}
