using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class InteractionTests
    {
        private string interactionForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            interactionForeignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( interactionForeignKey );
        }

        [TestMethod]
        public void InteractionDateKeyGetsSetCorrectly()
        {
            var testList = TestDataHelper.GetAnalyticsSourceDateTestData();

            foreach ( var keyValue in testList )
            {
                Interaction interaction = new Interaction();
                interaction.InteractionDateTime = keyValue.Value;
                Assert.AreEqual( keyValue.Key, interaction.InteractionDateKey );
            }
        }

        [TestMethod]
        public void InteractionDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );

            var interaction = BuildInteraction( rockContext, Convert.ToDateTime( "3/15/2010" ) );

            interactionService.Add( interaction );
            rockContext.SaveChanges();

            var interactionId = interaction.Id;

            // We're bypassing the model because the model doesn't user the InteractionDateKey from the database,
            // but it still needs to be correct for inner joins to work correctly.
            var result = rockContext.Database.
                            SqlQuery<int>( $"SELECT InteractionDateKey FROM Interaction WHERE Id = {interactionId}" ).First();

            Assert.AreEqual( 20100315, result );
        }

        [TestMethod]
        public void InteractionDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            var rockContext = new RockContext();

            var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
            var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

            var interactionService = new InteractionService( rockContext );

            for ( var i = 0; i < 15; i++ )
            {
                var interaction = BuildInteraction( rockContext, TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );
                interactionService.Add( interaction );
            }

            rockContext.SaveChanges();

            var interactions = interactionService.
                                Queryable( "AnalyticsSourceDate" ).
                                Where( i => i.ForeignKey == interactionForeignKey ).
                                Where( i => i.InteractionSourceDate.CalendarYear == year );

            Assert.AreEqual( expectedRecordCount, interactions.Count() );
        }

        private Rock.Model.Interaction BuildInteraction( RockContext rockContext, DateTime interactionDate )
        {
            var interactionComponentId = ( new InteractionComponentService( rockContext ) ).Queryable().FirstOrDefault().Id;
            var interaction = new Interaction();
            interaction.InteractionComponentId = interactionComponentId;
            interaction.Operation = "Test";
            interaction.ForeignKey = interactionForeignKey;
            interaction.InteractionDateTime = interactionDate;

            return interaction;
        }

        private void CleanUpData( string interactionForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [Interaction] WHERE [ForeignKey] = '{interactionForeignKey}'" );
        }
    }
}
