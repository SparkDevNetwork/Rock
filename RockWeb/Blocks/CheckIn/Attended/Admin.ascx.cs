//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Check-In Administration block" )]
    public partial class Admin : CheckInBlock
    {
        #region Control Methods

        // OnLoad we want to:
        // A) Match IP from actual device to a record in the Device table
        // B) Get a list of campuses
        // C) Get the list of ministries
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // Load Campuses
                CampusService campusService = new CampusService();
                List<Campus> CampusList = new List<Campus>();
                CampusList = campusService.Queryable().OrderBy( n => n.Name ).ToList();
                ddlCampus.DataSource = CampusList;
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, new ListItem( "Choose A Campus", None.IdValue ) );

                // get the Device from the database based on the device name
                var kiosk = new DeviceService().GetByDeviceName( Environment.MachineName );

                // if we could auto-detect the kiosk, use that to load the ministry repeater. 
                // otherwise, show the campus drop down list and use that to get the kiosk.
                if ( kiosk != null )
                {
                    campusDiv.Visible = false;
                    kiosk = new DeviceService().Get( 2 );   // ***************** TEMPORARY ******************** //
                    CurrentKioskId = kiosk.Id;
                    repMinistry.DataSource = kiosk.GetLocationGroupTypes();
                    repMinistry.DataBind();
                }
                else
                {
                    campusDiv.Visible = true;
                }
                
                SaveState();

                // auto select the campus based on the first three letters of the Device Name
                //switch ( kiosk.Name.Substring( 0, 3 ) )
                //{
                //    case "AND":
                //        ddlCampus.Items.FindByText( "Anderson" ).Selected = true;
                //        break;
                //    case "CHS":
                //        ddlCampus.Items.FindByText( "Charleston" ).Selected = true;
                //        break;
                //    case "COL":
                //        ddlCampus.Items.FindByText( "Columbia" ).Selected = true;
                //        break;
                //    case "FLO":
                //        ddlCampus.Items.FindByText( "Florence" ).Selected = true;
                //        break;
                //    case "GVL":
                //        ddlCampus.Items.FindByText( "Greenville" ).Selected = true;
                //        break;
                //    case "GWD":
                //        ddlCampus.Items.FindByText( "Greenwood" ).Selected = true;
                //        break;
                //    case "MYR":
                //        ddlCampus.Items.FindByText( "Myrtle Beach" ).Selected = true;
                //        break;
                //    case "SPA":
                //        ddlCampus.Items.FindByText( "Spartanburg" ).Selected = true;
                //        break;
                //    case "CEN":
                //        ddlCampus.Items.FindByText( "Central" ).Selected = true;
                //        break;
                //    default:
                //        // if the campus cannot be found in the first three letters of the device name...then don't set it automatically.
                //        foundKioskCampus = false;
                //        break;
                //}

                // if the campus was auto found and selected, then let's load the ministries here.
                //if ( !string.IsNullOrEmpty( campusName ) )
                //{
                //    kiosk = new DeviceService().Get( 2 );   // ***************** TEMPORARY ******************** //
                //    ddlCampus.Items.FindByText( campusName ).Selected = true;
                //    repMinistry.DataSource = kiosk.GetLocationGroupTypes();
                //    repMinistry.DataBind();
                //}

                //var kiosk = new Device();
                //if ( CurrentKioskId.HasValue )
                //{
                //    // if there is a value already cached for the kiosk...get that.
                //    kiosk = new DeviceService().Get( CurrentKioskId.Value );
                //}
                //else
                //{
                //    // ...otherwise get the kiosk by it's name & IP address
                //    kiosk = new DeviceService().Get( 2 );
                //}
                //hfKioskId.Value = kiosk.Id.ToString();
                //CurrentKioskId = kiosk.Id;

            }
        }

        #endregion

        #region Edit Events

        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            foreach ( RepeaterItem item in repMinistry.Items )
            {
                ( (LinkButton)item.FindControl( "lbMinistry" ) ).RemoveCssClass( "active" );
            }
            //var kiosk = new Device();
            //if ( CurrentKioskId.HasValue )
            //{
            //    // if there is a value already cached for the kiosk...get that.
            //    kiosk = new DeviceService().Get( CurrentKioskId.Value );
            //}
            //else
            //{
            //    // ...otherwise get the kiosk by it's name & IP address
            //    kiosk = new DeviceService().Get( 2 );
            //}
            if ( ddlCampus.SelectedIndex == 0 )
            {
                repMinistry.DataSource = null;
            }
            else
            {
                var kiosk = new DeviceService().GetByDeviceName( ddlCampus.SelectedValue );
                kiosk = new DeviceService().Get( 2 );   // ***************** TEMPORARY ******************** //
                CurrentKioskId = kiosk.Id;
                repMinistry.DataSource = kiosk.GetLocationGroupTypes();
            }
            repMinistry.DataBind();
            SaveState();
        }

        protected void repMinistry_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            if ( HasActiveClass( (LinkButton)e.Item.FindControl( "lbMinistry" ) ) )
            {
                ( (LinkButton)e.Item.FindControl( "lbMinistry" ) ).RemoveCssClass( "active" );
            }
            else
            {
                ( (LinkButton)e.Item.FindControl( "lbMinistry" ) ).AddCssClass( "active" );
            }
        }

        protected void lbOk_Click( object sender, EventArgs e )
        {
            // Check to make sure they picked a campus
            //if ( ddlCampus.SelectedIndex == 0 )
            //{
            //    maWarning.Show( "Please choose a campus", ModalAlertType.Warning );
            //    return;
            //}

            // Check to make sure they picked a ministry
            var ministryChosen = false;
            List<int> ministryGroupTypeIds = new List<int>();
            foreach ( RepeaterItem item in repMinistry.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbMinistry" );
                var blah = linky.Attributes["class"];
                if ( HasActiveClass( linky ) )
                {
                    ministryChosen = true;
                    ministryGroupTypeIds.Add( int.Parse( linky.CommandArgument ) );
                }
            }

            if (!ministryChosen)
            {
                maWarning.Show( "At least one ministry must be selected!", ModalAlertType.Warning );
                return;
            }

            //CurrentKioskId = int.Parse( ddlKiosk.SelectedValue );
            //hfKiosk.Value = CurrentKioskId.ToString();
            //hfGroupTypes.Value = CurrentGroupTypeIds.AsDelimited( "," );
            //CurrentRoomGroupTypes = roomGroupTypes;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            CurrentGroupTypeIds = ministryGroupTypeIds;
            SaveState();
            NavigateToNextPage();
        }

        #endregion

        #region Internal Methods

        protected bool HasActiveClass( WebControl webcontrol )
        {
            string match = @"\s*\b" + "active" + @"\b";
            string css = webcontrol.CssClass;
            if ( System.Text.RegularExpressions.Regex.IsMatch( css, match, System.Text.RegularExpressions.RegexOptions.IgnoreCase ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}