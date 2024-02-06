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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Blocks;
using Rock.Model;
using Rock.Tests.Integration.Events;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    [TestClass]
    public class RockEntityTests : LavaIntegrationTestBase
    {
        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            EventsDataManager.Instance.AddDataForRockSolidFinancesClass();
        }

        /// <summary>
        /// Tests the EventsCalendarItem to make sure that an item's EventItem and EventItem.Summary are returned.
        /// </summary>
        [TestMethod]
        [Ignore( "Fix required. This test requires specific test data that does not exist in the sample database." )]
        public void EventCalendarItemAllowsEventItemSummary()
        {
            RockEntityBlock.RegisterEntityCommands( LavaService.GetCurrentEngine() );

            var expectedOutput = @"
3 [2]: <br>
5 [3]: <br>
7 [4]: <br>
8 [5]: Scelerisque eleifend donec pretium vulputate sapien. Proin sed libero enim sed faucibus turpis in eu mi. Vel elit scelerisque mauris pellentesque pulvinar pellentesque habitant morbi. Egestas erat imperdiet sed euismod. Metus aliquam eleifend mi in.<br>
".Trim();

            var mergeFields = new Dictionary<string, object>();

            var lava = @"{% eventcalendaritem where:'EventCalendarId == 1' %}
{% for item in eventcalendaritemItems %}
{{ item.Id }} [{{ item.EventItemId }}]: {{ item.EventItem.Summary }}<br>
{% endfor %}
{% endeventcalendaritem %}";
            string output = lava.ResolveMergeFields( mergeFields, "RockEntity" ).Trim();

            Assert.That.AreEqualIgnoreNewline( expectedOutput, output );
        }

        [TestMethod]
        public void EntityCommandBlock_ContainingForBlock_ExecutesCorrectly()
        {
            var template = @"
{%- eventscheduledinstance eventid:'Rock Solid Finances Class' startdate:'2020-1-1' maxoccurrences:'25' daterange:'2m' -%}
    {%- for occurrence in EventItems -%}
        <b>Series {{forloop.index}}</b><br>

<br>
Occurrence Collection Type = {{ occurrence | TypeName }}
</br>

        {%- for item in occurrence -%}
            {%- if forloop.first -%}
                {{ item.Name }}
                <b>{{ item.DateTime | Date:'dddd' }} Series</b><br>
                <ol>
            {% endif %}

            <li>{{ item.DateTime | Date:'MMM d, yyyy' }} in {{ item.LocationDescription }}</li>

            {%- if forloop.last -%}
                </ol>
            {% endif %}
        {% endfor %}

    {% endfor %}
{% endeventscheduledinstance %}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<b>Series 1</b>" );
                Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                Assert.That.Contains( output, "<b>Series 2</b>" );
                Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }

        [TestMethod]
        public void EntityCommandBlock_NestedInParentEntityCommandBlock_ExecutesCorrectly()
        {
            var template = @"
{%- contentchannelitem where:'ContentChannelId == 21' limit:'1000' sort:'StartDateTime desc' iterator:'MessageSeries' -%}
  {%- for item in MessageSeries -%}

      {%- contentchannelitem ids:'391,392' sort:'StartDateTime desc' iterator:'Messages' -%}
        {%- for message in Messages -%}
          {{ message.Title }}
        {%- endfor -%}
      {%- endcontentchannelitem -%}
      
  {%- endfor -%}
{%- endcontentchannelitem -%}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template, engine.NewRenderContext( new List<string> { "All" } ) );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                //Assert.That.Contains( output, "<b>Series 1</b>" );
                //Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                //Assert.That.Contains( output, "<b>Series 2</b>" );
                //Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }

        [TestMethod]
        public void EntityCommandBlock_WhereFilterByAttribute_ReturnsMatchedEntitiesOnly()
        {
            var template = @"
{% contentchannelitem where:'Speaker == ""Pete Foster""' iterator:'items' %}

  {% for item in items %}
  {{ item.Title }} ({{ item | Attribute:'Speaker' }})
  {% endfor %}
{% endcontentchannelitem %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template, engine.NewRenderContext( new List<string> { "All" } ) );

                TestHelper.DebugWriteRenderResult( engine, template, output );

                Assert.That.Contains( output, "Of Faith and Firsts (Pete Foster)" );
                Assert.That.Contains( output, "1x8 (Pete Foster)" );
            } );
        }

        private const string _Count1AttributeGuid = "38850248-49DE-44B0-A150-1BA447AE35D0";
        private const string _Count2AttributeGuid = "32BAD99A-049E-4BC6-ABE7-786BF187484D";

        /// <summary>
        /// Verify sorting by an Entity Attribute that has the same Key as a second Attribute on a different
        /// Entity produces a correctly ordered result set for both entities.
        /// </summary>
        [TestMethod]
        public void EntityCommandBlock_SortByAttributeWithDuplicateKey_ReturnsCorrectlyOrderedSet()
        {
            AddDefinedTypesWithDuplicateAttribute();

            var template = @"
{% definedtype where:'Name == ""Title"" || Name == ""Suffix""' sort:'Name desc' iterator:'definedTypes' %}
    {% for definedType in definedTypes %}
        {% assign definedTypeId = definedType.Id %}
        <h1>{{ definedType.Name }}</h1>
        <ul>
        {% definedvalue where:'DefinedTypeId == {{definedTypeId}}' sort:'Count desc' iterator:'values' %}
            {% for value in values %}<li>{{ value | Attribute:'Count' }}: {{ value.Value }}</li>{% endfor %}
        {% enddefinedvalue %}
        </ul>
    {% endfor %}
{% enddefinedtype %}
";

            var expectedOutput = @"
<h1>Title</h1>
<ul>
    <li>7: Rev.</li><li>6: Ms.</li><li>5: Mrs.</li><li>4: Mr.</li><li>3: Miss</li><li>2: Dr.</li><li>1: Cpt.</li>
</ul>
<h1>Suffix</h1>
<ul>
    <li>8: VI</li><li>7: V</li><li>6: Sr.</li><li>5: Ph.D.</li><li>4: Jr.</li><li>3: IV</li><li>2: III</li><li>1: II</li>
</ul>
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, new LavaTestRenderOptions { EnabledCommands = "all" } );
        }

        private void AddDefinedTypesWithDuplicateAttribute()
        {
            var rockContext = new RockContext();

            // Add Attribute "Count" to multiple Defined Types.
            var definedTypeTitle = DefinedTypeCache.All().FirstOrDefault( c => c.Name == "Title" );
            var definedTypeSuffix = DefinedTypeCache.All().FirstOrDefault( c => c.Name == "Suffix" );

            var args = new TestDataHelper.Core.AddEntityAttributeArgs
            {
                ForeignKey = "IntegrationTest",

                EntityTypeIdentifier = SystemGuid.EntityType.DEFINED_VALUE,
                Key = "Count",
                FieldTypeIdentifier = SystemGuid.FieldType.INTEGER,
                EntityTypeQualifierColumn = "DefinedTypeId"
            };

            args.Guid = _Count1AttributeGuid.AsGuid();
            args.EntityTypeQualifierValue = definedTypeTitle.Id.ToString();

            var attribute1 = TestDataHelper.Core.AddEntityAttribute( args, rockContext );

            args.Guid = _Count2AttributeGuid.AsGuid();
            args.EntityTypeQualifierValue = definedTypeSuffix.Id.ToString();

            var attribute2 = TestDataHelper.Core.AddEntityAttribute( args, rockContext );

            rockContext.SaveChanges();

            // Set the Count values.
            var definedTypeIdList = new List<int> { definedTypeTitle.Id, definedTypeSuffix.Id };
            foreach ( var definedTypeId in definedTypeIdList )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var definedValues = definedValueService.Queryable()
                    .Where( dv => dv.DefinedTypeId == definedTypeId )
                    .OrderBy( dv => dv.Value )
                    .ToList();
                var count = 1;
                foreach ( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                    definedValue.SetAttributeValue( "Count", count.ToString() );
                    definedValue.SaveAttributeValues( rockContext );
                    count++;
                }
            }
        }

        /// <summary>
        /// Verify that the "business" tag is registered as an entity command.
        /// Tags are automatically registered for Rock Entity models, but the "business" tag is registered manually
        /// as an alias for the Person entity.
        /// </summary>
        [TestMethod]
        public void EntityCommandBlock_BusinessAlias_ReturnsPersonEntities()
        {
            var template = @"
{% business where:'LastName == ""Ace Hardware""' iterator:'items' %}
<ul>
  {% for item in items %}
    <li>{{ item.LastName }}</li>
  {% endfor %}
</ul>
{% endbusiness %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template, engine.NewRenderContext( new List<string> { "All" } ) );

                var options = new LavaTestRenderOptions
                {
                    EnabledCommands = "rockentity",
                    OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains
                };

                TestHelper.AssertTemplateOutput( engine, "Ace Hardware", template, options );
            } );
        }

        [DataTestMethod]
        [DataRow( @"LastName == ""Decker"" && NickName ==""Ted""", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName !=""Ted""", "Cindy Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName ^=""T""", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName *=""e""", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName *!""e""", "Cindy Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName *!""e""", "Cindy Decker" )]
        [DataRow( @"LastName == ""Decker"" && Employer _= """"", "Cindy Decker" )]
        [DataRow( @"LastName == ""Decker"" && Employer _! ""*""", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && BirthYear > 2000", "Alex Decker" )]
        [DataRow( @"LastName == ""Decker"" && BirthYear >= 2000", "Alex Decker" )]
        [DataRow( @"LastName == ""Decker"" && BirthYear < 2000", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && BirthYear <= 2000", "Ted Decker" )]
        [DataRow( @"LastName == ""Decker"" && NickName $=""dy""", "Cindy Decker" )]
        [DataRow( @"LastName == ""Decker"" || NickName ==""Bill""", "Bill Marble" )]
        public void EntityCommandBlock_WhereFilterOperators_AreProcessedCorrectly( string whereClause, string expectedOutputItem )
        {
            var template = @"
{% person where:'<whereClause>' iterator:'items' %}
<ul>
  {% for item in items %}
    <li>{{ item.NickName }} {{ item.LastName }}</li>
  {% endfor %}
</ul>
{% endperson %}
";

            template = template.Replace( "<whereClause>", whereClause );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var options = new LavaTestRenderOptions
                {
                    EnabledCommands = "rockentity",
                    OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains
                };
                TestHelper.AssertTemplateOutput( engine, expectedOutputItem, template, options );
            } );
        }


        [TestMethod]
        public void EntityCommandBlock_WhereWithLavaEmbeddedInStringLiteral_ResolvesLavaCorrectly()
        {
            var template = @"
{% person where:'BirthDate < ""{{ 'Now' | Date }}"" && LastName == ""Decker"" && NickName == ""Ted""' iterator:'items' %}
  {% for item in items %}
    {{ item.NickName }} {{ item.LastName }} ({{ item.BirthDate | Date:'yyyy-MM-dd' }})
  {% endfor %}
{% endperson %}
";

            var expectedOutput = @"Ted Decker (1985-02-10)";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var options = new LavaTestRenderOptions
                {
                    EnabledCommands = "rockentity",
                    IgnoreWhiteSpace = true
                };
                TestHelper.AssertTemplateOutput( engine, expectedOutput, template, options );
            } );
        }

        /// <summary>
        /// If a custom Lava component encounters an error and the exception handling strategy is set to "render",
        /// the Exception thrown by the component should be visible in the render output because it may contain important configuration information.
        /// </summary>
        [TestMethod]
        public void EntityCommandBlock_WithNoParameters_RendersErrorMessageToOutput()
        {
            // If a RockEntity command block is included without specifying any parameters, the block throws an Exception.
            // The Exception is wrapped in higher-level LavaExceptions, but we want to ensure that the original message 
            // is displayed in the render output to alert the user.
            var input = @"
{% person %}
    {% for person in personItems %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
            ";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var context = engine.NewRenderContext();

                context.SetEnabledCommands( "RockEntity" );

                var renderOptions = new LavaRenderParameters { Context = context, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput };

                var output = TestHelper.GetTemplateRenderResult( engine, input, renderOptions );

                TestHelper.DebugWriteRenderResult( engine, input, output.Text );

                Assert.IsTrue( output.Text.Contains( "No parameters were found in your command." ), "Expected message not found." );
            } );
		}

        /// Verify that Lava Entity Block parameter names are case-insensitive.
        /// Parameter names should be parsed and stored internally as lowercase.
        /// </summary>
        [TestMethod]
        public void EntityCommandBlock_ParameterNames_AreCaseInsentitive()
        {
            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };

            // Verify parameters: "where", "sort".
            var input1 = @"
{% person WHERE:'LastName == ""Decker""' Sort:'NickName' iterator:'people' %}
    {% for person in people %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
";

            var expectedOutput1 = @"
Alex Decker<br/>
Cindy Decker<br/>
Noah Decker<br/>
Ted Decker<br/>
";

            TestHelper.AssertTemplateOutput( expectedOutput1, input1, options );

            // Verify parameters: "id", "iterator".
            var tedPerson = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input2 = @"
{% person ID:$personId iTeRaToR:'people' %}
    {% for person in people %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
";

            var expectedOutput2 = @"
TedDecker<br/>
";

            input2 = input2.Replace( "$personId", tedPerson.Id.ToString() );
            TestHelper.AssertTemplateOutput( expectedOutput2, input2, options );
        }

        [TestMethod]
        public void EntityCommandBlock_WithCountParameterIsTrue_ReturnsCountVariableInContext()
        {
            var input = @"
{% group count:'true' expression:'Id != 0' limit:'10' %}
{{ count }}
{% endgroup %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var context = engine.NewRenderContext( new List<string> { "RockEntity" } );
                var result = engine.RenderTemplate( input, new LavaRenderParameters { Context = context } );
                var count = result.Text.AsInteger();

                Assert.IsTrue( count > 0, "Count variable is not set to a non-zero value." );
            } );
        }
    }
}
