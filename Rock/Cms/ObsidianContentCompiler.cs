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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using PuppeteerSharp;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Pdf;
using Rock.Utility;

namespace Rock.Cms
{
    /*
        8/6/2026 - CLAUDE

        Compiles Obsidian Content authored source (a Vue single-file component) on
        the server by running the SAME compiler bundle the block's browser editor
        uses (~/Obsidian/Libs/obsidianContentCompiler.js). This exists so MCP clients
        with no repo checkout and no JavaScript runtime of their own (Claude Chat,
        Claude Desktop) get a real compile with structured errors instead of saving
        source that never renders.

        Reason: Server-side compile for repo-less MCP authoring clients.

        8/14/2026 - CLAUDE

        The engine is now the headless Chromium that Rock already manages for PDF
        generation, reached through PuppeteerSharp. It replaces the in-process Jint
        host entirely, and the change is confined to this class: SetContentSource and
        every other caller are untouched.

        This is the complete fix for the problem the previous implementation could
        only mitigate. A stack overflow inside Jint terminated the worker process and
        could not be caught, because the stack being exhausted was Rock's own. The
        stack here belongs to a child process, so exhausting it closes a page and
        raises an ordinary catchable exception. The dedicated 16 MB thread that used
        to buy margin against that is gone; there is nothing left for it to protect.

        Three further gains, none of them the reason for the change but all of them
        real:

        - Output now matches the browser editor by construction rather than by
          convention. Same bundle AND same engine, so the two hosts cannot drift.
        - V8 compiles rather than interprets, so the 731 KB bundle costs far less to
          parse than it did under a tree-walking interpreter.
        - No new dependency. PuppeteerSharp and the pinned Chromium build are already
          shipped and version-managed for PdfGenerator.

        Deliberate choices, in the order they were decided:

        - A SEPARATE browser process from PdfGenerator's. The install is shared, so
          there is no second download, but a wedged compile must not disturb
          statement generation and vice versa.
        - A FRESH PAGE per compile, disposed afterward, so one compile cannot leave
          state behind for the next. The browser itself is long lived because
          launching one costs far more than opening a page.
        - Chromium is NEVER downloaded from this path. PdfGenerator will fetch it on
          demand, which is correct for a background job with no one waiting, but here
          a human is waiting behind an agent tool call and a 100 MB download would
          blow every timeout. A missing browser is reported as its own result so the
          caller can say something honest and retry later.

        There was briefly an ObsidianContentComplexityGuard ahead of this, refusing
        structurally deep source before any engine existed. It was the only way to
        turn the Jint stack overflow into an ordinary error, and it is deleted rather
        than kept, deliberately. Once the compile moved out of process its every
        remaining failure mode was downside: under-counting became harmless, because
        a dead page is survivable, while over-counting still refused work a person
        legitimately wrote. Its threshold was also calibrated against Jint's stack and
        described limits this engine does not have. A second safety mechanism that
        guards nothing is a maintenance liability, so the process boundary is the only
        one now. Do not reintroduce a nesting check without a measured reason.

        The engine COMPILES the source; it never executes the compiled output. That
        output runs later in visitors' browsers, gated by the same block
        authorization as a save from the editor.

        Reason: Containing the compile in a child process removes the uncatchable
        crash instead of merely making it less likely.
    */

    /// <summary>
    /// Compiles Obsidian Content authored source into a SystemJS module by running
    /// the shared compiler bundle in Rock's managed headless browser.
    /// </summary>
    [RockInternal( "18.0" )]
    internal class ObsidianContentCompiler
    {
        #region Constants

        /// <summary>
        /// The Rock-relative path of the shared compiler bundle. This is the same
        /// bundle the block's editor loads through the import map, so browser and
        /// server always compile identically.
        /// </summary>
        private const string CompilerBundleVirtualPath = "~/Obsidian/Libs/obsidianContentCompiler.js";

        /// <summary>
        /// The Rock-relative path of the shared Chromium install, matching the
        /// location <see cref="PdfGenerator"/> uses so the two features share one
        /// download and one pinned version.
        /// </summary>
        private const string ChromeEngineVirtualPath = "~/App_Data/ChromeEngine";

        /// <summary>
        /// How long a single compile may run in the page before it is abandoned.
        /// Generous next to the measured compile time, tight enough that a
        /// pathological source cannot pin a page indefinitely.
        /// </summary>
        private const int CompileTimeoutMilliseconds = 30000;

        /// <summary>
        /// The same structural check the MCP save path applies: the output must be
        /// a SystemJS module registration, not free-form text.
        /// </summary>
        private static readonly Regex SystemRegisterShape = new Regex( @"^\s*System\.register\s*\(\s*\[", RegexOptions.Compiled );

        /// <summary>
        /// A minimal System.register capture, mirroring the shim technique the
        /// block's view panel uses to instantiate stored modules. The compiler
        /// bundle registers with an empty dependency array; the shim throws if
        /// that assumption ever breaks rather than mis-linking silently.
        /// </summary>
        /// <remarks>
        /// Still required in a page. The bundle is SystemJS format and a blank page
        /// has no loader, so without this its registration would go nowhere.
        /// </remarks>
        private const string SystemRegisterShim = @"
window.__exports = {};
window.System = {
    register: function (deps, declare) {
        if (deps.length > 0) {
            throw new Error('The compiler bundle unexpectedly declared dependencies: ' + deps.join(', '));
        }
        var mod = declare(function (name, value) {
            if (typeof name === 'object' && name !== null) {
                for (var key in name) { window.__exports[key] = name[key]; }
            }
            else {
                window.__exports[name] = value;
            }
            return value;
        }, {});
        mod.execute();
    }
};
";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The long-lived browser dedicated to compiling, separate from the one
        /// PdfGenerator launches. Guarded by <see cref="_browserLock"/>.
        /// </summary>
        private static IBrowser _browser;

        /// <summary>
        /// Serializes browser creation so a burst of saves cannot launch several.
        /// </summary>
        private static readonly object _browserLock = new object();

        /// <summary>
        /// An explicit physical path to the compiler bundle, used by tests. When
        /// null the bundle is resolved from the web root at compile time.
        /// </summary>
        private readonly string _bundlePhysicalPath;

        /// <summary>
        /// An explicit physical path to the browser executable, used by tests.
        /// When null the browser is resolved from the shared install through
        /// <see cref="RockApp"/>, which does not exist outside a hosted app.
        /// </summary>
        private readonly string _browserExecutablePath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a compiler that resolves the bundle from the web root.
        /// </summary>
        public ObsidianContentCompiler()
        {
        }

        /// <summary>
        /// Creates a compiler that reads the bundle and the browser from explicit
        /// physical paths. This exists for tests, which run without a hosted web
        /// root and therefore cannot resolve either through <see cref="RockApp"/>.
        /// </summary>
        /// <param name="bundlePhysicalPath">The physical path of the compiler bundle.</param>
        /// <param name="browserExecutablePath">The physical path of the browser executable.</param>
        internal ObsidianContentCompiler( string bundlePhysicalPath, string browserExecutablePath = null )
        {
            _bundlePhysicalPath = bundlePhysicalPath;
            _browserExecutablePath = browserExecutablePath;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Compiles authored single-file-component source into a SystemJS module
        /// using the shared compiler bundle. Never throws; every failure mode is
        /// reported through the result.
        /// </summary>
        /// <param name="source">The authored Vue single-file-component source.</param>
        /// <returns>The result of the compile attempt.</returns>
        public ObsidianContentCompileResult CompileSource( string source )
        {
            if ( source.IsNullOrWhiteSpace() )
            {
                return ObsidianContentCompileResult.Failure( "No source was provided." );
            }

            var bundlePath = _bundlePhysicalPath ?? RockApp.Current.MapPath( CompilerBundleVirtualPath );

            if ( !File.Exists( bundlePath ) )
            {
                return ObsidianContentCompileResult.BundleMissing();
            }

            if ( !IsBrowserInstalled() )
            {
                return ObsidianContentCompileResult.BrowserMissing();
            }

            // Read per call rather than caching; the file changes on deploy and this
            // path is cold by design.
            var bundle = File.ReadAllText( bundlePath );

            try
            {
                return AsyncHelper.RunSync( () => CompileInPageAsync( bundle, source ) );
            }
            catch ( Exception ex )
            {
                // AsyncHelper unwraps to the original exception, but a faulted task
                // can still surface aggregated. Flatten so the caller sees the real
                // message rather than "One or more errors occurred".
                var actual = ex is AggregateException aggregate
                    ? aggregate.Flatten().InnerExceptions.FirstOrDefault() ?? ex
                    : ex;

                return ObsidianContentCompileResult.Failure( DescribeFailure( actual ) );
            }
        }

        /// <summary>
        /// Runs the compiler bundle in a fresh page and returns its output.
        /// </summary>
        /// <param name="bundle">The compiler bundle source.</param>
        /// <param name="source">The authored Vue single-file-component source.</param>
        /// <returns>The result of the compile attempt.</returns>
        private async Task<ObsidianContentCompileResult> CompileInPageAsync( string bundle, string source )
        {
            var browser = await GetBrowserAsync().ConfigureAwait( false );
            IPage page = null;

            try
            {
                page = await browser.NewPageAsync().ConfigureAwait( false );
                page.DefaultTimeout = CompileTimeoutMilliseconds;

                // A blank page, never navigated. The compiler needs no document and
                // must not be able to reach the network.
                await page.EvaluateExpressionAsync( SystemRegisterShim ).ConfigureAwait( false );
                await page.EvaluateExpressionAsync( bundle ).ConfigureAwait( false );

                var output = await page
                    .EvaluateFunctionAsync<CompileOutput>( "(src) => window.__exports.compileSource(src)", source )
                    .ConfigureAwait( false );

                if ( output == null || output.CompiledContent.IsNullOrWhiteSpace() )
                {
                    return ObsidianContentCompileResult.Failure( "The compiler returned no output." );
                }

                // The bundle already parse-validates its own output; this is the
                // final structural gate before the caller stores anything.
                if ( !SystemRegisterShape.IsMatch( output.CompiledContent ) )
                {
                    return ObsidianContentCompileResult.Failure( "The compiler produced output that is not a SystemJS module." );
                }

                return ObsidianContentCompileResult.Success( output.CompiledContent, output.VueVersion );
            }
            finally
            {
                if ( page != null )
                {
                    try
                    {
                        await page.CloseAsync().ConfigureAwait( false );
                    }
                    catch
                    {
                        // Intentionally ignored: the page may already be gone if the
                        // renderer died, and failing to close it must not mask the
                        // real result.
                    }
                }
            }
        }

        /// <summary>
        /// Gets the shared compile browser, launching it when it is missing or has
        /// disconnected.
        /// </summary>
        /// <returns>A connected browser.</returns>
        private async Task<IBrowser> GetBrowserAsync()
        {
            var existing = _browser;

            if ( existing != null && existing.IsConnected )
            {
                return existing;
            }

            // Launching is slow enough that a burst of saves could otherwise start
            // several browsers. The lock is held only while starting one.
            var launchTask = null as Task<IBrowser>;

            lock ( _browserLock )
            {
                if ( _browser != null && _browser.IsConnected )
                {
                    return _browser;
                }

                _browser = null;
                launchTask = LaunchBrowserAsync();
            }

            var browser = await launchTask.ConfigureAwait( false );

            lock ( _browserLock )
            {
                _browser = browser;
            }

            return browser;
        }

        /// <summary>
        /// Launches a headless browser dedicated to compiling, using the Chromium
        /// build already pinned and installed for PDF generation.
        /// </summary>
        /// <returns>The launched browser.</returns>
        private async Task<IBrowser> LaunchBrowserAsync()
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = GetBrowserExecutablePath()
            };

            return await Puppeteer.LaunchAsync( launchOptions ).ConfigureAwait( false );
        }

        /// <summary>
        /// Determines whether the pinned Chromium build is already installed.
        /// </summary>
        /// <remarks>
        /// This deliberately does not install anything. PdfGenerator downloads on
        /// demand because nobody is waiting on a background job; here an agent is
        /// waiting on a tool call, and a hundred-megabyte download would exceed
        /// every timeout between here and the caller.
        /// </remarks>
        /// <returns><c>true</c> when the browser can be launched.</returns>
        private bool IsBrowserInstalled()
        {
            try
            {
                var executablePath = GetBrowserExecutablePath();

                return executablePath.IsNotNullOrWhiteSpace() && File.Exists( executablePath );
            }
            catch
            {
                // Intentionally ignored: any failure resolving the install is
                // reported to the caller as "not installed", which carries the
                // correct advice either way.
                return false;
            }
        }

        /// <summary>
        /// Resolves the browser executable, preferring an explicit path supplied
        /// for tests over the shared install.
        /// </summary>
        /// <returns>The physical path of the browser executable.</returns>
        private string GetBrowserExecutablePath()
        {
            if ( _browserExecutablePath.IsNotNullOrWhiteSpace() )
            {
                return _browserExecutablePath;
            }

            return GetBrowserFetcher().GetExecutablePath( PdfGenerator.BrowserVersion );
        }

        /// <summary>
        /// Builds a fetcher pointed at the shared Chromium install location.
        /// </summary>
        /// <returns>The browser fetcher.</returns>
        private static BrowserFetcher GetBrowserFetcher()
        {
            var browserDownloadPath = RockApp.Current.MapPath( ChromeEngineVirtualPath );

            return new BrowserFetcher( new BrowserFetcherOptions
            {
                Browser = SupportedBrowser.Chrome,
                Path = browserDownloadPath
            } );
        }

        /// <summary>
        /// Turns an exception from the page into caller-facing text.
        /// </summary>
        /// <param name="exception">The exception the compile raised.</param>
        /// <returns>The message describing what went wrong.</returns>
        private static string DescribeFailure( Exception exception )
        {
            // A dead renderer is the case this whole design exists to survive. It is
            // an ordinary catchable exception now, so say plainly what happened
            // rather than passing through Puppeteer's wording.
            if ( exception is TargetClosedException )
            {
                return "The compiler process stopped unexpectedly, which usually means the source is too complex to compile. Simplify the component and try again.";
            }

            if ( exception is WaitTaskTimeoutException || exception is TimeoutException )
            {
                return $"Compilation exceeded the {CompileTimeoutMilliseconds / 1000} second limit and was cancelled.";
            }

            // EvaluationFailedException carries the compiler's own error text, which
            // is the feedback loop the agent needs; pass it through unaltered.
            if ( exception is EvaluationFailedException )
            {
                return exception.Message;
            }

            return $"The compile engine failed ({exception.GetType().Name}): {exception.Message}";
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// The shape the compiler bundle returns, deserialized from the page.
        /// </summary>
        private class CompileOutput
        {
            /// <summary>
            /// Gets or sets the compiled SystemJS module string.
            /// </summary>
            public string CompiledContent { get; set; }

            /// <summary>
            /// Gets or sets the Vue version the compile targeted.
            /// </summary>
            public string VueVersion { get; set; }
        }

        #endregion Support Classes
    }

    /// <summary>
    /// The result of a server-side Obsidian Content compile attempt.
    /// </summary>
    [RockInternal( "18.0" )]
    internal class ObsidianContentCompileResult
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the compile succeeded and
        /// <see cref="CompiledContent"/> is safe to store.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the compile could not be attempted
        /// because the compiler bundle is not deployed. Callers should fall back
        /// to a source-only save rather than reporting a compile error.
        /// </summary>
        public bool IsBundleMissing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the compile could not be attempted
        /// because the shared Chromium build is not installed yet. This is a
        /// transient state on an instance that has never generated a PDF, so
        /// callers should advise retrying rather than report a failure.
        /// </summary>
        public bool IsBrowserMissing { get; private set; }

        /// <summary>
        /// Gets the compiled SystemJS module string, or null when the compile failed.
        /// </summary>
        public string CompiledContent { get; private set; }

        /// <summary>
        /// Gets the Vue version the compile targeted, or null when the compile failed.
        /// </summary>
        public string VueVersion { get; private set; }

        /// <summary>
        /// Gets the compiler messages describing why the compile failed. Empty on success.
        /// </summary>
        public List<string> Errors { get; private set; } = new List<string>();

        #endregion Properties

        #region Factory Methods

        /// <summary>
        /// Creates a successful result carrying the compiled output.
        /// </summary>
        /// <param name="compiledContent">The compiled SystemJS module string.</param>
        /// <param name="vueVersion">The Vue version the compile targeted.</param>
        /// <returns>The result.</returns>
        public static ObsidianContentCompileResult Success( string compiledContent, string vueVersion )
        {
            return new ObsidianContentCompileResult
            {
                IsSuccess = true,
                CompiledContent = compiledContent,
                VueVersion = vueVersion
            };
        }

        /// <summary>
        /// Creates a failed result carrying the compiler's error text.
        /// </summary>
        /// <param name="error">The message describing why the compile failed.</param>
        /// <returns>The result.</returns>
        public static ObsidianContentCompileResult Failure( string error )
        {
            var result = new ObsidianContentCompileResult
            {
                IsSuccess = false
            };

            result.Errors.Add( error );

            return result;
        }

        /// <summary>
        /// Creates a result indicating the compiler bundle is not deployed, so no
        /// compile could be attempted at all.
        /// </summary>
        /// <returns>The result.</returns>
        public static ObsidianContentCompileResult BundleMissing()
        {
            var result = new ObsidianContentCompileResult
            {
                IsSuccess = false,
                IsBundleMissing = true
            };

            result.Errors.Add( "The compiler bundle is not deployed on this server." );

            return result;
        }

        /// <summary>
        /// Creates a result indicating the shared Chromium build has not been
        /// installed yet, so no compile could be attempted.
        /// </summary>
        /// <returns>The result.</returns>
        public static ObsidianContentCompileResult BrowserMissing()
        {
            var result = new ObsidianContentCompileResult
            {
                IsSuccess = false,
                IsBrowserMissing = true
            };

            result.Errors.Add( "The browser engine used to compile is not installed on this server yet." );

            return result;
        }

        #endregion Factory Methods
    }
}
