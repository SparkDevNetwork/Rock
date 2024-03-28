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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    [TestClass]
    public class SqlCommandTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void SqlBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}
";

            var expectedOutput = "The Lava command 'sql' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void SqlBlock_WithInvalidSql_RendersErrorMessage()
        {
            var input = @"
{% sql %}
    SELECT   [Unknown]
    FROM     [Person] 
{% endsql %}
";

            var expectedOutput = "Lava Error:(.*)Invalid column name 'Unknown'.";

            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "Sql",
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput,
                OutputMatchType = LavaTestOutputMatchTypeSpecifier.RegEx
            };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );

        }

        [TestMethod]
        public void SqlBlock_PersonWhereLastNameIsDecker_ReturnsDeckers()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"Alex_Decker;Ted_Decker;";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void SqlBlock_NullColumnValueInResult_IsRenderedAsEmptyString()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
UNION
    SELECT   null as [NickName], null as [LastName]
    ORDER BY [NickName]
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"_;Alex_Decker;Ted_Decker;";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verify that a nullable database column is mapped to a Nullable<> System.Type.
        /// </summary>
        [TestMethod]
        public void SqlBlock_NullableDatabaseColumn_IsReturnedAsNullableType()
        {
            var input = @"
{% sql return:'Items' %}
    SELECT TOP 1 p.[PhotoId] FROM [Person] as p WHERE [PhotoId] IS NULL
{% endsql %}
{% for item in Items %}
    {% if item.PhotoId == null %}
    Is Null
    {% endif %}
{% endfor %}
";

            var expectedOutput = @"Is Null";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verify that the Select filter operates correctly on the result set returned by the Sql Lava block.
        /// Verifies Issue #4938 ⁃ Select Lava Filter Does Not See Values In A SQL Results Array.
        /// Refer https://github.com/SparkDevNetwork/Rock/issues/4938.
        /// </summary>
        [TestMethod]
        [Ignore( "Test needs to be updated to add the Stepping Stone campus first." )]
        public void SqlBlock_SelectFilterAppliedToResultSet_ReturnsSelectedField()
        {
            var input = @"
{% sql %}
SELECT * FROM Campus WHERE [Name] IN ('Main Campus', 'Stepping Stone');
{% endsql %}
{% assign campusNames = results | Select:'Name' | Uniq %}
{% for campusName in campusNames %}
{{ campusName }};
{% endfor %}
";

            var expectedOutput = @"
Main Campus;Stepping Stone;
";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void SqlBlock_WithCommandType_ExecutesCorrectly()
        {
            var input = @"
{% sql statement:'command' %}
    DELETE FROM [Person]
    WHERE 1 != 1
{% endsql %}
{{ results }} records were selected.";

            var expectedOutput = @"
0 records were selected.
";
            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "sql",
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput

            };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }
    }

    #region SQL Timeout Tests

    [TestClass]
    [Ignore( "These tests are long-running and should only be enabled when testing changes to the Sql Command Block." )]
    public class SqlCommandTimeoutTests : LavaIntegrationTestBase
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
                SELECT TOP 5 * 
                FROM Person
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

        [TestMethod]
        public void SqlBlock_MultipleExecutionsWithTimeout_TimeoutShouldReturnToDefault()
        {
            var input = @"
{% sql statement:'command' timeout:'60' %}
    WAITFOR DELAY '00:00:35';
    DELETE FROM [DefinedValue] WHERE 1 != 1
{% endsql %}

{% sql timeout:'10' %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName];
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"Alex_Decker;Ted_Decker;";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verify that the SQL block timeout parameter does not affect subsequent database operations.
        /// </summary>
        [TestMethod]
        public void SqlBlock_ParallelExecutionsWithTimeout_TimeoutShouldReturnToDefault()
        {
            var templateLong = @"
***
Iteration: {{ iteration }}
***
{% sql statement:'command' timeout:'60' %}
    SELECT DELETE FROM [DefinedValue] WHERE 1 != 1
    WAITFOR DELAY '00:00:35';
{% endsql %}
";

            var templateShort = @"

{% sql %}
    WAITFOR DELAY '00:00:05';
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName];
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"***Iteration: <?>***Alex_Decker;Ted_Decker;";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    TestHelper.DebugWriteRenderResult( engine, "(Ignored)", "(Ignored)" );
                    return;
                }

                Action<int, string> renderAction = ( x, input ) =>
                       {
                           var context = new LavaDataDictionary();
                           context["iteration"] = x;
                           var options = new LavaTestRenderOptions() { EnabledCommands = "Sql", MergeFields = context, Wildcards = new List<string> { "<?>" } };

                           TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
                       };


                var task1 = Task.Run( () => renderAction( 1, templateLong ) );
                var task2 = Task.Run( () => renderAction( 2, templateShort ) );
                var task3 = Task.Run( () => renderAction( 2, templateShort ) );

                Task.WhenAll( task1, task2, task3 );
            } );
        }

        private LavaRenderResult ExecuteSqlBlock( ILavaEngine engine, string lavaScript )
        {
            var renderContext = engine.NewRenderContext( new List<string> { "Sql" } );

            var result = engine.RenderTemplate( lavaScript,
                new LavaRenderParameters { Context = renderContext, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

            return result;
        }
    }

    #endregion
}
