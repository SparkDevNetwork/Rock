using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

        public readonly string[] UserAgentStrings = new string[] {
            "Mozilla/5.0 (Linux; Android 5.0; SM-G900V Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13A452",
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; Trident/4.0)",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",
            "Safari/11601.4.4 CFNetwork/760.2.6 Darwin/15.3.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.4.4 (KHTML, like Gecko)",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.2.7 (KHTML, like Gecko) Version/9.0.1 Safari/601.2.7",
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36",
            "Safari/11601.1.56 CFNetwork/760.0.5 Darwin/15.0.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.97 Safari/537.36",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13A452",
            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; WOW64; Trident/6.0)",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) CriOS/46.0.2490.73 Mobile/13B143 Safari/600.1.4",
            "Safari/10601.4.4 CFNetwork/720.5.7 Darwin/14.5.0 (x86_64)",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240",
            "Mozilla/5.0 (Linux; Android 5.0; SM-G900V Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.1.56 (KHTML, like Gecko)",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/600.8.9 (KHTML, like Gecko) Version/8.0.8 Safari/600.8.9",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1 (KHTML, like Gecko) CriOS/48.0.2564.104 Mobile/13D15 Safari/601.1.46",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9"
        };

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, EventArgs e )
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            progressBar1.Show();

            int clientCount = tbClientCount.Text.AsInteger();
            int requestCountPerClient = tbRequestCount.Text.AsInteger();
            string url = tbUrl.Text;

            ConcurrentBag<double> results = new ConcurrentBag<double>();
            ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception>();

            var stopwatchTestDuration = Stopwatch.StartNew();
            long requestCount = 0;
            int threadCount = 0;
            progressBar1.Maximum = clientCount * requestCountPerClient;
            var requestUrl = new Uri( url );
            var baseUri = new Uri( requestUrl.Scheme + "://" + requestUrl.Host + ":" + requestUrl.Port.ToString() );


            Parallel.For( 1, clientCount, ( loopState ) =>
            {
                Interlocked.Increment( ref threadCount );
                try
                {
                    int requestCounter = 0;
                    var cookieContainer = new CookieContainer();
                    while ( requestCounter < requestCountPerClient )
                    {

                        var stopwatch = Stopwatch.StartNew();
                        var clientRequest = (HttpWebRequest)WebRequest.Create( url );
                        clientRequest.CookieContainer = cookieContainer;
                        clientRequest.UserAgent = UserAgentStrings[new Random().Next( 0, UserAgentStrings.Length )];
                        clientRequest.Timeout = 10000;

                        using ( var response = clientRequest.GetResponse() )
                        {
                            using ( var stream = response.GetResponseStream() )
                            {
                                using ( var reader = new StreamReader( stream ) )
                                {
                                    var responseHtml = reader.ReadToEnd();
                                    stopwatch.Stop();

                                    if ( requestCounter == 1 )
                                    {
                                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                                        htmlDoc.LoadHtml( responseHtml );
                                        var nodesWithSrc = htmlDoc.DocumentNode.DescendantsAndSelf()
                                            .Where( a => a.NodeType == HtmlAgilityPack.HtmlNodeType.Element )
                                            .Where( a => a.Attributes.Any( x => x.Name == "src" ) )
                                            .ToList();
                                        Parallel.ForEach( nodesWithSrc, ( srcNode ) =>
                                        {
                                            try
                                            {
                                                var srcRef = srcNode.Attributes["src"].Value;
                                                var srcUri = new Uri( baseUri, srcRef );
                                                var srcRequest = (HttpWebRequest)WebRequest.Create( srcUri );
                                                var srcResponse = srcRequest.GetResponse();

                                                using ( var resultStream = srcResponse.GetResponseStream() )
                                                {
                                                    using ( var resultReader = new StreamReader( resultStream ) )
                                                    {
                                                        var resultData = resultReader.ReadToEnd();
                                                    }
                                                }
                                            }
                                            catch ( Exception ex )
                                            {
                                                exceptions.Add( ex );
                                            }
                                        } );
                                    }

                                    Interlocked.Increment( ref requestCount );
                                    //if ( requestCount % 100 == 0 )
                                    {
                                        UpdateProgressBar( requestCount, threadCount );
                                    }
                                }
                            }

                        }

                        stopwatch.Stop();
                        results.Add( Math.Round( stopwatch.Elapsed.TotalMilliseconds, 3 ) );
                        requestCounter++;

                    }
                }
                catch ( Exception ex )
                {
                    exceptions.Add( ex );
                }

                Interlocked.Decrement( ref threadCount );
            } );

            UpdateProgressBar( requestCount, threadCount );
            var totalTime = results.Sum();
            var requestsPerMillisecond = requestCount / totalTime;


            var aveResponseTime = totalTime / requestCount;
            try
            {
                tbStats.Text = string.Format( @"
Median: {0:0.000}ms responseTime 
Mode: {1:0.000}ms responseTime 
Average: {2:0.000}ms responseTime 
TotalRequests: {3},
TotalTime: {4:0.000}ms
Requests/sec: {5:0.000}
Exceptions: {6}
", results.Median(),
       results.Mode(),
       results.Average(),
       results.Count(),
       stopwatchTestDuration.Elapsed.TotalMilliseconds,
       requestsPerMillisecond * 1000,
       exceptions.Count() );

                tbStats.Text = tbStats.Text.Trim();
            }
            catch ( Exception ex )
            {
                tbStats.Text = ex.Message;
            }

            tbResults.Text = exceptions.Select( a => a.Message ).ToList().AsDelimited( Environment.NewLine )
                + Environment.NewLine
                + results.ToList().AsDelimited( Environment.NewLine );

            progressBar1.Value = progressBar1.Maximum;
            progressBar1.Hide();
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="requestCount">The request count.</param>
        /// <param name="threadCount">The thread count.</param>
        private void UpdateProgressBar( long requestCount, long threadCount )
        {
            if ( InvokeRequired )
            {
                BeginInvoke( new Action<long, long>( UpdateProgressBar ), new object[] { requestCount, threadCount } );
                return;
            }

            progressBar1.Value = (int)Interlocked.Read( ref requestCount );
            lblThreadCount.Text = Interlocked.Read( ref threadCount ).ToString();
            lblThreadCount.Refresh();
        }
    }
}
