﻿// <copyright>
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
using Rock.Model;
using Rock.Reporting.DataFilter.Person;
using Rock.Tests.Integration.Crm.Steps;

namespace Rock.Tests.Integration.Reporting.DataFilter
{
    /// <summary>
    /// Test DataFilter: Person/StepsTaken.
    /// </summary>
    [TestClass]
    public class StepsTakenDataFilterTests : DataFilterTestBase
    {
        private const string _TestCategory = "Rock.Crm.Steps.Reporting.StepsTakenDataFilter.Tests";

        /// <summary>
        /// Verify that the settings can be correctly serialized to a string and deserialized from the same string.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void SettingsSerializationCanRoundTrip()
        {
            var startPeriod = new TimePeriod( new DateTime( 2019, 2, 1 ), new DateTime( 2019, 3, 1 ) );
            var endPeriod = new TimePeriod( new DateTime( 2019, 4, 1 ), new DateTime( 2019, 5, 1 ) );

            var settingsSource = new StepsTakenFilter.FilterSettings();

            settingsSource.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;

            settingsSource.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeBaptismGuid, StepsTests.Constants.StepTypeConfirmationGuid, StepsTests.Constants.StepTypeConfessionGuid };
            settingsSource.StepStatusGuids = new List<Guid> { StepsTests.Constants.StatusSacramentsSuccessGuid, StepsTests.Constants.StatusSacramentsPendingGuid };

            settingsSource.StartedInPeriod = startPeriod;
            settingsSource.CompletedInPeriod = endPeriod;

            var settingsString = settingsSource.ToSelectionString();

            var settingsTarget = new StepsTakenFilter.FilterSettings( settingsString );

            Assert.AreEqual( StepsTests.Constants.ProgramSacramentsGuid, settingsTarget.StepProgramGuid );

            Assert.AreEqual( StepsTests.Constants.StepTypeBaptismGuid, settingsTarget.StepTypeGuids[0] );
            Assert.AreEqual( StepsTests.Constants.StepTypeConfirmationGuid, settingsTarget.StepTypeGuids[1] );
            Assert.AreEqual( StepsTests.Constants.StepTypeConfessionGuid, settingsTarget.StepTypeGuids[2] );

            Assert.AreEqual( StepsTests.Constants.StatusSacramentsSuccessGuid, settingsTarget.StepStatusGuids[0] );
            Assert.AreEqual( StepsTests.Constants.StatusSacramentsPendingGuid, settingsTarget.StepStatusGuids[1] );

            Assert.AreEqual( startPeriod, settingsTarget.StartedInPeriod );
            Assert.AreEqual( endPeriod, settingsTarget.CompletedInPeriod );
        }

        /// <summary>
        /// Verify that filtering by Step Program only returns the complete set of participants in any of the associated Step Types.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterByProgramShouldReturnAllStepTypeParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for Program=Sacraments
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Ted Decker found - Baptism.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
            // Verify Sarah Simmons found - Confirmation.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.SarahSimmonsPersonGuid ) );
            // Verify Ben Jones not found - no Sacraments Steps, only Alpha.
            Assert.IsFalse( results.Any( x => x.Guid == StepsTests.Constants.BenJonesPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by a single Step Type returns only participants in that specific Step Type.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterBySingleStepTypeShouldReturnCorrectParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for Program=Sacraments, Step Type=Confirmation
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;

            settings.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeConfirmationGuid };

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Ted Decker found - Confirmation.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
            // Verify Sarah Simmons found - Confirmation.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.SarahSimmonsPersonGuid ) );
            // Verify Brian Jones not found - Baptism, no Confirmation.
            Assert.IsFalse( results.Any( x => x.Guid == StepsTests.Constants.BrianJonesPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by Step Program returns Steps associated with all Step Types.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterByMultipleStepTypesShouldReturnAnyParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for: Program=Sacraments, Step Type=Confirmation, Baptism
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;

            settings.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeBaptismGuid, StepsTests.Constants.StepTypeConfirmationGuid };

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Ted Decker found - Confirmation.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
            // Verify Sarah Simmons found - Confirmation.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.SarahSimmonsPersonGuid ) );
            // Verify Brian Jones found - Baptism.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.BrianJonesPersonGuid ) );
            // Verify Ben Jones not found - no Sacraments Steps
            Assert.IsFalse( results.Any( x => x.Guid == StepsTests.Constants.BenJonesPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by Step Program returns Steps associated with all Step Types.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterBySingleStatusShouldReturnCorrectParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for Step Program=Sacraments, Step Type=Baptism, Status=Pending
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;
            settings.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeBaptismGuid };
            settings.StepStatusGuids = new List<Guid> { StepsTests.Constants.StatusSacramentsPendingGuid };

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Brian Jones found - Baptism is Pending.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.BrianJonesPersonGuid ) );
            // Verify Ted Decker not found - Baptism is Completed.
            Assert.IsFalse( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by Step Program returns Steps associated with all Step Types.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterByMultipleStatusesShouldReturnAnyParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for Step Program=Sacraments, Step Type=Baptism, Status=Pending,Success.
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;
            settings.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeBaptismGuid };
            settings.StepStatusGuids = new List<Guid> { StepsTests.Constants.StatusSacramentsPendingGuid, StepsTests.Constants.StatusSacramentsSuccessGuid };

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Brian Jones found - Baptism is Pending.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.BrianJonesPersonGuid ) );
            // Verify Ted Decker found - Baptism is Completed.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
        }

        /// <summary>
        /// Verify that filtering by Step Program returns Steps associated with all Step Types.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Steps )]
        public void FilterByDateCompletedShouldReturnCorrectParticipants()
        {
            var settings = new StepsTakenFilter.FilterSettings();

            // Filter for Step Program=Sacraments, Step Type=Baptism, Date Completed=2001
            settings.StepProgramGuid = StepsTests.Constants.ProgramSacramentsGuid;
            settings.StepTypeGuids = new List<Guid> { StepsTests.Constants.StepTypeBaptismGuid };
            settings.CompletedInPeriod = new TimePeriod( new DateTime( 2001, 7, 1 ), new DateTime( 2001, 12, 31 ) );

            var personQuery = GetPersonQueryWithStepsTakenFilter( settings );

            var results = personQuery.ToList();

            // Verify Bill Marble found - baptised in second half of 2001.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.BillMarblePersonGuid ) );
            // Verify Alisha Marble found - baptised in second half of 2001.
            Assert.IsTrue( results.Any( x => x.Guid == StepsTests.Constants.AlishaMarblePersonGuid ) );
            // Verify Ted Decker not found - baptised in first half of 2001.
            Assert.IsFalse( results.Any( x => x.Guid == StepsTests.Constants.TedDeckerPersonGuid ) );
        }

        /// <summary>
        /// Create a Person Query using the StepsTaken filter with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IQueryable<IEntity> GetPersonQueryWithStepsTakenFilter( StepsTakenFilter.FilterSettings settings )
        {
            var settingsFilter = new StepsTakenFilter();

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            var parameterExpression = personService.ParameterExpression;

            var predicate = settingsFilter.GetExpression( typeof( Rock.Model.Person ), personService, parameterExpression, settings.ToSelectionString() );

            var personQuery = GetFilteredEntityQuery<Person>( dataContext, predicate, parameterExpression );

            return personQuery;
        }
    }
}
