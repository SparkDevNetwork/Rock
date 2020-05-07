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
using System.Collections.Generic;
using Rock.SystemKey;

namespace Rock.Logging
{
    /// <summary>
    /// This is the internal implementation of IRockLogConfiguration the gets
    /// the configuration information from SystemSetting.ROCK_LOGGING_SETTINGS
    /// global attribute.
    /// </summary>
    /// <seealso cref="Rock.Logging.IRockLogConfiguration" />
    internal class RockLogConfiguration : IRockLogConfiguration
    {
        private RockLogLevel _logLevel;
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public RockLogLevel LogLevel
        {
            get
            {
                UpdateConfigIfRequired();
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        private int _maxFileSize;
        /// <summary>
        /// Gets or sets the maximum size of the file.
        /// </summary>
        /// <value>
        /// The maximum size of the file.
        /// </value>
        public int MaxFileSize
        {
            get
            {
                UpdateConfigIfRequired();
                return _maxFileSize;
            }
            set
            {
                _maxFileSize = value;
            }
        }

        private int _numberOfLogFiles;
        /// <summary>
        /// Gets or sets the number of log files.
        /// </summary>
        /// <value>
        /// The number of log files.
        /// </value>
        public int NumberOfLogFiles
        {
            get
            {
                UpdateConfigIfRequired();
                return _numberOfLogFiles;
            }
            set
            {
                _numberOfLogFiles = value;
            }
        }

        private List<string> _domainsToLog;
        /// <summary>
        /// Gets or sets the domains to log.
        /// </summary>
        /// <value>
        /// The domains to log.
        /// </value>
        public List<string> DomainsToLog
        {
            get
            {
                UpdateConfigIfRequired();
                return _domainsToLog;
            }
            set
            {
                _domainsToLog = value;
            }
        }

        /// <summary>
        /// Gets or sets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the log path.
        /// </summary>
        /// <value>
        /// The log path.
        /// </value>
        public string LogPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockLogConfiguration"/> class.
        /// </summary>
        public RockLogConfiguration()
        {
            UpdateConfigFromSystemSettings();
        }

        private void UpdateConfigFromSystemSettings()
        {
            var rockSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.ROCK_LOGGING_SETTINGS ).FromJsonOrNull<RockLogSystemSettings>();

            if ( rockSettings == null )
            {
                LogLevel = RockLogLevel.Off;
                NumberOfLogFiles = 20;
                MaxFileSize = 20;
                DomainsToLog = new List<string>();
            }
            else
            {
                LogLevel = rockSettings.LogLevel;
                NumberOfLogFiles = rockSettings.NumberOfLogFiles;
                MaxFileSize = rockSettings.MaxFileSize;
                DomainsToLog = rockSettings.DomainsToLog;
            }

            LogPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data\\Logs\\Rock.log" );
            LastUpdated = DateTime.Now;
        }

        private void UpdateConfigIfRequired()
        {
            if ( LastUpdated < Rock.Web.SystemSettings.LastUpdated )
            {
                UpdateConfigFromSystemSettings();
            }
        }
    }
}
