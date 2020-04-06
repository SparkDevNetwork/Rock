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
using Rock.Data;

namespace Rock.Tests.Integration
{
    /// <summary>
    /// Provides basic functions for a test class that requires access to a Rock database.
    /// </summary>
    public abstract class DatabaseIntegrationTestClassBase
    {
        private static bool? _DatabaseConnectionIsValid = null;
        private static string _DatabaseConnectionStatusMessage = null;
        private static bool? _TestDataIsValid = null;
        private static string _TestDataStatusMessage = null;

        /// <summary>
        /// Override this method to perform custom validation to ensure that the required test data exists in the database.
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="stateMessage">A message describing the reasons for a failure state.</param>
        protected abstract void OnValidateTestData( out bool isValid, out string stateMessage );

        /// <summary>
        /// Verify that the preconditions necessary for a test to execute are valid.
        /// This method should be called first in each test to ensure that the test environment is valid.
        /// If any of the preconditions are not met, an Exception is thrown to abort the test.
        /// </summary>
        protected void VerifyTestPreconditionsOrThrow()
        {
            // Validate database connection.
            if ( _DatabaseConnectionIsValid == null )
            {
                ValidateDatabaseConnection();
            }

            if ( !_DatabaseConnectionIsValid.Value )
            {
                var message = "Database connection failed.";

                if ( _DatabaseConnectionStatusMessage.IsNotNullOrWhiteSpace() )
                {
                    message += "\n" + _DatabaseConnectionStatusMessage;
                }

                throw new InvalidOperationException( message );
            }

            // Validate test data.
            if ( _TestDataIsValid == null )
            {
                ValidateTestData();
            }

            if ( !_TestDataIsValid.Value )
            {
                var message = "Test data is invalid.";

                if ( _TestDataStatusMessage.IsNotNullOrWhiteSpace() )
                {
                    message += "\n" + _TestDataStatusMessage;
                }

                throw new InvalidOperationException( message );
            }
        }

        /// <summary>
        /// Validate the connection to the test database.
        /// </summary>
        private void ValidateDatabaseConnection()
        {
            try
            {
                var context = this.GetDataContext();

                var connection = context.Database.Connection;

                connection.Open();
                connection.Close();

                _DatabaseConnectionIsValid = true;
                _DatabaseConnectionStatusMessage = null;
            }
            catch ( Exception ex )
            {
                _DatabaseConnectionIsValid = false;
                _DatabaseConnectionStatusMessage = ex.ToString();
            }
        }

        /// <summary>
        /// Validate the database records that are required to execute the tests.
        /// </summary>
        private void ValidateTestData()
        {
            bool isValid;
            string message;

            OnValidateTestData( out isValid, out message );

            _TestDataIsValid = isValid;
            _TestDataStatusMessage = message;
        }

        /// <summary>
        /// Get a new database context.
        /// </summary>
        /// <returns></returns>
        protected RockContext GetDataContext()
        {
            return new RockContext();
        }

        /// <summary>
        /// Get a new database context.
        /// </summary>
        /// <returns></returns>
        protected static RockContext GetNewDataContext()
        {
            return new RockContext();
        }
    }
}
