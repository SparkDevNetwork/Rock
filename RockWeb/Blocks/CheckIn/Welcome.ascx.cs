// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Welcome" )]
    [Category( "Check-in" )]
    [Description( "Welcome screen for check-in." )]

    [LinkedPage( "Family Select Page", "", false, "", "", 5 )]
    [LinkedPage( "Scheduled Locations Page", "", false, "", "", 6 )]

    [TextField( "Not Active Title", "Title displayed when there are not any active options today.", false, "Check-in Is Not Active", "Text", 7 )]
    [TextField( "Not Active Caption", "Caption displayed when there are not any active options today.", false, "There are no current or future schedules for this kiosk today!", "Text", 8 )]
    [TextField( "Not Active Yet Title", "Title displayed when there are active options today, but none are active now.", false, "Check-in Is Not Active Yet", "Text", 9 )]
    [TextField( "Not Active Yet Caption", "Caption displayed when there are active options today, but none are active now. Use {0} for a countdown timer.", false, "This kiosk is not active yet.  Countdown until active: {0}.", "Text", 10 )]
    [TextField( "Closed Title", "", false, "Closed", "Text", 11 )]
    [TextField( "Closed Caption", "", false, "This location is currently closed.", "Text", 12 )]
    [TextField( "Check-in Button Text", "The text to display on the check-in button. If left blank, 'Check-in' (or 'Start' when check-out is enabled) will be used.", false, "", "Text", 13, "CheckinButtonText" )]
    [TextField( "No Option Caption", "The text to display when there are not any families found matching a scanned identifier (barcode, etc).", false, "Sorry, there were not any families found with the selected identifier.", "Text", 14 )]

    public partial class Welcome : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentCheckInState == null )
            {
                NavigateToPreviousPage();
                return;
            }

            RockPage.AddScriptLink( "~/scripts/jquery.plugin.min.js" );
            RockPage.AddScriptLink( "~/scripts/jquery.countdown.min.js" );

            RegisterScript();

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-welcome-bg" );
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( CurrentCheckInState != null )
                {
                    string script = string.Format( @"
    <script>
        $(document).ready(function (e) {{
            if (localStorage) {{
                localStorage.theme = '{0}'
                localStorage.checkInKiosk = '{1}';
                localStorage.checkInType = '{2}';
                localStorage.checkInGroupTypes = '{3}';
            }}
        }});
    </script>
", CurrentTheme, CurrentKioskId, CurrentCheckinTypeId, CurrentGroupTypeIds.AsDelimited( "," ) );
                    phScript.Controls.Add( new LiteralControl( script ) );

                    CurrentWorkflow = null;
                    CurrentCheckInState.CheckIn = new CheckInStatus();
                    SaveState();
                    RefreshView();

                    lNotActiveTitle.Text = GetAttributeValue( "NotActiveTitle" );
                    lNotActiveCaption.Text = GetAttributeValue( "NotActiveCaption" );
                    lNotActiveYetTitle.Text = GetAttributeValue( "NotActiveTitle" );
                    lNotActiveYetCaption.Text = string.Format( GetAttributeValue( "NotActiveCaption" ), "<span class='countdown-timer'></span>" );
                    lClosedTitle.Text = GetAttributeValue( "ClosedTitle" );
                    lClosedCaption.Text = GetAttributeValue( "ClosedCaption" );

                    string btnText = GetAttributeValue( "CheckinButtonText" );
                    if ( string.IsNullOrWhiteSpace( btnText ) )
                    {
                        btnText = CurrentCheckInState.CheckInType.AllowCheckout ? "Start" : "Check In";
                    }
                    lbSearch.Text = string.Format( "<span>{0}</span>", btnText );
                }
            }
            else
            {
                if ( Request.Form["__EVENTARGUMENT"] != null )
                {
                    if ( Request.Form["__EVENTARGUMENT"] == "Wedge_Entry" )
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID );
                        DoFamilySearch( dv, hfSearchEntry.Value );
                    }
                    else if ( Request.Form["__EVENTARGUMENT"] == "Family_Id_Search" )
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );
                        DoFamilySearch( dv, hfSearchEntry.Value );
                    }
                }

            }
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            RefreshView();
        }

        /// <summary>
        /// Handles the Click event of the btnOverride control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnOverride_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "Override", "True" );
            NavigateToNextPage( queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSearch_Click( object sender, EventArgs e )
        {
            NavigateToNextPage();
        }

        /// <summary>
        /// Registers the script.
        /// </summary>
        private void RegisterScript()
        {
            var script = new StringBuilder();
            script.AppendFormat( @"

        function PostRefresh() {{
            {0};
        }}

", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
            ScriptManager.RegisterStartupScript( lbRefresh, lbRefresh.GetType(), "refresh-postback", script.ToString(), true );
        }

        private void ClearSelection()
        {
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();
        }

        // TODO: Add support for scanner
        /// <summary>
        /// Somes the scanner search.
        /// </summary>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="searchValue">The search value.</param>
        private void DoFamilySearch( DefinedValueCache searchType, string searchValue )
        {
            CurrentCheckInState.CheckIn.UserEnteredSearch = false;
            CurrentCheckInState.CheckIn.ConfirmSingleFamily = false;
            CurrentCheckInState.CheckIn.SearchType = searchType;
            CurrentCheckInState.CheckIn.SearchValue = searchValue;

            var errors = new List<string>();
            if ( ProcessActivity( "Family Search", out errors ) )
            {
                if ( !CurrentCheckInState.CheckIn.Families.Any() )
                {
                    maWarning.Show( string.Format( "<p>{0}</p>", GetAttributeValue( "NoMatchText" ) ), Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
                else
                {
                    SaveState();
                    NavigateToLinkedPage( "FamilySelectPage" );
                }
            }
            else
            {
                ClearSelection();
                string errorMsg = "<p>" + errors.AsDelimited( "<br/>" ) + "</p>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        private void RefreshView()
        {
            bool isActive = false;

            hfRefreshTimerSeconds.Value = ( CurrentCheckInType != null ? CurrentCheckInType.RefreshInterval.ToString() : "10" );
            pnlNotActive.Visible = false;
            pnlNotActiveYet.Visible = false;
            pnlClosed.Visible = false;
            pnlActive.Visible = false;
            ManagerLoggedIn = false;
            pnlManagerLogin.Visible = false;
            pnlManager.Visible = false;
            btnManager.Visible = ( CurrentCheckInType != null ? CurrentCheckInType.EnableManagerOption : true );
            btnOverride.Visible = ( CurrentCheckInType != null ? CurrentCheckInType.EnableOverride : true );

            lblActiveWhen.Text = string.Empty;

            if ( CurrentCheckInState == null || IsMobileAndExpiredDevice() )
            {
                NavigateToPreviousPage();
                return;
            }

            // Set to null so that object will be recreated with a potentially updated group type cache.
            CurrentCheckInState.CheckInType = null;

            if ( CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ).Count == 0 )
            {
                pnlNotActive.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasLocations( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                DateTime activeAt = CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ).Select( g => g.NextActiveTime ).Min();
                lblActiveWhen.Text = activeAt.ToString( "o" );
                pnlNotActiveYet.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasActiveLocations( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                pnlClosed.Visible = true;
            }
            else
            {
                isActive = true;
                pnlActive.Visible = true;
            }

            bool? wasActive = PageParameter( "IsActive" ).AsBooleanOrNull();
            if ( !wasActive.HasValue || wasActive.Value != isActive )
            {
                //redirect to current page with correct IsActive querystring value
                var qryParams = Request.QueryString.AllKeys.ToDictionary( k => k, k => this.Request.QueryString[k] );
                qryParams.AddOrReplace( "IsActive", isActive.ToString() );
                NavigateToCurrentPage( qryParams );
            }

        }

        /// <summary>
        /// Determines if the device is "mobile" and if it is no longer valid.
        /// </summary>
        /// <returns>true if the mobile device has expired; false otherwise.</returns>
        private bool IsMobileAndExpiredDevice()
        {
            if ( Request.Cookies[CheckInCookie.ISMOBILE] != null
                && Request.Cookies[CheckInCookie.DEVICEID] == null )
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Handles the Click event of the btnManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnManager_Click( object sender, EventArgs e )
        {
            ManagerLoggedIn = false;
            pnlNotActive.Visible = false;
            pnlNotActiveYet.Visible = false;
            pnlClosed.Visible = false;
            pnlActive.Visible = false;
            pnlManager.Visible = false;

            tbPIN.Text = string.Empty;

            // Get room counts
            List<int> locations = new List<int>();
            foreach ( var groupType in CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                var lUl = new HtmlGenericControl( "ul" );
                lUl.AddCssClass( "kioskmanager-count-locations" );
                phCounts.Controls.Add( lUl );

                foreach ( var location in groupType.KioskGroups.SelectMany( g => g.KioskLocations ).OrderBy( l => l.Location.Name ).Distinct() )
                {
                    if ( !locations.Contains( location.Location.Id ) )
                    {
                        locations.Add( location.Location.Id );
                        var locationAttendance = KioskLocationAttendance.Read( location.Location.Id );

                        if ( locationAttendance != null )
                        {
                            var lLi = new HtmlGenericControl( "li" );
                            lUl.Controls.Add( lLi );
                            lLi.InnerHtml = string.Format( "<strong>{0}</strong>: {1}", locationAttendance.LocationName, locationAttendance.CurrentCount );

                            var gUl = new HtmlGenericControl( "ul" );
                            gUl.AddCssClass( "kioskmanager-count-groups" );
                            lLi.Controls.Add( gUl );

                            foreach ( var groupAttendance in locationAttendance.Groups )
                            {
                                var gLi = new HtmlGenericControl( "li" );
                                gUl.Controls.Add( gLi );
                                gLi.InnerHtml = string.Format( "<strong>{0}</strong>: {1}", groupAttendance.GroupName, groupAttendance.CurrentCount );

                                var sUl = new HtmlGenericControl( "ul" );
                                sUl.AddCssClass( "kioskmanager-count-schedules" );
                                gLi.Controls.Add( sUl );

                                foreach ( var scheduleAttendance in groupAttendance.Schedules.Where( s => s.IsActive ) )
                                {
                                    var sLi = new HtmlGenericControl( "li" );
                                    sUl.Controls.Add( sLi );
                                    sLi.InnerHtml = string.Format( "<strong>{0}</strong>: {1}", scheduleAttendance.ScheduleName, scheduleAttendance.CurrentCount );
                                }
                            }
                        }
                    }
                }
            }

            pnlManagerLogin.Visible = true;

            // set manager timer to 10 minutes
            hfRefreshTimerSeconds.Value = "600";
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            RefreshView();
            ManagerLoggedIn = false;
            pnlManager.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnScheduleLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduleLocations_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ScheduledLocationsPage" );
        }

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLogin_Click( object sender, EventArgs e )
        {
            ManagerLoggedIn = false;
            var pinAuth = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            var rockContext = new Rock.Data.RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( tbPIN.Text );
            if ( userLogin != null && userLogin.EntityTypeId.HasValue )
            {
                // make sure this is a PIN auth user login
                var userLoginEntityType = EntityTypeCache.Read( userLogin.EntityTypeId.Value );
                if ( userLoginEntityType != null && userLoginEntityType.Id == pinAuth.EntityType.Id )
                {
                    if ( pinAuth != null && pinAuth.IsActive )
                    {
                        // should always return true, but just in case
                        if ( pinAuth.Authenticate( userLogin, null ) )
                        {
                            if ( !( userLogin.IsConfirmed ?? true ) )
                            {
                                maWarning.Show( "Sorry, account needs to be confirmed.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else if ( ( userLogin.IsLockedOut ?? false ) )
                            {
                                maWarning.Show( "Sorry, account is locked-out.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                            else
                            {
                                ManagerLoggedIn = true;
                                ShowManagementDetails();
                                return;
                            }
                        }
                    }
                }
            }

            maWarning.Show( "Sorry, we couldn't find an account matching that PIN.", Rock.Web.UI.Controls.ModalAlertType.Warning );
        }

        /// <summary>
        /// Shows the management details.
        /// </summary>
        private void ShowManagementDetails()
        {
            pnlManagerLogin.Visible = false;
            pnlManager.Visible = true;
            btnManager.Visible = false;
            BindManagerLocationsGrid();
        }

        /// <summary>
        /// Binds the manager locations grid.
        /// </summary>
        private void BindManagerLocationsGrid()
        {
            var rockContext = new RockContext();
            if ( this.CurrentKioskId.HasValue )
            {
                var groupTypesLocations = this.GetGroupTypesLocations( rockContext );
                var selectQry = groupTypesLocations
                    .Select( a => new
                    {
                        LocationId = a.Id,
                        Name = a.Name,
                        a.IsActive
                    } )
                    .OrderBy( a => a.Name );

                rLocations.DataSource = selectQry.ToList();
                rLocations.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rLocations control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rLocations_ItemCommand( object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e )
        {
            int? locationId = ( e.CommandArgument as string ).AsIntegerOrNull();

            if ( locationId.HasValue )
            {
                var rockContext = new RockContext();
                var location = new LocationService( rockContext ).Get( locationId.Value );
                if ( location != null )
                {
                    if ( e.CommandName == "Open" && !location.IsActive )
                    {
                        location.IsActive = true;
                        rockContext.SaveChanges();
                        KioskDevice.FlushAll();
                    }
                    else if ( e.CommandName == "Close" && location.IsActive )
                    {
                        location.IsActive = false;
                        rockContext.SaveChanges();
                        KioskDevice.FlushAll();
                    }
                }

                BindManagerLocationsGrid();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rLocations_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            object locationDataItem = e.Item.DataItem;
            if ( locationDataItem != null )
            {
                var lbOpen = e.Item.FindControl( "lbOpen" ) as LinkButton;
                var lbClose = e.Item.FindControl( "lbClose" ) as LinkButton;
                var isActive = (bool)locationDataItem.GetPropertyValue( "IsActive" );

                if ( isActive )
                {
                    lbClose.RemoveCssClass( "btn-danger" );
                    lbClose.RemoveCssClass( "active" );
                    lbOpen.AddCssClass( "btn-success" );
                    lbOpen.AddCssClass( "active" );
                }
                else
                {
                    lbOpen.RemoveCssClass( "btn-success" );
                    lbOpen.RemoveCssClass( "active" );
                    lbClose.AddCssClass( "btn-danger" );
                    lbClose.AddCssClass( "active" );
                }

                var lLocationName = e.Item.FindControl( "lLocationName" ) as Literal;
                lLocationName.Text = locationDataItem.GetPropertyValue( "Name" ) as string;

                var lLocationCount = e.Item.FindControl( "lLocationCount" ) as Literal;
                lLocationCount.Text = KioskLocationAttendance.Read( (int)locationDataItem.GetPropertyValue( "LocationId" ) ).CurrentCount.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            btnBack_Click( sender, e );
        }

    }
}