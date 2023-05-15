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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Recreate the views for Metric Analytics during the upgrade to 14.1.
    /// </summary>
    [DisplayName( "Rock Update Helper v14.1 - Recreate Metric Analytics Views" )]
    [Description( "Recreate the views for Metric Analytics." )]

    public class PostV141RecreateMetricAnalyticsViews : RockJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        public override void Execute()
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var metricService = new MetricService( new RockContext() );
            metricService.EnsureMetricAnalyticsViews();
            // Log how long it took us to run.
            var logMessage = $"{RockDateTime.Now:MM/dd/yyyy HH:mm:ss.fff},[{stopWatch.Elapsed.TotalMilliseconds,5:#} ms],{this.GetType().FullName}";
            WriteToLog( logMessage );

            ServiceJobService.DeleteJob( GetJobId() );
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
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, "MigrationLog.csv" );
                using ( var logFile = new StreamWriter( filePath, true ) )
                {
                    logFile.WriteLine( message );
                }
            }
            catch
            {
                // Intentionally ignored, don't error if we couldn't log.
            }
        }
    }
}
