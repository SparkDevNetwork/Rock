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

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Lava.Blocks
{
    /// <summary>
    /// Tests for the Lava Cache block.
    /// </summary>
    [TestClass]
    public class CacheBlockTests
    {
        /// <summary>
        /// A Cache block rendered in a child scope (such as a loop or an included file)
        /// must keep the Lava commands enabled for the calling template.
        /// </summary>
        [TestMethod]
        public void CacheBlock_InChildScope_KeepsCallerEnabledCommands()
        {
            var template = @"
{% for i in (1..1) %}
    {% cache key:'issue-7166-outer' duration:'0' %}
        {% cache key:'issue-7166-inner' duration:'0' %}Inner Content{% endcache %}
    {% endcache %}
{% endfor %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var context = engine.NewRenderContext( new List<string> { "Cache" } );

                var output = engine.RenderTemplate( template, LavaRenderParameters.WithContext( context ) ).Text;

                Assert.AreEqual( "Inner Content", output.Trim() );
            } );
        }

        /// <summary>
        /// A Cache block rendered in a child scope must not pick up commands
        /// from the engine defaults that the calling template does not have.
        /// </summary>
        [TestMethod]
        public void CacheBlock_InChildScope_DoesNotEnableCommandsMissingFromCaller()
        {
            var template = @"
{% for i in (1..1) %}
    {% cache key:'issue-7166-sql' duration:'0' %}
        {% sql %}SELECT 1 AS [Value]{% endsql %}
    {% endcache %}
{% endfor %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Simulate a global default that is wider than the caller's commands.
                ( ( LavaEngineBase ) engine ).DefaultEnabledCommands = new List<string> { "Cache", "Sql" };

                var context = engine.NewRenderContext( new List<string> { "Cache" } );

                var output = engine.RenderTemplate( template, LavaRenderParameters.WithContext( context ) ).Text;

                Assert.AreEqual( "The Lava command 'sql' is not configured for this template.", output.Trim() );
            } );
        }
    }
}
