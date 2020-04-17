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
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Utility.Settings;

namespace Rock.Tests.Integration.Utility
{
    /// <summary>
    /// Integration tests for the RockInstanceConfiguration class.
    /// </summary>
    [TestClass]
    [Ignore("These tests should be disabled unless testing changes to this feature.")]
    public class RockInstanceConfigurationTests : DatabaseIntegrationTestClassBase
    {
        #region Configuration

        /*
         * 2020-04-17 [DL]
         * These integration tests use hard-coded values to verify the RockInstanceConfigurationService against known database instances.
         * The configuration settings below should mirror the connection string and settings for the test database.
         *
         * In the future, these tests should be improved to use dynamic methods of verification that do not require any hard-coded test values.
         */

        /* SQL Server Test Settings */
        private const string _TestSettingServerName = "agent13";
        private const string _TestSettingDatabaseName = "rock_spark_dev";
        private const string _TestSettingsDatabaseVersion = "11.0.3128.0";
        private const string _TestSettingsDatabaseVersionFriendlyName = "SQL Server 2012";
        private const string _TestSettingsDatabaseHostPlatform = "Windows NT 6.2 <X64> (Build 9200: ) (Hypervisor)";
        private const string _TestSettingsDatabaseEdition = "Standard Edition (64-bit)";
        private const string _TestSettingsDatabaseRecoveryMode = "SIMPLE";
        private const string _TestSettingsServiceObjective = null;
        private const bool _TestSettingReadCommittedSnapshotEnabled = true;
        private const bool _TestSettingSnapshotIsolationAllowed = false;
        private const RockInstanceDatabaseConfiguration.PlatformSpecifier _TestSettingPlatform = RockInstanceDatabaseConfiguration.PlatformSpecifier.SqlServer;

        /* Azure SQL Test Settings 
        private const string _TestSettingServerName = "tcp:oneheartbali.database.windows.net";
        private const string _TestSettingDatabaseName = "rock_live";
        private const string _TestSettingsDatabaseVersion = "12.0.2000.8";
        private const string _TestSettingsDatabaseVersionFriendlyName = "SQL Server 2014";
        private const string _TestSettingsDatabaseHostPlatform = "Azure";
        private const string _TestSettingsDatabaseEdition = "Standard";
        private const string _TestSettingsDatabaseRecoveryMode = "FULL";
        private const string _TestSettingsServiceObjective = "S0";
        private const bool _TestSettingReadCommittedSnapshotEnabled = true;
        private const bool _TestSettingSnapshotIsolationAllowed = true;
        private const RockInstanceDatabaseConfiguration.PlatformSpecifier _TestSettingPlatform = RockInstanceDatabaseConfiguration.PlatformSpecifier.AzureSql;
        */

        protected override void OnValidateTestData( out bool isValid, out string stateMessage )
        {
            try
            {
                // Verify that we are connected to the expected test database.                
                var dataContext = GetNewDataContext();

                var connectionString = dataContext.Database.Connection.ConnectionString;

                var csBuilder = new System.Data.Odbc.OdbcConnectionStringBuilder( connectionString );

                object value;
                bool hasValue;

                hasValue = csBuilder.TryGetValue( "server", out value );

                if ( !hasValue )
                {
                    hasValue = csBuilder.TryGetValue( "data source", out value );
                }

                var serverName = value.ToStringSafe();

                if ( !serverName.Equals( _TestSettingServerName, StringComparison.OrdinalIgnoreCase ) )
                {
                    throw new Exception( $"Test database is invalid. Server Name \"{serverName}\" does not match test configuration settings." );
                }

                csBuilder.TryGetValue( "initial catalog", out value );

                var databaseName = value.ToStringSafe();

                if ( !databaseName.Equals( _TestSettingDatabaseName, StringComparison.OrdinalIgnoreCase ) )
                {
                    throw new Exception( $"Test database is invalid. Database Name \"{databaseName}\" does not match test configuration settings." );
                }

                isValid = true;
                stateMessage = null;
            }
            catch ( Exception ex )
            {
                isValid = false;
                stateMessage = ex.Message;
            }
        }

        #endregion

        #region General Tests

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetSystemDateTime_ReturnsCorrectResult()
        {
            var value = RockInstanceConfig.SystemDateTime;

            Debug.Print( value.ToString() );
            Assert.That.AreProximate( DateTime.Now.Date, value.Date, new TimeSpan( 0, 1, 0 ) );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetRockDateTime_ReturnsCorrectResult()
        {
            var value = RockInstanceConfig.RockDateTime;

            Debug.Print( value.ToString() );
            Assert.That.AreProximate( DateTime.Now.Date, value.Date, new TimeSpan( 0, 1, 0 ) );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetApplicationDirectory_ReturnsValidPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var value = RockInstanceConfig.ApplicationDirectory;

            Assert.That.AreEqual( baseDir, value );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetPhysicalDirectory_ReturnsValidPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;

            var value = RockInstanceConfig.PhysicalDirectory;

            Assert.That.AreEqual( appDir, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseVersion_ReturnsValidVersionString()
        {
            var version = RockInstanceConfig.Database.Version;

            var isMatch = Regex.IsMatch( version, @"\d+(\.\d+)+" );

            Assert.That.IsTrue( isMatch, "Unexpected Version string format." );
        }

        /// <summary>
        /// Verify that the database log size property returns a value.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseSize_ReturnsNonZeroValue()
        {
            var value = RockInstanceConfig.Database.DatabaseSize;

            Debug.Print( value.ToString() );
            Assert.That.True( value.GetValueOrDefault( 0 ) > 0, "Database size not retrieved." );
        }

        /// <summary>
        /// Verify that the database log size property returns a value.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseLogSize_ReturnsNonZeroValue()
        {
            var value = RockInstanceConfig.Database.LogSize;

            Debug.Print( value.ToString() );
            Assert.That.True( value.GetValueOrDefault( 0 ) > 0, "Log size not retrieved." );
        }

        #endregion

        #region Configuration-specific Tests

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabasePlatform_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var platform = RockInstanceConfig.Database.Platform;

            Assert.That.AreEqual( _TestSettingPlatform, platform );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabasePropertySnapshotIsolationAllowed_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.SnapshotIsolationAllowed;

            Assert.That.AreEqual( _TestSettingSnapshotIsolationAllowed, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseName_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.DatabaseName;

            Assert.That.AreEqual( _TestSettingDatabaseName, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseServerName_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.ServerName;

            Assert.That.AreEqual( _TestSettingServerName, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseOperatingSystemName_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.DatabaseServerOperatingSystem;

            Assert.That.AreEqual( _TestSettingsDatabaseHostPlatform, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseEdition_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.Edition;

            Assert.That.AreEqual( _TestSettingsDatabaseEdition, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseRecoveryMode_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.RecoverMode;

            Assert.That.AreEqual( _TestSettingsDatabaseRecoveryMode, value );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseOptionReadCommittedSnapshot_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.ReadCommittedSnapshotEnabled;

            Assert.That.AreEqual( _TestSettingReadCommittedSnapshotEnabled, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseServiceObjectiveForSqlServer_ReturnsNull()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.ServiceObjective;

            Assert.That.AreEqual( _TestSettingsServiceObjective, value );
        }

        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseVersionForTestDatabase_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.Version;

            Assert.That.AreEqual( _TestSettingsDatabaseVersion, value );
        }

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        public void RockInstanceConfiguration_GetDatabaseVersionFriendlyName_ReturnsCurrentEnvironmentValue()
        {
            VerifyTestPreconditionsOrThrow();

            var value = RockInstanceConfig.Database.VersionFriendlyName;

            Assert.That.AreEqual( _TestSettingsDatabaseVersionFriendlyName, value );
        }

        #endregion
    }
}
