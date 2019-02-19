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


namespace RockWeb.Plugins.com_bemadev.Checkin
{
    [DisplayName( "Roster Button" )]
    [Category( "com_bemadev > Check-in" )]
    [Description( "Displays a button to print rosters for location" )]
    public partial class RosterButton : CheckInBlockMultiPerson
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var rockContext = new RockContext();
            //Get Report Page (if exists)
            var reportPage = new PageService( rockContext ).Get( Guid.Parse( "B3FAE8FA-4023-4239-9C28-F9D2C5285612" ) );
            if ( reportPage != null )
            {
                //only gets the first campus it finds from the first location it pulls in kiosk config
                var location = new DeviceService( rockContext ).Get( CurrentCheckInState.Kiosk.Device.Guid ).Locations.FirstOrDefault();
                if ( location != null )
                {
                    string locationIdList = "";
                    if ( this.CurrentKioskId.HasValue )
                    {
                        var groupTypesLocations = this.GetGroupTypesLocations( rockContext );
                        locationIdList = groupTypesLocations
                           .Select( a => a.Id)
                           .ToList().AsDelimited( "%2C" );
                    }
                    string url = string.Concat( "/Page/", reportPage.Id, "?CampusId=", location.CampusId, "&LocationIds=", locationIdList );
                    btnPrintRoster.Attributes.Add( "href", url );
                    btnPrintRoster.Attributes.Add( "target", "_blank" );
                    btnPrintRoster.Visible = true;


                }
                else
                {
                    btnPrintRoster.Visible = false;
                }

            }
            else
            {
                btnPrintRoster.Visible = false;
            }

        }
    }
}