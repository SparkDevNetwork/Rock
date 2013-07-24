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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // Load Campuses Drop Down
                CampusService campusService = new CampusService();
                List<Campus> campusList = new List<Campus>();
                campusList = campusService.Queryable().OrderBy( n => n.Name ).ToList();
                ddlCampus.DataSource = campusList;
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, new ListItem( "Choose A Campus", None.IdValue ) );

                // script to get info from local storage
                string script = string.Format( @"
                    <script>
                        $(document).ready(function (e) {{
                            if (localStorage) {{
                                if (localStorage.checkInKiosk) {{
                                    $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                                    if (localStorage.checkInGroupTypes) {{
                                        $('[id$=""hfGroupTypes""]').val(localStorage.checkInGroupTypes);
                                    }}
                                    {0};
                                }}
                            }}
                        }});
                    </script>
                ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                phScript.Controls.Add( new LiteralControl( script ) );

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
                // switch ( kiosk.Name.Substring( 0, 3 ) )
                // {
                //     case "AND":
                //         ddlCampus.Items.FindByText( "Anderson" ).Selected = true;
                //         break;
                //     case "CHS":
                //         ddlCampus.Items.FindByText( "Charleston" ).Selected = true;
                //         break;
                //     case "COL":
                //         ddlCampus.Items.FindByText( "Columbia" ).Selected = true;
                //         break;
                //     case "FLO":
                //         ddlCampus.Items.FindByText( "Florence" ).Selected = true;
                //         break;
                //     case "GVL":
                //         ddlCampus.Items.FindByText( "Greenville" ).Selected = true;
                //         break;
                //     case "GWD":
                //         ddlCampus.Items.FindByText( "Greenwood" ).Selected = true;
                //         break;
                //     case "MYR":
                //         ddlCampus.Items.FindByText( "Myrtle Beach" ).Selected = true;
                //         break;
                //     case "SPA":
                //         ddlCampus.Items.FindByText( "Spartanburg" ).Selected = true;
                //         break;
                //     case "CEN":
                //         ddlCampus.Items.FindByText( "Central" ).Selected = true;
                //         break;
                //     default:
                //         // if the campus cannot be found in the first three letters of the device name...then don't set it automatically.
                //         foundKioskCampus = false;
                //         break;
                // }
            }
            else
            {
                phScript.Controls.Clear();
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Used by the local storage script to rebind the group types if they were previously
        /// saved via local storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            BindGroupTypes( hfGroupTypes.Value );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            foreach ( RepeaterItem item in repMinistry.Items )
            {
                ( (Button)item.FindControl( "lbMinistry" ) ).RemoveCssClass( "active" );
            }

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

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            // Check to make sure they picked a ministry
            if ( !string.IsNullOrEmpty( hfSelected.Value ) )
            {
                CurrentGroupTypeIds = hfSelected.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
            }
            else
            {
                maWarning.Show( "At least one ministry must be selected!", ModalAlertType.Warning );
                return;
            }

            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();
            NavigateToNextPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the group types if there are values saved from local storage
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void BindGroupTypes( string selectedValues )
        {
            var selectedItems = selectedValues.Split( ',' );

            // make sure none of the ministry items are selected.
            foreach ( RepeaterItem item in repMinistry.Items )
            {
                ( (Button)item.FindControl( "lbMinistry" ) ).RemoveCssClass( "active" );
            }

            var kiosk = new DeviceService().GetByDeviceName( ddlCampus.SelectedValue );
            kiosk = new DeviceService().Get( 2 );   // ***************** TEMPORARY ******************** //
            repMinistry.DataSource = kiosk.GetLocationGroupTypes();
            repMinistry.DataBind();

            if ( selectedValues != string.Empty )
            {
                foreach ( string id in selectedValues.Split( ',' ) )
                {
                    foreach ( RepeaterItem item in repMinistry.Items )
                    {
                        var linky = (Button)item.FindControl( "lbMinistry" );
                        if ( linky.CommandArgument == id )
                        {
                            linky.AddCssClass( "active" );
                            hfSelected.Value += linky.CommandArgument + ',';
                        }
                    }
                }
            }
            else
            {
                if ( CurrentGroupTypeIds != null )
                {
                    foreach ( int id in CurrentGroupTypeIds )
                    {
                        foreach ( RepeaterItem item in repMinistry.Items )
                        {
                            var linky = (Button)item.FindControl( "lbMinistry" );
                            if ( int.Parse( linky.CommandArgument ) == id )
                            {
                                linky.AddCssClass( "active" );
                                hfSelected.Value += linky.CommandArgument + ',';
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether webcontrol has a CSS class of "active".
        /// </summary>
        /// <param name="webcontrol">The webcontrol.</param>
        /// <returns>
        ///   <c>true</c> if the webcontrol has an "active" CSS class; otherwise, <c>false</c>.
        /// </returns>
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