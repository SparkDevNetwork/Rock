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
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;

namespace Rock.Tests.Cms
{
    /// <summary>
    /// Tests for the server-side Obsidian Content compiler, which runs the shared
    /// compiler bundle inside a Jint engine. These tests require the built bundle
    /// at RockWeb/Obsidian/Libs/obsidianContentCompiler.js; when the Obsidian
    /// build has not produced it, the tests are inconclusive rather than failing.
    /// </summary>
    [TestClass]
    public class ObsidianContentCompilerTests
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
            var compiler = new ObsidianContentCompiler( GetBundlePathOrInconclusive() );

            var result = compiler.CompileSource( ValidSource );

            Assert.IsTrue( result.IsSuccess, "Compile failed: " + string.Join( "; ", result.Errors ) );
            Assert.IsTrue( Regex.IsMatch( result.CompiledContent, @"^\s*System\.register\s*\(\s*\[" ), "Output is not a System.register module." );
            Assert.IsTrue( result.CompiledContent.Contains( "ocstyle-" ), "Output is missing the scoped style injection guard." );
            Assert.IsTrue( result.CompiledContent.Contains( "__scopeId" ), "Output is missing the scope id assignment for the scoped style." );
            Assert.IsTrue( Regex.IsMatch( result.VueVersion ?? string.Empty, @"^\d+\.\d+\.\d+" ), "VueVersion is not a semver string." );
        }

        [TestMethod]
        public void CompileSource_WithBrokenTemplate_ReturnsCompilerErrors()
        {
            var compiler = new ObsidianContentCompiler( GetBundlePathOrInconclusive() );

            var result = compiler.CompileSource( BrokenSource );

            Assert.IsFalse( result.IsSuccess );
            Assert.IsFalse( result.IsBundleMissing );
            Assert.IsTrue( result.Errors.Count > 0, "A failed compile must carry the compiler's error text." );
        }

        [TestMethod]
        public void CompileSource_WithMissingBundle_ReportsBundleMissing()
        {
            var compiler = new ObsidianContentCompiler( Path.Combine( Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid() + ".js" ) );

            var result = compiler.CompileSource( ValidSource );

            Assert.IsFalse( result.IsSuccess );
            Assert.IsTrue( result.IsBundleMissing );
        }

        #endregion CompileSource

        #region Support

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
                var candidate = Path.Combine( directory.FullName, "RockWeb", "Obsidian", "Libs", "obsidianContentCompiler.js" );

                if ( File.Exists( candidate ) )
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            Assert.Inconclusive( "The compiler bundle was not found. Run the Rock.JavaScript.Obsidian build to produce RockWeb/Obsidian/Libs/obsidianContentCompiler.js." );
            return null;
        }

        #endregion Support
    }
}
