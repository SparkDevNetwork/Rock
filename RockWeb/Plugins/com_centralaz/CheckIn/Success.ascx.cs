// <copyright>
// Copyright by Central Christian Church
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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Web.UI;

using com.centralaz.CheckInLabels;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Success" )]
    [Category( "com_centralaz > Check-in" )]
    [Description( "Displays the details of a successful checkin." )]
    [LinkedPage( "Person Select Page" )]
    public partial class Success : CheckInBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/CheckIn/Scripts/checkin-core.js" );
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
                                                var li = new HtmlGenericControl( "li" );
                                                li.InnerText = string.Format( "{0} : {2} at {3}",
                                                    person.ToString(), group.ToString(), location.ToString(), schedule.ToString(), person.SecurityCode );

                                                phResults.Controls.Add( li );
                                            }
                                        }
                                    }

                                    try
                                    {
                                        var printFromClient = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ).OrderBy( l => l.Order );
                                        if ( printFromClient.Any() )
                                        {
                                            var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );
                                            printFromClient.ToList().ForEach( l => l.LabelFile = urlRoot + l.LabelFile );
                                            AddLabelScript( printFromClient.ToJson() );
                                        }

                                        var printFromServer = groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ).OrderBy( l => l.Order );
                                        if ( printFromServer.Any() )
                                        {
                                            PrintFromServerLabels( person, groupType, printFromServer );
                                        }
                                    }
                                    catch ( Exception ex )
                                    {
                                        phResults.Controls.Add( new LiteralControl( string.Format( "<br/><span class='text-danger '>Could not connect to printer! {0}</span>", ex.Message ) ) );

                                        // Problem printing person's labels.
                                        LogException( ex );
                                    }
                                }
                            }
                        }


                        /*
                        <table class="table table-condensed">
                            <tr>
                                <th></th><th>4:30</th><th>6:30</th><th></th>
                            </tr>
                            <tr>
                                <td>Noah</td><td class="bg-success">B131</td><td class="bg-danger text-danger">error</td>
                            </tr>
                            <tr>
                                <td>Alex</td><td class="bg-success">B131</td><td>B129</td><td><!--no error--></td>
                            </tr>
                        </table>
                        */


                    }
                    catch ( Exception ex )
                    {
                        LogException( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Prints the labels that are the "from server" ones.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="printFromServer">The print from server.</param>
        private void PrintFromServerLabels( CheckInPerson person, CheckInGroupType groupType, IEnumerable<CheckInLabel> printFromServer )
        {
            Socket socket = null;
            bool hasCutter = true;
            string currentIp = string.Empty;
            int numOfLabels = printFromServer.Count();
            int labelIndex = 0;
            foreach ( var label in printFromServer.OrderBy( l => l.Order ) )
            {
                labelIndex++;
                var labelCache = KioskLabel.Read( label.FileGuid );
                if ( labelCache != null )
                {
                    if ( !string.IsNullOrWhiteSpace( label.PrinterAddress ) )
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
                            var deviceId = label.PrinterDeviceId;
                            hasCutter = GetPrinterCutterOption( deviceId );
 
                            socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                            IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                            bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
                        }

                        string printContent = labelCache.FileContent;
                        // This is documented in <\IT\Projects\Rock RMS\CustomProjects\Check-in\Rock Central Check-in Setup and Design.docx>
                        if ( printContent.StartsWith( "Assembly:" ) )
                        {
                            if ( socket != null && socket.Connected )
                            {
                                socket.Shutdown( SocketShutdown.Both );
                                socket.Close();
                            }
                            LoadPrintLabelAndPrint( printContent, label, CurrentCheckInState, person, groupType );
                        }
                        else
                        {
                            foreach ( var mergeField in label.MergeFields )
                            {
                                if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                                {
                                    printContent = Regex.Replace( printContent, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), ZebraFormatString( mergeField.Value ) );
                                }
                                else
                                {
                                    // Remove the box preceding merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                                    // Remove the merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                                }
                            }

                            // Inject the cut command on the last label (if the printer is has a cutter)
                            // otherwise supress the backfeed (^XB)
                            if ( labelIndex == numOfLabels && hasCutter )
                            {
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^PQ1,0,1,Y", string.Empty );
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^MMT", @"^MMC" );
                            }
                            else
                            {
                                printContent = Regex.Replace( printContent.Trim(), @"\" + @"^XZ$", @"^XB^XZ" );
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
                } // labelCache != null
            }

            if ( socket != null && socket.Connected )
            {
                socket.Shutdown( SocketShutdown.Both );
                socket.Close();
            }
        }

        /// <summary>
        /// Gets the printer cutter option from either a "HasCutter" (boolean) attribute
        /// on the printer device or the words "w/Cutter" in the printer's Description.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>true if printer has a cutter; false otherwise</returns>
        protected bool GetPrinterCutterOption( int? deviceId )
        {
            bool hasCutter = false;

            // Get the device from cache
            var currentGroupTypeIds = ( Session["CheckInGroupTypeIds"] != null ) ? Session["CheckInGroupTypeIds"] as List<int> : new List<int>();
            KioskDevice kioskDevice = KioskDevice.Read( deviceId.GetValueOrDefault(), currentGroupTypeIds );
            hasCutter = kioskDevice.Device.GetAttributeValue( "HasCutter" ).AsBoolean();

            // also check the Description for the w/Cutter keywords
            if ( ! hasCutter )
            {
                hasCutter = Regex.IsMatch( kioskDevice.Device.Description, "w/Cutter", RegexOptions.IgnoreCase );
            }

            return hasCutter;
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

        private string ZebraFormatString( string input, bool isJson = false )
        {
            if ( isJson )
            {
                return input.Replace( "é", @"\\82" );  // fix acute e
            }
            else
            {
                return input.Replace( "é", @"\82" );  // fix acute e
            }
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

        # region Helper Methods

        private void LoadPrintLabelAndPrint( string assemblyString, CheckInLabel label, CheckInState checkInState, CheckInPerson person, CheckInGroupType groupType )
        {
            // Use only the first line
            string line1 = assemblyString.Split( new[] { '\r', '\n' } ).FirstOrDefault();
            // Remove the "Assembly:" prefix
            var assemblyParts = line1.ReplaceCaseInsensitive( "Assembly:", "" ).Trim().Split( ',' );
            var assemblyName = assemblyParts[0];
            var assemblyClass = assemblyParts[1];

            var printLabel = PrintLabelHelper.GetPrintLabelClass( assemblyName, assemblyClass );

            printLabel.Print( label, person, checkInState, groupType );
        }
        
        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
			printLabels();
		}}
		
		function alertDismissed() {{
		    // do something
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
                    navigator.notification.alert(
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", ZebraFormatString( jsonObject, true ) );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }

        #endregion
    }

}