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
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Logging;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Logging
{
    [TestClass]
    public class RockSerilogReaderTests : DatabaseTestsBase
    {
        private readonly string LogFolder = $"\\logs\\{Guid.NewGuid()}";
        [TestCleanup]
        public void Cleanup()
        {
            RockLoggingHelpers.DeleteFilesInFolder( LogFolder );
        }

        [TestMethod]
        public void RockLogReaderShouldReturnZeroLogEntriesIfDirectoryDoesNotExists()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );
            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 0;
            var pageSize = 1000;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            Assert.That.AreEqual( 0, results.Count );
        }

        [TestMethod]
        public void RockLogReaderShouldReturnZeroLogEntriesIfNoLogFilesExist()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );

            logger.Information( "Test" );
            logger.Close();

            System.IO.File.Delete( config.LogPath );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 0;
            var pageSize = 1000;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            Assert.That.AreEqual( 0, results.Count );
        }

        [TestMethod]
        public void RockLogReaderShouldReturnCorrectRecordCount()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );

            var expectedLogs = CreateLogFiles( logger );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            Assert.That.AreEqual( expectedLogs.Count, rockReader.RecordCount );
        }

        [TestMethod]
        public void RockLogReaderShouldReturnLogEntriesInCorrectOrder()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );

            var expectedLogs = CreateLogFiles( logger );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 0;
            var pageSize = 1000;
            var nextPageIndex = currentPageIndex + pageSize;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            var lastIndex = expectedLogs.Count - 1;
            for ( var i = lastIndex; i >= 0; i-- )
            {
                if ( ( lastIndex - i ) >= nextPageIndex )
                {
                    currentPageIndex = nextPageIndex;
                    nextPageIndex = currentPageIndex + pageSize;
                    results = rockReader.GetEvents( currentPageIndex, pageSize );
                }

                var resultIndex = lastIndex - i - currentPageIndex;
                Assert.That.Contains( results[resultIndex].Message, expectedLogs[i] );
            }

        }

        [TestMethod]
        public void RockLogReaderShouldReturnNoResultsWithOutOfRangePageIndex()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );

            var expectedLogs = CreateLogFiles( logger );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 19000;
            var pageSize = 1000;
            var nextPageIndex = currentPageIndex + pageSize;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            Assert.That.AreEqual( 0, results.Count );
        }

        [TestMethod]
        public void RockLogReaderShouldReturnAllResultsWithMaxPlusOnePageSize()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );

            var expectedLogs = CreateLogFiles( logger );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 0;
            var pageSize = 19000;
            var nextPageIndex = currentPageIndex + pageSize;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            Assert.That.AreEqual( expectedLogs.Count, results.Count );
        }

        [TestMethod]
        public void RockLogReaderShouldHandleDomainCorrectly()
        {
            var config = new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                MaxFileSize = 1,
                NumberOfLogFiles = 3,
                DomainsToLog = new List<string> { "OTHER" },
                LogPath = $"{LogFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };

            var logger = ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );
            var expectedMessage = "This is a test.";
            var expectedDomain = RockLogDomains.Other;

            logger.Information( expectedMessage );

            var rockReader = ReflectionHelper.InstantiateInternalObject<IRockLogReader>( "Rock.Logging.RockSerilogReader", logger );

            var currentPageIndex = 0;
            var pageSize = 100;
            var nextPageIndex = currentPageIndex + pageSize;

            var results = rockReader.GetEvents( currentPageIndex, pageSize );
            Assert.That.AreEqual( 1, results.Count );
            Assert.That.AreEqual( expectedMessage, results[0].Message );
            Assert.That.AreEqual( expectedDomain, results[0].Domain );
        }

        private List<string> CreateLogFiles( IRockLogger logger )
        {
            var maxByteCount = logger.LogConfiguration.MaxFileSize * 1024 * 1024 * ( logger.LogConfiguration.NumberOfLogFiles - 1 );
            var currentByteCount = 0;
            var logRecordSize = Encoding.ASCII.GetByteCount( "{\"@t\":\"0000-00-00T00:00:00.0000000Z\",\"@mt\":\"{domain} Test - 00000000-0000-0000-0000-000000000000\",\"domain\":\"OTHER\"}" );
            var expectedLogs = new List<string>();

            while ( currentByteCount < maxByteCount )
            {
                var guid = Guid.NewGuid();
                var expectedLogMessage = $"Test - {guid}";
                logger.Information( RockLogDomains.Other, expectedLogMessage );
                expectedLogs.Add( guid.ToString() );
                currentByteCount += logRecordSize;
            }
            return expectedLogs;
        }
    }
}
