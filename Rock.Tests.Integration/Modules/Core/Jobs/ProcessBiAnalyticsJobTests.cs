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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    [TestClass]
    public class ProcessBiAnalyticsJobTests : DatabaseTestsBase
    {
        [TestMethod]
        public void ProcessBiAnalytics_WithTestParameters_CanExecute()
        {
            // Run the Process BI Analytics Job to populate test data.
            TestHelper.Log( $"Started Job: ProcessBiAnalytics..." );

            var jobAnalytics = new Rock.Jobs.ProcessBIAnalytics();

            var analyticsSettings = new Dictionary<string, string>();
            analyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessPersonBIAnalytics, "true" );
            analyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessFamilyBIAnalytics, "true" );
            analyticsSettings.AddOrReplace( Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessAttendanceBIAnalytics, "true" );

            jobAnalytics.ExecuteInternal( analyticsSettings );

            var result = jobAnalytics.Result;
            Assert.That.Contains( jobAnalytics.Result, "completed" );

            TestHelper.Log( $"Finished Job: ProcessBiAnalytics." );
        }
    }
}
