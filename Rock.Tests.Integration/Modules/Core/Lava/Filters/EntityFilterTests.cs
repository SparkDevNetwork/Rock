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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
{
    /// <summary>
    /// Tests for Lava Filters that apply to Rock Entities.
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class EntityFilterTests : LavaIntegrationTestBase
    {
        #region IsInDataView

        private const string DataViewNameAdultMembersAndAttendees = "Adult Member & Attendees";
        private const string DataViewNameAdultMembersAndAttendeesFemales = "Adult Member & Attendees > Females";

        [TestMethod]
        public void IsInDataView_WithEntityInDataView_ReturnsTrue()
        {
            // Ted Decker exists in the Data View because he is an adult member.
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.TedDecker}'", DataViewNameAdultMembersAndAttendees, "true" );
        }

        [TestMethod]
        public void IsInDataView_WithEntityNotInDataView_ReturnsFalse()
        {
            // Brian Jones does not exist in the Data View because he is not an adult.
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.BrianJones}'", DataViewNameAdultMembersAndAttendees, "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsEntityObject_CorrectlyIdentifiesTargetEntity()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"
{% assign inDataView = CurrentPerson | IsInDataView:'<dataViewRef>' %}
{{ inDataView }}";

            TestHelper.AssertTemplateOutput( "true",
                template.Replace( "<dataViewRef>", DataViewNameAdultMembersAndAttendees ),
                options );
            TestHelper.AssertTemplateOutput( "false",
                template.Replace( "<dataViewRef>", DataViewNameAdultMembersAndAttendeesFemales ),
                options );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsEntityId_CorrectlyIdentifiesTargetEntity()
        {
            var values = AddTestPersonToMergeDictionary( TestGuids.TestPeople.TedDecker.AsGuid() );
            var person = ( Person ) values["CurrentPerson"];

            // Verify input as integer.
            IsInDataView_AssertResult( person.Id.ToString(), DataViewNameAdultMembersAndAttendees, "true" );
            IsInDataView_AssertResult( person.Id.ToString(), DataViewNameAdultMembersAndAttendeesFemales, "false" );
            // Verify input as numeric string.
            IsInDataView_AssertResult( $"'{person.Id}'", DataViewNameAdultMembersAndAttendees, "true" );
            IsInDataView_AssertResult( $"'{person.Id}'", DataViewNameAdultMembersAndAttendeesFemales, "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsEntityGuid_CorrectlyIdentifiesTargetEntity()
        {
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.TedDecker}'", DataViewNameAdultMembersAndAttendees, "true" );
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.TedDecker}'", DataViewNameAdultMembersAndAttendeesFemales, "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsInvalidEntityReference_ReturnsFalse()
        {
            IsInDataView_AssertResult( "'#invalid_entity_reference#'", DataViewNameAdultMembersAndAttendees, "false" );
        }

        [TestMethod]
        public void IsInDataView_WithParameterDataViewAsGuid_CorrectlyIdentifiesTargetDataView()
        {
            var dataView = IsInDataView_GetTestDataView();
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.TedDecker}'", dataView.Guid.ToString(), "true" );
        }

        [TestMethod]
        public void IsInDataView_WithParameterDataViewAsId_CorrectlyIdentifiesTargetDataView()
        {
            var dataView = IsInDataView_GetTestDataView();
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.TedDecker}'", dataView.Id.ToString(), "true" );
        }

        [TestMethod]
        public void IsInDataView_WithParameterDataViewAsInvalidReference_ReturnsFalse()
        {
            // The Data View "#Invalid#" does not exist, so the filter should return False.
            IsInDataView_AssertResult( $"'{TestGuids.TestPeople.BrianJones}'", "#Invalid#", "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsUnmatchedEntityReference_ReturnsFalse()
        {
            // The Entity Reference does not match any existing entity, so the filter should return False.
            IsInDataView_AssertResult( $"'{TestGuids.NoMatch}'", DataViewNameAdultMembersAndAttendees, "false" );
        }

        private DataView IsInDataView_GetTestDataView()
        {
            var dataView = new DataViewService( new RockContext() ).Queryable()
                .Where( dv => dv.Name == DataViewNameAdultMembersAndAttendees )
                .FirstOrDefault();

            Assert.IsNotNull( dataView, "Test DataView not found." );
            return dataView;
        }

        private void IsInDataView_AssertResult( string inputValue, string dataViewRef, string expectedOutput )
        {
            var template = @"
{% assign inDataView = <entityRef> | IsInDataView:'<dataViewRef>' %}
{{ inDataView }}"
                .Replace( "<entityRef>", inputValue )
                .Replace( "<dataViewRef>", dataViewRef );

            TestHelper.AssertTemplateOutput( expectedOutput,
                template );
        }

        #endregion

        #region GuidToId

        [TestMethod]
        public void GuidToId_DocumentationExample1_ReturnsExpectedOutput()
        {
            var tedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var template = @"
{% assign tedDeckerGuid = '<personGuid>' %}
{% assign personEntityTypeId = '<personEntityTypeGuid>' | GuidToId:'EntityType' %}
{% assign tedDeckerId = tedDeckerGuid | GuidToId:personEntityTypeId %}
Ted Decker's record can be identified by Guid '{{ tedDeckerGuid }}' or Id '{{ tedDeckerId }}'.
"
                .Replace( "<personGuid>", tedDecker.Guid.ToString() )
                .Replace( "<personEntityTypeGuid>", SystemGuid.EntityType.PERSON );
            var expectedOutput = @"
Ted Decker's record can be identified by Guid '$tedDeckerGuid' or Id '$tedDeckerId'.
"
                .Replace( "$tedDeckerGuid", tedDecker.Guid.ToString() )
                .Replace( "$tedDeckerId", tedDecker.Id.ToString() );

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                template );
        }

        [TestMethod]
        public void GuidToId_WithInvalidInput_ReturnsErrorMessage()
        {
            var template = @"
{% assign personEntityTypeId = '#invalid#' | GuidToId:'EntityType' %}
";
            var expectedOutput = "Lava Error: Invalid Input Guid Value.";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                template );
        }

        [TestMethod]
        public void GuidToId_WithSingleValueInput_ReturnsGuidValue()
        {
            var entityTypeMap = GetEntityTypeGuidToIdMap();
            var entityTypeGuid = entityTypeMap.Keys.First();
            var template = @"
{{ '$entityTypeGuid' | GuidToId:'EntityType' }}
"
                .Replace( "$entityTypeGuid", entityTypeGuid );
            var expectedOutput = entityTypeMap.Values.First();

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                template );
        }

        [TestMethod]
        public void GuidToId_WithDelimitedListInput_ReturnsGuidCollection()
        {
            var entityTypeMap = GetEntityTypeGuidToIdMap();
            var entityTypeGuidList = entityTypeMap.Keys.JoinStrings( "," );
            var template = @"
{% assign entityTypeIdList = '$guidList' | GuidToId:'EntityType' %}
{{ entityTypeIdList | Join:',' }}
"
                .Replace( "$guidList", entityTypeGuidList );
            var expectedOutput = entityTypeMap.Values.JoinStrings( "," );

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                template );
        }

        [TestMethod]
        public void GuidToId_WithArrayInput_ReturnsGuidCollection()
        {
            var entityTypeMap = GetEntityTypeGuidToIdMap();
            var template = @"
{% assign entityTypeIdList = EntityTypeGuidList | GuidToId:'EntityType' %}
{{ entityTypeIdList | Join:',' }}
";
            var expectedOutput = entityTypeMap.Values.JoinStrings( "," );

            var options = new LavaTestRenderOptions
            {
                MergeFields = LavaDataDictionary.FromKeyValue( "EntityTypeGuidList", entityTypeMap.Keys )
            };

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                template,
                options );
        }

        [TestMethod]
        public void GuidToId_WithInvalidEntityId_ReturnsErrorMessage()
        {
            var template = @"
{% assign entityTypeId = '94FF79FE-4BB0-4F9E-AD74-14766433FC06' | GuidToId:'#InvalidEntityType#' %}
";
            var expectedOutput = @"
LavaError: Invalid EntityType. [entityType=""#InvalidEntityType#""]
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput,
                template );
        }

        private Dictionary<string, string> GetEntityTypeGuidToIdMap()
        {
            var entityTypeMap = new Dictionary<string, string>()
            {
                { SystemGuid.EntityType.BLOCK, null },
                { SystemGuid.EntityType.PAGE, null },
                { SystemGuid.EntityType.SITE, null },
            };

            foreach ( var entityTypeGuid in entityTypeMap.Keys.ToList() )
            {
                entityTypeMap[entityTypeGuid] = EntityTypeCache.GetId( entityTypeGuid ).ToString();
            }

            return entityTypeMap;
        }

        #endregion

        private LavaDataDictionary AddTestPersonToMergeDictionary( Guid personGuid, LavaDataDictionary dictionary = null, string mergeKey = "CurrentPerson" )
        {
            var rockContext = new RockContext();
            var tedDeckerPerson = new PersonService( rockContext ).Queryable()
                .First( x => x.Guid == personGuid );

            if ( dictionary == null )
            {
                dictionary = new LavaDataDictionary();
            }
            dictionary.AddOrReplace( mergeKey, tedDeckerPerson );

            return dictionary;
        }
    }
}
