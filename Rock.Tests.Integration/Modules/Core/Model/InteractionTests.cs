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
using Rock.Tests.Integration.Data.Interactions;

namespace Rock.Tests.Integration.Reporting
{
    [TestClass]
    public class InteractionTests
    {
        private string interactionForeignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            TestDatabaseHelper.ResetDatabase();

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
        [Ignore("Fix required. Adding an interaction with a new Device Type in the same action causes an exception. [Last modified by MB]")]
        public void InteractionDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );

            var interaction = BuildInteraction( Convert.ToDateTime( "2010-3-15" ) );

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
        [Ignore( "Fix required. Adding an interaction with a new Device Type in the same action causes an exception. [Last modified by MB]" )]
        public void InteractionDateKeyJoinsCorrectly()
        {
            var expectedRecordCount = 15;
            var year = 2015;
            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {
                    var interaction = BuildInteraction( TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );
                    interactionService.Add( interaction );
                }

                rockContext.SaveChanges();
            }

            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );
                var interactions = interactionService.
                                Queryable().
                                Where( i => i.ForeignKey == interactionForeignKey ).
                                Where( i => i.InteractionSourceDate.CalendarYear == year );

                Assert.AreEqual( expectedRecordCount, interactions.Count() );
                Assert.IsNotNull( interactions.First().InteractionSourceDate );
            }
        }

        private Rock.Model.Interaction BuildInteraction( DateTime interactionDate )
        {
            var args = new CreatePageViewInteractionActionArgs
            {
                PageIdentifier = SystemGuid.Page.EXCEPTION_LIST,
                ForeignKey = interactionForeignKey,
                ViewDateTime = interactionDate
            };

            var interaction = InteractionsDataManager.Instance.CreatePageViewInteraction( args );
            return interaction;
        }

        private void CleanUpData( string interactionForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [Interaction] WHERE [ForeignKey] = '{interactionForeignKey}'" );
        }
    }
}
