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
using System.Data.Entity.Migrations.Infrastructure;
using System.Diagnostics;
using System.IO;

namespace Rock.Migrations
{
    /// <summary>
    /// Outputs migration messages depending on the LogVerbose, LogInfo, LogWarning.
    /// Note this will always log Info and Warnings when System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment == true
    /// </summary>
    /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.MigrationsLogger" />
    public class RockMigrationsLogger : MigrationsLogger
    {
        /// <summary>
        /// Gets or sets a value indicating whether verbose messages should also be logged
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log verbose]; otherwise, <c>false</c>.
        /// </value>
        public bool LogVerbose { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [log information].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log information]; otherwise, <c>false</c>.
        /// </value>
        public bool LogInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [log warning].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log warning]; otherwise, <c>false</c>.
        /// </value>
        public bool LogWarning { get; set; } = false;

        private string lastMigrationName = null;
        private Stopwatch stopwatch = null;
        private StreamWriter logFile = null;
        
        private const string MIGRATIONS_LOG_FILENAME = "MigrationLog";

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public override void Info( string message )
        {
            if ( LogInfo || System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                if ( message.StartsWith( "Applying explicit migration:" ) )
                {
                    LogCompletedMigration();

                    lastMigrationName = message.Replace( "Applying explicit migration:", string.Empty ).Trim();
                    stopwatch = Stopwatch.StartNew();
                }
                else
                {
                    if ( message.Equals( "Running Seed method." ) )
                    {
                        LogCompletedMigration();
                    }
                }
            }
        }

        private string lastLoggedCompletedMigration;

        /// <summary>
        /// Logs the completed migration.
        /// </summary>
        public void LogCompletedMigration()
        {
            if ( lastMigrationName != null && stopwatch != null )
            {
                stopwatch.Stop();
                if ( lastLoggedCompletedMigration != lastMigrationName )
                {
                    WriteToLog( $"[{stopwatch.Elapsed.TotalMilliseconds,5:#} ms],{lastMigrationName}" );
                    lastLoggedCompletedMigration = lastMigrationName;
                }
            }
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteToLog( string message )
        {
            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( message );
            }

            try
            {

                if ( logFile == null )
                {
                    string directory = AppDomain.CurrentDomain.BaseDirectory;
                    directory = Path.Combine( directory, "App_Data", "Logs" );

                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    string filePath = Path.Combine( directory, MIGRATIONS_LOG_FILENAME + ".csv" );
                    logFile = new StreamWriter( filePath, true  );
                }

                logFile.WriteLine( $"{RockDateTime.Now:MM/dd/yyyy HH:mm:ss.fff},{message}" );
                logFile.Flush();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Logs a warning that the user should be made aware of.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public override void Warning( string message )
        {
            if ( this.LogWarning || System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                WriteToLog( "WARNING: " + message );
            }
        }

        /// <summary>
        /// Logs some additional information that should only be presented to the user if they request verbose output.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public override void Verbose( string message )
        {
            if ( this.LogVerbose )
            {
                WriteToLog( "VERBOSE: " + message );
            }
        }

        /// <summary>
        /// Logs the system information message.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void LogSystemInfo( string key, string value )
        {
            WriteToLog( $"{key}: {value}" );
        }
    }
}
