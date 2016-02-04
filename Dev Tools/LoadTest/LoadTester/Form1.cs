using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Rock;

namespace LoadTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click( object sender, EventArgs e )
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;

            ConcurrentBag<string> clientRequests = new ConcurrentBag<string>();
            int clientCount = tbClientCount.Text.AsInteger();
            int clientIndex = 0;
            int requestCountPerClient = tbRequestCount.Text.AsInteger();
            string url = tbUrl.Text;
            while (clientIndex < clientCount)
            {
                var request = (HttpWebRequest)WebRequest.Create( url );
                request.Proxy = null;
                request.KeepAlive = true;
                clientRequests.Add( url );
                clientIndex++;
            }

            List<double> results = new List<double>();
            
            var stopwatchTotal = Stopwatch.StartNew();
            long requestCount = 0;
            

            Parallel.ForEach( clientRequests, ( clientRequestUrl ) =>
            {
                try
                {
                    int requestCounter = 0;
                    var cookieContainer = new CookieContainer();
                    while ( requestCounter < requestCountPerClient )
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var clientRequest = (HttpWebRequest)WebRequest.Create( url );
                        clientRequest.CookieContainer = cookieContainer;
                        clientRequest.Timeout = 10000;
                        using ( var response = clientRequest.GetResponse() )
                        {
                            using ( var stream = response.GetResponseStream() )
                            {
                                using ( var reader = new StreamReader( stream ) )
                                {
                                    reader.ReadToEnd();
                                    Interlocked.Increment( ref requestCount );
                                }
                            }
                        }

                        stopwatch.Stop();
                        results.Add(stopwatch.Elapsed.TotalMilliseconds );
                        requestCounter++;
                    }
                }
                catch ( Exception ex )
                {
                    //results.Add( ex.Message );
                }
            } );

            var requestsPerMillisecond = requestCount / stopwatchTotal.Elapsed.TotalMilliseconds;
            tbResults.Text = (requestsPerMillisecond * 1000).ToString() + " requests/sec" + Environment.NewLine + results.ToList().AsDelimited( Environment.NewLine );
            
        }

        private void tbClientCount_TextChanged( object sender, EventArgs e )
        {

        }
    }
}
