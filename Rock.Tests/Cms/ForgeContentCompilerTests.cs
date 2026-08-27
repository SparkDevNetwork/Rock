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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;

namespace Rock.Tests.Cms
{
    /// <summary>
    /// Tests for the server-side Forge Content compiler, which runs the shared
    /// compiler bundle in a headless Chromium page. These need two things present:
    /// the built bundle at RockWeb/Obsidian/Libs/forgeContentCompiler.js, and
    /// the pinned Chromium under RockWeb/App_Data/ChromeEngine. When either is
    /// absent the tests are inconclusive rather than failing, because neither is
    /// produced by building this solution.
    /// </summary>
    [TestClass]
    public class ForgeContentCompilerTests
    {
        #region Fixtures

        private const string ValidSource = @"<template>
    <div class=""stat-card"">
        <h3>{{ title }}</h3>
        <ul>
            <li v-for=""item in items"" :key=""item.id"">{{ item.name }}</li>
        </ul>
    </div>
</template>

<script setup>
import { ref, onMounted } from ""vue"";

const title = ref(""Dashboard"");
const items = ref([]);

onMounted(() => {
    items.value = [{ id: 1, name: ""First"" }];
});
</script>

<style scoped>
.stat-card {
    padding: 12px;
}
</style>
";

        /// <summary>
        /// A reduction of the component that terminated the worker process: nested
        /// v-for, object literals inside :class and :style bindings, event
        /// modifiers, and async functions in the setup block.
        /// </summary>
        private const string ComplexSource = @"<template>
    <div class=""board"">
        <div v-for=""status in statuses""
             :key=""status.id""
             class=""column""
             :class=""{ 'column-over': dropTargetId === status.id }""
             @dragover.prevent=""onDragOver(status.id)""
             @drop.prevent=""onDrop(status.id)"">
            <span :style=""{ backgroundColor: status.color || '#bfbfbf' }""></span>
            <div v-for=""item in cardsFor(status.id)""
                 :key=""item.id""
                 class=""card""
                 :class=""{ 'card-dragging': draggingId === item.id }""
                 @dragstart=""onDragStart(item.id)"">
                <div>{{ item.name }}</div>
                <div v-if=""item.note"">{{ item.note }}</div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from ""vue"";

const statuses = ref([]);
const items = ref([]);
const draggingId = ref(null);
const dropTargetId = ref(null);
const selectedId = ref("""");

const grouped = computed(function () {
    const map = {};
    statuses.value.forEach(function (s) { map[s.id] = []; });
    items.value.forEach(function (i) { if (map[i.statusId]) { map[i.statusId].push(i); } });
    return map;
});

function cardsFor(id) { return grouped.value[id] || []; }
function onDragStart(id) { draggingId.value = id; }
function onDragOver(id) { dropTargetId.value = id; }

async function load() {
    try {
        const found = items.value.find(function (i) { return i.id === draggingId.value; });
        if (found) { found.statusId = dropTargetId.value; }
    }
    catch (e) {
        return null;
    }
}

async function onDrop(id) {
    dropTargetId.value = null;
    await load();
}

watch(selectedId, function () { load(); });
onMounted(function () { load(); });
</script>
";

        /// <summary>
        /// A component with a plain, UNSCOPED style block. This is the shape that
        /// puts the style injection directly against the component declaration.
        /// </summary>
        private const string UnscopedStyleSource = @"<template>
    <div class=""plain"">{{ label }}</div>
</template>

<style>
.plain {
    padding: 12px;
}
</style>

<script setup>
import { ref } from ""vue"";

const label = ref(""hello"");
</script>
";

        private const string BrokenSource = @"<template>
    <div>
        <span>{{ count </span>
    </div>
</template>

<script setup>
const count = 1;
</script>
";

        #endregion Fixtures

        #region CompileSource

        [TestMethod]
        public void CompileSource_WithValidSource_ProducesSystemJsModule()
        {
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( ValidSource );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );
            Assert.IsTrue( Regex.IsMatch( result.CompiledContent, @"^\s*System\.register\s*\(\s*\[" ), "Output is not a System.register module." );
            Assert.IsTrue( result.CompiledContent.Contains( "ccstyle-" ), "Output is missing the scoped style injection guard." );
            Assert.IsTrue( result.CompiledContent.Contains( "__scopeId" ), "Output is missing the scope id assignment for the scoped style." );
            Assert.IsTrue( Regex.IsMatch( result.VueVersion ?? string.Empty, @"^\d+\.\d+\.\d+" ), "VueVersion is not a semver string." );
        }

        [TestMethod]
        public void CompileSource_WithBrokenTemplate_ReturnsCompilerErrors()
        {
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( BrokenSource );

            Assert.IsFalse( result.IsSuccess );
            Assert.IsFalse( result.IsBundleMissing );
            Assert.IsTrue( result.Errors.Count > 0, "A failed compile must carry the compiler's error text." );
        }

        [TestMethod]
        public void CompileSource_WithStructurallyComplexSource_DoesNotExhaustTheStack()
        {
            // Regression: this shape (nested v-for, object literals in :class and
            // :style bindings, event modifiers, several async functions) needed about
            // 900 KB of stack against a 1 MB default and terminated the worker process
            // with an uncatchable StackOverflowException. Byte count is not the risk
            // factor here, structural depth is, so this fixture is deliberately small.
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( ComplexSource );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );
            Assert.IsTrue( Regex.IsMatch( result.CompiledContent, @"^\s*System\.register\s*\(\s*\[" ) );
        }

        [TestMethod]
        public void CompileSource_WithDeeplyNestedTemplate_FailsWithoutKillingTheProcess()
        {
            // The compile runs on a dedicated large-stack thread; without it a
            // template this deep terminates the process rather than returning. The
            // assertion is deliberately weak: either outcome is acceptable as long
            // as control returns here at all.
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );
            var deep = new StringBuilder();

            deep.AppendLine( "<template>" );
            for ( var i = 0; i < 400; i++ )
            {
                deep.Append( "<div>" );
            }
            deep.Append( "x" );
            for ( var i = 0; i < 400; i++ )
            {
                deep.Append( "</div>" );
            }
            deep.AppendLine();
            deep.AppendLine( "</template>" );
            deep.AppendLine( "<script setup>" );
            deep.AppendLine( "const x = 1;" );
            deep.AppendLine( "</script>" );

            var result = compiler.CompileSource( deep.ToString() );

            Assert.IsNotNull( result, "The compiler must return a result rather than terminating the process." );
        }

        /*
            8/14/2026 - CLAUDE

            The two tests below cover structurally deep source. They date from the
            Jint host, where depth was the one input that could kill the worker
            process: template nesting compiled to 1023 and died at 1024, script
            bracket nesting compiled to 511 and died at 512, both bounded by
            CompileRecursionLimit rather than by the stack.

            None of that applies now. The compile runs in a child browser process, so
            depth is no longer dangerous, and the complexity guard that used to
            enforce a limit ahead of it has been deleted along with its threshold.

            The tests are kept because depth is still the shape most likely to expose
            a compiler regression, and because they are the only coverage of nesting
            at all. Their depths are no longer "safely under a boundary", they are
            simply more than any real component uses: the worst of the 2,098 .obs
            files in this repository measures 37 estimated frames.

            Reason: Depth coverage outlived the danger that motivated it.
        */

        [TestMethod]
        public void CompileSource_AtRealisticTemplateDepth_Compiles()
        {
            // Real dashboards nest roughly 10 to 30 levels. 200 is far above anything real.
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( BuildNestedTemplateSource( 200 ) );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );
        }

        [TestMethod]
        public void CompileSource_AtRealisticScriptNestingDepth_Compiles()
        {
            // Object literals in class and style bindings rarely nest past 5 to 10 levels.
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( BuildNestedObjectSource( 100 ) );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );
        }

        [TestMethod]
        public void CompileSource_WithUnscopedStyle_TerminatesTheStatementBeforeStyleInjection()
        {
            // Regression: the component is emitted as `const __component = { ... }`
            // with no trailing semicolon, and the style injection begins with `(`.
            // JavaScript reads that as a call rather than inserting a semicolon, so
            // the module loaded and then threw "is not a function" at runtime.
            //
            // A scoped style masked this, because the scope-id assignment sits
            // between the two and starts with an identifier. Only an UNSCOPED style
            // puts the two statements directly against each other, which is why
            // every earlier fixture passed.
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), GetBrowserPathOrInconclusive() );

            var result = compiler.CompileSource( UnscopedStyleSource );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );

            var injectionIndex = result.CompiledContent.IndexOf( "(function () { var __id", StringComparison.Ordinal );
            Assert.IsTrue( injectionIndex > 0, "The compiled output should contain the style injection." );

            var beforeInjection = result.CompiledContent.Substring( 0, injectionIndex ).TrimEnd();
            Assert.IsTrue( beforeInjection.EndsWith( ";", StringComparison.Ordinal ),
                "The statement before the style injection must be terminated, otherwise the component object is invoked as a function. Tail was: "
                    + beforeInjection.Substring( Math.Max( 0, beforeInjection.Length - 40 ) ) );
        }

        [TestMethod]
        public void CompileSource_WithMissingBundle_ReportsBundleMissing()
        {
            var compiler = new ForgeContentCompiler( Path.Combine( Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid() + ".js" ) );

            var result = compiler.CompileSource( ValidSource );

            Assert.IsFalse( result.IsSuccess );
            Assert.IsTrue( result.IsBundleMissing );
        }

        [TestMethod]
        public void CompileSource_WithUnreachableRenderEndpoint_ReportsEndpointUnreachable()
        {
            // A dead endpoint must surface as its own configuration-problem result:
            // not a compile error the caller should fix source for, and above all
            // not IsBrowserMissing, whose "still provisioning, retry shortly" advice
            // is a lie when the endpoint is simply down.
            var unreachableEndpoint = "ws://localhost:1/does-not-exist";
            var compiler = new ForgeContentCompiler( GetBundlePathOrInconclusive(), browserWSEndpoint: unreachableEndpoint );

            var result = compiler.CompileSource( ValidSource );

            Assert.IsFalse( result.IsSuccess );
            Assert.IsTrue( result.IsRenderEndpointUnreachable );
            Assert.IsFalse( result.IsBrowserMissing, "An unreachable endpoint must not masquerade as a browser still being provisioned." );
            Assert.IsTrue( result.Errors.Any( e => e.Contains( unreachableEndpoint ) ), "The error must name the endpoint that could not be reached." );
        }

        #endregion CompileSource

        #region Support

        /// <summary>
        /// Builds a component whose template nests elements to the given depth.
        /// One element level costs one JavaScript call frame.
        /// </summary>
        /// <param name="depth">How many levels of nesting to emit.</param>
        /// <returns>The generated source.</returns>
        private static string BuildNestedTemplateSource( int depth )
        {
            var builder = new StringBuilder();

            builder.AppendLine( "<template>" );
            builder.Append( string.Concat( Enumerable.Repeat( "<div>", depth ) ) );
            builder.Append( "x" );
            builder.Append( string.Concat( Enumerable.Repeat( "</div>", depth ) ) );
            builder.AppendLine();
            builder.AppendLine( "</template>" );
            builder.AppendLine( "<script setup>" );
            builder.AppendLine( "const x = 1;" );
            builder.AppendLine( "</script>" );

            return builder.ToString();
        }

        /// <summary>
        /// Builds a component whose script nests object literals to the given depth,
        /// mirroring the object literals used in class and style bindings. One level
        /// costs two JavaScript call frames.
        /// </summary>
        /// <param name="depth">How many levels of nesting to emit.</param>
        /// <returns>The generated source.</returns>
        private static string BuildNestedObjectSource( int depth )
        {
            var builder = new StringBuilder();

            builder.AppendLine( "<template>" );
            builder.AppendLine( "<div>x</div>" );
            builder.AppendLine( "</template>" );
            builder.AppendLine( "<script setup>" );
            builder.Append( "const x = " );
            builder.Append( string.Concat( Enumerable.Repeat( "{ a: ", depth ) ) );
            builder.Append( "1" );
            builder.Append( string.Concat( Enumerable.Repeat( " }", depth ) ) );
            builder.AppendLine( ";" );
            builder.AppendLine( "</script>" );

            return builder.ToString();
        }

        /// <summary>
        /// Locates the pinned Chromium build by walking up from the test directory
        /// to the repository root. Marks the test inconclusive when it is absent,
        /// which is the normal state until something generates a PDF.
        /// </summary>
        /// <remarks>
        /// Tests cannot resolve this through <c>RockApp</c> the way the compiler
        /// does at runtime, because there is no hosted application here.
        /// </remarks>
        /// <returns>The physical path of the browser executable.</returns>
        private static string GetBrowserPathOrInconclusive()
        {
            var directory = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

            while ( directory != null )
            {
                var engineRoot = Path.Combine( directory.FullName, "RockWeb", "App_Data", "ChromeEngine" );

                if ( Directory.Exists( engineRoot ) )
                {
                    var executable = Directory
                        .EnumerateFiles( engineRoot, "chrome.exe", SearchOption.AllDirectories )
                        .OrderByDescending( path => path )
                        .FirstOrDefault();

                    if ( executable != null )
                    {
                        return executable;
                    }
                }

                directory = directory.Parent;
            }

            Assert.Inconclusive( "The Chromium build used to compile was not found under RockWeb/App_Data/ChromeEngine. It installs automatically the first time a PDF is generated." );
            return null;
        }

        /// <summary>
        /// Locates the built compiler bundle by walking up from the test directory
        /// to the repository root. Marks the test inconclusive when the Obsidian
        /// build has not produced the bundle in this environment.
        /// </summary>
        /// <returns>The physical path of the bundle.</returns>
        private static string GetBundlePathOrInconclusive()
        {
            var directory = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

            while ( directory != null )
            {
                var candidate = Path.Combine( directory.FullName, "RockWeb", "Obsidian", "Libs", "forgeContentCompiler.js" );

                if ( File.Exists( candidate ) )
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            Assert.Inconclusive( "The compiler bundle was not found. Run the Rock.JavaScript.Obsidian build to produce RockWeb/Obsidian/Libs/forgeContentCompiler.js." );
            return null;
        }

        #endregion Support
    }
}
