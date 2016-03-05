using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
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
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; Trident/4.0)",
            "Mozilla/5.0 (Linux; Android 5.0; SM-G900V Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13A452",
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
            System.Net.ServicePointManager.DefaultConnectionLimit = 960;
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

            progressBar1.Show();

            int clientCount = tbClientCount.Text.AsInteger();
            int requestCountPerClient = tbRequestCount.Text.AsInteger();
            int requestDelayMS = tbRequestsDelay.Text.AsInteger();
            string url = tbUrl.Text;

            ConcurrentBag<ChartData> chartResults = new ConcurrentBag<ChartData>();
            ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception>();

            var stopwatchTestDuration = Stopwatch.StartNew();
            long requestCount = 0;
            int threadCount = 0;
            progressBar1.Maximum = clientCount * requestCountPerClient;
            var requestUrl = new Uri( url );
            var baseUri = new Uri( requestUrl.Scheme + "://" + requestUrl.Host + ":" + requestUrl.Port.ToString() );

            var random = new Random();
            long lastProgressCount = 0;
            bool downloadHeaderSrcElements = cbDownloadHeaderSrcElements.Checked;
            bool downloadBodySrcElements = cbDownloadBodySrcElements.Checked;
            lblThreadCount.Text = string.Empty;
            lblThreadCount.Visible = true;

            Parallel.For(
                0,
                clientCount,
                ( clientId ) =>
                {
                    Interlocked.Increment( ref threadCount );
                    try
                    {
                        int requestCounter = 0;
                        var cookieContainer = new CookieContainer();

                        while ( requestCounter < requestCountPerClient )
                        {
                            var clientRequest = HttpWebRequest.CreateHttp( url );
                            clientRequest.Proxy = null;
                            clientRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                            clientRequest.CookieContainer = cookieContainer;
                            clientRequest.UserAgent = UserAgentStrings[random.Next( 0, UserAgentStrings.Count() - 1 )];
                            clientRequest.Timeout = 10000;

                            var threadStopwatch = Stopwatch.StartNew();
                            string pageLoadTimeMS = string.Empty;
                            long responseLength = 0;

                            using ( var httpResponse = clientRequest.GetResponse() as HttpWebResponse )
                            {
                                if ( httpResponse.ResponseUri != clientRequest.RequestUri )
                                {
                                    throw new Exception( "Redirected:" + httpResponse.ResponseUri );
                                }

                                if ( httpResponse.StatusCode != HttpStatusCode.OK )
                                {
                                    throw new Exception( "StatusCode:" + httpResponse.StatusCode.ToString() );
                                }

                                responseLength = 0;

                                threadStopwatch.Stop();

                                using ( var stream = httpResponse.GetResponseStream() )
                                {
                                    using ( var reader = new StreamReader( stream ) )
                                    {
                                        var responseHtml = reader.ReadToEnd();
                                        responseLength = responseHtml.Length;
                                        if ( requestCounter == 1 && ( downloadHeaderSrcElements || downloadBodySrcElements ) )
                                        {
                                            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                                            htmlDoc.LoadHtml( responseHtml );
                                            List<HtmlNode> nodesWithSrc = new List<HtmlNode>();

                                            var headNode = htmlDoc.DocumentNode.Descendants( "head" ).First();
                                            var bodyNode = htmlDoc.DocumentNode.Descendants( "body" ).First();

                                            if ( downloadHeaderSrcElements )
                                            {
                                                nodesWithSrc.AddRange( headNode.DescendantsAndSelf()
                                                    .Where( a => a.NodeType == HtmlAgilityPack.HtmlNodeType.Element )
                                                    .Where( a => a.Attributes.Any( x => x.Name == "src" ) )
                                                    .ToList() );
                                            }

                                            if ( downloadBodySrcElements )
                                            {
                                                nodesWithSrc.AddRange( bodyNode.DescendantsAndSelf()
                                                    .Where( a => a.NodeType == HtmlAgilityPack.HtmlNodeType.Element )
                                                    .Where( a => a.Attributes.Any( x => x.Name == "src" ) )
                                                    .ToList() );
                                            }

                                            Parallel.ForEach(
                                                nodesWithSrc,
                                                ( srcNode ) =>
                                                {
                                                    string srcRef = string.Empty;
                                                    try
                                                    {
                                                        srcRef = srcNode.Attributes["src"].Value;
                                                        if ( !srcRef.StartsWith( "//" ) && srcRef.StartsWith( "/" ) )
                                                        {
                                                            var srcUri = new Uri( baseUri, srcRef );
                                                            var srcRequest = (HttpWebRequest)WebRequest.Create( srcUri );
                                                            srcRequest.Proxy = null;
                                                            srcRequest.Timeout = 1000;
                                                            srcRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                                                            var srcResponse = srcRequest.GetResponse();

                                                            using ( var resultStream = srcResponse.GetResponseStream() )
                                                            {
                                                                using ( var resultReader = new StreamReader( resultStream ) )
                                                                {
                                                                    var resultData = resultReader.ReadToEnd();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch ( Exception ex )
                                                    {
                                                        exceptions.Add( new Exception( ex.Message + srcRef ) );
                                                    }
                                                } );
                                        }

                                        Interlocked.Increment( ref requestCount );
                                        if ( requestCount != lastProgressCount )
                                        {
                                            UpdateProgressBar( requestCount, threadCount );
                                            lastProgressCount = requestCount;
                                        }
                                    }
                                }
                            }

                            threadStopwatch.Stop();
                            chartResults.Add( new ChartData
                            {
                                XValue = new DateTime( 2016, 1, 1 ).Add( stopwatchTestDuration.Elapsed ),
                                YValue = Math.Round( threadStopwatch.Elapsed.TotalMilliseconds, 3 ),
                                Series = string.Format( "ThreadId:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId ),
                                ResponseLength = responseLength
                            } );

                            requestCounter++;
                        }

                        System.Threading.Thread.Sleep( requestDelayMS );
                    }
                    catch ( Exception ex )
                    {
                        exceptions.Add( ex );
                    }

                    Interlocked.Decrement( ref threadCount );
                } );


            stopwatchTestDuration.Stop();
            UpdateProgressBar( requestCount, threadCount );
            var results = chartResults.Select( a => a.YValue );
            var totalTime = stopwatchTestDuration.Elapsed.TotalMilliseconds;
            var requestsPerMillisecond = requestCount / totalTime;

            var aveResponseTime = totalTime / requestCount;
            try
            {
                tbStats.Text = string.Format(
@"
Response Time (ms)
 - Median/Mode/Avg/Max/Min 
 - {0:0.0}/{1:0.0}/{2:0.0}/{3:0.0}/{4:0.0}
Total 
 - Requests: {5}
 - Time: {6:0.0}ms
 - Exceptions: {8}
---------------------
Average: {9}KB responseLength
Average: {2:0.0}ms responseTime
Requests/sec: {7:0.0}
---------------------
",
 results.Median(), // {0}
       results.Mode(), // {1}
       results.Average(), // {2}
       results.Max(), // {3}
       results.Min(), // {4}
       results.Count(), // {5}
       stopwatchTestDuration.Elapsed.TotalMilliseconds, // {6}
       requestsPerMillisecond * 1000, // {7}
       exceptions.Count(), // {8}
       Math.Round( ( chartResults.Select( a => a.ResponseLength ).Where( a => a > 0 ).Average() / 1024 ), 0 ) // {9}
       );

                tbStats.Text = tbStats.Text.Trim();
            }
            catch ( Exception ex )
            {
                tbStats.Text = ex.Message;
            }

            tbStats.Text += Environment.NewLine + exceptions.Select( a => a.Message ).ToList().AsDelimited( Environment.NewLine );

            progressBar1.Value = progressBar1.Maximum;
            progressBar1.Hide();

            chart1.Series.Clear();

            var seriesDictionary = new Dictionary<string, System.Windows.Forms.DataVisualization.Charting.Series>();
            var chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = chartResults.Max( a => a.XValue.TimeOfDay.TotalMilliseconds );
            chartArea.AxisX.CustomLabels.Clear();
            double labelPosition = 0;
            var labelInterval = ( chartArea.AxisX.Maximum / 10 );
            while (labelPosition < chartArea.AxisX.Maximum)
            {
                var label = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
                label.FromPosition = labelPosition;
                label.ToPosition = labelPosition + labelInterval;
                label.Text = string.Format( "@{0}s", Math.Round( label.ToPosition / 1000, 3 ) );
                label.GridTicks = System.Windows.Forms.DataVisualization.Charting.GridTickTypes.Gridline;
                chartArea.AxisX.CustomLabels.Add( label );

                labelPosition += labelInterval;
            }

            foreach ( string seriesName in chartResults.Select( a => a.Series ).Distinct() )
            {
                var series = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Name = seriesName,
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline,
                    Font = this.Font,
                    LabelToolTip = "#VALYms response @time #VALX{D}ms "
                };

                seriesDictionary.Add( seriesName, series );
                chart1.Series.Add( series );
            }

            foreach ( var item in chartResults.OrderBy( a => a.XValue ) )
            {
                var point = new System.Windows.Forms.DataVisualization.Charting.DataPoint();
                point.SetValueXY( item.XValue.TimeOfDay.TotalMilliseconds, item.YValue );
                point.ToolTip = string.Format( "{0}ms @ + {1}\n{2}bytes", item.YValue, item.XValue.TimeOfDay.TotalMilliseconds, item.ResponseLength );
                point.LabelToolTip = point.ToolTip;
                seriesDictionary[item.Series].Points.Add( point );
            }

            chart1.Invalidate();
            lblThreadCount.Visible = false;
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
            if ( lblThreadCount.Text != threadCount.ToString() )
            {
                lblThreadCount.Text = threadCount.ToString();
                lblThreadCount.Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ChartData
        {
            public string Series { get; set; }

            public DateTime XValue { get; set; }

            public double YValue { get; set; }

            public long ResponseLength { get; set; }
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Form1_Load( object sender, EventArgs e )
        {
            lblThreadCount.Visible = false;
        }
    }
}
