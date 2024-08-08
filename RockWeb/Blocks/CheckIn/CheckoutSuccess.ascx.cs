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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI;

using CheckInLabel = Rock.CheckIn.CheckInLabel;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Check Out Success" )]
    [Category( "Check-in" )]
    [Description( "Displays the details of a successful check out." )]

    [TextField( "Title", "Title to display.", false, "Checked Out", "Text", 5 )]
    [TextField( "Detail Message", "The message to display indicating person has been checked out. Use {0} for person, {1} for group, {2} for location, and {3} for schedule.", false,
        "{0} was checked out of {1} in {2} at {3}.", "Text", 6 )]

    [Rock.SystemGuid.BlockTypeGuid( "F499C4A9-9A60-404B-9383-B950EE6D7821" )]
    public partial class CheckoutSuccess : CheckInBlock
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
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

                        var printFromClient = new List<CheckInLabel>();
                        var printFromServer = new List<CheckInLabel>();

                        using ( var rockContext = new RockContext() )
                        {
                            var attendanceService = new AttendanceService( rockContext );

                            // Print the labels
                            foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                            {
                                foreach ( var person in family.CheckOutPeople.Where( p => p.Selected ) )
                                {
                                    foreach ( var attendance in attendanceService.Queryable()
                                        .Where( a => person.AttendanceIds.Contains( a.Id ) )
                                        .ToList() )
                                    {
                                        var now = attendance.Campus != null ? attendance.Campus.CurrentDateTime : RockDateTime.Now;

                                        attendance.EndDateTime = now;
                                        attendance.CheckedOutByPersonAliasId = GetCheckoutPersonAliasId();

                                        if ( attendance.Occurrence.Group != null &&
                                            attendance.Occurrence.Location != null &&
                                            attendance.Occurrence.Schedule != null )
                                        {
                                            var li = new HtmlGenericControl( "li" );
                                            li.InnerText = string.Format( GetAttributeValue( "DetailMessage" ),
                                                person.ToString(), attendance.Occurrence.Group.ToString(), attendance.Occurrence.Location.Name, attendance.Occurrence.Schedule.Name );

                                            phResults.Controls.Add( li );
                                        }

                                    }

                                    if ( person.Labels != null && person.Labels.Any() )
                                    {
                                        printFromClient.AddRange( person.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                                        printFromServer.AddRange( person.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                                    }
                                }
                            }

                            rockContext.SaveChanges();
                        }

                        if ( printFromClient.Any() )
                        {
                            var safeProxySafeUrl = Request.UrlProxySafe();
                            var urlRoot = $"{safeProxySafeUrl.Scheme}://{safeProxySafeUrl.Authority}";

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

                            foreach ( var message in messages )
                            {
                                phResults.Controls.Add( new LiteralControl( string.Format( "<br/>{0}", message ) ) );
                            }
                        }

                    }
                    catch ( Exception ex )
                    {
                        LogException( ex );
                    }
                }
            }

            base.OnLoad( e );
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

        private int? GetCheckoutPersonAliasId()
        {
            if ( CurrentCheckInState.CheckIn.CheckedInByPersonAliasId.HasValue )
            {
                return CurrentCheckInState.CheckIn.CheckedInByPersonAliasId;
            }

            int? personAliasId = null;
            if ( Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER] != null )
            {
                var personAliasGuid = Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER].Value.AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    var personAlias = new PersonAliasService( new RockContext() ).GetByAliasGuid( personAliasGuid.Value );
                    if ( personAlias != null )
                    {
                        personAliasId = personAlias.Id;
                    }
                }
            }

            if ( !personAliasId.HasValue )
            {
                personAliasId = CurrentPersonAliasId;
            }

            return personAliasId;
        }

    }
}