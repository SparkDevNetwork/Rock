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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using PuppeteerSharp;
using PuppeteerSharp.Media;

using Rock.Observability;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Pdf
{
    /// <summary>
    /// Class PDFGenerator.
    /// Note that if using <see cref="SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT"/>, the usage time is based
    /// on how long this PdfGenerator object is used. So create/dispose quickly.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class PdfGenerator : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerator"/> class.
        /// </summary>
        public PdfGenerator()
        {
            InitializeChromeEngine();
        }

        private IBrowser _puppeteerBrowser = null;
        private IPage _puppeteerPage;
        private static readonly string _browserVersion = "133.0.6943.141";
        private static int _lastProgressPercentage = 0;

        /// <summary>
        /// Ensures the chrome engine is downloaded and installed.
        /// Note this could take several minutes if the chrome engine hasn't been downloaded yet.
        /// </summary>
        public static void EnsureChromeEngineInstalled()
        {
            EnsureChromeEngineInstalled( GetBrowserFetcher(), true );
        }

        /// <inheritdoc cref="PdfGenerator.EnsureChromeEngineInstalled()"/>
        private static void EnsureChromeEngineInstalled( BrowserFetcher browserFetcher, bool checkForIncompleteInstall )
        {
            var pdfExternalRenderEndpoint = Rock.Web.SystemSettings.GetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT );

            if ( pdfExternalRenderEndpoint.IsNotNullOrWhiteSpace() )
            {
                // Using an External Render Endpoint, so we don't need to install a chrome engine on this server.
                return;
            }

            _lastProgressPercentage = 0;

            try
            {
                var executablePath = browserFetcher.GetExecutablePath( _browserVersion );
                var installingFlagFileName = Path.Combine( browserFetcher.CacheDir, ".installing" );
                var localInstallExists = browserFetcher.GetInstalledBrowsers().Any( b => b.BuildId == _browserVersion );

                // If checking for an incomplete install, check if there is an orphaned ".installing" file. Also make sure that the chrome.exe exists
                // (just in case files were deleted, but folders were not).
                bool reinstall = checkForIncompleteInstall && ( File.Exists( installingFlagFileName ) || ( localInstallExists && !File.Exists( executablePath ) ) );
                try
                {
                    if ( reinstall )
                    {
                        // Attempt to kill any chrome.exe processes that are running in our ChromeEngine directory so that we can remove it.
                        KillChromeProcesses();
                        browserFetcher.Uninstall( _browserVersion );
                        localInstallExists = false;
                    }
                }
                catch ( Exception ex )
                {
                    // If we get an error when attempting to re-install, log the exception and continue on in case it'll work even if we got an exception.
                    Rock.Model.ExceptionLogService.LogException( new PdfGeneratorException( "Error re-installing PDF Generator", ex ) );
                }

                if ( localInstallExists )
                {
                    // Already installed.
                    return;
                }

                File.WriteAllText( installingFlagFileName, "If this file exists, either the chrome engine is currently installing, or was interrupted before the install completed." );
                AsyncHelper.RunSync( () => browserFetcher.DownloadAsync( _browserVersion ) );
                File.Delete( installingFlagFileName );

                if ( _lastProgressPercentage > 99 )
                {
                    // If wasn't already downloaded, show a message that it downloaded successfully.
                    Debug.WriteLine( $"PdfGenerator ChromeEngine downloaded successfully." );
                }
            }
            catch ( IOException ioException )
            {
                // Could still be downloading and eventually work, so make exception a little friendlier.
                throw new PdfGeneratorException( "PDF Engine is not available. Please try again later.", ioException );
            }
            catch ( Exception ex )
            {
                // The IO Exception be the inner exception, so check that too.
                if ( ex.InnerException is IOException ioException )
                {
                    // Could still be downloading and eventually work, so make exception a little friendlier.
                    throw new PdfGeneratorException( "PDF Engine is not available. Please try again later.", ioException );
                }
                else
                {
                    throw new PdfGeneratorException( "Error downloading PDF Chrome Engine.", ex );
                }
            }
        }

        /// <summary>
        /// Gets the browser fetcher.
        /// </summary>
        /// <returns>BrowserFetcher.</returns>
        private static BrowserFetcher GetBrowserFetcher()
        {
            var browserDownloadPath = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/ChromeEngine" );
            Directory.CreateDirectory( browserDownloadPath );

            var browserFetcherOptions = new BrowserFetcherOptions
            {
                Browser = SupportedBrowser.Chrome,
                Path = browserDownloadPath,
            };

            return new BrowserFetcher( browserFetcherOptions );
        }

        /// <summary>
        /// Kills the chrome processes.
        /// </summary>
        private static void KillChromeProcesses()
        {
            try
            {
                var browserFetcher = GetBrowserFetcher();

                // Kill any chrome.exe's that got left running
                var executablePath = browserFetcher.GetExecutablePath( _browserVersion );
                var chromeProcesses = Process.GetProcessesByName( "chrome" );
                foreach ( var process in chromeProcesses )
                {
                    try
                    {
                        if ( process?.MainModule?.FileName == executablePath )
                        {
                            process.Kill();
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( $"INFO: Unable to cleanup orphaned chrome.exe processes, {ex}" );
                    }
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( $"INFO: Unable to cleanup orphaned chrome.exe processes, {ex}" );
            }
        }

        /// <summary>
        /// The paper formats
        /// </summary>
        public static readonly Dictionary<string, PaperFormat> PaperFormats = new Dictionary<string, PaperFormat>( StringComparer.OrdinalIgnoreCase )
        {
            { "Letter", PaperFormat.Letter },
            { "Legal", PaperFormat.Legal },
            { "Tabloid", PaperFormat.Tabloid },
            { "Ledger", PaperFormat.Ledger },
            { "A0", PaperFormat.A0 },
            { "A1", PaperFormat.A1 },
            { "A2", PaperFormat.A2 },
            { "A3", PaperFormat.A3 },
            { "A4", PaperFormat.A4 },
            { "A5", PaperFormat.A5 },
            { "A6", PaperFormat.A6 }
        };

        /// <summary>
        /// Gets or sets CSS @media. Defaults to <see cref="MediaType.Screen"/>
        /// </summary>
        /// <value>The type of the PDF media.</value>
        public MediaType PDFMediaType { get; set; } = MediaType.Screen;

        /// <summary>
        /// Gets or sets the paper format (Letter, Legal, A4). Defaults to <see cref="PaperFormat.Letter"/>
        /// </summary>
        /// <value>The paper format.</value>
        public PaperFormat PaperFormat { get; set; }

        /// <summary>
        /// Gets or sets the margin options. Refers to using inches as the unit of measurement.
        /// </summary>
        /// <value>The margin options.</value>
        public MarginOptions MarginOptions { get; set; }

        /// <summary>
        /// Paper ranges to print, e.g., <c>1-5, 8, 11-13</c>. Defaults to the empty string, which means print all pages.
        /// </summary>
        public string PageRanges { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public double? Width { get; set; } = 8.5;

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double? Height { get; set; } = 11;

        /// <summary>
        /// Display header and footer. Defaults to <c>false</c>.
        /// </summary>
        public bool DisplayHeaderFooter { get; set; } = false;

        /// <summary>
        /// Gets or sets the header HTML. This value will override the default. To use the default leave blank/null.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Print background graphics. Defaults to true.
        /// </summary>
        /// <value><c>true</c> if [print background]; otherwise, <c>false</c>.</value>
        public bool PrintBackground { get; set; } = true;

        /// <summary>
        /// Gets or sets the footer HTML. This value will override the default. To use the default leave blank/null.
        /// </summary>
        /// <value>The footer HTML.</value>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Initializes the chrome engine.
        /// </summary>
        private void InitializeChromeEngine()
        {
            var pdfExternalRenderEndpoint = Rock.Web.SystemSettings.GetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT );

            if ( pdfExternalRenderEndpoint.IsNotNullOrWhiteSpace() )
            {
                var connectOptions = new ConnectOptions
                {
                    BrowserWSEndpoint = pdfExternalRenderEndpoint
                };

                try
                {
                    _puppeteerBrowser = Puppeteer.ConnectAsync( connectOptions ).Result;
                }
                catch ( Exception ex )
                {
                    throw new PdfGeneratorException( $"Unable to connect using '{pdfExternalRenderEndpoint}'", ex );
                }
            }
            else
            {
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    DefaultViewport = new ViewPortOptions { Width = 1280, Height = 1024, DeviceScaleFactor = 1 },
                };

                var browserFetcher = GetBrowserFetcher();

                // should have already been installed, but just in case it hasn't, download it now.
                EnsureChromeEngineInstalled( browserFetcher, false );
                launchOptions.ExecutablePath = browserFetcher.GetExecutablePath( _browserVersion );

                _puppeteerBrowser = Puppeteer.LaunchAsync( launchOptions ).Result;
            }

            _puppeteerPage = _puppeteerBrowser.NewPageAsync().Result;
            _puppeteerPage.EmulateMediaTypeAsync( this.PDFMediaType ).Wait();
        }

        /// <summary>
        /// Generates the PDF document from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Stream.</returns>
        public Stream GetPDFDocumentFromURL( string url )
        {
            return GetPDFDocument( null, url );
        }

        /// <summary>
        /// Generates the PDF document from HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns>Stream.</returns>
        public Stream GetPDFDocumentFromHtml( string html )
        {
            return GetPDFDocument( html, null );
        }

        /// <summary>
        /// Creates a new BinaryFile record that has the PDF in it.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="html">The HTML.</param>
        /// <returns>Rock.Model.BinaryFile.</returns>
        public Rock.Model.BinaryFile GetAsBinaryFileFromHtml( int binaryFileTypeId, string fileName, string html )
        {
            return GetAsBinaryFile( binaryFileTypeId, fileName, html, null );
        }

        /// <summary>
        /// Creates a new BinaryFile record that has the PDF in it.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="url">The URL of the Html Page</param>
        /// <returns>Rock.Model.BinaryFile.</returns>
        public Rock.Model.BinaryFile GetAsBinaryFileFromUrl( int binaryFileTypeId, string fileName, string url )
        {
            return GetAsBinaryFile( binaryFileTypeId, fileName, null, url );
        }

        /// <summary>
        /// Creates a BinaryFile record that has the PDF in it.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="html">The HTML.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Rock.Model.BinaryFile.</returns>
        private Rock.Model.BinaryFile GetAsBinaryFile( int binaryFileTypeId, string fileName, string html, string url )
        {
            Rock.Model.BinaryFile binaryFile;

            using ( var pdfStream = this.GetPDFDocument( html, url ) )
            {
                binaryFile = new Rock.Model.BinaryFile();
                binaryFile.FileSize = pdfStream.Length;
                binaryFile.MimeType = "application/pdf";

                // copy the pdfStream into a new binaryfile.ContentStream so that it doesn't get disposed when we dispose the pdfStream.
                binaryFile.ContentStream = new MemoryStream( pdfStream.ReadBytesToEnd() );
                binaryFile.FileName = fileName.Replace( " ", "_" ).MakeValidFileName() + ".pdf";
                binaryFile.BinaryFileTypeId = binaryFileTypeId;
            }

            return binaryFile;
        }

        /// <summary>
        /// Generates the PDF document.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Stream.</returns>
        private Stream GetPDFDocument( string html, string url )
        {
            if ( html.IsNotNullOrWhiteSpace() )
            {
                var pdfHtml = html;

                // update all relative urls to absolute url
                string publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
                if ( publicAppRoot.IsNotNullOrWhiteSpace() )
                {
                    pdfHtml = pdfHtml.Replace( "~/", publicAppRoot );
                    pdfHtml = pdfHtml.Replace( @" src=""/", @" src=""" + publicAppRoot );
                    pdfHtml = pdfHtml.Replace( @" src='/", @" src='" + publicAppRoot );
                    pdfHtml = pdfHtml.Replace( @" href=""/", @" href=""" + publicAppRoot );
                    pdfHtml = pdfHtml.Replace( @" href='/", @" href='" + publicAppRoot );
                }

                _puppeteerPage.SetContentAsync( pdfHtml ).Wait();
            }
            else if ( url.IsNotNullOrWhiteSpace() )
            {
                _puppeteerPage.GoToAsync( url ).Wait();
            }
            else
            {
                return null;
            }

            var pdfOptions = new PdfOptions();

            // Set page format (e.g A4, Legal, Letter)
            if ( this.PaperFormat != null )
            {
                pdfOptions.Format = this.PaperFormat;
            }
            else
            {
                // Set Width
                if ( this.Width != null )
                {
                    pdfOptions.Width = this.Width.ToString() + "in";
                }

                // Set Height
                if ( this.Height != null )
                {
                    pdfOptions.Height = this.Height.ToString() + "in";
                }
            }


            // Set margins
            if ( this.MarginOptions != null )
            {
                pdfOptions.MarginOptions = this.MarginOptions;
            }
            else
            {
                pdfOptions.MarginOptions = new MarginOptions
                {
                    Top = "1in",
                    Right = "0.4in",
                    Left = "0.4in",
                    Bottom = "1in",
                };
            }

            pdfOptions.PrintBackground = this.PrintBackground;

            pdfOptions.DisplayHeaderFooter = this.DisplayHeaderFooter;
            if ( this.HeaderHtml.IsNotNullOrWhiteSpace() )
            {
                pdfOptions.HeaderTemplate = this.HeaderHtml;
            }

            if ( this.FooterHtml.IsNullOrWhiteSpace() )
            {
                // Set footer template to show pageNumber/totalPages on the bottom right.
                // See chromium source code at  https://source.chromium.org/chromium/chromium/src/+/main:components/printing/resources/print_header_footer_template_page.html
                pdfOptions.FooterTemplate = @"
<div class='text left grow'></div>
<div class='text right'>
    <span class='pageNumber'></span>/<span class='totalPages'></span>
</div>;";
            }
            else
            {
                pdfOptions.FooterTemplate = this.FooterHtml;
            }

            // Make Sure the margins doesn't push the document content out of bounds.
            if ( !IsMarginWithinBounds( pdfOptions ) )
            {
                throw new PdfGeneratorException( "The provided margins exceed the document dimensions." );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "PDF: Generate From HTML" ) )
            {
                activity?.AddTag( "rock.pdf.htmlsize", html.Length.ToString() );

                var pdfStreamTask = _puppeteerPage.PdfStreamAsync( pdfOptions );

                // It should only take a couple of seconds to create a PDF, even if it a big file. If it takes more than 30 seconds,
                // chrome probably had a page crash ( an 'Aw, snap' error). So just give up.
                TimeSpan maxWaitTime = TimeSpan.FromSeconds( 30 );
                if ( !pdfStreamTask.Wait( maxWaitTime ) )
                {
                    throw new PdfGeneratorException( "PDF Generator Time-Out" );
                }
                else
                {
                    return pdfStreamTask.Result;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified margin (in inches) is within the bounds of the document dimensions.
        /// </summary>
        /// <param name="pdfOptions"></param>
        /// <c>true</c> if the combined margins are within the document's width and height; otherwise, <c>false</c>.
        private bool IsMarginWithinBounds( PdfOptions pdfOptions )
        {
            if ( pdfOptions == null )
            {
                throw new ArgumentException( "pdfOptions is null or empty", nameof( pdfOptions ) );
            }

            if ( pdfOptions.Height == null || pdfOptions.Width == null )
            {
                return true;
            }

            // Parse page dimensions
            double height = ParseMeasurementToInches( pdfOptions.Height as string );
            double width = ParseMeasurementToInches( pdfOptions.Width as string );

            // Parse margin dimensions
            var margin = pdfOptions.MarginOptions;
            double marginLeft = ParseMeasurementToInches( margin.Left );
            double marginRight = ParseMeasurementToInches( margin.Right );
            double marginTop = ParseMeasurementToInches( margin.Top );
            double marginBottom = ParseMeasurementToInches( margin.Bottom );

            if ( ( marginLeft + marginRight ) > width )
            {
                return false;
            }
            if ( ( marginTop + marginBottom ) > height )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses a measurement string and converts it to inches. A string representing a measurement.
        /// </summary>
        /// <param name="measure">
        /// It should include a numeric value followed by an optional unit. 
        /// Supported units are "in" (inches), "cm" (centimeters), "mm" (millimeters), "pt" (points), "pc" (picas), and "px" (pixels).
        /// If no unit is provided, the value is assumed to be in inches. 
        /// </param>
        /// <returns>
        /// The measurement converted to inches as a double.
        /// </returns>
        private double ParseMeasurementToInches( string measure )
        {
            if ( string.IsNullOrWhiteSpace( measure ) )
            {
                throw new ArgumentException( "Measurement is null or empty", nameof( measure ) );
            }

            measure = measure.Trim().ToLowerInvariant();
            // default is inches
            double factor = 1.0;

            if ( measure.EndsWith( "in" ) )
            {
                measure = measure.Replace( "in", "" ).Trim();
            }
            else if ( measure.EndsWith( "cm" ) )
            {
                measure = measure.Replace( "cm", "" ).Trim();
                factor = 1 / 2.54;
            }
            else if ( measure.EndsWith( "mm" ) )
            {
                measure = measure.Replace( "mm", "" ).Trim();
                factor = 1 / 25.4;
            }
            else if ( measure.EndsWith( "pt" ) )
            {
                measure = measure.Replace( "pt", "" ).Trim();
                factor = 1 / 72.0;
            }
            else if ( measure.EndsWith( "pc" ) )
            {
                measure = measure.Replace( "pc", "" ).Trim();
                factor = 1 / 6.0;
            }
            else if ( measure.EndsWith( "px" ) )
            {
                measure = measure.Replace( "px", "" ).Trim();
                factor = 1 / 96.0;
            }

            // Convert the value to a double and multiply by the conversion faction.
            if ( double.TryParse( measure, NumberStyles.Any, CultureInfo.InvariantCulture, out double value ) )
            {
                return value * factor;
            }

            throw new FormatException( $"Invalid measurement format: {measure}" );
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _puppeteerPage?.Dispose();
            _puppeteerBrowser?.Dispose();
        }
    }
}