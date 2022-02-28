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
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    [Ignore("These tests are long-running and should only be enabled when testing changes to the Sql Command Block.")]
    public class SqlTests : LavaIntegrationTestBase
    {
        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectShortTimeoutShouldFail()
        {
            var lavaScript = @"{% sql timeout:'10' %}

            WAITFOR DELAY '00:00:20';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.Contains( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectLongTimeoutShouldPass()
        {
            var lavaScript = @"{% sql timeout:'40' %}

            WAITFOR DELAY '00:00:35';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.DoesNotContain( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        public void SqlSelectNoTimeoutShouldPass()
        {
            var lavaScript = @"{% sql %}

            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.DoesNotContain( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlSelectNoTimeoutButQueryLongerThen30SecondsShouldFail()
        {
            var lavaScript = @"{% sql %}

            WAITFOR DELAY '00:00:35';
            SELECT TOP 5 * 
            FROM Person
            {% endsql %}

            [
            {%- for item in results -%}
                {
                        ""CreatedDateTime"": {{ item.CreatedDateTime | ToJSON }},
                        ""LastName"": {{ item.LastName | ToJSON }},
                }{% unless forloop.last -%},{% endunless %}
            {%- endfor -%}
            ]";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.Contains( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandShortTimeoutShouldFail()
        {
            var lavaScript = @"{% sql statement:'command' timeout:'10' %}
                WAITFOR DELAY '00:00:20';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.Contains( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandLongTimeoutShouldPass()
        {
            var lavaScript = @"{% sql statement:'command' timeout:'40' %}
                WAITFOR DELAY '00:00:35';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.DoesNotContain( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        public void SqlCommandNoTimeoutShouldPass()
        {
            var lavaScript = @"{% sql statement:'command' %}
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = ExecuteSqlBlock( engine, lavaScript );

                Assert.That.DoesNotContain( result.Error?.Messages().JoinStrings( "//" ), "Execution Timeout Expired." );
            } );
        }

        [TestMethod]
        [TestProperty( "Execution Time", "Long" )]
        public void SqlCommandNoTimeoutButQueryLongerThen30SecondsShouldFail()
        {
            var lavaScript = @"{% sql statement:'command' %}
                WAITFOR DELAY '00:00:35';
                DELETE FROM [DefinedValue] WHERE 1 != 1
            {% endsql %}

            {{ results }} {{ 'record' | PluralizeForQuantity:results }} were deleted.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var renderContext = engine.NewRenderContext( new List<string> { "Sql" } );

                var result = engine.RenderTemplate( lavaScript,
                    new LavaRenderParameters { Context = renderContext, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

                var errorMessages = result.Error.Messages().JoinStrings( "//" );

                Assert.That.Contains( errorMessages, "Execution Timeout Expired." );
            } );
        }

        private LavaRenderResult ExecuteSqlBlock(ILavaEngine engine, string lavaScript)
        {
            var renderContext = engine.NewRenderContext( new List<string> { "Sql" } );

            var result = engine.RenderTemplate( lavaScript,
                new LavaRenderParameters { Context = renderContext, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

            return result;
        }
    }
}
