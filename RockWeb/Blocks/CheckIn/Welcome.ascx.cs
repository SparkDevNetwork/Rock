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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Welcome" )]
    [Category( "Check-in" )]
    [Description( "Welcome screen for check-in." )]

    [LinkedPage(
        "Family Select Page",
        Key = AttributeKey.FamilySelectPage,
        IsRequired = false,
        Order = 5 )]

    [LinkedPage(
        "Scheduled Locations Page",
        Key = AttributeKey.ScheduledLocationsPage,
        IsRequired = false,
        Order = 6 )]

    [TextField(
        "Not Active Title",
        Key = AttributeKey.NotActiveTitle,
        Description = "Title displayed when there are not any active options today.",
        IsRequired = false,
        DefaultValue = "Check-in Is Not Active",
        Category = "Text",
        Order = 7 )]

    [TextField(
        "Not Active Caption",
        Key = AttributeKey.NotActiveCaption,
        Description = "Caption displayed when there are not any active options today.",
        IsRequired = false,
        DefaultValue = "There are no current or future schedules for this kiosk today!",
        Category = "Text",
        Order = 8 )]

    [TextField(
        "Not Active Yet Title",
        Key = AttributeKey.NotActiveYetTitle,
        Description = "Title displayed when there are active options today, but none are active now.",
        IsRequired = false,
        DefaultValue = "Check-in Is Not Active Yet",
        Category = "Text",
        Order = 9 )]

    [TextField(
        "Not Active Yet Caption",
        Key = AttributeKey.NotActiveYetCaption,
        Description = "Caption displayed when there are active options today, but none are active now. Use {0} for a countdown timer.",
        IsRequired = false,
        DefaultValue = "This kiosk is not active yet.  Countdown until active: {0}.",
        Category = "Text",
        Order = 10 )]

    [TextField(
        "Closed Title",
        Key = AttributeKey.ClosedTitle,
        Description = "",
        IsRequired = false,
        DefaultValue = "Closed",
        Category = "Text",
        Order = 11 )]

    [TextField(
        "Closed Caption",
        Key = AttributeKey.ClosedCaption,
        IsRequired = false,
        DefaultValue = "This location is currently closed.",
        Category = "Text",
        Order = 12 )]

    [TextField(
        "Check-in Button Text",
        Key = AttributeKey.CheckinButtonText,
        Description = "The text to display on the check-in button. Defaults to 'Start' if left blank.",
        IsRequired = false,
        DefaultValue = "",
        Category = "Text",
        Order = 13 )]

    [TextField(
        "No Option Caption",
        Key = AttributeKey.NoOptionCaption,
        Description = "The text to display when there are not any families found matching a scanned identifier (barcode, etc).",
        IsRequired = false,
        DefaultValue = "Sorry, there were not any families found with the selected identifier.",
        Category = "Text",
        Order = 14 )]

    [BooleanField(
        "Allow Opening and Closing Rooms",
        Key = AttributeKey.AllowOpeningAndClosingRooms,
        Description = "Determines if opening and closing rooms should be allowed. If not allowed, the locations only show counts and the open/close toggles are not shown.",
        DefaultBooleanValue = true,
        Category = "Manager Settings",
        Order = 20 )]

    [BooleanField(
        "Allow Label Reprinting",
        Key = AttributeKey.AllowLabelReprinting,
        Description = " Determines if reprinting labels should be allowed.",
        DefaultBooleanValue = false,
        Category = "Manager Settings",
        Order = 21 )]

    public partial class Welcome : CheckInBlock
    {
        #region Attribute Keys

        protected static class AttributeKey
        {
            public const string FamilySelectPage = "FamilySelectPage";
            public const string ScheduledLocationsPage = "ScheduledLocationsPage";
            public const string NotActiveTitle = "NotActiveTitle";
            public const string NotActiveCaption = "NotActiveCaption";
            public const string NotActiveYetTitle = "NotActiveYetTitle";
            public const string NotActiveYetCaption = "NotActiveYetCaption";
            public const string ClosedTitle = "ClosedTitle";
            public const string ClosedCaption = "ClosedCaption";
            public const string CheckinButtonText = "CheckinButtonText";
            public const string NoOptionCaption = "NoOptionCaption";
            public const string AllowOpeningAndClosingRooms = "AllowOpeningAndClosingRooms";
            public const string AllowLabelReprinting = "AllowLabelReprinting";
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            // For Label Reprints
            string script = string.Format( @"
        function GetLabelTypeSelection() {{
            var ids = '';
            $('div.js-label-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('data-label-guid') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one tag');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}

        $('a.js-label-select').click( function() {{
            $(this).toggleClass('active');
            $(this).find('i').toggleClass('fa-check-square').toggleClass('fa-square-o');
            var ids = '';
            $('div.js-label-list').find('i.fa-check-square').each( function() {{
                ids += $(this).closest('a').attr('data-label-guid') + ',';
            }});
            $('#{1}').val(ids);
        }});

", lbReprintSelectLabelTypes.ClientID, hfLabelFileGuids.ClientID );
            ScriptManager.RegisterStartupScript( pnlReprintSelectedPersonLabels, pnlReprintSelectedPersonLabels.GetType(), "SelectLabelTypes", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                    CurrentCheckInState.Messages = new List<CheckInMessage>();
                    SaveState();

                    string familyId = PageParameter( "FamilyId" );
                    if ( familyId.IsNotNullOrWhiteSpace() )
                    {
                        var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );
                        DoFamilySearch( dv, familyId );
                    }
                    else
                    {
                        RefreshView();

                        lNotActiveTitle.Text = GetAttributeValue( AttributeKey.NotActiveTitle );
                        lNotActiveCaption.Text = GetAttributeValue( AttributeKey.NotActiveCaption );
                        lNotActiveYetTitle.Text = GetAttributeValue( AttributeKey.NotActiveYetTitle );
                        lNotActiveYetCaption.Text = string.Format( GetAttributeValue( AttributeKey.NotActiveYetCaption ), "<span class='countdown-timer'></span>" );
                        lClosedTitle.Text = GetAttributeValue( AttributeKey.ClosedTitle );
                        lClosedCaption.Text = GetAttributeValue( AttributeKey.ClosedCaption );

                        string checkinButtonText = GetAttributeValue( AttributeKey.CheckinButtonText ).IfEmpty( "Start" );

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                        mergeFields.Add( AttributeKey.CheckinButtonText, checkinButtonText );
                        mergeFields.Add( "Kiosk", CurrentCheckInState.Kiosk );
                        mergeFields.Add( "RegistrationModeEnabled", CurrentCheckInState.Kiosk.RegistrationModeEnabled );

                        if ( CurrentGroupTypeIds != null )
                        {
                            var checkInAreas = CurrentGroupTypeIds.Select( a => GroupTypeCache.Get( a ) );
                            mergeFields.Add( "CheckinAreas", checkInAreas );
                        }

                        lStartButtonHtml.Text = CurrentCheckInState.CheckInType.StartLavaTemplate.ResolveMergeFields( mergeFields );
                    }
                }
            }
            else
            {
                if ( Request.Form["__EVENTARGUMENT"] != null )
                {
                    if ( Request.Form["__EVENTARGUMENT"] == "Wedge_Entry" )
                    {
                        var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID );
                        DoFamilySearch( dv, hfSearchEntry.Value );
                    }
                    else if ( Request.Form["__EVENTARGUMENT"] == "Family_Id_Search" )
                    {
                        var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );
                        DoFamilySearch( dv, hfSearchEntry.Value );
                    }
                    else if ( Request.Form["__EVENTARGUMENT"] == "StartClick" )
                    {
                        HandleStartClick();
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
        /// Handles the start click.
        /// </summary>
        protected void HandleStartClick()
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
            window.location = ""javascript:{0}"";
        }}

", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
            ScriptManager.RegisterStartupScript( lbRefresh, lbRefresh.GetType(), "refresh-postback", script.ToString(), true );
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        private void ClearSelection()
        {
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();
            CurrentCheckInState.Messages = new List<CheckInMessage>();
        }

        /// <summary>
        /// Performs a search for the family.
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
                    maWarning.Show( string.Format( "<p>{0}</p>", GetAttributeValue( AttributeKey.NoOptionCaption ) ), Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
                else
                {
                    SaveState();
                    NavigateToLinkedPage( AttributeKey.FamilySelectPage );
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
            HideReprintPanels();
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
                if ( activeAt == DateTime.MaxValue )
                {
                    pnlClosed.Visible = true;
                }
                else
                {
                    lblActiveWhen.Text = activeAt.ToString( "o" ).Left( 27 );   // strip the timezone offset off of the string, so that countdown is displayed relative to kiosk's local time.
                    pnlNotActiveYet.Visible = true;
                }
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
            btnManager.Visible = false;

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
                        var locationAttendance = KioskLocationAttendance.Get( location.Location.Id );

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

        #region Device Manager

        #region Device Manager Events

        #region Device Manager Reprint Label Events

        protected void HideReprintPanels()
        {
            // Hide all the manager reprint operations
            pnlReprintLabels.Visible = false;
            pnlReprintSearchPersonResults.Visible = false;
            pnlReprintSelectedPersonLabels.Visible = false;
            pnlReprintResults.Visible = false;
        }

        protected void lbManagerCancel_Click( object sender, EventArgs e )
        {
            HideReprintPanels();
            // Show the manager panel since we're still in that mode.
            pnlManager.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnReprintLabels control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnReprintLabels_Click( object sender, EventArgs e )
        {
            pnlReprintLabels.Visible = true;
            pnlManager.Visible = false;
            tbNameOrPhone.Focus();
            tbNameOrPhone.Text = string.Empty;
        }

        /// <summary>
        /// Manager Reprint screen to search by name or phone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbManagerReprintSearch_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( tbNameOrPhone.Text ) )
            {
                tbNameOrPhone.Focus();
                maWarning.Show( "Please enter a phone number or name.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                return;
            }

            pnlReprintSearchPersonResults.Visible = true;

            var people = FindPossibleMatchingCheckedInPeople();

            if ( people == null || people.Count == 0 )
            {
                maWarning.Show( "There is no one currently checked-in that matches the search criteria.", Rock.Web.UI.Controls.ModalAlertType.Warning );
                pnlReprintLabels.Visible = true;
                pnlReprintSearchPersonResults.Visible = false;
                tbNameOrPhone.Text = string.Empty;
                tbNameOrPhone.Focus();
            }
            else
            {
                rReprintLabelPersonResults.DataSource = people;
                rReprintLabelPersonResults.DataBind();
                pnlReprintLabels.Visible = false;
                pnlReprintSearchPersonResults.Visible = true;
            }
        }

        private List<ReprintLabelPersonResult> FindPossibleMatchingCheckedInPeople()
        {
            var people = new List<ReprintLabelPersonResult>();

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                bool reversed = false;

                List<int> matchingPeopleIds = null;

                // Find all matching people
                var isNumericMatch = Regex.Match( tbNameOrPhone.Text, @"\d+" );

                if ( isNumericMatch.Success )
                {
                    matchingPeopleIds = PhoneSearch( tbNameOrPhone.Text ).ToList();
                }
                else
                {
                    matchingPeopleIds = personService
                        .GetByFullName( tbNameOrPhone.Text, false, false, false, out reversed )
                        .Select( p => p.Id ).ToList();
                }

                // Find all people currently checked in.
                var dayStart = RockDateTime.Today.AddDays( -1 );
                var attendees = new AttendanceService( rockContext )
                    .Queryable( "Occurrence.Group,PersonAlias.Person,Occurrence.Schedule,AttendanceCode" )
                    .AsNoTracking()
                    .Where( a =>
                        a.StartDateTime > dayStart &&
                        !a.EndDateTime.HasValue &&
                        a.Occurrence.LocationId.HasValue &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.Occurrence.ScheduleId.HasValue )
                    .Where( a => matchingPeopleIds.Contains( a.PersonAlias.PersonId ) )
                    .ToList()
                    .Where( a => a.IsCurrentlyCheckedIn )
                    .ToList();

                foreach ( var personId in attendees
                    .OrderBy( a => a.PersonAlias.Person.NickName )
                    .ThenBy( a => a.PersonAlias.Person.LastName )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct() )
                {
                    var matchingAttendeesAttendanceRecords = attendees
                        .Where( a => a.PersonAlias.PersonId == personId )
                        .ToList();

                    people.Add( new ReprintLabelPersonResult( matchingAttendeesAttendanceRecords ) );
                }
            }

            return people;
        }

        /// <summary>
        /// Handles when a person is selected from the re-print label button.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void rReprintLabelPersonResults_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            // Get the person Id
            int personId = Int32.Parse( e.CommandArgument.ToString() );
            hfSelectedPersonId.Value = personId.ToStringSafe();

            // Get the attendanceIds
            var hfAttendanceIds = e.Item.FindControl( "hfAttendanceIds" ) as HiddenField;
            if ( hfAttendanceIds == null )
            {
                return;
            }

            // save the attendance ids for later use.
            hfSelectedAttendanceIds.Value = hfAttendanceIds.Value;

            // hide the reprint person select results, then show the selected labels to pick from
            pnlReprintLabels.Visible = false;
            pnlManager.Visible = false;
            pnlReprintSearchPersonResults.Visible = false;
            pnlReprintSelectedPersonLabels.Visible = true;

            // bind all possible tags to reprint
            var possibleLabels = ZebraPrint.GetLabelTypesForPerson( personId, hfSelectedAttendanceIds.Value.SplitDelimitedValues().AsIntegerList() );
            if ( possibleLabels.Count != 0 )
            {
                lbReprintSelectLabelTypes.Visible = true;
            }
            else
            {
                lbReprintSelectLabelTypes.Visible = false;
                maNoLabelsFound.Show( "No labels were found for that selection.", ModalAlertType.Alert );
            }

            rReprintLabelTypeSelection.DataSource = possibleLabels;
            rReprintLabelTypeSelection.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rReprintLabelTypeSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var pnlLabel = e.Item.FindControl( "pnlLabel" ) as Panel;

                if ( pnlLabel != null )
                {
                    pnlLabel.CssClass = "col-md-10 col-sm-10 col-xs-8";

                    var lLabelButton = e.Item.FindControl( "lLabelButton" ) as Literal;
                    var labelType = e.Item.DataItem as ReprintLabelCheckInLabelType;

                    if ( lLabelButton != null && labelType != null )
                    {
                        lLabelButton.Text = string.Format( "{0}", labelType.Name );
                    }
                }
            }
        }

        /// <summary>
        /// Reprint the selected labels
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbReprintSelectLabelTypes_Click( object sender, EventArgs e )
        {
            var fileGuids = hfLabelFileGuids.Value.SplitDelimitedValues().AsGuidList();
            var personId = hfSelectedPersonId.ValueAsInt();
            var selectedAttendanceIds = hfSelectedAttendanceIds.Value.SplitDelimitedValues().AsIntegerList();

            List<string> messages = ZebraPrint.ReprintZebraLabels( fileGuids, personId, selectedAttendanceIds, this );

            pnlReprintResults.Visible = true;
            pnlReprintSelectedPersonLabels.Visible = false;

            lReprintResultsHtml.Text = messages.JoinStrings( "<br>" );
        }

        /// <summary>
        /// When label re-printing is done, this button would be pressed
        /// so the person is returned back to the device manager screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbManagerReprintDone_Click( object sender, EventArgs e )
        {
            // Hide our panels and show the Manager panel.
            HideReprintPanels();
            pnlManager.Visible = true;
        }

        /// <summary>
        /// Gets the icon CSS class for the checkbox for either state.
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected string GetCheckboxClass( bool selected )
        {
            return selected ? "fa fa-check-square" : "fa fa-square-o";
        }

        /// <summary>
        /// Gets the icon CSS class that represents the 'selected' or non-selected state.
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected string GetSelectedClass( bool selected )
        {
            return selected ? "active" : "";
        }

        /// <summary>
        /// Returns a list of people who belong to a family that matches the given number
        /// </summary>
        /// <param name="numericPhone"></param>
        /// <returns></returns>
        private IQueryable<int> PhoneSearch( string numericPhone )
        {
            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );
            var memberService = new GroupMemberService( rockContext );
            var groupService = new GroupService( rockContext );

            int personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;
            var dvInactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

            IQueryable<int> personIds = null;

            var phoneQry = new PhoneNumberService( rockContext ).Queryable().AsNoTracking();

            phoneQry = phoneQry.Where( o => o.Number.Contains( numericPhone ) );

            // Similar query used by the FindFamilies check-in workflow action
            var tmpQry = phoneQry
                .Join( personService.Queryable().AsNoTracking(),
                    o => new { PersonId = o.PersonId, IsDeceased = false, RecordTypeValueId = personRecordTypeId },
                    p => new { PersonId = p.Id, IsDeceased = p.IsDeceased, RecordTypeValueId = p.RecordTypeValueId.Value },
                    ( pn, p ) => new { Person = p, PhoneNumber = pn } )
                .Join( memberService.Queryable().AsNoTracking(),
                pn => pn.Person.Id,
                m => m.PersonId,
                ( o, m ) => new { PersonNumber = o.PhoneNumber, GroupMember = m } );

            personIds = groupService.Queryable()
                .Where( g => tmpQry.Any( o => o.GroupMember.GroupId == g.Id ) && g.GroupTypeId == familyGroupTypeId )
                .SelectMany( g => g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ) )
                .Select( p => p.PersonId )
                .Distinct();

            return personIds;
        }

        #endregion

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
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            RefreshView();
            ManagerLoggedIn = false;
            pnlManager.Visible = false;
            HideReprintPanels();
        }

        /// <summary>
        /// Handles the Click event of the btnScheduleLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduleLocations_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ScheduledLocationsPage );
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
                var userLoginEntityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
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
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            btnBack_Click( sender, e );
        }

        #endregion

        /// <summary>
        /// Shows the management screen details.
        /// </summary>
        private void ShowManagementDetails()
        {
            // Only show Schedule Locations if setting is not empty
            btnScheduleLocations.Visible = GetAttributeValue( AttributeKey.ScheduledLocationsPage ).IsNotNullOrWhiteSpace();

            btnReprintLabels.Visible = GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean();
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
            // Do this only once for efficiency sake vs in the repeater's ItemDataBound
            hfAllowOpenClose.Value = GetAttributeValue( AttributeKey.AllowOpeningAndClosingRooms );

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
                        KioskDevice.Clear();
                    }
                    else if ( e.CommandName == "Close" && location.IsActive )
                    {
                        location.IsActive = false;
                        rockContext.SaveChanges();
                        KioskDevice.Clear();
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
                if ( hfAllowOpenClose.Value.AsBoolean() )
                {
                    var lbOpen = e.Item.FindControl( "lbOpen" ) as LinkButton;
                    var lbClose = e.Item.FindControl( "lbClose" ) as LinkButton;
                    var isActive = ( bool ) locationDataItem.GetPropertyValue( "IsActive" );

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
                }
                else
                {
                    var divLocationToggle = e.Item.FindControl( "divLocationToggle" ) as HtmlGenericControl;
                    divLocationToggle.Visible = false;
                }

                var lLocationName = e.Item.FindControl( "lLocationName" ) as Literal;
                lLocationName.Text = locationDataItem.GetPropertyValue( "Name" ) as string;

                var lLocationCount = e.Item.FindControl( "lLocationCount" ) as Literal;
                lLocationCount.Text = KioskLocationAttendance.Get( ( int ) locationDataItem.GetPropertyValue( "LocationId" ) ).CurrentCount.ToString();
            }
        }

        #endregion
    }
}