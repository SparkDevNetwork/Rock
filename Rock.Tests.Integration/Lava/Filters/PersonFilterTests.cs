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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests for Lava Filters categorized as "Person".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class PersonFilterTests : LavaIntegrationTestBase
    {
        #region Address

        [TestMethod]
        public void PersonAddress_WithAddressTypeParameterOnly_ReturnsFullAddress()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "Home Address: {{ CurrentPerson | Address:'Home' }}";
            var outputExpected = @"Home Address: 11624 N 31st Dr Phoenix, AZ 85029-3202";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonAddress_WithFormatTemplateFieldCityState_ReturnsExpectedOutput()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = "{{ CurrentPerson | Address:'Home','[[City]], [[State]]' }}";
            var outputExpected = @"Phoenix, AZ";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void PersonAddress_WithFormatTemplateFieldGuid_ReturnsLocationGuid()
        {
            var values = AddPersonTedDeckerToMergeDictionary();

            var person = values["CurrentPerson"] as Person;

            var options = new LavaTestRenderOptions { MergeFields = values };

            var template = @"{{ CurrentPerson | Address:'Home','[[Guid]]' }}";

            var outputExpected = person.GetHomeLocation()?.Guid.ToString( "D" );

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
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

    }
}
