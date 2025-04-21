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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
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
            var rockContext = new RockContext();

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

            // Verify a portion of the expected output.
            TestHelper.AssertTemplateOutput( "<li><span class='lava-debug-key'>OrganizationName</span> <span class='lava-debug-value'> - Rock Solid Church</span></li>",
                template,
                new LavaTestRenderOptions { OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains } );
        }

        #endregion

        #region RockInstanceConfigFilter

        [TestMethod]
        public void RockInstanceConfigFilter_MachineName_RendersExpectedValue()
        {
            var template = "{{ 'MachineName' | RockInstanceConfig }}";
            var expectedValue = RockApp.Current.HostingSettings.MachineName;

            TestHelper.AssertTemplateOutput( expectedValue, template );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_ApplicationDirectory_RendersExpectedValue()
        {
            var template = "{{ 'ApplicationDirectory' | RockInstanceConfig }}";
            var expectedValue = RockApp.Current.HostingSettings.VirtualRootPath;

            TestHelper.AssertTemplateOutput( expectedValue, template );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_PhysicalDirectory_RendersExpectedValue()
        {
            var template = "{{ 'PhysicalDirectory' | RockInstanceConfig }}";
            var expectedValue = RockApp.Current.HostingSettings.WebRootPath;

            TestHelper.AssertTemplateOutput( expectedValue, template );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_IsClustered_RendersExpectedValue()
        {
            var template = "{{ 'IsClustered' | RockInstanceConfig }}";
            var expectedValue = WebFarm.RockWebFarm.IsEnabled().ToTrueFalse();

            TestHelper.AssertTemplateOutput( expectedValue, template, new LavaTestRenderOptions { IgnoreCase = true } );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_SystemDateTime_RendersExpectedValue()
        {
            var template = "{{ 'SystemDateTime' | RockInstanceConfig | Date:'yyyy-MM-dd HH:mm:ss' }}";
            var expectedValue = RockDateTime.SystemDateTime;

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( template );

                var actualDateTime = result.Text.AsDateTime();

                if ( actualDateTime == null )
                {
                    throw new System.Exception( $"Invalid DateTime - Output = \"{result.Text}\"" );
                }

                TestHelper.DebugWriteRenderResult( engine, template, result.Text );

                Assert.That.AreProximate( expectedValue, actualDateTime, new System.TimeSpan( 0, 0, 30 ) );
            } );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_LavaEngine_RendersExpectedValue()
        {
            var template = "{{ 'LavaEngine' | RockInstanceConfig }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( template );

                TestHelper.DebugWriteRenderResult( engine, template, result.Text );

                var expectedOutput = RockApp.Current.GetCurrentLavaEngineName();

                Assert.That.AreEqual( expectedOutput, result.Text );
            } );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_InvalidParameterName_RendersErrorMessage()
        {
            var template = "{{ 'unknown_setting' | RockInstanceConfig }}";
            var expectedOutput = "Configuration setting \"unknown_setting\" is not available.";

            TestHelper.AssertTemplateOutput( expectedOutput, template );
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
            if ( !LavaIntegrationTestHelper.FluidEngineIsEnabled )
            {
                Debug.Write( "The Fluid engine is not enabled for this test run." );
                return;
            }

            var inputTemplate = @"
{{ '1' | AppendValue:'2' }}
";

            var expectedOutput = @"12";

            var engine = LavaIntegrationTestHelper.GetEngineInstance( typeof( FluidEngine ) );

            // Filters are registered
            var filterMethodValid = typeof( TestLavaLibraryFilter ).GetMethod( "AppendString", new System.Type[] { typeof( object ), typeof( string ) } );
            var filterMethodInvalid = typeof( TestLavaLibraryFilter ).GetMethod( "AppendGuid", new System.Type[] { typeof( object ), typeof( Guid ) } );

            engine.RegisterFilter( filterMethodValid, "AppendValue" );

            // This should render correctly.
            TestHelper.AssertTemplateOutput( engine, expectedOutput, inputTemplate );

            // This should throw an exception when attempting to render a template containing the invalid filter.
            engine.RegisterFilter( filterMethodInvalid, "AppendValue" );

            var result = engine.RenderTemplate( inputTemplate, new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

            Assert.That.Contains( result.Error?.Messages().JoinStrings( "//" ), "Parameter type 'Guid' is not supported" );
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

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "{{ CurrentPerson | ZebraPhoto:'397',1.0,1.0,'LOGO',90 }}";

            // Because we are using Windows system calls to generate the PNG
            // data, the data changes for different Windows versions. So instead
            // of checking the actual data, just make sure it rendered the
            // minimal amount of information to satisfy us that it didn't error.
            TestHelper.Execute( template, options, actual =>
            {
                Assert.That.StartsWith( "^FS ~DYR:LOGO,P,P,", actual );
            } );
        }

        #endregion

        private LavaDataDictionary AddPersonTedDeckerToMergeDictionary( LavaDataDictionary dictionary = null, string mergeKey = "CurrentPerson" )
        {
            var personDecker = TestHelper.GetTestPersonTedDecker();

            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var rockContext = new RockContext();

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
            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity" };

            var template = @"
<p>Entity Command Example</p>
{%- person where:'Id != 1' limit:'3' iterator:'People' -%}
  {%- assign followedItems = People | AppendFollowing -%}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>
{%- endperson -%}
";
            var outputExpected = @"
<p>Entity Command Example</p>
<ul>
<li>Giver Anonymous - false</li>
<li>Ted Decker - false</li>
<li>Cindy Decker - false</li>
</ul>
";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void AppendFollowing_WhereCurrentUserHasFollowings_ShowsCorrectFollowingStatus()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

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

            var expectedOutputs = new List<string>()
            {
                "<li>Alisha Marble - false</li>",
                "<li>Ben Jones - true</li>",
                "<li>Bill Marble - true</li>"
            };

            TestHelper.AssertTemplateOutput( expectedOutputs,
                template,
                options );
        }

        [TestMethod]
        public void AppendFollowing_ForPersistedDataset_ShowsCorrectFollowingStatus()
        {
            var template = @"
{% assign followedItems = 'persons' | PersistedDataset | AppendFollowing | Sort:'FullName' %}
<ul>
  {%- for item in followedItems -%}
    <li>{{ item.FullName }} - {{ item.IsFollowing }}</li>
  {%- endfor -%}
</ul>

";

            var outputExpected = @"
<ul><li>Alisha Marble - false</li><li>Ben Jones - true</li><li>Bill Marble - true</li></ul>
";
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        #endregion

        #region FilterFollowed

        [TestMethod]
        public void FilterFollowed_ForEntityCollectionWithFollowedAndUnfollowed_ReturnsOnlyFollowed()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

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

            // Alisha Marble should be excluded because she is not followed by Ted.
            var expectedOutputs = new List<string>()
            {
                "<li>Ben Jones - true</li>",
                "<li>Bill Marble - true</li>"
            };

            TestHelper.AssertTemplateOutput( expectedOutputs,
                template,
                options );
        }

        #endregion

        #region FilterUnfollowed

        [TestMethod]
        public void FilterUnfollowed_ForEntityCollectionWithFollowedAndUnfollowed_ReturnsOnlyUnfollowed()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { EnabledCommands = "rockentity", MergeFields = values };

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

            // Alisha Marble should be excluded because she is not followed by Ted.
            var matches = new List<LavaTestOutputMatchRequirement>()
            {
                new LavaTestOutputMatchRequirement("<li>Alisha Marble - false</li>", LavaTestOutputMatchTypeSpecifier.Contains ),
                new LavaTestOutputMatchRequirement("<li>Ben Jones - true</li>", LavaTestOutputMatchTypeSpecifier.DoesNotContain ),
                new LavaTestOutputMatchRequirement("<li>Bill Marble - true</li>", LavaTestOutputMatchTypeSpecifier.DoesNotContain ),
            };

            TestHelper.AssertTemplateOutput( matches,
                template,
                options );
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
{% assign campus = CurrentPerson.PrimaryCampusId | FromCache:'Campus' %}
Current Person's Campus Is: {{ campus.Name }}
{% assign allCampuses = 'All' | FromCache:'Campus' | OrderBy:'Name' %}
<h4>All Campuses</h4>
<ul>
{% for c in allCampuses %}
    {% if c.Name == 'Main Campus' or c.Name == 'Stepping Stone' %}
    <li>{{ c.Name }} </li>
    {% endif %}
{% endfor %}
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
";

            // Set CurrentPerson to Ted Decker.
            var mergeFields = AddPersonTedDeckerToMergeDictionary();

            TestHelper.AssertTemplateOutput( expectedOutput, template, new LavaTestRenderOptions { MergeFields = mergeFields } );
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
{% capture lava %}{% raw %}{% assign test = 'hello' %}{{ test }}{% endraw %}{% endcapture %}
{{ lava | RunLava }}
";

            TestHelper.AssertTemplateOutput( "hello", template );
        }

        #endregion
    }
}
