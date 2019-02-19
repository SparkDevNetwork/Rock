using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using RockWeb;


namespace RockWeb.Plugins.com_bemaservices.Checkin
{
    [DisplayName( "Reprint Label Button" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Displays a button to print rosters for location" )]
    [LinkedPage( "Reprint Label Page" )]
    public partial class ReprintLabelButton : CheckInBlockMultiPerson
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        protected void btnReprint_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ReprintLabelPage" );
        }
    }
}