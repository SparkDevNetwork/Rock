using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class StepTests : DatabaseTestsBase
    {
        private string stepForiegnKey;

        [TestInitialize]
        public void TestInitialize()
        {
            stepForiegnKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( stepForiegnKey );
        }

        [TestMethod]
        public void StepCompletedDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.CompletedDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.CompletedDateKey );
            }
        }

        [TestMethod]
        public void StepStartDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.StartDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.StartDateKey );
            }
        }

        [TestMethod]
        public void StepEndDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                var step = new Rock.Model.Step();
                step.EndDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, step.EndDateKey );
            }
        }

        [TestMethod]
        public void StepCompletedDateKeyWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.CompletedDateTime = null;
            Assert.IsNull( step.CompletedDateKey );
        }

        [TestMethod]
        public void StepStartDateKeyWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.StartDateTime = null;
            Assert.IsNull( step.StartDateKey );
        }

        [TestMethod]
        public void StepEndDateKeyGetsWorksWithNullValue()
        {
            var step = new Rock.Model.Step();
            step.EndDateTime = null;
            Assert.IsNull( step.EndDateKey );
        }


        [TestMethod]
        public void StepDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var stepService = new StepService( rockContext );

            var step = BuildStep( rockContext, Convert.ToDateTime( "2010-3-16" ),
                            Convert.ToDateTime( "2010-3-15" ),
                            Convert.ToDateTime( "2010-3-17" ) );
            stepService.Add( step );
            rockContext.SaveChanges();

            var stepId = step.Id;

            // We're bypassing the model because the model doesn't user the StepDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT CompletedDateKey FROM Step WHERE Id = {stepId}" ).First();

            Assert.AreEqual( 20100316, result );

            result = rockContext.Database.
                            SqlQuery<int>( $"SELECT StartDateKey FROM Step WHERE Id = {stepId}" ).First();

            Assert.AreEqual( 20100315, result );

            result = rockContext.Database.
                            SqlQuery<int>( $"SELECT EndDateKey FROM Step WHERE Id = {stepId}" ).First();

            Assert.AreEqual( 20100317, result );
        }

        [TestMethod]
        public void StepDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var startYear = 2015;
            var completedYear = 2016;
            var endYear = 2017;

            using ( var rockContext = new RockContext() )
            {
                var stepService = new StepService( rockContext );

                var minStartDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, startYear );
                var maxStartDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, startYear );

                var minCompletedDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, completedYear );
                var maxCompletedDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, completedYear );

                var minEndDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, endYear );
                var maxEndDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, endYear );

                for ( var i = 0; i < 15; i++ )
                {
                    var startDate = TestDataHelper.GetRandomDateInRange( minStartDateValue, maxStartDateValue );
                    var endDate = TestDataHelper.GetRandomDateInRange( minEndDateValue, maxEndDateValue );
                    var completedDate = TestDataHelper.GetRandomDateInRange( minCompletedDateValue, maxCompletedDateValue );

                    while ( endDate < startDate )
                    {
                        endDate = TestDataHelper.GetRandomDateInRange( minEndDateValue, maxEndDateValue );
                    }

                    while ( completedDate < startDate )
                    {
                        completedDate = TestDataHelper.GetRandomDateInRange( minCompletedDateValue, maxCompletedDateValue );
                    }

                    var step = BuildStep( rockContext, completedDate, startDate, endDate );

                    stepService.Add( step );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var stepService = new StepService( rockContext );

                var steps = stepService.
                                Queryable().
                                Where( i => i.ForeignKey == stepForiegnKey ).
                                Where( i => i.CompletedSourceDate.CalendarYear == completedYear );

                Assert.AreEqual( expectedRecordCount, steps.Count() );
                Assert.IsNotNull( steps.First().CompletedSourceDate );

                steps = stepService.
                                    Queryable().
                                    Where( i => i.ForeignKey == stepForiegnKey ).
                                    Where( i => i.StartSourceDate.CalendarYear == startYear );

                Assert.AreEqual( expectedRecordCount, steps.Count() );
                Assert.IsNotNull( steps.First().StartSourceDate );

                steps = stepService.
                                    Queryable().
                                    Where( i => i.ForeignKey == stepForiegnKey ).
                                    Where( i => i.EndSourceDate.CalendarYear == endYear );

                Assert.AreEqual( expectedRecordCount, steps.Count() );
                Assert.IsNotNull( steps.First().EndSourceDate );
            }
        }

        private Rock.Model.Step BuildStep( RockContext rockContext, DateTime completeDateTime, DateTime startDateTime, DateTime endDateTime )
        {
            var stepType = new StepTypeService( rockContext ).Queryable().First();
            var personAlias = new PersonAliasService( rockContext ).Queryable().First();

            var step = new Rock.Model.Step();

            step.StepTypeId = stepType.Id;
            step.PersonAliasId = personAlias.Id;
            step.ForeignKey = stepForiegnKey;
            step.CompletedDateTime = completeDateTime;
            step.StartDateTime = startDateTime;
            step.EndDateTime = endDateTime;

            return step;
        }

        private void CleanUpData( string stepForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE Step WHERE [ForeignKey] = '{stepForeignKey}'" );
        }
    }
}
