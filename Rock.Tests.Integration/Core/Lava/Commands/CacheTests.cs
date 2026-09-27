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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core.Lava.Commands
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
{%- cache key:'decker-page-list' duration:'3600' -%}
This is the cached page list!
{%- endcache -%}
";

            var expectedOutput = "The Lava command 'cache' is not configured for this template.";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, new LavaRenderOptions { } );

                Assert.Contains( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void CacheBlock_ForEntityCommandResult_IsCached()
        {
            var input = @"
{%- cache key:'decker-page-list' duration:'3600' -%}
    {%- person where:'LastName == ""Decker"" && NickName == ""Ted""' -%}
        {%- for person in personItems -%}
            {{ person.FullName }} <br/>
        {%- endfor -%}
    {%- endperson -%}
{%- endcache -%}
";

            var expectedOutput = "\n"
                + "    \n"
                + "        \n"
                + "            Ted Decker <br/>\n"
                + "        \n"
                + "    \n";

            var options = new LavaRenderOptions() { EnabledCommands = "Cache,RockEntity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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
{%- assign color = 'blue' -%}
Color 1: {{ color }}

{%- cache key:'fav-color' duration:'1200' -%}
    Color 2: {{ color }}
    {%- assign color = 'red' -%}
    Color 3: {{color }}
{%- endcache -%}

Color 4: {{ color }}
";

            var expectedOutput = "Color 1: blue\n"
                + "    Color 2: blue\n"
                + "    \n"
                + "    Color 3: red\n"
                + "Color 4: blue\n";

            var options = new LavaRenderOptions() { EnabledCommands = "Cache" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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
{%- if 1 == 1 -%}
    {%- assign color = 'blue' -%}
    Color 1: {{ color }}

    {%- cache key:'fav-color' duration:'0' -%}
        Color 2: {{ color }}
        {%- assign color = 'red' -%}
        Color 3: {{color }}
    {%- endcache -%}

    Color 4: {{ color }}
{%- endif -%}
";

            var expectedOutput = "Color 1: blue\n"
                + "        Color 2: blue\n"
                + "        \n"
                + "        Color 3: red\n"
                + "    Color 4: blue";

            var options = new LavaRenderOptions() { EnabledCommands = "Cache" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var options = new LavaRenderOptions
            {
                EnabledCommands = "Cache,RockEntity"
            };

            var expectedOutput = @"
<divid=`mbb-1`data-topbar-name=`dismiss1Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
<divid=`mbb-2`data-topbar-name=`dismiss2Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
<divid=`mbb-3`data-topbar-name=`dismiss3Topbar`data-topbar-value=`dismissed`class=`topbar`style=`background-color:;color:;`><ahref=``><spanclass=`topbar-text`></span></a><buttontype=`button`class=`close`data-dismiss=`alert`aria-label=`Close`><spanaria-hidden=`true`>&times;</span></button></div>
".NormalizeLineEndings();

            expectedOutput = expectedOutput.Replace( "`", @"""" );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                // The expected value above is written with every whitespace
                // character already removed, which is how the predecessor
                // comparison treated it, so the output is reduced the same way
                // before looking for it.
                var comparableOutput = Regex.Replace( output, @"\s", string.Empty );
                var comparableExpected = Regex.Replace( expectedOutput, @"\s", string.Empty );

                Assert.Contains( comparableExpected, comparableOutput );
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

            var options = new LavaRenderOptions { EnabledCommands = "Cache" };

            var expectedOutput = "\n"
                + "This is the cache content.\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Render the template twice to ensure the result is the same.
                // The result is rendered and cached on the first pass and the same result should be rendered from the cache on the second pass.
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
                var secondPass = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, secondPass );
            } );
        }

        /// <summary>
        /// Verify that multiple cached Sql blocks on the same page maintain their individual contexts and output.
        /// </summary>
        [TestMethod]
        public void CacheBlock_MultipleInstancesOfCachedSqlBlocks_RendersCorrectOutput()
        {
            var input = @"
{%- cache key:'test1' duration:'10' -%}
{%- sql -%}
    SELECT 1 AS [Count]
{%- endsql -%}
{%- assign item = results | First -%}
Cache #{{ item.Count }}
{%- endcache -%}

{%- cache key:'test2' duration:'10' -%}
{%- sql -%}
    SELECT 2 AS [Count]
{%- endsql -%}
{%- assign item = results | First -%}
Cache #{{ item.Count }}
{%- endcache -%}

{%- cache key:'test3' duration:'10' -%}
{%- sql -%}
    SELECT 3 AS [Count]
{%- endsql -%}
{%- assign item = results | First -%}
Cache #{{ item.Count }}
{%- endcache -%}
";

            input = input.Replace( "`", "\"" );

            var options = new LavaRenderOptions { EnabledCommands = "Cache,Sql" };

            var expectedOutput = "\n"
                + "\n"
                + "\n"
                + "Cache #1\n"
                + "\n"
                + "\n"
                + "\n"
                + "Cache #2\n"
                + "\n"
                + "\n"
                + "\n"
                + "Cache #3\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.Contains( expectedOutput, output );
            } );
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
            var options = new LavaRenderOptions
            {
                EnabledCommands = "Cache",
                MergeFields = mergeFields
            };

            var input = @"
<h2>Cache Tests</h2>
Input Value={{i}}<br>
{%- cache key:'key1' tags:'tag1' -%} 
Cache Value = {{i}}, Key=key1, Tag=tag1
{%- endcache -%}
<br>
{%- cache key:'key2' tags:'tag2' -%} 
Cache Value = {{i}}, Key=key2, Tag=tag2
{%- endcache -%}
<br>
{%- cache key:'key3' tags:'tag1,tag2' -%} 
Cache Value = {{i}}, Key=key3, Tag=tag1,tag2
{%- endcache -%}
<br>
{%- cache key:'key4' tags:'undefined,tag2' -%} 
Cache Value = {{i}}, Key=key4, Tag=undefined,tag2
{%- endcache -%}
<br>
";

            // This was written with every space removed, which is how the
            // predecessor comparison treated it. Some lines end in a space that
            // is part of the output, so escapes are used to keep it visible.
            var expectedOutputTemplate = "\n"
                + "<h2>Cache Tests</h2>\n"
                + "Input Value={input}<br> \n"
                + "Cache Value = {cache1}, Key=key1, Tag=tag1\n"
                + "<br> \n"
                + "Cache Value = {cache2}, Key=key2, Tag=tag2\n"
                + "<br> \n"
                + "Cache Value = {cache2}, Key=key3, Tag=tag1,tag2\n"
                + "<br> \n"
                + "Cache Value = {cache2}, Key=key4, Tag=undefined,tag2\n"
                + "<br>\n";

            // Test the cache tags for each Lava engine.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Clear the cache for the current engine.
                RockCache.ClearAllCachedItems();

                // Render the template with value 1. The cache block output should be added to the cache.
                mergeFields["i"] = "1";
                var expectedOutput = expectedOutputTemplate.Replace( "{input}", "1" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "1" );
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );

                // Render the template with value 2. The cache block output should be retrieved from the cache unchanged.
                mergeFields["i"] = "2";
                expectedOutput = expectedOutputTemplate.Replace( "{input}", "2" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "1" );

                output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );

                // Verify that the tag "tag2" is associated with the correct keys.
                Assert.AreEqual( 3, RockCache.GetCountOfCachedItemsForTag( "tag2" ), "Invalid cache count for tag 'tag2'." );

                // Clear the cache keys associated with the tag "tag2".
                RockCache.RemoveForTags( "tag2" );

                Assert.AreEqual( 0, RockCache.GetCountOfCachedItemsForTag( "tag2" ), "Invalid cache count for tag 'tag2'." );

                // Render the template with value 2. The output should be updated for cache keys associated with tag "tag2".
                expectedOutput = expectedOutputTemplate.Replace( "{input}", "2" )
                    .Replace( "{cache1}", "1" )
                    .Replace( "{cache2}", "2" );

                output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );

        }

        #endregion

        #region CreateEntitySet filter

        [TestMethod]
        public void CreateEntitySet_WithInvalidInput_RendersErrorMessage()
        {
            var inputTemplate = @"
{%- assign entitySet = null | CreateEntitySet -%}
{{ entitySet.Id }}
";
            var options = new LavaRenderOptions { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate, options );

                Assert.Contains( "CreateEntitySet failed.", output );
            } );
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

            Assert.AreEqual( definedValueId, entitySet.EntitySetPurposeValueId.GetValueOrDefault() );
        }

        [TestMethod]
        public void CreateEntitySet_WithNote_ReturnsCorrectlyConfiguredEntitySet()
        {
            var note = "Test note.";
            var entitySet = CreatePersonEntitySetWithOptions( note: note );

            Assert.AreEqual( note, entitySet.Note );
        }

        [TestMethod]
        public void CreateEntitySet_WithParentSet_ReturnsCorrectlyConfiguredEntitySet()
        {
            var entitySetParent = CreatePersonEntitySetWithOptions();
            var entitySetChild = CreatePersonEntitySetWithOptions( parentSetId: entitySetParent.Id );

            Assert.AreEqual( entitySetParent.Id, entitySetChild.ParentEntitySetId );
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
{%- person where:'LastName == ""Decker""' select:'Id' iterator:'personIds' limit:4 -%}
{%- assign personIdList = personIds -%}
{%- endperson -%} 
{%- entitytype where:'FriendlyName == ""Person""' -%}
{%- assign entityTypeId = entitytype.Id -%}
{%- endentitytype -%}
{%- assign entitySet = personIdList | CreateEntitySet:entityTypeId,5,'Person Merge Request','Test note.','' -%}
An Entity Set (Id={{ entitySet.Id }}) was created and {{ personIdList | Size }} people have been added.
";

            var expectedOutput = "An Entity Set (Id=13) was created and 4 people have been added.\n";

            var options = new LavaRenderOptions { EnabledCommands = "rockentity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate, options );

                LavaAssert.Matches( expectedOutput, output, "*" );
            } );
        }

        private void AssertCreateEntitySetForEntityTypeParameter( string entityTypeParameter )
        {
            var inputTemplate = @"
{%- assign entitySet = '$personId' | CreateEntitySet:'$entityType' -%}
{{ entitySet.Id }}
";
            // Require an Id integer value as output.
            var expectedOutput = "[0-9]+";

            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            inputTemplate = inputTemplate.Replace( "$personId", person.Id.ToString() )
                .Replace( "$entityType", entityTypeParameter );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                /*
                    9/26/26 - CLAUDE

                    The comment says an Id is required, and the expected value
                    is written as a pattern, but the predecessor call compared
                    it as a literal string. It is matched as a pattern here, as
                    the comment intends.

                    Reason: The expected value was always meant to be a pattern.
                */
                StringAssert.Matches( output.Trim(), new Regex( expectedOutput ) );
            } );
        }

        private List<Person> GetTestPersonEntityList()
        {
            var rockContext = RockApp.Current.CreateRockContext();
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
{%- assign entitySet = $input | CreateEntitySet:$args -%}
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

            var options = new LavaRenderOptions
            {
                EnabledCommands = "rockentity"
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


            // This helper reads the rendered identifier and then goes on to check
            // the entity set it refers to, so it renders once against a single
            // engine rather than iterating the active engines.
            var engine = LavaRenderTestHelper.CreateActiveEngines().First();

            var output = LavaRenderTestHelper.Render( engine, inputTemplate, options );

            var entitySetId = output.ConvertToIntegerOrThrow();

            // Get the entity set, including the items. 
            var rockContext = RockApp.Current.CreateRockContext();
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
            Assert.HasCount( personList.Count(), entitySet.Items );

            return entitySet;
        }

        #endregion
    }
}
