using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI;
using com.blueboxmoon.AcmeCertificate;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Challenge" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Responds to challenges for the Acme certification system." )]
    public partial class AcmeChallenge : RockBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "Token" ) ) )
                {
                    Response.Clear();
                    Response.Write( AcmeHelper.GetAuthorizationForToken( PageParameter( "Token" ) ) );
                    Response.Flush();
                    Response.SuppressContent = true;
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}
