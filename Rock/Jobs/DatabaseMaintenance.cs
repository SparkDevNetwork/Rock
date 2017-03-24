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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [BooleanField( "Run Integrity Check", "Determines if an integrity check should be performed.", true, order:0 )]
    [BooleanField( "Run Index Rebuild", "Determines if indexes should be rebuilt.", true, order: 1 )]
    [BooleanField( "Run Statistics Update", "Determines if the statistics should be updated.", true, order: 2 )]
    [EmailField ("Alert Email", "Email address to send alerts to errors occur (multiple address delimited with comma).", true, order: 3)]

    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each step to complete.", false, 900, "Advanced", 4, "CommandTimeout")]
    [IntegerField( "Minimum Index Page Count", "The minimum size in pages that an index must be before it's considered for being re-built. Default value is 100.", false, 100, category: "Advanced", order: 5 )]
    [IntegerField( "Minimum Fragmentation Percentage", "The minimum fragmentation percentage for an index to be considered for re-indexing. If the fragmentation is below is amount nothing will be done. Default value is 10%.", false, 10, category: "Advanced", order: 6 )]
    [IntegerField( "Rebuild Threshold Percentage", "The threshold percentage where a REBUILD will be completed instead of a REORGANIZE. Default value is 30%.", false, 30, category: "Advanced", order: 7 )]
    [DisallowConcurrentExecution]
    public class DatabaseMaintenance : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public DatabaseMaintenance()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get job parms
            bool runIntegrityCheck = dataMap.GetBoolean( "RunIntegrityCheck" );
            bool runIndexRebuild = dataMap.GetBoolean( "RunIndexRebuild" );
            bool runStatisticsUpdate = dataMap.GetBoolean( "RunStatisticsUpdate" );

            int commandTimeout = dataMap.GetString( "CommandTimeout").AsInteger();
            int minimumIndexPageCount = dataMap.GetString( "MinimumIndexPageCount" ).AsInteger();
            int minimunFragmentationPercentage = dataMap.GetString( "MinimumFragmentationPercentage" ).AsInteger();
            int rebuildThresholdPercentage = dataMap.GetString( "RebuildThresholdPercentage" ).AsInteger();

            string alertEmail = dataMap.GetString( "AlertEmail" );

            StringBuilder resultsMessage = new StringBuilder();
            Stopwatch stopwatch;

            bool errorsFound = false;

            // run integrity check
            if ( runIntegrityCheck ) {
                string databaseName = new RockContext().Database.Connection.Database;
                string integrityQuery = $"DBCC CHECKDB('{ databaseName }',NOINDEX) WITH PHYSICAL_ONLY, NO_INFOMSGS";

                stopwatch = Stopwatch.StartNew();
                int errors = DbService.ExecuteCommand( integrityQuery, System.Data.CommandType.Text, null, commandTimeout );
                stopwatch.Stop();

                resultsMessage.Append( $"Integrity Check took {(stopwatch.ElapsedMilliseconds / 1000)}s" );

                if (errors > 0 )
                {
                    // oh no...
                    errorsFound = true;
                    string errorMessage = $"Some errors were reported when running a database integrity check on your Rock database. We'd recommend running the command below under 'Admin Tools > Power Tools > SQL Command' to get further details. <p>DBCC CHECKDB ('{ databaseName }') WITH NO_INFOMSGS, ALL_ERRORMSGS</p>";

                    resultsMessage.Append( errorMessage );

                    if ( alertEmail.IsNotNullOrWhitespace() )
                    {
                        var globalAttributes = GlobalAttributesCache.Read();
                        string globalFrom = globalAttributes.GetValue( "OrganizationEmail" );
                        string emailHeader = globalAttributes.GetValue( "EmailHeader" );
                        string emailFooter = globalAttributes.GetValue( "EmailFooter" );

                        List<string> recipients = alertEmail.Split( ',' ).ToList();

                        string messageBody = $"{emailHeader} {errorMessage} <p><small>This message was generated from the Rock Database Maintenance Job</small></p>s {emailFooter}";

                        Email.Send( globalFrom, "Rock: Database Integrity Check Error", recipients, messageBody );
                    }
                }
            }

            if ( !errorsFound )
            {
                // rebuild fragmented indexes
                if ( runIndexRebuild)
                {
                    Dictionary<string, object> parms = new Dictionary<string, object>();
                    parms.Add( "@PageCountLimit", minimumIndexPageCount );
                    parms.Add( "@MinFragmentation", minimunFragmentationPercentage );
                    parms.Add( "@MinFragmentationRebuild", rebuildThresholdPercentage );

                    stopwatch = Stopwatch.StartNew();
                    DbService.ExecuteCommand( "spDbaRebuildIndexes", System.Data.CommandType.StoredProcedure, parms, commandTimeout );
                    stopwatch.Stop();

                    resultsMessage.Append( $", Index Rebuild took {(stopwatch.ElapsedMilliseconds / 1000)}s" );
                }
                
                // update statistics
                if ( runStatisticsUpdate )
                {
                    string statisticsQuery = "EXEC sp_MSForEachtable 'UPDATE STATISTICS ?'";

                    stopwatch = Stopwatch.StartNew();
                    DbService.ExecuteCommand( statisticsQuery, System.Data.CommandType.Text, null, commandTimeout );
                    stopwatch.Stop();

                    resultsMessage.Append( $", Statistics Update took {(stopwatch.ElapsedMilliseconds / 1000)}s" );
                }
            }

            context.Result = resultsMessage.ToString().TrimStart(',');
        }

    }
}
