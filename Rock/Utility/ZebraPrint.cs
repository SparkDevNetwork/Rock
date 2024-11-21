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
using System.Threading.Tasks;
using System.Threading;
using System.Web.UI;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using CheckInLabel = Rock.CheckIn.CheckInLabel;
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Utility;

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
            var cloudLabels = new List<RenderedLabel>();

            Socket socket = null;
            string currentIp = string.Empty;
            bool hasPrinterCutter = PrinterHasCutter( labels );
            int labelCount = 0;

            foreach ( var label in labels.OrderBy( l => l.PersonId ).ThenBy( l => l.Order ) )
            {
                labelCount++;

                var labelCache = KioskLabel.Get( label.FileGuid );
                if ( labelCache != null )
                {
                    if ( !string.IsNullOrWhiteSpace( label.PrinterAddress ) )
                    {
                        string printContent = ZebraPrint.MergeLabelFields( labelCache.FileContent, label.MergeFields ).TrimEnd();

                        // Check if this label needs to be cloud printed.
                        if ( label.PrinterDeviceId.HasValue )
                        {
                            var printerCache = DeviceCache.Get( label.PrinterDeviceId.Value );

                            if ( printerCache != null && printerCache.ProxyDeviceId.HasValue )
                            {
                                cloudLabels.Add( new RenderedLabel
                                {
                                    PrintFrom = PrintFrom.Server,
                                    Data = System.Text.Encoding.UTF8.GetBytes( printContent ),
                                    PrintTo = printerCache
                                } );

                                continue;
                            }
                        }

                        if ( label.PrinterAddress != currentIp )
                        {
                            if ( socket != null && socket.Connected )
                            {
                                socket.Shutdown( SocketShutdown.Both );
                                socket.Close();
                            }

                            socket = ZebraPrint.OpenSocket( label.PrinterAddress );
                        }

                        // If the "enable label cutting" feature is enabled, then we are going to
                        // control which mode the printer is in. In this case, we will remove any
                        // tear-mode (^MMT) commands from the content and add the cut-mode (^MMC).
                        if ( hasPrinterCutter )
                        {
                            printContent = printContent.Replace( "^MMT", string.Empty );

                            // Here we are forcing the printer into cut mode (because
                            // we don't know if it has been put into cut-mode already) even
                            // though we might be suppressing the cut below. This is correct.
                            printContent = printContent.ReplaceIfEndsWith( "^XZ", "^MMC^XZ" );

                            // If it's not the last label or a "ROCK_CUT" label, then inject
                            // a suppress back-feed (^XB) command which will also suppress the cut.
                            if ( !( labelCount == labels.Count() || printContent.Contains( "ROCK_CUT" ) ) )
                            {
                                printContent = printContent.ReplaceIfEndsWith( "^XZ", "^XB^XZ" );
                            }
                        }

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

            // If we have any labels that need to be printed via cloud print
            // then do so now.
            if ( cloudLabels.Any() )
            {
                try
                {
                    var printProvider = new LabelPrintProvider();
                    var cts = new CancellationTokenSource( 5000 );

                    var task = Task.Run( async () => await printProvider.PrintLabelsAsync( cloudLabels, cts.Token ) );

                    task.Wait();

                    if ( task.Result.Any() )
                    {
                        messages.AddRange( task.Result );
                    }
                }
                catch ( Exception ex )
                {
                    messages.Add( ex.Message );
                }
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
        /// Gets all label types that can be re-printed for the specified
        /// attendance identifiers.
        /// </summary>
        /// <param name="attendanceIds">The attendance identifiers to reprint.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects that represent the types of labels that can be printed.</returns>
        [RockInternal( "1.16.7", true )]
        public static List<ListItemBag> GetReprintNextGenLabelTypes( List<int> attendanceIds )
        {
            var director = new CheckIn.v2.CheckInDirector( new RockContext() );
            var labels = director.LabelProvider.RenderLabels( attendanceIds, null, false );

            return labels
                .Where( l => l.Error.IsNullOrWhiteSpace() )
                .Select( l => new ListItemBag
                {
                    Value = l.LabelId,
                    Text = l.LabelName
                } )
                .DistinctBy( l => l.Value )
                .OrderBy( l => l.Text )
                .ToList();
        }

        /// <summary>
        /// Attempts to re-print any next-gen labels for the specified attendance
        /// identifiers.
        /// </summary>
        /// <param name="attendanceIds">The attendance identifiers to reprint.</param>
        /// <param name="kiosk">The kiosk device requesting the re-print.</param>
        /// <param name="printerOverride">The printer device to use as an override for normal print destination. Leave <c>null</c> for no override.</param>
        /// <param name="printFromOverride">The <see cref="PrintFrom"/> value to use as an override. Leave <c>null</c> for no override.</param>
        /// <param name="onlyPrintLabelTypes">If not <c>null</c> or empty, this contains the encrypted identifier values of the <see cref="RenderedLabel.LabelId"/> items that should be printed.</param>
        /// <param name="errorMessages">On return contains any error messages.</param>
        /// <param name="clientLabels">On return contains any labels that need to be printed on the client.</param>
        /// <returns><c>true</c> if any labels were found to be printed.</returns>
        [RockInternal( "1.16.7", true )]
        public static bool TryReprintNextGenLabels( List<int> attendanceIds, DeviceCache kiosk, DeviceCache printerOverride, PrintFrom? printFromOverride, List<string> onlyPrintLabelTypes, out List<string> errorMessages, out List<ClientLabelBag> clientLabels )
        {
            var director = new CheckIn.v2.CheckInDirector( new RockContext() );
            var labels = director.LabelProvider.RenderLabels( attendanceIds, kiosk, false );

            errorMessages = labels.Where( l => l.Error.IsNotNullOrWhiteSpace() )
                .Select( l => l.Error )
                .ToList();

            if ( onlyPrintLabelTypes != null && onlyPrintLabelTypes.Any() )
            {
                labels = labels.Where( l => onlyPrintLabelTypes.Contains( l.LabelId ) ).ToList();
            }

            labels = labels.Where( l => l.Error.IsNullOrWhiteSpace() ).ToList();

            if ( !labels.Any() )
            {
                clientLabels = new List<ClientLabelBag>();
                return false;
            }

            var printer = printerOverride ?? DeviceCache.Get( kiosk?.PrinterDeviceId ?? 0 );

            foreach ( var label in labels )
            {
                label.PrintTo = printer;

                if ( printFromOverride.HasValue )
                {
                    label.PrintFrom = printFromOverride.Value;
                }
            }

            clientLabels = labels.Where( l => l.PrintFrom == PrintFrom.Client )
                .Select( l => new ClientLabelBag
                {
                    PrinterAddress = l.PrintTo?.IPAddress,
                    Data = Convert.ToBase64String( l.Data )
                } )
                .ToList();

            // Print the labels with a 5 second timeout.
            var cts = new CancellationTokenSource( 5_000 );
            var printProvider = new LabelPrintProvider();

            try
            {
                var serverLabels = labels.Where( l => l.PrintFrom == PrintFrom.Server );

                if ( serverLabels.Any() )
                {
                    var printerErrors = Task.Run( async () => await printProvider.PrintLabelsAsync( serverLabels, cts.Token ) ).Result;

                    errorMessages.AddRange( printerErrors );
                }
            }
            catch ( TaskCanceledException ) when ( cts.IsCancellationRequested )
            {
                errorMessages.Add( "Timeout waiting for labels to print." );
            }

            return true;
        }

        /// <summary>
        /// Handles printing labels for the given parameters using the
        /// label data stored on the AttendanceData model.
        /// </summary>
        /// <param name="fileGuids">The file guids of the label types to print.</param>
        /// <param name="personId">The person whose labels to print.</param>
        /// <param name="selectedAttendanceIds">The attendance Ids that have the labels to be reprinted.</param>
        /// <param name="control">The control to register/inject the client side printing into. This should be
        /// a control that is inside an UpdatePanel control so that the ScriptManager can register the needed client script block.</param>
        /// <param name="request">The HTTP Request so that the request's URL can be used when needed for client printing.</param>
        /// <param name="printerAddress">The IP Address of a printer to send the print job to, overriding what is in the label.</param>
        /// <returns>
        /// A list of any messages that occur during printing.
        /// </returns>
        [Obsolete( "Use the ReprintZebraLabels with the ReprintLabelOptions option" )]
        [RockObsolete( "1.12" )]
        public static List<string> ReprintZebraLabels( List<Guid> fileGuids, int personId, List<int> selectedAttendanceIds, Control control, System.Web.HttpRequest request, string printerAddress = null )
        {
            ReprintLabelOptions reprintLabelOptions;
            if ( printerAddress.IsNotNullOrWhiteSpace() )
            {
                reprintLabelOptions = new ReprintLabelOptions
                {
                    ServerPrinterIPAddress = printerAddress,
                    PrintFrom = PrintFrom.Server
                };
            }
            else
            {
                reprintLabelOptions = new ReprintLabelOptions
                {
                    ServerPrinterIPAddress = null,
                    PrintFrom = null
                };
            }

            return ReprintZebraLabels( fileGuids, personId, selectedAttendanceIds, control, request, reprintLabelOptions );
        }

        /// <summary>
        /// Handles printing labels for the given parameters using the
        /// label data stored on the AttendanceData model.
        /// </summary>
        /// <param name="fileGuids">The file guids.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="selectedAttendanceIds">The selected attendance ids.</param>
        /// <param name="control">The control.</param>
        /// <param name="request">The request.</param>
        /// <param name="reprintLabelOptions">The reprint label options.</param>
        /// <returns></returns>
        public static List<string> ReprintZebraLabels( List<Guid> fileGuids, int personId, List<int> selectedAttendanceIds, Control control, System.Web.HttpRequest request, ReprintLabelOptions reprintLabelOptions )
        {
            var (messages, printFromClient) = ReprintZebraLabels( fileGuids, personId, selectedAttendanceIds, reprintLabelOptions );

            // Print client labels
            if ( printFromClient.Any() )
            {
                var urlRoot = string.Format( "{0}://{1}", request.UrlProxySafe().Scheme, request.UrlProxySafe().Authority );

                /*
                                // This is extremely useful when debugging with ngrok and an iPad on the local network.
                                // X-Original-Host will contain the name of your ngrok hostname, therefore the labels will
                                // get a LabelFile url that will actually work with that iPad.
                                if ( request.Headers["X-Original-Host"] != null )
                                {
                                    var scheme = request.Headers["X-Forwarded-Proto"] ?? "http";
                                    urlRoot = string.Format( "{0}://{1}", scheme, request.Headers.GetValues( "X-Original-Host" ).First() );
                                }
                */

                printFromClient
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order )
                    .ToList()
                    .ForEach( l => l.LabelFile = urlRoot + l.LabelFile );

                AddLabelScript( printFromClient.ToJson(), control );
            }

            return messages;
        }

        /// <summary>
        /// Handles printing labels for the given parameters using the
        /// label data stored on the AttendanceData model.
        /// </summary>
        /// <param name="fileGuids">The file guids.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="selectedAttendanceIds">The selected attendance ids.</param>
        /// <param name="reprintLabelOptions">The reprint label options.</param>
        /// <returns>A tuple that contains a list of error messages and a list of labels to be printed on the client.</returns>
        internal static (List<string>, List<CheckInLabel>) ReprintZebraLabels( List<Guid> fileGuids, int personId, List<int> selectedAttendanceIds, ReprintLabelOptions reprintLabelOptions )
        {
            // Fetch the actual labels and print them
            var rockContext = new RockContext();
            var attendanceService = new Rock.Model.AttendanceService( rockContext );

            reprintLabelOptions = reprintLabelOptions ?? new ReprintLabelOptions();

            // Get the selected attendance records (but only the ones that have label data)
            var labelDataList = attendanceService
                .GetByIds( selectedAttendanceIds )
                .Where( a => a.AttendanceData.LabelData != null )
                .Select( a => a.AttendanceData.LabelData );

            var printFromClient = new List<CheckInLabel>();
            var printFromServer = new List<CheckInLabel>();

            // Now grab only the selected label types (matching fileGuids) from those record's AttendanceData
            // for the selected  person
            foreach ( var labelData in labelDataList )
            {
                var json = labelData.Trim();

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

                // Take only the labels that match the selected person (or if they are Family type labels) and file guids).
                checkinLabels = checkinLabels.Where( l => ( l.PersonId == personId || l.LabelType == KioskLabelType.Family ) && fileGuids.Contains( l.FileGuid ) ).ToList();

                if ( reprintLabelOptions.PrintFrom == PrintFrom.Server && reprintLabelOptions.ServerPrinterIPAddress.IsNotNullOrWhiteSpace() )
                {
                    // Override the printer by printing to the given printerAddress?
                    checkinLabels.ToList().ForEach( l => l.PrinterAddress = reprintLabelOptions.ServerPrinterIPAddress );
                    printFromServer.AddRange( checkinLabels );
                }
                else if ( reprintLabelOptions.PrintFrom == PrintFrom.Client )
                {
                    // Override the printer by printing to the client
                    // send the checkin labels to ZebraPrint.js, which the ClientApp will use to print it
                    // IP Address of the printer will stay the same IP Address as where the original label was,
                    printFromClient.AddRange( checkinLabels );
                }
                else
                {
                    // Print to label's printer
                    printFromClient.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                    printFromServer.AddRange( checkinLabels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                }
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

            return (messages, printFromClient);
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
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/) && typeof window.RockCheckinNative === 'undefined') {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

		function onDeviceReady() {{
            try {{			
                printLabels();
            }} 
            catch (err) {{
                // if there is a js-reprintlabel-notification, show the error there 
                $('.js-reprintlabel-notification').text(err).removeClass('alert-info').addClass('alert-danger');
                console.log('An error occurred printing labels: ' + err);
            }}
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify({0}), 
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
            ScriptManager.RegisterClientScriptBlock( control, control.GetType(), "addLabelScript", script, true );
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

        /// <summary>
        /// Printers the has cutter.
        /// </summary>
        /// <param name="labels">The labels.</param>
        /// <returns></returns>
        private static bool PrinterHasCutter( List<CheckInLabel> labels )
        {
            bool hasCutter = false;
            var deviceId = labels.Select( a => a.PrinterDeviceId ).FirstOrDefault();
            if ( deviceId != null )
            {
                KioskDevice kioskDevice = KioskDevice.Get( deviceId.GetValueOrDefault(), new List<int>() );
                hasCutter = kioskDevice.Device.GetAttributeValue( Rock.SystemKey.DeviceAttributeKey.DEVICE_HAS_CUTTER ).AsBoolean();
            }

            return hasCutter;
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

                    var fileGuids = checkinLabels.Where( l => ( l.PersonId == personId || l.LabelType == KioskLabelType.Family ) && !handledList.ContainsKey( l.FileGuid ) )
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
    /// 
    /// </summary>
    public sealed class ReprintLabelOptions
    {
        /// <summary>
        /// Instead of printing to the label's printer, print to this IP instead
        /// </summary>
        /// <value>
        /// The printer IP address.
        /// </value>
        public string ServerPrinterIPAddress { get; set; }

        /// <summary>
        /// Where to print the labels. Leave null to print to label's original printer
        /// </summary>
        /// <value>
        /// The print from.
        /// </value>
        public PrintFrom? PrintFrom { get; set; }
    }

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
