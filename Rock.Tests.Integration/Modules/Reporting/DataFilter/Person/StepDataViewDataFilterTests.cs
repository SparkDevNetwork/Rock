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

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter.Person
{
    /// <summary>
    /// Test DataFilter: Person/Related Data View/Step Data View.  
    [TestClass]
    public class StepDataViewDataFilterTests : DataFilterTestBase
    {
        private const string _TestCategory = "Rock.Crm.Steps.Reporting.StepDataView.Tests";

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            TestDataHelper.AddTestDataSet( TestDataHelper.DataSetIdentifiers.StepsSampleData );
        }

        /// <summary>
        /// Verify that the settings can be correctly serialized to a string and deserialized from the same string.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        public void StepsRelatedDataViewFilterSettingsSerializationCanRoundTrip()
        {
            var settingsSource = new StepDataViewFilter.FilterSettings();

            settingsSource.DataViewGuid = TestGuids.Steps.ProgramSacramentsGuid;
            
            var settingsString = settingsSource.ToSelectionString();

            var settingsTarget = new StepDataViewFilter.FilterSettings( settingsString );

            Assert.That.AreEqual( TestGuids.Steps.ProgramSacramentsGuid, settingsTarget.DataViewGuid );
        }

        /// <summary>
        /// Verify that filtering by a Step Data View correctly returns a Person having a Step referenced in that Data View.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        public void StepsRelatedDataViewFilterShouldReturnPersonWithRelatedSteps()
        {
            var settings = new StepDataViewFilter.FilterSettings();

            // Filter for DataView="Steps Completed in 2001";
            settings.DataViewGuid = TestGuids.Steps.DataViewStepsCompleted2001Guid;

            var personQuery = GetPersonQueryWithStepDataViewFilter( settings );

            var results = personQuery.ToList();

            // Verify Ted Decker found - Baptised in 2001.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.Steps.TedDeckerPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by a Step Data View does not return a Person who does not have a matching Step.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        public void StepsRelatedDataViewFilterShouldNotReturnPersonWithNoRelatedSteps()
        {
            var settings = new StepDataViewFilter.FilterSettings();

            // Filter for DataView="Steps Completed in 2001";
            settings.DataViewGuid = TestGuids.Steps.DataViewStepsCompleted2001Guid;

            var personQuery = GetPersonQueryWithStepDataViewFilter( settings );

            var results = personQuery.ToList();

            // Verify Ben Jones not found - Alpha Attendee in 2015.
            Assert.That.IsFalse( results.Any( x => x.Guid == TestGuids.Steps.BenJonesPersonGuid ) );
        }

        /// <summary>
        /// Create a Person Query using the StepDataViewFilter with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IQueryable<IEntity> GetPersonQueryWithStepDataViewFilter( StepDataViewFilter.FilterSettings settings )
        {
            var settingsFilter = new StepDataViewFilter();

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            var parameterExpression = personService.ParameterExpression;

            var predicate = settingsFilter.GetExpression( typeof( Rock.Model.Person ), personService, parameterExpression, settings.ToSelectionString() );

            var personQuery = GetFilteredEntityQuery<Rock.Model.Person>( dataContext, predicate, parameterExpression );

            return personQuery;
        }
    }
}
