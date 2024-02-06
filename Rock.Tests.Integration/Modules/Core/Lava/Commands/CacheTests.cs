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
using Rock.Tests.Shared.Lava;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class CacheTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void CacheBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% cache key:'decker-page-list' duration:'3600' %}
This is the cached page list!
{% endcache %}
";

            var expectedOutput = "The Lava command 'cache' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        [TestMethod]
        public void CacheBlock_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% cache key:'decker-page-list' duration:'3600' %}
    {% person where:'LastName == ""Decker"" && NickName == ""Ted""' %}
        {% for person in personItems %}
            {{ person.FullName }} <br/>
        {% endfor %}
    {% endperson %}
{% endcache %}
";

            var expectedOutput = @"
TedDecker<br/>
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "Cache,RockEntity" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verifies the variable scoping behavior of the Cache block.
        /// Within the scope of a Cache block, an Assign statement should not affect the value of a same-named variable in the outer scope.
        /// This behavior differs from the standard scoping behavior for Liquid blocks.
        /// </summary>
        [TestMethod]
        public void CacheBlock_InnerScopeAssign_DoesNotModifyOuterVariable()
        {
            var input = @"
{% assign color = 'blue' %}
Color 1: {{ color }}

{% cache key:'fav-color' duration:'1200' %}
    Color 2: {{ color }}
    {% assign color = 'red' %}
    Color 3: {{color }}
{% endcache %}

Color 4: {{ color }}
";

            var expectedOutput = @"
Color 1: blue
Color 2: blue
Color 3: red
Color 4: blue
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "Cache" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verifies the variable scoping behavior of the Cache block.
        /// Within the scope of a Cache block, an Assign statement should not affect the value of a same-named variable in the outer scope.
        /// This behavior differs from the standard scoping behavior for Liquid blocks.
        /// </summary>
        [TestMethod]
        public void CacheBlock_InsideNewScope_HasAccessToOuterVariable()
        {
            var input = @"
{% if 1 == 1 %}
    {% assign color = 'blue' %}
    Color 1: {{ color }}

    {% cache key:'fav-color' duration:'0' %}
        Color 2: {{ color }}
        {% assign color = 'red' %}
        Color 3: {{color }}
    {% endcache %}

    Color 4: {{ color }}
{% endif %}
";

            var expectedOutput = @"
Color 1: blue
Color 2: blue
Color 3: red
Color 4: blue
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "Cache" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void CacheBlock_WithTwoPassOptionEnabled_EmitsCorrectOutput()
        {
            var input = @"
{%- cache key:'marketing-butter-bar' duration:'1800' twopass:'true' tags:'butter-bars' -%}
{%- assign now = 'Now' | Date:'yyyy-MM-ddTHH:mm:sszzz' | AsDateTime -%}
{%- contentchannelitem where:'StartDateTime < `{{now}}`' limit:'50' iterator:'Items' -%}
    {%- for item in Items -%}
        <div id=`mbb-{{item.Id}}` data-topbar-name=`dismiss{{item.Id}}Topbar` data-topbar-value=`dismissed` class=`topbar` style=`background-color:{{ item | Attribute:'BackgroundColor' }};color:{{ item | Attribute:'ForegroundColor' }};`>
        <a href=`{{ item | Attribute:'Link' | StripHtml }}`>
            <span class=`topbar-text`>
                {{ item | Attribute:'Text' }}
            </span>
        </a>
        <button type=`button` class=`close` data-dismiss=`alert` aria-label=`Close`><span aria-hidden=`true`>&times;</span></button>
        </div>
    {%- endfor -%}
{%- endcontentchannelitem -%}
{%- endcache -%}
";

            input = input.Replace( "`", "\"" );

            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "Cache,RockEntity",
                OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains
            };

            var expectedOutput = @"
<divid=`mbb-1`data-topbar-name=`dismiss1Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
<divid=`mbb-2`data-topbar-name=`dismiss2Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
<divid=`mbb-3`data-topbar-name=`dismiss3Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
";

            expectedOutput = expectedOutput.Replace( "`", @"""" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        [TestMethod]
        public void CacheBlock_MultipleRenderingPasses_ProducesSameOutput()
        {
            var input = @"
{%- cache key:'duplicate-test' duration:'10' -%}
This is the cache content.
{%- endcache -%}
";

            input = input.Replace( "`", "\"" );

            var options = new LavaTestRenderOptions { EnabledCommands = "Cache" };

            var expectedOutput = @"This is the cache content.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Render the template twice to ensure the result is the same.
                // The result is rendered and cached on the first pass and the same result should be rendered from the cache on the second pass.
                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        /// <summary>
        /// Verify that multiple cached Sql blocks on the same page maintain their individual contexts and output.
        /// </summary>
        [TestMethod]
        public void CacheBlock_MultipleInstancesOfCachedSqlBlocks_RendersCorrectOutput()
        {
            var input = @"
{% cache key:'test1' duration:'10' %}
{% sql %}
    SELECT 1 AS [Count]
{% endsql %}
{% assign item = results | First %}
Cache #{{ item.Count }}
{% endcache %}

{%- cache key:'test2' duration:'10' -%}
{% sql %}
    SELECT 2 AS [Count]
{% endsql %}
{% assign item = results | First %}
Cache #{{ item.Count }}
{% endcache %}

{%- cache key:'test3' duration:'10' -%}
{% sql %}
    SELECT 3 AS [Count]
{% endsql %}
{% assign item = results | First %}
Cache #{{ item.Count }}
{% endcache %}
";

            input = input.Replace( "`", "\"" );

            var options = new LavaTestRenderOptions { EnabledCommands = "Cache,Sql" };

            var expectedOutput = @"Cache #1 Cache #2 Cache #3";

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void CacheBlock_WithMultipleCacheTags_CachesTaggedContent()
        {
            // Define some new cache tags.
            var newTags = new List<TestDataHelper.Core.AddCacheTagArgs>
            {
                new TestDataHelper.Core.AddCacheTagArgs { Name = "tag1" },
                new TestDataHelper.Core.AddCacheTagArgs { Name = "tag2" },

            };

            TestDataHelper.Core.AddCacheTags( newTags );

            // Define the test Lava templates.
            var mergeFields = new LavaDataDictionary();
            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "Cache",
                MergeFields = mergeFields
            };

            var input = @"
<h2>Cache Tests</h2>
Input Value={{i}}<br>
{% cache key:'key1' tags:'tag1' %} 
Cache Value = {{i}}, Key=key1, Tag=tag1
{% endcache %}
<br>
{% cache key:'key2' tags:'tag2' %} 
Cache Value = {{i}}, Key=key2, Tag=tag2
{% endcache %}
<br>
{% cache key:'key3' tags:'tag1,tag2' %} 
Cache Value = {{i}}, Key=key3, Tag=tag1,tag2
{% endcache %}
<br>
{% cache key:'key4' tags:'undefined,tag2' %} 
Cache Value = {{i}}, Key=key4, Tag=undefined,tag2
{% endcache %}
<br>
";

            var expectedOutputTemplate = @"
<h2>CacheTests</h2>
InputValue={input}<br>
CacheValue={cache1},Key=key1,Tag=tag1<br>
CacheValue={cache2},Key=key2,Tag=tag2<br>
CacheValue={cache2},Key=key3,Tag=tag1,tag2<br>
CacheValue={cache2},Key=key4,Tag=undefined,tag2<br>
";

            // Test the cache tags for each Lava engine.
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Clear the cache for the current engine.
                RockCache.ClearAllCachedItems(); 

                // Render the template with value 1. The cache block output should be added to the cache.
                mergeFields["i"] = "1";
                var expectedOutput = expectedOutputTemplate.Replace( "{input}", "1" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "1" );
                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );

                // Render the template with value 2. The cache block output should be retrieved from the cache unchanged.
                mergeFields["i"] = "2";
                expectedOutput = expectedOutputTemplate.Replace( "{input}", "2" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "1" );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );

                // Verify that the tag "tag2" is associated with the correct keys.
                Assert.AreEqual( 3, RockCache.GetCountOfCachedItemsForTag( "tag2" ), "Invalid cache count for tag 'tag2'." );

                // Clear the cache keys associated with the tag "tag2".
                RockCache.RemoveForTags( "tag2" );

                Assert.AreEqual( 0, RockCache.GetCountOfCachedItemsForTag( "tag2" ), "Invalid cache count for tag 'tag2'." );

                // Render the template with value 2. The output should be updated for cache keys associated with tag "tag2".
                expectedOutput = expectedOutputTemplate.Replace( "{input}", "2" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "2" );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );

        }
    }
}
