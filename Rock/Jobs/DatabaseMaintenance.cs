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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisplayName( "Database Maintenance" )]
    [Description( "Performs routine SQL Server database maintenance." )]

    [BooleanField( "Run Integrity Check", "Determines if an integrity check should be performed.", true, order: 0 )]
    [BooleanField( "Run Index Rebuild", "Determines if indexes should be rebuilt.", true, order: 1 )]
    [BooleanField( "Run Statistics Update", "Determines if the statistics should be updated.", true, order: 2 )]
    [TextField( "Alert Email", "Email address to send alerts to errors occur (multiple address delimited with comma).", true, order: 3 )]

    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each step to complete.", false, 900, "Advanced", 4, "CommandTimeout" )]
    [IntegerField( "Minimum Index Page Count", "The minimum size in pages that an index must be before it's considered for being re-built. Default value is 100.", false, 100, category: "Advanced", order: 5 )]
    [IntegerField( "Minimum Fragmentation Percentage", "The minimum fragmentation percentage for an index to be considered for re-indexing. If the fragmentation is below is amount nothing will be done. Default value is 10%.", false, 10, category: "Advanced", order: 6 )]
    [IntegerField( "Rebuild Threshold Percentage", "The threshold percentage where a REBUILD will be completed instead of a REORGANIZE. Default value is 30%.", false, 30, category: "Advanced", order: 7 )]
    [BooleanField( "Use ONLINE Index Rebuild", "Use the ONLINE option when rebuilding indexes. NOTE: This is only supported on SQL Enterprise and Azure SQL Database.", false, category: "Advanced", order: 8 )]
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

            int commandTimeout = dataMap.GetString( "CommandTimeout" ).AsInteger();
            StringBuilder resultsMessage = new StringBuilder();
            bool integrityCheckPassed = false;
            bool integrityCheckIgnored = false;

            /*
             * DJL 2020-04-08
             * For Microsoft Azure, disable the Integrity Check and Statistics Update tasks.
             * Refer: https://azure.microsoft.com/en-us/blog/data-integrity-in-azure-sql-database/
             */
            if ( RockInstanceConfig.Database.Platform == RockInstanceDatabaseConfiguration.PlatformSpecifier.AzureSql )
            {
                runIntegrityCheck = false;
                runStatisticsUpdate = false;
            }

            // run integrity check
            if ( runIntegrityCheck )
            {
                try
                {
                    string alertEmail = dataMap.GetString( "AlertEmail" );
                    integrityCheckPassed = IntegrityCheck( commandTimeout, alertEmail, resultsMessage );

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, HttpContext.Current );
                    ExceptionLogService.LogException( resultsMessage.ToString() );
                    throw;
                }
            }
            else
            {
                integrityCheckIgnored = true;
            }

            if ( integrityCheckPassed || integrityCheckIgnored )
            {
                // rebuild fragmented indexes
                if ( runIndexRebuild )
                {
                    try
                    {
                        RebuildFragmentedIndexes( dataMap, commandTimeout, resultsMessage );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, HttpContext.Current );
                        ExceptionLogService.LogException( resultsMessage.ToString() );
                        throw;
                    }
                }

                // update statistics
                if ( runStatisticsUpdate )
                {
                    try
                    {
                        UpdateStatistics( commandTimeout, resultsMessage );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, HttpContext.Current );
                        ExceptionLogService.LogException( resultsMessage.ToString() );
                        throw;
                    }
                }
            }

            context.Result = resultsMessage.ToString().TrimStart( ',' );
        }

        private bool IntegrityCheck( int commandTimeout, string alertEmail, StringBuilder resultsMessage )
        {
            string databaseName = new RockContext().Database.Connection.Database;
            string integrityQuery = $"DBCC CHECKDB('{ databaseName }',NOINDEX) WITH PHYSICAL_ONLY, NO_INFOMSGS";
            bool checkPassed = true;

            Stopwatch stopwatch = Stopwatch.StartNew();
            int errors = DbService.ExecuteCommand( integrityQuery, System.Data.CommandType.Text, null, commandTimeout );
            stopwatch.Stop();

            resultsMessage.Append( $"Integrity Check took {( stopwatch.ElapsedMilliseconds / 1000 )}s" );

            if ( errors > 0 )
            {
                // oh no...
                checkPassed = false;
                string errorMessage = $"Some errors were reported when running a database integrity check on your Rock database. We'd recommend running the command below under 'Admin Tools > Power Tools > SQL Command' to get further details. <p>DBCC CHECKDB ('{ databaseName }') WITH NO_INFOMSGS, ALL_ERRORMSGS</p>";

                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null );
                mergeFields.Add( "ErrorMessage", errorMessage );
                mergeFields.Add( "Errors", errors );

                resultsMessage.Append( errorMessage );

                if ( alertEmail.IsNotNullOrWhiteSpace() )
                {
                    var globalAttributes = GlobalAttributesCache.Get();
                    string emailHeader = globalAttributes.GetValue( "EmailHeader" );
                    string emailFooter = globalAttributes.GetValue( "EmailFooter" );
                    string messageBody = $"{emailHeader} {errorMessage} <p><small>This message was generated from the Rock Database Maintenance Job</small></p>{emailFooter}";

                    var emailMessage = new RockEmailMessage();
                    var alertEmailList = alertEmail.Split( ',' ).ToList();
                    var recipients = alertEmailList.Select( a => RockEmailMessageRecipient.CreateAnonymous( a, mergeFields ) ).ToList();
                    emailMessage.Subject = "Rock: Database Integrity Check Error";
                    emailMessage.Message = messageBody;
                    emailMessage.Send();
                }
            }

            return checkPassed;
        }

        private void RebuildFragmentedIndexes( JobDataMap dataMap, int commandTimeout, StringBuilder resultsMessage )
        {
            int minimumIndexPageCount = dataMap.GetString( "MinimumIndexPageCount" ).AsInteger();
            int minimunFragmentationPercentage = dataMap.GetString( "MinimumFragmentationPercentage" ).AsInteger();
            int rebuildThresholdPercentage = dataMap.GetString( "RebuildThresholdPercentage" ).AsInteger();
            bool useONLINEIndexRebuild = dataMap.GetString( "UseONLINEIndexRebuild" ).AsBoolean();

            Dictionary<string, object> parms = new Dictionary<string, object>();
            parms.Add( "@PageCountLimit", minimumIndexPageCount );
            parms.Add( "@MinFragmentation", minimunFragmentationPercentage );
            parms.Add( "@MinFragmentationRebuild", rebuildThresholdPercentage );
            parms.Add( "@UseONLINEIndexRebuild", useONLINEIndexRebuild );

            Stopwatch stopwatch = Stopwatch.StartNew();
            DbService.ExecuteCommand( "spDbaRebuildIndexes", System.Data.CommandType.StoredProcedure, parms, commandTimeout );
            stopwatch.Stop();

            resultsMessage.Append( $", Index Rebuild took {( stopwatch.ElapsedMilliseconds / 1000 )}s" );
        }

        private void UpdateStatistics( int commandTimeout, StringBuilder resultsMessage )
        {
            // derived from http://www.sqlservercentral.com/scripts/Indexing/31823/
            // NOTE: Can't use sp_MSForEachtable because it isn't supported on AZURE (and it is undocumented)
            // NOTE: Can't use sp_updatestats because it requires membership in the sysadmin fixed server role, or ownership of the database (dbo)
            string statisticsQuery = @"
                DECLARE updatestats CURSOR
                FOR
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'
                ORDER BY TABLE_NAME

                OPEN updatestats

                DECLARE @tablename NVARCHAR(max)
                DECLARE @Statement NVARCHAR(max)

                FETCH NEXT
                FROM updatestats
                INTO @tablename

                WHILE (@@FETCH_STATUS = 0)
                BEGIN
	                PRINT N'UPDATING STATISTICS [' + @tablename + ']'
	                SET @Statement = 'UPDATE STATISTICS [' + @tablename + ']'

	                EXEC sp_executesql @Statement

	                FETCH NEXT
	                FROM updatestats
	                INTO @tablename
                END

                CLOSE updatestats

                DEALLOCATE updatestats";

            Stopwatch stopwatch = Stopwatch.StartNew();
            DbService.ExecuteCommand( statisticsQuery, System.Data.CommandType.Text, null, commandTimeout );
            stopwatch.Stop();

            resultsMessage.Append( $", Statistics Update took {( stopwatch.ElapsedMilliseconds / 1000 )}s" );
        }
    }
}
