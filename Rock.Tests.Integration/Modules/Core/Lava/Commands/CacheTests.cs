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
using Rock.Lava.Fluid;
using Rock.Tests.Shared;
using Rock.Model;
using Rock.Data;
using System.Linq;
using System;
using System.Collections;
using System.Data.Entity;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class CacheTests : LavaIntegrationTestBase
    {
        #region Cache Block

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

        #endregion

        #region CreateEntitySet filter

        [TestMethod]
        public void CreateEntitySet_WithInvalidInput_RendersErrorMessage()
        {
            var inputTemplate = @"
{% assign entitySet = null | CreateEntitySet %}
{{ entitySet.Id }}
";
            var options = new LavaTestRenderOptions { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput };
            var output = TestHelper.GetTemplateOutput( typeof( FluidEngine ), inputTemplate, options );

            Assert.That.Contains( output, "CreateEntitySet failed." );
        }

        [TestMethod]
        public void CreateEntitySet_WithInputAsEntityArray_ReturnsExpectedOutput()
        {
            var personList = GetTestPersonEntityList();
            CreateEntitySet( personList );
        }

        [TestMethod]
        public void CreateEntitySet_WithInputAsIntegerArray_ReturnsExpectedOutput()
        {
            var personIdArray = GetTestPersonEntityList().Select( p => p.Id ).ToList();
            CreateEntitySet( personIdArray, "Person" );
        }

        [TestMethod]
        public void CreateEntitySet_WithInputAsDelimitedString_ReturnsExpectedOutput()
        {
            var personIdList = GetTestPersonEntityList().Select( p => p.Id ).ToList().AsDelimited( "," );
            CreateEntitySet( personIdList, "Person" );
        }

        [TestMethod]
        public void CreateEntitySet_WithExpiryInMinutes_ReturnsCorrectlyConfiguredEntitySet()
        {
            var expectedExpiry = RockDateTime.Now.AddMinutes( 7 );
            var entitySet = CreatePersonEntitySetWithOptions( 7, null, null, null );

            Assert.That.AreProximate( expectedExpiry, entitySet.ExpireDateTime, new TimeSpan( 0, 1, 0 ) );
        }

        [TestMethod]
        public void CreateEntitySet_WithDefinedPurpose_ReturnsCorrectlyConfiguredEntitySet()
        {
            var definedValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid() );
            var entitySet = CreatePersonEntitySetWithOptions( purposeId: definedValueId );

            Assert.That.AreEqual( definedValueId, entitySet.EntitySetPurposeValueId.GetValueOrDefault() );
        }

        [TestMethod]
        public void CreateEntitySet_WithNote_ReturnsCorrectlyConfiguredEntitySet()
        {
            var note = "Test note.";
            var entitySet = CreatePersonEntitySetWithOptions( note: note );

            Assert.That.AreEqual( note, entitySet.Note );
        }

        [TestMethod]
        public void CreateEntitySet_WithParentSet_ReturnsCorrectlyConfiguredEntitySet()
        {
            var entitySetParent = CreatePersonEntitySetWithOptions();
            var entitySetChild = CreatePersonEntitySetWithOptions( parentSetId: entitySetParent.Id );

            Assert.That.AreEqual( entitySetParent.Id, entitySetChild.ParentEntitySetId );
        }

        [TestMethod]
        public void CreateEntitySet_EntityTypeParameter_AllowsIdOrIdKeyOrGuidOrName()
        {
            var personEntityType = EntityTypeCache.Get( typeof( Rock.Model.Person ) );

            // Specify EntityType as Id.
            AssertCreateEntitySetForEntityTypeParameter( personEntityType.Id.ToString() );

            // Specify EntityType as Guid.
            AssertCreateEntitySetForEntityTypeParameter( personEntityType.Guid.ToString() );

            // Specify EntityType as IdKey.
            AssertCreateEntitySetForEntityTypeParameter( personEntityType.IdKey );

            // Specify EntityType as Name.
            AssertCreateEntitySetForEntityTypeParameter( personEntityType.FriendlyName );
        }

        [TestMethod]
        public void CreateEntitySet_DocumentationExample1_ReturnsExpectedOutput()
        {
            var inputTemplate = @"
{% person where:'LastName == ""Decker""' select:'Id' iterator:'personIds' limit:4 %}
{% assign personIdList = personIds %}
{% endperson %} 
{% entitytype where:'FriendlyName == ""Person""' %}
{% assign entityTypeId = entitytype.Id %}
{% endentitytype %}
{% assign entitySet = personIdList | CreateEntitySet:entityTypeId,5,'Person Merge Request','Test note.','' %}
An Entity Set (Id={{ entitySet.Id }}) was created and {{ personIdList | Size }} people have been added.
";

            var expectedOutput = @"
An Entity Set (Id=*) was created and 4 people have been added.
";

            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity", Wildcards = new List<string>() { "*" } };
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, inputTemplate, options );
        }

        private void AssertCreateEntitySetForEntityTypeParameter( string entityTypeParameter )
        {
            var inputTemplate = @"
{% assign entitySet = '$personId' | CreateEntitySet:'$entityType' %}
{{ entitySet.Id }}
";
            // Require an Id integer value as output.
            var expectedOutput = "[0-9]*";

            var person = TestHelper.GetTestPersonTedDecker();

            inputTemplate = inputTemplate.Replace( "$personId", person.Id.ToString() )
                .Replace( "$entityType", entityTypeParameter );

            var options = new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.RegEx };
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, inputTemplate, options );
        }

        private List<Person> GetTestPersonEntityList()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var personTedDecker = personService.GetByIdentifierOrThrow( TestGuids.TestPeople.TedDecker );
            var personBillMarble = personService.GetByIdentifierOrThrow( TestGuids.TestPeople.BillMarble );
            var personMaddie = personService.GetByIdentifierOrThrow( TestGuids.TestPeople.MaddieLowe );

            var personList = new List<Person>
            {
                personTedDecker, personBillMarble, personMaddie
            };

            return personList;
        }

        private EntitySet CreateEntitySet( object input, string entityType = null, int? expiryInMinutes = null, int? purposeId = null, string note = null, int? parentSetId = null )
        {
            var inputTemplate = @"
{% assign entitySet = $input | CreateEntitySet:$args %}
{{ entitySet.Id }}
";

            var args = new List<string>
            {
                $"'{entityType}'",
                expiryInMinutes.GetValueOrDefault(20).ToStringSafe(),
                $"'{purposeId.ToStringSafe()}'",
                $@"'{note}'",
                parentSetId.ToStringSafe()
            };

            inputTemplate = inputTemplate.Replace( "$args", args.AsDelimited( "," ).TrimEnd( ',' ) );

            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "rockentity",
                Wildcards = new List<string>() { "*" }
            };

            if ( input is IEnumerable )
            {
                options.MergeFields = new LavaDataDictionary
                {
                    { "Input", input }
                };
                inputTemplate = inputTemplate.Replace( "$input", "Input" );
            }
            else
            {
                inputTemplate = inputTemplate.Replace( "$input", $"'{input}'" );
            }


            var output = TestHelper.GetTemplateOutput( typeof( FluidEngine ), inputTemplate, options );

            var entitySetId = output.ConvertToIntegerOrThrow();

            // Get the entity set, including the items. 
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );

            var entitySet = entitySetService.Queryable()
                .Include( s => s.Items )
                .FirstOrDefault( s => s.Id == entitySetId );

            return entitySet;
        }

        private EntitySet CreatePersonEntitySetWithOptions( int? expiryInMinutes = null, int? purposeId = null, string note = null, int? parentSetId = null )
        {
            var personList = GetTestPersonEntityList();
            var personIdList = personList.Select( p => p.Id )
                .ToList()
                .AsDelimited( "," );

            var entitySet = CreateEntitySet( personIdList, "Person", expiryInMinutes, purposeId, note, parentSetId );

            // Verify the number of items in the set.
            Assert.That.AreEqual( personList.Count(), entitySet.Items.Count );

            return entitySet;
        }

        #endregion
    }
}
