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
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Success" )]
    [Category( "Check-in" )]
    [Description( "Displays the details of a successful checkin." )]

    [LinkedPage( "Person Select Page", "", false, "", "", 5 )]
    [TextField( "Title", "", false, "Checked-in", "Text", 6 )]
    [TextField( "Detail Message", "The message to display indicating person has been checked in. Use {0} for person, {1} for group, {2} for schedule, and {3} for the security code", false,
        "{0} was checked into {1} in {2} at {3}", "Text", 7 )]
    public partial class Success : CheckInBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-success-bg" );
            }
        }

        /// <summary>
        /// CheckinResult for rendering the Success Lava Template
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class CheckinResult : DotLiquid.Drop
        {
            /// <summary>
            /// Gets the person.
            /// </summary>
            /// <value>
            /// The person.
            /// </value>
            public CheckInPerson Person { get; internal set; }

            /// <summary>
            /// Gets the group.
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            public CheckInGroup Group { get; internal set; }

            /// <summary>
            /// Gets the location.
            /// </summary>
            /// <value>
            /// The location.
            /// </value>
            public Location Location { get; internal set; }

            /// <summary>
            /// Gets the schedule.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public CheckInSchedule Schedule { get; internal set; }

            /// <summary>
            /// Gets the detail message.
            /// </summary>
            /// <value>
            /// The detail message.
            /// </value>
            public string DetailMessage { get; internal set; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    try
                    {
                        lTitle.Text = GetAttributeValue( "Title" );
                        string detailMsg = GetAttributeValue( "DetailMessage" );

                        var printFromClient = new List<CheckInLabel>();
                        var printFromServer = new List<CheckInLabel>();

                        List<CheckinResult> checkinResultList = new List<CheckinResult>();

                        // Print the labels
                        foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                        {
                            lbAnother.Visible =
                                CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Individual &&
                                family.People.Count > 1;

                            foreach ( var person in family.GetPeople( true ) )
                            {
                                foreach ( var groupType in person.GetGroupTypes( true ) )
                                {
                                    foreach ( var group in groupType.GetGroups( true ) )
                                    {
                                        foreach ( var location in group.GetLocations( true ) )
                                        {
                                            foreach ( var schedule in location.GetSchedules( true ) )
                                            {
                                                string detailMessage = string.Format( detailMsg, person.ToString(), group.ToString(), location.Location.Name, schedule.ToString(), person.SecurityCode );
                                                CheckinResult checkinResult = new CheckinResult();
                                                checkinResult.Person = person;
                                                checkinResult.Group = group;
                                                checkinResult.Location = location.Location;
                                                checkinResult.Schedule = schedule;
                                                checkinResult.DetailMessage = detailMessage;
                                                checkinResultList.Add( checkinResult );
                                            }
                                        }
                                    }

                                    if ( groupType.Labels != null && groupType.Labels.Any() )
                                    {
                                        printFromClient.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                                        printFromServer.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                                    }
                                }
                            }
                        }

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                        mergeFields.Add( "CheckinResultList", checkinResultList );
                        mergeFields.Add( "Kiosk", CurrentCheckInState.Kiosk );
                        mergeFields.Add( "RegistrationModeEnabled", CurrentCheckInState.Kiosk.RegistrationModeEnabled );
                        mergeFields.Add( "Messages", CurrentCheckInState.Messages );
                        if ( LocalDeviceConfig.CurrentGroupTypeIds != null )
                        {
                            var checkInAreas = LocalDeviceConfig.CurrentGroupTypeIds.Select( a => Rock.Web.Cache.GroupTypeCache.Get( a ) );
                            mergeFields.Add( "CheckinAreas", checkInAreas );
                        }

                        if ( printFromClient.Any() )
                        {
                            var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
#if DEBUG
                            // This is extremely useful when debugging with ngrok and an iPad on the local network.
                            // X-Original-Host will contain the name of your ngrok hostname, therefore the labels will
                            // get a LabelFile url that will actually work with that iPad.
                            if ( Request.Headers["X-Forwarded-Proto"] != null && Request.Headers["X-Original-Host" ] != null )
                            {
                                urlRoot = string.Format( "{0}://{1}", Request.Headers.GetValues( "X-Forwarded-Proto" ).First(), Request.Headers.GetValues( "X-Original-Host" ).First() );
                            }
#endif
                            printFromClient
                                .OrderBy( l => l.PersonId )
                                .ThenBy( l => l.Order )
                                .ToList()
                                .ForEach( l => l.LabelFile = urlRoot + l.LabelFile );

                            AddLabelScript( printFromClient.ToJson() );
                        }

                        if ( printFromServer.Any() )
                        {
                            var messages = ZebraPrint.PrintLabels( printFromServer );
                            mergeFields.Add( "ZebraPrintMessageList", messages );
                        }

                        var successLavaTemplate = CurrentCheckInState.CheckInType.SuccessLavaTemplate;
                        lCheckinResultsHtml.Text = successLavaTemplate.ResolveMergeFields( mergeFields );

                        if ( LocalDeviceConfig.GenerateQRCodeForAttendanceSessions )
                        {
                            HttpCookie attendanceSessionGuidsCookie = Request.Cookies[CheckInCookieKey.AttendanceSessionGuids];
                            if ( attendanceSessionGuidsCookie == null )
                            {
                                attendanceSessionGuidsCookie = new HttpCookie( CheckInCookieKey.AttendanceSessionGuids );
                                attendanceSessionGuidsCookie.Value = string.Empty;
                            }

                            // set (or reset) the expiration to be 8 hours from the current time)
                            attendanceSessionGuidsCookie.Expires = RockDateTime.Now.AddHours( 8 );

                            var attendanceSessionGuids = attendanceSessionGuidsCookie.Value.Split( ',' ).AsGuidList();
                            if ( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.HasValue )
                            {
                                attendanceSessionGuids.Add( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.Value );
                            }

                            attendanceSessionGuidsCookie.Value = attendanceSessionGuids.AsDelimited( "," );

                            Response.Cookies.Set( attendanceSessionGuidsCookie );

                            lCheckinQRCodeHtml.Text = string.Format( "<div class='qr-code-container text-center'><img class='img-responsive qr-code' src='{0}' alt='Check-in QR Code' width='500' height='500'></div>", GetAttendanceSessionsQrCodeImageUrl( attendanceSessionGuidsCookie ) );
                        }

                    }
                    catch ( Exception ex )
                    {
                        LogException( ex );
                    }
                }
            }
        }



        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            NavigateToHomePage();
        }

        /// <summary>
        /// Handles the Click event of the lbAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAnother_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        person.Selected = false;

                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            groupType.Selected = false;
                        }
                    }
                }

                SaveState();
                NavigateToLinkedPage( "PersonSelectPage" );

            }
            else
            {
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/) && typeof window.RockCheckinNative === 'undefined') {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{
                printLabels();
            }}
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}

		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData),
            	function(result) {{
			        console.log('Tag printed');
			    }},
			    function(error) {{
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    alert('An error occurred while printing the labels. ' + error[0]);
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

    }
}