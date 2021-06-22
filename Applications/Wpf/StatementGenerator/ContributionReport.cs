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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

using PuppeteerSharp;
using PuppeteerSharp.Media;

using RestSharp;

using Rock.Client;
using Rock.Client.Enums;

using Page = PuppeteerSharp.Page;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    ///
    /// </summary>
    public class ContributionReport : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionReport"/> class.
        /// </summary>
        public ContributionReport( Rock.Client.FinancialStatementGeneratorOptions options, ProgressPage progressPage )
        {
            this.Options = options;
            this.ProgressPage = progressPage;
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Rock.Client.FinancialStatementGeneratorOptions Options { get; set; }

        private ProgressPage ProgressPage { get; set; }

        private bool _cancelRunning = false;
        private bool _cancelled = false;

        // The max number of Chrome.exe threads to allow to run at the same time.
        // The optimal number seems to be the computer's ProcessorCount, plus 4 more.
        private readonly int _maxRenderThreads = Environment.ProcessorCount + 4;

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            _cancelRunning = true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is canceled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled => _cancelled;

        /// <summary>
        /// Gets the records completed count.
        /// </summary>
        /// <value>
        /// The records completed count.
        /// </value>
        public long RecordsCompletedCount => Interlocked.Read( ref _recordsCompleted );

        private long _recordsCompleted = 0;

        private ConcurrentBag<Task> _saveAndUploadTasks;

        private ConcurrentBag<Task> _renderPdfTasks;

        private FinancialStatementTemplateReportSettings _reportSettings;

        private ConcurrentBag<double> _generatePdfTimingsMS = null;
        private ConcurrentBag<double> _saveAndUploadPdfTimingsMS = null;
        private ConcurrentBag<double> _getStatementHtmlTimingsMS = null;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ContributionReport"/> is resume.
        /// </summary>
        /// <value>
        ///   <c>true</c> if resume; otherwise, <c>false</c>.
        /// </value>
        internal bool Resume { get; set; } = false;

        /// <summary>
        /// Gets or sets the resume run date.
        /// </summary>
        /// <value>
        /// The resume run date.
        /// </value>
        internal DateTime? ResumeRunDate { get; set; }

        private FinancialStatementIndividualSaveOptions _individualSaveOptions;
        private bool _saveStatementsForIndividualsToDocument;

        private RestClient _uploadPdfDocumentRestClient;

        /// <summary>
        /// The start date time
        /// </summary>
        public DateTime StartDateTime { get; private set; }

        private Stopwatch _stopwatchAll;
        private Stopwatch _stopwatchRenderPDFsOverall;

        private string _currentDayTemporaryDirectory { get; set; }

        private Browser browser;

        private ConcurrentStack<Page> availablePagesCache;

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        /// <returns></returns>
        public ResultsSummary RunReport()
        {
            InitializeChromeEngine();

            UpdateProgress( "Starting...", 0, 0 );

            // spin up Chrome render engines for each thread
            // These will show as chrome.exe in Task Manager
            availablePagesCache = new ConcurrentStack<Page>();
            for ( int i = 0; i < _maxRenderThreads; i++ )
            {
                var page = browser.NewPageAsync().Result;
                page.EmulateMediaTypeAsync( PuppeteerSharp.Media.MediaType.Screen ).Wait();
                availablePagesCache.Push( page );
            }

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            StartDateTime = DateTime.Now;
            _stopwatchAll = new Stopwatch();
            _stopwatchAll.Start();

            RockConfig rockConfig = RockConfig.Load();

            UpdateProgress( "Connecting...", 0, 0 );

            // Login and setup options for REST calls
            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            _individualSaveOptions = rockConfig.IndividualSaveOptionsJson.FromJsonOrNull<FinancialStatementIndividualSaveOptions>();
            _saveStatementsForIndividualsToDocument = _individualSaveOptions.SaveStatementsForIndividuals;
            if ( _saveStatementsForIndividualsToDocument )
            {
                _uploadPdfDocumentRestClient = new RestClient( rockConfig.RockBaseUrl );
                _uploadPdfDocumentRestClient.LoginToRock( rockConfig.Username, rockConfig.Password );
            }
            else
            {
                _uploadPdfDocumentRestClient = null;
            }

            FinancialStatementTemplate financialStatementTemplate = GetFinancialStatementTemplate( rockConfig, restClient );

            _reportSettings = financialStatementTemplate.ReportSettingsJson.FromJsonOrNull<FinancialStatementTemplateReportSettings>();

            List<FinancialStatementGeneratorRecipient> recipientList;

            if ( Resume && ResumeRunDate.HasValue )
            {
                _currentDayTemporaryDirectory = GetStatementGeneratorTemporaryDirectory( rockConfig, ResumeRunDate.Value );
                UpdateProgress( "Resuming Incomplete Recipients...", 0, 0 );

                // Get Recipients from save recipient list from the incomplete session
                recipientList = GetSavedRecipientList( ResumeRunDate.Value );
                SaveGeneratorConfig( _currentDayTemporaryDirectory, incrementRunAttempts: true, reportsCompleted: false );
            }
            else
            {
                _currentDayTemporaryDirectory = GetStatementGeneratorTemporaryDirectory( rockConfig, DateTime.Today );

                // Get Recipients from save recipient list from the incomplete session
                try
                {
                    if ( rockConfig.TemporaryDirectory.IsNotNullOrWhiteSpace() )
                    {
                        if ( Directory.Exists( rockConfig.TemporaryDirectory ) )
                        {
                            Directory.Delete( rockConfig.TemporaryDirectory, true );
                        }
                    }

                    if ( Directory.Exists( _currentDayTemporaryDirectory ) )
                    {
                        Directory.Delete( _currentDayTemporaryDirectory, true );
                    }
                }
                catch ( Exception ex )
                {
                    WriteToExceptionLog( $"INFO: Error deleting temp directories", ex );
                }

                // re-create directory
                Directory.CreateDirectory( _currentDayTemporaryDirectory );

                // Get Recipients from Rock REST Endpoint
                UpdateProgress( "Getting Statement Recipients...", 0, 0 );
                recipientList = GetRecipients( restClient );
                SaveGeneratorConfig( _currentDayTemporaryDirectory, incrementRunAttempts: false, reportsCompleted: false );
            }

            _recordsCompleted = 0;

            _renderPdfTasks = new ConcurrentBag<Task>();
            _saveAndUploadTasks = new ConcurrentBag<Task>();

            _generatePdfTimingsMS = new ConcurrentBag<double>();
            _saveAndUploadPdfTimingsMS = new ConcurrentBag<double>();
            _getStatementHtmlTimingsMS = new ConcurrentBag<double>();

            Directory.CreateDirectory( Path.Combine( _currentDayTemporaryDirectory, "Statements" ) );

            var recipientProgressMax = recipientList.Count;

            StartDateTime = DateTime.Now;

            List<FinancialStatementGeneratorRecipient> incompleteRecipients;
            int progressOffset = 0;
            if ( Resume )
            {
                incompleteRecipients = recipientList.Where( a => !a.IsComplete ).ToList();
                progressOffset = recipientList.Where( a => a.IsComplete ).Count();
            }
            else
            {
                incompleteRecipients = recipientList;
            }

            var enablePageCountPredetermination = this.Options.EnablePageCountPredetermination;

            SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory, false );

            _stopwatchRenderPDFsOverall = Stopwatch.StartNew();

            foreach ( var recipient in incompleteRecipients )
            {
                if ( _cancelRunning == true )
                {
                    break;
                }

                var recipientProgressPosition = Interlocked.Read( ref _recordsCompleted );

                UpdateProgress( "Generating Individual Documents...", recipientProgressPosition + progressOffset, recipientProgressMax, true );

                StartGenerateStatementForRecipient( recipient, restClient, enablePageCountPredetermination );
                SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory, true );
            }

            // all the render tasks should be done, but just in case
            UpdateProgress( $"Finishing up tasks", 0, 0 );

            // some of the render tasks could be running, so wait for those
            var remainingRenderTasks = _renderPdfTasks.ToArray().Where( a => !a.IsCompleted ).ToList();
            while ( remainingRenderTasks.Any() )
            {
                var finishedTask = Task.WhenAny( remainingRenderTasks.ToArray() );
                Thread.Sleep( 10 );
                remainingRenderTasks = remainingRenderTasks.ToArray().Where( a => !a.IsCompleted ).ToList();
                var recipientProgressPosition = Interlocked.Read( ref _recordsCompleted );
                UpdateProgress( $"Finishing up {remainingRenderTasks.Count() } Individual Statements...", recipientProgressPosition, recipientProgressMax );
            }

            Task.WaitAll( _renderPdfTasks.ToArray() );

            browser?.CloseAsync().Wait();

            // some of the 'Save and Upload' tasks could be running, so wait for those
            var remainingDocumentUploadTasks = _saveAndUploadTasks.ToArray().Where( a => !a.IsCompleted ).ToList();
            while ( remainingDocumentUploadTasks.Any() )
            {
                var finishedTask = Task.WhenAny( remainingDocumentUploadTasks.ToArray() );
                Thread.Sleep( 10 );
                remainingDocumentUploadTasks = remainingDocumentUploadTasks.ToArray().Where( a => !a.IsCompleted ).ToList();
                UpdateProgress( $"Finishing up {remainingDocumentUploadTasks.Count() } document uploads...", 0, 0 );
            }

            Task.WaitAll( remainingDocumentUploadTasks.ToArray() );

            if ( _cancelRunning )
            {
                this._cancelled = true;
                return new ResultsSummary( recipientList );
            }

            SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory, false );

            var reportCount = this.Options.ReportConfigurationList.Count();
            var reportNumber = 0;

            var resultsSummary = new ResultsSummary( recipientList );

            foreach ( var financialStatementReportConfiguration in this.Options.ReportConfigurationList )
            {
                reportNumber++;

                if ( reportCount == 1 )
                {
                    UpdateProgress( "Generating Report...", 0, 0 );
                }
                else
                {
                    UpdateProgress( $"Generating Report {reportNumber}", reportNumber-1, reportCount );
                }

                var summary = WriteStatementPDFs( financialStatementReportConfiguration, recipientList );
                resultsSummary.PaperStatementsSummaryList.Add( summary );
            }

            SaveGeneratorConfig( _currentDayTemporaryDirectory, incrementRunAttempts: false, reportsCompleted: true );

            UpdateProgress( "Cleaning up temporary files.", 0, 0 );

            // remove temp files (including ones from opted out)
            foreach ( var pdfTempFilePath in recipientList.Select( a => a.GetPdfDocumentFilePath( _currentDayTemporaryDirectory ) ) )
            {
                if ( File.Exists( pdfTempFilePath ) )
                {
                    try
                    {
                        File.Delete( pdfTempFilePath );
                    }
                    catch
                    {
                        // OK if it can't be deleted
                    }
                }
            }

            UpdateProgress( "Complete", 0, 0 );

            _stopwatchAll.Stop();
            var elapsedSeconds = _stopwatchAll.ElapsedMilliseconds / 1000;
            Debug.WriteLine( $"{elapsedSeconds:n0} seconds" );
            Debug.WriteLine( $"{resultsSummary.StatementCount:n0} statements" );
            if ( resultsSummary.StatementCount > 0 )
            {
                Debug.WriteLine( $"{( _stopwatchAll.ElapsedMilliseconds / resultsSummary.StatementCount ):n0}ms per statement" );
            }

            return resultsSummary;
        }

        private string GetStatementGeneratorLocalApplicationDataFolder()
        {
            // have the chrome rendering engine download and run in AppData\Local\Spark_Development_Network\StatementGenerator\.local-chromium
            var statementGeneratorUserDataFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "Spark_Development_Network", "StatementGenerator" );
            var browserDownloadPath = Path.Combine( statementGeneratorUserDataFolder, ".local-chromium" );
            if ( !Directory.Exists( browserDownloadPath ) )
            {
                Directory.CreateDirectory( browserDownloadPath );
            }
            return browserDownloadPath;
        }

        /// <summary>
        /// Initializes the chrome engine.
        /// </summary>
        private void InitializeChromeEngine()
        {
            var browserDownloadPath = GetStatementGeneratorLocalApplicationDataFolder();

            var browserFetcherOptions = new BrowserFetcherOptions
            {
                Product = Product.Chrome,
                Path = browserDownloadPath,
            };

            var browserFetcher = new BrowserFetcher( browserFetcherOptions );
            browserFetcher.DownloadProgressChanged += BrowserFetcher_DownloadProgressChanged;
            browserFetcher.DownloadAsync().Wait();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions { Width = 1280, Height = 1024, DeviceScaleFactor = 1 },
                ExecutablePath = browserFetcher.RevisionInfo().ExecutablePath
            };

            browser = Puppeteer.LaunchAsync( launchOptions ).Result;

            // 
            try
            {
                // just in case the Statement Generator didn't close cleanly previously,
                // Kill any chrome.exe's that got left running in
                // AppData\Local\Spark_Development_Network\StatementGenerator\.local-chromium\Win64-848005\chrome-win
                var browserProcessModule = browser.Process.MainModule;
                var allProcesses = Process.GetProcesses();
                foreach ( var process in allProcesses.Where( a => a.ProcessName == browser.Process.ProcessName ) )
                {
                    if ( process.Id != browser.Process.Id && ( process.MainModule.FileName == browserProcessModule.FileName ) )
                    {
                        process.Kill();
                    }
                }
            }
            catch ( Exception ex )
            {
                WriteToLog( $"INFO: Unable to cleanup orphaned chrome.exe processes, {ex}" );
            }
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteToLog( string message )
        {
            try
            {
                Debug.WriteLine( $"\n{message}" );
                var exceptionFilePath = Path.Combine( GetStatementGeneratorLocalApplicationDataFolder(), "Exceptions.Log" );
                File.AppendAllText( exceptionFilePath, $"{DateTime.Now} {message}" );
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Writes to exception log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        private void WriteToExceptionLog( string message, Exception ex )
        {
            WriteToLog( $"\n{message}\n{ex}" );
            try
            {
                App.LogException( ex, message );
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Handles the DownloadProgressChanged event of the BrowserFetcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Net.DownloadProgressChangedEventArgs"/> instance containing the event data.</param>
        private void BrowserFetcher_DownloadProgressChanged( object sender, System.Net.DownloadProgressChangedEventArgs e )
        {
            UpdateProgress( "Downloading Rendering Engine", e.ProgressPercentage, 100, true );
        }

        /// <summary>
        /// Gets the financial statement template.
        /// </summary>
        /// <param name="rockConfig">The rock configuration.</param>
        /// <param name="restClient">The rest client.</param>
        /// <returns></returns>
        private FinancialStatementTemplate GetFinancialStatementTemplate( RockConfig rockConfig, RestClient restClient )
        {
            if ( !this.Options.FinancialStatementTemplateId.HasValue )
            {
                var getFinancialStatementTemplateIdRequest = new RestRequest( $"api/FinancialStatementTemplates?$filter=Guid eq guid'{rockConfig.FinancialStatementTemplateGuid}'" );
                this.Options.FinancialStatementTemplateId = restClient.Execute<List<FinancialStatementTemplate>>( getFinancialStatementTemplateIdRequest ).Data.FirstOrDefault()?.Id;
            }

            var getFinancialStatementTemplatesRequest = new RestRequest( $"api/FinancialStatementTemplates/{this.Options.FinancialStatementTemplateId ?? 0}" );
            var getFinancialStatementTemplatesResponse = restClient.Execute<Client.FinancialStatementTemplate>( getFinancialStatementTemplatesRequest );

            if ( getFinancialStatementTemplatesResponse.ErrorException != null )
            {
                throw getFinancialStatementTemplatesResponse.ErrorException;
            }

            Rock.Client.FinancialStatementTemplate financialStatementTemplate = getFinancialStatementTemplatesResponse.Data;
            if ( !this.Options.FinancialStatementTemplateId.HasValue )
            {
                this.Options.FinancialStatementTemplateId = financialStatementTemplate.Id;
            }

            return financialStatementTemplate;
        }

        /// <summary>
        /// Starts the generate statement for recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="restClient">The rest client.</param>
        private void StartGenerateStatementForRecipient( FinancialStatementGeneratorRecipient recipient, RestClient restClient, bool enablePageCountPredetermination )
        {
            const int AssumedRenderedPageCount = 1;

            // Make an assumption that the RenderedPageCount is one.
            // The actual rendered page count will be updated after the Render is complete.
            recipient.RenderedPageCount = AssumedRenderedPageCount;
            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = GetFinancialStatementGeneratorRecipientResult( restClient, recipient );

            recipient.OptedOut = financialStatementGeneratorRecipientResult.OptedOut;
            recipient.ContributionTotal = financialStatementGeneratorRecipientResult.ContributionTotal;

            if ( string.IsNullOrWhiteSpace( financialStatementGeneratorRecipientResult.Html ) )
            {
                // don't generate a statement if no statement HTML
                return;
            }

            var incompleteTasks = _renderPdfTasks.Where( a => !a.IsCompleted ).ToArray();
            while ( incompleteTasks.Count() > _maxRenderThreads )
            {
                WriteToLog( $"incompleteTasks.Count():{incompleteTasks.Count()}" );
                Thread.Sleep( 100 );
                Task.WaitAny( incompleteTasks, 1000 );
                incompleteTasks = _renderPdfTasks.Where( a => !a.IsCompleted ).ToArray();
            }

            var renderPDFFromHtmlTask = new Task( () =>
            {
                Stopwatch generatePdfStopWatch = Stopwatch.StartNew();
                var pdfDocument = RenderPDFDocument( financialStatementGeneratorRecipientResult );
                recipient.RenderedPageCount = pdfDocument.PageCount;
                generatePdfStopWatch.Stop();

                // we don't need to do the 2nd pass if the 1st pass's rendered page count matches what we assumed it would be.l 
                if ( enablePageCountPredetermination && AssumedRenderedPageCount != recipient.RenderedPageCount )
                {
                    financialStatementGeneratorRecipientResult = GetFinancialStatementGeneratorRecipientResult( restClient, recipient );
                    generatePdfStopWatch.Start();
                    pdfDocument = RenderPDFDocument( financialStatementGeneratorRecipientResult );
                    recipient.RenderedPageCount = pdfDocument.PageCount;
                    generatePdfStopWatch.Stop();
                }

                var generatePdfStopWatchElapsedMS = generatePdfStopWatch.Elapsed.TotalMilliseconds;

                var recordsCompleted = Interlocked.Increment( ref _recordsCompleted );

                // launch a task to save and upload the document
                // This is thread safe, so we can spin these up as needed 
                var saveAndUploadTask = new Task( () =>
                {
                    Stopwatch savePdfStopWatch = Stopwatch.StartNew();

                    var pdfTempFilePath = recipient.GetPdfDocumentFilePath( _currentDayTemporaryDirectory );
                    pdfDocument.Save( pdfTempFilePath );

                    if ( _saveStatementsForIndividualsToDocument )
                    {
                        // if there is an exception, we'll want to know that the statement wasn't uploaded
                        recipient.PaperlessStatementUploaded = false;

                        FinancialStatementGeneratorUploadGivingStatementData uploadGivingStatementData = new FinancialStatementGeneratorUploadGivingStatementData
                        {
                            FinancialStatementGeneratorRecipient = recipient,
                            FinancialStatementIndividualSaveOptions = _individualSaveOptions,
                            PDFData = File.ReadAllBytes( pdfTempFilePath )
                        };

                        RestRequest uploadDocumentRequest = new RestRequest( "api/FinancialGivingStatement/UploadGivingStatementDocument" );
                        uploadDocumentRequest.AddJsonBody( uploadGivingStatementData );

                        IRestResponse<FinancialStatementGeneratorUploadGivingStatementResult> uploadDocumentResponse = _uploadPdfDocumentRestClient.ExecutePostAsync<FinancialStatementGeneratorUploadGivingStatementResult>( uploadDocumentRequest ).Result;
                        if ( uploadDocumentResponse.ErrorException != null )
                        {
                            throw uploadDocumentResponse.ErrorException;
                        }

                        recipient.PaperlessStatementUploaded = uploadDocumentResponse.Data != null;
                        recipient.PaperlessStatementsIndividualCount = uploadDocumentResponse.Data?.NumberOfIndividuals;
                    }

                    recipient.IsComplete = true;
                    if ( recordsCompleted > 2 && Debugger.IsAttached )
                    {
                        _saveAndUploadPdfTimingsMS.Add( savePdfStopWatch.Elapsed.TotalMilliseconds );
                    }
                } );

                _saveAndUploadTasks.Add( saveAndUploadTask );
                saveAndUploadTask.Start();

                if ( recordsCompleted > 2 && Debugger.IsAttached && recordsCompleted % 10 == 0 )
                {
                    _generatePdfTimingsMS.Add( generatePdfStopWatchElapsedMS );

                    var averageGetStatementHtmlTimingsMS = _getStatementHtmlTimingsMS.Any() ? Math.Round( _getStatementHtmlTimingsMS.Average(), 0 ) : 0;
                    var averageGeneratePDFTimingMS = Math.Round( _generatePdfTimingsMS.Average(), 0 );
                    var averageSaveAndUploadPDFTimingMS = _saveAndUploadPdfTimingsMS.Any() ?
                        Math.Round( _saveAndUploadPdfTimingsMS.Average(), 0 )
                        : ( double? ) null;

                    var overallMSPerPDF = Math.Round( _stopwatchRenderPDFsOverall.Elapsed.TotalMilliseconds / recordsCompleted, 2 );
                    var overallPDFPerSecond = Math.Round( recordsCompleted / _stopwatchRenderPDFsOverall.Elapsed.TotalSeconds, 2 );

                    Debug.WriteLine( $@"
RenderPDF          Avg: {averageGeneratePDFTimingMS} ms
GetStatementHtml   Avg: {averageGetStatementHtmlTimingsMS} ms
Save/Upload  PDF   Avg: {averageSaveAndUploadPDFTimingMS} ms
Total PDFs Elapsed    : {Math.Round( _stopwatchRenderPDFsOverall.Elapsed.TotalSeconds, 2 )} seconds 
Records Completed     : {recordsCompleted}
Overall ms/PDF     Avg: {overallMSPerPDF} ms
Overall PDF/sec    Avg: {overallPDFPerSecond }/sec
" );
                }
            } );

            _renderPdfTasks.Add( renderPDFFromHtmlTask );
            renderPDFFromHtmlTask.ContinueWith( ( t ) =>
            {
                if ( t.Exception != null )
                {
                    throw t.Exception;
                }
            } );
            renderPDFFromHtmlTask.Start();
        }

        /// <summary>
        /// Handles the UnobservedTaskException event of the TaskScheduler control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void TaskScheduler_UnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
        {
            e.SetObserved();
            WriteToExceptionLog( "UnobservedTaskException", e.Exception );
        }

        /// <summary>
        /// Renders the PDF document.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientResult">The financial statement generator recipient result.</param>
        /// <returns></returns>
        private PdfDocument RenderPDFDocument( FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult )
        {
            var html = financialStatementGeneratorRecipientResult.Html;

            Page page;
            availablePagesCache.TryPop( out page );
            if ( page == null )
            {
                page = browser.NewPageAsync().Result;
                page.EmulateMediaTypeAsync( PuppeteerSharp.Media.MediaType.Screen ).Wait();
                WriteToLog( "INFO: Adding additional Page process" );
            }

            page.SetContentAsync( html ).Wait();

            MarginOptions marginOptions = new MarginOptions
            {
                Bottom = $"{_reportSettings.PDFSettings.MarginBottomMillimeters ?? 15}mm",
                Left = $"{_reportSettings.PDFSettings.MarginLeftMillimeters ?? 10}mm",
                Top = $"{_reportSettings.PDFSettings.MarginTopMillimeters ?? 10}mm",
                Right = $"{_reportSettings.PDFSettings.MarginRightMillimeters ?? 10}mm",
            };

            var pdfOptions = new PdfOptions();
            pdfOptions.MarginOptions = marginOptions;
            pdfOptions.PrintBackground = true;
            pdfOptions.DisplayHeaderFooter = true;
            pdfOptions.FooterTemplate = financialStatementGeneratorRecipientResult.FooterHtmlFragment;

            // set HeaderTemplate to something so that it doesn't end up using the default
            pdfOptions.HeaderTemplate = "<!-- -->";

            switch ( _reportSettings.PDFSettings.PaperSize )
            {
                case FinancialStatementTemplatePDFSettingsPaperSize.A4:
                    pdfOptions.Format = PaperFormat.A4;
                    break;
                case FinancialStatementTemplatePDFSettingsPaperSize.Legal:
                    pdfOptions.Format = PaperFormat.Legal;
                    break;
                case FinancialStatementTemplatePDFSettingsPaperSize.Letter:
                default:
                    pdfOptions.Format = PaperFormat.Letter;
                    break;
            }

            var pdfStream = page.PdfStreamAsync( pdfOptions ).Result;

            availablePagesCache.Push( page );

            var pdfDoc = PdfReader.Open( pdfStream, PdfDocumentOpenMode.Import );
            return pdfDoc;
        }

        // From https://stackoverflow.com/a/41262413/1755417
        // Do this to prevent corrupting the RecipientData.Json file
        private ReaderWriterLockSlim recipientDataJsonFileLocker = new ReaderWriterLockSlim();

        private DateTime _lastSaveRecipientListStatus = DateTime.MinValue;
        private const int LimitedUpdateSaveRecipientListStatusSeconds = 2;

        /// <summary>
        /// Saves the recipient list status.
        /// </summary>
        /// <param name="recipientList">The recipient list.</param>
        /// <param name="reportRockStatementGeneratorTemporaryDirectory">The report rock statement generator temporary directory.</param>
        private void SaveRecipientListStatus( List<FinancialStatementGeneratorRecipient> recipientList, string reportRockStatementGeneratorTemporaryDirectory, bool limitUpdates )
        {
            if ( limitUpdates )
            {
                // if we are updating this every time a recipient is processed, it could be writing
                // a new file 10+ times a second. If it is a big list of recipients
                // each ToJson/write can take 100ms or more.
                // To avoid too much overhead,we can safely just update the file every 2 seconds.
                // So, if the process it interrupted it'll only be at most 2 seconds behind. I'll just
                // end up re-doing 2 seconds worth of recipients. 
                var timeSinceLastUpdate = DateTime.Now - _lastSaveRecipientListStatus;

                if ( timeSinceLastUpdate.TotalSeconds < LimitedUpdateSaveRecipientListStatusSeconds )
                {
                    return;
                }

                _lastSaveRecipientListStatus = DateTime.Now;
            }

            // if still writing (very rare), just skip. It is OK if it is a little behind
            if ( recipientDataJsonFileLocker.WaitingWriteCount > 0 )
            {
                WriteToLog( $"recipientDataJsonFileLocker.WaitingWriteCount: {recipientDataJsonFileLocker.WaitingWriteCount}" );
                return;
            }

            try
            {
                recipientDataJsonFileLocker.EnterWriteLock();
                WriteRecipientListToFile( recipientList, reportRockStatementGeneratorTemporaryDirectory );
            }
            finally
            {
                recipientDataJsonFileLocker.ExitWriteLock();
            }
        }

        private static void WriteRecipientListToFile( List<FinancialStatementGeneratorRecipient> recipientList, string reportRockStatementGeneratorTemporaryDirectory )
        {
            var recipientListJsonFileName = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "RecipientData.Json" );
            recipientList.ToJsonFile( Formatting.None, recipientListJsonFileName, true );
        }

        /// <summary>
        /// Gets the recipient list status.
        /// </summary>
        /// <returns></returns>
        public static List<FinancialStatementGeneratorRecipient> GetSavedRecipientList( DateTime runDate )
        {
            var rockConfig = RockConfig.Load();

            var fileName = Path.Combine( GetStatementGeneratorTemporaryDirectory( rockConfig, runDate ), "RecipientData.Json" );
            if ( File.Exists( fileName ) )
            {
                string resultsJson = File.ReadAllText( fileName );
                return resultsJson.FromJsonOrNull<List<FinancialStatementGeneratorRecipient>>();
            }

            return null;
        }

        /// <summary>
        /// If there are some incomplete recipients, it verifies that the completed ones are really completed.
        /// </summary>
        public static void EnsureIncompletedSavedRecipientListCompletedStatus( DateTime runDate )
        {
            var rockStatementGeneratorTemporaryDirectory = GetStatementGeneratorTemporaryDirectory( RockConfig.Load(), runDate );
            var savedRecipientList = GetSavedRecipientList( runDate );
            if ( savedRecipientList == null )
            {
                // hasn't run
                return;
            }

            // if there are some that are not complete, make sure the temp files haven't been cleaned up
            foreach ( var savedRecipient in savedRecipientList.Where( a => a.IsComplete ) )
            {
                if ( savedRecipient.PdfFileExists( rockStatementGeneratorTemporaryDirectory ) == false )
                {
                    // if it was marked complete, but the temp file is gone, we'll have to re-do this recipient
                    savedRecipient.IsComplete = false;
                    continue;
                }
            }

            if ( savedRecipientList.Any( a => !a.IsComplete ) == false )
            {
                // if the whole thing is completed, then everything is all done
                return;
            }

            WriteRecipientListToFile( savedRecipientList.ToList(), rockStatementGeneratorTemporaryDirectory );
        }

        /// <summary>
        /// Saves the generator configuration.
        /// </summary>
        /// <param name="runDate">The run date.</param>
        /// <param name="incrementRunAttempts">if set to <c>true</c> [increment run attempts].</param>
        private void SaveGeneratorConfig( string currentDayTemporaryDirectory, bool incrementRunAttempts, bool reportsCompleted )
        {
            GeneratorConfig generatorConfig = null;

            var fileName = Path.Combine( currentDayTemporaryDirectory, "GeneratorConfig.Json" );
            if ( File.Exists( fileName ) )
            {
                var resultsJson = File.ReadAllText( fileName );
                generatorConfig = resultsJson.FromJsonOrNull<GeneratorConfig>();
            }

            if ( generatorConfig == null )
            {
                generatorConfig = new GeneratorConfig { RunAttempts = 1 };
                generatorConfig.RunDate = DateTime.Today;
            }
            else if ( incrementRunAttempts )
            {
                generatorConfig.RunAttempts++;
            }

            generatorConfig.ReportsCompleted = reportsCompleted;

            generatorConfig.ConfiguredOptions = this.Options;

            generatorConfig.ToJsonFile( Formatting.Indented, fileName, false );
        }

        /// <summary>
        /// Gets the last saved generator configuration. Returns null if there wasn't one.
        /// </summary>
        /// <returns></returns>
        public static GeneratorConfig GetSavedGeneratorConfigFromLastRun()
        {
            var rockConfig = RockConfig.Load();

            var reportTemporaryDirectoryPath = rockConfig.TemporaryDirectory;
            if ( reportTemporaryDirectoryPath.IsNotNullOrWhiteSpace() )
            {
                Directory.CreateDirectory( reportTemporaryDirectoryPath );
            }
            else
            {
                reportTemporaryDirectoryPath = Path.GetTempPath();
            }

            List<string> previousRunFiles = new List<string>();
            var folders = Directory.EnumerateDirectories( reportTemporaryDirectoryPath, "Rock Statement Generator-*", SearchOption.TopDirectoryOnly );
            foreach ( var folder in folders )
            {
                Directory.EnumerateFiles( folder, "GeneratorConfig.Json" );

                previousRunFiles.AddRange( Directory.EnumerateFiles( folder, "GeneratorConfig.Json", SearchOption.TopDirectoryOnly ).ToList() );
            }

            var generatorConfigs = previousRunFiles.Select( a => File.ReadAllText( a ).FromJsonOrNull<GeneratorConfig>() ).ToList();
            var mostRecentGeneratorConfig = generatorConfigs.Where( a => a != null ).OrderByDescending( g => g.RunDate ).FirstOrDefault();

            return mostRecentGeneratorConfig;
        }

        /// <summary>
        /// Gets the financial statement generator recipient result.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        private FinancialStatementGeneratorRecipientResult GetFinancialStatementGeneratorRecipientResult( RestClient restClient, FinancialStatementGeneratorRecipient recipient )
        {
            FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest()
            {
                FinancialStatementGeneratorOptions = this.Options,
                FinancialStatementGeneratorRecipient = recipient
            };

            var financialStatementGeneratorRecipientResultRequest = new RestRequest( "api/FinancialGivingStatement/GetStatementGeneratorRecipientResult", Method.POST );
            financialStatementGeneratorRecipientResultRequest.AddJsonBody( financialStatementGeneratorRecipientRequest );

            Stopwatch getStatementHtml = Stopwatch.StartNew();

            var financialStatementGeneratorRecipientResultResponse = restClient.Execute<Client.FinancialStatementGeneratorRecipientResult>( financialStatementGeneratorRecipientResultRequest );
            if ( financialStatementGeneratorRecipientResultResponse.ErrorException != null )
            {
                throw financialStatementGeneratorRecipientResultResponse.ErrorException;
            }

            getStatementHtml.Stop();

            _getStatementHtmlTimingsMS.Add( getStatementHtml.Elapsed.TotalMilliseconds );

            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = financialStatementGeneratorRecipientResultResponse.Data;
            return financialStatementGeneratorRecipientResult;
        }

        /// <summary>
        /// Gets the rock statement generator temporary directory.
        /// </summary>
        /// <param name="rockConfig">The rock configuration.</param>
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
        private static string GetStatementGeneratorTemporaryDirectory( RockConfig rockConfig, DateTime runDate )
        {
            var reportTemporaryDirectory = rockConfig.TemporaryDirectory;
            if ( reportTemporaryDirectory.IsNotNullOrWhiteSpace() )
            {
                Directory.CreateDirectory( reportTemporaryDirectory );
            }
            else
            {
                reportTemporaryDirectory = Path.GetTempPath();
            }

            var reportRockStatementGeneratorTemporaryDirectory = Path.Combine( reportTemporaryDirectory, $"Rock Statement Generator-{runDate.ToString( "MM_dd_yyyy" )}" );
            Directory.CreateDirectory( reportRockStatementGeneratorTemporaryDirectory );
            return reportRockStatementGeneratorTemporaryDirectory;
        }

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <returns></returns>
        private List<Client.FinancialStatementGeneratorRecipient> GetRecipients( RestClient restClient )
        {
            var financialStatementGeneratorRecipientsRequest = new RestRequest( "api/FinancialGivingStatement/GetFinancialStatementGeneratorRecipients", Method.POST );
            financialStatementGeneratorRecipientsRequest.AddJsonBody( this.Options );
            var financialStatementGeneratorRecipientsResponse = restClient.Execute<List<Client.FinancialStatementGeneratorRecipient>>( financialStatementGeneratorRecipientsRequest );
            if ( financialStatementGeneratorRecipientsResponse.ErrorException != null )
            {
                throw financialStatementGeneratorRecipientsResponse.ErrorException;
            }

            return financialStatementGeneratorRecipientsResponse.Data;
        }

        /// <summary>
        /// Writes the group of statements to document.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="financialStatementGeneratorRecipientResults">The statement generator recipient PDF results.</param>
        private ReportPaperStatementsSummary WriteStatementPDFs( FinancialStatementReportConfiguration financialStatementReportConfiguration, IEnumerable<FinancialStatementGeneratorRecipient> financialStatementGeneratorRecipientResults )
        {

            var recipientList = financialStatementGeneratorRecipientResults.Where( a => a.IsComplete ).ToList();
            ReportPaperStatementsSummary reportPaperStatementsSummary = new ReportPaperStatementsSummary( recipientList, financialStatementReportConfiguration );

            if ( !financialStatementGeneratorRecipientResults.Any() )
            {
                return reportPaperStatementsSummary;
            }

            if ( financialStatementReportConfiguration.ExcludeOptedOutIndividuals )
            {
                recipientList = recipientList.Where( a => a.OptedOut == false ).ToList();
            }

            if ( financialStatementReportConfiguration.MinimumContributionAmount.HasValue )
            {
                recipientList = recipientList.Where( a => a.ContributionTotal >= financialStatementReportConfiguration.MinimumContributionAmount.Value ).ToList();
            }

            if ( financialStatementReportConfiguration.IncludeInternationalAddresses == false )
            {
            
                recipientList = recipientList.Where( a => a.IsInternationalAddress == false ).ToList();
            }

            if ( financialStatementReportConfiguration.ExcludeRecipientsThatHaveAnIncompleteAddress )
            {
                recipientList = recipientList.Where( a => a.HasValidMailingAddress == true ).ToList();
            }

            IOrderedEnumerable<FinancialStatementGeneratorRecipient> sortedRecipientList = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, recipientList );
            recipientList = sortedRecipientList.ToList();

            // make sure the directory exists
            Directory.CreateDirectory( financialStatementReportConfiguration.DestinationFolder );

            /* Splitting Logic
             * 
             * Max/Chapter | Split On Primary  | PreventSplitting | Result (some cases end up with same output)
             * null        | false             | false            | Single PDF - no splitting or max chapters
             * null        | false             | true             | Single PDF - no splitting or max chapters (prevent splitting doesn't matter since we aren't splitting by anything)
             *
             * 1000        | false             | false            | 1000 per PDF, last doc might have less than 1000 - Simple
             * 
             * null        | true              | false            | One per Sort (for example, each zip code has its own doc) - simple case (PreventSplitting doesn't matter since we are already splitting)
             * null        | true              | true             | One per Sort (for example, each zip code has its own doc) - simple case (PreventSplitting doesn't matter since we are already splitting)
             * 1000        | true              | true             | One per Sort (for example, each zip code has its own doc) - max is ignored since we are have to prevent splitting
             *             
             * 1000        | true              | false            | 1000 or less per PDF. Examples per PDF
             *                                                             999 85083 = one doc
             *                                                             900 85083 + 100 85444 = two docs (we are splitting on primary)
             *                                                            1001 85083 = 2 docs. 85083 has two docs" 85083-chapter1 (1000 statements). 85083-chapter2 (1 statement)
             * 
             * 1000        | false             | true             | Up to 1000 (don't go over if next has too many). For example:
             *                                                             999 85083 = one doc, 
             *                                                             900 85123 + 100 85444 = one doc (they fit)
             *                                                             900 85123 + 101 85444 = 2 docs (2nd doc could hold additional zip codes)
             *                                                            1001 85083 = that doc has more than max since splitting is not allowed
             */

            int? maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter;
            bool splitOnPrimary = financialStatementReportConfiguration.SplitFilesOnPrimarySortValue;
            bool preventSplitting = financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters;

            if ( maxStatementsPerChapter == null && !splitOnPrimary )
            {
                // Single PDF case
                // No splitting or chaptering, just one giant doc
                var singleFileName = Path.Combine( financialStatementReportConfiguration.DestinationFolder, $"{financialStatementReportConfiguration.FilenamePrefix}statements.pdf" );
                ProgressPage.ShowSaveMergeDocProgress( 0, 1, "Saving Merged Document" );
                SaveToMergedDocument( singleFileName, recipientList );
                ProgressPage.ShowSaveMergeDocProgress( 1, 1, "Saving Merged Document" );
                return reportPaperStatementsSummary;
            }

            if ( maxStatementsPerChapter.HasValue && !preventSplitting && !splitOnPrimary )
            {
                // 1000 per PDF case (last doc might have less than 1000) - Simple
                // simply break into maxStatementsPerChapter, where the last doc may have less than maxStatementsPerChapter
                SaveAsChapterDocs( financialStatementReportConfiguration, recipientList, maxStatementsPerChapter.Value );
                return reportPaperStatementsSummary;
            }

            if ( splitOnPrimary )
            {
                if ( preventSplitting || maxStatementsPerChapter == null )
                {
                    // One per Sort case ( Split on primary and also prevent splitting (or there isn't a max per chapter) )
                    // This ends up as simple one chapter per Sort (for example, Each Zip Code has it's own file)
                    SaveAsSimpleSplitOnPrimarySort( financialStatementReportConfiguration, recipientList );
                    return reportPaperStatementsSummary;
                }
                else
                {
                    // 1000 or Less per PDF case (splitting is allowed, but strictly no more than max )
                    //   - If there are more than 1000 for a split, it can be put into more than one doc.
                    //   - However, don't have more than one sort key per doc.
                    //   - Therefore, each doc would never have more then the max, but could have fewer than the max
                    SaveAsSplitOnPrimarySortWithChapters( financialStatementReportConfiguration, recipientList, maxStatementsPerChapter.Value );
                    return reportPaperStatementsSummary;
                }
            }

            // final case
            //   - If there are more than 1000 for a split, keep it together.
            //   - Otherwise, limit to an absolute max (a doc may have more than one sort key)
            //   - Therefore, each doc could have more than the max or less than the max
            SaveAsChapterDocsPreventSplitting( financialStatementReportConfiguration, recipientList, maxStatementsPerChapter.Value );
            return reportPaperStatementsSummary;
        }

        /// <summary>
        /// Saves as split on primary sort with chapters.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <param name="maxStatementsPerChapter">The maximum statements per chapter.</param>
        private void SaveAsSplitOnPrimarySortWithChapters( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList, int maxStatementsPerChapter )
        {
            var recipientsByPrimarySortKey = GetRecipientsByPrimarySortKey( financialStatementReportConfiguration, recipientList );

            foreach ( var primarySort in recipientsByPrimarySortKey )
            {
                List<FinancialStatementGeneratorRecipient> recipientsForSort = primarySort.Value;
                SaveAsChapterDocs( financialStatementReportConfiguration, recipientsForSort, maxStatementsPerChapter, primarySort.Key );
            }
        }

        /// <summary>
        /// Saves as chapter docs prevent splitting.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <param name="maxStatementsPerChapter">The maximum statements per chapter.</param>
        private void SaveAsChapterDocsPreventSplitting( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList, int maxStatementsPerChapter )
        {
            /*
             * Up to maxStatementsPerChapter (don't go over if next has too many). For example:
             *  
             *  999 85083 = one doc,                                                          
             *  900 85123 + 100 85444 = one doc (they fit)                                                         
             *  900 85123 + 101 85444 = 2 docs (2nd doc could hold additional zip codes)                                                         
             * 1001 85083 = that doc has more than max since splitting is not allowed                                                         
             */

            List<FinancialStatementGeneratorRecipient> recipientsForChapter = new List<FinancialStatementGeneratorRecipient>();

            var recipientsByPrimarySortKey = GetRecipientsByPrimarySortKey( financialStatementReportConfiguration, recipientList );
            var sortKeys = recipientsByPrimarySortKey.Keys.ToArray();
            var recipientSortIndex = 0;
            var recipientSortMaxIndex = recipientsByPrimarySortKey.Count() - 1;
            var startSortKey = recipientsByPrimarySortKey.Keys.FirstOrDefault();
            var currentChapterNumber = 1;

            while ( recipientSortIndex <= recipientSortMaxIndex )
            {
                ProgressPage.ShowSaveMergeDocProgress( recipientSortIndex, recipientSortMaxIndex, "Saving Chapter Docs" );
                var currentSortKey = sortKeys[recipientSortIndex];
                int? nextSortCount = null;
                int nextSortIndex = recipientSortIndex + 1;
                if ( nextSortIndex <= recipientSortMaxIndex )
                {
                    nextSortCount = recipientsByPrimarySortKey[sortKeys[nextSortIndex]].Count;
                }

                recipientsForChapter.AddRange( recipientsByPrimarySortKey[currentSortKey] );

                // save and start new doc if
                //   -- we are already over,
                //   -- we will be over if we try to add the next set
                //   -- this is the last sort key (nextSortCount is null)
                if ( !nextSortCount.HasValue || ( recipientsForChapter.Count() + nextSortCount ) > maxStatementsPerChapter )
                {
                    var chapterFileName = $"{financialStatementReportConfiguration.FilenamePrefix}{startSortKey}_{currentSortKey}_chapter{currentChapterNumber}.pdf".MakeValidFileName();
                    SaveToMergedDocument( Path.Combine( financialStatementReportConfiguration.DestinationFolder, chapterFileName ), recipientsForChapter );
                    recipientsForChapter = new List<FinancialStatementGeneratorRecipient>();
                }

                recipientSortIndex++;
            }
        }

        /// <summary>
        /// Saves as chapter docs.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <param name="maxStatementsPerChapter">The maximum statements per chapter.</param>
        private void SaveAsChapterDocs( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList, int maxStatementsPerChapter, string chapterSplitName = null )
        {
            var skipCount = 0;
            IEnumerable<FinancialStatementGeneratorRecipient> chapterStatements;
            int chapterNumber = 1;
            int chapterCountEstimate = recipientList.Count() / maxStatementsPerChapter;

            // sort, just in case it isn't sorted already
            var sortedRecipients = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, recipientList );

            do
            {
                chapterStatements = sortedRecipients.Skip( skipCount ).Take( maxStatementsPerChapter );
                if ( !chapterStatements.Any() )
                {
                    break;
                }

                var chapterFileName = $"{financialStatementReportConfiguration.FilenamePrefix}{chapterSplitName}{chapterNumber}.pdf".MakeValidFileName();
                ProgressPage.ShowSaveMergeDocProgress( chapterNumber, chapterCountEstimate, "Saving Chapter Docs" );

                SaveToMergedDocument( Path.Combine( financialStatementReportConfiguration.DestinationFolder, chapterFileName ), chapterStatements.ToList() );

                chapterNumber++;
                skipCount += maxStatementsPerChapter;
            } while ( chapterStatements.Any() );
        }

        private void SaveAsSimpleSplitOnPrimarySort( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList )
        {
            var recipientsByPrimarySortKey = GetRecipientsByPrimarySortKey( financialStatementReportConfiguration, recipientList );

            var progressMax = recipientsByPrimarySortKey.Count;
            var progressPosition = 0;

            foreach ( var primarySort in recipientsByPrimarySortKey )
            {
                string primarySortFileName = $"{financialStatementReportConfiguration.FilenamePrefix}{primarySort.Key}.pdf".MakeValidFileName();
                string mergeDocFilePath = Path.Combine( financialStatementReportConfiguration.DestinationFolder, primarySortFileName );

                var sortedRecipients = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, primarySort.Value );
                SaveToMergedDocument( mergeDocFilePath, sortedRecipients.ToList() );

                progressPosition++;

                ProgressPage.ShowSaveMergeDocProgress( progressPosition, progressMax, "Saving Docs" );
            }
        }

        private Dictionary<string, List<FinancialStatementGeneratorRecipient>> GetRecipientsByPrimarySortKey( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList )
        {
            Dictionary<string, List<FinancialStatementGeneratorRecipient>> recipientsByPrimarySortKey;
            if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.PageCount )
            {
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.RenderedPageCount.ToString() )
                    .ToDictionary( k => k.Key.ToString(), v => v.ToList() );
            }
            else if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.LastName )
            {
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.LastName )
                    .ToDictionary( k => k.Key, v => v.ToList() );
            }
            else
            {
                // group by postal code
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.GetFiveDigitPostalCode() )
                    .ToDictionary( k => k.Key, v => v.ToList() );
            }

            return recipientsByPrimarySortKey;
        }

        /// <summary>
        /// Saves to merged document.
        /// </summary>
        /// <param name="mergedFileName">Name of the merged file.</param>
        /// <param name="recipientList">The recipient list.</param>
        private void SaveToMergedDocument( string mergedFileName, List<FinancialStatementGeneratorRecipient> recipientList )
        {
            var allPdfsEnumerable = recipientList.Select( a =>
            {
                return a.GetPdfDocument( _currentDayTemporaryDirectory );
            } );

            var singleFinalDoc = new PdfDocument();
            foreach ( var doc in allPdfsEnumerable )
            {
                foreach ( var page in doc.Pages )
                {
                    singleFinalDoc.Pages.Add( page );
                }
            }

            singleFinalDoc.Save( mergedFileName );
        }

        /// <summary>
        /// Sorts the by primary and secondary order.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <returns></returns>
        private IOrderedEnumerable<FinancialStatementGeneratorRecipient> SortByPrimaryAndSecondaryOrder( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipient> recipientList )
        {
            IOrderedEnumerable<FinancialStatementGeneratorRecipient> sortedRecipientList;

            switch ( financialStatementReportConfiguration.PrimarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.RenderedPageCount );
                        break;
                    }

                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.PostalCode );
                        break;
                    }

                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
                        break;
                    }
            }

            switch ( financialStatementReportConfiguration.SecondarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.RenderedPageCount );
                        break;
                    }

                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.PostalCode );
                        break;
                    }

                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.LastName ).ThenBy( a => a.NickName );
                        break;
                    }
            }

            return sortedRecipientList;
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressMessage">The message.</param>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        private void UpdateProgress( string progressMessage, long position, int max, bool limitUpdates = false )
        {
            ProgressPage.ShowProgress( ( int ) position, max, progressMessage, limitUpdates );
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if ( availablePagesCache != null )
                {
                    foreach ( var p in availablePagesCache )
                    {
                        p.CloseAsync().Wait();
                        p.Dispose();
                    }
                }

                if ( browser != null )
                {
                    browser.CloseAsync().Wait();
                    browser.Dispose();
                }

                browser?.CloseAsync().Wait();

                recipientDataJsonFileLocker?.Dispose();
            }
            catch ( Exception ex )
            {
                WriteToExceptionLog( "INFO: Errors during Dispose", ex );
            }
        }
    }
}
