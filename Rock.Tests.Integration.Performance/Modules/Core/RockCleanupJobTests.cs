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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Performance.Core
{
    /// <summary>
    /// Tests that verify the performance of the Rock Cleanup Job.
    /// </summary>
    [TestClass]
    public class RockCleanupJobTests
    {
        [TestMethod]
        public void RockCleanupJob_CleanupInteractionSessions()
        {
            CalculateGroupRequirementsJob_MeasurePerformance();
        }

        private void CalculateGroupRequirementsJob_MeasurePerformance()
        {
            TestHelper.ExecuteWithTimer( "Rock Cleanup: Cleanup Interaction Sessions", () =>
            {
                var jobTests = new global::Rock.Tests.Integration.Core.Jobs.RockCleanupJobTests();

                jobTests.RockCleanup_CleanupInteractionSessions_RemovesSessionsWithNoInteractions();
            } );
        }
    }
}
