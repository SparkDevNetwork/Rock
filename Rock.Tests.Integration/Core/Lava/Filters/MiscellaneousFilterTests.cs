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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core.Lava.Filters
{
    /// <summary>
    /// Tests for Lava Filters categorized as "Miscellaneous".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class MiscellaneousFilterTests : LavaIntegrationTestBase
    {
        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            var rockContext = RockApp.Current.CreateRockContext();

            // Add Followings for Ted Decker.
            var followingService = new FollowingService( rockContext );

            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var benJonesGuid = TestGuids.TestPeople.BenJones.AsGuid();
            var billMarbleGuid = TestGuids.TestPeople.BillMarble.AsGuid();

            var personService = new PersonService( rockContext );

            var tedDeckerAliasId = personService.Get( tedDeckerGuid ).PrimaryAliasId.GetValueOrDefault();

            var personEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.PERSON ).GetValueOrDefault();

            followingService.GetOrAddFollowing( personEntityTypeId, personService.Get( benJonesGuid ).Id, tedDeckerAliasId, null );
            followingService.GetOrAddFollowing( personEntityTypeId, personService.Get( billMarbleGuid ).Id, tedDeckerAliasId, null );

            rockContext.SaveChanges();

            // Add a Persisted Dataset containing some test people.
            PersistedDatasetDataManager.Instance.AddPersistedDatasetForPersonBasicInfo( "99FF9EFA-D9E3-48DE-AD08-C67389FF688F".AsGuid(),
                "persons" );
        }

        #region Debug

        [TestMethod]
        public void DebugFilter_WithLavaParameter_ReturnsDebugInfo()
        {
            var template = "{{ 'Lava' | Debug }}";

            // The filter writes a table of every available merge field, so this
            // checks for one known row rather than the whole document.
            var expectedFragment = "<li><span class='lava-debug-key'>OrganizationName</span> <span class='lava-debug-value'> - Rock Solid Church</span></li>";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.Contains( expectedFragment, output );
            } );
        }

        #endregion

        #region LavaLibraryTestFilters

        /// <summary>
        /// Registering a filter with an invalid parameter type correctly throws a Lava exception.
        /// </summary>
        [TestMethod]
        [Ignore( "The restriction on parameter types for Fluid has been removed." )]
        public void Fluid_MismatchedFilterParameters_ShowsCorrectErrorMessage()
        {
            var inputTemplate = @"
{{ '1' | AppendValue:'2' }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                // Filters are registered against the engine this test was given,
                // which is built for this test alone, so the registrations below
                // cannot affect any other test.
                var filterMethodValid = typeof( TestLavaLibraryFilter ).GetMethod( "AppendString", new Type[] { typeof( object ), typeof( string ) } );
                var filterMethodInvalid = typeof( TestLavaLibraryFilter ).GetMethod( "AppendGuid", new Type[] { typeof( object ), typeof( Guid ) } );

                engine.RegisterFilter( filterMethodValid, "AppendValue" );

                // This should render correctly.
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                /*
                    9/26/26 - CLAUDE

                    This test is ignored, so the exact output it produces cannot
                    be observed. The whitespace-insensitive comparison the
                    predecessor helper applied is kept rather than replaced with
                    an exact expectation that nobody can verify.

                    Reason: Do not invent an expected value for a test that
                    cannot be run.
                */
                Assert.That.AreEqualIgnoreWhitespace( "12", output );

                // This should throw an exception when attempting to render a template containing the invalid filter.
                engine.RegisterFilter( filterMethodInvalid, "AppendValue" );

                var result = engine.RenderTemplate( inputTemplate, new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

                Assert.Contains( "Parameter type 'Guid' is not supported", result.Error?.Messages().JoinStrings( "//" ) );
            } );
        }

        public static class TestLavaLibraryFilter
        {
            public static string AppendString( object input, string input1 )
            {
                return input.ToString() + input1.ToString();
            }
            public static string AppendGuid( object input, Guid input1 )
            {
                return input.ToString() + input1.ToString();
            }
        }

        #endregion

        #region ZebraPhoto

        [TestMethod]
        public void ZebraPhoto_WithParameters_ReturnsEncodedImageInfo()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { MergeFields = values };

            var template = "{{ CurrentPerson | ZebraPhoto:'397',1.0,1.0,'LOGO',90 }}";

            // Because we are using Windows system calls to generate the PNG
            // data, the data changes for different Windows versions. So instead
            // of checking the actual data, just make sure it rendered the
            // minimal amount of information to satisfy us that it didn't error.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.StartsWith( "^FS ~DYR:LOGO,P,P,", output );
            } );
        }

        #endregion

        private LavaDataDictionary AddPersonTedDeckerToMergeDictionary( LavaDataDictionary dictionary = null, string mergeKey = "CurrentPerson" )
        {
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = RockApp.Current.CreateRockContext();

            var tedDeckerPerson = new PersonService( rockContext ).Queryable().First( x => x.Guid == tedDeckerGuid );

            if ( dictionary == null )
            {
                dictionary = new LavaDataDictionary();
            }

            dictionary.AddOrReplace( mergeKey, tedDeckerPerson );

            return dictionary;
        }

        #region AppendFollowing

        [TestMethod]
        public void AppendFollowing_DocumentationExample_ProducesExpectedResult()
        {
            var options = new LavaRenderOptions { EnabledCommands = "rockentity" };

            var template = @"
<p>Entity Command Example</p>
{%- person where:'LastName == ""Decker""' limit:'2' iterator:'People' -%}
  {%- assign followedItems = People | AppendFollowing -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>
{%- endperson -%}
";
            // The whitespace control on the block and loop tags trims every
            // newline inside them, so the list renders as one unbroken line.
            var expectedOutput = "\n<p>Entity Command Example</p><ul><li>Ted Decker - false</li><li>Cindy Decker - false</li></ul>\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void AppendFollowing_WhereCurrentUserHasFollowings_ShowsCorrectFollowingStatus()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

            var template = @"
{%- person where:'Guid == ""<benJonesGuid>"" || Guid == ""<billMarbleGuid>"" || Guid == ""<alishaMarbleGuid>""' iterator:'People' -%}
  {%- assign followedItems = People | AppendFollowing | Sort:'FullName' -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>
{%- endperson -%}
";

            template = template.Replace( "<benJonesGuid>", TestGuids.TestPeople.BenJones )
                .Replace( "<billMarbleGuid>", TestGuids.TestPeople.BillMarble )
                .Replace( "<alishaMarbleGuid>", TestGuids.TestPeople.AlishaMarble );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.Contains( "<li>Alisha Marble - false</li>", output );
                Assert.Contains( "<li>Ben Jones - true</li>", output );
                Assert.Contains( "<li>Bill Marble - true</li>", output );
            } );
        }

        [TestMethod]
        public void AppendFollowing_ForPersistedDataset_ShowsCorrectFollowingStatus()
        {
            var template = @"
{%- assign followedItems = 'persons' | PersistedDataset | AppendFollowing | Sort:'FullName' -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>

";

            // The assign tag leaves behind the newline that followed it, and the
            // template ends with a blank line of its own.
            var expectedOutput = "<ul><li>Alisha Marble - false</li><li>Ben Jones - true</li><li>Bill Marble - true</li></ul>\n"
                + "\n";

            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion

        #region FilterFollowed

        [TestMethod]
        public void FilterFollowed_ForEntityCollectionWithFollowedAndUnfollowed_ReturnsOnlyFollowed()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

            var template = @"
{%- person where:'Guid == ""<benJonesGuid>"" || Guid == ""<billMarbleGuid>"" || Guid == ""<alishaMarbleGuid>""' iterator:'People' -%}
  {%- assign followedItems = People | AppendFollowing | FilterFollowed | Sort:'FullName' -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>
{%- endperson -%}
";
            template = template.Replace( "<benJonesGuid>", TestGuids.TestPeople.BenJones )
                .Replace( "<billMarbleGuid>", TestGuids.TestPeople.BillMarble )
                .Replace( "<alishaMarbleGuid>", TestGuids.TestPeople.AlishaMarble );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.Contains( "<li>Ben Jones - true</li>", output );
                Assert.Contains( "<li>Bill Marble - true</li>", output );

                // Alisha Marble should be excluded because she is not followed by Ted.
                Assert.DoesNotContain( "Alisha Marble", output );
            } );
        }

        #endregion

        #region FilterUnfollowed

        [TestMethod]
        public void FilterUnfollowed_ForEntityCollectionWithFollowedAndUnfollowed_ReturnsOnlyUnfollowed()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

            var template = @"
{%- person where:'Guid == ""<benJonesGuid>"" || Guid == ""<billMarbleGuid>"" || Guid == ""<alishaMarbleGuid>""' iterator:'People' -%}
  {%- assign followedItems = People | AppendFollowing | FilterUnfollowed | Sort:'FullName' -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>
{%- endperson -%}
";
            template = template.Replace( "<benJonesGuid>", TestGuids.TestPeople.BenJones )
                .Replace( "<billMarbleGuid>", TestGuids.TestPeople.BillMarble )
                .Replace( "<alishaMarbleGuid>", TestGuids.TestPeople.AlishaMarble );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                // Alisha Marble should be the only one listed, because she is
                // the only one Ted does not follow.
                Assert.Contains( "<li>Alisha Marble - false</li>", output );
                Assert.DoesNotContain( "<li>Ben Jones - true</li>", output );
                Assert.DoesNotContain( "<li>Bill Marble - true</li>", output );
            } );
        }

        #endregion

        #region FromCache

        /// <summary>
        /// Verify the documentation example for this filter.
        /// </summary>
        [TestMethod]
        [Ignore( "Test needs to add Stepping Stone campus before running." )]
        public void FromCache_DocumentationExample_ReturnsExpectedOutput()
        {
            var template = @"
<h4>Current Person's Campus</h4>
{%- assign campus = CurrentPerson.PrimaryCampusId | FromCache:'Campus' -%}
Current Person's Campus Is: {{ campus.Name }}
{%- assign allCampuses = 'All' | FromCache:'Campus' | OrderBy:'Name' -%}
<h4>All Campuses</h4>
<ul>
{%- for c in allCampuses -%}
    {%- if c.Name == 'Main Campus' or c.Name == 'Stepping Stone' -%}
    <li>{{ c.Name }} </li>
    {%- endif -%}
{%- endfor -%}
</ul>
";

            var expectedOutput = @"
<h4>Current Person's Campus</h4>
Current Person's Campus Is: Main Campus
<h4>All Campuses</h4>
<ul>
    <li>Main Campus</li>
    <li>Stepping Stone</li>
</ul>
".NormalizeLineEndings();

            // Set CurrentPerson to Ted Decker.
            var mergeFields = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaRenderOptions { MergeFields = mergeFields };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                // This test is ignored, so its exact output cannot be observed.
                // The whitespace-insensitive comparison is kept rather than
                // replaced with an expectation nobody can verify.
                Assert.That.AreEqualIgnoreWhitespace( expectedOutput, output );
            } );
        }

        #endregion

        #region RunLavaFilter

        /// <summary>
        /// Verify the documentation example for this filter.
        /// </summary>
        [TestMethod]
        public void RunLavaFilter_DocumentationExample_ReturnsExpectedOutput()
        {
            var template = @"
{%- capture lava -%}{%- raw -%}{%- assign test = 'hello' -%}{{ test }}{%- endraw -%}{%- endcapture -%}
{{ lava | RunLava }}
";

            // The capture tag leaves behind the newline that followed it.
            var expectedOutput = "hello\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion
    }
}
