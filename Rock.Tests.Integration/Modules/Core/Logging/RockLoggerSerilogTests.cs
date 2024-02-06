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
    public class RockLoggerSerilogTests : DatabaseTestsBase
    {
        private const string TEST_EXCEPTION_SERIALIZATION = "System.Exception: Test Exception";

        private readonly Exception TestException = new Exception( "Test Exception" );
        private readonly RockLogConfiguration ObjectToLog;
        private readonly string LogFolder = $"\\logs\\{Guid.NewGuid()}";

        public RockLoggerSerilogTests()
        {
            ObjectToLog = GetTestObjectToSerialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            RockLoggingHelpers.DeleteFilesInFolder( LogFolder );
        }

        [TestMethod]
        public void LoggerShouldContinueToWorkEvenIfClosed()
        {
            var logger = GetTestLogger();

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            logger.Close();

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            // The lack of an assert is valid here because if it runs without an exception everything is fine.
            // The logger.Close() calls dispose on the actual logger object and we want to make sure the class re-initializes it as necessary.

            logger.Close();
        }

        [TestMethod]
        public void LoggerDeleteShouldRemoveFiles()
        {
            var logger = GetTestLogger();

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            logger.Close();

            Assert.That.AreEqual( logger.LogConfiguration.NumberOfLogFiles, logger.LogFiles.Count );

            logger.Delete();

            Assert.That.AreEqual( 0, logger.LogFiles.Count );
        }

        [TestMethod]
        public void LoggerShouldKeepOnlyMaxFileCount()
        {
            var logger = GetTestLogger();

            TestContext.WriteLine( "Running LoggerShouldKeepOnlyMaxFileCount MaxFileSize: {0}; MaxFileCount: {1}", logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles );
            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            logger.Close();

            var logFolderPath = System.IO.Path.GetFullPath( System.IO.Path.GetDirectoryName( logger.LogConfiguration.LogPath ) );

            Assert.That.FolderHasCorrectNumberOfFiles( logFolderPath, logger.LogConfiguration.NumberOfLogFiles );
        }

        [TestMethod]
        public void LoggerShouldUpdateMaxFileCountAutomatically()
        {
            var originalLogFolder = $"{LogFolder}\\{System.Guid.NewGuid()}";
            var originalLogCount = 5;

            var expectedLogFolder = $"{LogFolder}\\{System.Guid.NewGuid()}";
            var expectedLogCount = 3;

            var logger = GetTestLogger( logFolder: originalLogFolder, numberOfLogFiles: originalLogCount );

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            logger.LogConfiguration.LogPath = $"{expectedLogFolder}\\{Guid.NewGuid()}.log";
            logger.LogConfiguration.NumberOfLogFiles = expectedLogCount;
            logger.LogConfiguration.LastUpdated = RockDateTime.Now;

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles + 2 );

            logger.Close();

            var logFolderPath = System.IO.Path.GetFullPath( System.IO.Path.GetDirectoryName( logger.LogConfiguration.LogPath ) );

            Assert.That.FolderHasCorrectNumberOfFiles( logFolderPath, logger.LogConfiguration.NumberOfLogFiles );
        }

        [TestMethod]
        public void LoggerLogFileSizeShouldBeWithinRange()
        {
            var logger = GetTestLogger( numberOfLogFiles: 5, logSize: 5 );
            var expectedMaxFileSize = logger.LogConfiguration.MaxFileSize * 1024 * 1024;
            var onePercentVariation = .01;

            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles );

            logger.Close();

            var logFolderPath = System.IO.Path.GetFullPath( System.IO.Path.GetDirectoryName( logger.LogConfiguration.LogPath ) );

            Assert.That.FolderFileSizeIsWithinRange( logFolderPath, 0, expectedMaxFileSize, onePercentVariation );
        }

        [TestMethod]
        public void LoggerShouldUpdateFileSizeAutomatically()
        {
            var originalLogFolder = $"{LogFolder}\\{System.Guid.NewGuid()}";
            var originalLogSize = 5;

            var expectedLogFolder = $"{LogFolder}\\{System.Guid.NewGuid()}";
            var expectedLogSize = 3;

            var logger = GetTestLogger( logFolder: originalLogFolder, numberOfLogFiles: 10, logSize: originalLogSize );
            var expectedMaxFileSize = logger.LogConfiguration.MaxFileSize * 1024 * 1024;
            var onePercentVariation = .01;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles );
            TestContext.WriteLine( "Log Creation 1 took {0} ms.", sw.ElapsedMilliseconds );

            logger.LogConfiguration.LogPath = $"{expectedLogFolder}\\{Guid.NewGuid()}.log";
            logger.LogConfiguration.MaxFileSize = expectedLogSize;
            logger.LogConfiguration.LastUpdated = RockDateTime.Now;

            sw = System.Diagnostics.Stopwatch.StartNew();
            CreateLogFiles( logger, logger.LogConfiguration.MaxFileSize, logger.LogConfiguration.NumberOfLogFiles );
            TestContext.WriteLine( "Log Creation 2 took {0} ms.", sw.ElapsedMilliseconds );

            sw = System.Diagnostics.Stopwatch.StartNew();
            logger.Close();
            TestContext.WriteLine( "Logger Close took {0} ms.", sw.ElapsedMilliseconds );

            var logFolderPath = System.IO.Path.GetFullPath( System.IO.Path.GetDirectoryName( logger.LogConfiguration.LogPath ) );

            sw = System.Diagnostics.Stopwatch.StartNew();
            Assert.That.FolderFileSizeIsWithinRange( logFolderPath, 0, expectedMaxFileSize, onePercentVariation );
            TestContext.WriteLine( "FolderFileSizeIsWithinRange took {0} ms.", sw.ElapsedMilliseconds );
        }

        [TestMethod]
        public void LoggerVerboseShouldLogCorrectly()
        {
            var logger = GetTestLogger();
            var logLevel = "Verbose";
            var expectedLogMessages = new List<string>();


            var logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel, true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        [TestMethod]
        public void LoggerDebugShouldLogCorrectly()
        {
            var logger = GetTestLogger();
            var expectedLogMessages = new List<string>();
            var logGuid = $"{Guid.NewGuid()}";
            var logLevel = "Debug";

            logger.Debug( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel, true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        private string GetSimpleExpectedString( string logGuid, string domain, string logLevel = "", bool includeException = false )
        {
            var logLevelText = string.Empty;
            if ( !string.IsNullOrWhiteSpace( logLevel ) )
            {
                logLevelText = $"\"@l\":\"{logLevel}\",";
            }

            if ( includeException )
            {
                return $"\"@mt\":\"{{domain}} {logGuid}\",{logLevelText}\"@x\":\"{TEST_EXCEPTION_SERIALIZATION}\",\"domain\":\"{domain}\"";
            }
            return $"\"@mt\":\"{{domain}} {logGuid}\",{logLevelText}\"domain\":\"{domain}\"";
        }

        private string GetStructuredExpectedString( string logGuid, string domain, string logLevel = "", bool includeException = false )
        {
            var logLevelText = string.Empty;
            if ( !string.IsNullOrWhiteSpace( logLevel ) )
            {
                logLevelText = $"\"@l\":\"{logLevel}\",";
            }

            if ( includeException )
            {
                return $"\"@mt\":\"{{domain}} {logGuid} {{@oneProperty}} {{@twoProperty}}\",{logLevelText}\"@x\":\"{TEST_EXCEPTION_SERIALIZATION}\",\"domain\":\"{domain}\",\"oneProperty\":{{\"LogLevel\":\"All\",\"MaxFileSize\":1,\"NumberOfLogFiles\":1,\"DomainsToLog\":[\"OTHER\",\"crm\"],\"LogPath\":\"logs\\\\rock.log\",\"LastUpdated\":\"2020-04-01T11:11:11.0000000\",\"$type\":\"RockLogConfiguration\"}},\"twoProperty\":\"All\"";
            }
            return $"\"@mt\":\"{{domain}} {logGuid} {{@oneProperty}} {{@twoProperty}}\",{logLevelText}\"domain\":\"{domain}\",\"oneProperty\":{{\"LogLevel\":\"All\",\"MaxFileSize\":1,\"NumberOfLogFiles\":1,\"DomainsToLog\":[\"OTHER\",\"crm\"],\"LogPath\":\"logs\\\\rock.log\",\"LastUpdated\":\"2020-04-01T11:11:11.0000000\",\"$type\":\"RockLogConfiguration\"}},\"twoProperty\":\"All\"";
        }

        [TestMethod]
        public void LoggerInformationShouldLogCorrectly()
        {
            var logger = GetTestLogger();

            var expectedLogMessages = new List<string>();
            var logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", includeException: true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", includeException: true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", includeException: true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", includeException: true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        [TestMethod]
        public void LoggerWarningShouldLogCorrectly()
        {
            var logger = GetTestLogger();

            var expectedLogMessages = new List<string>();
            var logLevel = "Warning";

            var logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel, true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        [TestMethod]
        public void LoggerErrorShouldLogCorrectly()
        {
            var logger = GetTestLogger();

            var expectedLogMessages = new List<string>();
            var logLevel = "Error";

            var logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel, true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        [TestMethod]
        public void LoggerFatalShouldLogCorrectly()
        {
            var logger = GetTestLogger();

            var expectedLogMessages = new List<string>();
            var logLevel = "Fatal";

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( "CRM", logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( "CRM", $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( "CRM", TestException, $"{logGuid}" );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "CRM", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "OTHER", logLevel, true ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( "CRM", TestException, $"{logGuid} {{@oneProperty}} {{@twoProperty}}", ObjectToLog, RockLogLevel.All );
            expectedLogMessages.Add( GetStructuredExpectedString( logGuid, "CRM", logLevel, true ) );

            logger.Close();

            foreach ( var expectedMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedMessage );
            }
        }

        [TestMethod]
        public void LoggerVerboseShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );
            var logLevel = "Verbose";

            var logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", logLevel );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", logLevel );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerDebugShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );
            var logLevel = "Debug";

            var logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", logLevel );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", logLevel );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerInformationShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );

            var logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER" );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM" );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerWarningShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );
            var logLevel = "Warning";
            var logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", logLevel );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", logLevel );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerErrorShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );
            var logLevel = "Error";
            var logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", logLevel );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", logLevel );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerFatalShouldLogOnlySpecifiedDomains()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );
            var logLevel = "Fatal";
            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", logLevel );

            logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", logLevel );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        [TestMethod]
        public void LoggerLogLevelOffShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Off );

            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            Assert.That.FileNotFound( logger.LogConfiguration.LogPath );
        }

        [TestMethod]
        public void LoggerLogLevelFatalShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Fatal );

            var expectedLogMessages = new List<string>();
            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }

            foreach ( var excludedLogMessage in excludedLogMessages )
            {
                Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerLogLevelErrorShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Error );

            var expectedLogMessages = new List<string>();
            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }

            foreach ( var excludedLogMessage in excludedLogMessages )
            {
                Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerLogLevelWarningShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Warning );

            var expectedLogMessages = new List<string>();
            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }

            foreach ( var excludedLogMessage in excludedLogMessages )
            {
                Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerLogLevelInformationShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Info );

            var expectedLogMessages = new List<string>();
            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }

            foreach ( var excludedLogMessage in excludedLogMessages )
            {
                Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerLogLevelDebugShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.Debug );

            var expectedLogMessages = new List<string>();
            var excludedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            excludedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }

            foreach ( var excludedLogMessage in excludedLogMessages )
            {
                Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerLogLevelAllShouldLogCorrectly()
        {
            var logger = GetTestLogger( logLevel: RockLogLevel.All );

            var expectedLogMessages = new List<string>();

            var logGuid = $"{Guid.NewGuid()}";
            logger.Fatal( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Fatal" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Error( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Error" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Warning" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Information( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Debug( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Debug" ) );

            logGuid = $"{Guid.NewGuid()}";
            logger.Verbose( logGuid );
            expectedLogMessages.Add( GetSimpleExpectedString( logGuid, "OTHER", "Verbose" ) );

            logger.Close();

            foreach ( var expectedLogMessage in expectedLogMessages )
            {
                Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            }
        }

        [TestMethod]
        public void LoggerShouldAutomaticallyStartLoggingAdditionalDomainsAddedToList()
        {
            var logger = GetTestLogger( domainsToLog: new List<string> { "other" } );

            var logGuid = $"{Guid.NewGuid()}";
            logger.Warning( logGuid );
            var expectedLogMessage = GetSimpleExpectedString( logGuid, "OTHER", "Warning" );

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", logGuid );
            var excludedLogMessage = GetSimpleExpectedString( logGuid, "CRM", "Warning" );

            logger.LogConfiguration.DomainsToLog.Add( "CRM" );
            logger.LogConfiguration.LastUpdated = RockDateTime.Now;

            logGuid = $"{Guid.NewGuid()}";
            logger.Warning( "CRM", logGuid );
            var expectedLogMessage2 = GetSimpleExpectedString( logGuid, "CRM", "Warning" );

            logger.Close();

            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage );
            Assert.That.FileContains( logger.LogConfiguration.LogPath, expectedLogMessage2 );
            Assert.That.FileDoesNotContains( logger.LogConfiguration.LogPath, excludedLogMessage );
        }

        #region Private Helper Code
        private RockLogConfiguration GetTestObjectToSerialize()
        {
            return new RockLogConfiguration
            {
                LogLevel = RockLogLevel.All,
                LastUpdated = new DateTime( 2020, 04, 01, 11, 11, 11 ),
                LogPath = "logs\\rock.log",
                DomainsToLog = new List<string> { "OTHER", "crm" },
                MaxFileSize = 1,
                NumberOfLogFiles = 1
            };
        }

        private IRockLogger GetTestLogger( string logFolder = "", List<string> domainsToLog = null, RockLogLevel logLevel = RockLogLevel.All, int numberOfLogFiles = 2, int logSize = 1 )
        {
            if ( string.IsNullOrWhiteSpace( logFolder ) )
            {
                logFolder = LogFolder;
            }

            if ( domainsToLog == null )
            {
                domainsToLog = new List<string> { "OTHER", "crm" };
            }

            var config = new RockLogConfiguration
            {
                LogLevel = logLevel,
                MaxFileSize = logSize,
                NumberOfLogFiles = numberOfLogFiles,
                DomainsToLog = domainsToLog,
                LogPath = $"{logFolder}\\{Guid.NewGuid()}.log",
                LastUpdated = RockDateTime.Now
            };
            return ReflectionHelper.InstantiateInternalObject<IRockLogger>( "Rock.Logging.RockLoggerSerilog", config );
        }

        private void CreateLogFiles( IRockLogger logger, int maxFilesizeInMB, int numberOfFiles )
        {
            var maxByteCount = maxFilesizeInMB * 1024 * 1024 * numberOfFiles;
            var currentByteCount = 0;
            var logHeaderInformation = "0000-00-00 00:00:00.000 -00:00 [INF] OTHER";

            while ( currentByteCount < maxByteCount )
            {
                var expectedLogMessage = $"Test - {Guid.NewGuid()}";
                logger.Information( expectedLogMessage );

                currentByteCount += Encoding.ASCII.GetByteCount( $"{logHeaderInformation} {expectedLogMessage}" );
            }
            TestContext.WriteLine( "CreateLogFiles wrote {0} bytes of logs to {1}.", currentByteCount, logger.LogConfiguration.LogPath );
        }

        #endregion
    }

    /// <summary>
    /// This class is used just for testing purposes, because the goal here
    /// is to test the logger not the configuration functionality. The rock implementation
    /// of IRockLogConfiguration is internal to Rock and therefore not accessible here.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogConfiguration" />
    internal class RockLogConfiguration : IRockLogConfiguration
    {
        public RockLogLevel LogLevel { get; set; }
        public int MaxFileSize { get; set; }
        public int NumberOfLogFiles { get; set; }
        public List<string> DomainsToLog { get; set; }
        public string LogPath { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
