using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace CheckinClient
{
    [PermissionSet( SecurityAction.Demand, Name = "FullTrust" )]
    [System.Runtime.InteropServices.ComVisibleAttribute( true )]
    public class RockCheckinScriptManager
    {
        Page browserPage;
        ObjectCache cache;
        bool warnedPrinterError = false;

        public RockCheckinScriptManager( Page p )
        {
            this.browserPage = (Page)p;
            cache = MemoryCache.Default;
        }

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

                int cacheDuration = 1440;

                Int32.TryParse( ConfigurationManager.AppSettings["CacheLabelDuration"], out cacheDuration );

                CacheItemPolicy cachePolicy = new CacheItemPolicy();
                cachePolicy.AbsoluteExpiration = new DateTimeOffset( DateTime.Now.AddMilliseconds( cacheDuration ) );
                //add an item to the cache   
                cache.Add( labelFile, labelContents, cachePolicy );
            }

            return labelContents;
        }

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

        private void PrintLabel( string labelContents, string labelPrinterIp )
        {
            // if IP override
            if ( ConfigurationManager.AppSettings["PrinterOverrideIp"] != string.Empty )
            {
                PrintViaIp( labelContents, ConfigurationManager.AppSettings["PrinterOverrideIp"] );
            }
            else if ( ConfigurationManager.AppSettings["PrinterOverrideLocal"] != string.Empty ) // if printer local
            {  
                RawPrinterHelper.SendStringToPrinter( ConfigurationManager.AppSettings["PrinterOverrideLocal"], labelContents );
            }
            else // else print to given IP
            {
                PrintViaIp( labelContents, labelPrinterIp );
            }
        }

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

    public class LabelItem
    {
        public int PrinterDeviceId { get; set; }
        public string PrinterAddress { get; set; }
        public string LabelFile { get; set; }
        public Dictionary<string, string> MergeFields { get; set; }
    }
}