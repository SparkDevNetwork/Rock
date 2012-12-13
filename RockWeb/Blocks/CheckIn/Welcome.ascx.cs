//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock.CheckIn;
using Rock.Constants;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [TextField( 0, "Admin Page Url", "", "The url of the Check-In admin page", false, "~/checkin/welcome" )]
    [TextField( 1, "Search Page Url", "", "The url of the Check-In admin page", false, "~/checkin/welcome" )]
    [TextField( 2, "Family Select Page Url", "", "The url of the Check-In admin page", false, "~/checkin/welcome" )]
    [IntegerField( 3, "Workflow Type Id", "0", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in" )]
    public partial class Welcome : Rock.Web.UI.RockBlock
    {
        private KioskStatus kiosk;

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( Session["CheckInWorkflow"] != null )
                {
                    Session.Remove( "CheckInWorkflow" );
                }

                RefreshKioskData();
            }
        }

        // TODO: This is a timer event initiated client-side somehow
        protected void SomeTimerEvent()
        {
            RefreshKioskData();
        }

        private void RefreshKioskData()
        {
            string adminPageUrl = AttributeValue("AdminPageUrl");

            if ( Session["CheckInKioskId"] == null || Session["CheckInGroupTypeIds"] == null )
            {
                Response.Redirect( adminPageUrl, false );
                return;
            }

            int kioskId = (int)Session["CheckInKioskId"];
            var groupTypeIds = Session["CheckInGroupTypeIds"] as List<int>;

            var kioskStatus = KioskCache.Kiosks.Where( k => k.Id == kioskId ).FirstOrDefault();

            var result = Rock.Net.WebRequest.Get(string.Format("~/api/kiosks/{0}", kioskId));

        }
    }
}