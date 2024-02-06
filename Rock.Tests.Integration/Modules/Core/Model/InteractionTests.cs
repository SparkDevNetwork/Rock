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
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class InteractionTests : DatabaseTestsBase
    {
        private readonly string interactionForeignKey = $"Test {Guid.NewGuid()}";

        [TestCleanup]
        public void TestCleanup()
        {
            CleanUpData( interactionForeignKey );
        }

        [TestMethod]
        public void InteractionDateKeySavesCorrectly()
        {
            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );

            var interaction = BuildInteraction( rockContext, Convert.ToDateTime( "2010-3-15" ) );

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
            using ( var rockContext = new RockContext() )
            {
                var interactionService = new InteractionService( rockContext );

                var minDateValue = TestDataHelper.GetAnalyticsSourceMinDateForYear( rockContext, year );
                var maxDateValue = TestDataHelper.GetAnalyticsSourceMaxDateForYear( rockContext, year );

                for ( var i = 0; i < 15; i++ )
                {
                    var interaction = BuildInteraction( rockContext, TestDataHelper.GetRandomDateInRange( minDateValue, maxDateValue ) );
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

        private Interaction BuildInteraction( RockContext rockContext, DateTime interactionDate )
        {
            var args = new TestDataHelper.Interactions.CreatePageViewInteractionActionArgs
            {
                PageIdentifier = SystemGuid.Page.EXCEPTION_LIST,
                ForeignKey = interactionForeignKey,
                ViewDateTime = interactionDate,
                BrowserIpAddress = "127.0.0.1"
            };

            var interaction = TestDataHelper.Interactions.CreatePageViewInteraction( args, rockContext );
            return interaction;
        }

        private void CleanUpData( string interactionForeignKey )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( $"DELETE [Interaction] WHERE [ForeignKey] = '{interactionForeignKey}'" );
        }
    }
}
