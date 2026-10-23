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
using System.Runtime.ExceptionServices;

using Rock.Lava;

namespace Rock.Tests.Lava.Shared
{
    /// <summary>
    /// Renders Lava templates for tests. This helper renders and returns; it never
    /// asserts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The predecessor helpers combined render configuration, execution and
    /// comparison into a single <c>AssertTemplateOutput</c> call. A failure could
    /// not be attributed to the template or to the comparison without rewriting
    /// the call, and the comparison rules lived in defaults on an options object
    /// where no reader of the test could see them - the default stripped every
    /// whitespace character from both sides, so an assertion that read as an exact
    /// match could not tell "Line 1" from "Line1".
    /// </para>
    /// <para>
    /// Here a test renders, then asserts with plain MSTest. Write the expected
    /// value as a raw string literal, which strips the source indentation for you,
    /// and call <see cref="LavaTestStringExtensions.NormalizeLineEndings"/> on it
    /// when it spans more than one line. Use <see cref="LavaAssert"/> for the cases
    /// plain equality cannot express.
    /// </para>
    /// </remarks>
    public static class LavaRenderTestHelper
    {
        #region Properties

        /// <summary>
        /// Builds the engines a test renders against.
        /// </summary>
        /// <remarks>
        /// An assembly whose engines need more than the default configuration
        /// replaces this during its assembly initialization. Integration tests are
        /// the case that needs it: shortcodes defined as LavaShortcode rows can
        /// only be registered once a database is available, so they cannot be part
        /// of the default a unit test gets.
        /// </remarks>
        public static Func<IEnumerable<ILavaEngine>> ActiveEngineFactory { get; set; }
            = () => new List<ILavaEngine> { LavaTestEngineFactory.CreateFluidEngine() };

        #endregion Properties

        #region Methods

        /// <summary>
        /// Runs the test body once for each active engine, so that a test written
        /// once covers every engine Rock supports.
        /// </summary>
        /// <remarks>
        /// Only one engine is active today. The callback is kept so that adding an
        /// engine is a change to this helper rather than to every test.
        /// </remarks>
        /// <param name="testMethod">The test body, receiving the engine under test.</param>
        public static void ExecuteForActiveEngines( Action<ILavaEngine> testMethod )
        {
            ExecuteForActiveEngines( null, testMethod );
        }

        /// <summary>
        /// Runs the test body once for each active engine, after registering the
        /// components the test needs against each engine.
        /// </summary>
        /// <remarks>
        /// Registration has to happen here rather than in a class initializer
        /// because the engines do not outlive the call - see
        /// <see cref="CreateActiveEngines"/> for why.
        /// </remarks>
        /// <param name="configureEngine">Registers the components the test needs; may be null.</param>
        /// <param name="testMethod">The test body, receiving the engine under test.</param>
        public static void ExecuteForActiveEngines( Action<ILavaEngine> configureEngine, Action<ILavaEngine> testMethod )
        {
            var engines = CreateActiveEngines();
            var failures = new List<KeyValuePair<string, Exception>>();

            foreach ( var engine in engines )
            {
                // Lava components resolve the engine through the service locator
                // rather than receiving it, so the engine under test has to be
                // installed as the current one before the body runs.
                LavaService.SetCurrentEngine( engine );

                configureEngine?.Invoke( engine );

                try
                {
                    testMethod( engine );
                }
                catch ( Exception ex )
                {
                    failures.Add( new KeyValuePair<string, Exception>( engine.EngineName, ex ) );
                }
            }

            if ( !failures.Any() )
            {
                return;
            }

            if ( engines.Count == 1 )
            {
                /*
                    9/27/26 - CLAUDE

                    Rethrow the failure as it was raised rather than wrapping it.
                    Wrapping turns an AssertFailedException into a plain Exception,
                    which the test runner reports as an error carrying a generic
                    message instead of as a failure carrying the assertion text,
                    and it turns an Assert.Inconclusive into a failure. Naming the
                    engine is unnecessary here because only one ran.

                    ExceptionDispatchInfo preserves the original stack trace, which
                    a bare "throw ex" would reset to this line.

                    Reason: A single engine's failure must reach the test report
                    with its own type, message and stack trace.
                */
                ExceptionDispatchInfo.Capture( failures[0].Value ).Throw();
            }

            // With more than one engine, the failure has to say which engine
            // raised it, so each one is wrapped in a message that names it.
            var wrapped = failures
                .Select( f => new Exception( $"Engine \"{f.Key}\" reported an error.", f.Value ) )
                .ToList();

            throw new AggregateException( "At least one engine reported errors.", wrapped );
        }

        /// <summary>
        /// Builds a fresh set of engines to render against.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A new engine per call, rather than one shared by the assembly, because
        /// an engine is mutable: registering a tag, a block, a shortcode or a safe
        /// type changes it for every later caller. Sharing one made the outcome of
        /// a test depend on which tests had already run, which is the kind of
        /// failure that only shows up when someone runs a single test in isolation.
        /// </para>
        /// <para>
        /// A shared engine also could not see anything registered from the database,
        /// because it was built before any test had a database to seed. Building
        /// here means an engine is constructed inside whatever scope the test has
        /// already set up.
        /// </para>
        /// <para>
        /// This costs about a millisecond, measured against a suite where a test
        /// takes tens of milliseconds, so the isolation is close to free.
        /// </para>
        /// </remarks>
        /// <returns>The engines to render against.</returns>
        public static List<ILavaEngine> CreateActiveEngines()
        {
            var engines = ActiveEngineFactory?.Invoke()?.ToList() ?? new List<ILavaEngine>();

            if ( !engines.Any() )
            {
                throw new InvalidOperationException( $"At least one Lava engine is required. Check {nameof( ActiveEngineFactory )}." );
            }

            return engines;
        }

        /// <summary>
        /// Renders the template and returns the output text, with line terminators
        /// normalized to "\n".
        /// </summary>
        /// <remarks>
        /// Line endings are normalized because they reflect the line terminator of
        /// the source file the template literal came from, which varies between a
        /// Windows and a Linux checkout of this repository. A test that genuinely
        /// needs the raw text should call <see cref="RenderResult"/> and read
        /// <see cref="LavaRenderResult.Text"/>.
        /// </remarks>
        /// <param name="engine">The engine to render with.</param>
        /// <param name="inputTemplate">The template to render.</param>
        /// <param name="options">Optional render configuration.</param>
        /// <returns>The rendered output.</returns>
        public static string Render( ILavaEngine engine, string inputTemplate, LavaRenderOptions options = null )
        {
            var result = RenderResult( engine, inputTemplate, options );

            return result.Text.NormalizeLineEndings();
        }

        /// <summary>
        /// Renders the template and returns the full result, including any error.
        /// Use this when the test asserts on the error rather than the output.
        /// </summary>
        /// <param name="engine">The engine to render with.</param>
        /// <param name="inputTemplate">The template to render.</param>
        /// <param name="options">Optional render configuration.</param>
        /// <returns>The render result.</returns>
        public static LavaRenderResult RenderResult( ILavaEngine engine, string inputTemplate, LavaRenderOptions options = null )
        {
            if ( engine == null )
            {
                throw new ArgumentNullException( nameof( engine ) );
            }

            var context = engine.NewRenderContext();
            var parameters = LavaRenderParameters.WithContext( context );

            if ( options != null )
            {
                /*
                    9/27/26 - CLAUDE

                    Only set the enabled commands when the test asked for some.
                    NewRenderContext seeds the context from the engine's
                    DefaultEnabledCommands, and SetEnabledCommands replaces that
                    list rather than adding to it, so calling it with nothing to
                    set would clear the defaults for any test that supplies
                    options for an unrelated reason, such as merge fields.

                    Reason: Supplying render options must not disable the engine's
                    default commands.
                */
                if ( options.EnabledCommands.IsNotNullOrWhiteSpace() )
                {
                    context.SetEnabledCommands( options.EnabledCommands, options.EnabledCommandsDelimiter );
                }

                context.SetMergeFields( options.MergeFields );

                parameters.ExceptionHandlingStrategy = options.ExceptionHandlingStrategy;
                parameters.TimeZone = options.TimeZone;
                parameters.Culture = options.Culture;
                parameters.ShouldEncodeStringsAsXml = options.ShouldEncodeStringsAsXml;
            }

            return engine.RenderTemplate( inputTemplate ?? string.Empty, parameters );
        }

        #endregion Methods
    }
}
