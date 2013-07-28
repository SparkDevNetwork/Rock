//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Check-In Administration block" )]
    [BooleanField( "Allow Manual Setup", "If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.", true )]
    [BooleanField( "Enable Location Sharing", "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.", false, "Geo Location", 0 )]
    [IntegerField( "Time to Cache Kiosk GeoLocation", "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.", false, 20, "Geo Location", 1 )]
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
                AttemptKioskMatchByIpOrName();

                // script to get info from local storage
                string script = string.Format( @"
                    <script>
                        $(document).ready(function (e) {{
                            if (localStorage) {{
                                if (localStorage.checkInKiosk) {{
                                    $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                                    if (localStorage.checkInGroupTypes) {{
                                        $('[id$=""hfParentTypes""]').val(localStorage.checkInGroupTypes);
                                    }}
                                    {0};
                                }}
                            }}
                        }});
                    </script>
                ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                phScript.Controls.Add( new LiteralControl( script ) );

                if ( CurrentKioskId == null )
                {   // #DEBUG, might be a local machine
                    // DEVICE because KioskCache location is always null
                    var kiosk = new DeviceService().GetByDeviceName( Environment.MachineName );
                    if ( kiosk != null )
                    {   
                        CurrentKioskId = kiosk.Id;
                        hfKiosk.Value = CurrentKioskId.ToString();
                        BindGroupTypes();
                    }
                }
                else
                {
                    maWarning.Show( "This device has not been set up for check in.", ModalAlertType.Warning );
                    return;
                }
                                
                SaveState();
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
            BindGroupTypes( hfParentTypes.Value );
        }
        
        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            // Check to make sure they picked a ministry
            if ( !string.IsNullOrEmpty( hfParentTypes.Value ) )
            {
                var parentGroupTypeIds = hfParentTypes.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
                

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
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void AttemptKioskMatchByIpOrName()
        {
            // try to find matching kiosk by REMOTE_ADDR (ip/name).
            var ipAddr = Request.ServerVariables["REMOTE_ADDR"];
            if ( Regex.IsMatch( ipAddr, @"\d+\.\d+\.\d+\.\d+" ) )
            {
                var kioskStatus = KioskCache.GetKiosk( ipAddr, skipReverseLookup: false );
                if ( kioskStatus != null )
                {
                    CurrentKioskId = kioskStatus.Device.Id;
                    CurrentGroupTypeIds = GetAllKiosksGroupTypes( kioskStatus.Device ); ;
                    CurrentCheckInState = null;
                    CurrentWorkflow = null;
                    SaveState();
                    NavigateToNextPage();
                }
            }

        }

        /// <summary>
        /// Binds the group types.
        /// </summary>
        private void BindGroupTypes()
        {
            BindGroupTypes( string.Empty );
        }

        /// <summary>
        /// Binds the group types if there are values saved from local storage
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void BindGroupTypes( string selectedValues )
        {
            var selectedItems = selectedValues.Split( ',' );

            if ( CurrentKioskId != null )
            {
                var groupTypeIds = GetKioskParentGroupTypes( (int)CurrentKioskId );
                repMinistry.DataSource = new GroupTypeService().Queryable().Where( gt => groupTypeIds.Contains( gt.Id ) ).ToList();
                repMinistry.DataBind();
                
            }

            // do something here with selected values
        }

        /// <summary>
        /// Handles the ItemDataBound event of the repMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repMinistry_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( CurrentGroupTypeIds != null )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( CurrentGroupTypeIds.Contains( ( (GroupType)e.Item.DataItem ).Id ) )
                    {
                        ( (Button)e.Item.FindControl( "lbMinistry" ) ).AddCssClass( "active" );
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

        /// <summary>
        /// Returns a list of IDs that are the GroupTypes the kiosk is responsible for.
        /// </summary>
        /// <param name="kiosk"></param>
        /// <returns></returns>
        private List<int> GetAllKiosksGroupTypes( Device kiosk )
        {
            var groupTypes = kiosk.GetLocationGroupTypes();
            var groupTypeIds = groupTypes.Select( gt => gt.Id ).ToList();
            return groupTypeIds;
        }

        /// <summary>
        /// Gets the location parent group types.
        /// </summary>
        /// <param name="kioskDevice">The kiosks's device.</param>
        /// <returns></returns>
        private List<int> GetKioskParentGroupTypes( int kioskId )
        {
            var parentGroupTypes = new List<int>();
            var kiosk = new DeviceService().Get( kioskId );  // DEVICE because KioskCache location is always null
            foreach ( var groupTypes in new GroupLocationService().Queryable()
                .Where( gl => gl.Location.ParentLocationId == kiosk.LocationId )
                .Select( gl => gl.Group.GroupType.ParentGroupTypes ) )
            {
                foreach ( var groupType in groupTypes.Where( gt => !parentGroupTypes.Contains( gt.Id ) ) ) 
                {
                    parentGroupTypes.Add( groupType.Id );   
                }                            
            }

            return parentGroupTypes;
        }
        
        #endregion
        
}
}