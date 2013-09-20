//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Check-in Success block" )]
    [LinkedPage("Person Select Page")]
    public partial class Success : CheckInBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RockPage.AddScriptLink(this.Page, "http://www.sparkdevnetwork.org/public/js/cordova-2.4.0.js");
            RockPage.AddScriptLink(this.Page, "http://www.sparkdevnetwork.org/public/js/ZebraPrint.js");
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
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    lbAnother.Visible = family.People.Count > 1;

                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            foreach ( var group in groupType.Groups.Where( g => g.Selected ) )
                            {
                                foreach ( var location in group.Locations.Where( l => l.Selected ) )
                                {
                                    foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                                    {
                                        var li = new HtmlGenericControl( "li" );
                                        li.InnerText = string.Format( "{0} was checked into {1} for {2} at {3}",
                                            person.ToString(), group.ToString(), location.ToString(), schedule.ToString(), person.SecurityCode );

                                        phResults.Controls.Add( li );
                                    }
                                }
                            }

                            var printFromClient = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client);
                            if ( printFromClient.Any() )
                            {
                                AddLabelScript( printFromClient.ToJson() );
                            }

                            var printFromServer = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server );
                            if ( printFromServer.Any() )
                            {
                                Socket socket = null;
                                string currentIp = string.Empty;

                                foreach ( var label in printFromServer )
                                {
                                    var labelCache = KioskLabel.Read( label.FileId );
                                    if ( labelCache != null )
                                    {
                                        if ( label.PrinterAddress != currentIp )
                                        {
                                            if ( socket != null && socket.Connected )
                                            {
                                                socket.Shutdown( SocketShutdown.Both );
                                                socket.Close();
                                            }

                                            currentIp = label.PrinterAddress;
                                            var printerIp = new IPEndPoint( IPAddress.Parse( currentIp ), 9100 );

                                            socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                                            IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                                            bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
                                        }

                                        string printContent = labelCache.FileContent;
                                        foreach ( var mergeField in label.MergeFields )
                                        {
                                            var rgx = new Regex( string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ) );
                                            printContent = rgx.Replace( printContent, mergeField.Value );
                                        }

                                        if ( socket.Connected )
                                        {
                                            var ns = new NetworkStream( socket );
                                            byte[] toSend = System.Text.Encoding.ASCII.GetBytes( printContent );
                                            ns.Write( toSend, 0, toSend.Length );
                                        }
                                        else
                                        {
                                            phResults.Controls.Add( new LiteralControl( "<br/>NOTE: Could not connect to printer!" ) );
                                        }
                                    }
                                }

                                if ( socket != null && socket.Connected )
                                {
                                    socket.Shutdown( SocketShutdown.Both );
                                    socket.Close();
                                }
                            }
                        }
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
                            groupType.Groups = new List<CheckInGroup>();
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
	    document.addEventListener('deviceready', onDeviceReady, false);

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
	
			//navigator.notification.alert('Oh boy! It's going to be a good day!, alertDismissed, 'Success', 'Continue');
			printLabels();
		}}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('I printed that tag like a champ!!!');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

    }
}