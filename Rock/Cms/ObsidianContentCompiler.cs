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
using System.Text.RegularExpressions;
using System.Threading;

using Jint;
using Jint.Runtime;

using Rock.Attribute;
using Rock.Configuration;

namespace Rock.Cms
{
    /*
        8/6/2026 - CLAUDE

        Compiles Obsidian Content authored source (a Vue single-file component) on
        the server by running the SAME compiler bundle the block's browser editor
        uses (~/Obsidian/Libs/obsidianContentCompiler.js) inside a Jint JavaScript
        engine. This exists so MCP clients with no repo checkout and no JavaScript
        runtime of their own (Claude Chat, Claude Desktop) get a real compile with
        structured errors instead of saving source that never renders.

        A new engine is created per compile and disposed afterward, deliberately.
        Compiles are rare, administrator-initiated, and a human is waiting, so the
        roughly one second cold path is acceptable while the steady-state memory
        cost stays zero. That matters on web farms, and it also sidesteps every
        engine thread-safety question. Do not cache the engine or the bundle text.

        The engine COMPILES the source; it never executes the compiled output. The
        output runs later in browsers, gated by the same block authorization as a
        save from the editor.

        See specs/260806-jint-in-process-obsidian-compile-plan.md for the design,
        the Phase 0 spike results, and the source-map constraint that lives in the
        bundle itself.

        Reason: Server-side compile for repo-less MCP authoring clients.

        8/6/2026 - CLAUDE

        The compile runs on a dedicated thread with a 16 MB stack because it can
        otherwise overflow the stack, and a StackOverflowException cannot be caught
        in .NET: it terminates the worker process, taking every request on the site
        with it rather than failing the one save.

        The cause is frame amplification. Jint is a tree-walking interpreter, so a
        single JavaScript call frame costs many native frames, and the compiler
        bundle contains recursive-descent parsers written in JavaScript (Babel for
        the script block, the template AST walk, then a final parse of the generated
        module). Measured: a moderately complex component needs about 900 KB of
        stack against a 1 MB default, and an ASP.NET request has already consumed
        part of that budget before this code runs. The same source compiles in a
        standalone console harness and dies under IIS for exactly that reason.

        Two things this is NOT:

        - It is not a size problem. A 30 KB component compiled fine while a 10 KB
          one overflowed. Structural depth drives stack use, not byte count, so the
          timing model in the spec says nothing about stack safety.
        - It is not solved by LimitRecursion. That counts JavaScript frames; the
          recursion that exhausts the stack is inside the engine. Setting it to 64
          still allowed the process to die.

        This is a mitigation, not a proof. It closes the margin by roughly eighteen
        times, but any input that recurses deeply enough can still reach the end of
        any fixed stack, and the failure remains uncatchable. Compiling in a
        short-lived child process is the only complete answer, and is an open
        question rather than a decision.

        Reason: An uncatchable stack overflow here would kill the whole worker.
    */

    /// <summary>
    /// Compiles Obsidian Content authored source into a SystemJS module by running
    /// the shared compiler bundle in an in-process JavaScript engine.
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
        /// How long a single compile may run before it is cancelled. Generous next
        /// to the measured sub-second compile time, tight enough that a
        /// pathological source cannot pin a thread.
        /// </summary>
        private static readonly TimeSpan CompileTimeout = TimeSpan.FromSeconds( 10 );

        /// <summary>
        /// <para>
        /// The stack size of the dedicated compile thread. Measured requirement for
        /// a moderately complex component (nested v-for, object literals in
        /// bindings, several async functions) is about 900 KB, against a default
        /// thread stack of 1 MB, and an ASP.NET request has already consumed part
        /// of that before the compile begins. 16 MB is roughly eighteen times the
        /// measured need.
        /// </para>
        /// <para>
        /// This is reserved address space, not committed memory: pages commit only
        /// as the stack is actually used, so an idle or shallow compile costs
        /// nothing near this figure.
        /// </para>
        /// </summary>
        private const int CompileThreadStackBytes = 16 * 1024 * 1024;

        /// <summary>
        /// The maximum script recursion depth.
        /// </summary>
        /// <remarks>
        /// This does NOT protect against the stack overflow this class guards
        /// against, and must not be relied on for that. It counts JavaScript call
        /// frames, while the recursion that exhausts the stack happens inside the
        /// engine's own evaluator and parser. Verified by experiment: a value of 64
        /// still let the process die. The dedicated thread stack is the real
        /// defense; this only bounds runaway recursion in authored code.
        /// </remarks>
        private const int CompileRecursionLimit = 1024;

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
        private const string SystemRegisterShim = @"
var __exports = {};
var System = {
    register: function (deps, declare) {
        if (deps.length > 0) {
            throw new Error('The compiler bundle unexpectedly declared dependencies: ' + deps.join(', '));
        }
        var mod = declare(function (name, value) {
            if (typeof name === 'object' && name !== null) {
                for (var key in name) { __exports[key] = name[key]; }
            }
            else {
                __exports[name] = value;
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
        /// An explicit physical path to the compiler bundle, used by tests. When
        /// null the bundle is resolved from the web root at compile time.
        /// </summary>
        private readonly string _bundlePhysicalPath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a compiler that resolves the bundle from the web root.
        /// </summary>
        public ObsidianContentCompiler()
        {
        }

        /// <summary>
        /// Creates a compiler that reads the bundle from an explicit physical
        /// path. This exists for tests, which run without a hosted web root.
        /// </summary>
        /// <param name="bundlePhysicalPath">The physical path of the compiler bundle.</param>
        internal ObsidianContentCompiler( string bundlePhysicalPath )
        {
            _bundlePhysicalPath = bundlePhysicalPath;
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

            // Read per call rather than caching; the file changes on deploy and
            // this path is cold by design. Done on the calling thread so any file
            // or path problem is reported normally rather than from the worker.
            var bundle = File.ReadAllText( bundlePath );

            // Run the engine on a dedicated thread with a large stack. A stack
            // overflow cannot be caught in .NET, so it would terminate the whole
            // worker process rather than failing this one request; the only real
            // defense is to make the stack big enough that the compiler cannot
            // reach the end of it. See CompileThreadStackBytes for the measurements.
            ObsidianContentCompileResult result = null;

            var worker = new Thread( () => result = RunEngine( bundle, source ), CompileThreadStackBytes )
            {
                IsBackground = true,
                Name = "ObsidianContentCompile"
            };

            worker.Start();

            // The engine's own timeout should end the work well before this, so
            // this join only bounds the case where the engine fails to honor it.
            // The thread is background, so a wedged worker cannot hold up shutdown.
            if ( !worker.Join( CompileTimeout + TimeSpan.FromSeconds( 5 ) ) )
            {
                return ObsidianContentCompileResult.Failure( "Compilation did not finish within the allowed time and was abandoned." );
            }

            return result ?? ObsidianContentCompileResult.Failure( "The compiler returned no result." );
        }

        /// <summary>
        /// Runs the compiler bundle in a JavaScript engine. Always called on the
        /// dedicated large-stack thread, never directly.
        /// </summary>
        /// <param name="bundle">The compiler bundle source.</param>
        /// <param name="source">The authored Vue single-file-component source.</param>
        /// <returns>The result of the compile attempt.</returns>
        private ObsidianContentCompileResult RunEngine( string bundle, string source )
        {
            try
            {
                using ( var engine = new Engine( options => options
                    .TimeoutInterval( CompileTimeout )
                    .LimitRecursion( CompileRecursionLimit ) ) )
                {
                    engine.Execute( SystemRegisterShim );
                    engine.Execute( bundle );
                    engine.SetValue( "__ocSource", source );

                    var result = engine.Evaluate( "__exports.compileSource(__ocSource)" ).AsObject();
                    var compiledContent = result.Get( "compiledContent" ).AsString();
                    var vueVersion = result.Get( "vueVersion" ).AsString();

                    // The bundle already parse-validates its own output; this is the
                    // final structural gate before the caller stores anything.
                    if ( !SystemRegisterShape.IsMatch( compiledContent ) )
                    {
                        return ObsidianContentCompileResult.Failure( "The compiler produced output that is not a SystemJS module." );
                    }

                    return ObsidianContentCompileResult.Success( compiledContent, vueVersion );
                }
            }
            catch ( JavaScriptException ex )
            {
                // The compiler throws a JavaScript Error whose message carries the
                // real compile problem (parse errors, bad filters, unknown syntax).
                // That text is the feedback loop; pass it through unaltered.
                return ObsidianContentCompileResult.Failure( ex.Message );
            }
            catch ( TimeoutException )
            {
                return ObsidianContentCompileResult.Failure( $"Compilation exceeded the {CompileTimeout.TotalSeconds:0} second limit and was cancelled." );
            }
            catch ( Exception ex )
            {
                // Constraint violations and engine faults land here. Include the
                // type name so an operator can tell a recursion limit from a
                // genuine engine bug without a debugger.
                return ObsidianContentCompileResult.Failure( $"The compile engine failed ({ex.GetType().Name}): {ex.Message}" );
            }
        }

        #endregion Methods
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

        #endregion Factory Methods
    }
}
