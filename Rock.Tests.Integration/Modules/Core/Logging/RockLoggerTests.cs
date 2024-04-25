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
using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Logging;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Logging
{
    [TestClass]
    public class RockLoggerTests : DatabaseTestsBase
    {
        [TestMethod]
        [Ignore( "Test randomly fails, even when running by itself." )]
        public void ConfirmRockLoggerLogsCorrectly()
        {
            var originalLogLevel = RockLogLevel.All;
            var originalFileSize = 5;
            var originalFileCount = 10;
            var originalDomains = new List<string> { RockLogDomains.Other, RockLogDomains.Communications };

            RockLoggingHelpers.SaveRockLogConfiguration( originalDomains, originalLogLevel, originalFileSize, originalFileCount );

            var expectedMessage = $"Test {Guid.NewGuid()}";
            RockLogger.Log.Information( expectedMessage );
            RockLogger.Log.Close();
            Assert.That.FileContains( RockLogger.Log.LogConfiguration.LogPath, expectedMessage );
        }

        [TestCleanup]
        public void Cleanup()
        {
            var folder = System.IO.Path.GetDirectoryName( RockLogger.Log.LogConfiguration.LogPath );
            RockLoggingHelpers.DeleteFilesInFolder( folder );
        }
    }
}
