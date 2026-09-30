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

using Rock.Mobile;

namespace Rock.Tests.Mobile
{
    /// <summary>
    /// Checks the rules on the platform mobile app builder's ladder that can be checked
    /// without a database. The forward-only rule's other half, "nothing added at or below the
    /// highest shipped Rock version", cannot be tested from one build and is enforced at
    /// code review.
    /// </summary>
    [TestClass]
    public class PlatformMobileAppBuilderTests
    {
        [TestMethod]
        public void Ladder_HasAtLeastOneVersion()
        {
            Assert.IsTrue( PlatformMobileAppBuilder.LadderVersions.Any() );
        }

        [TestMethod]
        public void Ladder_IsStrictlyIncreasing()
        {
            var versions = PlatformMobileAppBuilder.LadderVersions;

            for ( var i = 1; i < versions.Count; i++ )
            {
                Assert.IsTrue( versions[i] > versions[i - 1], $"Ladder version {versions[i]} must be above {versions[i - 1]}." );
            }
        }

        [TestMethod]
        public void Ladder_HasNoEntryAboveThisBuildsRockVersion()
        {
            var assemblyVersion = typeof( PlatformMobileAppBuilder ).Assembly.GetName().Version;
            var rockVersion = new Version( assemblyVersion.Major, assemblyVersion.Minor );

            foreach ( var version in PlatformMobileAppBuilder.LadderVersions )
            {
                Assert.IsTrue( version <= rockVersion, $"Ladder version {version} is above this build's Rock version {rockVersion}." );
            }
        }

        [TestMethod]
        public void Ladder_VersionsAreMajorMinorOnly()
        {
            foreach ( var version in PlatformMobileAppBuilder.LadderVersions )
            {
                Assert.AreEqual( -1, version.Build, $"Ladder version {version} must be a Rock major.minor version such as 21.0." );
            }
        }

        [TestMethod]
        public void CurrentDefinitionVersion_IsTopOfLadder()
        {
            Assert.AreEqual( PlatformMobileAppBuilder.LadderVersions.Last(), PlatformMobileAppBuilder.CurrentDefinitionVersion );
        }
    }
}
