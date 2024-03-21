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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.Modules.Core.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.BugFixes
{
    /// <summary>
    /// Tests that verify specific bug fixes for a Rock version.
    /// </summary>
    /// <remarks>
    /// These tests are developed to verify bugs and fixes that are difficult or time-consuming to reproduce.
    /// They are only relevant to the Rock version in which the bug is fixed, and should be removed in subsequent versions.
    /// </remarks>
    /// 
    [TestClass]
    [RockObsolete( "1.15" )]
    public class BugFixVerificationTests_v15 : LavaIntegrationTestBase
    {
        /// <summary>
        /// Verifies that attempting to process an Interaction session having an empty Guid will ignore the invalid session and continue processing.
        /// </summary>
        [TestMethod]
        public void InteractionSessionPopulateLocation_WithInteractionSessionHavingEmptyGuid_ContinuesProcessing()
        {
            var testInteractionGuid1 = "415EAAFD-5E32-47EA-A174-06CB46A17C4C";
            var testInteractionGuid2 = "092FD380-84CB-46B7-94F8-FC8796E67AEB";

            // Create an interaction session with an empty Browser Sesion Guid.
            // The session can be created because direct SQL is used, but any attempt to update the record
            // results in a validation error from Entity Framework.
            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );

            var args = new TestDataHelper.Interactions.CreatePageViewInteractionActionArgs
            {
                Guid = testInteractionGuid1.AsGuid(),
                ForeignKey = "IntegrationTestData",
                ViewDateTime = RockDateTime.Now,
                SiteIdentifier = SystemGuid.Site.EXTERNAL_SITE,
                PageIdentifier = SystemGuid.Page.EXTERNAL_HOMEPAGE,
                UserAgentString = "test-agent",
                BrowserIpAddress = "1.1.1.1",
                BrowserSessionGuid = Guid.Empty,
                RequestUrl = "http://localhost:12345/page/1",
                UserPersonAliasId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).PrimaryAliasId
            };

            var interaction1 = TestDataHelper.Interactions.CreatePageViewInteraction( args, rockContext );
            interactionService.Add( interaction1 );

            // Create an interaction session with a valid Browser Session Guid.
            args.Guid = testInteractionGuid2.AsGuid();
            args.BrowserIpAddress = "1.1.1.2";
            args.BrowserSessionGuid = Guid.NewGuid();

            var interaction2 = TestDataHelper.Interactions.CreatePageViewInteraction( args, rockContext );
            interactionService.Add( interaction2 );

            rockContext.SaveChanges();

            var settings = new PopulateInteractionSessionData.PopulateInteractionSessionDataJobSettings();
            settings.IpAddressLookupIsDisabled = true;
            settings.MaxRecordsToProcessPerRun = 10;

            var job = new PopulateInteractionSessionData();
            var jobResult = job.Execute( settings );

            // Verify that the job executed without throwing an exception, but reported the ignored session.
            Assert.That.IsNull( jobResult.Exception, "Unexpected Job Exception reported." );

            var output = jobResult.OutputMessages.JoinStrings( "\n" );
            Assert.That.Contains( output, "Interaction Session with invalid Guid ignored" );
            Assert.That.Contains( output, "Updated Interaction Count And Session Duration for 1 interaction session" );
        }
    }
}
