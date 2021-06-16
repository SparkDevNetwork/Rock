﻿// <copyright>
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

namespace Rock.Utility.Settings
{
    /// <summary>
    /// A wrapper for the RockInstanceConfigurationService that provides a global shared instance of the service.
    /// </summary>
    public static class RockInstanceConfig
    {
        private static RockInstanceConfigurationService _serviceInstance = new RockInstanceConfigurationService();

        static RockInstanceConfig()
        {
            // Create an instance of the configuration service using the database connection stored in the configuration file.
            _serviceInstance = new RockInstanceConfigurationService();

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RockContext"];

            _serviceInstance.Database.SetConnectionString( connectionString.ToStringSafe() );
        }

        /// <summary>
        /// Returns the virtual path of the Rock application on the host.
        /// </summary>
        public static string ApplicationDirectory
        {
            get
            {
                return _serviceInstance.ApplicationDirectory;
            }
        }

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <value>
        /// The name of the machine.
        /// </value>
        public static string MachineName
        {
            get
            {
                return _serviceInstance.MachineName;
            }
        }

        /// <summary>
        /// Returns the install path of the Rock application on the host.
        /// </summary>
        public static string PhysicalDirectory
        {
            get
            {
                return _serviceInstance.PhysicalDirectory;
            }
        }

        /// <summary>
        /// Returns the date and time of the application host server, localised according to the timezone specified in the Rock application settings.
        /// </summary>
        public static DateTime RockDateTime
        {
            get
            {
                return _serviceInstance.RockDateTime;
            }
        }

        /// <summary>
        /// Returns the date and time of the application host server.
        /// </summary>
        public static DateTime SystemDateTime
        {
            get
            {
                return _serviceInstance.SystemDateTime;
            }
        }

        /// <summary>
        /// Gets the ASP net version.
        /// </summary>
        /// <value>
        /// The ASP net version.
        /// </value>
        public static string AspNetVersion
        {
            get
            {
                return _serviceInstance.AspNetVersion;
            }
        }

        /// <summary>
        /// Gets information about the database configuration for the current Rock instance.
        /// </summary>
        public static RockInstanceDatabaseConfiguration Database
        {
            get
            {
                return _serviceInstance.Database;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is clustered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is clustered; otherwise, <c>false</c>.
        /// </value>
        public static bool IsClustered
        {
            get
            {
                return _serviceInstance.IsClustered;
            }
        }
    }
}
