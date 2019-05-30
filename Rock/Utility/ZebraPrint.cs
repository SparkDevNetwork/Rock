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

using Rock.CheckIn;

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
    }
}
