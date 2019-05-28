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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.UI;

using Newtonsoft.Json;

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility
{
    /// <summary>
    /// Utility class for printing to Zebra printers
    /// </summary>
    public static class ZebraPrint
    {
        #region Public Methods
        /// <summary>
        /// Prints the labels.
        /// </summary>
        /// <param name="labels">The labels.</param>
        /// <returns></returns>
        public static List<string> PrintLabels( List<CheckInLabel> labels )
        {
            var messages = new List<string>();

            Socket socket = null;
            string currentIp = string.Empty;

            foreach ( var label in labels
                                .OrderBy( l => l.PersonId )
                                .ThenBy( l => l.Order ) )
            {
                var labelCache = KioskLabel.Get( label.FileGuid );
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

                            socket = ZebraPrint.OpenSocket( label.PrinterAddress );
                        }

                        string printContent = ZebraPrint.MergeLabelFields( labelCache.FileContent, label.MergeFields );

                        if ( socket.Connected )
                        {
                            ZebraPrint.Print( printContent, socket );
                        }
                        else
                        {
                            messages.Add( "NOTE: Could not connect to printer!" );
                        }
                    }
                }
            }

            // Close the socket
            if ( socket != null && socket.Connected )
            {
                socket.Shutdown( SocketShutdown.Both );
                socket.Close();
            }

            return messages;
        }

        /// <summary>
        /// Prints a single label.
        /// </summary>
        /// <param name="printerIpAddress">The printer ip address.</param>
        /// <param name="label">The label.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public static string PrintLabel( string printerIpAddress, string label, Dictionary<string, string> mergeFields = null )
        {
            var message = string.Empty;

            if ( mergeFields == null )
            {
                mergeFields = new Dictionary<string, string>();
            }

            var socket = ZebraPrint.OpenSocket( printerIpAddress );

            string printContent = ZebraPrint.MergeLabelFields( label, mergeFields );

            if ( socket.Connected )
            {
                ZebraPrint.Print( printContent, socket );
            }
            else
            {
                message = "NOTE: Could not connect to printer!";
            }

            // Close the socket
            if ( socket != null && socket.Connected )
            {
                socket.Shutdown( SocketShutdown.Both );
                socket.Close();
            }

            return message;
        }
        #endregion

        /// <summary>
        /// Handles printing labels for the given parameters using the
        /// label data stored on the AttendanceData model.
        /// </summary>
        /// <param name="fileGuids">The file guids of the label types to print.</param>
        /// <param name="personId">The person whose labels to print.</param>
        /// <param name="selectedAttendanceIds">The attendance Ids that have the labels to be reprinted.</param>
        /// <param name="control">The control to register/inject the client side printing into.</param>
        /// <param name="printerAddress">The IP Address of a printer to send the print job to, overriding what is in the label.</param>
        /// <returns>A list of any messages that occur during printing.</returns>
        public static List<string> ReprintZebraLabels( List<Guid> fileGuids, int personId, List<int> selectedAttendanceIds, Control control, string printerAddress = null )
        {
            // Fetch the actual labels and print them
            var rockContext = new RockContext();
            var attendanceService = new Rock.Model.AttendanceService( rockContext );

            // Get the selected attendance records
            var attendanceRecords = attendanceService.GetByIds( selectedAttendanceIds );

            var printFromClient = new List<CheckInLabel>();
            var printFromServer = new List<CheckInLabel>();

            // Now grab only the selected label types (matching fileGuids) from those record's AttendanceData
            // for the selected  person
            foreach ( var attendance in attendanceRecords )
            {
                var attendanceData = attendance.AttendanceData;
                var json = attendanceData.LabelData.Trim();

                // skip if the return type is not an array
                if ( json.Substring( 0, 1 ) != "[" )
                {
                    continue;
                }

                // De-serialize the JSON into a list of objects
                var checkinLabels = JsonConvert.DeserializeObject<List<CheckInLabel>>( json );

                // skip if no labels were found
                if ( checkinLabels == null )
                {
                    continue;
                }

                // Take only the labels that match the selected person and label types (file guids).
                checkinLabels = checkinLabels.Where( l => l.PersonId == personId && fileGuids.Contains( l.FileGuid ) ).ToList();

                // Override the printer by printing to the given printerAddress?
                if ( !string.IsNullOrEmpty( printerAddress ) )
                {
                    checkinLabels.ToList().ForEach( l => l.PrinterAddress = printerAddress );
                    printFromServer.AddRange( checkinLabels );
                }
                else
                {
                    printFromClient.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                    printFromServer.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                }
            }

            // Print client labels
            if ( printFromClient.Any() )
            {
                printFromClient
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order )
                    .ToList();

                AddLabelScript( printFromClient.ToJson(), control );
            }

            var messages = new List<string>();

            // Print server labels
            if ( printFromServer.Any() )
            {
                messages = ZebraPrint.PrintLabels( printFromServer );
            }

            // No messages is "good news".
            if ( messages.Count == 0 )
            {
                messages.Add( "The labels have been printed." );
            }

            return messages;
        }

        /// <summary>
        /// Adds the label script, registering it to the given control.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        /// <param name="control">The control.</param>
        public static void AddLabelScript( string jsonObject, Control control )
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
            try {{			
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
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
", jsonObject );
            ScriptManager.RegisterStartupScript( control, control.GetType(), "addLabelScript", script, true );
        }

        #region Private Methods
        /// <summary>
        /// Opens a socket to the IP Address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        private static Socket OpenSocket( string ipAddress )
        {
            int printerPort = 9100;
            var printerIpAddress = ipAddress;

            // If the user specified in 0.0.0.0:1234 syntax then pull our the IP and port numbers.
            if ( printerIpAddress.Contains( ":" ) )
            {
                var segments = printerIpAddress.Split( ':' );

                printerIpAddress = segments[0];
                printerPort = segments[1].AsInteger();
            }

            var printerEndpoint = new IPEndPoint( IPAddress.Parse( printerIpAddress ), printerPort );

            var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            IAsyncResult result = socket.BeginConnect( printerEndpoint, null, null );
            bool success = result.AsyncWaitHandle.WaitOne( 5000, true );

            return socket;
        }

        /// <summary>
        /// Merges the label fields.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        private static string MergeLabelFields( string label, Dictionary<string, string> mergeFields )
        {
            foreach ( var mergeField in mergeFields )
            {
                if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                {
                    label = Regex.Replace( label, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), mergeField.Value );
                }
                else
                {
                    // Remove the box preceding merge field
                    label = Regex.Replace( label, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                    // Remove the merge field
                    label = Regex.Replace( label, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                }
            }

            return label;
        }

        /// <summary>
        /// Prints the specified label with the proper encoding.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="socket">The socket.</param>
        private static void Print( string label, Socket socket )
        {
            if ( socket.Connected )
            {
                var ns = new NetworkStream( socket );
                //var encoder = System.Text.Encoding.GetEncoding( "ISO-8859-1" );
                var encoder = System.Text.Encoding.UTF8;
                byte[] toSend = encoder.GetBytes( label );
                ns.Write( toSend, 0, toSend.Length );
            }
        }
        #endregion

        #region Reprint Label Helper Methods & Classes

        /// <summary>
        /// Get a list of available check-in label types to reprint for the given person and attendanceIds.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="attendanceIds"></param>
        /// <returns>a list of available ReprintLabelCheckInLabelType</returns>
        public static List<ReprintLabelCheckInLabelType> GetLabelTypesForPerson( int personId, List<int> attendanceIds )
        {
            List<ReprintLabelCheckInLabelType> labelTypes = new List<ReprintLabelCheckInLabelType>();

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );

            // Get the attendance records for the set given to us
            var attendanceRecords = attendanceService.GetByIds( attendanceIds );

            // If no data was found return the empty list.
            if ( attendanceRecords == null )
            {
                return labelTypes;
            }

            var handledList = new Dictionary<Guid, bool>();

            foreach ( var attendance in attendanceRecords.Where( a => a.AttendanceData != null ) )
            {
                var attendanceData = attendance.AttendanceData;

                var json = attendanceData.LabelData.Trim();

                // If no data was found, then skip this attendance record.
                if ( json == null )
                {
                    continue;
                }

                // determine if the return type is an array or not
                if ( json.Substring( 0, 1 ) == "[" )
                {
                    // De-serialize the JSON into a list of objects
                    var checkinLabels = JsonConvert.DeserializeObject<List<CheckInLabel>>( json );
                    if ( checkinLabels == null )
                    {
                        continue;
                    }

                    var fileGuids = checkinLabels.Where( l => l.PersonId == personId && !handledList.ContainsKey( l.FileGuid ) )
                        .Select( l => l.FileGuid )
                        .ToList();

                    if ( fileGuids == null || fileGuids.Count == 0 )
                    {
                        continue;
                    }

                    var labels = binaryFileService.GetByGuids( fileGuids );

                    foreach ( var label in labels )
                    {
                        handledList.AddOrReplace( label.Guid, true );
                        labelTypes.Add( new ReprintLabelCheckInLabelType
                        {
                            Name = label.FileName,
                            LabelFileId = ( int ) label.Id,
                            FileGuid = label.Guid,
                            PersonId = personId,
                            AttendanceIds = attendanceIds
                        } );
                    }
                }
            }

            return labelTypes;
        }

        #endregion
    }

    #region Reprint Label Helper Classes

    /// <summary>
    /// The class structure used with label reprinting as a view model for on screen button choices.
    /// </summary>
    public class ReprintLabelPersonResult
    {
        /// <summary>
        /// The Person's Id 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// A list of Attendance Ids.
        /// </summary>
        public List<int> AttendanceIds { get; set; }
        /// <summary>
        /// The Guid of the Person.
        /// </summary>
        public Guid PersonGuid { get; set; }
        /// <summary>
        /// The Person's name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the location and schedule names shown on the button.
        /// </summary>
        /// <value>
        /// The location and schedule names.
        /// </value>
        public string LocationAndScheduleNames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReprintLabelPersonResult"/> class.
        /// </summary>
        public ReprintLabelPersonResult()
        {
        }

        /// <summary>
        /// Constructor to create an object for the given attendance list.
        /// </summary>
        /// <param name="attendances"></param>
        public ReprintLabelPersonResult( List<Attendance> attendances )
        {
            if ( attendances.Any() )
            {
                var person = attendances.First().PersonAlias.Person;
                Id = person.Id;
                AttendanceIds = attendances.Select( a => a.Id ).ToList();
                PersonGuid = person.Guid;
                Name = person.FullName;

                LocationAndScheduleNames = attendances
                    .Select( a => string.Format( "{0} {1}",
                            a.Occurrence.Location.Name,
                            a.Occurrence.Schedule != null ? a.Occurrence.Schedule.Name : string.Empty ) )
                    .Distinct()
                    .ToList()
                    .AsDelimited( "\r\n" );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} <span class='pull-right'>{1}</span>", Name, LocationAndScheduleNames );
        }
    }

    /// <summary>
    /// Class used as a structure for the reprinting of labels.
    /// </summary>
    public class ReprintLabelCheckInLabelType
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the label binary file identifier.
        /// </summary>
        /// <value>
        /// The label file identifier.
        /// </value>
        public int LabelFileId { get; set; }
        /// <summary>
        /// Gets or sets the label binary file unique identifier.
        /// </summary>
        /// <value>
        /// The file unique identifier.
        /// </value>
        public Guid FileGuid { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the person's Id.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }
        /// <summary>
        /// Gets or sets a list of attendance ids.
        /// </summary>
        /// <value>
        /// The attendance ids.
        /// </value>
        public List<int> AttendanceIds { get; set; }
    }
    #endregion
}
