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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

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
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.TedDecker }'", DataViewNameAdultMembersAndAttendees, "true" );
        }

        [TestMethod]
        public void IsInDataView_WithEntityNotInDataView_ReturnsFalse()
        {
            // Brian Jones does not exist in the Data View because he is not an adult.
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.BrianJones }'", DataViewNameAdultMembersAndAttendees, "false" );
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
            IsInDataView_AssertResult( $"'{ person.Id }'", DataViewNameAdultMembersAndAttendees, "true" );
            IsInDataView_AssertResult( $"'{ person.Id }'", DataViewNameAdultMembersAndAttendeesFemales, "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsEntityGuid_CorrectlyIdentifiesTargetEntity()
        {
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.TedDecker }'", DataViewNameAdultMembersAndAttendees, "true" );
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.TedDecker }'", DataViewNameAdultMembersAndAttendeesFemales, "false" );
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
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.TedDecker }'", dataView.Guid.ToString(), "true" );
        }

        [TestMethod]
        public void IsInDataView_WithParameterDataViewAsId_CorrectlyIdentifiesTargetDataView()
        {
            var dataView = IsInDataView_GetTestDataView();
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.TedDecker }'", dataView.Id.ToString(), "true" );
        }

        [TestMethod]
        public void IsInDataView_WithParameterDataViewAsInvalidReference_ReturnsFalse()
        {
            // The Data View "#Invalid#" does not exist, so the filter should return False.
            IsInDataView_AssertResult( $"'{ TestGuids.TestPeople.BrianJones }'", "#Invalid#", "false" );
        }

        [TestMethod]
        public void IsInDataView_WithInputAsUnmatchedEntityReference_ReturnsFalse()
        {
            // The Entity Reference does not match any existing entity, so the filter should return False.
            IsInDataView_AssertResult( $"'{ TestGuids.NoMatch }'", DataViewNameAdultMembersAndAttendees, "false" );
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
