// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace CheckinClient
{
    /// <summary>
    /// 
    /// </summary>
    [PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
    [System.Runtime.InteropServices.ComVisibleAttribute( true )]
    public class RockCheckinScriptManager
    {
        Page browserPage;
        ObjectCache cache;
        bool warnedPrinterError = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCheckinScriptManager"/> class.
        /// </summary>
        /// <param name="p">The p.</param>
        public RockCheckinScriptManager( Page p )
        {
            this.browserPage = (Page)p;
            cache = MemoryCache.Default;
        }

        /// <summary>
        /// Prints the labels.
        /// </summary>
        /// <param name="labelData">The label data.</param>
        public void PrintLabels( string labelData )
        {
            warnedPrinterError = false;

            string labelContents = string.Empty;
            var labels = JsonConvert.DeserializeObject<List<LabelItem>>( labelData );

            foreach ( LabelItem label in labels )
            {
                // get label file
                labelContents = GetLabelContents( label.LabelFile );   

                // merge fields
                labelContents = MergeLabelFields( labelContents, label.MergeFields );

                // print label
                PrintLabel( labelContents, label.PrinterAddress );
            }

            //RawPrinterHelper.SendStringToPrinter( "ZDesigner GX420d (Copy 1)", s );
        }

        /// <summary>
        /// Gets the label contents.
        /// </summary>
        /// <param name="labelFile">The label file.</param>
        /// <returns></returns>
        private string GetLabelContents( string labelFile )
        {
            string labelContents = string.Empty;

            if ( cache.Contains( labelFile ) )
            {
                //get an item from the cache  
                labelContents = cache.Get( labelFile ).ToString();
            }
            else
            {
                // get label from site
                using ( WebClient client = new WebClient() )
                {
                    labelContents = client.DownloadString( labelFile );
                }

                var rockConfig = RockConfig.Load();

                CacheItemPolicy cachePolicy = new CacheItemPolicy();
                cachePolicy.AbsoluteExpiration = new DateTimeOffset( DateTime.Now.AddSeconds( rockConfig.CacheLabelDuration ) );
                //add an item to the cache   
                cache.Add( labelFile, labelContents, cachePolicy );
            }

            return labelContents;
        }

        /// <summary>
        /// Merges the label fields.
        /// </summary>
        /// <param name="labelContents">The label contents.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        private string MergeLabelFields( string labelContents, Dictionary<string, string> mergeFields )
        {
            foreach ( var mergeField in mergeFields )
            {
                if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                {
                    labelContents = Regex.Replace( labelContents, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), mergeField.Value );
                }
                else
                {
                    // Remove the box preceding merge field
                    labelContents = Regex.Replace( labelContents, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                    // Remove the merge field
                    labelContents = Regex.Replace( labelContents, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                }
            }

            return labelContents;
        }

        /// <summary>
        /// Prints the label.
        /// </summary>
        /// <param name="labelContents">The label contents.</param>
        /// <param name="labelPrinterIp">The label printer ip.</param>
        private void PrintLabel( string labelContents, string labelPrinterIp )
        {
            var rockConfig = RockConfig.Load();
            
            // if IP override
            if ( !string.IsNullOrEmpty(rockConfig.PrinterOverrideIp) )
            {
                PrintViaIp( labelContents, rockConfig.PrinterOverrideIp );
            }
            else if ( !string.IsNullOrEmpty(rockConfig.PrinterOverrideLocal) ) // if printer local
            {
                RawPrinterHelper.SendStringToPrinter( rockConfig.PrinterOverrideLocal, labelContents );
            }
            else // else print to given IP
            {
                PrintViaIp( labelContents, labelPrinterIp );
            }
        }

        /// <summary>
        /// Prints the via ip.
        /// </summary>
        /// <param name="labelContents">The label contents.</param>
        /// <param name="ipAddress">The ip address.</param>
        private void PrintViaIp( string labelContents, string ipAddress )
        {
            if ( !warnedPrinterError )
            {
                Socket socket = null;
                var printerIp = new IPEndPoint( IPAddress.Parse( ipAddress ), 9100 );

                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                IAsyncResult result = socket.BeginConnect( printerIp, null, null );
                bool success = result.AsyncWaitHandle.WaitOne( 5000, true );

                if ( socket.Connected )
                {
                    var ns = new NetworkStream( socket );
                    byte[] toSend = System.Text.Encoding.ASCII.GetBytes( labelContents );
                    ns.Write( toSend, 0, toSend.Length );
                }
                else
                {

                    MessageBox.Show( String.Format( "Could not connect to the printer {0}.", ipAddress ), "Print Error", MessageBoxButton.OK, MessageBoxImage.Error );
                    warnedPrinterError = true;
                }

                if ( socket != null && socket.Connected )
                {
                    socket.Shutdown( SocketShutdown.Both );
                    socket.Close();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LabelItem
    {
        public int? PrinterDeviceId { get; set; }
        public string PrinterAddress { get; set; }
        public string LabelFile { get; set; }
        public Dictionary<string, string> MergeFields { get; set; }
    }
}