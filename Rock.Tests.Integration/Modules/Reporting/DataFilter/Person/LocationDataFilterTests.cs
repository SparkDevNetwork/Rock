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
using Rock.Data;
using Rock.Model;
using Rock.Reporting.DataFilter.Person;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Reporting.DataFilter
{
    /// <summary>
    /// Test DataFilter: Person/Location.
    /// </summary>
    [TestClass]
    public class LocationDataFilterTests : DataFilterTestBase
    {
        [TestMethod]
        public void LocationFilter_StreetAddress_ReturnsMatchingPeople()
        {
            var settings = new LocationFilter.FilterSettings
            {
                Street1 = "11624 N 31st Dr"
            };

            var personQuery = GetPersonQueryWithLocationFilter( settings );
            var results = personQuery.ToList();

            // Verify that only members of the family residing at this address are returned.
            Assert.That.IsTrue( results.Any( x => x.Guid == new Guid( TestGuids.TestPeople.TedDecker ) ) );
            Assert.That.IsTrue( results.Any( x => x.Guid == new Guid( TestGuids.TestPeople.CindyDecker ) ) );

            Assert.That.IsFalse( results.Any( x => x.Guid == new Guid( TestGuids.TestPeople.BillMarble ) ) );
        }

        /// <summary>
        /// Create a Person Query using the Location filter with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IQueryable<IEntity> GetPersonQueryWithLocationFilter( LocationFilter.FilterSettings settings )
        {
            var settingsFilter = new LocationFilter();
            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );
            var parameterExpression = personService.ParameterExpression;

            var predicate = settingsFilter.GetExpression( typeof( Rock.Model.Person ), personService, parameterExpression, settings.ToSelectionString() );
            var personQuery = GetFilteredEntityQuery<Person>( dataContext, predicate, parameterExpression );

            return personQuery;
        }
    }
}
